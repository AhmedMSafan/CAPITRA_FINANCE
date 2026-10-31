using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// The factoring workflow, copied from FINOVA_FINANCEING (WFE_WORKFLOW_DEFINITIONS DGRM_ID 1, FACTORING_LIMIT_APPROVAL) as
    /// it is, except the Legal Review route parameter "agreementId " lost its trailing space, and Committee Approval got
    /// ACCEPT / REJECT outcomes (REJECT ends the workflow, as Ahmed confirmed). The steps open the factoring
    /// pages by the same route names as in FINOVA_FINANCEING.
    /// </summary>
    [DbContext(typeof(CapitraDbContext))]
    [Migration("20261028230000_SeedFactoringWorkflow")]
    public partial class SeedFactoringWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('FACTORING_LIMIT_APPROVAL', 'Factoring Limit Approval', '1', 'released', '{FactoringBpmn.Replace("'", "''")}',
                        'Factoring application: operation received, sales review, investigation, credit analysis, risk review, committee approval, client acceptance, contract generation, legal review and limit creation.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'FACTORING_LIMIT_APPROVAL';""");
        }

        public const string FactoringBpmn = """
            <?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_FINOVA" targetNamespace="http://finova/workflow">
              <bpmn:process id="Process_FINOVA" name="FINOVA Workflow Engine" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_name&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_number&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true}]">
                <bpmn:startEvent id="StartEvent_1" name="Start" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:outgoing>Flow_1</bpmn:outgoing>
                </bpmn:startEvent>
                <bpmn:userTask id="Task_1" name="Operation Received" finova:stepCode="OPERATION_RECEIVED" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-agreement-assign-sales" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="ACCEPT,APPROVE,REJECT,RETURN,COMPLETE,CLAIM">
                  <bpmn:incoming>Flow_1</bpmn:incoming>
                  <bpmn:outgoing>Flow_2</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:incoming>Flow_1n70sc6</bpmn:incoming>
                  <bpmn:incoming>Flow_16r22h2</bpmn:incoming>
                  <bpmn:incoming>Flow_0dfqa7w</bpmn:incoming>
                  <bpmn:incoming>Flow_1bcnv9y</bpmn:incoming>
                </bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_1" name="" sourceRef="StartEvent_1" targetRef="Task_1" finova:outcome="" />
                <bpmn:sequenceFlow id="Flow_2" name="" sourceRef="Task_1" targetRef="Activity_0cefpuo" finova:outcome="" />
                <bpmn:task id="Activity_0cefpuo" name="Sales Review" finova:stepCode="SALES_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-agreement-sales-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
                  <bpmn:incoming>Flow_2</bpmn:incoming>
                  <bpmn:outgoing>Flow_1gknbdy</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_1gknbdy" name="" sourceRef="Activity_0cefpuo" targetRef="Activity_0wa22in" finova:outcome="" />
                <bpmn:task id="Activity_0wa22in" name="Investigation" finova:stepCode="INVESTIGATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="factoring-inquiry-field-report" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,ACCEPT">
                  <bpmn:incoming>Flow_1gknbdy</bpmn:incoming>
                  <bpmn:outgoing>Flow_01poi2g</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_1n70sc6" name="REJECT" sourceRef="Activity_0btcy1n" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:task id="Activity_0punm0q" name="Credit Analysis" finova:stepCode="CREDIT_ANALYSIS" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_APPROVER" finova:route="factoring-credit-analysis" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
                  <bpmn:incoming>Flow_01poi2g</bpmn:incoming>
                  <bpmn:outgoing>Flow_0vi5ko2</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_01poi2g" name="" sourceRef="Activity_0wa22in" targetRef="Activity_0punm0q" finova:outcome="" />
                <bpmn:task id="Activity_0btcy1n" name="Risk Review" finova:stepCode="RISK_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="RISK_OFFICER" finova:route="factoring-risk-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
                  <bpmn:incoming>Flow_0vi5ko2</bpmn:incoming>
                  <bpmn:outgoing>Flow_1n70sc6</bpmn:outgoing>
                  <bpmn:outgoing>Flow_0z38dkw</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_0vi5ko2" name="" sourceRef="Activity_0punm0q" targetRef="Activity_0btcy1n" finova:outcome="" />
                <bpmn:task id="Activity_0pogexl" name="Committe Approval" finova:stepCode="COMMITTEE_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="COMMITTEE_SECRETARY" finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
                  <bpmn:incoming>Flow_0z38dkw</bpmn:incoming>
                  <bpmn:incoming>Flow_0db6oid</bpmn:incoming>
                  <bpmn:outgoing>Flow_0qv4imo</bpmn:outgoing>
                  <bpmn:outgoing>Flow_1bcnv9y</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_0z38dkw" name="ACCEPT" sourceRef="Activity_0btcy1n" targetRef="Activity_0pogexl" finova:outcome="ACCEPT" />
                <bpmn:task id="Activity_10lu0rl" name="Client Acceptance" finova:stepCode="CLIENT_ACCEPTANCE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-contract-request" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
                  <bpmn:incoming>Flow_0qv4imo</bpmn:incoming>
                  <bpmn:outgoing>Flow_0db6oid</bpmn:outgoing>
                  <bpmn:outgoing>Flow_0igy339</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_0qv4imo" name="ACCEPT" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_0db6oid" name="REJECT" sourceRef="Activity_10lu0rl" targetRef="Activity_0pogexl" finova:outcome="REJECT" />
                <bpmn:task id="Activity_1ciisf3" name="Generate Contract" finova:stepCode="CONTRACT_GENERATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-contract-generation" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,APPROVE">
                  <bpmn:incoming>Flow_0igy339</bpmn:incoming>
                  <bpmn:outgoing>Flow_0qybf5i</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_0igy339" name="ACCEPT" sourceRef="Activity_10lu0rl" targetRef="Activity_1ciisf3" finova:outcome="ACCEPT" />
                <bpmn:task id="Activity_0nb9j68" name="Legal Review and Limit Creation" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="factoring-legal-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
                  <bpmn:incoming>Flow_0qybf5i</bpmn:incoming>
                  <bpmn:outgoing>Flow_16r22h2</bpmn:outgoing>
                  <bpmn:outgoing>Flow_0dfqa7w</bpmn:outgoing>
                </bpmn:task>
                <bpmn:sequenceFlow id="Flow_0qybf5i" name="" sourceRef="Activity_1ciisf3" targetRef="Activity_0nb9j68" finova:outcome="" />
                <bpmn:sequenceFlow id="Flow_16r22h2" name="REJECT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_0dfqa7w" name="ACCEPT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" />
                <bpmn:sequenceFlow id="Flow_1bcnv9y" name="REJECT" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" finova:outcome="REJECT" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_FINOVA">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
                    <dc:Bounds x="1446" y="242" width="36" height="36" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1452" y="278" width="24" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_1_di" bpmnElement="Task_1">
                    <dc:Bounds x="1544" y="220" width="120" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
                    <dc:Bounds x="2066" y="466" width="36" height="36" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="2112" y="477" width="20" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0cefpuo_di" bpmnElement="Activity_0cefpuo">
                    <dc:Bounds x="1734" y="220" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0wa22in_di" bpmnElement="Activity_0wa22in">
                    <dc:Bounds x="1914" y="220" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0punm0q_di" bpmnElement="Activity_0punm0q">
                    <dc:Bounds x="2060" y="220" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0btcy1n_di" bpmnElement="Activity_0btcy1n">
                    <dc:Bounds x="2044" y="343" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0pogexl_di" bpmnElement="Activity_0pogexl">
                    <dc:Bounds x="1870" y="343" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_10lu0rl_di" bpmnElement="Activity_10lu0rl">
                    <dc:Bounds x="1704" y="343" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_1ciisf3_di" bpmnElement="Activity_1ciisf3">
                    <dc:Bounds x="1544" y="343" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Activity_0nb9j68_di" bpmnElement="Activity_0nb9j68">
                    <dc:Bounds x="1544" y="540" width="100" height="80" />
                    <bpmndi:BPMNLabel />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1">
                    <di:waypoint x="1482" y="260" />
                    <di:waypoint x="1544" y="260" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_2_di" bpmnElement="Flow_2">
                    <di:waypoint x="1664" y="260" />
                    <di:waypoint x="1734" y="260" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_1gknbdy_di" bpmnElement="Flow_1gknbdy">
                    <di:waypoint x="1834" y="260" />
                    <di:waypoint x="1914" y="260" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_1n70sc6_di" bpmnElement="Flow_1n70sc6">
                    <di:waypoint x="2084" y="423" />
                    <di:waypoint x="2084" y="466" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="2078" y="440" width="43" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_01poi2g_di" bpmnElement="Flow_01poi2g">
                    <di:waypoint x="2014" y="260" />
                    <di:waypoint x="2060" y="260" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0vi5ko2_di" bpmnElement="Flow_0vi5ko2">
                    <di:waypoint x="2160" y="260" />
                    <di:waypoint x="2214" y="260" />
                    <di:waypoint x="2214" y="383" />
                    <di:waypoint x="2144" y="383" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0z38dkw_di" bpmnElement="Flow_0z38dkw">
                    <di:waypoint x="2044" y="383" />
                    <di:waypoint x="1970" y="383" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1985" y="365" width="45" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0qv4imo_di" bpmnElement="Flow_0qv4imo">
                    <di:waypoint x="1870" y="383" />
                    <di:waypoint x="1837" y="383" />
                    <di:waypoint x="1837" y="363" />
                    <di:waypoint x="1804" y="363" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0db6oid_di" bpmnElement="Flow_0db6oid">
                    <di:waypoint x="1804" y="403" />
                    <di:waypoint x="1837" y="403" />
                    <di:waypoint x="1837" y="380" />
                    <di:waypoint x="1870" y="380" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1831" y="465" width="43" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0igy339_di" bpmnElement="Flow_0igy339">
                    <di:waypoint x="1704" y="383" />
                    <di:waypoint x="1644" y="383" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1653" y="365" width="45" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0qybf5i_di" bpmnElement="Flow_0qybf5i">
                    <di:waypoint x="1594" y="423" />
                    <di:waypoint x="1594" y="540" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_16r22h2_di" bpmnElement="Flow_16r22h2">
                    <di:waypoint x="1644" y="560" />
                    <di:waypoint x="2084" y="560" />
                    <di:waypoint x="2084" y="502" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1868" y="537" width="43" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_0dfqa7w_di" bpmnElement="Flow_0dfqa7w">
                    <di:waypoint x="1644" y="580" />
                    <di:waypoint x="1699" y="580" />
                    <di:waypoint x="1699" y="620" />
                    <di:waypoint x="2094" y="620" />
                    <di:waypoint x="2094" y="499" />
                    <bpmndi:BPMNLabel>
                      <dc:Bounds x="1875" y="613" width="45" height="14" />
                    </bpmndi:BPMNLabel>
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_1bcnv9y_di" bpmnElement="Flow_1bcnv9y">
                    <di:waypoint x="1920" y="423" />
                    <di:waypoint x="1920" y="484" />
                    <di:waypoint x="2066" y="484" />
                  </bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            <!-- FINOVA_VIEWBOX:{"x":1138.0359955005624,"y":189.458942632171,"width":1096.7379077615299,"height":511.81102362204723} -->
            </bpmn:definitions>
            """;
    }
}
