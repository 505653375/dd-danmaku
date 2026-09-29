namespace Jellyfin.Plugin.DD.Danmaku;

using Jellyfin.Plugin.DD.Danmaku.Danmaku;
using Jellyfin.Plugin.DD.Danmaku.Services;
using System.Collections.Concurrent;

internal sealed class HostServices : IDisposable
{
    private readonly Plugin _plugin;
    private int _disposed;

    private readonly ConcurrentDictionary<string, DanmakuRecord> _records = new();

    internal DanmakuStorageService? Storage { get; set; }
    internal ISidecarStorageService? Sidecar { get; set; }
    internal IPluginConfigurationService Configuration { get; }

    internal HostServices(Plugin plugin)
    {
        _plugin = plugin;
        Configuration = new PluginConfigurationService(
            () => plugin.Configuration,
            cfg => plugin.UpdateConfiguration(cfg));
    }

    internal void Initialize(string dataDirectory)
    {
        var store = new JsonDanmakuRecordStore(dataDirectory);
        var files = new DanmakuFileService();
        var config = _plugin.Configuration;
        var paths = new MediaSidecarPathResolver(
            async (itemId, token) => await ResolvePlaybackFileAsync(itemId, token),
            () => config);

        Storage = new DanmakuStorageService(paths, files);
        Sidecar = new SidecarStorageService(
            (itemId, token) => paths.ResolveAsync(itemId, token), files);
    }

    private Task<string?> ResolvePlaybackFileAsync(string itemId, CancellationToken token)
    {
        return Task.FromResult<string?>(null);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
    }
}
