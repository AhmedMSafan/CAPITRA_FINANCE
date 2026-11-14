-- =====================================================================
-- Capitra: Workflow documents (EF migration 20261109125556_WorkflowDocuments). Safe to run more than once.
--   WFE_FORMS becomes the Document Designer table: WORKFLOW_DEFINITION_ID is now optional (a document belongs to no workflow)
--   and the new column FIELDS_JSON holds the document's own fields. Existing forms keep their workflow and keep working.
--   Run after migrate-workflow-forms.sql.
-- =====================================================================

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261109125556_WorkflowDocuments') THEN
    ALTER TABLE "WFE_FORMS" ALTER COLUMN "WORKFLOW_DEFINITION_ID" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261109125556_WorkflowDocuments') THEN
    ALTER TABLE "WFE_FORMS" ADD "FIELDS_JSON" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261109125556_WorkflowDocuments') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261109125556_WorkflowDocuments', '10.0.13');
    END IF;
END $EF$;
COMMIT;

