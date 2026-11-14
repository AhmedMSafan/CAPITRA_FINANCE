using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DisbursementRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FCT_INVOICE_BATCHES",
                columns: table => new
                {
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BATCH_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BATCH_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BATCH_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    SUBMISSION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SUBMISSION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SUBMISSION_METHOD_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BATCH_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BATCH_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    TOTAL_INVOICES = table.Column<long>(type: "bigint", nullable: true),
                    TOTAL_GROSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_NET_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_TAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VALIDATED_INVOICES = table.Column<long>(type: "bigint", nullable: true),
                    REJECTED_INVOICES = table.Column<long>(type: "bigint", nullable: true),
                    APPROVED_INVOICES = table.Column<long>(type: "bigint", nullable: true),
                    FUNDED_INVOICES = table.Column<long>(type: "bigint", nullable: true),
                    APPROVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    REJECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROCESSING_START_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PROCESSING_END_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PROCESSING_DURATION_SEC = table.Column<long>(type: "bigint", nullable: true),
                    ERROR_COUNT = table.Column<long>(type: "bigint", nullable: true),
                    WARNING_COUNT = table.Column<long>(type: "bigint", nullable: true),
                    UPLOAD_FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UPLOAD_FILE_SIZE = table.Column<int>(type: "integer", nullable: true),
                    UPLOAD_FILE_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPLOAD_FILE_CONTENT = table.Column<byte[]>(type: "bytea", nullable: true),
                    UPLOAD_TEMPLATE_VER = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VALIDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VALIDATION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REMARKS = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REMARKS_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_INVOICE_BATCHES", x => x.BATCH_ID);
                    table.ForeignKey(
                        name: "FK_FCT_INVOICE_BATCHES_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_INVOICE_BATCHES_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_INVOICE_BATCHES_CRM_LKP_STATUS_CODES",
                        column: x => x.STATUS_ID,
                        principalTable: "CRM_LKP_STATUS_CODES",
                        principalColumn: "STATUS_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_INVOICE_BATCHES_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_BATCH_DATE",
                table: "FCT_INVOICE_BATCHES",
                column: "BATCH_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_BATCH_TYPE",
                table: "FCT_INVOICE_BATCHES",
                column: "BATCH_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_CLIENT_ID",
                table: "FCT_INVOICE_BATCHES",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_CURRENCY_ID",
                table: "FCT_INVOICE_BATCHES",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_DISBURSEMENT_ID",
                table: "FCT_INVOICE_BATCHES",
                column: "DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_STATUS_ID",
                table: "FCT_INVOICE_BATCHES",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_SUBMISSION_AT",
                table: "FCT_INVOICE_BATCHES",
                column: "SUBMISSION_AT");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICE_BATCHES_SUBMISSION_METHOD",
                table: "FCT_INVOICE_BATCHES",
                column: "SUBMISSION_METHOD");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_INVOICE_BATCHES_BATCH_NUMBER",
                table: "FCT_INVOICE_BATCHES",
                column: "BATCH_NUMBER",
                unique: true);

            // App 150722 disbursement workflow (FINOVA WFE definition 22, DISBURSEMENT_REQUEST_WF): Initiation, Invoice Upload,
            // Treasury Disburse, then the workflow's own end (FINOVA's last transition points at another workflow's COMPLETED step).
            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('DISBURSEMENT_REQUEST_WF', 'Disbursement Request Workflow', '1', 'released', '{DisbursementRequestBpmn.Replace("'", "''")}',
                        'Customer disbursement request: initiation, invoice upload and treasury disbursement.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF';""");

            migrationBuilder.DropTable(
                name: "FCT_INVOICE_BATCHES");
        }

        private const string DisbursementRequestBpmn = """
            <?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_DISBURSEMENT_REQUEST_WF" targetNamespace="http://finova/workflow">
              <bpmn:process id="Process_DISBURSEMENT_REQUEST_WF" name="Disbursement Request Workflow" isExecutable="true" finova:variables='["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]'>
                <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
                <bpmn:userTask id="Task_INITIATION" name="Initiation" finova:stepCode="INITIATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-request" finova:routeParameters='[{"name": "disbursementId", "value": "ENTITY_ID"}]' finova:actions="COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_INVOICE_UPLOAD" name="Invoice Upload" finova:stepCode="INVOICE_UPLOAD" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-request" finova:routeParameters='[{"name": "disbursementId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:incoming><bpmn:outgoing>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:outgoing></bpmn:userTask>
                <bpmn:userTask id="Task_TREASURY_DISBURSE" name="Tresaury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="crm-disbursement-request" finova:routeParameters='[{"name": "disbursementId", "value": "ENTITY_ID"}]' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:incoming><bpmn:outgoing>Flow_TREASURY_TO_COMPLETE</bpmn:outgoing></bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="Workflow Completed"><bpmn:incoming>Flow_TREASURY_TO_COMPLETE</bpmn:incoming></bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_INITIATION" />
                <bpmn:sequenceFlow id="Flow_INIT_TO_INVOICE_UPLOAD" sourceRef="Task_INITIATION" targetRef="Task_INVOICE_UPLOAD" name="Initiation → Invoice Upload" />
                <bpmn:sequenceFlow id="Flow_FCT_OPERATIONS_TO_TREASUARY" sourceRef="Task_INVOICE_UPLOAD" targetRef="Task_TREASURY_DISBURSE" name="SALES TO TREASUARY" />
                <bpmn:sequenceFlow id="Flow_TREASURY_TO_COMPLETE" sourceRef="Task_TREASURY_DISBURSE" targetRef="EndEvent_1" name="TREASURY TO COMPLETE" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_DISBURSEMENT_REQUEST_WF">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="120" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_INITIATION_di" bpmnElement="Task_INITIATION"><dc:Bounds x="200" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_INVOICE_UPLOAD_di" bpmnElement="Task_INVOICE_UPLOAD"><dc:Bounds x="380" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_TREASURY_DISBURSE_di" bpmnElement="Task_TREASURY_DISBURSE"><dc:Bounds x="560" y="160" width="130" height="80" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="740" y="182" width="36" height="36" /></bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="156" y="200" /><di:waypoint x="200" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_INIT_TO_INVOICE_UPLOAD_di" bpmnElement="Flow_INIT_TO_INVOICE_UPLOAD"><di:waypoint x="330" y="200" /><di:waypoint x="380" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_FCT_OPERATIONS_TO_TREASUARY_di" bpmnElement="Flow_FCT_OPERATIONS_TO_TREASUARY"><di:waypoint x="510" y="200" /><di:waypoint x="560" y="200" /></bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_TREASURY_TO_COMPLETE_di" bpmnElement="Flow_TREASURY_TO_COMPLETE"><di:waypoint x="690" y="200" /><di:waypoint x="740" y="200" /></bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            </bpmn:definitions>
            """;
    }
}
