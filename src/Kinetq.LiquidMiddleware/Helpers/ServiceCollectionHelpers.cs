using Kinetq.LiquidMiddleware.Interfaces;
using Kinetq.LiquidMiddleware.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetq.LiquidMiddleware.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceCollection AddLiquidSimpleServer(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILiquidFilterManager, LiquidFilterManager>();
        serviceCollection.AddSingleton<ILiquidRegisteredTypesManager, LiquidRegisteredTypesManager>();
        serviceCollection.AddSingleton<ILiquidRoutesManager, LiquidRoutesManager>();
        serviceCollection.AddSingleton<IFluidParserManager, FluidParserManager>();
        serviceCollection.AddScoped<IHtmlRenderer, HtmlRenderer>();
        serviceCollection.AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>();

        return serviceCollection;
    }
}