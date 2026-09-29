namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

public interface IDanmakuRecordService
{
    Task<DanmakuRecord?> GetByItemIdAsync(string itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DanmakuRecord>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task SetRefreshOnNextPlaybackAsync(string recordId, bool value, CancellationToken cancellationToken);
    Task DeleteAsync(string recordId, CancellationToken cancellationToken);
}
