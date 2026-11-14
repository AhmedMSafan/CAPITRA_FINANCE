using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Adm;
using Capitra.Infrastructure.Persistence;
using Capitra.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Persistence;

public sealed class AuditInterceptorTests
{
    private sealed class FakeUser : ICurrentUser
    {
        public string? UserId { get; set; } = "u1";
        public string? UserName { get; set; } = "creator";
        public bool IsAuthenticated => UserName is not null;
        public IReadOnlyCollection<string> Roles => [];
    }

    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
    }

    private static CapitraDbContext CreateContext(string database, ICurrentUser user, IClock clock) =>
        new(new DbContextOptionsBuilder<CapitraDbContext>()
            .UseInMemoryDatabase(database)
            .AddInterceptors(new AuditInterceptor(user, clock))
            .Options);

    [Fact]
    public async Task Stamps_created_and_updated_columns_from_the_server()
    {
        var database = Guid.NewGuid().ToString();
        var user = new FakeUser();
        var clock = new FakeClock();

        await using (var context = CreateContext(database, user, clock))
        {
            context.Add(new AdmLkpCurrency { CurrencyCode = "EGP", CurrencyNameAr = "جنيه", CurrencyNameEn = "Egyptian Pound", CreatedBy = "spoofed", RowVersion = 99 });
            await context.SaveChangesAsync();
        }

        user.UserName = "editor";
        var created = clock.UtcNow;
        clock.UtcNow = created.AddHours(2);

        await using (var context = CreateContext(database, user, clock))
        {
            var currency = await context.Set<AdmLkpCurrency>().SingleAsync();
            Assert.Equal("creator", currency.CreatedBy);
            Assert.Equal(created, currency.CreatedAt);
            Assert.Null(currency.UpdatedBy);
            Assert.Equal(1, currency.RowVersion);

            currency.CurrencyNameEn = "Egyptian pound";
            currency.CreatedBy = "tampered";
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext(database, user, clock))
        {
            var currency = await context.Set<AdmLkpCurrency>().SingleAsync();
            Assert.Equal("creator", currency.CreatedBy);
            Assert.Equal("editor", currency.UpdatedBy);
            Assert.Equal(clock.UtcNow, currency.UpdatedAt);
            Assert.Equal(2, currency.RowVersion);
        }
    }

    [Fact]
    public async Task Uses_SYSTEM_when_nobody_is_signed_in()
    {
        await using var context = CreateContext(Guid.NewGuid().ToString(), new FakeUser { UserName = null }, new FakeClock());
        context.Add(new AdmLkpRiskLevel { RiskLevelCode = "LOW", RiskLevelNameAr = "منخفض", RiskLevelNameEn = "Low" });
        await context.SaveChangesAsync();

        Assert.Equal(AuditInterceptor.SystemUser, (await context.Set<AdmLkpRiskLevel>().SingleAsync()).CreatedBy);
    }
}
