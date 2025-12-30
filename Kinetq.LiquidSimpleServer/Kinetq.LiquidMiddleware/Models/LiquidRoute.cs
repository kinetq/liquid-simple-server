using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidMiddleware.Models;

public partial class LiquidRoute
{
    public Regex RoutePattern { get; set; }
    public string LiquidTemplatePath { get; set; }
    public IFileProvider FileProvider { get; set; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; set; }
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
}