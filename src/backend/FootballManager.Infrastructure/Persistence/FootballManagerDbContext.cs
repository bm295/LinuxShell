using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Persistence;

public sealed class FootballManagerDbContext(DbContextOptions<FootballManagerDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    public DbSet<League> Leagues => Set<League>();

    public DbSet<Club> Clubs => Set<Club>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<AcademyPlayer> AcademyPlayers => Set<AcademyPlayer>();

    public DbSet<Formation> Formations => Set<Formation>();

    public DbSet<Lineup> Lineups => Set<Lineup>();

    public DbSet<Season> Seasons => Set<Season>();

    public DbSet<Fixture> Fixtures => Set<Fixture>();

    public DbSet<GameSave> GameSaves => Set<GameSave>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FootballManagerDbContext).Assembly);
    }
}
