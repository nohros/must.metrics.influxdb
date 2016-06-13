using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Nohros.Collections;
using Nohros.Concurrent;
using Nohros.Extensions.Time;
using Nohros.Logging;
using Nohros.Resources;

namespace Nohros.Metrics.Influx
{
  internal class InfluxObserver : IInfluxMeasureObserver
  {
    const int kMaxPointsPerPost = 5000;

    readonly IApiEndpoint endpoint_;
    readonly ConcurrentQueue<InfluxMeasure> measures_;
    readonly NonReentrantSchedule scheduler_;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfluxObserver"/> by
    /// using the given <paramref name="endpoint"/>.
    /// </summary>
    /// <param name="endpoint">
    /// The <see cref="ApiEndpoint"/> to be used to send the measures.
    /// </param>
    public InfluxObserver(IApiEndpoint endpoint)
      : this(endpoint, TimeSpan.FromSeconds(30)) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfluxObserver"/> by
    /// using the given <paramref name="endpoint"/> and <paramref name="ttl"/>.
    /// </summary>
    /// <param name="endpoint">
    /// The <see cref="ApiEndpoint"/> to be used to send the measures.
    /// </param>
    /// <param name="ttl">
    /// The maximum time that a mesure should be keep in cache, before send it
    /// to influx's.
    /// </param>
    /// <remarks>
    /// The <paramref name="ttl"/> should be greater than or equals to
    /// <see cref="TimeSpan.Zero"/>. If <paramref name="ttl"/> is equals to
    /// <see cref="TimeSpan.Zero"/> the default <paramref name="ttl"/> of
    /// third seconds will be used.
    /// <para>
    /// The application's name will be added as a prefix to all measures
    /// before sending it to influx's endpoint.
    /// </para>
    /// </remarks>
    public InfluxObserver(IApiEndpoint endpoint, TimeSpan ttl) {
      if (ttl < TimeSpan.Zero) {
        throw new ArgumentOutOfRangeException("ttl",
          StringResources.ArgumentOutOfRange_NeedNonNegNum);
      }

      endpoint_ = endpoint;

      measures_ = new ConcurrentQueue<InfluxMeasure>();

      TimeSpan half_ttl =
        ttl == TimeSpan.Zero
          ? TimeSpan.FromSeconds(15)
          : TimeSpan.FromSeconds(ttl.TotalSeconds/2);
      scheduler_ = NonReentrantSchedule.Every(half_ttl);
      scheduler_.Run(Post);
    }

    public void Start() {
      scheduler_.Run(Post);
    }

    public void Stop() {
      scheduler_.Stop().WaitOne();
    }

    /// <inheritdoc/>
    public bool Post(IReadOnlyList<InfluxMeasure> measures) {
      var points = new StringBuilder();
      foreach (InfluxMeasure measure in measures) {
        WriteSerie(measure, points);
      }
      return endpoint_.PostSeries(points.ToString());
    }

    void Post() {
      var measures = new List<InfluxMeasure>(measures_.Count);
      int count = 0;
      do {
        // Keep removing measures from the queue until the operation fail or
        // the limit is reached.
        InfluxMeasure measure;
        while (measures_.TryDequeue(out measure) && count < kMaxPointsPerPost) {
          measures.Add(measure);
          count++;
        }

        if (measures.Count > 0) {
          Post(measures.AsReadOnlyList());
        }
        count = 0;
      } while (count >= kMaxPointsPerPost);
    }

    void WriteSerie(InfluxMeasure measure, StringBuilder points) {
      long epoch_ns = measure.Timestamp.ToUnixEpoch().ToNanos(TimeUnit.Seconds);
      points
        .Append(measure.Name)
        .Append(",");

      foreach (string tag in measure.Tags) {
        points
          .Append(tag)
          .Append(",");
      }

      points[points.Length - 1] = ' ';
      points
        .Append("value=")
        .Append(measure.Measure.ToString(CultureInfo.InvariantCulture))
        .Append(" ")
        .Append(epoch_ns)
        .Append("\n");
    }

    public void Observe(Measure measure, DateTime timestamp) {
      var point = new InfluxMeasure(measure, timestamp);
      measures_.Enqueue(point);
    }
  }
}
