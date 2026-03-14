using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/bootstrap")]
public sealed class BootstrapController(IBootstrapSummaryService bootstrapSummaryService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BootstrapSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await bootstrapSummaryService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
