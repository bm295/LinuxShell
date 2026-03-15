using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Infrastructure.Services.Game;

internal static class FinanceCalculator
{
    public static decimal CalculateWage(Player player)
    {
        var overall = player.GetOverallRating();
        var readiness = player.GetReadinessScore();
        var positionBonus = player.Position switch
        {
            PlayerPosition.Goalkeeper => 500m,
            PlayerPosition.Defender => 1_250m,
            PlayerPosition.Midfielder => 2_250m,
            PlayerPosition.Forward => 3_000m,
            _ => 1_000m
        };

        var wage = 3_500m + (overall * 215m) + (readiness * 52m) + positionBonus;
        return RoundCurrency(wage, 250m);
    }

    public static decimal CalculateWageBill(IEnumerable<Player> players) =>
        players.Sum(CalculateWage);

    public static decimal CalculateMatchIncome(Club homeClub, Club awayClub, int roundNumber)
    {
        var homeQuality = GetAverageOverall(homeClub);
        var awayQuality = GetAverageOverall(awayClub);
        var competitivenessBonus = Math.Abs(homeQuality - awayQuality) <= 3 ? 35_000m : 0m;
        var baseIncome = 250_000m
            + (homeQuality * 2_400m)
            + (awayQuality * 1_600m)
            + (roundNumber * 8_500m)
            + competitivenessBonus;

        return decimal.Max(180_000m, RoundCurrency(baseIncome, 10_000m));
    }

    private static decimal GetAverageOverall(Club club)
    {
        if (club.Players.Count == 0)
        {
            return 60m;
        }

        return decimal.Round(
            club.Players.Average(player => (decimal)player.GetOverallRating()),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal RoundCurrency(decimal amount, decimal increment)
    {
        if (increment <= 0)
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        return decimal.Round(amount / increment, 0, MidpointRounding.AwayFromZero) * increment;
    }
}
