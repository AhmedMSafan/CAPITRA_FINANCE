-- =====================================================================
-- Capitra: Factoring workflow, Committee Approval outcomes
-- (EF migration 20261028233000_FactoringCommitteeOutcomes).
--   FACTORING_LIMIT_APPROVAL: Committee Approval now offers ACCEPT and REJECT.
--   ACCEPT goes on to Client Acceptance, REJECT ends the workflow.
-- Run once on CAPITRA_DB after migrate-factoring-workspace.sql; it skips what is already applied.
-- =====================================================================
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261028233000_FactoringCommitteeOutcomes') THEN
    UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = replace(replace(replace("BPMN_XML", 'finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">', 'finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">'), '<bpmn:sequenceFlow id="Flow_0qv4imo" name="" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="" />', '<bpmn:sequenceFlow id="Flow_0qv4imo" name="ACCEPT" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="ACCEPT" />'), '<bpmn:sequenceFlow id="Flow_1bcnv9y" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" />', '<bpmn:sequenceFlow id="Flow_1bcnv9y" name="REJECT" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" finova:outcome="REJECT" />') WHERE "WORKFLOW_CODE" = 'FACTORING_LIMIT_APPROVAL';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261028233000_FactoringCommitteeOutcomes') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261028233000_FactoringCommitteeOutcomes', '10.0.13');
    END IF;
END $EF$;
COMMIT;

