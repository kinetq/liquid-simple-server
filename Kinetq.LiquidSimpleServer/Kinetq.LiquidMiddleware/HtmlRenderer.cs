using Fluid;
using HtmlAgilityPack;
using Kinetq.LiquidMiddleware.Exceptions;
using Kinetq.LiquidMiddleware.Helpers;
using Kinetq.LiquidMiddleware.Interfaces;
using Kinetq.LiquidMiddleware.Models;

namespace Kinetq.LiquidMiddleware;

public class HtmlRenderer : IHtmlRenderer
{
    private readonly IFluidParserManager _fluidParserManager;
    private readonly ILiquidFilterManager _liquidFilterManager;
    private readonly ILiquidRegisteredTypesManager _liquidRegisteredTypesManager;

    public HtmlRenderer(IFluidParserManager fluidParserManager, ILiquidFilterManager liquidFilterManager, ILiquidRegisteredTypesManager liquidRegisteredTypesManager)
    {
        _fluidParserManager = fluidParserManager;
        _liquidFilterManager = liquidFilterManager;
        _liquidRegisteredTypesManager = liquidRegisteredTypesManager;
    }

    public async Task<string> RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute)
    {
        if (liquidRoute == null)
        {
            return null;
        }

        var fileInfo = liquidRoute.FileProvider.GetFileInfo(liquidRoute.LiquidTemplatePath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        string liquidTemplate = await fileInfo.GetFileContents();

        var parser = _fluidParserManager.FluidParser;
        if (parser.TryParse(liquidTemplate, out IFluidTemplate fluidTemplate, out string error))
        {
            var options = new TemplateOptions
            {
                FileProvider = liquidRoute.FileProvider,
                MemberAccessStrategy = new DefaultMemberAccessStrategy()
                {
                    MemberNameStrategy = MemberNameStrategies.SnakeCase
                }
            };

            foreach (var registeredType in _liquidRegisteredTypesManager.RegisteredTypes)
            {
                options.MemberAccessStrategy.Register(registeredType);
            }

            foreach (var filterDelegate in _liquidFilterManager.LiquidFilters)
            {
                options.Filters.AddFilter(filterDelegate.Key, filterDelegate.Value);
            }

            var templateContext = new TemplateContext(renderModel, options);

            string html = await fluidTemplate.RenderAsync(templateContext);

            // Validate HTML using HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                throw new HtmlSyntaxException(htmlDoc.ParseErrors);
            }

            return html;
        }

        throw new LiquidSyntaxException(error);
    }
}