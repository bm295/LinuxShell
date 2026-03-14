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
        Id = Guid.NewGuid();
        SelectedClubId = selectedClub.Id;
        SeasonId = season.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid SelectedClubId { get; private set; }

    public Club? SelectedClub { get; private set; }

    public Guid SeasonId { get; private set; }

    public Season? Season { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
