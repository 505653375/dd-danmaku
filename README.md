# Jellyfin DD-Danmaku 弹幕插件

基于 [l429609201/dd-danmaku](https://github.com/l429609201/dd-danmaku) (Emby 弹幕插件) 移植到 Jellyfin。

## 功能

- 从弹弹play (DanDanPlay) 获取弹幕并在播放器中显示
- 支持本地弹幕文件管理 (XML 格式)
- 支持智能剧集匹配
- 支持自定义弹幕 API 服务器
- 支持 AI 匹配 (OpenAI 兼容接口)
- 支持弹幕过滤、合并、密度控制

## 安装

### 方法一：通过 Jellyfin 插件仓库安装

在 Jellyfin 控制台中添加自定义插件仓库：

1. 进入 Jellyfin 控制台 → 插件 → 仓库
2. 点击「添加仓库」
3. 输入仓库清单 URL：

```
https://github.com/505653375/dd-danmaku/raw/master/meta.json
```

4. 保存后，在插件目录的「可用插件」中找到 **DD-Danmaku**
5. 点击安装，重启 Jellyfin

### 方法二：手动安装

1. 下载 `Jellyfin.Plugin.DD.Danmaku.dll`
2. 将 DLL 放入 Jellyfin 插件目录：
   - **Windows**: `%LocalAppData%\jellyfin\plugins\DD-Danmaku\`
   - **Linux**: `/var/lib/jellyfin/plugins/DD-Danmaku/`
   - **Docker**: 映射到 `/config/plugins/DD-Danmaku/`
3. 重启 Jellyfin
4. 在控制台 → 插件中确认插件已加载
5. 在菜单中访问 "DD-Danmaku 弹幕管理" 配置插件

### 方法三：从源码编译

```bash
cd Jellyfin.Plugin.DD.Danmaku
dotnet build -c Release
```

构建产物在 `bin/Release/net8.0/Jellyfin.Plugin.DD.Danmaku.dll`

## 前端注入

插件通过 Jellyfin 的 `IHasWebPages` 提供配置页面。前端弹幕脚本 (`ede.js`) 可以通过以下方式注入：

### 方法一：使用 CustomCssJS 插件
安装 Jellyfin 的 CustomCssJS 插件，将 `ede.js` 添加到自定义脚本。

### 方法二：手动修改 index.html
1. 找到 Jellyfin Web 的 `index.html` (通常位于 Jellyfin 安装目录的 `jellyfin-web` 下)
2. 在 `</body>` 标签前添加：
   ```html
   <script src="/dd-danmaku/api/resource/ede.js" charset="utf-8"></script>
   ```
3. 重启 Jellyfin

### 方法三：用户脚本
使用 Tampermonkey/Greasemonkey 安装 `ede.js`。

## 架构

```
jellyfin-dd-danmaku/
├── meta.json                          # 插件仓库清单
├── Jellyfin.Plugin.DD.Danmaku/        # 插件项目
│   ├── Plugin.cs                       # 插件入口
│   ├── Configuration/                  # 配置
│   │   ├── PluginConfiguration.cs      # 插件配置类
│   │   └── configPage.html             # 配置页面
│   ├── Api/                            # API 控制器
│   │   ├── DanmakuController.cs        # 弹幕 API
│   │   ├── ConfigController.cs         # 配置 API
│   │   └── ApiContracts.cs             # API 数据契约
│   ├── Danmaku/                        # 弹幕核心服务
│   │   ├── DanmakuComment.cs           # 弹幕数据模型
│   │   ├── DanmakuXml.cs               # XML 读写
│   │   ├── DanmakuFileService.cs       # 文件服务
│   │   ├── DanmakuRecordService.cs     # 记录服务
│   │   ├── SidecarStorageService.cs    # 旁车存储
│   │   └── MediaSidecarPathResolver.cs
│   ├── Services/                       # 业务服务
│   │   ├── ApiFacade.cs                # API 外观层
│   │   ├── PlaybackService.cs          # 播放服务
│   │   ├── ResourceService.cs          # 资源服务
│   │   ├── CapabilitiesService.cs      # 能力声明服务
│   │   ├── PluginConfigurationService.cs
│   │   ├── StatisticsService.cs
│   │   ├── PlaybackFileResolver.cs
│   │   └── ServiceRegistrator.cs       # DI 注册
│   ├── Hosting/
│   │   └── HostServices.cs             # 宿主服务
│   ├── Injection/
│   │   └── BootstrapInjector.cs        # 脚本注入
│   └── Resources/
│       └── ede.js                      # 前端弹幕脚本
├── UI/                                 # Vue.js 管理面板源码
├── Worker/tools/                       # 前端依赖库
└── DataCenter/web/                     # 数据中心 Web UI
```

## 与原版 (Emby) 的主要差异

| 方面 | Emby 原版 | Jellyfin 移植版 |
|------|----------|----------------|
| 目标框架 | .NET 8.0 + Emby SDK 4.8 | .NET 8.0 + Jellyfin.Controller 10.10 |
| HTTP API | ServiceStack `[Route]` | ASP.NET Core MVC `[ApiController]` |
| 服务注册 | Emby 自动发现 | `IServiceCollection` DI |
| 脚本注入 | 拦截 shortcuts.js | IHasWebPages + 前端手动注入 |
| 配置保存 | Emby 自动 XML | `BasePlugin<T>` UpdateConfiguration |
| 前端脚本 | ede.js (Emby 兼容) | ede.js (Jellyfin 兼容层) |

## API 端点

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/dd-danmaku/api/capabilities` | 查询可用能力 |
| GET | `/dd-danmaku/api/playback/{itemId}` | 查询弹幕 |
| POST | `/dd-danmaku/api/playback/{itemId}/result` | 上报播放结果 |
| GET | `/dd-danmaku/api/config` | 读取配置 (管理员) |
| PUT | `/dd-danmaku/api/config` | 更新配置 (管理员) |
| GET | `/dd-danmaku/api/resource/ede.js` | 获取前端脚本 |
| GET | `/dd-danmaku/api/danmu/{itemId}` | 读取弹幕 (兼容接口) |
| PUT | `/dd-danmaku/api/danmu/{itemId}` | 保存弹幕 (兼容接口, 管理员) |
| DELETE | `/dd-danmaku/api/danmu/{itemId}` | 删除弹幕 (兼容接口, 管理员) |

## 已知限制

- [ ] 文件持久化 (旁车 XML 保存) 需要 Jellyfin 的文件系统权限配置
- [ ] WebSocket 实时同步尚未实现
- [ ] AI 匹配功能尚未完全移植
- [ ] 批量管理功能尚未完全移植
- [ ] 前端 `ede.js` 的 Jellyfin 兼容性层需要进一步测试和优化
- [ ] `Emby.importModule` 回退使用动态 script 加载替代

## 致谢

- 原作者: [misaka10876](https://github.com/l429609201)
- Fork 维护: [chen3861229](https://github.com/chen3861229)
- 参考项目: [pipi20xx/dd-danmaku](https://github.com/pipi20xx/dd-danmaku)

## License

MIT
