using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioFinancialReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "SUBMITTED_DATE",
                table: "MTG_PORTFOLIO",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                columns: table => new
                {
                    FINANCIAL_REVIEW_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PV_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    PV_BASE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CHEQUE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CHEQUES_FACE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHEQUES_PV_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MARGIN_BASE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MARGIN_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MARGIN_BASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MARGIN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ADMIN_BASE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ADMIN_EXPENSE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    ADMIN_BASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ADMIN_CALC_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ADMIN_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_ADMIN_INCLUDED = table.Column<bool>(type: "boolean", nullable: false),
                    UNIT_COUNT = table.Column<int>(type: "integer", nullable: false),
                    LEGAL_FEE_PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LEGAL_CALC_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LEGAL_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_LEGAL_INCLUDED = table.Column<bool>(type: "boolean", nullable: false),
                    VALUATION_FEE_PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VALUATION_CALC_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VALUATION_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_VALUATION_INCLUDED = table.Column<bool>(type: "boolean", nullable: false),
                    TOTAL_CHARGES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PROPOSED_PURCHASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EXPECTED_RETURN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO_FINANCIAL_REVIEW", x => x.FINANCIAL_REVIEW_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_FINANCIAL_REVIEW_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { "FINANCIAL_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 14, true, "تم قبول المراجعة المالية", "Financial Review Accepted", 1, null, null },
                    { "FINANCIAL_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 15, true, "تم رفض المراجعة المالية", "Financial Review Rejected", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PORTFOLIO_FINANCIAL_REVIEW_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                column: "PORTFOLIO_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO_FINANCIAL_REVIEW");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "FINANCIAL_REVIEW_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "FINANCIAL_REVIEW_REJECTED");

            migrationBuilder.DropColumn(
                name: "SUBMITTED_DATE",
                table: "MTG_PORTFOLIO");
        }
    }
}
