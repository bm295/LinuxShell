using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/lineup")]
public sealed class LineupController(ISquadManagementService squadManagementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LineupEditorDto>> GetLineup(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var lineup = await squadManagementService.GetLineupAsync(gameId, cancellationToken);
        return lineup is null ? NotFound() : Ok(lineup);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LineupDto>> UpdateLineup(
        [FromQuery] Guid gameId,
        [FromBody] UpdateLineupRequestDto request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        if (request.FormationId == Guid.Empty)
        {
            return BadRequest(new { message = "formationId is required." });
        }

        try
        {
            var lineup = await squadManagementService.UpdateLineupAsync(gameId, request, cancellationToken);
            return lineup is null ? NotFound() : Ok(lineup);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
