using System.Collections.Concurrent;
using Fluid;
using Kinetq.LiquidMiddleware.Interfaces;

namespace Kinetq.LiquidMiddleware.Managers;

public class LiquidFilterManager : ILiquidFilterManager
{
    private readonly Lazy<IDictionary<string, FilterDelegate>> _liquidFilters =
        new(() => new ConcurrentDictionary<string, FilterDelegate>());

    public IDictionary<string, FilterDelegate> LiquidFilters => _liquidFilters.Value;

    public void RegisterFilter(string name, FilterDelegate filterDelegate)
    {
        if (!LiquidFilters.TryGetValue(name, out var value))
        {
            _liquidFilters.Value[name] = filterDelegate;
        }
    }
}