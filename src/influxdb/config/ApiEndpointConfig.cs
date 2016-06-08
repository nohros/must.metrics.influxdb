using System;
using System.Configuration;
using Nohros.Configuration;

namespace Nohros.Metrics.Influx.Config
{
  /// <summary>
  /// Defines the configuration elements for the influxdb.
  /// </summary>
  public class ApiEndpointConfig : EncryptedConfigurationElement
  {
    /// <summary>
    /// Specifies the uri of the influxdb endpoint.
    /// </summary>
    [ConfigurationProperty("Database", IsRequired = true)]
    public string Database {
      get { return (string) this["Database"]; }
      set { this["Database"] = value; }
    }

    /// <summary>
    /// Specifies the uri of the influxdb endpoint.
    /// </summary>
    [ConfigurationProperty("Uri", IsRequired = true)]
    public string Uri {
      get { return (string) this["Uri"]; }
      set { this["Uri"] = value; }
    }

    /// <summary>
    /// Specifies the proxy to be used to send the metrics to the influxdb's
    /// endpoint.
    /// </summary>
    /// <remarks>
    /// The proxy should be specified in the format:
    /// "http[s]://[username]:[password]@proxy.com"
    /// </remarks>
    [ConfigurationProperty("Proxy", IsRequired = false, DefaultValue = "")]
    public string Proxy {
      get { return (string) this["Proxy"]; }
      set { this["Proxy"] = value; }
    }
  }
}
