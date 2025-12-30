using Fluid;

namespace Kinetq.LiquidMiddleware.Interfaces;

public interface ILiquidFilterManager
{
    IDictionary<string, FilterDelegate> LiquidFilters { get; }
    void RegisterFilter(string name, FilterDelegate filterDelegate);
}