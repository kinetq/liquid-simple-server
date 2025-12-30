using System.Collections.Specialized;

namespace Kinetq.LiquidMiddleware.Models;

public class LiquidRequestModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public object? Body { get; set; }
    public NameValueCollection Headers { get; set; }
}