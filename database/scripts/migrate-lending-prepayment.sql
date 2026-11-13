-- =====================================================================================================================
-- Capitra: Retail Lending part prepayment and early settlement (migration 20261113000000_LendingPrepayment).
-- Adds to LND_PRODUCT_VERSIONS and LND_LOANS: IS_PREPAYMENT_ALLOWED (true), PREPAYMENT_RECALC_CODE (REDUCE_INSTALMENT),
-- PREPAYMENT_FEE_PCT and EARLY_SETTLEMENT_FEE_PCT (0). Existing products and loans get these defaults; change a product's in a new
-- version (Administration > Lending Product Terms).
-- Run in pgAdmin > Query Tool on CAPITRA_DB after migrate-lending-write-off.sql. Press F5. Safe to run again.
-- =====================================================================================================================
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_PRODUCT_VERSIONS" ADD "IS_PREPAYMENT_ALLOWED" boolean NOT NULL DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_PRODUCT_VERSIONS" ADD "PREPAYMENT_RECALC_CODE" character varying(20) NOT NULL DEFAULT 'REDUCE_INSTALMENT';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_PRODUCT_VERSIONS" ADD "PREPAYMENT_FEE_PCT" numeric(9,6) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_PRODUCT_VERSIONS" ADD "EARLY_SETTLEMENT_FEE_PCT" numeric(9,6) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_LOANS" ADD "IS_PREPAYMENT_ALLOWED" boolean NOT NULL DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_LOANS" ADD "PREPAYMENT_RECALC_CODE" character varying(20) NOT NULL DEFAULT 'REDUCE_INSTALMENT';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_LOANS" ADD "PREPAYMENT_FEE_PCT" numeric(9,6) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    ALTER TABLE "LND_LOANS" ADD "EARLY_SETTLEMENT_FEE_PCT" numeric(9,6) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261113000000_LendingPrepayment') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261113000000_LendingPrepayment', '10.0.13');
    END IF;
END $EF$;
COMMIT;
