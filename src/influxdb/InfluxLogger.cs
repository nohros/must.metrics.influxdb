using System;
using Nohros.Logging;

namespace Nohros.Metrics.Influx
{
  public class InfluxLogger : ForwardingLogger
  {
    static InfluxLogger() {
      ForCurrentProcess = new InfluxLogger(new NOPLogger());
    }

    public InfluxLogger(ILogger logger) : base(logger) {
    }

    public static InfluxLogger ForCurrentProcess { get; set; }
  }
}
