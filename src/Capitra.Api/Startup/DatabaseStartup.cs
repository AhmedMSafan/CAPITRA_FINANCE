using System.Security.Claims;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Startup;

/// <summary>
/// Optional startup work, both off unless configured:
/// Database:ApplyMigrationsOnStartup applies pending migrations;
/// Seed:AdminUserName and Seed:AdminPassword create the first administrator (role ADMIN, every permission).
/// </summary>
public static class DatabaseStartup
{
    public const string AdminRoleName = "ADMIN";

    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        if (configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
            await provider.GetRequiredService<CapitraDbContext>().Database.MigrateAsync();

        var adminUserName = configuration["Seed:AdminUserName"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminUserName) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        await SeedAdministratorAsync(provider, adminUserName, adminPassword);
    }

    private static async Task SeedAdministratorAsync(IServiceProvider provider, string userName, string password)
    {
        var roles = provider.GetRequiredService<RoleManager<SecRole>>();
        var users = provider.GetRequiredService<UserManager<SecUser>>();

        var role = await roles.FindByNameAsync(AdminRoleName);
        if (role is null)
        {
            role = new SecRole { Name = AdminRoleName, Description = "System administrator, all permissions" };
            EnsureSucceeded(await roles.CreateAsync(role), "create role " + AdminRoleName);
            EnsureSucceeded(await roles.AddClaimAsync(role, new Claim(Permissions.ClaimType, Permissions.All)), "grant role permissions");
        }

        if (await users.FindByNameAsync(userName) is not null)
            return;

        var user = new SecUser { UserName = userName, FullName = "System Administrator", IsActive = true };
        EnsureSucceeded(await users.CreateAsync(user, password), "create administrator " + userName);
        EnsureSucceeded(await users.AddToRoleAsync(user, AdminRoleName), "add administrator to role");
    }

    private static void EnsureSucceeded(IdentityResult result, string step)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding failed to {step}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
