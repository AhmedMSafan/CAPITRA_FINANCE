using Capitra.Application.Abstractions;
using Capitra.Infrastructure.Communication;
using Capitra.Infrastructure.Files;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Capitra.Infrastructure.Persistence.Interceptors;
using Capitra.Infrastructure.Persistence.Reports;
using Capitra.Infrastructure.Time;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddSingleton<IWorkbookReader, ExcelWorkbookReader>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<CapitraDbContext>((sp, options) =>
        {
            ConfigureDatabase(options, connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<ICapitraDbContext>(sp => sp.GetRequiredService<CapitraDbContext>());
        services.AddScoped<IApexSqlReader, ApexSqlReader>();
        services.AddScoped<IUserDirectory, Identity.UserDirectory>();
        services.AddScoped<Capitra.Application.Modules.Scr.IScrUserProfiles, Identity.ScrUserProfiles>();
        services.AddScoped<Capitra.Application.Modules.Scr.IScrCompanyUserCounts, Identity.ScrUserProfiles>();

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

        // account e-mails: activation and password-reset links (no password is ever e-mailed)
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.Section));
        var linkHours = configuration.GetSection(EmailOptions.Section).GetValue<int?>(nameof(EmailOptions.LinkHours)) ?? 48;
        services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(linkHours));
        // Data Protection keys (links, stored mail-server passwords) live in the database, so they survive restarts and new servers
        services.AddDataProtection().SetApplicationName("CAPITRA");
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
            new ConfigureOptions<KeyManagementOptions>(o => o.XmlRepository = new DatabaseKeyRepository(sp.GetRequiredService<IServiceScopeFactory>())));
        // Communication Center: the active mail server (else the "Email" settings) sends every e-mail
        services.AddScoped<MailSettings>();
        services.AddScoped<CmcMailServerService>();
        services.AddScoped<IMailTransport, SmtpMailTransport>();
        services.AddScoped<AccountEmails>();
        services.AddScoped<CompanyAccess>();

        return services;
    }

    public static void ConfigureDatabase(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable(CapitraDbContext.MigrationsHistoryTable));
}
