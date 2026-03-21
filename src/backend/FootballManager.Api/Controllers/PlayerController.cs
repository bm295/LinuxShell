using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/player")]
public sealed class PlayerController(ISquadManagementService squadManagementService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDetailDto>> GetPlayer(
        Guid id,
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var player = await squadManagementService.GetPlayerAsync(gameId, id, cancellationToken);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpPut("{id:guid}/position")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDetailDto>> UpdatePosition(
        Guid id,
        [FromQuery] Guid gameId,
        [FromBody] UpdatePlayerPositionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Position))
        {
            return BadRequest(new { message = "position is required." });
        }

        try
        {
            var player = await squadManagementService.UpdatePlayerPositionAsync(gameId, id, request, cancellationToken);
            return player is null ? NotFound() : Ok(player);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
