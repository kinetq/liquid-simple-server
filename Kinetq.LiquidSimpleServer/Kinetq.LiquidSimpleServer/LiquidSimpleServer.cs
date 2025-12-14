using System.Net;
using System.Text;
using Kinetq.LiquidSimpleServer.Helpers;
using Kinetq.LiquidSimpleServer.Interfaces;
using Kinetq.LiquidSimpleServer.Models;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidSimpleServer;

public class LiquidSimpleServer : ILiquidSimpleServer
{
    private readonly HttpListener _listener;
    private readonly int _port;
    private bool _isRunning;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly ILiquidRoutesManager _liquidRoutesManager;
    private readonly ILogger<LiquidSimpleServer> _logger;
    private readonly IHtmlRenderer _htmlRenderer;

    public LiquidSimpleServer(
        ILiquidRoutesManager liquidRoutesManager,
        ILogger<LiquidSimpleServer> logger, 
        IHtmlRenderer htmlRenderer)
    {
        _liquidRoutesManager = liquidRoutesManager;
        _logger = logger;
        _htmlRenderer = htmlRenderer;

        _port = HttpHelpers.GetRandomUnusedPort();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");

        _cancellationTokenSource = new CancellationTokenSource();
    }

    public Uri Prefix => new Uri(_listener.Prefixes.FirstOrDefault() ?? string.Empty);

    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        _listener.Start();
        _isRunning = true;

        _logger.LogInformation($"Liquid web server started on http://localhost:{_port}");

        await Task.Run(() => HandleRequestsAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cancellationTokenSource.Cancel();
        _listener.Stop();

        _logger.LogInformation("Liquid web server stopped");
    }

    private async Task HandleRequestsAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(context), cancellationToken);
            }
            catch (HttpListenerException)
            {
                // Expected when listener is stopped
                break;
            }
            catch (ObjectDisposedException)
            {
                // Expected when listener is disposed
                break;
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var (content, contentType, statusCode) = await HandleRequestAsync(request);

            response.ContentLength64 = content.Length;
            response.ContentType = contentType;
            response.StatusCode = statusCode;

            await response.OutputStream.WriteAsync(content);
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            byte[] errorBuffer = Encoding.UTF8.GetBytes($"Internal Server Error: {ex.Message}");
            response.ContentLength64 = errorBuffer.Length;
            response.ContentType = "text/html";
            await response.OutputStream.WriteAsync(errorBuffer);
        }
        finally
        {
            response.Close();
        }
    }

    private async Task<(byte[] content, string contentType, int statusCode)> HandleRequestAsync(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath;
        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(request.Url?.Query))
        {
            var query = request.Url.Query.TrimStart('?');
            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    queryParams[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
                }
            }
        }

        var renderModel = new RenderModel()
        {
            Route = path,
            QueryParams = queryParams
        };

        LiquidRoute? liquidRoute = _liquidRoutesManager.GetRouteForPath(path);
        if (liquidRoute?.Execute != null)
        {
            try
            {
                renderModel.ViewModel = await liquidRoute.Execute(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing route logic for path {Path}", path);
                var errorRoute = _liquidRoutesManager.GetRouteForStatusCode(HttpStatusCode.InternalServerError);
                var errorHtmlResponse = await _htmlRenderer.RenderHtml(renderModel, errorRoute);
                if (errorHtmlResponse != null)
                {
                    return (Encoding.UTF8.GetBytes(errorHtmlResponse), "text/html", 500);
                }

                return (Encoding.UTF8.GetBytes($"<h1>500 - Internal Server Error</h1><p>{ex.Message}</p>"), "text/html",
                    500);
            }
        }

        // Handle static routes
        var htmlResponse = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        if (htmlResponse != null)
        {
            return (Encoding.UTF8.GetBytes(htmlResponse), "text/html", 200);
        }

        try
        {
            string referer = request.Headers["Referer"];
            Uri refererUri = new Uri(referer);

            LiquidRoute? referrerLiquidRoute = _liquidRoutesManager.GetRouteForPath(refererUri.AbsolutePath);
            var localPath = referrerLiquidRoute?.FileProvider.GetFileInfo(path);
            if (localPath != null && File.Exists(localPath.PhysicalPath))
            {
                var fileContent = await File.ReadAllBytesAsync(localPath.PhysicalPath);
                var contentType = GetContentType(Path.GetExtension(localPath.PhysicalPath));
                return (fileContent, contentType, 200);
            }
        }
        catch
        {
            // Fall through to 404
        }

        var notFoundRoute = _liquidRoutesManager.GetRouteForStatusCode(HttpStatusCode.NotFound);
        var notFoundHtmlResponse = await _htmlRenderer.RenderHtml(renderModel, notFoundRoute);
        if (notFoundHtmlResponse != null)
        {
            return (Encoding.UTF8.GetBytes(notFoundHtmlResponse), "text/html", 200);
        }

        return ("<h1>404 - Page Not Found</h1>"u8.ToArray(), "text/html", 404);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".txt" => "text/plain",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
        _listener?.Close();
    }
}