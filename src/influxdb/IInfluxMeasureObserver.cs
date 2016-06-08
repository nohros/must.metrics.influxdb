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
  }
}
