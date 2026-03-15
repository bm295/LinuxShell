namespace FootballManager.Application.Contracts;

public sealed record FinanceSummaryDto(
    string ClubName,
    decimal CurrentBudget,
    decimal WageTotal,
    decimal TransferSpending,
    decimal TransferIncome,
    decimal MatchIncome,
    decimal RecentIncome,
    string RecentIncomeLabel,
    decimal TotalIncome,
    decimal TotalExpenses,
    string TrendSummary,
    string BoardConfidence,
    string BoardConfidenceNote,
    IReadOnlyCollection<FinanceEventDto> RecentEvents);
