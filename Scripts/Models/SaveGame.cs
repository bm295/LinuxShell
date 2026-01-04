using System.Collections.Generic;

namespace FootballManagerSim.Models;

public sealed class SaveGameData
{
    public string LeagueName { get; set; } = string.Empty;
    public List<ClubData> Clubs { get; set; } = new();
    public int CurrentRound { get; set; }
    public List<ClubStandingData> Standings { get; set; } = new();
}

public sealed class ClubData
{
    public string Name { get; set; } = string.Empty;
    public int Reputation { get; set; }
    public int Budget { get; set; }
    public string Formation { get; set; } = string.Empty;
    public List<Player> Squad { get; set; } = new();
    public List<string> Lineup { get; set; } = new();
}

public sealed class ClubStandingData
{
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
}
