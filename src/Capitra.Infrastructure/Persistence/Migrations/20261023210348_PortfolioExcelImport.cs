using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioExcelImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.AddColumn<string>(
                name: "SELLER_CONTRACT_NO",
                table: "MTG_PORTFOLIO_ACCOUNT",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO_IMPORT",
                columns: table => new
                {
                    PORTFOLIO_IMPORT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IMPORT_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TOTAL_ROW_COUNT = table.Column<int>(type: "integer", nullable: false),
                    VALID_ROW_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ERROR_ROW_COUNT = table.Column<int>(type: "integer", nullable: false),
                    IMPORTED_ROW_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO_IMPORT", x => x.PORTFOLIO_IMPORT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_IMPORT_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[] { "DATA_LOADING_COMPLETED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 2, true, "اكتمل تحميل البيانات", "Data Loading Completed", 1, null, null });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PORTFOLIO_ACCOUNT",
                columns: new[] { "PORTFOLIO_ID", "SELLER_CONTRACT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PORTFOLIO_IMPORT_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_IMPORT",
                column: "PORTFOLIO_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO_IMPORT");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "DATA_LOADING_COMPLETED");

            migrationBuilder.DropColumn(
                name: "SELLER_CONTRACT_NO",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PORTFOLIO_ID");
        }
    }
}
