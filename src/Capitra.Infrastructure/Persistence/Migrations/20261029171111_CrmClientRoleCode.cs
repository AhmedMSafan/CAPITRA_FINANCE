using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrmClientRoleCode : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS",
                column: "CLIENT_ROLE_CODE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CRM_CLIENTS_CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "CLIENT_ROLE_CODE",
                table: "CRM_CLIENTS");
        }
    }
}
