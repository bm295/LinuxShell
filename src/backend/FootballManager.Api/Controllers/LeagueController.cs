using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/league")]
public sealed class LeagueController(ILeagueOverviewService leagueOverviewService) : ControllerBase
{
    [HttpGet("table")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<LeagueTableEntryDto>>> GetTable(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var table = await leagueOverviewService.GetLeagueTableAsync(gameId, cancellationToken);
        return table is null ? NotFound() : Ok(table);
    }
}
