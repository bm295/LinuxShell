using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/club")]
public sealed class ClubController(IClubDashboardService clubDashboardService) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClubDashboardDto>> GetDashboard(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var dashboard = await clubDashboardService.GetDashboardAsync(gameId, cancellationToken);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }
}
