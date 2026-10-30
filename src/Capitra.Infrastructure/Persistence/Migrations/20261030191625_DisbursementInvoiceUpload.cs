using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DisbursementInvoiceUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_RATE_INDEXES",
                columns: table => new
                {
                    RATE_INDEX_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RATE_INDEX_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RATE_INDEX_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_RATE_INDEXES", x => x.RATE_INDEX_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_INVOICES",
                columns: table => new
                {
                    INVOICE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INVOICE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INVOICE_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLIENT_INVOICE_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLIENT_INVOICE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLIENT_PO_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLIENT_DO_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLIENT_CONTRACT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    AGRMT_DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREDIT_LIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    INVOICE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    INVOICE_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    INVOICE_CATEGORY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    INVOICE_CATEGORY_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FACTORING_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FACTORING_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RECOURSE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RECOURSE_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    GROSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TAX_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DISCOUNT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NET_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ELIGIBLE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INELIGIBLE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INELIGIBLE_REASON = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    INELIGIBLE_REASON_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    APPROVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ADVANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    FUNDED_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    UNFUNDED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RESERVE_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    RESERVE_HELD_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    COLLECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUTSTANDING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SETTLED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    WRITE_OFF_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREDIT_NOTE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DEBIT_NOTE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ADJUSTMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DILUTION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DILUTION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DISPUTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_DISPUTED = table.Column<bool>(type: "boolean", nullable: false),
                    DISPUTE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DISPUTE_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DISPUTE_REASON_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SUBMISSION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VERIFICATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FUNDING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ORIGINAL_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXTENDED_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GRACE_PERIOD_DAYS = table.Column<int>(type: "integer", nullable: true),
                    EFFECTIVE_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FIRST_COLLECTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_COLLECTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SETTLEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DAYS_OUTSTANDING = table.Column<int>(type: "integer", nullable: true),
                    DAYS_PAST_DUE = table.Column<int>(type: "integer", nullable: true),
                    AGING_BUCKET = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AGING_BUCKET_AR = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IS_OVERDUE = table.Column<bool>(type: "boolean", nullable: false),
                    OVERDUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    OVERDUE_DAYS = table.Column<int>(type: "integer", nullable: true),
                    INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PENALTY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_CHARGES = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    BASE_CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    BASE_GROSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BASE_NET_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BASE_FUNDED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BASE_OUTSTANDING = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    IS_DEBTOR_CONFIRMED = table.Column<bool>(type: "boolean", nullable: false),
                    DEBTOR_CONFIRM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DEBTOR_CONFIRM_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DEBTOR_CONFIRM_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DEBTOR_REJECTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DEBTOR_REJECTION_REASON_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ASSIGNED = table.Column<bool>(type: "boolean", nullable: false),
                    ASSIGNMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ASSIGNMENT_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_NOTICE_SENT = table.Column<bool>(type: "boolean", nullable: false),
                    NOTICE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTICE_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_NOTICE_ACKNOWLEDGED = table.Column<bool>(type: "boolean", nullable: false),
                    NOTICE_ACK_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_INSURED = table.Column<bool>(type: "boolean", nullable: false),
                    INSURED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSURANCE_POLICY_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    INSURANCE_COVERAGE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RISK_GRADE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RISK_SCORE = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    PROVISION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PROVISION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EXPECTED_LOSS = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    IS_RECOURSE = table.Column<bool>(type: "boolean", nullable: false),
                    RECOURSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RECOURSE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RECOURSE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RECOURSE_STATUS_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VERIFIED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VERIFICATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VERIFICATION_NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VERIFICATION_NOTES_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    APPROVAL_NOTES_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REJECTION_REASON = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REJECTION_REASON_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    PREVIOUS_STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DESCRIPTION_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RISK_RATING_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ATTRIBUTE2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE6 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE7 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE8 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE9 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE10 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    REMARKS = table.Column<string>(type: "text", nullable: true),
                    REMARKS_AR = table.Column<string>(type: "text", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_INVOICES", x => x.INVOICE_ID);
                });

            migrationBuilder.CreateTable(
                name: "ADM_RATE_INDEX_VALUES",
                columns: table => new
                {
                    RATE_VALUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RATE_INDEX_ID = table.Column<long>(type: "bigint", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ANNUAL_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_RATE_INDEX_VALUES", x => x.RATE_VALUE_ID);
                    table.ForeignKey(
                        name: "FK_ADM_RATE_INDEX_VALUES_ADM_RATE_INDEXES",
                        column: x => x.RATE_INDEX_ID,
                        principalTable: "ADM_RATE_INDEXES",
                        principalColumn: "RATE_INDEX_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_RATE_INDEX_VALUES_RATE_INDEX_ID_EFFECTIVE_FROM_DATE",
                table: "ADM_RATE_INDEX_VALUES",
                columns: new[] { "RATE_INDEX_ID", "EFFECTIVE_FROM_DATE", "EFFECTIVE_TO_DATE" });

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_RATE_INDEX_VALUES_RATE_INDEX_ID_EFFECTIVE_FROM_DATE",
                table: "ADM_RATE_INDEX_VALUES",
                columns: new[] { "RATE_INDEX_ID", "EFFECTIVE_FROM_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_RATE_INDEXES_RATE_INDEX_CODE",
                table: "ADM_RATE_INDEXES",
                column: "RATE_INDEX_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_INVOICES_CLIENT_ID_CURRENCY_ID_CLIENT_INVOICE_AT",
                table: "FCT_INVOICES",
                columns: new[] { "CLIENT_ID", "CURRENCY_ID", "CLIENT_INVOICE_AT" });

            // The Invoice Upload step now opens the Invoice Entry page; its open tasks and inbox rows follow.
            migrationBuilder.Sql(RouteSql("crm-disbursement-request", "crm-disbursement-invoice-entry"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RouteSql("crm-disbursement-invoice-entry", "crm-disbursement-request"));

            migrationBuilder.DropTable(
                name: "ADM_RATE_INDEX_VALUES");

            migrationBuilder.DropTable(
                name: "FCT_INVOICES");

            migrationBuilder.DropTable(
                name: "ADM_RATE_INDEXES");
        }

        private static string RouteSql(string from, string to) => $"""
            UPDATE "WFE_WORKFLOW_DEFINITIONS"
               SET "BPMN_XML" = replace("BPMN_XML",
                   'finova:stepCode="INVOICE_UPLOAD" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="{from}"',
                   'finova:stepCode="INVOICE_UPLOAD" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="{to}"')
             WHERE "WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF';
            UPDATE "WFE_TASKS" t SET "ROUTE" = '{to}'
              FROM "WFE_STEP_INSTANCES" s
              JOIN "WFE_WORKFLOW_INSTANCES" i ON i."WORKFLOW_INSTANCE_ID" = s."WORKFLOW_INSTANCE_ID"
              JOIN "WFE_WORKFLOW_DEFINITIONS" w ON w."WORKFLOW_DEFINITION_ID" = i."WORKFLOW_DEFINITION_ID"
             WHERE t."STEP_INSTANCE_ID" = s."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'INVOICE_UPLOAD'
               AND w."WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF' AND t."ROUTE" = '{from}';
            UPDATE "WFE_INBOX" x SET "ROUTE" = '{to}'
              FROM "WFE_STEP_INSTANCES" s
              JOIN "WFE_WORKFLOW_INSTANCES" i ON i."WORKFLOW_INSTANCE_ID" = s."WORKFLOW_INSTANCE_ID"
              JOIN "WFE_WORKFLOW_DEFINITIONS" w ON w."WORKFLOW_DEFINITION_ID" = i."WORKFLOW_DEFINITION_ID"
             WHERE x."STEP_INSTANCE_ID" = s."STEP_INSTANCE_ID" AND s."STEP_CODE" = 'INVOICE_UPLOAD'
               AND w."WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF' AND x."ROUTE" = '{from}';
            """;
    }
}
