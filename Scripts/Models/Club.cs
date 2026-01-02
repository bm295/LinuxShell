using System.Collections.Generic;

namespace FootballManagerSim.Models;

public sealed record Club(
    string Name,
    int Reputation,
    int Budget,
    string Formation,
    IReadOnlyList<Player> Squad,
    IReadOnlyList<string> Lineup);
