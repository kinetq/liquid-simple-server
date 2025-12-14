using System.Text.RegularExpressions;
using Kinetq.LiquidSimpleServer.Helpers;
using Kinetq.LiquidSimpleServer.Interfaces;
using Kinetq.LiquidSimpleServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Kinetq.LiquidSimpleServer.Tests
{
    public class LiquidSimpleServerTests : IAsyncLifetime
    {
        private ILiquidSimpleServer _liquidSimpleServer;
        private ILiquidFilterManager _liquidFilterManager;
        private ILiquidRegisteredTypesManager _liquidRegisteredTypesManager;
        private ILiquidRoutesManager _liquidRoutesManager;
        private IFluidParserManager _fluidParserManager;
        private IFileProvider _fileProvider;

        public async Task InitializeAsync()
        {
            var serviceCollection = new ServiceCollection();

            var serviceProvider = serviceCollection
                .AddLiquidSimpleServer()
                .BuildServiceProvider();

            _liquidSimpleServer = serviceProvider.GetRequiredService<ILiquidSimpleServer>();
            _liquidFilterManager = serviceProvider.GetRequiredService<ILiquidFilterManager>();
            _liquidRegisteredTypesManager = serviceProvider.GetRequiredService<ILiquidRegisteredTypesManager>();
            _liquidRoutesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();
            _fluidParserManager = serviceProvider.GetRequiredService<IFluidParserManager>();
            _fileProvider = new EmbeddedFileProvider(typeof(LiquidSimpleServerTests).Assembly, "Kinetq.LiquidSimpleServer.Tests.Templates");

            _liquidRoutesManager.RegisterRoute(new LiquidRoute()
            {
                FileProvider = _fileProvider,
                RoutePattern = new Regex("^/$"),
                LiquidTemplatePath = "index.liquid"
            });

            await _liquidSimpleServer.StartAsync();
        }

        public void Test_Index_Renders()
        {

        }

        public Task DisposeAsync()
        {
            _liquidSimpleServer.Stop();
            return Task.CompletedTask;
        }
    }
}
