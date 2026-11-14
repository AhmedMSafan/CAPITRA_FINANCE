using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioInquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_INQUIRY_RESULT",
                columns: table => new
                {
                    INQUIRY_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    SUBJECT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    INQUIRY_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INQUIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESULT_TEXT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RISK_LEVEL_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RESULT_DETAILS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_INQUIRY_RESULT", x => x.INQUIRY_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_INQUIRY_RESULT_MTG_LKP_INQUIRY_TYPE",
                        column: x => x.INQUIRY_TYPE_CODE,
                        principalTable: "MTG_LKP_INQUIRY_TYPE",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_INQUIRY_RESULT_MTG_PORTFOLIO",
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
                    { "INQUIRY_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 5, true, "تم قبول الاستعلامات", "Inquiry Accepted", 1, null, null },
                    { "INQUIRY_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 6, true, "تم رفض الاستعلامات", "Inquiry Rejected", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INQUIRY_RESULT_INQUIRY_TYPE_CODE",
                table: "MTG_INQUIRY_RESULT",
                column: "INQUIRY_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INQUIRY_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_INQUIRY_RESULT",
                columns: new[] { "PORTFOLIO_ID", "SUBJECT_TYPE_CODE", "SUBJECT_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_INQUIRY_RESULT");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "INQUIRY_ACCEPTED");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "INQUIRY_REJECTED");
        }
    }
}
