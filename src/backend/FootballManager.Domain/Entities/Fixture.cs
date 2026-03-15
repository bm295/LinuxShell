namespace FootballManager.Domain.Entities;

public sealed class Fixture
{
    private Fixture()
    {
    }

    internal Fixture(Season season, Club homeClub, Club awayClub, int roundNumber, DateTime scheduledAt)
    {
        if (homeClub is null)
        {
            throw new ArgumentNullException(nameof(homeClub));
        }

        if (awayClub is null)
        {
            throw new ArgumentNullException(nameof(awayClub));
        }

        if (homeClub.Id == awayClub.Id)
        {
            throw new InvalidOperationException("A club cannot play against itself.");
        }

        Id = Guid.NewGuid();
        Season = season ?? throw new ArgumentNullException(nameof(season));
        SeasonId = season.Id;
        HomeClub = homeClub;
        HomeClubId = homeClub.Id;
        AwayClub = awayClub;
        AwayClubId = awayClub.Id;
        RoundNumber = roundNumber;
        ScheduledAt = scheduledAt;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid SeasonId { get; private set; }

    public Season? Season { get; private set; }

    public Guid HomeClubId { get; private set; }

    public Club? HomeClub { get; private set; }

    public Guid AwayClubId { get; private set; }

    public Club? AwayClub { get; private set; }

    public int RoundNumber { get; private set; }

    public DateTime ScheduledAt { get; private set; }

    public bool IsPlayed { get; private set; }

    public int? HomeGoals { get; private set; }

    public int? AwayGoals { get; private set; }

    public DateTime? PlayedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void Complete(int homeGoals, int awayGoals, DateTime playedAt)
    {
        if (IsPlayed)
        {
            throw new InvalidOperationException("This fixture has already been played.");
        }

        if (homeGoals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(homeGoals), "Goals cannot be negative.");
        }

        if (awayGoals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(awayGoals), "Goals cannot be negative.");
        }

        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        PlayedAt = playedAt;
        IsPlayed = true;
    }
}
