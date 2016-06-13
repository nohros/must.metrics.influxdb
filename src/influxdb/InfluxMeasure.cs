using System;
using System.Linq;
using Nohros.Collections;

namespace Nohros.Metrics.Influx
{
  /// <summary>
  /// 
  /// </summary>
  public class InfluxMeasure
  {
    readonly string name_;
    readonly double measure_;
    readonly string[] tags_;
    readonly DateTime timestamp_;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfluxMeasure"/> class
    /// by using the given <paramref name="measure"/> and
    /// <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="measure">
    /// A <see cref="Measure"/> containing the measured valud and its metadata.
    /// </param>
    /// <param name="timestamp">
    /// The date and time when the <paramref name="measure"/> was measured.
    /// </param>
    public InfluxMeasure(Measure measure, DateTime timestamp) {
      name_ = measure.MetricConfig.Name;
      measure_ = measure.Value;
      timestamp_ = timestamp;

      // influxdb recommend to sort the tags by key for better performance.
      Tag[] tags = measure.MetricConfig.Tags.OrderBy(s => s.Name).ToArray();
      var plain_tags = new string[tags.Length];
      for (int i = 0; i < tags.Length; i++) {
        Tag tag = tags[i];
        plain_tags[i] = tag.Name + "=" + tag.Value;
      }
      tags_ = plain_tags;
    }

    /// <summary>
    /// Gets the name of the measure
    /// </summary>
    public string Name {
      get { return name_; }
    }

    /// <summary>
    /// Gets the measured value.
    /// </summary>
    public double Measure {
      get { return measure_; }
    }

    /// <summary>
    /// Gets a list of tags associated with the measure.
    /// </summary>
    /// <remarks>
    /// The tags is ordered in ascending order.
    /// </remarks>
    public IReadOnlyList<string> Tags {
      get { return tags_.AsReadOnlyList(); }
    }

    /// <summary>
    /// Gets the date and time when the measure was measured.
    /// </summary>
    public DateTime Timestamp {
      get { return timestamp_; }
    }
  }
}
