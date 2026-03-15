using FootballManager.Application.Contracts;
using FootballManager.Domain.Entities;

namespace FootballManager.Infrastructure.Services.Game;

internal static class LeagueTableCalculator
{
    public static IReadOnlyList<LeagueTableEntryDto> BuildTable(
        IReadOnlyCollection<Club> clubs,
        IReadOnlyCollection<Fixture> fixtures)
    {
        var rows = clubs.ToDictionary(
            club => club.Id,
            club => new TableRow(club.Id, club.Name));

        foreach (var fixture in fixtures.Where(fixture => fixture.IsPlayed && fixture.HomeGoals.HasValue && fixture.AwayGoals.HasValue))
        {
            var homeRow = rows[fixture.HomeClubId];
            var awayRow = rows[fixture.AwayClubId];
            var homeGoals = fixture.HomeGoals!.Value;
            var awayGoals = fixture.AwayGoals!.Value;

            homeRow.Played++;
            awayRow.Played++;
            homeRow.GoalsFor += homeGoals;
            homeRow.GoalsAgainst += awayGoals;
            awayRow.GoalsFor += awayGoals;
            awayRow.GoalsAgainst += homeGoals;

            if (homeGoals > awayGoals)
            {
                homeRow.Wins++;
                awayRow.Losses++;
                homeRow.Points += 3;
            }
            else if (homeGoals < awayGoals)
            {
                awayRow.Wins++;
                homeRow.Losses++;
                awayRow.Points += 3;
            }
            else
            {
                homeRow.Draws++;
                awayRow.Draws++;
                homeRow.Points++;
                awayRow.Points++;
            }
        }

        return rows.Values
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.ClubName)
            .Select((row, index) => row.ToDto(index + 1))
            .ToList();
    }

    private sealed class TableRow(Guid clubId, string clubName)
    {
        public Guid ClubId { get; } = clubId;

        public string ClubName { get; } = clubName;

        public int Played { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int GoalDifference => GoalsFor - GoalsAgainst;

        public int Points { get; set; }

        public LeagueTableEntryDto ToDto(int position) =>
            new(
                ClubId,
                ClubName,
                position,
                Played,
                Wins,
                Draws,
                Losses,
                GoalsFor,
                GoalsAgainst,
                GoalDifference,
                Points);
    }
}
