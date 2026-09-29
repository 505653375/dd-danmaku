namespace Jellyfin.Plugin.DD.Danmaku.Danmaku;

public sealed record DanmakuComment(string Text, double Time, int Mode, int Color, string? UserId);
