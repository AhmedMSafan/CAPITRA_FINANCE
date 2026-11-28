using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Tests.Persistence;

public sealed class MigrationTests
{
    [Fact]
    public void Design_time_model_matches_the_migrations()
    {
        using var context = new CapitraDbContextFactory().CreateDbContext([]);
        Assert.False(context.Database.HasPendingModelChanges(), "Run 'dotnet ef migrations add <Name>' for the latest model change.");
    }

    [Fact]
    public void Runtime_model_matches_the_migrations()
    {
        using var factory = new CapitraApiFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CapitraDbContext>();
        Assert.False(context.Database.HasPendingModelChanges(), "The runtime model differs from the design-time model used for migrations.");
    }
}
