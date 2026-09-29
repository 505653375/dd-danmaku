namespace Jellyfin.Plugin.DD.Danmaku.Services;

using Jellyfin.Plugin.DD.Danmaku.Api;
using Jellyfin.Plugin.DD.Danmaku.Configuration;

public sealed class ApiFacade
{
    private readonly ICapabilitiesService _capabilities;
    private readonly IPluginConfigurationService _configuration;
    private readonly IPlaybackService _playback;
    private readonly IStatisticsService _statistics;

    public ApiFacade(
        ICapabilitiesService capabilities,
        IPluginConfigurationService configuration,
        IPlaybackService playback,
        IStatisticsService statistics)
    {
        _capabilities = capabilities;
        _configuration = configuration;
        _playback = playback;
        _statistics = statistics;
    }

    public PluginConfigDto GetConfig() => _configuration.Get();
    public PluginConfigDto UpdateConfig(PluginConfigDto requested) => _configuration.Update(requested);

    public ApiResponse<CapabilitiesDto> GetCapabilities()
    {
        return new ApiResponse<CapabilitiesDto>(
            true, null, null,
            _capabilities.Create(_configuration.Get().ToPluginConfiguration()), null);
    }

    public async Task<ApiResponse<PlaybackQueryDto>> GetPlaybackAsync(
        string itemId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return new ApiResponse<PlaybackQueryDto>(false, "缺少 ItemId", "INVALID_ITEM_ID", null, null);

        var data = await _playback.QueryAsync(itemId, cancellationToken);
        return new ApiResponse<PlaybackQueryDto>(true, null, null, data, null);
    }

    public async Task<ApiResponse<object>> PostPlaybackResultAsync(
        string itemId, PlaybackResultDto result, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return new ApiResponse<object>(false, "缺少 ItemId", "INVALID_ITEM_ID", null, null);

        await _playback.RecordResultAsync(itemId, result, cancellationToken);
        return new ApiResponse<object>(true, "播放结果已接收", null, null, null);
    }

    public async Task<ApiResponse<IReadOnlyDictionary<string, object>>> GetStatisticsAsync(
        CancellationToken cancellationToken)
    {
        var data = await _statistics.GetSummaryAsync(cancellationToken);
        return new ApiResponse<IReadOnlyDictionary<string, object>>(true, null, null, data, null);
    }
}

public static class PluginConfigDtoExtensions
{
    public static PluginConfiguration ToPluginConfiguration(this Api.PluginConfigDto dto)
    {
        return new PluginConfiguration
        {
            AutoInjectionEnabled = dto.AutoInjectionEnabled,
            EdeResourceEnabled = dto.EdeResourceEnabled,
            ResourceVersion = dto.ResourceVersion,
            LogLevel = dto.LogLevel,
            AutoRefreshOnNextPlayback = dto.AutoRefreshOnNextPlayback,
            FilePersistenceEnabled = dto.FilePersistenceEnabled,
            AiEnabled = dto.AiEnabled.HasValue && dto.AiEnabled.Value,
            AiUserAccessEnabled = dto.AiUserAccessEnabled.HasValue && dto.AiUserAccessEnabled.Value,
            AiAllowedUserIds = dto.AiAllowedUserIds != null ? dto.AiAllowedUserIds : Array.Empty<string>(),
            MatchStrategy = dto.MatchStrategy ?? "traditional-first",
            AllowMatchFallback = dto.AllowMatchFallback.HasValue && dto.AllowMatchFallback.Value
        };
    }
}
