using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CF_FINANCE (Ahmed, 26 Nov 2026): a consumer finance from a supplier goes through Credit approval (CREDIT_OFFICER), Agreement and
    /// upfront payment and Delivery and customer receipt (OPERATIONS_OFFICER), and Supplier payment (TREASURY_OFFICER). LND_LOANS keeps
    /// its run and who entered it; the existing roles get the lending permissions they need.
    /// </summary>
    public partial class CfFinanceWorkflow : Migration
    {
        private static readonly (string Role, string[] Permissions)[] Roles =
        [
            ("SALES_OFFICER", ["LND.LOAN.VIEW", "LND.LOAN.EDIT", "CRM.VIEW"]),
            ("CREDIT_OFFICER", ["LND.LOAN.VIEW", "CRM.VIEW"]),
            ("OPERATIONS_OFFICER", ["LND.LOAN.VIEW", "CRM.VIEW"]),
            ("TREASURY_OFFICER", ["LND.LOAN.VIEW", "CRM.VIEW"]),
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "ENTERED_BY_PERSON", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<long>(name: "WORKFLOW_INSTANCE_ID", table: "LND_LOANS", type: "bigint", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_LND_LOANS_WORKFLOW_INSTANCE_ID", table: "LND_LOANS", column: "WORKFLOW_INSTANCE_ID");

            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('CF_FINANCE', 'Consumer Finance', '1', 'released', '{FinanceBpmn.Replace("'", "''", StringComparison.Ordinal)}',
                        'A consumer finance from a supplier: credit approval, agreement and upfront payment, delivery and customer receipt, supplier payment. No step is done by the person who entered it.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);
            foreach (var (role, permissions) in Roles)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "SEC_ROLES" ("ROLE_ID", "ROLE_NAME", "NORMALIZED_ROLE_NAME", "CONCURRENCY_STAMP", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT gen_random_uuid()::text, '{role}', '{role}', gen_random_uuid()::text, 'SYSTEM', now(), 1
                    WHERE NOT EXISTS (SELECT 1 FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" = '{role}');
                    INSERT INTO "SEC_ROLE_CLAIMS" ("ROLE_ID", "CLAIM_TYPE", "CLAIM_VALUE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT r."ROLE_ID", 'permission', p.code, 'SYSTEM', now(), 1
                    FROM "SEC_ROLES" r CROSS JOIN unnest(ARRAY[{string.Join(", ", permissions.Select(p => $"'{p}'"))}]) AS p(code)
                    WHERE r."NORMALIZED_ROLE_NAME" = '{role}'
                      AND NOT EXISTS (SELECT 1 FROM "SEC_ROLE_CLAIMS" c WHERE c."ROLE_ID" = r."ROLE_ID" AND c."CLAIM_TYPE" = 'permission' AND c."CLAIM_VALUE" = p.code);
                    """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'CF_FINANCE';""");
            migrationBuilder.DropIndex(name: "IX_LND_LOANS_WORKFLOW_INSTANCE_ID", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "ENTERED_BY_PERSON", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "WORKFLOW_INSTANCE_ID", table: "LND_LOANS");
        }

        /// <summary>CF_FINANCE: four steps in a row; REJECT at any of them ends the run (the finance is rejected).</summary>
        public const string FinanceBpmn = """
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
                <bpmn:userTask id="Task_Agreement" name="Agreement and upfront payment" finova:stepCode="CF_AGREEMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Credit</bpmn:incoming>
                  <bpmn:outgoing>Flow_Agreement</bpmn:outgoing>
                  <bpmn:outgoing>Flow_AgreementReject</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Delivery" name="Delivery and customer receipt" finova:stepCode="CF_DELIVERY" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Agreement</bpmn:incoming>
                  <bpmn:outgoing>Flow_Delivery</bpmn:outgoing>
                  <bpmn:outgoing>Flow_DeliveryReject</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Payment" name="Supplier payment" finova:stepCode="CF_SUPPLIER_PAYMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Delivery</bpmn:incoming>
                  <bpmn:outgoing>Flow_Payment</bpmn:outgoing>
                  <bpmn:outgoing>Flow_PaymentReject</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:incoming>Flow_Payment</bpmn:incoming>
                  <bpmn:incoming>Flow_CreditReject</bpmn:incoming>
                  <bpmn:incoming>Flow_AgreementReject</bpmn:incoming>
                  <bpmn:incoming>Flow_DeliveryReject</bpmn:incoming>
                  <bpmn:incoming>Flow_PaymentReject</bpmn:incoming>
                </bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_Start" name="" sourceRef="StartEvent_1" targetRef="Task_Credit" finova:outcome="" />
                <bpmn:sequenceFlow id="Flow_Credit" name="ACCEPT" sourceRef="Task_Credit" targetRef="Task_Agreement" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Agreement" name="ACCEPT" sourceRef="Task_Agreement" targetRef="Task_Delivery" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Delivery" name="ACCEPT" sourceRef="Task_Delivery" targetRef="Task_Payment" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Payment" name="ACCEPT" sourceRef="Task_Payment" targetRef="EndEvent_1" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_CreditReject" name="REJECT" sourceRef="Task_Credit" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_AgreementReject" name="REJECT" sourceRef="Task_Agreement" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_DeliveryReject" name="REJECT" sourceRef="Task_Delivery" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_PaymentReject" name="REJECT" sourceRef="Task_Payment" targetRef="EndEvent_1" finova:outcome="REJECT" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_CF_FINANCE">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="100" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Credit_di" bpmnElement="Task_Credit"><dc:Bounds x="180" y="160" width="140" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Agreement_di" bpmnElement="Task_Agreement"><dc:Bounds x="360" y="160" width="140" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Delivery_di" bpmnElement="Task_Delivery"><dc:Bounds x="540" y="160" width="140" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Payment_di" bpmnElement="Task_Payment"><dc:Bounds x="720" y="160" width="140" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="920" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="136" y="200" /><di:waypoint x="180" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Credit_di" bpmnElement="Flow_Credit"><di:waypoint x="320" y="200" /><di:waypoint x="360" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Agreement_di" bpmnElement="Flow_Agreement"><di:waypoint x="500" y="200" /><di:waypoint x="540" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Delivery_di" bpmnElement="Flow_Delivery"><di:waypoint x="680" y="200" /><di:waypoint x="720" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Payment_di" bpmnElement="Flow_Payment"><di:waypoint x="860" y="200" /><di:waypoint x="920" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_CreditReject_di" bpmnElement="Flow_CreditReject"><di:waypoint x="250" y="240" /><di:waypoint x="250" y="300" /><di:waypoint x="938" y="300" /><di:waypoint x="938" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_AgreementReject_di" bpmnElement="Flow_AgreementReject"><di:waypoint x="430" y="240" /><di:waypoint x="430" y="300" /><di:waypoint x="938" y="300" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_DeliveryReject_di" bpmnElement="Flow_DeliveryReject"><di:waypoint x="610" y="240" /><di:waypoint x="610" y="300" /><di:waypoint x="938" y="300" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_PaymentReject_di" bpmnElement="Flow_PaymentReject"><di:waypoint x="790" y="240" /><di:waypoint x="790" y="300" /><di:waypoint x="938" y="300" /></bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            </bpmn:definitions>
            """;
    }
}
