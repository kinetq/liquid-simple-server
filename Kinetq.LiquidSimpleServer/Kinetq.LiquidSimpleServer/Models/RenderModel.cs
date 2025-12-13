namespace Kinetq.LiquidSimpleServer.Models;

public class RenderModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public object ViewModel { get; set; }
}