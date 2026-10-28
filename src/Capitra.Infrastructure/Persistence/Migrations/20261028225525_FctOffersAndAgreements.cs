using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FctOffersAndAgreements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FCT_AGREEMENTS",
                columns: table => new
                {
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AGREEMENT_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AGREEMENT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AGREEMENT_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AGREEMENT_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    AGREEMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AGREEMENT_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FACTORING_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FACTORING_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RECOURSE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RECOURSE_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NOTIFICATION_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NOTIFICATION_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PRODUCT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACILITY_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    APPROVED_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    UTILIZED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AVAILABLE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SUBLIMIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MAX_ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MIN_ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RESERVE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BASE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SPREAD_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    EFFECTIVE_RATE_PCT = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    MARGIN_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PENALTY_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DAY_COUNT_CONVENTION = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INTEREST_CALC_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    INTEREST_FREQ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    APPLICATION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SIGNING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MATURITY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FIRST_UTILIZATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_UTILIZATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RENEWAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NEXT_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TERMINATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TENOR_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    MAX_INVOICE_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    MIN_INVOICE_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    GRACE_PERIOD_DAYS = table.Column<int>(type: "integer", nullable: true),
                    MAX_DEBTOR_CONCENTRATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_SINGLE_INVOICE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_SINGLE_INVOICE_AMT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    MIN_INVOICE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAX_DILUTION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DILUTION_RESERVE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_AUTHORITY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_AUTHORITY_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    PREVIOUS_AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    VERSION_NUMBER = table.Column<int>(type: "integer", nullable: true),
                    RELATIONSHIP_MANAGER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RM_EMPLOYEE_ID = table.Column<long>(type: "bigint", nullable: true),
                    OPERATIONS_OFFICER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OPS_EMPLOYEE_ID = table.Column<long>(type: "bigint", nullable: true),
                    GOVERNING_LAW = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GOVERNING_LAW_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JURISDICTION = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JURISDICTION_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SPECIAL_CONDITIONS = table.Column<string>(type: "text", nullable: true),
                    SPECIAL_CONDITIONS_AR = table.Column<string>(type: "text", nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    NOTES_AR = table.Column<string>(type: "text", nullable: true),
                    ATTRIBUTE1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INVESTIGATION_REPORT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREDIT_BUREAU_SCORE = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    INVESTIGATION_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AML_RISK_BAND = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AML_SCREENING_REPORT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AML_SCREENING_RESULT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AML_SCREENING_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LEGAL_OPENION_REPORT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LEGAL_OPENION_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FS_ASSESSMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    FS_ASSESSMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FS_ASSESSMENT_RESULT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RR_ASSESSMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    RR_ASSESSMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RR_ASSESSMENT_RESULT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    REQUIRED_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    RECOMMENDED_LIMT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    RECOMMENDED_ADV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RECOMMENDED_TENOR_MTH = table.Column<int>(type: "integer", nullable: true),
                    CREDIT_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREDIT_MEMO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RISK_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RISK_MEMO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RISK_DECISION = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    APPROVED_ADV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    APPROVED_TENOR_MTH = table.Column<int>(type: "integer", nullable: true),
                    FS_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RR_NOTES = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CONTRACT_START_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONTRACT_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONSTRACT_SIGNOFF_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ADDITIONAL_TERMS_CONDITIONS = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CURRENT_STEP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    TASK_ID = table.Column<long>(type: "bigint", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_AGREEMENTS", x => x.AGREEMENT_ID);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENTS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENTS_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENTS_CRM_LKP_STATUS_CODES",
                        column: x => x.STATUS_ID,
                        principalTable: "CRM_LKP_STATUS_CODES",
                        principalColumn: "STATUS_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENTS_FCT_AGREEMENTS",
                        column: x => x.PREVIOUS_AGREEMENT_ID,
                        principalTable: "FCT_AGREEMENTS",
                        principalColumn: "AGREEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_OFFERS",
                columns: table => new
                {
                    OFFER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OFFER_REF = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACTORING_TYPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RECOURSE_TYPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PRODUCT_CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUESTED_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MAX_ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MIN_ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RESERVE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BASE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SPREAD_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    EFFECTIVE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MARGIN_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PENALTY_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DAY_COUNT_CONVENTION = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INTEREST_CALC_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    INTEREST_FREQ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OFFER_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CLIENT_RESPONSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    AVG_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    MAX_INVOICE_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    MIN_INVOICE_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    MAX_DEBTOR_CONCENTRATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_SINGLE_INVOICE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_SINGLE_INVOICE_AMT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    MIN_INVOICE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAX_DILUTION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DILUTION_RESERVE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RELATIONSHIP_MANAGER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RM_EMPLOYEE_ID = table.Column<long>(type: "bigint", nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    RISK_BAND = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ATTRIBUTE2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MAX_LIMIT_TENOR_MTH = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    AGREEMENT_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_OFFERS", x => x.OFFER_ID);
                    table.ForeignKey(
                        name: "FK_FCT_OFFERS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_OFFERS_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_AGREEMENT_TYPE_FACTORING_TYPE",
                table: "FCT_AGREEMENTS",
                columns: new[] { "AGREEMENT_TYPE", "FACTORING_TYPE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_CLIENT_ID",
                table: "FCT_AGREEMENTS",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_CURRENCY_ID",
                table: "FCT_AGREEMENTS",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_EFFECTIVE_AT_MATURITY_AT",
                table: "FCT_AGREEMENTS",
                columns: new[] { "EFFECTIVE_AT", "MATURITY_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_NEXT_REVIEW_DATE",
                table: "FCT_AGREEMENTS",
                column: "NEXT_REVIEW_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_PREVIOUS_AGREEMENT_ID",
                table: "FCT_AGREEMENTS",
                column: "PREVIOUS_AGREEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_RATE_TYPE_ID",
                table: "FCT_AGREEMENTS",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_RECOURSE_TYPE",
                table: "FCT_AGREEMENTS",
                column: "RECOURSE_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_RM_EMPLOYEE_ID",
                table: "FCT_AGREEMENTS",
                column: "RM_EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENTS_STATUS_ID",
                table: "FCT_AGREEMENTS",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_AGREEMENTS_AGREEMENT_NUMBER",
                table: "FCT_AGREEMENTS",
                column: "AGREEMENT_NUMBER",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_OFFERS_CLIENT_ID_CURRENCY_ID_OFFER_AT",
                table: "FCT_OFFERS",
                columns: new[] { "CLIENT_ID", "CURRENCY_ID", "OFFER_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_OFFERS_CURRENCY_ID",
                table: "FCT_OFFERS",
                column: "CURRENCY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_AGREEMENTS");

            migrationBuilder.DropTable(
                name: "FCT_OFFERS");
        }
    }
}
