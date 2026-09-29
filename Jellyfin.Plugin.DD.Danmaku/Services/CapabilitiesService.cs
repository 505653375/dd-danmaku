namespace Jellyfin.Plugin.DD.Danmaku.Services;

using Jellyfin.Plugin.DD.Danmaku.Api;
using Jellyfin.Plugin.DD.Danmaku.Configuration;

public interface ICapabilitiesService
{
    CapabilitiesDto Create(PluginConfiguration configuration);
}

public sealed class CapabilitiesService : ICapabilitiesService
{
    public CapabilitiesDto Create(PluginConfiguration configuration)
    {
        var capabilities = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["LocalDanmaku"] = true,
            ["FileSidecarPersistence"] = configuration.FilePersistenceEnabled,
            ["BatchManagement"] = true,
            ["Statistics"] = true,
            ["RefreshPolicy"] = true,
            ["MediaMatch"] = true,
            ["AiMatching"] = configuration.AiEnabled,
            ["ParameterPersistence"] = true,
            ["Injection"] = configuration.AutoInjectionEnabled
        };
        return new CapabilitiesDto(
            "dd-danmaku",
            typeof(CapabilitiesService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            1, "dll", capabilities, true,
            capabilities["FileSidecarPersistence"],
            configuration.AutoRefreshOnNextPlayback);
    }
}
