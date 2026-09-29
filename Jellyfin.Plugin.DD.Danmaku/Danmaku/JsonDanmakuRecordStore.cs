namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

using System.Text.Json;

public sealed class JsonDanmakuRecordStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = null, WriteIndented = true };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonDanmakuRecordStore(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "records.json");
    }

    public async Task<IReadOnlyList<DanmakuRecord>> LoadAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return await ReadUnlockedAsync(cancellationToken); }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(IReadOnlyList<DanmakuRecord> records, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { await WriteUnlockedAsync(records, cancellationToken); }
        finally { _gate.Release(); }
    }

    public async Task MutateAsync(Action<List<DanmakuRecord>> mutation, CancellationToken token)
    {
        await _gate.WaitAsync(token);
        try
        {
            var records = await ReadUnlockedAsync(token);
            mutation(records);
            await WriteUnlockedAsync(records, token);
        }
        finally { _gate.Release(); }
    }

    private async Task<List<DanmakuRecord>> ReadUnlockedAsync(CancellationToken token)
    {
        try
        {
            await using var stream = File.OpenRead(_filePath);
            if (stream.Length > 16 * 1024 * 1024) throw Corrupt();
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() > 10000) throw Corrupt();
            var records = root.Deserialize<List<DanmakuRecord>>(_options);
            Validate(records);
            return records!;
        }
        catch (FileNotFoundException) { return []; }
        catch (JsonException ex) { throw new IOException("弹幕记录索引格式无效", ex); }
    }

    private static void Validate(IReadOnlyList<DanmakuRecord>? records)
    {
        if (records is null || records.Count > 10000 || records.Any(r => r is null
            || !(long.TryParse(r.ItemId, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var numericId) && numericId > 0
                || (Guid.TryParseExact(r.ItemId, "N", out var id)
                    || Guid.TryParseExact(r.ItemId, "D", out id)) && id != Guid.Empty)
            || r.CommentCount < 0 || r.CommentCount > DanmakuXml.MaxComments
            || r.FormatVersion != 1 || r.StorageLocation != "sidecar"
            || r.StoredAt == default || r.UpdatedAt == default)
            || records.Select(r => (r.ItemId, Source: GetEffectiveSource(r))).Distinct().Count() != records.Count)
            throw Corrupt();
    }

    internal static string? GetEffectiveSource(DanmakuRecord record)
        => record.Source == "upload" && record.RecordId == record.ItemId
            ? null : string.IsNullOrWhiteSpace(record.Source) ? null : record.Source.Trim();

    private static IOException Corrupt() => new("弹幕记录索引损坏或超出限制");

    private async Task WriteUnlockedAsync(IReadOnlyList<DanmakuRecord> records, CancellationToken token)
    {
        Validate(records);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(records, _options);
        if (bytes.Length > 16 * 1024 * 1024) throw Corrupt();
        var tempPath = _filePath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllBytesAsync(tempPath, bytes, token);
            token.ThrowIfCancellationRequested();
            File.Move(tempPath, _filePath, true);
        }
        finally
        {
            try { File.Delete(tempPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
