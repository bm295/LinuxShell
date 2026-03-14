using FootballManager.Domain.Common;

namespace FootballManager.Domain.Entities;

public sealed class League
{
    private League()
    {
    }

    public League(string name, bool isTemplate = false)
    {
        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        IsTemplate = isTemplate;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsTemplate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<Club> Clubs { get; private set; } = [];

    public ICollection<Season> Seasons { get; private set; } = [];

    public Club AddClub(string name, decimal transferBudget)
    {
        if (Clubs.Any(club => string.Equals(club.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Club '{name}' already exists in league '{Name}'.");
        }

        var club = new Club(name, transferBudget, this);
        Clubs.Add(club);
        return club;
    }

    public Season StartSeason(string name)
    {
        var season = new Season(name, this);
        Seasons.Add(season);
        return season;
    }
}
