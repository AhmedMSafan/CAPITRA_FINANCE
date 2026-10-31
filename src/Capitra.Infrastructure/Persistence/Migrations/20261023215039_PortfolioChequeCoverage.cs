using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioChequeCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO_CHEQUE",
                columns: table => new
                {
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CHEQUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_ENDORSED_TO_COMPANY = table.Column<bool>(type: "boolean", nullable: false),
                    CHEQUE_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CLEARING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RETURN_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO_CHEQUE", x => x.CHEQUE_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_CHEQUE_ADM_LKP_CHEQUE_STATUS",
                        column: x => x.CHEQUE_STATUS_CODE,
                        principalTable: "ADM_LKP_CHEQUE_STATUS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO_ACCOUNT",
                        column: x => x.PORTFOLIO_ACCOUNT_ID,
                        principalTable: "MTG_PORTFOLIO_ACCOUNT",
                        principalColumn: "PORTFOLIO_ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { "CHEQUE_COVERAGE_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 12, true, "تم قبول تغطية الشيكات", "Cheque Coverage Accepted", 1, null, null },
                    { "CHEQUE_COVERAGE_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 13, true, "تم رفض تغطية الشيكات", "Cheque Coverage Rejected", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_CHEQUE_STATUS_CODE",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "CHEQUE_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "PORTFOLIO_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "CHEQUE_COVERAGE_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "CHEQUE_COVERAGE_REJECTED");
        }
    }
}
