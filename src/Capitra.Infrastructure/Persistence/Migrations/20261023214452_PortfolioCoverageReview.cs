using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioCoverageReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { "COVERAGE_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 10, true, "تم قبول مراجعة التغطية", "Coverage Review Accepted", 1, null, null },
                    { "COVERAGE_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 11, true, "تم رفض مراجعة التغطية", "Coverage Review Rejected", 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "COVERAGE_REVIEW_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "COVERAGE_REVIEW_REJECTED");
        }
    }
}
