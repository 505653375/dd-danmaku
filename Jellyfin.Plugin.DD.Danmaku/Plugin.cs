namespace Jellyfin.Plugin.DD.Danmaku;

using Jellyfin.Plugin.DD.Danmaku.Configuration;
using Jellyfin.Plugin.DD.Danmaku.Services;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

public sealed partial class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public static readonly Guid PluginId = Guid.Parse("4b59d7ce-5a09-4f14-a5ca-0b9e9c8e53d7");
    public const string PluginName = "DD-Danmaku";

    public static Plugin? Instance { get; private set; }

    public override Guid Id => PluginId;
    public override string Name => PluginName;
    public override string Description => "Jellyfin 弹幕插件 - 从弹弹play获取弹幕并显示在播放器中";

    private readonly object _lifecycleGate = new();
    private HostServices? _host;
    private string _recordsDirectory = string.Empty;

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        var data = Path.Combine(applicationPaths.PluginConfigurationsPath, "Jellyfin.DD.Danmaku");
        _recordsDirectory = Path.Combine(data, "Data");
        Instance = this;
        _host = new HostServices(this);
        _host.Initialize(_recordsDirectory);
    }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "DD-Danmaku",
                DisplayName = "DD-Danmaku 弹幕管理",
                EmbeddedResourcePath = "Jellyfin.Plugin.DD.Danmaku.Configuration.configPage.html",
                EnableInMainMenu = true,
                MenuSection = "server"
            }
        };
    }
}

