using System.Text.RegularExpressions;
using Fluid;
using Fluid.Values;
using HtmlAgilityPack;
using Kinetq.LiquidMiddleware.Exceptions;
using Kinetq.LiquidMiddleware.Interfaces;
using Kinetq.LiquidMiddleware.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Kinetq.LiquidMiddleware.Tests;

public class HtmlRendererTests : IAsyncLifetime
{
    private Mock<IFluidParserManager> _fluidParserManagerMock;
    private Mock<ILiquidFilterManager> _liquidFilterManagerMock;
    private Mock<ILiquidRegisteredTypesManager> _liquidRegisteredTypesManagerMock;
    private IHtmlRenderer _htmlRenderer;
    private IFileProvider _embeddedFileProvider;
    private IFileProvider _phyicalFileProvider;
    public Task InitializeAsync()
    {
        _fluidParserManagerMock = new Mock<IFluidParserManager>();
        _liquidFilterManagerMock = new Mock<ILiquidFilterManager>();
        _liquidRegisteredTypesManagerMock = new Mock<ILiquidRegisteredTypesManager>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_fluidParserManagerMock.Object);
        serviceCollection.AddSingleton(_liquidFilterManagerMock.Object);
        serviceCollection.AddSingleton(_liquidRegisteredTypesManagerMock.Object);
        serviceCollection.AddSingleton<IHtmlRenderer, HtmlRenderer>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _htmlRenderer = serviceProvider.GetRequiredService<IHtmlRenderer>();
        _embeddedFileProvider = new EmbeddedFileProvider(typeof(LiquidSimpleServerTests).Assembly, "Kinetq.LiquidMiddleware.Tests.Templates");
        string executingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        _phyicalFileProvider = new PhysicalFileProvider(executingDirectory);

        _fluidParserManagerMock.Setup(x => x.FluidParser).Returns(new FluidParser());
        _liquidFilterManagerMock.Setup(x => x.LiquidFilters).Returns(new Dictionary<string, FilterDelegate>());
        _liquidRegisteredTypesManagerMock.Setup(x => x.RegisteredTypes).Returns(new List<Type>()
        {
            typeof(RenderViewModel),
            typeof(Page)
        });

        return Task.CompletedTask;
    }

    [Fact]
    private async Task Can_Find_Embedded_Templates()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _embeddedFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>()
        };

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.NotNull(html);
    }

    [Fact]
    private async Task Can_Find_Physical_Templates()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _phyicalFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>()
        };

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.NotNull(html);
    }

    [Fact]
    private async Task Can_Render_View_Model()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _phyicalFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>(),
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var headingNode = htmlDoc.DocumentNode.SelectSingleNode("//h2");
        Assert.Equal("Test Heading", headingNode.InnerText);
    }

    [Fact]
    private async Task Rertuns_Errors_For_Malformed_Liquid_Syntax()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _phyicalFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "malformed.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>(),
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        await Assert.ThrowsAsync<LiquidSyntaxException>(async () => await _htmlRenderer.RenderHtml(renderModel, liquidRoute));
    }

    [Fact]
    private async Task Rertuns_Errors_For_Malformed_HTML_Syntax()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _phyicalFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "malformed_html.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>(),
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        await Assert.ThrowsAsync<HtmlSyntaxException>(async () => await _htmlRenderer.RenderHtml(renderModel, liquidRoute));
    }

    [Fact]
    private async Task Returns_Posts_From_Registered_Filter()
    {
        var liquidRoute = new LiquidRoute()
        {
            FileProvider = _phyicalFileProvider,
            RoutePattern = new Regex("^/$"),
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            Route = "/",
            QueryParams = new Dictionary<string, string>(),
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        _liquidFilterManagerMock.Setup(x => x.LiquidFilters)
            .Returns(new Dictionary<string, FilterDelegate>()
            {
                {
                    "get_posts", (input, arguments, context) =>
                    {
                        var posts = new List<Post>()
                        {
                            new Post()
                            {
                                Title = "First Post",
                                Url = "/posts/first-post",
                                Date = new DateTime(2024, 1, 1)
                            },
                            new Post()
                            {
                                Title = "Second Post",
                                Url = "/posts/second-post",
                                Date = new DateTime(2024, 1, 1)
                            },
                        };
                        return FluidValue.Create(posts, context.Options);
                    }
                }
            });

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var posts = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, 'post')]");
        Assert.Equal(2, posts.Count);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

public class RenderViewModel
{
    public Page Page { get; set; }
}

public class Page
{
    public string Heading { get; set; }
}

public class Post
{
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
}