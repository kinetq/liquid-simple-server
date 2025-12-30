using System.Net;
using System.Net.Sockets;

namespace Kinetq.LiquidMiddleware.Helpers;

public static class HttpHelpers
{
    public static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static IDictionary<string, string> GetQueryParams(this HttpListenerRequest request)
    {
        var queryParams = new Dictionary<string, string>();
        var queryString = request.Url.Query;

        if (!string.IsNullOrEmpty(queryString))
        {
            var queryPairs = queryString.TrimStart('?').Split('&');

            foreach (var pair in queryPairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    queryParams[WebUtility.UrlDecode(keyValue[0])] = WebUtility.UrlDecode(keyValue[1]);
                }
            }
        }

        return queryParams;
    }
}