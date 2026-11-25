using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Client is every customer's built-in role (Ahmed, 25 Nov 2026): new customers get it when created, and this gives it to the
    /// customers created before, as ONBOARDING like a new one. Down leaves the roles in place.
    /// </summary>
    public partial class ClientRoleForEveryCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""CRM_CLIENT_ROLES"" (""CLIENT_ID"", ""ROLE_CODE"", ""STATUS_CODE"", ""CREATED_AT"", ""CREATED_BY"", ""ROW_VERSION"")
SELECT c.""CLIENT_ID"", 'CLIENT', 'ONBOARDING', now(), 'SYSTEM', 0
FROM ""CRM_CLIENTS"" c
WHERE NOT EXISTS (SELECT 1 FROM ""CRM_CLIENT_ROLES"" r WHERE r.""CLIENT_ID"" = c.""CLIENT_ID"" AND r.""ROLE_CODE"" = 'CLIENT');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
