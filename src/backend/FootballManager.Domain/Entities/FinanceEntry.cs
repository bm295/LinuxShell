using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class FinanceEntry
{
    private FinanceEntry()
    {
    }

    public FinanceEntry(
        Club club,
        FinanceEntryType type,
        decimal amount,
        string description,
        DateTime? occurredAt = null)
    {
        Id = Guid.NewGuid();
        Club = club ?? throw new ArgumentNullException(nameof(club));
        ClubId = club.Id;
        Type = type;
        Amount = Guard.AgainstNegative(amount, nameof(amount));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description));
        OccurredAt = occurredAt ?? DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ClubId { get; private set; }

    public Club? Club { get; private set; }

    public FinanceEntryType Type { get; private set; }

    public decimal Amount { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public DateTime OccurredAt { get; private set; }
}
