using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/game")]
public sealed class GameController(IGameSetupService gameSetupService) : ControllerBase
{
    [HttpGet("clubs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ClubOptionDto>>> GetAvailableClubs(CancellationToken cancellationToken)
    {
        var clubs = await gameSetupService.GetAvailableClubsAsync(cancellationToken);
        return Ok(clubs);
    }

    [HttpPost("new")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateNewGameResponseDto>> CreateNewGame(
        [FromBody] CreateNewGameRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await gameSetupService.CreateNewGameAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
