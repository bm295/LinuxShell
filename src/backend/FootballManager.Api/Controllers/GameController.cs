using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/game")]
public sealed class GameController(IGameSetupService gameSetupService, IGameSaveService gameSaveService) : ControllerBase
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

    [HttpPost("save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameSaveSummaryDto>> SaveGame(
        [FromQuery] Guid gameId,
        [FromBody] SaveGameRequestDto? request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var save = await gameSaveService.SaveAsync(gameId, request ?? new SaveGameRequestDto(null), cancellationToken);
        return save is null ? NotFound() : Ok(save);
    }

    [HttpGet("load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoadGameResponseDto>> LoadGame(
        [FromQuery] Guid? gameId,
        CancellationToken cancellationToken)
    {
        var response = await gameSaveService.LoadAsync(gameId, cancellationToken);

        if (gameId.HasValue && response.SelectedSave is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpDelete("save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameSaveSummaryDto>> DeleteSave(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var deletedSave = await gameSaveService.DeleteAsync(gameId, cancellationToken);
        return deletedSave is null ? NotFound() : Ok(deletedSave);
    }
}
