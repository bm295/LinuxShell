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
        var league = new League("Founders League");

        for (var clubIndex = 0; clubIndex < ClubNames.Length; clubIndex++)
        {
            var club = league.AddClub(ClubNames[clubIndex], 12_500_000m + (clubIndex * 450_000m));

            for (var playerIndex = 0; playerIndex < 20; playerIndex++)
            {
                var nameOffset = (clubIndex * 3 + playerIndex) % FirstNames.Length;
                var lastNameOffset = (clubIndex * 5 + playerIndex) % LastNames.Length;

                club.AddPlayer(
                    FirstNames[nameOffset],
                    LastNames[lastNameOffset],
                    ResolvePosition(playerIndex),
                    playerIndex + 1);
            }
        }

        return league;
    }

    private static PlayerPosition ResolvePosition(int playerIndex) =>
        playerIndex switch
        {
            < 2 => PlayerPosition.Goalkeeper,
            < 8 => PlayerPosition.Defender,
            < 14 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Forward
        };
}
