using System.Text.Json;

namespace FootballManager.Domain.Entities;

public sealed class Lineup
{
    private Lineup()
    {
    }

    internal Lineup(GameSave gameSave, Formation formation, IEnumerable<Guid> starterPlayerIds)
    {
        GameSave = gameSave ?? throw new ArgumentNullException(nameof(gameSave));
        GameSaveId = gameSave.Id;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Update(formation, starterPlayerIds);
    }

    public Guid Id { get; private set; }

    public Guid GameSaveId { get; private set; }

    public GameSave? GameSave { get; private set; }

    public Guid FormationId { get; private set; }

    public Formation? Formation { get; private set; }

    public string StarterPlayerIds { get; private set; } = "[]";

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Guid> GetStarterPlayerIds() =>
        JsonSerializer.Deserialize<List<Guid>>(StarterPlayerIds) ?? [];

    public void Update(Formation formation, IEnumerable<Guid> starterPlayerIds)
    {
        Formation = formation ?? throw new ArgumentNullException(nameof(formation));
        FormationId = formation.Id;

        if (starterPlayerIds is null)
        {
            throw new ArgumentNullException(nameof(starterPlayerIds));
        }

        var starterIds = starterPlayerIds
            .Distinct()
            .ToList();

        if (starterIds.Count != formation.RequiredStarters)
        {
            throw new InvalidOperationException($"Lineup must contain exactly {formation.RequiredStarters} unique starters.");
        }

        StarterPlayerIds = JsonSerializer.Serialize(starterIds);
        UpdatedAt = DateTime.UtcNow;
    }
}
