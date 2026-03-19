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
}
