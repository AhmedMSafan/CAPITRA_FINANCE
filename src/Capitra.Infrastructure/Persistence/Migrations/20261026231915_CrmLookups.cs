using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrmLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_LKP_COUNTRIES",
                columns: table => new
                {
                    COUNTRY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COUNTRY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    COUNTRY_CODE_A3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    COUNTRY_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    COUNTRY_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NATIONALITY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NATIONALITY_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PHONE_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CONTINENT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CONTINENT_AR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_DOMESTIC = table.Column<bool>(type: "boolean", nullable: false),
                    RISK_RATING = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IS_SANCTIONED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_LKP_COUNTRIES", x => x.COUNTRY_ID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_LKP_STATUS_CODES",
                columns: table => new
                {
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    STATUS_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    STATUS_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    STATUS_GROUP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STATUS_GROUP_AR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MODULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ENTITY_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    COLOR_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ICON_CLASS = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_INITIAL = table.Column<bool>(type: "boolean", nullable: false),
                    IS_FINAL = table.Column<bool>(type: "boolean", nullable: false),
                    IS_NEGATIVE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ALLOWS_EDIT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ALLOWS_DELETE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DESCRIPTION_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_LKP_STATUS_CODES", x => x.STATUS_ID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_LKP_CURRENCIES",
                columns: table => new
                {
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CURRENCY_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CURRENCY_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CURRENCY_SYMBOL = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NUMERIC_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    MINOR_UNIT = table.Column<int>(type: "integer", nullable: false),
                    COUNTRY_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_BASE_CURRENCY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_LKP_CURRENCIES", x => x.CURRENCY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_LKP_CURRENCIES_CRM_LKP_COUNTRIES",
                        column: x => x.COUNTRY_ID,
                        principalTable: "CRM_LKP_COUNTRIES",
                        principalColumn: "COUNTRY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_BASE_CURRENCY_ID",
                table: "CRM_CLIENTS",
                column: "BASE_CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_LKP_COUNTRIES_COUNTRY_CODE",
                table: "CRM_LKP_COUNTRIES",
                column: "COUNTRY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_LKP_COUNTRIES_COUNTRY_CODE_A3",
                table: "CRM_LKP_COUNTRIES",
                column: "COUNTRY_CODE_A3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRM_LKP_CURRENCIES_COUNTRY_ID",
                table: "CRM_LKP_CURRENCIES",
                column: "COUNTRY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_LKP_CURRENCIES_CURRENCY_CODE",
                table: "CRM_LKP_CURRENCIES",
                column: "CURRENCY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRM_LKP_STATUS_CODES_ENTITY_TYPE",
                table: "CRM_LKP_STATUS_CODES",
                column: "ENTITY_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_LKP_STATUS_CODES_MODULE_CODE",
                table: "CRM_LKP_STATUS_CODES",
                column: "MODULE_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_LKP_STATUS_CODES_STATUS_CODE_MODULE_CODE_ENTITY_TYPE",
                table: "CRM_LKP_STATUS_CODES",
                columns: new[] { "STATUS_CODE", "MODULE_CODE", "ENTITY_TYPE" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_COUNTRIES",
                table: "CRM_CLIENTS",
                column: "COUNTRY_ID",
                principalTable: "CRM_LKP_COUNTRIES",
                principalColumn: "COUNTRY_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_CURRENCIES",
                table: "CRM_CLIENTS",
                column: "BASE_CURRENCY_ID",
                principalTable: "CRM_LKP_CURRENCIES",
                principalColumn: "CURRENCY_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_STATUS_CODES",
                table: "CRM_CLIENTS",
                column: "STATUS_ID",
                principalTable: "CRM_LKP_STATUS_CODES",
                principalColumn: "STATUS_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_COUNTRIES",
                table: "CRM_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_CURRENCIES",
                table: "CRM_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CRM_CLIENTS_CRM_LKP_STATUS_CODES",
                table: "CRM_CLIENTS");

            migrationBuilder.DropTable(
                name: "CRM_LKP_CURRENCIES");

            migrationBuilder.DropTable(
                name: "CRM_LKP_STATUS_CODES");

            migrationBuilder.DropTable(
                name: "CRM_LKP_COUNTRIES");

            migrationBuilder.DropIndex(
                name: "IX_CRM_CLIENTS_BASE_CURRENCY_ID",
                table: "CRM_CLIENTS");
        }
    }
}
