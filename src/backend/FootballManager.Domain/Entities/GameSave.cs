namespace FootballManager.Domain.Entities;

public sealed class GameSave
{
    private GameSave()
    {
    }

    public GameSave(Club selectedClub, Season season)
    {
        SelectedClub = selectedClub ?? throw new ArgumentNullException(nameof(selectedClub));
        Season = season ?? throw new ArgumentNullException(nameof(season));
        var now = DateTime.UtcNow;
        Id = Guid.NewGuid();
        SelectedClubId = selectedClub.Id;
        SeasonId = season.Id;
        CreatedAt = now;
        LastSavedAt = now;
        SaveName = BuildDefaultName(selectedClub.Name, season.Name);
    }

    public Guid Id { get; private set; }

    public Guid SelectedClubId { get; private set; }

    public Club? SelectedClub { get; private set; }

    public Guid SeasonId { get; private set; }

    public Season? Season { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string SaveName { get; private set; } = string.Empty;

    public DateTime LastSavedAt { get; private set; }

    public Lineup? Lineup { get; private set; }

    public void Save(string? saveName)
    {
        SaveName = string.IsNullOrWhiteSpace(saveName)
            ? (string.IsNullOrWhiteSpace(SaveName) ? BuildDefaultName(SelectedClub?.Name, Season?.Name) : SaveName)
            : saveName.Trim();
        LastSavedAt = DateTime.UtcNow;
    }

    public void EnsureMetadata()
    {
        if (string.IsNullOrWhiteSpace(SaveName))
        {
            SaveName = BuildDefaultName(SelectedClub?.Name, Season?.Name);
        }

        if (LastSavedAt == default)
        {
            LastSavedAt = CreatedAt == default ? DateTime.UtcNow : CreatedAt;
        }
    }

    public Lineup SetLineup(Formation formation, IEnumerable<Guid> starterPlayerIds)
    {
        if (Lineup is null)
        {
            Lineup = new Lineup(this, formation, starterPlayerIds);
            return Lineup;
        }

        Lineup.Update(formation, starterPlayerIds);
        return Lineup;
    }

    private static string BuildDefaultName(string? clubName, string? seasonName)
    {
        var club = string.IsNullOrWhiteSpace(clubName) ? "Club Journey" : clubName.Trim();
        var season = string.IsNullOrWhiteSpace(seasonName) ? "Season 1" : seasonName.Trim();
        return $"{club} - {season}";
    }
}
