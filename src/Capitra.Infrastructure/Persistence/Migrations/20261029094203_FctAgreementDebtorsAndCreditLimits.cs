using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FctAgreementDebtorsAndCreditLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FCT_AGREEMENT_DEBTORS",
                columns: table => new
                {
                    AGRMT_DEBTOR_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLIENT_DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    AGRMT_PRODUCT_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEBTOR_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    APPROVED_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    UTILIZED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AVAILABLE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_INVOICE_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    CONCENTRATION_LIMIT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CURRENT_CONCENTRATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PAYMENT_TERMS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAYMENT_TERMS_AR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    STANDARD_PAYMENT_DAYS = table.Column<int>(type: "integer", nullable: true),
                    IS_NOTIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    NOTIFICATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTIFICATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_ACKNOWLEDGMENT_RECEIVED = table.Column<bool>(type: "boolean", nullable: false),
                    ACKNOWLEDGMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RISK_GRADE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IS_INSURANCE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    INSURANCE_COVERAGE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVAL_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EFFECTIVE_FROM_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    REMARKS = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REMARKS_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DEBTOR_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TAX_ID = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_AGREEMENT_DEBTORS", x => x.AGRMT_DEBTOR_ID);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DEBTORS_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DEBTORS_CRM_LKP_STATUS_CODES",
                        column: x => x.STATUS_ID,
                        principalTable: "CRM_LKP_STATUS_CODES",
                        principalColumn: "STATUS_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DEBTORS_FCT_AGREEMENTS",
                        column: x => x.AGREEMENT_ID,
                        principalTable: "FCT_AGREEMENTS",
                        principalColumn: "AGREEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_CREDIT_LIMITS",
                columns: table => new
                {
                    CREDIT_LIMIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LIMIT_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LIMIT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LIMIT_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LIMIT_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LIMIT_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    AGRMT_DEBTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    LIMIT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LIMIT_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LIMIT_CATEGORY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LIMIT_CATEGORY_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUESTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RECOMMENDED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    APPROVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENT_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UTILIZED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AVAILABLE_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    PENDING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RESERVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EXCESS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UTILIZATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PEAK_UTILIZATION = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    PEAK_UTILIZATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    AVG_UTILIZATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SUBLIMIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SUBLIMIT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SUBLIMIT_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PARENT_LIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    RISK_GRADE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RISK_GRADE_AR = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RISK_SCORE = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    PD_PERCENTAGE = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    LGD_PERCENTAGE = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    RISK_WEIGHT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    EXPECTED_LOSS = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    PROVISION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROVISION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CONCENTRATION_LIMIT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CURRENT_CONCENTRATION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    IS_INSURANCE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    INSURED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSURANCE_COVERAGE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INSURANCE_POLICY_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPLICATION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EXPIRY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LAST_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NEXT_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REVIEW_FREQUENCY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LAST_UTILIZATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REQUESTED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RECOMMENDED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RECOMMENDATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_AUTHORITY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_AUTHORITY_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPROVAL_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVAL_NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    APPROVAL_NOTES_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LIMIT_CONDITIONS = table.Column<string>(type: "text", nullable: true),
                    LIMIT_CONDITIONS_AR = table.Column<string>(type: "text", nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: false),
                    PREVIOUS_LIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    VERSION_NUMBER = table.Column<int>(type: "integer", nullable: true),
                    IS_TEMPORARY = table.Column<bool>(type: "boolean", nullable: false),
                    TEMP_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REMARKS = table.Column<string>(type: "text", nullable: true),
                    REMARKS_AR = table.Column<string>(type: "text", nullable: true),
                    ATTRIBUTE1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    SPREAD_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_CREDIT_LIMITS", x => x.CREDIT_LIMIT_ID);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_CRM_LKP_STATUS_CODES",
                        column: x => x.STATUS_ID,
                        principalTable: "CRM_LKP_STATUS_CODES",
                        principalColumn: "STATUS_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_FCT_AGREEMENTS",
                        column: x => x.AGREEMENT_ID,
                        principalTable: "FCT_AGREEMENTS",
                        principalColumn: "AGREEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_FCT_AGREEMENT_DEBTORS",
                        column: x => x.AGRMT_DEBTOR_ID,
                        principalTable: "FCT_AGREEMENT_DEBTORS",
                        principalColumn: "AGRMT_DEBTOR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_FCT_CREDIT_LIMITS_PARENT_LIMIT_ID",
                        column: x => x.PARENT_LIMIT_ID,
                        principalTable: "FCT_CREDIT_LIMITS",
                        principalColumn: "CREDIT_LIMIT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CREDIT_LIMITS_FCT_CREDIT_LIMITS_PREVIOUS_LIMIT_ID",
                        column: x => x.PREVIOUS_LIMIT_ID,
                        principalTable: "FCT_CREDIT_LIMITS",
                        principalColumn: "CREDIT_LIMIT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_AGREEMENT_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "AGREEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_AGRMT_PRODUCT_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "AGRMT_PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_CLIENT_DEBTOR_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "CLIENT_DEBTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_CURRENCY_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_DEBTOR_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "DEBTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_IS_NOTIFIED",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "IS_NOTIFIED");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DEBTORS_STATUS_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_AGREEMENT_DEBTORS_AGREEMENT_ID_DEBTOR_ID",
                table: "FCT_AGREEMENT_DEBTORS",
                columns: new[] { "AGREEMENT_ID", "DEBTOR_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_AGREEMENT_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "AGREEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_AGRMT_DEBTOR_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "AGRMT_DEBTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_CLIENT_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_CURRENCY_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_DEBTOR_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "DEBTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_EFFECTIVE_AT_EXPIRY_AT",
                table: "FCT_CREDIT_LIMITS",
                columns: new[] { "EFFECTIVE_AT", "EXPIRY_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_IS_TEMPORARY_TEMP_EXPIRY_DATE",
                table: "FCT_CREDIT_LIMITS",
                columns: new[] { "IS_TEMPORARY", "TEMP_EXPIRY_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_LIMIT_LEVEL",
                table: "FCT_CREDIT_LIMITS",
                column: "LIMIT_LEVEL");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_LIMIT_TYPE",
                table: "FCT_CREDIT_LIMITS",
                column: "LIMIT_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_NEXT_REVIEW_DATE",
                table: "FCT_CREDIT_LIMITS",
                column: "NEXT_REVIEW_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_PARENT_LIMIT_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "PARENT_LIMIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_PREVIOUS_LIMIT_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "PREVIOUS_LIMIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_RISK_GRADE",
                table: "FCT_CREDIT_LIMITS",
                column: "RISK_GRADE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CREDIT_LIMITS_STATUS_ID",
                table: "FCT_CREDIT_LIMITS",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_CREDIT_LIMITS_LIMIT_NUMBER",
                table: "FCT_CREDIT_LIMITS",
                column: "LIMIT_NUMBER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_CREDIT_LIMITS");

            migrationBuilder.DropTable(
                name: "FCT_AGREEMENT_DEBTORS");
        }
    }
}
