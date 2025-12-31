using Godot;
using FootballManagerSim.Models;
using FootballManagerSim.Simulation;

namespace FootballManagerSim;

public partial class Main : Node
{
    public override void _Ready()
    {
        var league = League.Sample();
        var simulator = new MatchSimulator();
        var result = simulator.PlayMatch(league.Clubs[0], league.Clubs[1]);

        GD.Print($"Welcome to {league.Name}!");
        GD.Print($"{result.Home.Name} {result.HomeGoals} - {result.AwayGoals} {result.Away.Name}");
    }
}
