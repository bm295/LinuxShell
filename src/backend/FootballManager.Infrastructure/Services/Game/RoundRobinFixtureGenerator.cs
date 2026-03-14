using FootballManager.Domain.Entities;

namespace FootballManager.Infrastructure.Services.Game;

internal static class RoundRobinFixtureGenerator
{
    public static void BuildSeasonFixtures(Season season, IReadOnlyList<Club> clubs)
    {
        if (clubs.Count < 2 || clubs.Count % 2 != 0)
        {
            throw new InvalidOperationException("Fixture generation requires an even number of clubs.");
        }

        var rotation = clubs.ToList();
        var roundsPerLeg = rotation.Count - 1;
        var matchesPerRound = rotation.Count / 2;
        var seasonStart = season.StartsAt;

        for (var roundIndex = 0; roundIndex < roundsPerLeg; roundIndex++)
        {
            for (var matchIndex = 0; matchIndex < matchesPerRound; matchIndex++)
            {
                var home = rotation[matchIndex];
                var away = rotation[rotation.Count - 1 - matchIndex];

                // Flip the first pairing every other round to balance home fixtures.
                if (matchIndex == 0 && roundIndex % 2 == 1)
                {
                    (home, away) = (away, home);
                }

                var firstLegRound = roundIndex + 1;
                var secondLegRound = firstLegRound + roundsPerLeg;

                season.ScheduleFixture(home, away, firstLegRound, seasonStart.AddDays((firstLegRound - 1) * 7));
                season.ScheduleFixture(away, home, secondLegRound, seasonStart.AddDays((secondLegRound - 1) * 7));
            }

            Rotate(rotation);
        }
    }

    private static void Rotate(List<Club> clubs)
    {
        var lastClub = clubs[^1];
        clubs.RemoveAt(clubs.Count - 1);
        clubs.Insert(1, lastClub);
    }
}
