-- =====================================================================
-- Capitra: Workflow parallel approvals (EF migration 20261110205942_WorkflowParallelApprovals). Safe to run more than once.
--   A step may now have several tasks (one per approver), so the unique index on WFE_TASKS.STEP_INSTANCE_ID becomes a
--   normal index. No data changes; existing tasks keep working. Run after migrate-workflow-sla.sql.
-- =====================================================================

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261110205942_WorkflowParallelApprovals') THEN
    DROP INDEX "UQ_WFE_TASKS_STEP_INSTANCE_ID";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261110205942_WorkflowParallelApprovals') THEN
    CREATE INDEX "IX_WFE_TASKS_STEP_INSTANCE_ID" ON "WFE_TASKS" ("STEP_INSTANCE_ID");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261110205942_WorkflowParallelApprovals') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261110205942_WorkflowParallelApprovals', '10.0.13');
    END IF;
END $EF$;
COMMIT;

