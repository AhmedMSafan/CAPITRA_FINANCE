using Capitra.Application.Abstractions;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Capitra.Infrastructure.Persistence.Interceptors;
using Capitra.Infrastructure.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "CapitraDb";

    public static IServiceCollection AddCapitraInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. Set ConnectionStrings__{ConnectionStringName} or use dotnet user-secrets.");

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<CapitraDbContext>((sp, options) =>
        {
            ConfigureDatabase(options, connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<ICapitraDbContext>(sp => sp.GetRequiredService<CapitraDbContext>());

        services
            .AddIdentityCore<SecUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = false;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<SecRole>()
            .AddEntityFrameworkStores<CapitraDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static void ConfigureDatabase(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable(CapitraDbContext.MigrationsHistoryTable));
}
