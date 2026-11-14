using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioLegalReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_LEGAL_REVIEW",
                columns: table => new
                {
                    LEGAL_REVIEW_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_OWNERSHIP_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BUILDING_LICENSE_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REGISTRATION_POSSIBLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MORTGAGE_POSSIBLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ENCUMBRANCE_FREE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_TITLE_DEED_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    LEGAL_FINDINGS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LEGAL_RECOMMENDATION = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    REVIEWED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    APPROVED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LEGAL_REVIEW", x => x.LEGAL_REVIEW_ID);
                    table.ForeignKey(
                        name: "FK_MTG_LEGAL_REVIEW_MTG_PORTFOLIO",
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
                    { "LEGAL_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 7, true, "تم قبول المراجعة القانونية", "Legal Review Accepted", 1, null, null },
                    { "LEGAL_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 8, true, "تم رفض المراجعة القانونية", "Legal Review Rejected", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_LEGAL_REVIEW_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEW",
                column: "PORTFOLIO_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_LEGAL_REVIEW");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "LEGAL_REVIEW_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "LEGAL_REVIEW_REJECTED");
        }
    }
}
