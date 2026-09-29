namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

public sealed class DanmakuRecordService : IDanmakuRecordService
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<DanmakuRecord>>> _loader;
    private readonly Func<IReadOnlyList<DanmakuRecord>, CancellationToken, Task> _saver;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DanmakuRecordService(
        Func<CancellationToken, Task<IReadOnlyList<DanmakuRecord>>> loader,
        Func<IReadOnlyList<DanmakuRecord>, CancellationToken, Task> saver)
    {
        _loader = loader;
        _saver = saver;
    }

    public async Task<DanmakuRecord?> GetByItemIdAsync(string itemId, CancellationToken cancellationToken)
    {
        var records = await _loader(cancellationToken);
        return records.FirstOrDefault(x => x.ItemId == itemId);
    }

    public async Task<IReadOnlyList<DanmakuRecord>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var records = await _loader(cancellationToken);
        return records.OrderByDescending(x => x.UpdatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToArray();
    }

    public async Task SetRefreshOnNextPlaybackAsync(string recordId, bool value, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var records = (await _loader(cancellationToken)).ToList();
            var index = records.FindIndex(x => x.RecordId == recordId);
            if (index < 0) return;
            records[index] = records[index] with { RefreshOnNextPlayback = value, UpdatedAt = DateTimeOffset.UtcNow };
            await _saver(records, cancellationToken);
        }
        finally { _gate.Release(); }
    }

    public async Task DeleteAsync(string recordId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var records = (await _loader(cancellationToken)).Where(x => x.RecordId != recordId).ToArray();
            await _saver(records, cancellationToken);
        }
        finally { _gate.Release(); }
    }
}
