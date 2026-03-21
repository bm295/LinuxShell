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
        "Arsenal",
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

    private static readonly string[] AcademyTrainingFocuses =
    [
        "Finishing",
        "Ball retention",
        "Defensive timing",
        "Tempo control",
        "Composure"
    ];

    public static League CreateInitialLeague()
    {
        var league = new League("Founders League", isTemplate: true);

        for (var clubIndex = 0; clubIndex < ClubNames.Length; clubIndex++)
        {
            var club = league.AddClub(ClubNames[clubIndex], 12_500_000m + (clubIndex * 450_000m));

            for (var playerIndex = 0; playerIndex < 20; playerIndex++)
            {
                var playerName = ResolveSeniorPlayerName(ClubNames[clubIndex], clubIndex, playerIndex);
                var position = ResolvePosition(playerIndex);
                var attributes = ResolveAttributes(position, clubIndex, playerIndex);

                var seniorPlayer = club.AddPlayer(
                    playerName.FirstName,
                    playerName.LastName,
                    position,
                    ResolvePlayerAge(ClubNames[clubIndex], position, clubIndex, playerIndex),
                    playerIndex + 1,
                    attributes.Attack,
                    attributes.Defense,
                    attributes.Passing,
                    attributes.Fitness,
                    attributes.Morale);

                if (ClubNames[clubIndex] == "Arsenal" && playerIndex == 3)
                {
                    club.SetCaptain(seniorPlayer);
                }
            }

            club.EnsureCaptain();

            foreach (var academyPlayer in CreateAcademyProfiles(club.Name))
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
        var clubIndex = ResolveClubIndex(clubName);
        return ResolveAttributes(position, clubIndex, squadNumber - 1);
    }

    public static int CreatePlayerAge(
        string clubName,
        PlayerPosition position,
        int squadNumber)
    {
        var clubIndex = ResolveClubIndex(clubName);
        return ResolvePlayerAge(clubName, position, clubIndex, squadNumber - 1);
    }

    public static IReadOnlyCollection<AcademySeed> CreateAcademyProfiles(string clubName)
    {
        var clubIndex = ResolveClubIndex(clubName);
        var academyPlayers = new List<AcademySeed>();

        for (var academyIndex = 0; academyIndex < 5; academyIndex++)
        {
            var academyPlayerName = ResolveAcademyPlayerName(clubName, clubIndex, academyIndex);
            var position = ResolveAcademyPosition(academyIndex);
            var focus = ResolveTrainingFocus(position, academyIndex);
            var attributes = ResolveAcademyAttributes(position, clubIndex, academyIndex);

            academyPlayers.Add(new AcademySeed(
                academyPlayerName.FirstName,
                academyPlayerName.LastName,
                position,
                attributes.Age,
                attributes.Attack,
                attributes.Defense,
                attributes.Passing,
                attributes.Fitness,
                attributes.Morale,
                attributes.Potential,
                attributes.DevelopmentProgress,
                focus));
        }

        return academyPlayers;
    }

    private static PlayerPosition ResolvePosition(int playerIndex) =>
        playerIndex switch
        {
            < 2 => PlayerPosition.Goalkeeper,
            < 8 => PlayerPosition.Defender,
            < 14 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Forward
        };

    private static PlayerPosition ResolveAcademyPosition(int academyIndex) =>
        academyIndex switch
        {
            0 => PlayerPosition.Forward,
            1 => PlayerPosition.Midfielder,
            2 => PlayerPosition.Defender,
            3 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Goalkeeper
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

    private static (int Age, int Attack, int Defense, int Passing, int Fitness, int Morale, int Potential, int DevelopmentProgress) ResolveAcademyAttributes(
        PlayerPosition position,
        int clubIndex,
        int academyIndex)
    {
        var age = 15 + ((clubIndex + academyIndex) % 4);
        var potential = Clamp(74 + ((clubIndex * 9 + academyIndex * 7) % 18));
        var developmentProgress = Math.Clamp(38 + ((clubIndex * 11 + academyIndex * 13) % 34), 25, 82);
        var technicalBase = 46 + ((clubIndex * 5 + academyIndex * 6) % 16);
        var defensiveBase = 44 + ((clubIndex * 7 + academyIndex * 5) % 16);
        var passingBase = 45 + ((clubIndex * 4 + academyIndex * 7) % 16);
        var fitness = Clamp(66 + ((clubIndex * 3 + academyIndex * 7) % 16));
        var morale = Clamp(63 + ((clubIndex * 6 + academyIndex * 5) % 18));

        return position switch
        {
            PlayerPosition.Goalkeeper => (
                age,
                Clamp(35 + (technicalBase / 5)),
                Clamp(defensiveBase + 12),
                Clamp(passingBase),
                fitness,
                morale,
                potential,
                developmentProgress),
            PlayerPosition.Defender => (
                age,
                Clamp(42 + (technicalBase / 4)),
                Clamp(defensiveBase + 9),
                Clamp(passingBase),
                fitness,
                morale,
                potential,
                developmentProgress),
            PlayerPosition.Midfielder => (
                age,
                Clamp(49 + (technicalBase / 3)),
                Clamp(defensiveBase + 2),
                Clamp(passingBase + 8),
                fitness,
                morale,
                potential,
                developmentProgress),
            _ => (
                age,
                Clamp(55 + (technicalBase / 2)),
                Clamp(defensiveBase - 2),
                Clamp(passingBase + 4),
                fitness,
                morale,
                potential,
                developmentProgress)
        };
    }

    private static int ResolvePlayerAge(string clubName, PlayerPosition position, int clubIndex, int playerIndex)
    {
        if (clubName == "Arsenal" && playerIndex == 3)
        {
            return 17;
        }

        var baseAge = position switch
        {
            PlayerPosition.Goalkeeper => 23,
            PlayerPosition.Defender => 21,
            PlayerPosition.Midfielder => 20,
            _ => 19
        };

        var ageOffset = position switch
        {
            PlayerPosition.Goalkeeper => (clubIndex * 2 + playerIndex) % 9,
            PlayerPosition.Defender => (clubIndex * 3 + playerIndex) % 10,
            PlayerPosition.Midfielder => (clubIndex * 4 + playerIndex) % 9,
            _ => (clubIndex * 5 + playerIndex) % 8
        };

        return Math.Clamp(baseAge + ageOffset, 17, 35);
    }

    private static string ResolveTrainingFocus(PlayerPosition position, int academyIndex) =>
        position switch
        {
            PlayerPosition.Forward => AcademyTrainingFocuses[0],
            PlayerPosition.Defender => AcademyTrainingFocuses[2],
            PlayerPosition.Goalkeeper => AcademyTrainingFocuses[4],
            PlayerPosition.Midfielder when academyIndex % 2 == 0 => AcademyTrainingFocuses[1],
            _ => AcademyTrainingFocuses[3]
        };

    private static int ResolveClubIndex(string clubName)
    {
        var clubIndex = Array.IndexOf(ClubNames, clubName);
        if (clubIndex >= 0)
        {
            return clubIndex;
        }

        return Math.Abs(clubName.GetHashCode(StringComparison.OrdinalIgnoreCase)) % ClubNames.Length;
    }

    private static (string FirstName, string LastName) ResolveSeniorPlayerName(string clubName, int clubIndex, int playerIndex)
    {
        if (clubName == "Arsenal" && playerIndex == 3)
        {
            return ("Cesc", "Fàbregas");
        }

        var nameOffset = (clubIndex * 3 + playerIndex) % FirstNames.Length;
        var lastNameOffset = (clubIndex * 5 + playerIndex) % LastNames.Length;
        return (FirstNames[nameOffset], LastNames[lastNameOffset]);
    }

    private static (string FirstName, string LastName) ResolveAcademyPlayerName(string clubName, int clubIndex, int academyIndex)
    {
        if (clubName == "Arsenal" && academyIndex == 0)
        {
            return ("Thierry", "Henry");
        }

        if (clubName == "Arsenal" && academyIndex == 3)
        {
            return ("Patrick", "Vieira");
        }

        if (clubName == "Arsenal" && academyIndex == 4)
        {
            return ("David", "Seaman");
        }

        var nameOffset = (clubIndex * 7 + academyIndex * 2 + 1) % FirstNames.Length;
        var lastNameOffset = (clubIndex * 4 + academyIndex * 3 + 2) % LastNames.Length;
        return (FirstNames[nameOffset], LastNames[lastNameOffset]);
    }

    private static int Clamp(int value) => Math.Clamp(value, 35, 95);

    public sealed record AcademySeed(
        string FirstName,
        string LastName,
        PlayerPosition Position,
        int Age,
        int Attack,
        int Defense,
        int Passing,
        int Fitness,
        int Morale,
        int Potential,
        int DevelopmentProgress,
        string TrainingFocus);
}
