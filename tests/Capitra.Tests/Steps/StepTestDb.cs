using Capitra.Application.Abstractions;
using Capitra.Infrastructure.Persistence;
using Capitra.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Capitra.Tests.Steps;

/// <summary>In-memory database for the workflow step service tests. Every change is stamped by user "loader".</summary>
internal static class StepTestDb
{
    private sealed class FakeUser : ICurrentUser
    {
        public string? UserId => "u1";
        public string? UserName => "loader";
        public bool IsAuthenticated => true;
    }

    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc);
    }

    public static CapitraDbContext Create(string database) =>
        new(new DbContextOptionsBuilder<CapitraDbContext>()
            .UseInMemoryDatabase(database)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new AuditInterceptor(new FakeUser(), new FakeClock()))
            .Options);
}
