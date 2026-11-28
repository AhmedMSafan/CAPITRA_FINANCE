using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CF_FINANCE in six steps (Ahmed, 26 Nov 2026): Application (New Finance: main data only), Credit approval (decides each charge:
    /// upfront or added to the schedule), Charges and claim (executes the rule CF_CHARGES for the upfront charges, then the claim payment),
    /// Schedule and contract, Delivery and customer receipt, Supplier payment. LND_LOANS keeps the claim and LND_LOAN_CHARGES whether
    /// Credit changed a charge's default. The rule CF_CHARGES (admin fee, earned at once, upfront by default, its own GL lines) replaces
    /// CF_CLAIM and the admin fee on CF_DELIVERY; the product action CF_NEW_FINANCE is removed.
    /// </summary>
    public partial class CfFinanceStepsV2 : Migration
    {
        private const string FinanceBpmn = """
<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_CFF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_CF_FINANCE" name="Consumer Finance" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_ref&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;module_code&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false}]">
    <bpmn:startEvent id="StartEvent_1" name="Submitted" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:outgoing>Flow_Start</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id="Task_Credit" name="Credit approval" finova:stepCode="CF_CREDIT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Start</bpmn:incoming>
      <bpmn:outgoing>Flow_Credit</bpmn:outgoing>
      <bpmn:outgoing>Flow_CreditReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_Charges" name="Charges and claim" finova:stepCode="CF_CHARGES" finova:ruleCode="CF_CHARGES" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Credit</bpmn:incoming>
      <bpmn:outgoing>Flow_Charges</bpmn:outgoing>
      <bpmn:outgoing>Flow_ChargesReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_Schedule" name="Schedule and contract" finova:stepCode="CF_SCHEDULE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Charges</bpmn:incoming>
      <bpmn:outgoing>Flow_Schedule</bpmn:outgoing>
      <bpmn:outgoing>Flow_ScheduleReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_Delivery" name="Delivery and customer receipt" finova:stepCode="CF_DELIVERY" finova:ruleCode="CF_DELIVERY" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Schedule</bpmn:incoming>
      <bpmn:outgoing>Flow_Delivery</bpmn:outgoing>
      <bpmn:outgoing>Flow_DeliveryReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_Payment" name="Supplier payment" finova:stepCode="CF_SUPPLIER_PAYMENT" finova:ruleCode="CF_SUPPLIER_PAYMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Delivery</bpmn:incoming>
      <bpmn:outgoing>Flow_Payment</bpmn:outgoing>
      <bpmn:outgoing>Flow_PaymentReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:incoming>Flow_CreditReject</bpmn:incoming>
      <bpmn:incoming>Flow_ChargesReject</bpmn:incoming>
      <bpmn:incoming>Flow_ScheduleReject</bpmn:incoming>
      <bpmn:incoming>Flow_DeliveryReject</bpmn:incoming>
      <bpmn:incoming>Flow_Payment</bpmn:incoming>
      <bpmn:incoming>Flow_PaymentReject</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" name="" sourceRef="StartEvent_1" targetRef="Task_Credit" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_Credit" name="ACCEPT" sourceRef="Task_Credit" targetRef="Task_Charges" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_CreditReject" name="REJECT" sourceRef="Task_Credit" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_Charges" name="ACCEPT" sourceRef="Task_Charges" targetRef="Task_Schedule" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_ChargesReject" name="REJECT" sourceRef="Task_Charges" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_Schedule" name="ACCEPT" sourceRef="Task_Schedule" targetRef="Task_Delivery" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_ScheduleReject" name="REJECT" sourceRef="Task_Schedule" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_Delivery" name="ACCEPT" sourceRef="Task_Delivery" targetRef="Task_Payment" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_DeliveryReject" name="REJECT" sourceRef="Task_Delivery" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_Payment" name="ACCEPT" sourceRef="Task_Payment" targetRef="EndEvent_1" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_PaymentReject" name="REJECT" sourceRef="Task_Payment" targetRef="EndEvent_1" finova:outcome="REJECT" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_CF_FINANCE">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="100" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Credit_di" bpmnElement="Task_Credit"><dc:Bounds x="180" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Charges_di" bpmnElement="Task_Charges"><dc:Bounds x="360" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Schedule_di" bpmnElement="Task_Schedule"><dc:Bounds x="540" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Delivery_di" bpmnElement="Task_Delivery"><dc:Bounds x="720" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Payment_di" bpmnElement="Task_Payment"><dc:Bounds x="900" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="1080" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="136" y="200" /><di:waypoint x="180" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Credit_di" bpmnElement="Flow_Credit"><di:waypoint x="320" y="200" /><di:waypoint x="360" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_CreditReject_di" bpmnElement="Flow_CreditReject"><di:waypoint x="250" y="240" /><di:waypoint x="250" y="300" /><di:waypoint x="1098" y="300" /><di:waypoint x="1098" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Charges_di" bpmnElement="Flow_Charges"><di:waypoint x="500" y="200" /><di:waypoint x="540" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_ChargesReject_di" bpmnElement="Flow_ChargesReject"><di:waypoint x="430" y="240" /><di:waypoint x="430" y="300" /><di:waypoint x="1098" y="300" /><di:waypoint x="1098" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Schedule_di" bpmnElement="Flow_Schedule"><di:waypoint x="680" y="200" /><di:waypoint x="720" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_ScheduleReject_di" bpmnElement="Flow_ScheduleReject"><di:waypoint x="610" y="240" /><di:waypoint x="610" y="300" /><di:waypoint x="1098" y="300" /><di:waypoint x="1098" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Delivery_di" bpmnElement="Flow_Delivery"><di:waypoint x="860" y="200" /><di:waypoint x="900" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_DeliveryReject_di" bpmnElement="Flow_DeliveryReject"><di:waypoint x="790" y="240" /><di:waypoint x="790" y="300" /><di:waypoint x="1098" y="300" /><di:waypoint x="1098" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Payment_di" bpmnElement="Flow_Payment"><di:waypoint x="1040" y="200" /><di:waypoint x="1080" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_PaymentReject_di" bpmnElement="Flow_PaymentReject"><di:waypoint x="970" y="240" /><di:waypoint x="970" y="300" /><di:waypoint x="1098" y="300" /><di:waypoint x="1098" y="218" /></bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>
""";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "CLAIM_CHARGES_AMOUNT", table: "LND_LOANS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "CLAIM_DOWN_PAYMENT_AMOUNT", table: "LND_LOANS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_GL_TRAN_SET_ID", table: "LND_LOANS", type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "CLAIM_PAID_AT", table: "LND_LOANS", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_PAID_BY", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_PAYMENT_GL_TRAN_SET_ID", table: "LND_LOANS", type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_PAYMENT_METHOD_CODE", table: "LND_LOANS", type: "character varying(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_PAYMENT_REFERENCE", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CLAIM_STATUS_CODE", table: "LND_LOANS", type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IS_OVERRIDDEN", table: "LND_LOAN_CHARGES", type: "boolean", nullable: false, defaultValue: false);

            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  v_by     text := 'MIGRATION';
                  v_source bigint;
                  v_cat    bigint;
                  v_fee    bigint;
                  v_rule   bigint;
                  v_comp   bigint;
                  prod     record;
                BEGIN
                  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'CONSUMER_FINANCE';
                  SELECT "CHARGE_ID" INTO v_fee FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = 'CF_ADMIN_FEE';
                  IF v_source IS NULL OR v_fee IS NULL THEN
                    RAISE NOTICE 'No journal source CONSUMER_FINANCE or charge CF_ADMIN_FEE: rule CF_CHARGES not added (add it in Admin Setup).';
                  ELSE
                    INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                                         "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                                         "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT 'CF_CHARGES', 'Consumer Finance Charges', 'رسوم التمويل الاستهلاكي', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
                           'The customer''s upfront charges claimed at the Charges and claim step.', v_by, now(), 1
                     WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" x WHERE x."CATEGORY_CODE" = 'CF_CHARGES');
                    SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'CF_CHARGES';

                    FOR prod IN SELECT "PRODUCT_ID" FROM "ADM_PRODUCTS" WHERE "BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE' LOOP
                      IF NOT EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULES" x WHERE x."PRODUCT_ID" = prod."PRODUCT_ID" AND x."RULE_CODE" = 'CF_CHARGES') THEN
                        INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR",
                                                               "PRIORITY_NO", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                        VALUES (prod."PRODUCT_ID", v_source, v_cat, 'CF_CHARGES', 'Customer charges', 'رسوم العميل', 5, DATE '2026-01-01', true, v_by, now(), 1)
                        RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
                        -- the admin fee: 1% of the financed amount, earned at once, paid upfront unless Credit adds it to the schedule
                        INSERT INTO "ADM_PRODUCT_EVENT_RULE_CHARGES" ("PRODUCT_EVENT_RULE_ID", "CHARGE_ID", "SEQUENCE_NO", "CALCULATION_METHOD_CODE", "CHARGE_RATE",
                                                                      "AMOUNT_BASE_CODE", "INCOME_RECOGNITION_CODE", "SPREAD_RATE", "DAY_COUNT_BASIS", "ROUNDING_SCALE",
                                                                      "CHARGE_TIMING_CODE", "DUE_DAYS", "TAX_RATE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE",
                                                                      "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                        VALUES (v_rule, v_fee, 10, 'PERCENTAGE', 1.0, 'FINANCED_AMOUNT', 'IMMEDIATE', 0, 365, 2, 'PAID_UPFRONT', 0, 0, DATE '2026-01-01', true, v_by, now(), 1)
                        RETURNING "RULE_CHARGE_ID" INTO v_comp;
                        -- its GL lines (Component GL Entries): claimed, not earned until the claim is paid
                        INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                                       "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                        SELECT v_rule, v_comp, 1, gl.no, gl.dc, 'CHARGE_AMOUNT', 'ACCOUNT',
                               (SELECT ga."ACCOUNT_ID" FROM "GL_ACCOUNTS" ga WHERE ga."ACCOUNT_CODE" = gl.acc AND ga."IS_ACTIVE" ORDER BY ga."ACCOUNT_ID" LIMIT 1),
                               gl.descr, true, v_by, now(), 1
                          FROM (VALUES (10, 'D', '120802', 'Upfront charge claimed'), (20, 'C', '120803', 'Upfront charge not earned yet')) AS gl(no, dc, acc, descr)
                         WHERE EXISTS (SELECT 1 FROM "GL_ACCOUNTS" ga WHERE ga."ACCOUNT_CODE" = gl.acc AND ga."IS_ACTIVE");
                      END IF;
                    END LOOP;
                  END IF;

                  -- the admin fee is no longer on CF_DELIVERY: a charge added to the schedule is part of the principal there
                  DELETE FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" gl
                   USING "ADM_PRODUCT_EVENT_RULE_CHARGES" rc, "ADM_PRODUCT_EVENT_RULES" er, "ADM_PRODUCTS" pr
                   WHERE gl."RULE_CHARGE_ID" = rc."RULE_CHARGE_ID" AND rc."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID" AND pr."PRODUCT_ID" = er."PRODUCT_ID"
                     AND er."RULE_CODE" = 'CF_DELIVERY' AND pr."BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE';
                  DELETE FROM "ADM_PRODUCT_EVENT_RULE_CHARGES" rc
                   USING "ADM_PRODUCT_EVENT_RULES" er, "ADM_PRODUCTS" pr
                   WHERE rc."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID" AND pr."PRODUCT_ID" = er."PRODUCT_ID"
                     AND er."RULE_CODE" = 'CF_DELIVERY' AND pr."BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE';

                  -- CF_CLAIM is replaced by CF_CHARGES, and New Finance no longer reads a rule
                  DELETE FROM "ADM_PRODUCT_ACTIONS" WHERE "ACTION_CODE" = 'CF_NEW_FINANCE';
                  DELETE FROM "ADM_PRODUCT_ACTIONS" a
                   USING "ADM_PRODUCT_EVENT_RULES" er
                   WHERE a."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID" AND er."RULE_CODE" = 'CF_CLAIM';
                  DELETE FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" gl
                   USING "ADM_PRODUCT_EVENT_RULES" er
                   WHERE gl."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID" AND er."RULE_CODE" = 'CF_CLAIM';
                  DELETE FROM "ADM_PRODUCT_EVENT_RULE_CHARGES" rc
                   USING "ADM_PRODUCT_EVENT_RULES" er
                   WHERE rc."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID" AND er."RULE_CODE" = 'CF_CLAIM';
                  DELETE FROM "ADM_PRODUCT_EVENT_RULES" WHERE "RULE_CODE" = 'CF_CLAIM';
                END $$;
                """);

            // the six steps: new finances start on this version (WfeVersions keeps a new one); running finances stay on theirs
            migrationBuilder.Sql($"""
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = '{FinanceBpmn.Replace("'", "''", StringComparison.Ordinal)}',
                       "DESCRIPTION" = 'A consumer finance from a supplier: credit approval (charges upfront or added to the schedule), charges and claim, schedule and contract, delivery and customer receipt, supplier payment. No step is done by the person who entered it.',
                       "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                 WHERE "WORKFLOW_CODE" = 'CF_FINANCE';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var c in new[] { "CLAIM_CHARGES_AMOUNT", "CLAIM_DOWN_PAYMENT_AMOUNT", "CLAIM_GL_TRAN_SET_ID", "CLAIM_PAID_AT", "CLAIM_PAID_BY",
                         "CLAIM_PAYMENT_GL_TRAN_SET_ID", "CLAIM_PAYMENT_METHOD_CODE", "CLAIM_PAYMENT_REFERENCE", "CLAIM_STATUS_CODE" })
                migrationBuilder.DropColumn(name: c, table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "IS_OVERRIDDEN", table: "LND_LOAN_CHARGES");
        }
    }
}
