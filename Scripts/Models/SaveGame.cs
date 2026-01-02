using System.Collections.Generic;

namespace FootballManagerSim.Models;

public sealed class SaveGameData
{
    public string LeagueName { get; set; } = string.Empty;
    public List<ClubData> Clubs { get; set; } = new();
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
