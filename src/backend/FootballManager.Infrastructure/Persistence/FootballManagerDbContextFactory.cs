using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FootballManager.Infrastructure.Persistence;

public sealed class FootballManagerDbContextFactory : IDesignTimeDbContextFactory<FootballManagerDbContext>
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=football_manager;Username=postgres;Password=postgres";

    public FootballManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FootballManagerDbContext>();
        optionsBuilder.UseNpgsql(DefaultConnectionString);

        return new FootballManagerDbContext(optionsBuilder.Options);
    }
}
