namespace FootballManager.Application.Contracts;

public sealed record FinanceEventDto(
    Guid Id,
    string Type,
    string Description,
    decimal Amount,
    DateTime OccurredAt,
    bool IsIncome);
