using FootballManager.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<HealthStatusDto> Get()
    {
        return Ok(new HealthStatusDto("ok"));
    }
}
