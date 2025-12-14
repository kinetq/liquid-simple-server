using Kinetq.LiquidSimpleServer.Interfaces;
using Kinetq.LiquidSimpleServer.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetq.LiquidSimpleServer.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceCollection AddLiquidSimpleServer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILiquidSimpleServer, LiquidSimpleServer>();
        serviceCollection.AddSingleton<ILiquidFilterManager, LiquidFilterManager>();
        serviceCollection.AddSingleton<ILiquidRegisteredTypesManager, LiquidRegisteredTypesManager>();
        serviceCollection.AddSingleton<ILiquidRoutesManager, LiquidRoutesManager>();
        serviceCollection.AddSingleton<IFluidParserManager, FluidParserManager>();
        serviceCollection.AddScoped<IHtmlRenderer, HtmlRenderer>();

        return serviceCollection;
    }
}