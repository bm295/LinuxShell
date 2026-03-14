using FootballManager.Infrastructure.Persistence;
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
        if (await dbContext.Leagues.AnyAsync(league => league.IsTemplate, cancellationToken))
        {
            return;
        }

        var league = SeedDataFactory.CreateInitialLeague();
        dbContext.Leagues.Add(league);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsTransient(Exception exception) =>
        exception is NpgsqlException or TimeoutException ||
        exception.InnerException is NpgsqlException or TimeoutException;
}
