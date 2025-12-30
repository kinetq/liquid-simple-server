using System.Net;
using System.Text.RegularExpressions;
using Kinetq.LiquidMiddleware.Interfaces;
using Kinetq.LiquidMiddleware.Models;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidMiddleware.Managers;

public class LiquidRoutesManager : ILiquidRoutesManager
{
    private readonly ILogger<LiquidRoutesManager> _logger;

    private readonly Lazy<IList<LiquidRoute>> _liquidRoutes =
        new(() => new List<LiquidRoute>());

    private readonly Lazy<IDictionary<int, LiquidRoute>> _errorRoutes =
                new(() => new Dictionary<int, LiquidRoute>());

    public LiquidRoutesManager(ILogger<LiquidRoutesManager> logger)
    {
        _logger = logger;
    }

    public IList<LiquidRoute> LiquidRoutes => _liquidRoutes.Value;
    public IDictionary<int, LiquidRoute> ErrorRoutes => _errorRoutes.Value;

    public void RegisterRoute(LiquidRoute route)
    {
        if (LiquidRoutes.Any(r => r.RoutePattern.Equals(route.RoutePattern)))
        {
            _logger.LogWarning("Route already exists: {Route}", route.RoutePattern);
            return;
        }

        LiquidRoutes.Add(route);
        _logger.LogDebug("Added route: {Route}", route);
    }

    public void RegisterErrorRoute(int statusCode, LiquidRoute route)
    {
        if (_errorRoutes.Value.ContainsKey(statusCode))
        {
            _logger.LogWarning("Error route already exists for status code {StatusCode}", statusCode);
            return;
        }

        _errorRoutes.Value[statusCode] = route;
        _logger.LogDebug("Added error route for status code {StatusCode}: {Route}", statusCode, route);
    }

    public LiquidRoute? GetRouteForStatusCode(HttpStatusCode statusCode)
    {
        _errorRoutes.Value.TryGetValue((int)statusCode, out var route);
        return route;
    }

    public LiquidRoute? GetRouteForPath(string path, IDictionary<string, string>? queryParams = null)
    {
        LiquidRoute? liquidRoute = null;
        foreach (var route in LiquidRoutes)
        {
            var match = route.RoutePattern.Match(path);
            if (match.Success)
            {
                if (queryParams != null)
                {
                    foreach (Group group in match.Groups)
                    {
                        queryParams[group.Name] = Uri.UnescapeDataString(group.Value);
                    }
                }

                liquidRoute = route;
                break;
            }
        }

        return liquidRoute;
    }
}