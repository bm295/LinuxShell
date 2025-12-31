using System.Collections.Generic;

namespace FootballManagerSim.Models;

public sealed class League
{
    public string Name { get; }
    public IReadOnlyList<Club> Clubs { get; }

    public League(string name, IReadOnlyList<Club> clubs)
    {
        Name = name;
        Clubs = clubs;
    }

    public static League Sample()
    {
        var clubs = new List<Club>
        {
            new("Harbor City", 60, 2_000_000),
            new("Northbridge United", 58, 1_750_000),
            new("Old Town Athletic", 55, 1_500_000),
            new("Kingsgate FC", 52, 1_250_000)
        };

        return new League("Coastal Premier", clubs);
    }
}
