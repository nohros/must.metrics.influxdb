using System.Configuration;

namespace Nohros.Metrics.Influx.Config
{
  /// <summary>
  /// Defines the configuration section that will contains the configuration
  /// information for infliuxdb's endpoints.
  /// </summary>
  public class ApiEndpointsConfig : ConfigurationSection
  {
    [ConfigurationProperty("", IsRequired = false, IsKey = false,
      IsDefaultCollection = true)]
    public ApiEndpointConfigCollection ApiEndpoints {
      get { return this[""] as ApiEndpointConfigCollection; }
      set { this[""] = value; }
    }
  }
}
