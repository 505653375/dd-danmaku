namespace Jellyfin.Plugin.DD.Danmaku.Configuration;

using MediaBrowser.Model.Plugins;

public sealed class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        AutoInjectionEnabled = true;
        EdeResourceEnabled = true;
        ResourceVersion = "1";
        LogLevel = "Information";
        FilePersistenceEnabled = false;
        FilePersistenceName = ".xml";
        MatchStrategy = "traditional-first";
        AllowMatchFallback = true;
        AiProviderOrder = ["local", "remote"];
        AiTimeoutSeconds = 15;
        AiConfidenceThreshold = 0.85m;
        AiMaxCandidates = 200;
        AiCacheEnabled = true;
        RefreshAfterHours = 168;
        KeepOldDanmakuOnRefreshFail = true;
        PreferLocalDanmaku = true;
    }

    public bool ScanDeepEnabled { get; set; }
    public bool AutoInjectionEnabled { get; set; }
    public bool EdeResourceEnabled { get; set; }
    public string ResourceVersion { get; set; } = "1";
    public bool InjectionLoggingEnabled { get; set; }
    public string LogLevel { get; set; } = "Information";
    public bool FilePersistenceEnabled { get; set; }
    public bool FilePersistenceReadEnabled { get; set; } = true;
    public bool FilePersistenceWriteEnabled { get; set; }
    public string FilePersistenceName { get; set; } = ".xml";
    public bool FilePersistenceFallback { get; set; }
    public bool AutoRefreshOnNextPlayback { get; set; }
    public int RefreshAfterHours { get; set; } = 168;
    public bool KeepOldDanmakuOnRefreshFail { get; set; } = true;
    public bool AiEnabled { get; set; }
    public bool AiUserAccessEnabled { get; set; }
    public string[] AiAllowedUserIds { get; set; } = [];
    public string? AiBaseUrl { get; set; }
    public string? AiModel { get; set; }
    public string? AiApiKey { get; set; }
    public string[] AiProviderOrder { get; set; } = ["local", "remote"];
    public string? LocalAiBaseUrl { get; set; }
    public string? LocalAiModel { get; set; }
    public string? RemoteAiBaseUrl { get; set; }
    public string? RemoteAiModel { get; set; }
    public string? RemoteAiApiKey { get; set; }
    public int AiTimeoutSeconds { get; set; } = 15;
    public decimal AiConfidenceThreshold { get; set; } = 0.85m;
    public int AiMaxCandidates { get; set; } = 200;
    public bool AiAllowRemote { get; set; }
    public bool AiCacheEnabled { get; set; } = true;
    public string MatchStrategy { get; set; } = "traditional-first";
    public bool AllowMatchFallback { get; set; } = true;
    public string[] ScanLibraryIds { get; set; } = [];
    public bool PreferLocalDanmaku { get; set; } = true;
    public bool AutoSaveDanmaku { get; set; }
}
