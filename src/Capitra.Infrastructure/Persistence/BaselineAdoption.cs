using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Persistence;

/// <summary>
/// Keeps databases built by the 74 migrations the Baseline replaced, with all their data. Such a database already has every
/// table the Baseline would create, so the Baseline is recorded as applied instead of run; the migrations after it (Screening, ...)
/// then apply as usual. Nothing is dropped or rewritten. A database that stopped before the last old migration is refused with an
/// error before anything changes: bring it up to date with develop as of 18 Nov 2026 first. Run before Migrate (DatabaseStartup)
/// and by database/scripts/adopt-baseline.sql for `dotnet ef database update`.
/// </summary>
public static class BaselineAdoption
{
    public const string BaselineId = "20261118173547_Baseline";
    public const string LastOldMigrationId = "20261115180000_ToolsLoanCalculator";

    public const string Sql = $"""
        DO $$
        BEGIN
            IF to_regclass('public."{CapitraDbContext.MigrationsHistoryTable}"') IS NULL THEN
                RETURN; -- an empty database: the Baseline builds it
            END IF;
            IF EXISTS (SELECT 1 FROM "{CapitraDbContext.MigrationsHistoryTable}" WHERE "MigrationId" = '{BaselineId}') THEN
                RETURN; -- already adopted, or built from the Baseline
            END IF;
            IF EXISTS (SELECT 1 FROM "{CapitraDbContext.MigrationsHistoryTable}" WHERE "MigrationId" = '{LastOldMigrationId}') THEN
                INSERT INTO "{CapitraDbContext.MigrationsHistoryTable}" ("MigrationId", "ProductVersion") VALUES ('{BaselineId}', '10.0.13');
                RETURN;
            END IF;
            IF EXISTS (SELECT 1 FROM "{CapitraDbContext.MigrationsHistoryTable}") THEN
                RAISE EXCEPTION 'This database stopped before migration {LastOldMigrationId}. Run develop as of 18 Nov 2026 once to bring it up to date, then this version.';
            END IF;
        END $$;
        """;

    public static Task AdoptAsync(DbContext db, CancellationToken cancellationToken = default) =>
        db.Database.ExecuteSqlRawAsync(Sql, cancellationToken);
}
