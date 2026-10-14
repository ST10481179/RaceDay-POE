using Microsoft.AspNetCore.Identity;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Organiser", "Participant" })
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed the {role} role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        var email = configuration["BootstrapOrganiser:Email"];
        var password = configuration["BootstrapOrganiser:Password"];
        var hasEmail = !string.IsNullOrWhiteSpace(email);
        var hasPassword = !string.IsNullOrWhiteSpace(password);
        if (hasEmail != hasPassword)
        {
            throw new InvalidOperationException(
                "Both BootstrapOrganiser:Email and BootstrapOrganiser:Password must be configured together.");
        }

        if (!hasEmail)
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email!);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = configuration["BootstrapOrganiser:FirstName"] ?? "RaceDay",
                LastName = configuration["BootstrapOrganiser:LastName"] ?? "Organiser"
            };
            var createResult = await userManager.CreateAsync(user, password!);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create the bootstrap Organiser: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        else if (!await userManager.IsInRoleAsync(user, "Organiser"))
        {
            throw new InvalidOperationException(
                "The configured BootstrapOrganiser email belongs to an account without the Organiser role. " +
                "Use a dedicated bootstrap account rather than promoting an existing account.");
        }

        if (!await userManager.IsInRoleAsync(user, "Organiser"))
        {
            var roleResult = await userManager.AddToRoleAsync(user, "Organiser");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the Organiser role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }
    }
}
