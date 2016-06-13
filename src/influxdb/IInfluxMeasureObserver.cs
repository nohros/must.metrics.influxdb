using Nohros.Collections;
using Nohros.Metrics.Reporting;

namespace Nohros.Metrics.Influx
{
  /// <summary>
  /// Defines a <see cref="IMeasureObserver"/> that send measures
  /// to a influx endpoint.
  /// </summary>
  /// <remarks>
  /// </remarks>
  public interface IInfluxMeasureObserver : IMeasureObserver
  {
    /// <summary>
    /// Start sending metrics to the influx's endpoint.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop sending metrics to the influx's endpoint.
    /// </summary>
    void Stop();

    /// <summary>
    /// Posts a collection of <see cref="InfluxMeasure"/> to a influxdb
    /// endpoint synchronously.
    /// </summary>
    /// <param name="measures">
    /// The collection of <see cref="InfluxMeasure"/> to be posted.
    /// </param>
    bool Post(IReadOnlyList<InfluxMeasure> measures);
  }
}
