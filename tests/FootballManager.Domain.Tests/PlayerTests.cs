using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Tests;

public sealed class PlayerTests
{
    [Fact]
    public void ChangePosition_RecalculatesOverallUsingTheNewRoleWeights()
    {
        var league = new League("Founders League");
        var club = league.AddClub("Northbridge FC", 1_000_000m);
        var player = club.AddPlayer("Theo", "Ward", PlayerPosition.Midfielder, 24, 8, 80, 40, 60, 83, 78);
        var midfieldOverall = player.GetOverallRating();

        player.ChangePosition(PlayerPosition.Forward);

        Assert.Equal(PlayerPosition.Forward, player.Position);
        Assert.NotEqual(midfieldOverall, player.GetOverallRating());
    }
}
