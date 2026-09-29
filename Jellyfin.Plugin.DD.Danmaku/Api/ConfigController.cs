namespace Jellyfin.Plugin.DD.Danmaku.Api;

using Jellyfin.Plugin.DD.Danmaku.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("dd-danmaku")]
public class ConfigController : ControllerBase
{
    private readonly ApiFacade _facade;

    public ConfigController(ApiFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("api/config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<PluginConfigDto>> GetConfig()
    {
        return Ok(new ApiResponse<PluginConfigDto>(true, null, null, _facade.GetConfig(), null));
    }

    [HttpPut("api/config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<PluginConfigDto>> UpdateConfig([FromBody] PluginConfigDto config)
    {
        try
        {
            var result = _facade.UpdateConfig(config);
            return Ok(new ApiResponse<PluginConfigDto>(true, null, null, result, null));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<PluginConfigDto>(false, ex.Message, "CONFIG_UPDATE_ERROR", null, null));
        }
    }
}
