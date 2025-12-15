using System.Net;
using System.Text.RegularExpressions;
using Kinetq.LiquidSimpleServer.Helpers;
using Kinetq.LiquidSimpleServer.Interfaces;
using Kinetq.LiquidSimpleServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kinetq.LiquidSimpleServer.Tests
{
    public class LiquidSimpleServerTests : IAsyncLifetime
    {
        private ILiquidSimpleServer _liquidSimpleServer;
        private Mock<ILiquidRoutesManager> _liquidRoutesManagerMock;
        private Mock<IHtmlRenderer> _htmlRendererMock;
        private IFileProvider _embeddedFileProvider;

        public async Task InitializeAsync()
        {
            _liquidRoutesManagerMock = new Mock<ILiquidRoutesManager>();
            _htmlRendererMock = new Mock<IHtmlRenderer>();

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddSingleton(_liquidRoutesManagerMock.Object)
                .AddSingleton(_htmlRendererMock.Object)
                .AddSingleton<ILiquidSimpleServer, LiquidSimpleServer>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _liquidSimpleServer = serviceProvider.GetRequiredService<ILiquidSimpleServer>();
            _embeddedFileProvider = new EmbeddedFileProvider(typeof(LiquidSimpleServerTests).Assembly, "Kinetq.LiquidSimpleServer.Tests.Templates");

            Task.Run(async () => await _liquidSimpleServer.StartAsync());
        }

        public Task DisposeAsync()
        {
            _liquidSimpleServer.Stop();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetHomePageAsync_ShouldReturnRenderedHtml_WhenRouteExists()
        {
            // Arrange
            const string expectedRoute = "/";
            const string expectedRenderedHtml = "<html><body>Welcome to Home Page</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForPath(expectedRoute, It.IsAny<IDictionary<string, string>>()))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "index.liquid",
                    FileProvider = null // Not needed for this test
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync(expectedRenderedHtml);

            // Act
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_liquidSimpleServer.Prefix}");
            var actualHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNotFoundAsync_ShouldReturnRenderedHtml_WhenRouteExists()
        {
            // Arrange
            const string expectedRenderedHtml = "<html><body>Not Found</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForStatusCode(HttpStatusCode.NotFound))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "404.liquid",
                    FileProvider = null
                });

            _htmlRendererMock
                .SetupSequence(x => 
                    x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null) // First call returns null to simulate not found
                .ReturnsAsync(expectedRenderedHtml);

            // Act
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_liquidSimpleServer.Prefix}");
            var actualHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.False(response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData("/styles/styles.css")]
        [InlineData("/scripts/site.js")]
        [InlineData("/assets/data.json")]
        [InlineData("/assets/image.svg")]
        [InlineData("/assets/image.png")]
        [InlineData("/assets/image.jpeg")]
        public async Task GetHomePageAsync_ShouldReturnAssetFile_WhenRouteExists(string assetPath)
        {
            // Arrange
            const string expectedRoute = "/";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForPath(expectedRoute, It.IsAny<IDictionary<string, string>>()))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "index.liquid",
                    FileProvider = _embeddedFileProvider // Not needed for this test
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null);

            // Act
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_liquidSimpleServer.Prefix}{assetPath.Substring(1, assetPath.Length - 1)}");
            request.Headers.Add("Referer", _liquidSimpleServer.Prefix.ToString());
            var response = await httpClient.SendAsync(request);
            var assetByteArray = await response.Content.ReadAsByteArrayAsync();

            // Load expected CSS content from embedded file
            var fileInfo = _embeddedFileProvider.GetFileInfo(assetPath);
            var fileBytes = await fileInfo.GetFileContentsBytes();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(fileBytes, assetByteArray);
        }
    }
}
