using System.Net;
using Microsoft.Extensions.FileProviders;
using System.Text.RegularExpressions;

namespace Kinetq.LiquidSimpleServer.Models;

public class LiquidRoute
{
    public Regex RoutePattern { get; set; }
    public string LiquidTemplatePath { get; set; }
    public IFileProvider FileProvider { get; set; }
    public Func<HttpListenerRequest, Task<object>> Execute { get; set; }
}