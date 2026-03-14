using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using FootballManager.Infrastructure.Seeding;
using FootballManager.Infrastructure.Services;
using FootballManager.Infrastructure.Services.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=football_manager;Username=postgres;Password=postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FootballManagerDatabase") ?? DefaultConnectionString;

        services.AddDbContext<FootballManagerDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBootstrapSummaryService, BootstrapSummaryService>();
        services.AddScoped<IGameSetupService, GameSetupService>();
        services.AddScoped<IClubDashboardService, ClubDashboardService>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
