namespace FootballManager.Application.Contracts;

public sealed record TransferActivityDto(
    Guid TransferId,
    string PlayerName,
    string FromClub,
    string ToClub,
    decimal Fee,
    DateTime CompletedAt,
    bool IsIncoming);
