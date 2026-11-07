using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModuleSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_MODULE_SETTING_HISTORY",
                columns: table => new
                {
                    HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SETTING_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OLD_VALUE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NEW_VALUE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_MODULE_SETTING_HISTORY", x => x.HISTORY_ID);
                });

            migrationBuilder.CreateTable(
                name: "ADM_MODULE_SETTINGS",
                columns: table => new
                {
                    SETTING_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MODULE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SETTING_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    VALUE_TEXT = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PENDING_VALUE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PENDING_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REQUESTED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CHANGED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_MODULE_SETTINGS", x => x.SETTING_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_MODULE_SETTING_HISTORY_SETTING_CODE",
                table: "ADM_MODULE_SETTING_HISTORY",
                column: "SETTING_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_MODULE_SETTINGS_SETTING_CODE",
                table: "ADM_MODULE_SETTINGS",
                column: "SETTING_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_MODULE_SETTING_HISTORY");

            migrationBuilder.DropTable(
                name: "ADM_MODULE_SETTINGS");
        }
    }
}
