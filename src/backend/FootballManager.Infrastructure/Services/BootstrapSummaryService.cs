using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services;

public sealed class BootstrapSummaryService(FootballManagerDbContext dbContext) : IBootstrapSummaryService
{
    public async Task<BootstrapSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var leagueCount = await dbContext.Leagues.CountAsync(cancellationToken);
        var clubCount = await dbContext.Clubs.CountAsync(cancellationToken);
        var playerCount = await dbContext.Players.CountAsync(cancellationToken);

        return new BootstrapSummaryDto(leagueCount, clubCount, playerCount);
    }
}
