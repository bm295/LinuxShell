using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Tests;

public sealed class LeagueTests
{
    [Fact]
    public void AddClub_RejectsDuplicateClubNames()
    {
        var league = new League("Founders League");
        league.AddClub("Northbridge FC", 1_000_000m);

        var action = () => league.AddClub("northbridge fc", 2_000_000m);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddPlayer_RejectsDuplicateSquadNumbersWithinClub()
    {
        var league = new League("Founders League");
        var club = league.AddClub("Northbridge FC", 1_000_000m);
        club.AddPlayer("Alex", "Ward", PlayerPosition.Goalkeeper, 31, 1, 45, 82, 60, 84, 79);

        var action = () => club.AddPlayer("Theo", "Silva", PlayerPosition.Defender, 24, 1, 63, 80, 68, 81, 77);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("Squad number", exception.Message);
    }
}
