using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Screening (FINOVA merged into CAPITRA): the decision approval workflow and the screening roles.
    /// <list type="bullet">
    /// <item>SCR_DECISION_APPROVAL: Review decision (role SCR_REVIEWER), then Approve and publish (role SCR_PUBLISHER). REJECT on
    /// either step returns the decision to data entry and ends the workflow; submitting again starts a new one.</item>
    /// <item>Roles: federation intake, data entry, reviewer, publisher, administrator and auditor; member company administrator and
    /// user. Each gets its SCR permissions; users are added to them in Security or, for companies, in Screening > Member companies.</item>
    /// </list>
    /// </summary>
    [DbContext(typeof(CapitraDbContext))]
    [Migration("20261119000000_ScreeningSeed")]
    public partial class ScreeningSeed : Migration
    {
        private static readonly (string Role, string Description, string[] Permissions)[] Roles =
        [
            ("SCR_INTAKE", "Screening: registers incoming letters and their scans", ["SCR.DECISION.VIEW", "SCR.LETTER.EDIT"]),
            ("SCR_DATA_ENTRY", "Screening: registers parties and enters decisions", ["SCR.DECISION.VIEW", "SCR.DECISION.EDIT"]),
            ("SCR_REVIEWER", "Screening: reviews decisions (workflow step)", ["SCR.DECISION.VIEW", "SCR.DECISION.REVIEW"]),
            ("SCR_PUBLISHER", "Screening: approves and publishes decisions to the Final List (workflow step)", ["SCR.DECISION.VIEW", "SCR.DECISION.APPROVE"]),
            ("SCR_ADMIN", "Screening: member companies and their accounts, inquiry audit", ["SCR.DECISION.VIEW", "SCR.COMPANY.ADMIN", "SCR.INQUIRY.AUDIT"]),
            ("SCR_AUDITOR", "Screening: reads decisions, the Final List and every inquiry", ["SCR.DECISION.VIEW", "SCR.INQUIRY.AUDIT"]),
            ("SCR_COMPANY_ADMIN", "Member company: screens and manages the company's user accounts", ["SCR.INQUIRY.CREATE", "SCR.COMPANY.USERS"]),
            ("SCR_COMPANY_USER", "Member company: screens people and companies against the Final List", ["SCR.INQUIRY.CREATE"]),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('SCR_DECISION_APPROVAL', 'Screening Decision Approval', '1', 'released', '{DecisionApprovalBpmn.Replace("'", "''", StringComparison.Ordinal)}',
                        'A screening decision is reviewed, then approved and published to the Federation Final List. Reviewer and approver are never the person who entered it.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);

            foreach (var (role, description, permissions) in Roles)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "SEC_ROLES" ("ROLE_ID", "ROLE_NAME", "NORMALIZED_ROLE_NAME", "CONCURRENCY_STAMP", "DESCRIPTION", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT gen_random_uuid()::text, '{role}', '{role}', gen_random_uuid()::text, '{description.Replace("'", "''", StringComparison.Ordinal)}', 'SYSTEM', now(), 1
                    WHERE NOT EXISTS (SELECT 1 FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" = '{role}');

                    INSERT INTO "SEC_ROLE_CLAIMS" ("ROLE_ID", "CLAIM_TYPE", "CLAIM_VALUE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT r."ROLE_ID", 'permission', p.code, 'SYSTEM', now(), 1
                    FROM "SEC_ROLES" r CROSS JOIN unnest(ARRAY[{string.Join(", ", permissions.Select(p => $"'{p}'"))}]) AS p(code)
                    WHERE r."NORMALIZED_ROLE_NAME" = '{role}'
                      AND NOT EXISTS (SELECT 1 FROM "SEC_ROLE_CLAIMS" c WHERE c."ROLE_ID" = r."ROLE_ID" AND c."CLAIM_TYPE" = 'permission' AND c."CLAIM_VALUE" = p.code);
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'SCR_DECISION_APPROVAL';""");
            var names = string.Join(", ", Roles.Select(r => $"'{r.Role}'"));
            migrationBuilder.Sql($"""
                DELETE FROM "SEC_ROLE_CLAIMS" WHERE "ROLE_ID" IN (SELECT "ROLE_ID" FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names}));
                DELETE FROM "SEC_USER_ROLES" WHERE "ROLE_ID" IN (SELECT "ROLE_ID" FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names}));
                DELETE FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names});
                """);
        }

        public const string DecisionApprovalBpmn = """
            <?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_SCR" targetNamespace="http://finova/workflow">
              <bpmn:process id="Process_SCR_DECISION" name="Screening Decision Approval" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_ref&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;decision_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;official_no&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;module_code&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false}]">
                <bpmn:startEvent id="StartEvent_1" name="Submitted" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:outgoing>Flow_1</bpmn:outgoing>
                </bpmn:startEvent>
                <bpmn:userTask id="Task_Review" name="Review decision" finova:stepCode="SCR_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SCR_REVIEWER" finova:route="scr-decision" finova:routeParameters="[{&#34;name&#34;:&#34;decisionId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_1</bpmn:incoming>
                  <bpmn:outgoing>Flow_Reviewed</bpmn:outgoing>
                  <bpmn:outgoing>Flow_ReviewReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Approval" name="Approve and publish" finova:stepCode="SCR_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SCR_PUBLISHER" finova:route="scr-decision" finova:routeParameters="[{&#34;name&#34;:&#34;decisionId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Reviewed</bpmn:incoming>
                  <bpmn:outgoing>Flow_Published</bpmn:outgoing>
                  <bpmn:outgoing>Flow_ApprovalReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:incoming>Flow_Published</bpmn:incoming>
                  <bpmn:incoming>Flow_ReviewReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_ApprovalReturn</bpmn:incoming>
                </bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_1" name="" sourceRef="StartEvent_1" targetRef="Task_Review" finova:outcome="" />
                <bpmn:sequenceFlow id="Flow_Reviewed" name="ACCEPT" sourceRef="Task_Review" targetRef="Task_Approval" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_ReviewReturn" name="REJECT" sourceRef="Task_Review" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_Published" name="ACCEPT" sourceRef="Task_Approval" targetRef="EndEvent_1" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_ApprovalReturn" name="REJECT" sourceRef="Task_Approval" targetRef="EndEvent_1" finova:outcome="REJECT" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_SCR_DECISION">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
                    <dc:Bounds x="152" y="182" width="36" height="36" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Review_di" bpmnElement="Task_Review">
                    <dc:Bounds x="250" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Approval_di" bpmnElement="Task_Approval">
                    <dc:Bounds x="470" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
                    <dc:Bounds x="692" y="182" width="36" height="36" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1">
                    <di:waypoint x="188" y="200" />
                    <di:waypoint x="250" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Reviewed_di" bpmnElement="Flow_Reviewed">
                    <di:waypoint x="390" y="200" />
                    <di:waypoint x="470" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ReviewReturn_di" bpmnElement="Flow_ReviewReturn">
                    <di:waypoint x="320" y="240" />
                    <di:waypoint x="320" y="300" />
                    <di:waypoint x="710" y="300" />
                    <di:waypoint x="710" y="218" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Published_di" bpmnElement="Flow_Published">
                    <di:waypoint x="610" y="200" />
                    <di:waypoint x="692" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ApprovalReturn_di" bpmnElement="Flow_ApprovalReturn">
                    <di:waypoint x="540" y="160" />
                    <di:waypoint x="540" y="110" />
                    <di:waypoint x="710" y="110" />
                    <di:waypoint x="710" y="182" />
                  </bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            </bpmn:definitions>
            """;
    }
}
