using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class TransferMarketService(FootballManagerDbContext dbContext) : ITransferMarketService
{
    public async Task<TransferMarketDto?> GetMarketAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub?.League is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var leagueClubs = selectedClub.League.Clubs.OrderBy(club => club.Name).ToList();
        var targets = leagueClubs
            .Where(club => club.Id != selectedClub.Id)
            .SelectMany(club => club.Players.Select(player => new { Club = club, Player = player }))
            .Select(item =>
            {
                var fee = CalculateBuyFee(item.Player);
                return new TransferMarketPlayerDto(
                    item.Player.Id,
                    item.Player.FullName,
                    item.Player.Position.ToString(),
                    item.Player.SquadNumber,
                    item.Club.Name,
                    item.Player.GetOverallRating(),
                    item.Player.Fitness,
                    item.Player.Morale,
                    fee,
                    selectedClub.TransferBudget >= fee);
            })
            .OrderByDescending(target => target.IsAffordable)
            .ThenByDescending(target => target.OverallRating)
            .ThenBy(target => target.Name)
            .ToList();

        var otherClubs = leagueClubs.Where(club => club.Id != selectedClub.Id).ToList();
        var saleOpportunities = selectedClub.Players
            .OrderByDescending(player => CalculateSellFee(player))
            .ThenByDescending(player => player.GetOverallRating())
            .Select(player =>
            {
                var fee = CalculateSellFee(player);
                var buyer = FindBuyingClub(player, otherClubs, fee);

                return new SaleOpportunityDto(
                    player.Id,
                    player.FullName,
                    player.Position.ToString(),
                    player.SquadNumber,
                    player.GetOverallRating(),
                    player.Fitness,
                    player.Morale,
                    fee,
                    buyer?.Name ?? "No buyer lined up");
            })
            .ToList();

        var recentTransfers = await GetRecentActivityAsync(selectedClub.Id, cancellationToken);

        return new TransferMarketDto(
            selectedClub.TransferBudget,
            targets,
            saleOpportunities,
            recentTransfers);
    }

    public async Task<TransferActionResultDto?> BuyPlayerAsync(
        Guid gameId,
        BuyTransferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub?.League is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var player = gameSave.SelectedClub.League.Clubs
            .Where(club => club.Id != selectedClub.Id)
            .SelectMany(club => club.Players)
            .SingleOrDefault(candidate => candidate.Id == request.PlayerId);

        if (player?.Club is null)
        {
            throw new InvalidOperationException("That transfer target is no longer available.");
        }

        var fromClub = player.Club;
        var fee = CalculateBuyFee(player);
        selectedClub.AdjustTransferBudget(-fee);
        fromClub.AdjustTransferBudget(fee);

        var nextSquadNumber = selectedClub.Players.All(candidate => candidate.SquadNumber != player.SquadNumber)
            ? player.SquadNumber
            : selectedClub.GetNextAvailableSquadNumber();

        player.TransferTo(selectedClub, nextSquadNumber);
        player.AdjustMorale(6);
        fromClub.EnsureCaptain();
        selectedClub.EnsureCaptain();

        var transfer = new Transfer(player, fromClub, selectedClub, fee);
        dbContext.Transfers.Add(transfer);
        dbContext.FinanceEntries.AddRange(
            new FinanceEntry(
                selectedClub,
                FinanceEntryType.TransferExpense,
                fee,
                $"{selectedClub.Name} signed {player.FullName} from {fromClub.Name}.",
                transfer.CompletedAt),
            new FinanceEntry(
                fromClub,
                FinanceEntryType.TransferIncome,
                fee,
                $"{fromClub.Name} sold {player.FullName} to {selectedClub.Name}.",
                transfer.CompletedAt.AddSeconds(1)));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TransferActionResultDto(
            selectedClub.TransferBudget,
            MapActivity(transfer, player.FullName, selectedClub.Id),
            $"{selectedClub.Name} land {player.FullName} from {fromClub.Name} for {fee:C0}.");
    }

    public async Task<TransferActionResultDto?> SellPlayerAsync(
        Guid gameId,
        SellTransferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub?.League is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var player = selectedClub.Players.SingleOrDefault(candidate => candidate.Id == request.PlayerId);
        if (player is null)
        {
            throw new InvalidOperationException("That player is not part of the active club squad.");
        }

        var otherClubs = selectedClub.League.Clubs.Where(club => club.Id != selectedClub.Id).ToList();
        var fee = CalculateSellFee(player);
        var buyer = FindBuyingClub(player, otherClubs, fee);
        if (buyer is null)
        {
            throw new InvalidOperationException("The market does not have a buyer ready for that player right now.");
        }

        selectedClub.AdjustTransferBudget(fee);
        buyer.AdjustTransferBudget(-fee);

        var nextSquadNumber = buyer.Players.All(candidate => candidate.SquadNumber != player.SquadNumber)
            ? player.SquadNumber
            : buyer.GetNextAvailableSquadNumber();

        player.TransferTo(buyer, nextSquadNumber);
        player.AdjustMorale(3);
        selectedClub.EnsureCaptain();
        buyer.EnsureCaptain();

        var transfer = new Transfer(player, selectedClub, buyer, fee);
        dbContext.Transfers.Add(transfer);
        dbContext.FinanceEntries.AddRange(
            new FinanceEntry(
                selectedClub,
                FinanceEntryType.TransferIncome,
                fee,
                $"{selectedClub.Name} sold {player.FullName} to {buyer.Name}.",
                transfer.CompletedAt),
            new FinanceEntry(
                buyer,
                FinanceEntryType.TransferExpense,
                fee,
                $"{buyer.Name} signed {player.FullName} from {selectedClub.Name}.",
                transfer.CompletedAt.AddSeconds(1)));
        await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TransferActionResultDto(
            selectedClub.TransferBudget,
            MapActivity(transfer, player.FullName, selectedClub.Id),
            $"{selectedClub.Name} move {player.FullName} to {buyer.Name} for {fee:C0}.");
    }

    private async Task<FootballManager.Domain.Entities.GameSave?> LoadGameSaveAsync(Guid gameId, CancellationToken cancellationToken)
    {
        return await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.League)
                    .ThenInclude(league => league!.Clubs)
                        .ThenInclude(club => club.Players)
            .Include(save => save.Lineup)
                .ThenInclude(lineup => lineup!.Formation)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);
    }

    private async Task<IReadOnlyCollection<TransferActivityDto>> GetRecentActivityAsync(Guid selectedClubId, CancellationToken cancellationToken)
    {
        var transfers = await dbContext.Transfers
            .AsNoTracking()
            .Include(transfer => transfer.Player)
            .Include(transfer => transfer.FromClub)
            .Include(transfer => transfer.ToClub)
            .Where(transfer => transfer.FromClubId == selectedClubId || transfer.ToClubId == selectedClubId)
            .OrderByDescending(transfer => transfer.CompletedAt)
            .Take(6)
            .ToListAsync(cancellationToken);

        return transfers
            .Select(transfer => MapActivity(
                transfer,
                transfer.Player?.FullName ?? "Unknown player",
                selectedClubId))
            .ToList();
    }

    private static TransferActivityDto MapActivity(Transfer transfer, string playerName, Guid selectedClubId) =>
        new(
            transfer.Id,
            playerName,
            transfer.FromClub?.Name ?? "Unknown club",
            transfer.ToClub?.Name ?? "Unknown club",
            transfer.Fee,
            transfer.CompletedAt,
            transfer.ToClubId == selectedClubId);

    private static Club? FindBuyingClub(Player player, IReadOnlyCollection<Club> clubs, decimal fee)
    {
        return clubs
            .Where(club => club.TransferBudget >= fee)
            .OrderBy(club => club.Players.Count(candidate => candidate.Position == player.Position))
            .ThenByDescending(club => club.TransferBudget)
            .FirstOrDefault();
    }

    private static decimal CalculateBuyFee(Player player) =>
        RoundFee(CalculateBaseValue(player) * 1.05m);

    private static decimal CalculateSellFee(Player player) =>
        RoundFee(CalculateBaseValue(player) * 0.92m);

    private static decimal CalculateBaseValue(Player player)
    {
        var multiplier = player.Position switch
        {
            PlayerPosition.Forward => 1.18m,
            PlayerPosition.Midfielder => 1.10m,
            PlayerPosition.Defender => 0.98m,
            _ => 0.88m
        };

        var baseValue = ((player.GetOverallRating() * 85_000m) + (player.GetReadinessScore() * 22_500m)) * multiplier;
        return decimal.Round(baseValue, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundFee(decimal fee)
    {
        var rounded = decimal.Round(fee / 50_000m, 0, MidpointRounding.AwayFromZero) * 50_000m;
        return decimal.Max(250_000m, rounded);
    }
}
