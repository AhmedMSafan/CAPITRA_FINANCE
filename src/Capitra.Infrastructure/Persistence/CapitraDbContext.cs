using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence.Conventions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Capitra.Infrastructure.Persistence;

public sealed class CapitraDbContext(DbContextOptions<CapitraDbContext> options)
    : IdentityDbContext<SecUser, SecRole, string, SecUserClaim, SecUserRole, SecUserLogin, SecRoleClaim, SecUserToken, SecUserPasskey>(options),
      ICapitraDbContext
{
    public const string MigrationsHistoryTable = "SYS_EF_MIGRATION_HISTORY";

    /// <summary>Pinned so the design-time model (migrations) and the runtime model are always the same.</summary>
    protected override Version SchemaVersion => IdentitySchemaVersions.Version3;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await TrackCompanyUserActivityAsync(cancellationToken);
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new ConflictException(
                "This record is used by other records, so it cannot be deleted. Make it inactive instead.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("A record with the same code already exists.", ex);
        }
    }

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>
    /// Billing: a member-company account can use the service while it is active, has a password (activated) and belongs to the
    /// company. Whenever that starts or stops, by whatever screen, a SCR_USER_ACTIVITY period is opened or closed (Cairo dates).
    /// </summary>
    private async Task TrackCompanyUserActivityAsync(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var changed = ChangeTracker.Entries<SecUser>().Where(e => e.State is EntityState.Added or EntityState.Modified).ToList();
        if (changed.Count == 0)
            return;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Cairo));
        foreach (var e in changed)
        {
            var u = e.Entity;
            var nowIn = Billable(u.IsActive, u.PasswordHash, u.MemberCompanyId);
            long? before = null;
            if (e.State == EntityState.Modified)
            {
                var o = e.OriginalValues;
                before = Billable(o.GetValue<bool>(nameof(SecUser.IsActive)), o.GetValue<string?>(nameof(SecUser.PasswordHash)),
                    o.GetValue<long?>(nameof(SecUser.MemberCompanyId)));
            }

            if (before == nowIn)
                continue;
            if (before is { } oldCompany)
            {
                var open = await Set<Capitra.Domain.Modules.Scr.ScrUserActivity>()
                    .Where(a => a.UserId == u.Id && a.MemberCompanyId == oldCompany && a.ActiveTo == null).ToListAsync(ct);
                foreach (var a in open)
                    a.ActiveTo = today < a.ActiveFrom ? a.ActiveFrom : today;
            }

            if (nowIn is { } newCompany)
                Add(new Capitra.Domain.Modules.Scr.ScrUserActivity { MemberCompanyId = newCompany, UserId = u.Id, UserName = u.UserName ?? u.Id, ActiveFrom = today });
        }
    }

    private static long? Billable(bool isActive, string? passwordHash, long? companyId) =>
        isActive && passwordHash is not null && companyId is { } c ? c : null;

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    public IDbContextTransaction? CurrentTransaction => Database.CurrentTransaction;

    public Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default) =>
        Database.ExecuteSqlAsync(sql, cancellationToken);

    public void DiscardChanges() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CapitraDbContext).Assembly);
        NamingStandard.Apply(builder);
    }
}
