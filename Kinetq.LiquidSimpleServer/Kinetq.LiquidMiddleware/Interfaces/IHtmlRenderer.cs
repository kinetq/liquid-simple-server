using Kinetq.LiquidMiddleware.Models;

namespace Kinetq.LiquidMiddleware.Interfaces;

public interface IHtmlRenderer
{
    Task<string> RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute);
}