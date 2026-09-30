namespace Jellyfin.Plugin.DD.Danmaku.Api;

using Jellyfin.Plugin.DD.Danmaku.Danmaku;
using Jellyfin.Plugin.DD.Danmaku.Services;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("dd-danmaku")]
public class DanmakuController : ControllerBase
{
    private readonly ApiFacade _facade;
    private readonly IResourceService _resourceService;
    private readonly ILibraryManager _libraryManager;

    public DanmakuController(
        ApiFacade facade,
        IResourceService resourceService,
        ILibraryManager libraryManager)
    {
        _facade = facade;
        _resourceService = resourceService;
        _libraryManager = libraryManager;
    }

    [HttpGet("api/capabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<CapabilitiesDto>> GetCapabilities()
    {
        return Ok(_facade.GetCapabilities());
    }

    [HttpGet("api/playback/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PlaybackQueryDto>>> GetPlayback(
        [FromRoute] string itemId, CancellationToken cancellationToken)
    {
        return Ok(await _facade.GetPlaybackAsync(itemId, cancellationToken));
    }

    [HttpPost("api/playback/{itemId}/result")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> PostPlaybackResult(
        [FromRoute] string itemId, [FromBody] PlaybackResultDto result,
        CancellationToken cancellationToken)
    {
        return Ok(await _facade.PostPlaybackResultAsync(itemId, result, cancellationToken));
    }

    [HttpGet("api/statistics")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyDictionary<string, object>>>> GetStatistics(
        CancellationToken cancellationToken)
    {
        return Ok(await _facade.GetStatisticsAsync(cancellationToken));
    }

    [HttpGet("api/resource/ede.js")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEdeScript(CancellationToken cancellationToken)
    {
        var stream = await _resourceService.OpenAsync("Resources/ede.js", cancellationToken);
        if (stream is null)
            return NotFound();
        var contentType = _resourceService.GetContentType("ede.js");
        return File(stream, contentType);
    }

    [HttpGet("admin/index.html")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdminIndex(CancellationToken cancellationToken)
    {
        var stream = await _resourceService.OpenAsync("Resources/Admin/index.html", cancellationToken);
        if (stream is null)
            return NotFound();
        return File(stream, "text/html; charset=utf-8");
    }

    [HttpGet("admin/{*path}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdminResource(string path, CancellationToken cancellationToken)
    {
        var resourcePath = "Resources/Admin/" + path;
        var stream = await _resourceService.OpenAsync(resourcePath, cancellationToken);
        if (stream is null)
            return NotFound();
        var contentType = _resourceService.GetContentType(path);
        return File(stream, contentType);
    }

    [HttpGet("jellyfin-bridge.js")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBridgeScript(CancellationToken cancellationToken)
    {
        var stream = await _resourceService.OpenAsync("Resources/Admin/jellyfin-bridge.js", cancellationToken);
        if (stream is null)
            return NotFound();
        return File(stream, "application/javascript; charset=utf-8");
    }

    [HttpGet("api/danmu/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetDanmaku(
        [FromRoute] string itemId, [FromQuery] string? option = "DownloadXml",
        CancellationToken cancellationToken = default)
    {
        var result = await _facade.GetPlaybackAsync(itemId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/danmu/{itemId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SaveDanmaku(
        [FromRoute] string itemId, [FromQuery] bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        var item = _libraryManager.GetItemById(itemId);
        if (item is null)
            return NotFound(new ApiResponse<object>(false, "媒体不存在", "ITEM_NOT_FOUND", null, null));

        return Ok(new ApiResponse<object>(true, "弹幕保存接口已接收", null, null, null));
    }

    [HttpDelete("api/danmu/{itemId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult DeleteDanmaku([FromRoute] string itemId)
    {
        return Ok(new ApiResponse<object>(true, "弹幕删除接口已接收", null, null, null));
    }

    [HttpGet("api/records")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetRecords([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        return Ok(new ApiResponse<object>(true, null, null, new { page, pageSize, total = 0, items = Array.Empty<object>() }, null));
    }

    [HttpGet("api/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetStatus()
    {
        return Ok(new ApiResponse<object>(true, null, null,
            new { status = "running", version = "1.0.0", pluginId = "dd-danmaku" }, null));
    }
}
