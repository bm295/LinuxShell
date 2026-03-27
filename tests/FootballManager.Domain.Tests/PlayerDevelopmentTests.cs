using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Tests;

public sealed class PlayerDevelopmentTests
{
    [Fact]
    public void AdvanceDevelopment_AcademyPlayerGainsProgressAndOverall()
    {
        var league = new League("Founders League");
        var club = league.AddClub("Northbridge FC", 1_000_000m);
        var player = club.AddAcademyPlayer(
            "Theo",
            "Ward",
            PlayerPosition.Forward,
            17,
            58,
            44,
            51,
            69,
            70,
            89,
            40,
            "Finishing");
        var startingProgress = player.DevelopmentProgress;
        var startingOverall = player.GetOverallRating();

        player.AdvanceDevelopment();

        Assert.True(player.DevelopmentProgress >= startingProgress + 3);
        Assert.True(player.GetOverallRating() > startingOverall);
    }

    [Fact]
    public void ApplyAgeBasedMatchDevelopment_YoungStarterRaisesVisibleOverallAfterOneMatch()
    {
        var league = new League("Founders League");
        var club = league.AddClub("Northbridge FC", 1_000_000m);
        var youngPlayer = club.AddPlayer("Leo", "Young", PlayerPosition.Midfielder, 20, 8, 70, 70, 70, 82, 78);
        var overallBefore = youngPlayer.GetOverallRating();
        var coreTotalBefore = youngPlayer.Attack + youngPlayer.Defense + youngPlayer.Passing;

        youngPlayer.ApplyAgeBasedMatchDevelopment(playedMatch: true);

        var coreTotalAfter = youngPlayer.Attack + youngPlayer.Defense + youngPlayer.Passing;

        Assert.True(youngPlayer.GetOverallRating() > overallBefore);
        Assert.True(coreTotalAfter > coreTotalBefore);
        Assert.Equal(1, youngPlayer.AttributeProgress);
    }

    [Fact]
    public void ApplyAgeBasedMatchDevelopment_PrimeStarterDoesNotReceiveYoungPlayerGrowthSpike()
    {
        var league = new League("Founders League");
        var club = league.AddClub("Northbridge FC", 1_000_000m);
        var primePlayer = club.AddPlayer("Mason", "Prime", PlayerPosition.Midfielder, 27, 10, 70, 70, 70, 82, 78);
        var overallBefore = primePlayer.GetOverallRating();
        var coreTotalBefore = primePlayer.Attack + primePlayer.Defense + primePlayer.Passing;

        primePlayer.ApplyAgeBasedMatchDevelopment(playedMatch: true);

        var coreTotalAfter = primePlayer.Attack + primePlayer.Defense + primePlayer.Passing;

        Assert.Equal(overallBefore, primePlayer.GetOverallRating());
        Assert.Equal(coreTotalBefore, coreTotalAfter);
        Assert.Equal(0, primePlayer.AttributeProgress);
    }
}
