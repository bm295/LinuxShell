using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/academy")]
public sealed class AcademyController(IAcademyService academyService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademySummaryDto>> GetAcademy(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var academy = await academyService.GetAcademyAsync(gameId, cancellationToken);
        return academy is null ? NotFound() : Ok(academy);
    }

    [HttpPost("promote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademyPromotionResultDto>> Promote(
        [FromQuery] Guid gameId,
        [FromBody] PromoteAcademyPlayerRequestDto request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        try
        {
            var result = await academyService.PromotePlayerAsync(gameId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
