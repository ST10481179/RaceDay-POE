using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;
using Xunit;

namespace RaceDay.WebAPI.Tests;

public sealed class RaceDayApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "RaceDayIntegrationTestSigningKey_AtLeast32Bytes!");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        });
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using (var dbContext = new ApplicationDbContext(options))
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        using var client = CreateClient();

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var organiser = new ApplicationUser
        {
            UserName = "organiser@raceday.test",
            Email = "organiser@raceday.test",
            EmailConfirmed = true,
            FirstName = "RaceDay",
            LastName = "Organiser"
        };
        var createResult = await userManager.CreateAsync(organiser, "RaceDay1234!");
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not create the test Organiser: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
        }

        var roleResult = await userManager.AddToRoleAsync(organiser, "Organiser");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not assign the test Organiser role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await connection.DisposeAsync();
    }
}