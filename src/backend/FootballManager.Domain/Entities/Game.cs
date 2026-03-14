namespace FootballManager.Domain.Entities;

public sealed class Game
{
    private Game()
    {
    }

    public Game(DateTime? createdAt = null)
    {
        Id = Guid.NewGuid();
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
