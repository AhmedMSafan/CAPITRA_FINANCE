using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingLoanCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_LOANS",
                columns: table => new
                {
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRODUCT_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    LICENCE_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_NON_ACCRUAL = table.Column<bool>(type: "boolean", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TERM_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    INSTALLMENT_COUNT = table.Column<int>(type: "integer", nullable: false),
                    INSTALLMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DISBURSEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    FIRST_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    REPAYMENT_FREQUENCY_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    INTEREST_METHOD_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ANNUAL_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    DAY_COUNT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LATE_FEE_GRACE_DAYS = table.Column<int>(type: "integer", nullable: false),
                    LATE_FEE_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LATE_FEE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LATE_FEE_FLAT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LATE_FEE_BASE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PAYMENT_ORDER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IS_INSURANCE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LOANS", x => x.LOAN_ID);
                    table.ForeignKey(
                        name: "FK_LND_LOANS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LND_LOANS_ADM_PRODUCTS",
                        column: x => x.PRODUCT_ID,
                        principalTable: "ADM_PRODUCTS",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LND_LOANS_LND_PRODUCT_VERSIONS",
                        column: x => x.PRODUCT_VERSION_ID,
                        principalTable: "LND_PRODUCT_VERSIONS",
                        principalColumn: "PRODUCT_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LND_INSTALLMENTS",
                columns: table => new
                {
                    INSTALLMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    OPENING_PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PRINCIPAL_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGES_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INSURANCE_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LATE_FEE_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PRINCIPAL_PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGES_PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INSURANCE_PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LATE_FEE_PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CLOSING_PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAID_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_INSTALLMENTS", x => x.INSTALLMENT_ID);
                    table.ForeignKey(
                        name: "FK_LND_INSTALLMENTS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LND_LOAN_STATUS_HIST",
                columns: table => new
                {
                    LOAN_STATUS_HIST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    FROM_STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TO_STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LOAN_STATUS_HIST", x => x.LOAN_STATUS_HIST_ID);
                    table.ForeignKey(
                        name: "FK_LND_LOAN_STATUS_HIST_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOANS_CLIENT_ID_STATUS_CODE",
                table: "LND_LOANS",
                columns: new[] { "CLIENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_LND_LOANS_LOAN_NUMBER",
                table: "LND_LOANS",
                column: "LOAN_NUMBER",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOANS_PRODUCT_ID",
                table: "LND_LOANS",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOANS_PRODUCT_VERSION_ID",
                table: "LND_LOANS",
                column: "PRODUCT_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOANS_STATUS_CODE_LICENCE_CODE",
                table: "LND_LOANS",
                columns: new[] { "STATUS_CODE", "LICENCE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_LND_INSTALLMENTS_DUE_DATE_STATUS_CODE",
                table: "LND_INSTALLMENTS",
                columns: new[] { "DUE_DATE", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_LND_INSTALLMENTS_LOAN_ID_INSTALLMENT_NO",
                table: "LND_INSTALLMENTS",
                columns: new[] { "LOAN_ID", "INSTALLMENT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOAN_STATUS_HIST_LOAN_ID",
                table: "LND_LOAN_STATUS_HIST",
                column: "LOAN_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_INSTALLMENTS");

            migrationBuilder.DropTable(
                name: "LND_LOAN_STATUS_HIST");

            migrationBuilder.DropTable(
                name: "LND_LOANS");
        }
    }
}
