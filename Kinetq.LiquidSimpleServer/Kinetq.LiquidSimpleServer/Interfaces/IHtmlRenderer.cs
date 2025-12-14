using Fluid;
using HtmlAgilityPack;
using Kinetq.LiquidSimpleServer.Managers;
using Kinetq.LiquidSimpleServer.Models;

namespace Kinetq.LiquidSimpleServer.Interfaces;

public interface IHtmlRenderer
{
    Task<string> RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute);
}