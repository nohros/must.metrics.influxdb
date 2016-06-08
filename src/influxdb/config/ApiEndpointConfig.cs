using System;
using System.Configuration;
using Nohros.Configuration;

namespace Nohros.Metrics.Influx.Config
{
  /// <summary>
  /// Defines the configuration elements for the datadog.
  /// </summary>
  public class ApiEndpointConfig : EncryptedConfigurationElement
  {
    /// <summary>
    /// Specifies the name of the host.
    /// </summary>
    /// <remarks>
    /// If the host's name is not specified the value of
    /// <see cref="Environment.MachineName"/> will be used as the host.
    /// </remarks>
    [ConfigurationProperty("Host", IsRequired = false, DefaultValue = "")]
    public string Host {
      get { return (string) this["Host"]; }
      set { this["Host"] = value; }
    }

    /// <summary>
    /// Specifies the uri of the datadog endpoint.
    /// </summary>
    [ConfigurationProperty("Uri", IsRequired = false,
      DefaultValue = "https://app.datadoghq.com/api/v1")]
    public string Uri {
      get { return (string) this["Uri"]; }
      set { this["Uri"] = value; }
    }

    /// <summary>
    /// Specifies the proxy to be used to send the metrics to the datadog's
    /// endpoint.
    /// </summary>
    /// <remarks>
    /// The proxy should be specified in the format:
    /// "http[s]://[username]:[password]@proxy.com"
    /// </remarks>
    [ConfigurationProperty("Proxy", IsRequired = false, DefaultValue = "")]
    public string Proxy {
      get { return (string)this["Proxy"]; }
      set { this["Proxy"] = value; }
    }
  }
}
