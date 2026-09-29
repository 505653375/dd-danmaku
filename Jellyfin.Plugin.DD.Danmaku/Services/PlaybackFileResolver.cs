namespace Jellyfin.Plugin.DD.Danmaku.Services;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;

internal sealed class PlaybackFileResolver
{
    private readonly ILibraryManager _libraryManager;
    private readonly IFileSystem _fileSystem;

    internal PlaybackFileResolver(ILibraryManager libraryManager, IFileSystem fileSystem)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    internal Task<string?> ResolveAsync(string itemId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(itemId) || itemId.Length > 64 || itemId.Any(char.IsControl))
            throw new ArgumentException("媒体标识无效", nameof(itemId));

        if (!Guid.TryParse(itemId, out var guid) || guid == Guid.Empty)
            return Task.FromResult<string?>(null);

        token.ThrowIfCancellationRequested();
        var item = _libraryManager.GetItemById(guid);
        if (item is not Video video || video.IsFolder || video.IsPlaceHolder)
            return Task.FromResult<string?>(null);

        var path = video.Path;
        if (string.IsNullOrWhiteSpace(path) || path.Any(char.IsControl) || !Path.IsPathFullyQualified(path))
            return Task.FromResult<string?>(null);
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension) || extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>(null);
        var fullPath = Path.GetFullPath(path);
        if (!_fileSystem.FileExists(fullPath)) return Task.FromResult<string?>(null);
        token.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(fullPath);
    }
}
