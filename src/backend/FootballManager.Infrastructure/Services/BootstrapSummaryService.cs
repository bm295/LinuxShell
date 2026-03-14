using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services;

public sealed class BootstrapSummaryService(FootballManagerDbContext dbContext) : IBootstrapSummaryService
{
    public async Task<BootstrapSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var leagueCount = await dbContext.Leagues.CountAsync(league => league.IsTemplate, cancellationToken);
        var clubCount = await dbContext.Clubs.CountAsync(club => club.League != null && club.League.IsTemplate, cancellationToken);
        var playerCount = await dbContext.Players.CountAsync(
            player => player.Club != null && player.Club.League != null && player.Club.League.IsTemplate,
            cancellationToken);

        return new BootstrapSummaryDto(leagueCount, clubCount, playerCount);
    }
}
