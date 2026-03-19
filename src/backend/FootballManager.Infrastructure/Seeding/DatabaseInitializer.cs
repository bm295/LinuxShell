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

        await BackfillClubNamesAsync(cancellationToken);
        await BackfillArsenalSquadIdentityAsync(cancellationToken);
        await BackfillClubCaptainsAsync(cancellationToken);
        await BackfillPlayerAttributesAsync(cancellationToken);
        await BackfillAcademyPlayersAsync(cancellationToken);
        await BackfillArsenalAcademyIdentitiesAsync(cancellationToken);
        await BackfillGameSaveMetadataAsync(cancellationToken);
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
                             player.Morale == 0 ||
                             player.Age == 0)
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
            var age = SeedDataFactory.CreatePlayerAge(
                player.Club?.Name ?? string.Empty,
                player.Position,
                player.SquadNumber);

            dbContext.Entry(player).Property(current => current.Attack).CurrentValue = attributes.Attack;
            dbContext.Entry(player).Property(current => current.Defense).CurrentValue = attributes.Defense;
            dbContext.Entry(player).Property(current => current.Passing).CurrentValue = attributes.Passing;
            dbContext.Entry(player).Property(current => current.Fitness).CurrentValue = attributes.Fitness;
            dbContext.Entry(player).Property(current => current.Morale).CurrentValue = attributes.Morale;
            dbContext.Entry(player).Property(current => current.Age).CurrentValue = age;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillClubNamesAsync(CancellationToken cancellationToken)
    {
        var leagues = await dbContext.Leagues
            .Include(league => league.Clubs)
            .ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var league in leagues)
        {
            var cedarCity = league.Clubs.SingleOrDefault(club => club.Name == "Cedar City FC");
            if (cedarCity is null)
            {
                continue;
            }

            var arsenal = league.Clubs.SingleOrDefault(club => club.Name == "Arsenal");
            var northbridge = league.Clubs.SingleOrDefault(club => club.Name == "Northbridge FC");

            if (arsenal is not null && northbridge is null)
            {
                arsenal.Rename("Northbridge FC");
                cedarCity.Rename("Arsenal");
                hasChanges = true;
                continue;
            }

            if (arsenal is null)
            {
                cedarCity.Rename("Arsenal");
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task BackfillGameSaveMetadataAsync(CancellationToken cancellationToken)
    {
        var saves = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
            .Include(save => save.Season)
            .Where(save => save.SaveName == string.Empty || save.LastSavedAt == default)
            .ToListAsync(cancellationToken);

        if (saves.Count == 0)
        {
            return;
        }

        foreach (var save in saves)
        {
            save.EnsureMetadata();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillArsenalSquadIdentityAsync(CancellationToken cancellationToken)
    {
        var arsenalNumberFours = await dbContext.Players
            .Include(player => player.Club)
            .Where(player => player.Club != null &&
                             player.Club.Name == "Arsenal" &&
                             player.SquadNumber == 4 &&
                             (player.FirstName != "Cesc" || player.LastName != "Fàbregas"))
            .ToListAsync(cancellationToken);

        if (arsenalNumberFours.Count == 0)
        {
            return;
        }

        foreach (var player in arsenalNumberFours)
        {
            player.Rename("Cesc", "Fàbregas");
            player.SetAge(17);
            player.AssignCaptaincy();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillClubCaptainsAsync(CancellationToken cancellationToken)
    {
        var clubs = await dbContext.Clubs
            .Include(club => club.Players)
            .ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var club in clubs)
        {
            if (club.Players.Count == 0)
            {
                continue;
            }

            var captainCount = club.Players.Count(player => player.IsCaptain);
            if (club.Name == "Arsenal")
            {
                var arsenalCaptain = club.Players.SingleOrDefault(player => player.SquadNumber == 4);
                if (arsenalCaptain is not null &&
                    (!arsenalCaptain.IsCaptain || captainCount != 1))
                {
                    club.SetCaptain(arsenalCaptain);
                    hasChanges = true;
                    continue;
                }

                if (arsenalCaptain is not null)
                {
                    continue;
                }
            }

            if (captainCount == 1)
            {
                continue;
            }

            club.EnsureCaptain();
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task BackfillAcademyPlayersAsync(CancellationToken cancellationToken)
    {
        var clubs = await dbContext.Clubs
            .Include(club => club.AcademyPlayers)
            .ToListAsync(cancellationToken);

        if (clubs.Count == 0)
        {
            return;
        }

        var hasChanges = false;

        foreach (var club in clubs.Where(club => club.AcademyPlayers.Count == 0))
        {
            foreach (var academyPlayer in SeedDataFactory.CreateAcademyProfiles(club.Name))
            {
                club.AddAcademyPlayer(
                    academyPlayer.FirstName,
                    academyPlayer.LastName,
                    academyPlayer.Position,
                    academyPlayer.Age,
                    academyPlayer.Attack,
                    academyPlayer.Defense,
                    academyPlayer.Passing,
                    academyPlayer.Fitness,
                    academyPlayer.Morale,
                    academyPlayer.Potential,
                    academyPlayer.DevelopmentProgress,
                    academyPlayer.TrainingFocus);
            }

            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task BackfillArsenalAcademyIdentitiesAsync(CancellationToken cancellationToken)
    {
        var arsenalAcademyPlayers = await dbContext.AcademyPlayers
            .Include(player => player.Club)
            .Where(player => player.Club != null &&
                             player.Club.Name == "Arsenal" &&
                             ((player.FirstName == "Julian" && player.LastName == "Reed") ||
                              (player.FirstName == "Isaac" && player.LastName == "Pereira")))
            .ToListAsync(cancellationToken);
        var arsenalSeniorPlayers = await dbContext.Players
            .Include(player => player.Club)
            .Where(player => player.Club != null &&
                             player.Club.Name == "Arsenal" &&
                             player.FirstName == "Isaac" &&
                             player.LastName == "Pereira")
            .ToListAsync(cancellationToken);

        if (arsenalAcademyPlayers.Count == 0 && arsenalSeniorPlayers.Count == 0)
        {
            return;
        }

        foreach (var player in arsenalAcademyPlayers)
        {
            if (player.FirstName == "Julian" && player.LastName == "Reed")
            {
                player.Rename("David", "Seaman");
                continue;
            }

            player.Rename("Thierry", "Henry");
        }

        foreach (var player in arsenalSeniorPlayers)
        {
            player.Rename("Thierry", "Henry");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
