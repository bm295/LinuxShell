using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[Route("api/transfer")]
public sealed class TransferController(ITransferMarketService transferMarketService) : ControllerBase
{
    [HttpGet("market")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferMarketDto>> GetMarket(
        [FromQuery] Guid gameId,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        var market = await transferMarketService.GetMarketAsync(gameId, cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    [HttpPost("buy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferActionResultDto>> Buy(
        [FromQuery] Guid gameId,
        [FromBody] BuyTransferRequestDto request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        try
        {
            var result = await transferMarketService.BuyPlayerAsync(gameId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("sell")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferActionResultDto>> Sell(
        [FromQuery] Guid gameId,
        [FromBody] SellTransferRequestDto request,
        CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            return BadRequest(new { message = "gameId is required." });
        }

        try
        {
            var result = await transferMarketService.SellPlayerAsync(gameId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
