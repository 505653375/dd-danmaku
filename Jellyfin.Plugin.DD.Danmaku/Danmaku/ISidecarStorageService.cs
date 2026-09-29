namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

public interface ISidecarStorageService
{
    Task<string?> ResolveAuthorizedPathAsync(string itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DanmakuComment>> ReadAsync(DanmakuRecord record, CancellationToken cancellationToken);
    Task WriteAtomicallyAsync(DanmakuRecord record, IReadOnlyList<DanmakuComment> comments, CancellationToken cancellationToken);
    Task DeleteAsync(DanmakuRecord record, CancellationToken cancellationToken);
}
