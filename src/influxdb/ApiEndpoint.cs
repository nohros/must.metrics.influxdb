using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nohros.Extensions;

namespace Nohros.Metrics.Influx
{
  /// <summary>
  /// A implementation of the <see cref="IApiEndpoint"/> class.
  /// </summary>
  internal class ApiEndpoint : IApiEndpoint
  {
    /// <summary>
    /// Series retry metadata.
    /// </summary>
    class Retry
    {
      public Retry(string series) {
        Series = series;
        RetryAttempt = 0;
      }

      public string Series { get; set; }
      public int RetryAttempt { get; set; }
    }

    const string kClassName = "Nohros.Metrics.Datadog.ApiEndpoint";

    const int kMaxRetryAttempts = 5;

    const string kRequestPath = "write?db={0}";

    readonly CookieContainer cookies_;
    readonly Uri request_uri_;
    readonly InfluxLogger logger_;
    readonly IWebProxy proxy_;
    readonly Queue<Retry> retries_;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpoint"/> by
    /// using the given endpoint uri and database.
    /// </summary>
    /// <param name="endpoint_uri">
    /// The address to the influxdb endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    public ApiEndpoint(string endpoint_uri, string database)
      : this(new Uri(endpoint_uri), database, string.Empty) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpoint"/> by
    /// using the given endpoint uri and api key.
    /// </summary>
    /// <param name="endpoint_uri">
    /// The address to the influxdb endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    /// <param name="proxy">
    /// A string containing the proxy to be used to post the series to
    /// influx servers. The proxy should be specified in the format:
    /// "http[s]://[username]:[password]@proxy.com"
    /// </param>
    public ApiEndpoint(string endpoint_uri, string database, string proxy)
      : this(new Uri(endpoint_uri), database, proxy) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpoint"/> by
    /// using the given endpoint uri and api key.
    /// </summary>
    /// <param name="base_uri">
    /// The address to the influx endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    public ApiEndpoint(Uri base_uri, string database)
      : this(base_uri, database, string.Empty) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpoint"/> by
    /// using the given endpoint uri and api key.
    /// </summary>
    /// <param name="base_uri">
    /// The address to the influxdb endpoint.
    /// </param>
    /// <param name="database">
    /// The name of the database that should be used to store the metrics.
    /// </param>
    /// <param name="proxy">
    /// A string containing the proxy to be used to post the series to
    /// influx servers. The proxy should be specified in the format:
    /// "http[s]://[username]:[password]@proxy.com"
    /// </param>
    public ApiEndpoint(Uri base_uri, string database, string proxy) {
      if (base_uri == null || database == null) {
        throw new ArgumentNullException(base_uri == null
          ? "base_uri"
          : "database");
      }

      request_uri_ = new Uri(base_uri, kRequestPath.Fmt(database));
      cookies_ = new CookieContainer();

      logger_ = InfluxLogger.ForCurrentProcess;

      ServicePointManager.Expect100Continue = false;

      proxy_ = GetProxy(proxy == null ? string.Empty : proxy.Trim());

      retries_ = new Queue<Retry>();
    }

    IWebProxy GetProxy(string proxy) {
      IWebProxy default_proxy = WebRequest.DefaultWebProxy;
      if (proxy == string.Empty) {
        return default_proxy;
      }

      string[] uri_parts = proxy.Split('@');
      if (uri_parts.Length == 1) {
        return new WebProxy(uri_parts[0]);
      }

      if (uri_parts.Length != 2) {
        return default_proxy;
      }

      var web_proxy = new WebProxy(uri_parts[1]);
      string[] login_parts = uri_parts[0].Split(':');
      if (login_parts.Length != 2) {
        return default_proxy;
      }

      web_proxy.Credentials =
        new NetworkCredential(login_parts[0], login_parts[1]);
      return web_proxy;
    }

    /// <inheritdoc/>
    public bool PostSeries(string series) {
      bool posted = Post(series);
      if (posted) {
        // The series was sucessfully posted, lets try to post the series
        // that has been failed in the past.
        while (retries_.Count > 0) {
          Retry retry = retries_.Peek();
          if (retry.RetryAttempt <= kMaxRetryAttempts) {
            posted = Post(retry.Series);
            if (!posted) {
              retry.RetryAttempt++;
              break;
            }
          } else {
            logger_.Warn(R.Endpoint_GivingUpRetry.Fmt(series));
          }
          retries_.Dequeue();
        }
      } else {
        retries_.Enqueue(new Retry(series));
      }
      return posted;
    }

    bool Post(string series) {
      try {
        using (var response = HttpPost(series)) {
          return HasSucceed(response);
        }
      } catch (WebException ex) {
        switch (ex.Status) {
          case WebExceptionStatus.KeepAliveFailure:
          case WebExceptionStatus.ConnectFailure:
          case WebExceptionStatus.ConnectionClosed:
          case WebExceptionStatus.Timeout:
            logger_.Error(R.Endpoint_WebException_PostFailRetry, ex);
            return false;
        }
        throw;
      } catch (IOException io) {
        logger_.Error(R.Endpoint_WebException_PostFailRetry, io);
        return false;
      }
    }

    bool HasSucceed(HttpWebResponse response) {
      bool accepted = response.StatusCode == HttpStatusCode.Accepted;
      if (!accepted) {
        logger_.Warn(R.Endpoint_SeriesPostFail.Fmt(response.StatusDescription));
      }
      return accepted;
    }

    HttpWebResponse HttpPost(string series) {
      var request = CreateRequest();
      request.CookieContainer = cookies_;
      request.Accept = "*/*";
      request.ContentType = "text/plain; charset=utf-8";

      byte[] data = Encoding.UTF8.GetBytes(series);

      request.Method = "POST";
      request.ContentLength = data.Length;

      using (Stream stream = request.GetRequestStream()) {
        stream.Write(data, 0, data.Length);
        stream.Close();
      }

      return (HttpWebResponse) request.GetResponse();
    }

    HttpWebRequest CreateRequest() {
      var request = (HttpWebRequest) WebRequest.Create(request_uri_);
      request.Proxy = proxy_;
      return request;
    }
  }
}
