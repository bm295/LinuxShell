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
        var rubinSquad = new List<Player>
        {
            new("David Seaman", "GK", 86),
            new("Lee Dixon", "RB", 84),
            new("Tony Adams", "CB", 90),
            new("Sol Campbell", "CB", 87),
            new("Ashley Cole", "LB", 88),
            new("Patrick Vieira", "DM", 91),
            new("Cesc Fabregas", "CM", 88),
            new("Robert Pires", "CM", 87),
            new("Freddie Ljungberg", "RW", 85),
            new("Thierry Henry", "ST", 93),
            new("Dennis Bergkamp", "LW", 92)
        };

        var rubinLineup = new List<string>
        {
            "GK - David Seaman",
            "RB - Lee Dixon",
            "CB - Tony Adams",
            "CB - Sol Campbell",
            "LB - Ashley Cole",
            "DM - Patrick Vieira",
            "CM - Cesc Fabregas",
            "CM - Robert Pires",
            "RW - Freddie Ljungberg",
            "ST - Thierry Henry",
            "LW - Dennis Bergkamp"
        };

        var clubs = new List<Club>
        {
            new("Rubin Kazan", 62, 2_250_000, "4-3-3", rubinSquad, rubinLineup),
            new("Harbor City", 60, 2_000_000, "4-4-2", new List<Player>(), new List<string>()),
            new("Northbridge United", 58, 1_750_000, "4-2-3-1", new List<Player>(), new List<string>()),
            new("Old Town Athletic", 55, 1_500_000, "3-5-2", new List<Player>(), new List<string>()),
            new("Kingsgate FC", 52, 1_250_000, "4-5-1", new List<Player>(), new List<string>()),
            new("Redwood Rovers", 51, 1_200_000, "4-3-3", new List<Player>(), new List<string>()),
            new("Silverlake FC", 57, 1_600_000, "4-4-2", new List<Player>(), new List<string>()),
            new("Summit Athletic", 54, 1_450_000, "4-2-3-1", new List<Player>(), new List<string>()),
            new("Stonebridge Town", 53, 1_380_000, "3-4-3", new List<Player>(), new List<string>()),
            new("Eastport United", 56, 1_520_000, "4-1-4-1", new List<Player>(), new List<string>()),
            new("Ironclad FC", 59, 1_820_000, "4-3-3", new List<Player>(), new List<string>()),
            new("Riverbend SC", 50, 1_150_000, "4-4-2", new List<Player>(), new List<string>()),
            new("Golden Shore", 61, 2_050_000, "4-2-3-1", new List<Player>(), new List<string>()),
            new("Westhaven City", 49, 1_100_000, "3-5-2", new List<Player>(), new List<string>()),
            new("Brighton Vale", 48, 1_050_000, "4-5-1", new List<Player>(), new List<string>()),
            new("Cedar Point FC", 47, 1_000_000, "4-3-3", new List<Player>(), new List<string>()),
            new("Mariner's Bay", 46, 980_000, "4-4-2", new List<Player>(), new List<string>()),
            new("Lakeside United", 45, 950_000, "4-2-3-1", new List<Player>(), new List<string>()),
            new("Granite Albion", 44, 925_000, "3-4-3", new List<Player>(), new List<string>()),
            new("Forestgate FC", 43, 900_000, "4-1-4-1", new List<Player>(), new List<string>())
        };

        return new League("Coastal Premier", clubs);
    }
}
