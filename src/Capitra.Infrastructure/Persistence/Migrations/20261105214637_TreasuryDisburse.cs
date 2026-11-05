using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TreasuryDisburse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FUNDING_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TRS_DISBURSEMENT_FUNDINGS",
                columns: table => new
                {
                    FUNDING_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SUBLIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: true),
                    FUNDED_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_OVERRIDE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    OVERRIDE_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OVERRIDE_APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OVERRIDE_APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CHANGED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EXECUTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXECUTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_DISBURSEMENT_FUNDINGS", x => x.FUNDING_ID);
                    table.ForeignKey(
                        name: "FK_TRS_DISBURSEMENT_FUNDINGS_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_BANKS",
                        column: x => x.BANK_ID,
                        principalTable: "TRS_BANKS",
                        principalColumn: "BANK_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_CREDIT_FACILITIES",
                        column: x => x.FACILITY_ID,
                        principalTable: "TRS_CREDIT_FACILITIES",
                        principalColumn: "FACILITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_FACILITY_SUBLIMITS",
                        column: x => x.SUBLIMIT_ID,
                        principalTable: "TRS_FACILITY_SUBLIMITS",
                        principalColumn: "SUBLIMIT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_FUNDING_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                column: "FUNDING_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_BANK_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                column: "BANK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_DISBURSEMENT_ID_STATUS_CODE",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                columns: new[] { "DISBURSEMENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_FACILITY_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                column: "FACILITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_SUBLIMIT_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                column: "SUBLIMIT_ID");

            // the Treasury Disburse step opens the new source-of-funds page (and its name is spelled right); open tasks follow
            migrationBuilder.Sql("""
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = REPLACE("BPMN_XML", 'name="Tresaury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="crm-disbursement-request"', 'name="Treasury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="trs-disburse"')
                WHERE "WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF';
                UPDATE "WFE_TASKS" t SET "ROUTE" = 'trs-disburse' FROM "WFE_STEP_INSTANCES" s
                WHERE s."STEP_INSTANCE_ID" = t."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'TREASURY_DISBURSE' AND t."ROUTE" = 'crm-disbursement-request';
                UPDATE "WFE_INBOX" i SET "ROUTE" = 'trs-disburse' FROM "WFE_STEP_INSTANCES" s
                WHERE s."STEP_INSTANCE_ID" = i."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'TREASURY_DISBURSE' AND i."ROUTE" = 'crm-disbursement-request';
                UPDATE "WFE_STEP_INSTANCES" SET "STEP_NAME" = 'Treasury Disburse' WHERE "STEP_CODE" = 'TREASURY_DISBURSE' AND "STEP_NAME" = 'Tresaury Disburse';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // back to the disbursement request page
            migrationBuilder.Sql("""
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = REPLACE("BPMN_XML", 'name="Treasury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="trs-disburse"', 'name="Tresaury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="crm-disbursement-request"')
                WHERE "WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF';
                UPDATE "WFE_TASKS" t SET "ROUTE" = 'crm-disbursement-request' FROM "WFE_STEP_INSTANCES" s
                WHERE s."STEP_INSTANCE_ID" = t."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'TREASURY_DISBURSE' AND t."ROUTE" = 'trs-disburse';
                UPDATE "WFE_INBOX" i SET "ROUTE" = 'crm-disbursement-request' FROM "WFE_STEP_INSTANCES" s
                WHERE s."STEP_INSTANCE_ID" = i."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'TREASURY_DISBURSE' AND i."ROUTE" = 'trs-disburse';
                UPDATE "WFE_STEP_INSTANCES" SET "STEP_NAME" = 'Tresaury Disburse' WHERE "STEP_CODE" = 'TREASURY_DISBURSE' AND "STEP_NAME" = 'Treasury Disburse';
                """);

            migrationBuilder.DropTable(
                name: "TRS_DISBURSEMENT_FUNDINGS");

            migrationBuilder.DropIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_FUNDING_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS");

            migrationBuilder.DropColumn(
                name: "FUNDING_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS");
        }
    }
}
