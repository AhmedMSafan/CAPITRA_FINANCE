using Capitra.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Capitra.Infrastructure.Persistence;

/// <summary>
/// Used by the EF tools (dotnet ef migrations add / database update).
/// Reads the connection string from the CAPITRA_DB_CONNECTION environment variable.
/// </summary>
public sealed class CapitraDbContextFactory : IDesignTimeDbContextFactory<CapitraDbContext>
{
    public CapitraDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CAPITRA_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=CAPITRA_DB";

        var options = new DbContextOptionsBuilder<CapitraDbContext>();
        DependencyInjection.ConfigureDatabase(options, connectionString);
        return new CapitraDbContext(options.Options);
    }
}
