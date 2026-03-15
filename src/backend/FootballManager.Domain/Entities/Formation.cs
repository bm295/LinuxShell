using FootballManager.Domain.Common;

namespace FootballManager.Domain.Entities;

public sealed class Formation
{
    private Formation()
    {
    }

    public Formation(string name, int defenders, int midfielders, int forwards)
    {
        if (defenders + midfielders + forwards != 10)
        {
            throw new InvalidOperationException("A formation must assign exactly 10 outfield players.");
        }

        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Defenders = defenders;
        Midfielders = midfielders;
        Forwards = forwards;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Defenders { get; private set; }

    public int Midfielders { get; private set; }

    public int Forwards { get; private set; }

    public int Goalkeepers => 1;

    public int RequiredStarters => Goalkeepers + Defenders + Midfielders + Forwards;

    public DateTime CreatedAt { get; private set; }

    public ICollection<Lineup> Lineups { get; private set; } = [];
}
