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
            // A database that ran FINOVA_FINANCEING already has CLIENT_ROLE_CODE, so add the column and index only when missing.
            migrationBuilder.Sql("""ALTER TABLE "CRM_CLIENTS" ADD COLUMN IF NOT EXISTS "CLIENT_ROLE_CODE" character varying(30);""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_CRM_CLIENTS_CLIENT_ROLE_CODE" ON "CRM_CLIENTS" ("CLIENT_ROLE_CODE");""");
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
