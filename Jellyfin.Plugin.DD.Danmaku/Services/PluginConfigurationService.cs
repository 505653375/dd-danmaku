namespace Jellyfin.Plugin.DD.Danmaku.Services;

using Jellyfin.Plugin.DD.Danmaku.Api;
using Jellyfin.Plugin.DD.Danmaku.Configuration;

public interface IPluginConfigurationService
{
    PluginConfigDto Get();
    PluginConfigDto Update(PluginConfigDto requested);
}

public sealed class PluginConfigurationService : IPluginConfigurationService
{
    private readonly Func<PluginConfiguration> _getConfiguration;
    private readonly Action<PluginConfiguration>? _saveConfiguration;
    internal static readonly object ConfigurationGate = new();

    public PluginConfigurationService(PluginConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _getConfiguration = () => configuration;
    }

    public PluginConfigurationService(Func<PluginConfiguration> getConfiguration,
        Action<PluginConfiguration> saveConfiguration)
    {
        _getConfiguration = getConfiguration ?? throw new ArgumentNullException(nameof(getConfiguration));
        _saveConfiguration = saveConfiguration ?? throw new ArgumentNullException(nameof(saveConfiguration));
    }

    public PluginConfigDto Get()
    {
        lock (ConfigurationGate) return ToDto(_getConfiguration());
    }

    public PluginConfigDto Update(PluginConfigDto requested)
    {
        ArgumentNullException.ThrowIfNull(requested);
        lock (ConfigurationGate)
        {
            var current = _getConfiguration();
            current.AutoInjectionEnabled = requested.AutoInjectionEnabled;
            current.EdeResourceEnabled = requested.EdeResourceEnabled;
            current.ResourceVersion = string.IsNullOrWhiteSpace(requested.ResourceVersion)
                ? current.ResourceVersion : requested.ResourceVersion.Trim();
            current.LogLevel = requested.LogLevel is "Warning" or "Error" ? requested.LogLevel : "Information";
            current.AutoRefreshOnNextPlayback = requested.AutoRefreshOnNextPlayback;
            current.FilePersistenceEnabled = requested.FilePersistenceEnabled;
            current.MatchStrategy = requested.MatchStrategy is "traditional-first" or "ai-first"
                ? requested.MatchStrategy : current.MatchStrategy;
            current.AllowMatchFallback = requested.AllowMatchFallback ?? current.AllowMatchFallback;
            _saveConfiguration?.Invoke(current);
            return ToDto(_getConfiguration());
        }
    }

    private static PluginConfigDto ToDto(PluginConfiguration configuration) => new(
        configuration.AutoInjectionEnabled, configuration.EdeResourceEnabled,
        configuration.ResourceVersion, configuration.LogLevel,
        configuration.AutoRefreshOnNextPlayback, configuration.FilePersistenceEnabled,
        configuration.AiEnabled, configuration.AiUserAccessEnabled,
        (configuration.AiAllowedUserIds ?? []).ToArray(),
        configuration.MatchStrategy, configuration.AllowMatchFallback);
}
