using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class Player
{
    private Player()
    {
    }

    internal Player(
        string firstName,
        string lastName,
        PlayerPosition position,
        int squadNumber,
        int attack,
        int defense,
        int passing,
        int fitness,
        int morale,
        Club club)
    {
        Id = Guid.NewGuid();
        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));
        Position = position;
        SquadNumber = Guard.AgainstOutOfRange(squadNumber, 1, 99, nameof(squadNumber));
        Attack = Guard.AgainstOutOfRange(attack, 1, 100, nameof(attack));
        Defense = Guard.AgainstOutOfRange(defense, 1, 100, nameof(defense));
        Passing = Guard.AgainstOutOfRange(passing, 1, 100, nameof(passing));
        Fitness = Guard.AgainstOutOfRange(fitness, 1, 100, nameof(fitness));
        Morale = Guard.AgainstOutOfRange(morale, 1, 100, nameof(morale));
        Club = club ?? throw new ArgumentNullException(nameof(club));
        ClubId = club.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public PlayerPosition Position { get; private set; }

    public int SquadNumber { get; private set; }

    public int Attack { get; private set; }

    public int Defense { get; private set; }

    public int Passing { get; private set; }

    public int Fitness { get; private set; }

    public int Morale { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid ClubId { get; private set; }

    public Club? Club { get; private set; }

    public int GetOverallRating()
    {
        var weightedScore = Position switch
        {
            PlayerPosition.Goalkeeper => (Attack * 0.05) + (Defense * 0.75) + (Passing * 0.20),
            PlayerPosition.Defender => (Attack * 0.15) + (Defense * 0.60) + (Passing * 0.25),
            PlayerPosition.Midfielder => (Attack * 0.30) + (Defense * 0.25) + (Passing * 0.45),
            PlayerPosition.Forward => (Attack * 0.60) + (Defense * 0.15) + (Passing * 0.25),
            _ => (Attack + Defense + Passing) / 3d
        };

        return (int)Math.Round(weightedScore, MidpointRounding.AwayFromZero);
    }

    public int GetReadinessScore()
    {
        var readinessScore = (Fitness * 0.6) + (Morale * 0.4);
        return (int)Math.Round(readinessScore, MidpointRounding.AwayFromZero);
    }
}
