namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

public sealed record DanmakuRecord(
    string RecordId,
    string ItemId,
    string? AnimeId,
    string? EpisodeId,
    string? Source,
    int CommentCount,
    string? ContentHash,
    DateTimeOffset StoredAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastRequestedAt,
    DateTimeOffset? LastPlayedAt,
    bool RefreshOnNextPlayback,
    DateTimeOffset? RefreshAfter,
    string RefreshState,
    string StorageLocation,
    int FormatVersion);
