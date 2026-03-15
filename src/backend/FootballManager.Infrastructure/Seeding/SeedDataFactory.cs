using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Infrastructure.Seeding;

public static class SeedDataFactory
{
    private static readonly string[] ClubNames =
    [
        "Northbridge FC",
        "Ironvale Athletic",
        "Kingsport Rovers",
        "Redhaven United",
        "Cedar City FC",
        "Harbor Lights SC",
        "Oakridge Town",
        "Riverside Albion"
    ];

    private static readonly string[] FirstNames =
    [
        "Alex", "Marcus", "Theo", "Noah", "Luca",
        "Jonah", "Elias", "Mateo", "Finn", "Isaac",
        "Leo", "Owen", "Hugo", "Callum", "Mason",
        "Victor", "Nico", "Julian", "Rowan", "Kian"
    ];

    private static readonly string[] LastNames =
    [
        "Ward", "Bennett", "Silva", "Hayes", "Morgan",
        "Costa", "Brooks", "Foster", "Turner", "Santos",
        "Reed", "Bauer", "Mills", "Nolan", "Keller",
        "Price", "Dawson", "Fleming", "Pereira", "Ellis"
    ];

    public static League CreateInitialLeague()
    {
        var league = new League("Founders League", isTemplate: true);

        for (var clubIndex = 0; clubIndex < ClubNames.Length; clubIndex++)
        {
            var club = league.AddClub(ClubNames[clubIndex], 12_500_000m + (clubIndex * 450_000m));

            for (var playerIndex = 0; playerIndex < 20; playerIndex++)
            {
                var nameOffset = (clubIndex * 3 + playerIndex) % FirstNames.Length;
                var lastNameOffset = (clubIndex * 5 + playerIndex) % LastNames.Length;
                var position = ResolvePosition(playerIndex);
                var attributes = ResolveAttributes(position, clubIndex, playerIndex);

                club.AddPlayer(
                    FirstNames[nameOffset],
                    LastNames[lastNameOffset],
                    position,
                    playerIndex + 1,
                    attributes.Attack,
                    attributes.Defense,
                    attributes.Passing,
                    attributes.Fitness,
                    attributes.Morale);
            }
        }

        return league;
    }

    public static IReadOnlyCollection<Formation> CreateFormations() =>
    [
        new Formation("4-3-3", 4, 3, 3),
        new Formation("4-4-2", 4, 4, 2),
        new Formation("3-5-2", 3, 5, 2),
        new Formation("4-2-3-1", 4, 5, 1)
    ];

    public static (int Attack, int Defense, int Passing, int Fitness, int Morale) CreatePlayerAttributes(
        string clubName,
        PlayerPosition position,
        int squadNumber)
    {
        var clubIndex = Array.IndexOf(ClubNames, clubName);
        if (clubIndex < 0)
        {
            clubIndex = Math.Abs(clubName.GetHashCode(StringComparison.OrdinalIgnoreCase)) % ClubNames.Length;
        }

        return ResolveAttributes(position, clubIndex, squadNumber - 1);
    }

    private static PlayerPosition ResolvePosition(int playerIndex) =>
        playerIndex switch
        {
            < 2 => PlayerPosition.Goalkeeper,
            < 8 => PlayerPosition.Defender,
            < 14 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Forward
        };

    private static (int Attack, int Defense, int Passing, int Fitness, int Morale) ResolveAttributes(
        PlayerPosition position,
        int clubIndex,
        int playerIndex)
    {
        var technicalBase = 58 + ((clubIndex * 7 + playerIndex * 5) % 20);
        var defensiveBase = 56 + ((clubIndex * 11 + playerIndex * 4) % 22);
        var passingBase = 57 + ((clubIndex * 5 + playerIndex * 6) % 21);
        var fitness = Clamp(70 + ((clubIndex * 13 + playerIndex * 7) % 22));
        var morale = Clamp(64 + ((clubIndex * 9 + playerIndex * 5) % 24));

        return position switch
        {
            PlayerPosition.Goalkeeper => (
                Clamp(38 + (technicalBase / 5)),
                Clamp(defensiveBase + 14),
                Clamp(passingBase + 2),
                fitness,
                morale),
            PlayerPosition.Defender => (
                Clamp(46 + (technicalBase / 4)),
                Clamp(defensiveBase + 10),
                Clamp(passingBase),
                fitness,
                morale),
            PlayerPosition.Midfielder => (
                Clamp(54 + (technicalBase / 3)),
                Clamp(defensiveBase + 3),
                Clamp(passingBase + 10),
                fitness,
                morale),
            _ => (
                Clamp(60 + (technicalBase / 2)),
                Clamp(defensiveBase - 4),
                Clamp(passingBase + 5),
                fitness,
                morale)
        };
    }

    private static int Clamp(int value) => Math.Clamp(value, 35, 95);
}
