namespace Jellyfin.Plugin.DD.Danmaku.Services;

public interface IResourceService
{
    Task<Stream?> OpenAsync(string resourcePath, CancellationToken cancellationToken);
    string GetContentType(string resourcePath);
}

public sealed class ResourceService : IResourceService
{
    private readonly string? _rootPath;

    public ResourceService() { }

    public ResourceService(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
    }

    public Task<Stream?> OpenAsync(string resourcePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var relative = resourcePath.Replace('\\', '/').TrimStart('/');
        if (relative.Contains("..", StringComparison.Ordinal)
            || !(relative == "Resources/ede.js"
                || relative.StartsWith("Resources/Admin/", StringComparison.Ordinal)))
            return Task.FromResult<Stream?>(null);
        var embedded = typeof(ResourceService).Assembly
            .GetManifestResourceStream("Jellyfin.Plugin.DD.Danmaku." + relative);
        if (embedded is not null) return Task.FromResult<Stream?>(embedded);
        if (_rootPath is null) return Task.FromResult<Stream?>(null);
        var fullPath = ResolvePath(relative);
        if (fullPath is null || !File.Exists(fullPath)) return Task.FromResult<Stream?>(null);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public string GetContentType(string resourcePath)
    {
        var extension = Path.GetExtension(resourcePath).ToLowerInvariant();
        return extension switch
        {
            ".js" => "application/javascript; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".html" => "text/html; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".woff2" => "font/woff2",
            _ => "application/octet-stream"
        };
    }

    private string? ResolvePath(string resourcePath)
    {
        if (_rootPath is null) return null;
        var relative = resourcePath.Replace('\\', '/').TrimStart('/');
        if (relative.Contains("..", StringComparison.Ordinal) ||
            !relative.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase)) return null;
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relative["Resources/".Length..]));
        return fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : null;
    }
}
