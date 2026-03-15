using FootballManager.Domain.Common;

namespace FootballManager.Domain.Entities;

public sealed class Transfer
{
    private Transfer()
    {
    }

    public Transfer(Player player, Club fromClub, Club toClub, decimal fee)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        FromClub = fromClub ?? throw new ArgumentNullException(nameof(fromClub));
        ToClub = toClub ?? throw new ArgumentNullException(nameof(toClub));

        if (fromClub.Id == toClub.Id)
        {
            throw new InvalidOperationException("A transfer must move the player between different clubs.");
        }

        Id = Guid.NewGuid();
        PlayerId = player.Id;
        FromClubId = fromClub.Id;
        ToClubId = toClub.Id;
        Fee = Guard.AgainstNegative(fee, nameof(fee));
        CompletedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid PlayerId { get; private set; }

    public Player? Player { get; private set; }

    public Guid FromClubId { get; private set; }

    public Club? FromClub { get; private set; }

    public Guid ToClubId { get; private set; }

    public Club? ToClub { get; private set; }

    public decimal Fee { get; private set; }

    public DateTime CompletedAt { get; private set; }
}
