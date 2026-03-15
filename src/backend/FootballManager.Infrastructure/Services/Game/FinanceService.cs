using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class FinanceService(FootballManagerDbContext dbContext) : IFinanceService
{
    public async Task<FinanceSummaryDto?> GetSummaryAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.Players)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);

        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var entries = await dbContext.FinanceEntries
            .AsNoTracking()
            .Where(entry => entry.ClubId == selectedClub.Id)
            .OrderByDescending(entry => entry.OccurredAt)
            .ToListAsync(cancellationToken);

        var wageTotal = FinanceCalculator.CalculateWageBill(selectedClub.Players);
        var transferSpending = Sum(entries, FinanceEntryType.TransferExpense);
        var transferIncome = Sum(entries, FinanceEntryType.TransferIncome);
        var matchIncome = Sum(entries, FinanceEntryType.MatchIncome);
        var totalIncome = entries.Where(IsIncome).Sum(entry => entry.Amount);
        var totalExpenses = entries.Where(entry => !IsIncome(entry)).Sum(entry => entry.Amount);
        var recentIncomeEntry = entries.FirstOrDefault(IsIncome);
        var boardView = BuildBoardView(selectedClub.TransferBudget, wageTotal, transferIncome, transferSpending, totalIncome, totalExpenses);

        return new FinanceSummaryDto(
            selectedClub.Name,
            selectedClub.TransferBudget,
            wageTotal,
            transferSpending,
            transferIncome,
            matchIncome,
            recentIncomeEntry?.Amount ?? 0m,
            recentIncomeEntry is null ? "No income booked yet" : DescribeIncome(recentIncomeEntry.Type),
            totalIncome,
            totalExpenses,
            BuildTrendSummary(entries, selectedClub.TransferBudget, wageTotal, totalIncome, totalExpenses),
            boardView.Confidence,
            boardView.Note,
            entries
                .Take(8)
                .Select(MapEvent)
                .ToList());
    }

    private static decimal Sum(IEnumerable<FinanceEntry> entries, FinanceEntryType type) =>
        entries
            .Where(entry => entry.Type == type)
            .Sum(entry => entry.Amount);

    private static bool IsIncome(FinanceEntry entry) =>
        entry.Type is FinanceEntryType.MatchIncome or FinanceEntryType.TransferIncome;

    private static FinanceEventDto MapEvent(FinanceEntry entry) =>
        new(
            entry.Id,
            DescribeEventType(entry.Type),
            entry.Description,
            entry.Amount,
            entry.OccurredAt,
            IsIncome(entry));

    private static string DescribeEventType(FinanceEntryType type) =>
        type switch
        {
            FinanceEntryType.WageExpense => "Wages",
            FinanceEntryType.MatchIncome => "Match income",
            FinanceEntryType.TransferExpense => "Transfer spend",
            FinanceEntryType.TransferIncome => "Transfer income",
            _ => "Finance"
        };

    private static string DescribeIncome(FinanceEntryType type) =>
        type switch
        {
            FinanceEntryType.MatchIncome => "Latest gate receipts",
            FinanceEntryType.TransferIncome => "Latest sale income",
            _ => "Latest income"
        };

    private static (string Confidence, string Note) BuildBoardView(
        decimal currentBudget,
        decimal wageTotal,
        decimal transferIncome,
        decimal transferSpending,
        decimal totalIncome,
        decimal totalExpenses)
    {
        var wageCoverage = wageTotal <= 0 ? 99m : currentBudget / wageTotal;
        var netTransfer = transferIncome - transferSpending;

        if (currentBudget < 0 || wageCoverage < 1.5m)
        {
            return (
                "Pressure rising",
                "The board can feel the cash line tightening. Sales or a strong run of home dates need to arrive quickly.");
        }

        if (wageCoverage < 4m)
        {
            return (
                "Watching closely",
                "There is room to move, but every signing and every wage commitment now carries real heat.");
        }

        if (netTransfer > 0 && totalIncome >= totalExpenses)
        {
            return (
                "Backing the plan",
                "The board like the balance between outgoing deals, match income, and squad control.");
        }

        if (wageCoverage >= 8m)
        {
            return (
                "Calm boardroom",
                "The cash position can support ambition. You can chase upgrades without every move becoming a crisis.");
        }

        return (
            "Stable footing",
            "The finances are under control, but the board still expect discipline around wages and fees.");
    }

    private static string BuildTrendSummary(
        IReadOnlyList<FinanceEntry> entries,
        decimal currentBudget,
        decimal wageTotal,
        decimal totalIncome,
        decimal totalExpenses)
    {
        var latestEntry = entries.FirstOrDefault();

        if (latestEntry is null)
        {
            return "The books are still quiet. The next matchday or transfer decision will set the first financial tone.";
        }

        if (latestEntry.Type == FinanceEntryType.TransferExpense)
        {
            return "Money has just gone into the squad. The board now expect results to justify the fee.";
        }

        if (latestEntry.Type == FinanceEntryType.TransferIncome)
        {
            return "A sale has freshened the budget. The room is there for the next move if the squad can absorb the exit.";
        }

        if (latestEntry.Type == FinanceEntryType.MatchIncome)
        {
            return "Matchday money has landed. Gate receipts are helping keep the season moving without panic.";
        }

        if (currentBudget < wageTotal * 3m || totalExpenses > totalIncome)
        {
            return "The wage line is chewing through flexibility. Every round now carries real budget consequences.";
        }

        return "The wage bill is being covered, but the board still want sharper value from every pound on the books.";
    }
}
