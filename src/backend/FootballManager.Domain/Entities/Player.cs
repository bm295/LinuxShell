using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class Player
{
    private Player()
    {
    }

    internal Player(string firstName, string lastName, PlayerPosition position, int squadNumber, Club club)
    {
        Id = Guid.NewGuid();
        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));
        Position = position;
        SquadNumber = Guard.AgainstOutOfRange(squadNumber, 1, 99, nameof(squadNumber));
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

    public DateTime CreatedAt { get; private set; }

    public Guid ClubId { get; private set; }

    public Club? Club { get; private set; }
}
