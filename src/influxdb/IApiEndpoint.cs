namespace Nohros.Metrics.Influx
{
  public interface IApiEndpoint
  {
    bool PostSeries(string series);
  }
}
