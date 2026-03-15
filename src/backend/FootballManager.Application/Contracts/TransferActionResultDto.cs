namespace FootballManager.Application.Contracts;

public sealed record TransferActionResultDto(
    decimal AvailableBudget,
    TransferActivityDto Transfer,
    string Summary);
