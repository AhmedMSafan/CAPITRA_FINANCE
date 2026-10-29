using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrmClientRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ADM_LKP_CLIENT_ROLES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_CLIENT_ROLES", x => x.CODE);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS",
                column: "CLIENT_ROLE_CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_CRM_CLIENTS_ADM_LKP_CLIENT_ROLES",
                table: "CRM_CLIENTS",
                column: "CLIENT_ROLE_CODE",
                principalTable: "ADM_LKP_CLIENT_ROLES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRM_CLIENTS_ADM_LKP_CLIENT_ROLES",
                table: "CRM_CLIENTS");

            migrationBuilder.DropTable(
                name: "ADM_LKP_CLIENT_ROLES");

            migrationBuilder.DropIndex(
                name: "IX_CRM_CLIENTS_CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS");
        }
    }
}
