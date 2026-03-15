using FootballManager.Infrastructure.Persistence;
using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FootballManager.Infrastructure.Seeding;

public sealed class DatabaseInitializer(FootballManagerDbContext dbContext, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                await SeedAsync(cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < maxAttempts && IsTransient(exception))
            {
                logger.LogWarning(
                    exception,
                    "Database initialization attempt {Attempt} of {MaxAttempts} failed. Retrying in 3 seconds.",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        var hasChanges = false;

        if (!await dbContext.Formations.AnyAsync(cancellationToken))
        {
            dbContext.Formations.AddRange(SeedDataFactory.CreateFormations());
            hasChanges = true;
        }

        if (!await dbContext.Leagues.AnyAsync(league => league.IsTemplate, cancellationToken))
        {
            dbContext.Leagues.Add(SeedDataFactory.CreateInitialLeague());
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await BackfillPlayerAttributesAsync(cancellationToken);
    }

    private static bool IsTransient(Exception exception) =>
        exception is NpgsqlException or TimeoutException ||
        exception.InnerException is NpgsqlException or TimeoutException;

    private async Task BackfillPlayerAttributesAsync(CancellationToken cancellationToken)
    {
        var playersNeedingAttributes = await dbContext.Players
            .Include(player => player.Club)
            .Where(player => player.Attack == 0 ||
                             player.Defense == 0 ||
                             player.Passing == 0 ||
                             player.Fitness == 0 ||
                             player.Morale == 0)
            .ToListAsync(cancellationToken);

        if (playersNeedingAttributes.Count == 0)
        {
            return;
        }

        foreach (var player in playersNeedingAttributes)
        {
            var attributes = SeedDataFactory.CreatePlayerAttributes(
                player.Club?.Name ?? string.Empty,
                player.Position,
                player.SquadNumber);

            dbContext.Entry(player).Property(current => current.Attack).CurrentValue = attributes.Attack;
            dbContext.Entry(player).Property(current => current.Defense).CurrentValue = attributes.Defense;
            dbContext.Entry(player).Property(current => current.Passing).CurrentValue = attributes.Passing;
            dbContext.Entry(player).Property(current => current.Fitness).CurrentValue = attributes.Fitness;
            dbContext.Entry(player).Property(current => current.Morale).CurrentValue = attributes.Morale;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
