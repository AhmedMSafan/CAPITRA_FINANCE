using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CF_FINANCE in five steps (Ahmed, 27 Nov 2026): Credit approval creates the upfront claim (rule CF_CHARGES, named by the Credit step)
    /// and the Charges and claim step is removed for new finances; Schedule and contract waits until the claim is paid. The claim is paid
    /// like a factoring claim, in one or several payments: LND_CF_CLAIM_PAYMENTS, and CLAIM_PAID_AMOUNT on LND_LOANS (nullable). Claims
    /// already paid get their payment copied; running finances keep the workflow version they started on.
    /// </summary>
    public partial class CfClaimAtCredit : Migration
    {
        private const string FinanceBpmn = """
<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_CFF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_CF_FINANCE" name="Consumer Finance" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_ref&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;module_code&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false}]">
    <bpmn:startEvent id="StartEvent_1" name="Submitted" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:outgoing>Flow_Start</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id="Task_Credit" name="Credit approval" finova:stepCode="CF_CREDIT" finova:ruleCode="CF_CHARGES" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Start</bpmn:incoming>
      <bpmn:outgoing>Flow_Credit</bpmn:outgoing>
      <bpmn:outgoing>Flow_CreditReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_Schedule" name="Schedule and contract" finova:stepCode="CF_SCHEDULE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="lnd-loan" finova:routeParameters="[{&#34;name&#34;:&#34;loanId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Credit</bpmn:incoming>
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
      <bpmn:incoming>Flow_ScheduleReject</bpmn:incoming>
      <bpmn:incoming>Flow_DeliveryReject</bpmn:incoming>
      <bpmn:incoming>Flow_Payment</bpmn:incoming>
      <bpmn:incoming>Flow_PaymentReject</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" name="" sourceRef="StartEvent_1" targetRef="Task_Credit" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_Credit" name="ACCEPT" sourceRef="Task_Credit" targetRef="Task_Schedule" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_CreditReject" name="REJECT" sourceRef="Task_Credit" targetRef="EndEvent_1" finova:outcome="REJECT" />
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
      <bpmndi:BPMNShape id="Task_Schedule_di" bpmnElement="Task_Schedule"><dc:Bounds x="360" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Delivery_di" bpmnElement="Task_Delivery"><dc:Bounds x="540" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Payment_di" bpmnElement="Task_Payment"><dc:Bounds x="720" y="160" width="140" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="900" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="136" y="200" /><di:waypoint x="180" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Credit_di" bpmnElement="Flow_Credit"><di:waypoint x="320" y="200" /><di:waypoint x="360" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_CreditReject_di" bpmnElement="Flow_CreditReject"><di:waypoint x="250" y="240" /><di:waypoint x="250" y="300" /><di:waypoint x="918" y="300" /><di:waypoint x="918" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Schedule_di" bpmnElement="Flow_Schedule"><di:waypoint x="500" y="200" /><di:waypoint x="540" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_ScheduleReject_di" bpmnElement="Flow_ScheduleReject"><di:waypoint x="430" y="240" /><di:waypoint x="430" y="300" /><di:waypoint x="918" y="300" /><di:waypoint x="918" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Delivery_di" bpmnElement="Flow_Delivery"><di:waypoint x="680" y="200" /><di:waypoint x="720" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_DeliveryReject_di" bpmnElement="Flow_DeliveryReject"><di:waypoint x="610" y="240" /><di:waypoint x="610" y="300" /><di:waypoint x="918" y="300" /><di:waypoint x="918" y="218" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Payment_di" bpmnElement="Flow_Payment"><di:waypoint x="860" y="200" /><di:waypoint x="900" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_PaymentReject_di" bpmnElement="Flow_PaymentReject"><di:waypoint x="790" y="240" /><di:waypoint x="790" y="300" /><di:waypoint x="918" y="300" /><di:waypoint x="918" y="218" /></bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>
""";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_CF_CLAIM_PAYMENTS",
                columns: table => new
                {
                    CLAIM_PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DOWN_PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    PAYMENT_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PAYMENT_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_CF_CLAIM_PAYMENTS", x => x.CLAIM_PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_LND_CF_CLAIM_PAYMENTS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_CF_CLAIM_PAYMENTS_LOAN_ID",
                table: "LND_CF_CLAIM_PAYMENTS",
                column: "LOAN_ID");

            migrationBuilder.AddColumn<decimal>(name: "CLAIM_PAID_AMOUNT", table: "LND_LOANS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);

            // claims paid before: what was paid, and that one payment kept as the claim's payment (bank account not known then)
            migrationBuilder.Sql("""
                UPDATE "LND_LOANS" SET "CLAIM_PAID_AMOUNT" = COALESCE("CLAIM_CHARGES_AMOUNT", 0) + COALESCE("CLAIM_DOWN_PAYMENT_AMOUNT", 0)
                 WHERE "CLAIM_STATUS_CODE" = 'PAID';
                INSERT INTO "LND_CF_CLAIM_PAYMENTS" ("LOAN_ID", "PAYMENT_DATE", "PAYMENT_AMOUNT", "CHARGES_AMOUNT", "DOWN_PAYMENT_AMOUNT", "BANK_ACCOUNT_ID",
                                                    "PAYMENT_METHOD_CODE", "PAYMENT_REFERENCE", "GL_TRAN_SET_ID", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT "LOAN_ID", (COALESCE("CLAIM_PAID_AT", now()) AT TIME ZONE 'Africa/Cairo')::date,
                       COALESCE("CLAIM_CHARGES_AMOUNT", 0) + COALESCE("CLAIM_DOWN_PAYMENT_AMOUNT", 0), COALESCE("CLAIM_CHARGES_AMOUNT", 0),
                       COALESCE("CLAIM_DOWN_PAYMENT_AMOUNT", 0), NULL, COALESCE("CLAIM_PAYMENT_METHOD_CODE", 'CASH'), "CLAIM_PAYMENT_REFERENCE",
                       "CLAIM_PAYMENT_GL_TRAN_SET_ID", COALESCE("CLAIM_PAID_BY", 'MIGRATION'), COALESCE("CLAIM_PAID_AT", now()), 1
                  FROM "LND_LOANS"
                 WHERE "CLAIM_STATUS_CODE" = 'PAID' AND COALESCE("CLAIM_CHARGES_AMOUNT", 0) + COALESCE("CLAIM_DOWN_PAYMENT_AMOUNT", 0) > 0;
                """);

            // new finances: five steps; running finances keep the version they started on
            migrationBuilder.Sql($"""
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = '{FinanceBpmn.Replace("'", "''", StringComparison.Ordinal)}',
                       "DESCRIPTION" = 'A consumer finance from a supplier: credit approval (charges upfront or added to the schedule; approving creates the upfront claim), schedule and contract once the claim is paid, delivery and customer receipt, supplier payment. No step is done by the person who entered it.',
                       "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                 WHERE "WORKFLOW_CODE" = 'CF_FINANCE';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CLAIM_PAID_AMOUNT", table: "LND_LOANS");
            migrationBuilder.DropTable(name: "LND_CF_CLAIM_PAYMENTS");
        }
    }
}
