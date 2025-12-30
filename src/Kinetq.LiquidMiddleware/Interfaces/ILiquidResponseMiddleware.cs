using Kinetq.LiquidMiddleware.Models;

namespace Kinetq.LiquidMiddleware.Interfaces;

public interface ILiquidResponseMiddleware
{
    Task<(byte[] content, string contentType, int statusCode)> HandleRequestAsync(LiquidRequestModel request);
}