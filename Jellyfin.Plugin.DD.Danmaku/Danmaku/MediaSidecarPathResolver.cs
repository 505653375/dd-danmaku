namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

using Jellyfin.Plugin.DD.Danmaku.Configuration;

public sealed class MediaSidecarPathResolver
{
    private readonly Func<string, CancellationToken, Task<string?>> _resolvePlaybackFile;
    private readonly Func<PluginConfiguration> _configuration;

    public MediaSidecarPathResolver(Func<string, CancellationToken, Task<string?>> resolvePlaybackFile,
        Func<PluginConfiguration> configuration)
        => (_resolvePlaybackFile, _configuration) = (resolvePlaybackFile, configuration);

    public async Task<string?> ResolveAsync(string itemId, CancellationToken token, string? source = null)
    {
        if (!_configuration().FilePersistenceEnabled) return null;
        if (string.IsNullOrWhiteSpace(itemId) || itemId.Length > 256 || itemId.Any(char.IsControl))
            throw new ArgumentException("媒体标识无效", nameof(itemId));
        var mediaPath = await _resolvePlaybackFile(itemId, token);
        if (string.IsNullOrWhiteSpace(mediaPath) || !Path.IsPathFullyQualified(mediaPath)
            || !File.Exists(mediaPath))
            return null;
        var fullPath = Path.GetFullPath(mediaPath);
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0) return null;
        for (var directory = Directory.GetParent(fullPath); directory is not null; directory = directory.Parent)
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0) return null;
        var target = Path.ChangeExtension(fullPath, ".xml");
        if (string.IsNullOrWhiteSpace(source)) return target;
        var safeSource = source.Trim();
        if (safeSource.Length is 0 or > 64 || safeSource.Any(c => char.IsControl(c)
            || "<>:\"/\\|?*".Contains(c) || Path.GetInvalidFileNameChars().Contains(c)))
            throw new ArgumentException("弹幕来源标识无效", nameof(source));
        return Path.Combine(Path.GetDirectoryName(target)!,
            Path.GetFileNameWithoutExtension(target) + "_" + safeSource + ".xml");
    }
}
