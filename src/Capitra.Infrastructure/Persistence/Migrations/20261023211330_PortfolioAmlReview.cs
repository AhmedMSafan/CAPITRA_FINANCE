using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioAmlReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DEAL_DECISION_CODE",
                table: "MTG_PORTFOLIO_ACCOUNT",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "REJECTION_REASON",
                table: "MTG_PORTFOLIO",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SELECTED_ACCOUNT_COUNT",
                table: "MTG_PORTFOLIO",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SELECTED_FACE_VALUE_AMOUNT",
                table: "MTG_PORTFOLIO",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SELECTED_OUTSTANDING_AMOUNT",
                table: "MTG_PORTFOLIO",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SELECTED_PAID_AMOUNT",
                table: "MTG_PORTFOLIO",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MTG_AML_RESULT",
                columns: table => new
                {
                    AML_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    SUBJECT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    AML_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SCREENING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AML_RESULT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RISK_LEVEL_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MATCH_SCORE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RESULT_DETAILS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_AML_RESULT", x => x.AML_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_AML_RESULT_MTG_PORTFOLIO",
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
                    { "AML_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 3, true, "تم قبول مراجعة مكافحة غسل الأموال", "AML Accepted", 1, null, null },
                    { "AML_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 4, true, "تم رفض مراجعة مكافحة غسل الأموال", "AML Rejected", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_AML_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_AML_RESULT",
                columns: new[] { "PORTFOLIO_ID", "SUBJECT_TYPE_CODE", "SUBJECT_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_AML_RESULT");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "AML_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "AML_REJECTED");

            migrationBuilder.DropColumn(
                name: "DEAL_DECISION_CODE",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "REJECTION_REASON",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "SELECTED_ACCOUNT_COUNT",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "SELECTED_FACE_VALUE_AMOUNT",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "SELECTED_OUTSTANDING_AMOUNT",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "SELECTED_PAID_AMOUNT",
                table: "MTG_PORTFOLIO");
        }
    }
}
