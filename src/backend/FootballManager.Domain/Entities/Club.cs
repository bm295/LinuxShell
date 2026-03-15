using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class Club
{
    private Club()
    {
    }

    internal Club(string name, decimal transferBudget, League league)
    {
        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        TransferBudget = Guard.AgainstNegative(transferBudget, nameof(transferBudget));
        League = league ?? throw new ArgumentNullException(nameof(league));
        LeagueId = league.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal TransferBudget { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid LeagueId { get; private set; }

    public League? League { get; private set; }

    public ICollection<Player> Players { get; private set; } = [];

    public Player AddPlayer(
        string firstName,
        string lastName,
        PlayerPosition position,
        int squadNumber,
        int attack,
        int defense,
        int passing,
        int fitness,
        int morale)
    {
        if (Players.Any(player => player.SquadNumber == squadNumber))
        {
            throw new InvalidOperationException($"Squad number '{squadNumber}' is already assigned at club '{Name}'.");
        }

        var player = new Player(
            firstName,
            lastName,
            position,
            squadNumber,
            attack,
            defense,
            passing,
            fitness,
            morale,
            this);
        Players.Add(player);
        return player;
    }
}
