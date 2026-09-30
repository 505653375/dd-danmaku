namespace Jellyfin.Plugin.DD.Danmaku.Services;

using Jellyfin.Plugin.DD.Danmaku.Configuration;
using Jellyfin.Plugin.DD.Danmaku.Danmaku;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.DependencyInjection;

public class ServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IResourceService>(sp =>
        {
            var plugin = Plugin.Instance;
            return plugin is not null
                ? new ResourceService(plugin.DataFolderPath)
                : new ResourceService();
        });

        serviceCollection.AddSingleton<IPlaybackService>(sp =>
        {
            var libraryManager = sp.GetRequiredService<ILibraryManager>();
            var fileSystem = sp.GetRequiredService<IFileSystem>();
            var plugin = Plugin.Instance;
            var config = plugin?.Configuration ?? new PluginConfiguration();
            var playbackFiles = new PlaybackFileResolver(libraryManager, fileSystem);
            var paths = new MediaSidecarPathResolver(playbackFiles.ResolveAsync, () => plugin?.Configuration ?? new PluginConfiguration());
            var files = new DanmakuFileService();
            var sidecar = new SidecarStorageService((itemId, token) => paths.ResolveAsync(itemId, token), files);
            var dataDir = Path.Combine(plugin?.DataFolderPath ?? string.Empty, "Data");
            var store = new JsonDanmakuRecordStore(dataDir);
            var records = new DanmakuRecordService(
                ct => store.LoadAsync(ct),
                (r, ct) => store.SaveAsync(r, ct));
            return new PlaybackService(records, sidecar);
        });

        serviceCollection.AddSingleton<ICapabilitiesService, CapabilitiesService>();
        serviceCollection.AddSingleton<IStatisticsService, StatisticsService>();

        serviceCollection.AddSingleton<IPluginConfigurationService>(sp =>
        {
            var plugin = Plugin.Instance;
            if (plugin is null)
                return new PluginConfigurationService(new PluginConfiguration());
            return new PluginConfigurationService(
                () => plugin.Configuration,
                cfg => plugin.UpdateConfiguration(cfg));
        });

        serviceCollection.AddSingleton<ApiFacade>();
        serviceCollection.AddControllersWithViews()
            .AddApplicationPart(typeof(ServiceRegistrator).Assembly);
    }
}
