using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Screening uses CAPITRA's existing roles where one fits, and adds roles only for member companies.
    /// <list type="bullet">
    /// <item>OPERATIONS_OFFICER: registers letters, enters parties and decisions, runs the approval workflow.</item>
    /// <item>LEGAL_OFFICER: workflow step 1, Review decision.</item>
    /// <item>RISK_OFFICER: workflow step 2, Approve and publish to the Federation Final List.</item>
    /// <item>Admin keeps every permission (member companies, inquiry audit).</item>
    /// <item>New: MEMBER_COMPANY_ADMIN and MEMBER_COMPANY_USER, the member company logins. Users holding SCR_COMPANY_ADMIN or
    /// SCR_COMPANY_USER are moved to them.</item>
    /// </list>
    /// SCR_DECISION_APPROVAL is marked a system workflow (started by Screening's Run button) and its steps, including tasks already
    /// open, are assigned to LEGAL_OFFICER and RISK_OFFICER. The other SCR_ roles are removed only when no user holds them; a role
    /// still held by a user is kept until the user is moved. No business data is touched.
    /// </summary>
    [DbContext(typeof(CapitraDbContext))]
    [Migration("20261119120000_ScreeningRoleMapping")]
    public partial class ScreeningRoleMapping : Migration
    {
        private static readonly (string Role, string Description, string[] Permissions)[] Grants =
        [
            ("OPERATIONS_OFFICER", "Operations officer", ["SCR.DECISION.VIEW", "SCR.LETTER.EDIT", "SCR.DECISION.EDIT"]),
            ("LEGAL_OFFICER", "Legal officer", ["SCR.DECISION.VIEW", "SCR.DECISION.REVIEW"]),
            ("RISK_OFFICER", "Risk officer", ["SCR.DECISION.VIEW", "SCR.DECISION.APPROVE"]),
            ("MEMBER_COMPANY_ADMIN", "Member company: screens and manages the company's user accounts", ["SCR.INQUIRY.CREATE", "SCR.COMPANY.USERS"]),
            ("MEMBER_COMPANY_USER", "Member company: screens people and companies against the Final List", ["SCR.INQUIRY.CREATE"]),
        ];

        private static readonly (string From, string To)[] Moves =
        [
            ("SCR_COMPANY_ADMIN", "MEMBER_COMPANY_ADMIN"),
            ("SCR_COMPANY_USER", "MEMBER_COMPANY_USER"),
        ];

        private static readonly string[] OldRoles =
            ["SCR_INTAKE", "SCR_DATA_ENTRY", "SCR_REVIEWER", "SCR_PUBLISHER", "SCR_ADMIN", "SCR_AUDITOR", "SCR_COMPANY_ADMIN", "SCR_COMPANY_USER"];

        private static readonly (string From, string To)[] Assignees =
        [
            ("SCR_REVIEWER", "LEGAL_OFFICER"),
            ("SCR_PUBLISHER", "RISK_OFFICER"),
        ];

        private const string ProcessTag = "<bpmn:process id=\"Process_SCR_DECISION\" ";
        private const string SystemTag = "<bpmn:process id=\"Process_SCR_DECISION\" finova:workflowType=\"SYSTEM\" ";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (role, description, permissions) in Grants)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "SEC_ROLES" ("ROLE_ID", "ROLE_NAME", "NORMALIZED_ROLE_NAME", "CONCURRENCY_STAMP", "DESCRIPTION", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT gen_random_uuid()::text, '{role}', '{role}', gen_random_uuid()::text, '{description.Replace("'", "''", StringComparison.Ordinal)}', 'SYSTEM', now(), 1
                    WHERE NOT EXISTS (SELECT 1 FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" = '{role}');

                    INSERT INTO "SEC_ROLE_CLAIMS" ("ROLE_ID", "CLAIM_TYPE", "CLAIM_VALUE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT r."ROLE_ID", 'permission', p.code, 'SYSTEM', now(), 1
                    FROM "SEC_ROLES" r CROSS JOIN unnest(ARRAY[{string.Join(", ", permissions.Select(p => $"'{p}'"))}]) AS p(code)
                    WHERE r."NORMALIZED_ROLE_NAME" = '{role}'
                      AND NOT EXISTS (SELECT 1 FROM "SEC_ROLE_CLAIMS" c WHERE c."ROLE_ID" = r."ROLE_ID" AND c."CLAIM_TYPE" = 'permission' AND c."CLAIM_VALUE" = p.code);
                    """);
            }

            // member company users move to the new roles
            foreach (var (from, to) in Moves)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "SEC_USER_ROLES" ("USER_ID", "ROLE_ID", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT ur."USER_ID", n."ROLE_ID", 'SYSTEM', now(), 1
                    FROM "SEC_USER_ROLES" ur
                    JOIN "SEC_ROLES" o ON o."ROLE_ID" = ur."ROLE_ID" AND o."NORMALIZED_ROLE_NAME" = '{from}'
                    CROSS JOIN "SEC_ROLES" n
                    WHERE n."NORMALIZED_ROLE_NAME" = '{to}'
                      AND NOT EXISTS (SELECT 1 FROM "SEC_USER_ROLES" x WHERE x."USER_ID" = ur."USER_ID" AND x."ROLE_ID" = n."ROLE_ID");

                    DELETE FROM "SEC_USER_ROLES" ur USING "SEC_ROLES" o, "SEC_ROLES" n
                    WHERE o."ROLE_ID" = ur."ROLE_ID" AND o."NORMALIZED_ROLE_NAME" = '{from}' AND n."NORMALIZED_ROLE_NAME" = '{to}'
                      AND EXISTS (SELECT 1 FROM "SEC_USER_ROLES" x WHERE x."USER_ID" = ur."USER_ID" AND x."ROLE_ID" = n."ROLE_ID");
                    """);
            }

            // the old SCR_ roles go only when nobody holds them
            var old = string.Join(", ", OldRoles.Select(r => $"'{r}'"));
            migrationBuilder.Sql($"""
                DELETE FROM "SEC_ROLE_CLAIMS" c USING "SEC_ROLES" r
                WHERE c."ROLE_ID" = r."ROLE_ID" AND r."NORMALIZED_ROLE_NAME" IN ({old})
                  AND NOT EXISTS (SELECT 1 FROM "SEC_USER_ROLES" ur WHERE ur."ROLE_ID" = r."ROLE_ID");
                DELETE FROM "SEC_ROLES" r
                WHERE r."NORMALIZED_ROLE_NAME" IN ({old})
                  AND NOT EXISTS (SELECT 1 FROM "SEC_USER_ROLES" ur WHERE ur."ROLE_ID" = r."ROLE_ID");
                """);

            // the workflow: a system workflow, its steps on the existing roles, open tasks included
            Workflow(migrationBuilder, Assignees, ProcessTag, SystemTag);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Workflow(migrationBuilder, [.. Assignees.Select(a => (a.To, a.From))], SystemTag, ProcessTag);
            foreach (var (role, _, permissions) in Grants.Take(3))
            {
                migrationBuilder.Sql($"""
                    DELETE FROM "SEC_ROLE_CLAIMS" c USING "SEC_ROLES" r
                    WHERE c."ROLE_ID" = r."ROLE_ID" AND r."NORMALIZED_ROLE_NAME" = '{role}' AND c."CLAIM_TYPE" = 'permission'
                      AND c."CLAIM_VALUE" IN ({string.Join(", ", permissions.Select(p => $"'{p}'"))});
                    """);
            }
        }

        private static void Workflow(MigrationBuilder migrationBuilder, (string From, string To)[] assignees, string fromTag, string toTag)
        {
            var xml = "\"BPMN_XML\"";
            foreach (var (from, to) in assignees)
                xml = $"replace({xml}, 'finova:assignee=\"{from}\"', 'finova:assignee=\"{to}\"')";
            xml = $"replace({xml}, '{fromTag}', '{toTag}')";

            migrationBuilder.Sql($"""
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = {xml}, "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                WHERE "WORKFLOW_CODE" = 'SCR_DECISION_APPROVAL';
                UPDATE "WFE_DEFINITION_VERSIONS" SET "BPMN_XML" = {xml}, "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                WHERE "WORKFLOW_DEFINITION_ID" IN (SELECT "WORKFLOW_DEFINITION_ID" FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'SCR_DECISION_APPROVAL');
                """);
            foreach (var (from, to) in assignees)
            {
                migrationBuilder.Sql($"""
                    UPDATE "WFE_TASKS" SET "ASSIGNEE" = '{to}', "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                    WHERE upper("ASSIGNMENT_TYPE") = 'ROLE' AND "ASSIGNEE" = '{from}';
                    UPDATE "WFE_INBOX" SET "ASSIGNED_ROLE" = '{to}', "UPDATED_BY" = 'SYSTEM', "UPDATED_AT" = now()
                    WHERE "ASSIGNED_ROLE" = '{from}';
                    """);
            }
        }
    }
}
