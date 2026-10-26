using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPurchasedPortfolioWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // App 105 portfolio workflow (APEX WFE definition PURCHASED_PORTFOLIO_WF), built by tools/oracle-port/wfe_bpmn.py.
            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('PURCHASED_PORTFOLIO_WF', 'Purchased Portfolio Workflow', '1', 'released', '{PurchasedPortfolioBpmn.Replace("'", "''")}',
                        'Mortgage portfolio purchase: data load, AML, inquiry, legal, valuation, coverage, cheque coverage, financial and board review, disbursement.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'PURCHASED_PORTFOLIO_WF';""");
        }

        private const string PurchasedPortfolioBpmn = """
            <?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_PURCHASED_PORTFOLIO_WF" targetNamespace="http://finova/workflow">
              <bpmn:process id="Process_PURCHASED_PORTFOLIO_WF" name="Purchased Portfolio Workflow" isExecutable="true" finova:variables='["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]'>
                <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
                <bpmn:userTask id="Task_LOAD_DATA" name="Loading Excel Data" finova:stepCode="LOAD_DATA" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-208" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_LOAD_DATA_NEXT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_AML_REVIEW" name="AML Review" finova:stepCode="AML_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-209" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_LOAD_DATA_NEXT</bpmn:incoming><bpmn:outgoing>Flow_AML_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_AML_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_PORTFOLIO_INQUIRY" name="Portfolio Inquiry" finova:stepCode="PORTFOLIO_INQUIRY" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="mtg-210" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_AML_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_LEGAL_REVIEW" name="Legal Review" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="mtg-211" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_LEGAL_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_LEGAL_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_VALUATION_REVIEW" name="Portfolio Valuation" finova:stepCode="VALUATION_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-401" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_LEGAL_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_VALUATION_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_COVERAGE_REVIEW" name="Coverage Review" finova:stepCode="COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-212" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_VALUATION_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_COVERAGE_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_CHEQUE_COVERAGE_REVIEW" name="Cheque Coverage Review" finova:stepCode="CHEQUE_COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-213" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_COVERAGE_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_FINANCIAL_REVIEW" name="Financial Review" finova:stepCode="FINANCIAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-214" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_FINANCIAL_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_BOARD_REVIEW" name="Board Review" finova:stepCode="BOARD_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-215" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_FINANCIAL_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_BOARD_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_BOARD_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_DISBURSEMENT" name="Disbursement" finova:stepCode="DISBURSEMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-217" finova:routeParameters='[{"name": "portfolioId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_BOARD_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_DISBURSEMENT_NEXT</bpmn:outgoing></bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="End"><bpmn:incoming>Flow_AML_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:incoming><bpmn:incoming>Flow_LEGAL_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_BOARD_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_DISBURSEMENT_NEXT</bpmn:incoming></bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_LOAD_DATA" />
                <bpmn:sequenceFlow id="Flow_LOAD_DATA_NEXT" sourceRef="Task_LOAD_DATA" targetRef="Task_AML_REVIEW" />
                <bpmn:sequenceFlow id="Flow_AML_REVIEW_APPROVE" sourceRef="Task_AML_REVIEW" targetRef="Task_PORTFOLIO_INQUIRY" name="APPROVE" finova:outcome="APPROVE" />
                <bpmn:sequenceFlow id="Flow_AML_REVIEW_REJECT" sourceRef="Task_AML_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_APPROVE" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="Task_LEGAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
                <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_REJECT" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_APPROVE" sourceRef="Task_LEGAL_REVIEW" targetRef="Task_VALUATION_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
                <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_REJECT" sourceRef="Task_LEGAL_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_VALUATION_REVIEW_NEXT" sourceRef="Task_VALUATION_REVIEW" targetRef="Task_COVERAGE_REVIEW" />
                <bpmn:sequenceFlow id="Flow_COVERAGE_REVIEW_NEXT" sourceRef="Task_COVERAGE_REVIEW" targetRef="Task_CHEQUE_COVERAGE_REVIEW" />
                <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="Task_FINANCIAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
                <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_FINANCIAL_REVIEW_NEXT" sourceRef="Task_FINANCIAL_REVIEW" targetRef="Task_BOARD_REVIEW" />
                <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_APPROVE" sourceRef="Task_BOARD_REVIEW" targetRef="Task_DISBURSEMENT" name="APPROVE" finova:outcome="APPROVE" />
                <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_REJECT" sourceRef="Task_BOARD_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_DISBURSEMENT_NEXT" sourceRef="Task_DISBURSEMENT" targetRef="EndEvent_1" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_PURCHASED_PORTFOLIO_WF">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="120" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_LOAD_DATA_di" bpmnElement="Task_LOAD_DATA"><dc:Bounds x="200" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_AML_REVIEW_di" bpmnElement="Task_AML_REVIEW"><dc:Bounds x="380" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_PORTFOLIO_INQUIRY_di" bpmnElement="Task_PORTFOLIO_INQUIRY"><dc:Bounds x="560" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_LEGAL_REVIEW_di" bpmnElement="Task_LEGAL_REVIEW"><dc:Bounds x="740" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_VALUATION_REVIEW_di" bpmnElement="Task_VALUATION_REVIEW"><dc:Bounds x="920" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_COVERAGE_REVIEW_di" bpmnElement="Task_COVERAGE_REVIEW"><dc:Bounds x="1100" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_CHEQUE_COVERAGE_REVIEW_di" bpmnElement="Task_CHEQUE_COVERAGE_REVIEW"><dc:Bounds x="1280" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_FINANCIAL_REVIEW_di" bpmnElement="Task_FINANCIAL_REVIEW"><dc:Bounds x="1460" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_BOARD_REVIEW_di" bpmnElement="Task_BOARD_REVIEW"><dc:Bounds x="1640" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_DISBURSEMENT_di" bpmnElement="Task_DISBURSEMENT"><dc:Bounds x="1820" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="2000" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="156" y="200" /><di:waypoint x="200" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_LOAD_DATA_NEXT_di" bpmnElement="Flow_LOAD_DATA_NEXT"><di:waypoint x="330" y="200" /><di:waypoint x="380" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_AML_REVIEW_APPROVE_di" bpmnElement="Flow_AML_REVIEW_APPROVE"><di:waypoint x="510" y="200" /><di:waypoint x="560" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_AML_REVIEW_REJECT_di" bpmnElement="Flow_AML_REVIEW_REJECT"><di:waypoint x="445" y="240" /><di:waypoint x="445" y="310" /><di:waypoint x="2018" y="310" /><di:waypoint x="2018" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_APPROVE_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_APPROVE"><di:waypoint x="690" y="200" /><di:waypoint x="740" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_REJECT_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_REJECT"><di:waypoint x="625" y="240" /><di:waypoint x="625" y="310" /><di:waypoint x="2018" y="310" /><di:waypoint x="2018" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_APPROVE_di" bpmnElement="Flow_LEGAL_REVIEW_APPROVE"><di:waypoint x="870" y="200" /><di:waypoint x="920" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_REJECT_di" bpmnElement="Flow_LEGAL_REVIEW_REJECT"><di:waypoint x="805" y="240" /><di:waypoint x="805" y="310" /><di:waypoint x="2018" y="310" /><di:waypoint x="2018" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_VALUATION_REVIEW_NEXT_di" bpmnElement="Flow_VALUATION_REVIEW_NEXT"><di:waypoint x="1050" y="200" /><di:waypoint x="1100" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_COVERAGE_REVIEW_NEXT_di" bpmnElement="Flow_COVERAGE_REVIEW_NEXT"><di:waypoint x="1230" y="200" /><di:waypoint x="1280" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE"><di:waypoint x="1410" y="200" /><di:waypoint x="1460" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_REJECT"><di:waypoint x="1345" y="240" /><di:waypoint x="1345" y="310" /><di:waypoint x="2018" y="310" /><di:waypoint x="2018" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_FINANCIAL_REVIEW_NEXT_di" bpmnElement="Flow_FINANCIAL_REVIEW_NEXT"><di:waypoint x="1590" y="200" /><di:waypoint x="1640" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_APPROVE_di" bpmnElement="Flow_BOARD_REVIEW_APPROVE"><di:waypoint x="1770" y="200" /><di:waypoint x="1820" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_REJECT_di" bpmnElement="Flow_BOARD_REVIEW_REJECT"><di:waypoint x="1705" y="240" /><di:waypoint x="1705" y="310" /><di:waypoint x="2018" y="310" /><di:waypoint x="2018" y="218" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_DISBURSEMENT_NEXT_di" bpmnElement="Flow_DISBURSEMENT_NEXT"><di:waypoint x="1950" y="200" /><di:waypoint x="2000" y="200" /></bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            </bpmn:definitions>
            """;
    }
}
