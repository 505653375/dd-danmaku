namespace Jellyfin.Plugin.DD.Danmaku.Services;

using Jellyfin.Plugin.DD.Danmaku.Api;
using Jellyfin.Plugin.DD.Danmaku.Danmaku;

public interface IPlaybackService
{
    Task<PlaybackQueryDto> QueryAsync(string itemId, CancellationToken cancellationToken);
    Task RecordResultAsync(string itemId, PlaybackResultDto result, CancellationToken cancellationToken);
}

public sealed class PlaybackService : IPlaybackService
{
    private readonly IDanmakuRecordService _records;
    private readonly ISidecarStorageService _storage;

    public PlaybackService(IDanmakuRecordService records, ISidecarStorageService storage)
    {
        _records = records;
        _storage = storage;
    }

    public async Task<PlaybackQueryDto> QueryAsync(string itemId, CancellationToken cancellationToken)
    {
        var record = await _records.GetByItemIdAsync(itemId, cancellationToken);
        if (record is null)
        {
            return new PlaybackQueryDto(itemId, false, [], 0, null, null, null, false, null, null, null);
        }

        var comments = await _storage.ReadAsync(record, cancellationToken);
        var refresh = record.RefreshOnNextPlayback ||
            (record.RefreshAfter is not null && record.RefreshAfter <= DateTimeOffset.UtcNow);
        var reason = refresh ? "记录已标记刷新或已过期" : null;
        var match = string.IsNullOrWhiteSpace(record.AnimeId) && string.IsNullOrWhiteSpace(record.EpisodeId)
            ? null
            : new MatchSummaryDto(record.AnimeId, record.EpisodeId, null, 1m, "stored");

        return new PlaybackQueryDto(
            itemId, true, comments.Select(ToDto).ToArray(), comments.Count,
            record.StoredAt, record.UpdatedAt, record.Source, refresh, reason,
            record.StorageLocation, match);
    }

    public Task RecordResultAsync(string itemId, PlaybackResultDto result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static DanmakuCommentDto ToDto(DanmakuComment comment) =>
        new(comment.Text, comment.Time, comment.Mode, comment.Color, comment.UserId);
}
