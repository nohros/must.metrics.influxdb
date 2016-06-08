using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nohros.Concurrent;
using Nohros.Extensions.Time;
using System.Linq;
using Nohros.Resources;

namespace Nohros.Metrics.Influx
{
  internal class InfluxObserver : IInfluxMeasureObserver
  {
    class Serie
    {
      public string Name { get; set; }
      public double Measure { get; set; }
      public string[] Tags { get; set; }
      public DateTime Timestamp { get; set; }
    }

    const int kMaxPointsPerPost = 5000;

    readonly IApiEndpoint endpoint_;
    readonly ConcurrentQueue<Serie> measures_;
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

      measures_ = new ConcurrentQueue<Serie>();

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

    void Post() {
      var series = new List<Serie>(measures_.Count);

      int count = 0;
      do {
        // Keep removing series from the queue until the operation fail or the
        // limit is reached.
        Serie serie;
        while (measures_.TryDequeue(out serie) && count < kMaxPointsPerPost) {
          series.Add(serie);
          count++;
        }

        if (series.Count > 0) {
          var points = new StringBuilder();
          foreach (var s in series) {
            WriteSerie(s, points);
          }
          endpoint_.PostSeries(points.ToString());
        }
        count = 0;
      } while (count >= kMaxPointsPerPost);
    }

    void WriteSerie(Serie serie, StringBuilder points) {
      long epoch_ns = serie.Timestamp.ToUnixEpoch().ToNanos(TimeUnit.Seconds);
      points
        .Append(serie.Name)
        .Append(",")
        .Append(serie.Tags)
        .Append(" ")
        .Append("value=")
        .Append(serie.Measure.ToString(CultureInfo.InvariantCulture))
        .Append(" ")
        .Append(epoch_ns)
        .Append("\n");
    }

    public void Observe(Measure measure, DateTime timestamp) {
      var serie = new Serie {
        Measure = measure.Value,
        Name = measure.MetricConfig.Name,
        Timestamp = timestamp
      };

      // influxdb recommendto sort the tags by key for better performance
      Tag[] tags = measure.MetricConfig.Tags.OrderBy(s => s.Name).ToArray();
      var plain_tags = new string[tags.Length];
      for (int i = 0; i < tags.Length; i++) {
        Tag tag = tags[i];
        plain_tags[i] = tag.Name + "=" + tag.Value;
      }
      serie.Tags = plain_tags;

      measures_.Enqueue(serie);
    }
  }
}
