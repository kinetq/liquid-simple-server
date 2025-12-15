using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidSimpleServer.Helpers;

public static class FileProviderHelpers
{
    public static async Task<string> GetFileContents(this IFileInfo fileInfo)
    {
        string liquidTemplate;
        // Check if file is embedded (no physical path) or physical file
        if (string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            // Embedded file - use FileProvider stream
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            liquidTemplate = await reader.ReadToEndAsync();
        }
        else
        {
            // Physical file - use File.ReadAllTextAsync
            liquidTemplate = await File.ReadAllTextAsync(fileInfo.PhysicalPath);
        }

        return liquidTemplate;
    }

    public static async Task<byte[]> GetFileContentsBytes(this IFileInfo fileInfo)
    {
        byte[] fileContents;
        // Check if file is embedded (no physical path) or physical file
        if (string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            // Embedded file - use FileProvider stream
            using var stream = fileInfo.CreateReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            fileContents = memoryStream.ToArray();
        }
        else
        {
            // Physical file - use File.ReadAllBytesAsync
            fileContents = await File.ReadAllBytesAsync(fileInfo.PhysicalPath);
        }

        return fileContents;
    }

    public static async Task<string> GetFileContentType(this IFileInfo fileInfo)
    {
        string? contentType;
        // Check if file is embedded (no physical path) or physical file
        if (string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            // Embedded file - use FileProvider to get content type
            contentType = GetContentType(Path.GetExtension(fileInfo.Name));
        }
        else
        {
            // Physical file - use File.ReadAllTextAsync
            contentType = GetContentType(Path.GetExtension(fileInfo.PhysicalPath));
        }

        return contentType;
    }

    public static string? GetContentType(this string extension)
    {
        return extension.ToLowerInvariant() switch
        {
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
            _ => null
        };
    }
}