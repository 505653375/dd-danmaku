namespace Jellyfin.Plugin.DD.Danmaku.Services;

public interface IStatisticsService
{
    Task<IReadOnlyDictionary<string, object>> GetSummaryAsync(CancellationToken cancellationToken);
}

public sealed class StatisticsService : IStatisticsService
{
    public Task<IReadOnlyDictionary<string, object>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, object> result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["injectionAvailable"] = true,
            ["resourceVersion"] = "1.0.0",
            ["startedAt"] = DateTimeOffset.UtcNow,
        };
        return Task.FromResult(result);
    }
}
