using System;
using System.Collections.Generic;
using System.Linq;
using Nohros.Metrics.Influx.Config;
using Nohros.Metrics.Reporting;

namespace Nohros.Metrics.Influx
{
  /// <summary>
  /// Defines a factory for the <see cref="IMeasureObserver"/> implementation
  /// for the influx's.
  /// </summary>
  public class InfluxObserverFactory
  {
    /// <summary>
    /// Creates a instance of the influx's implementation of the
    /// <see cref="IInfluxMeasureObserver"/> for each endpoint defined by the
    /// <paramref name="endpoints"/>.
    /// </summary>
    /// <param name="endpoints">
    /// A <see cref="ApiEndpointsConfig"/> object containing the configuration
    /// information for the influx's endpoints.
    /// </param>
    /// <returns></returns>
    public IEnumerable<IInfluxMeasureObserver> Create(
      ApiEndpointsConfig endpoints) {
      return endpoints
        .ApiEndpoints
        .Cast<ApiEndpointConfig>()
        .Select(Create);
    }

    /// <summary>
    /// Creates a instance of the influx's implementation of the
    /// <see cref="IInfluxMeasureObserver"/> for the given
    /// <paramref name="endpoint"/>.
    /// </summary>
    /// <param name="endpoint">
    /// A <see cref="ApiEndpointConfig"/> object containing the configuration
    /// information for the influx's endpoint.
    /// </param>
    /// <returns></returns>
    public IInfluxMeasureObserver Create(ApiEndpointConfig endpoint) {
      return Create(endpoint.Uri, endpoint.Database, endpoint.Proxy);
    }

    /// <summary>
    /// Creates a instance of the influx's implementation of the
    /// <see cref="IInfluxMeasureObserver"/> by using the given
    /// <paramref name="endpoint_uri"/> proxy information.
    /// </summary>
    /// <param name="endpoint_uri">
    /// A <see cref="ApiEndpointConfig"/> object containing the configuration
    /// information for the influx's endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    /// <returns></returns>
    public IInfluxMeasureObserver Create(string endpoint_uri,
      string database) {
      return Create(endpoint_uri, database, string.Empty);
    }

    /// <summary>
    /// Creates a instance of the influx's implementation of the
    /// <see cref="IInfluxMeasureObserver"/> by using the given
    /// <paramref name="endpoint_uri"/> proxy information.
    /// </summary>
    /// <param name="endpoint_uri">
    /// A <see cref="ApiEndpointConfig"/> object containing the configuration
    /// information for the influx's endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    /// <param name="proxy">
    /// Specifies the proxy to be used to send the metrics to the influx's
    /// endpoint. This value should be specified in the format:
    /// "http[s]://[username]:[password]@proxy.com"
    /// </param>
    /// <returns></returns>
    public IInfluxMeasureObserver Create(string endpoint_uri,
      string database,
      string proxy) {
      var endpoint = new ApiEndpoint(endpoint_uri, database, proxy);
      return new InfluxObserver(endpoint);
    }
  }
}
