using FootballManager.Domain.Common;

namespace FootballManager.Domain.Entities;

public sealed class Season
{
    private Season()
    {
    }

    internal Season(string name, League league)
    {
        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        League = league ?? throw new ArgumentNullException(nameof(league));
        LeagueId = league.Id;
        StartsAt = DateTime.UtcNow.Date;
        CurrentRound = 1;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime StartsAt { get; private set; }

    public int CurrentRound { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid LeagueId { get; private set; }

    public League? League { get; private set; }

    public ICollection<Fixture> Fixtures { get; private set; } = [];

    public Fixture ScheduleFixture(Club homeClub, Club awayClub, int roundNumber, DateTime scheduledAt)
    {
        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), "Round number must be greater than zero.");
        }

        var fixture = new Fixture(this, homeClub, awayClub, roundNumber, scheduledAt);
        Fixtures.Add(fixture);
        return fixture;
    }

    public void RefreshCurrentRound()
    {
        var nextRound = Fixtures
            .Where(fixture => !fixture.IsPlayed)
            .OrderBy(fixture => fixture.RoundNumber)
            .Select(fixture => fixture.RoundNumber)
            .FirstOrDefault();

        if (nextRound > 0)
        {
            CurrentRound = nextRound;
            return;
        }

        if (Fixtures.Count > 0)
        {
            CurrentRound = Fixtures.Max(fixture => fixture.RoundNumber);
        }
    }
}
