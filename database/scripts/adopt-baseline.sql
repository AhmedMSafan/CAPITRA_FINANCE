-- Keeps a database built by the 74 migrations squashed into the Baseline, with all its data.
-- The API does this itself on startup (BaselineAdoption, when Database:ApplyMigrationsOnStartup is on).
-- Run it by hand only before `dotnet ef database update`:
--   psql -d CAPITRA_DB -f database/scripts/adopt-baseline.sql
-- Nothing is dropped or rewritten: the Baseline is only recorded as applied, so the migrations after it add their tables.
DO $$
BEGIN
    IF to_regclass('public."SYS_EF_MIGRATION_HISTORY"') IS NULL THEN
        RETURN;
    END IF;
    IF EXISTS (SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261118173547_Baseline') THEN
        RETURN;
    END IF;
    IF EXISTS (SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115180000_ToolsLoanCalculator') THEN
        INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion") VALUES ('20261118173547_Baseline', '10.0.13');
        RETURN;
    END IF;
    IF EXISTS (SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY") THEN
        RAISE EXCEPTION 'This database stopped before migration 20261115180000_ToolsLoanCalculator. Run develop as of 18 Nov 2026 once to bring it up to date, then this version.';
    END IF;
END $$;
