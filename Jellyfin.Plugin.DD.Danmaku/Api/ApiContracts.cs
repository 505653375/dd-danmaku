namespace Jellyfin.Plugin.DD.Danmaku.Api;

public sealed record ApiResponse<T>(
    bool Success,
    string? Message,
    string? ErrorCode,
    T? Data,
    string? TraceId);

public sealed record CapabilitiesDto(
    string PluginId,
    string Version,
    int ApiVersion,
    string Mode,
    IReadOnlyDictionary<string, bool> Capabilities,
    bool Enabled,
    bool FilePersistenceEnabled,
    bool RefreshOnNextPlayback);

public sealed record PlaybackQueryDto(
    string ItemId,
    bool Found,
    IReadOnlyList<DanmakuCommentDto> Comments,
    int CommentCount,
    DateTimeOffset? StoredAt,
    DateTimeOffset? UpdatedAt,
    string? Source,
    bool RefreshRequired,
    string? RefreshReason,
    string? StorageLocation,
    MatchSummaryDto? Match);

public sealed record DanmakuCommentDto(string Text, double Time, int Mode, int Color, string? UserId);
public sealed record MatchSummaryDto(string? AnimeId, string? EpisodeId, string? Title, decimal Confidence, string Mode);

public sealed record PlaybackResultDto(
    bool Success,
    int CommentCount,
    long DurationMilliseconds,
    string? ErrorCode,
    string? SessionSummary);

public sealed record PluginConfigDto(
    bool AutoInjectionEnabled,
    bool EdeResourceEnabled,
    string ResourceVersion,
    string LogLevel,
    bool AutoRefreshOnNextPlayback,
    bool FilePersistenceEnabled = false,
    bool? AiEnabled = null,
    bool? AiUserAccessEnabled = null,
    string[]? AiAllowedUserIds = null,
    string? MatchStrategy = null,
    bool? AllowMatchFallback = null);
