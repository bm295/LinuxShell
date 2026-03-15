using FootballManager.Infrastructure.Persistence;
using FootballManager.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;

namespace FootballManager.Api.IntegrationTests;

public sealed class FootballManagerApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"football-manager-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<FootballManagerDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FootballManagerDbContext>>();
            services.RemoveAll<FootballManagerDbContext>();

            services.AddDbContext<FootballManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FootballManagerDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            if (!dbContext.Formations.Any())
            {
                dbContext.Formations.AddRange(SeedDataFactory.CreateFormations());
                dbContext.SaveChanges();
            }

            if (!dbContext.Leagues.Any(league => league.IsTemplate))
            {
                dbContext.Leagues.Add(SeedDataFactory.CreateInitialLeague());
                dbContext.SaveChanges();
            }
        });
    }
}
