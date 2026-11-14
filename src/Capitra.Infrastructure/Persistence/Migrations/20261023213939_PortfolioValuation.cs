using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioValuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_VALUATION",
                columns: table => new
                {
                    VALUATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROJECT_UNIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALUATION_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALUER_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VALUER_REGISTRATION_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VALUATION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MARKET_VALUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RECOMMENDED_VALUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROJECT_COMPLETION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    FACILITIES_STATUS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_MTG_VALUATION", x => x.VALUATION_ID);
                    table.ForeignKey(
                        name: "FK_MTG_VALUATION_MTG_LKP_VALUATION_TYPE",
                        column: x => x.VALUATION_TYPE_CODE,
                        principalTable: "MTG_LKP_VALUATION_TYPE",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_VALUATION_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_VALUATION_MTG_PROJECT",
                        column: x => x.PROJECT_ID,
                        principalTable: "MTG_PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_VALUATION_MTG_PROJECT_UNIT",
                        column: x => x.PROJECT_UNIT_ID,
                        principalTable: "MTG_PROJECT_UNIT",
                        principalColumn: "PROJECT_UNIT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[] { "VALUATION_COMPLETED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 9, true, "اكتمل التقييم", "Valuation Completed", 1, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_VALUATION_PORTFOLIO_ID",
                table: "MTG_VALUATION",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_VALUATION_PROJECT_ID",
                table: "MTG_VALUATION",
                column: "PROJECT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_VALUATION_PROJECT_UNIT_ID",
                table: "MTG_VALUATION",
                column: "PROJECT_UNIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_VALUATION_VALUATION_TYPE_CODE",
                table: "MTG_VALUATION",
                column: "VALUATION_TYPE_CODE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_VALUATION");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "VALUATION_COMPLETED");
        }
    }
}
