using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChequeBatchGlPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GL_TRAN_SET_ID",
                table: "CHQ_UPLOAD_BATCHES",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ADM_PRODUCT_ACTIONS",
                columns: table => new
                {
                    PRODUCT_ACTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_PRODUCT_ACTIONS", x => x.PRODUCT_ACTION_ID);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_ACTIONS_ADM_PRODUCTS",
                        column: x => x.PRODUCT_ID,
                        principalTable: "ADM_PRODUCTS",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_ACTIONS_ADM_PRODUCT_EVENT_RULES",
                        column: x => x.PRODUCT_EVENT_RULE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULES",
                        principalColumn: "PRODUCT_EVENT_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_TRAN_SET_SEQUENCES",
                columns: table => new
                {
                    MODULE_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    YEAR_NO = table.Column<int>(type: "integer", nullable: false),
                    APP_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MODULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MODULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LAST_NO = table.Column<long>(type: "bigint", nullable: false),
                    DIGITS = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_TRAN_SET_SEQUENCES", x => new { x.MODULE_CODE, x.YEAR_NO });
                });

            // the modules agreed with Ahmed (30 Oct); a later year gets its row from the module's latest row
            migrationBuilder.Sql("""
                INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('MPA', 2026, 'MTG', 'Mortgage Portfolio Acquisition', 0, 8, true, 'SYSTEM', now(), 1),
                       ('FCT', 2026, 'FCT', 'Factoring', 0, 8, true, 'SYSTEM', now(), 1),
                       ('CHW', 2026, 'FCT', 'Cheques Wallet', 0, 8, true, 'SYSTEM', now(), 1),
                       ('COL', 2026, 'COL', 'Collection and Repayments', 0, 8, true, 'SYSTEM', now(), 1),
                       ('GLM', 2026, 'GL', 'Manual GL journals', 0, 8, true, 'SYSTEM', now(), 1)
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_ACTIONS_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_ACTIONS",
                column: "PRODUCT_EVENT_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_ACTIONS_PRODUCT_ID",
                table: "ADM_PRODUCT_ACTIONS",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_ACTIONS_ACTION_CODE",
                table: "ADM_PRODUCT_ACTIONS",
                column: "ACTION_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_PRODUCT_ACTIONS");

            migrationBuilder.DropTable(
                name: "GL_TRAN_SET_SEQUENCES");

            migrationBuilder.DropColumn(
                name: "GL_TRAN_SET_ID",
                table: "CHQ_UPLOAD_BATCHES");
        }
    }
}
