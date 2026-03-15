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

    public ICollection<AcademyPlayer> AcademyPlayers { get; private set; } = [];

    public void Rename(string name)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
    }

    public void AdjustTransferBudget(decimal amount, bool allowNegative = false)
    {
        var nextBudget = TransferBudget + amount;
        if (!allowNegative && nextBudget < 0)
        {
            throw new InvalidOperationException($"Club '{Name}' does not have enough budget for that move.");
        }

        TransferBudget = decimal.Round(nextBudget, 2, MidpointRounding.AwayFromZero);
    }

    public int GetNextAvailableSquadNumber()
    {
        for (var squadNumber = 1; squadNumber <= 99; squadNumber++)
        {
            if (Players.All(player => player.SquadNumber != squadNumber))
            {
                return squadNumber;
            }
        }

        throw new InvalidOperationException($"Club '{Name}' does not have a free squad number available.");
    }

    public void EnsureCaptain()
    {
        if (Players.Count == 0)
        {
            return;
        }

        var currentCaptains = Players
            .Where(player => player.IsCaptain)
            .ToList();

        if (currentCaptains.Count == 1)
        {
            return;
        }

        var captain = SelectCaptainCandidate();
        SetCaptain(captain);
    }

    public void SetCaptain(Player player)
    {
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (!Players.Contains(player))
        {
            throw new InvalidOperationException($"Player '{player.FullName}' does not belong to club '{Name}'.");
        }

        foreach (var squadPlayer in Players)
        {
            if (squadPlayer.Id == player.Id)
            {
                squadPlayer.AssignCaptaincy();
                continue;
            }

            squadPlayer.RemoveCaptaincy();
        }
    }

    public Player AddPlayer(
        string firstName,
        string lastName,
        PlayerPosition position,
        int age,
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
            age,
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

    public AcademyPlayer AddAcademyPlayer(
        string firstName,
        string lastName,
        PlayerPosition position,
        int age,
        int attack,
        int defense,
        int passing,
        int fitness,
        int morale,
        int potential,
        int developmentProgress,
        string trainingFocus)
    {
        var academyPlayer = new AcademyPlayer(
            firstName,
            lastName,
            position,
            age,
            attack,
            defense,
            passing,
            fitness,
            morale,
            potential,
            developmentProgress,
            trainingFocus,
            this);

        AcademyPlayers.Add(academyPlayer);
        return academyPlayer;
    }

    private Player SelectCaptainCandidate()
    {
        return Players
            .OrderByDescending(player => player.GetOverallRating())
            .ThenByDescending(player => player.Morale)
            .ThenByDescending(player => player.Age)
            .ThenBy(player => player.SquadNumber)
            .First();
    }
}
