-- =====================================================================
-- Capitra: Treasury payment (EF migration 20261105222116_TreasuryPayment). Safe to run more than once.
--   TRS_DISBURSEMENT_FUNDINGS gets PAID_FROM_ACCOUNT_ID (our bank account the client is paid from)
--   and GL_TRAN_SET_ID (the GL transaction set Execute posted the drawdown and payment under).
-- Run in pgAdmin > Query Tool on CAPITRA_DB (F5). Then run setup-treasury-disburse-gl.sql.
-- =====================================================================
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261105222116_TreasuryPayment') THEN
    ALTER TABLE "TRS_DISBURSEMENT_FUNDINGS" ADD "GL_TRAN_SET_ID" character varying(40);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261105222116_TreasuryPayment') THEN
    ALTER TABLE "TRS_DISBURSEMENT_FUNDINGS" ADD "PAID_FROM_ACCOUNT_ID" bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261105222116_TreasuryPayment') THEN
    CREATE INDEX "IX_TRS_DISBURSEMENT_FUNDINGS_PAID_FROM_ACCOUNT_ID" ON "TRS_DISBURSEMENT_FUNDINGS" ("PAID_FROM_ACCOUNT_ID");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261105222116_TreasuryPayment') THEN
    ALTER TABLE "TRS_DISBURSEMENT_FUNDINGS" ADD CONSTRAINT "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_BANK_ACCOUNTS" FOREIGN KEY ("PAID_FROM_ACCOUNT_ID") REFERENCES "TRS_BANK_ACCOUNTS" ("ACCOUNT_ID") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261105222116_TreasuryPayment') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261105222116_TreasuryPayment', '10.0.13');
    END IF;
END $EF$;
COMMIT;

