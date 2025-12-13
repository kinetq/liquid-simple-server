using Fluid;

namespace Kinetq.LiquidSimpleServer.Interfaces;

public interface ILiquidFilterManager
{
    IDictionary<string, FilterDelegate> LiquidFilters { get; }
    void RegisterFilter(string name, FilterDelegate filterDelegate);
}