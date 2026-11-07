using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioBoardReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "APPROVED_DATE",
                table: "MTG_PORTFOLIO",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BOARD_DECISION_CODE",
                table: "MTG_PORTFOLIO",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BOARD_DECISION_DATE",
                table: "MTG_PORTFOLIO",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BOARD_DECISION_NO",
                table: "MTG_PORTFOLIO",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { "BOARD_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 16, true, "تمت موافقة مجلس الإدارة", "Board Review Accepted", 1, null, null },
                    { "BOARD_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 17, true, "تم رفض مجلس الإدارة", "Board Review Rejected", 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "BOARD_REVIEW_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "BOARD_REVIEW_REJECTED");

            migrationBuilder.DropColumn(
                name: "APPROVED_DATE",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "BOARD_DECISION_CODE",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "BOARD_DECISION_DATE",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropColumn(
                name: "BOARD_DECISION_NO",
                table: "MTG_PORTFOLIO");
        }
    }
}
