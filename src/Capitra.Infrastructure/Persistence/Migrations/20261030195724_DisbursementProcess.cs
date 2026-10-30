using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DisbursementProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AMOUNT_BASE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENT_CHARGES",
                columns: table => new
                {
                    DISBURSEMENT_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RULE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CALCULATION_METHOD = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    BASE_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    BASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UNITS = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    FIXED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RATE_PERCENT = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SPREAD_PERCENT = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    DAY_COUNT_BASIS = table.Column<int>(type: "integer", nullable: true),
                    CHARGE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TAX_RATE_PERCENT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    TAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHARGE_TIMING_CODE = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    INCOME_RECOGNITION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COLLECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WAIVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CALCULATION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_DISBURSEMENT_CHARGES", x => x.DISBURSEMENT_CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHARGES_ADM_PRODUCT_EVENT_RULES",
                        column: x => x.PRODUCT_EVENT_RULE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULES",
                        principalColumn: "PRODUCT_EVENT_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHARGES_ADM_PRODUCT_EVENT_RULE_CHARGES",
                        column: x => x.RULE_CHARGE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                        principalColumn: "RULE_CHARGE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHARGES_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_REPAYMENTS",
                columns: table => new
                {
                    SCHEDULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ADJUSTED_DUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OPENING_PRINCIPAL = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    OUTSTANDING_PRINCIPAL = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    OUTSTANDING_INTEREST = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    TOTAL_INSTALLMENT = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    CLOSING_PRINCIPAL = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PAID_PRINCIPAL = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PAID_INTEREST = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PAID_FEE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PAID_PENALTY = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PRINCIPAL_DUE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    INTEREST_DUE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    PENALTY_DUE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    FEE_DUE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    INTEREST_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INDEX_RATE_ID = table.Column<long>(type: "bigint", nullable: true),
                    DAY_COUNT = table.Column<int>(type: "integer", nullable: true),
                    DAY_COUNT_BASIS = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LAST_PAYMENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DAYS_OVERDUE = table.Column<int>(type: "integer", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SCHEDULE_VERSION = table.Column<int>(type: "integer", nullable: true),
                    IS_CURRENT_VERSION = table.Column<bool>(type: "boolean", nullable: false),
                    RECALC_REASON = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_REPAYMENTS", x => x.SCHEDULE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENTS_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHARGES_DISBURSEMENT_ID_STATUS_CODE",
                table: "FCT_DISBURSEMENT_CHARGES",
                columns: new[] { "DISBURSEMENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHARGES_PRODUCT_EVENT_RULE_ID",
                table: "FCT_DISBURSEMENT_CHARGES",
                column: "PRODUCT_EVENT_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHARGES_RULE_CHARGE_ID",
                table: "FCT_DISBURSEMENT_CHARGES",
                column: "RULE_CHARGE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_DISBURSEMENT_CHARGES_DISBURSEMENT_ID_RULE_CHARGE_ID",
                table: "FCT_DISBURSEMENT_CHARGES",
                columns: new[] { "DISBURSEMENT_ID", "RULE_CHARGE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENTS_DISBURSEMENT_ID_IS_CURRENT_VERSION_DUE_DATE",
                table: "FCT_REPAYMENTS",
                columns: new[] { "DISBURSEMENT_ID", "IS_CURRENT_VERSION", "DUE_DATE" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENT_CHARGES");

            migrationBuilder.DropTable(
                name: "FCT_REPAYMENTS");

            migrationBuilder.AlterColumn<string>(
                name: "AMOUNT_BASE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);
        }
    }
}
