using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer Finance supplier onboarding (Ahmed, 25 Nov 2026).
    /// <list type="bullet">
    /// <item>CRM_CLIENT_ROLES.BUSINESS_LINE_CODE: a SUPPLIER role is for FACTORING or CONSUMER_FINANCE; the suppliers so far are
    /// factoring suppliers. One role of a kind per customer and business line.</item>
    /// <item>CRM_CF_SUPPLIER_APPLICATIONS (with its parties, documents and step notes): the supplier's application and agreement.</item>
    /// <item>CF_SUPPLIER_APPROVAL: document check, compliance, site visit, risk review, manager approval, committee approval (HIGH
    /// risk only, every member), contract signing. A return (REJECT) ends the run; submitting again starts a new one.</item>
    /// <item>Roles CFS_SALES ... CFS_LEGAL with their permissions; users are added to them in Security.</item>
    /// </list>
    /// </summary>
    public partial class CfSupplierOnboarding : Migration
    {
        private static readonly (string Role, string Description, string[] Permissions)[] Roles =
        [
            ("CFS_SALES", "Consumer Finance suppliers: enters supplier applications and agreement terms, and submits them", ["CRM.VIEW", "CRM.EDIT", "CRM.CF_SUPPLIER.VIEW", "CRM.CF_SUPPLIER.EDIT"]),
            ("CFS_OPERATIONS", "Consumer Finance suppliers: document check (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_COMPLIANCE", "Consumer Finance suppliers: AML screening of owners and signatories, risk rating (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_SITE_VISIT", "Consumer Finance suppliers: site visit and its report (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_RISK", "Consumer Finance suppliers: risk review (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_MANAGER", "Consumer Finance suppliers: manager approval (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_COMMITTEE", "Consumer Finance suppliers: committee approval of HIGH risk suppliers, every member (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
            ("CFS_LEGAL", "Consumer Finance suppliers: contract signing, which activates the supplier (workflow step)", ["CRM.VIEW", "CRM.CF_SUPPLIER.VIEW"]),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BUSINESS_LINE_CODE",
                table: "CRM_CLIENT_ROLES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.Sql("""UPDATE "CRM_CLIENT_ROLES" SET "BUSINESS_LINE_CODE" = 'FACTORING' WHERE "ROLE_CODE" = 'SUPPLIER';""");

            migrationBuilder.DropIndex(
                name: "UQ_CRM_CLIENT_ROLES_CLIENT_ID_ROLE_CODE",
                table: "CRM_CLIENT_ROLES");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CLIENT_ROLES_CLIENT_ID_ROLE_CODE_BUSINESS_LINE_CODE",
                table: "CRM_CLIENT_ROLES",
                columns: new[] { "CLIENT_ID", "ROLE_CODE", "BUSINESS_LINE_CODE" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_APPLICATIONS",
                columns: table => new
                {
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACTIVATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPLICATION_REF = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CLIENT_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLIENT_ROLE_ID = table.Column<long>(type: "bigint", nullable: false),
                    COMMISSION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    DELIVERY_DAYS = table.Column<int>(type: "integer", nullable: false),
                    DOWN_PAYMENT_COLLECTOR_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ENTERED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ENTERED_BY_PERSON = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    GOODS_CATEGORY_CODES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MIN_DOWN_PAYMENT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PAYMENT_DAYS = table.Column<int>(type: "integer", nullable: false),
                    QUOTATION_VALIDITY_DAYS = table.Column<int>(type: "integer", nullable: false),
                    RETURN_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RISK_RATING_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SITE_VISIT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VALID_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    VALID_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_APPLICATIONS", x => x.APPLICATION_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_APPLICATIONS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_APPLICATIONS_CRM_CLIENT_ROLES",
                        column: x => x.CLIENT_ROLE_ID,
                        principalTable: "CRM_CLIENT_ROLES",
                        principalColumn: "CLIENT_ROLE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_PARTIES",
                columns: table => new
                {
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NATIONAL_ID_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NATIONALITY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OWNERSHIP_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PARTY_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SCREENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SCREENED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SCREENING_RESULT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_PARTIES", x => x.PARTY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_PARTIES_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    DOCUMENT_TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FILE_SIZE_BYTES = table.Column<long>(type: "bigint", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SHA256_HASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_DOCUMENTS_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_STEP_NOTES",
                columns: table => new
                {
                    STEP_NOTE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    COMMENT_TEXT = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DONE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DONE_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    STEP_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_STEP_NOTES", x => x.STEP_NOTE_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_STEP_NOTES_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CF_SUPPLIER_APPLICATIONS_APPLICATION_REF",
                table: "CRM_CF_SUPPLIER_APPLICATIONS",
                column: "APPLICATION_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_APPLICATIONS_CLIENT_ID",
                table: "CRM_CF_SUPPLIER_APPLICATIONS",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_APPLICATIONS_CLIENT_ROLE_ID",
                table: "CRM_CF_SUPPLIER_APPLICATIONS",
                column: "CLIENT_ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_APPLICATIONS_STATUS_CODE",
                table: "CRM_CF_SUPPLIER_APPLICATIONS",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_PARTIES_APPLICATION_ID",
                table: "CRM_CF_SUPPLIER_PARTIES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_DOCUMENTS_APPLICATION_ID_STEP_CODE",
                table: "CRM_CF_SUPPLIER_DOCUMENTS",
                columns: new[] { "APPLICATION_ID", "STEP_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_STEP_NOTES_APPLICATION_ID",
                table: "CRM_CF_SUPPLIER_STEP_NOTES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CF_SUPPLIER_STEP_NOTES_STEP_INSTANCE_ID",
                table: "CRM_CF_SUPPLIER_STEP_NOTES",
                column: "STEP_INSTANCE_ID",
                unique: true);

            migrationBuilder.Sql($"""
                INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
                    ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('CF_SUPPLIER_APPROVAL', 'Consumer Finance Supplier Approval', '1', 'released', '{SupplierApprovalBpmn.Replace("'", "''", StringComparison.Ordinal)}',
                        'A Consumer Finance supplier (merchant) and its agreement: document check, compliance, site visit, risk review, manager approval, committee approval for HIGH risk, contract signing. No step is done by the person who entered it.',
                        TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;
                """);
            foreach (var (role, description, permissions) in Roles)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "WFE_WORKFLOW_DEFINITIONS" WHERE "WORKFLOW_CODE" = 'CF_SUPPLIER_APPROVAL';""");
            var names = string.Join(", ", Roles.Select(r => $"'{r.Role}'"));
            migrationBuilder.Sql($"""
                DELETE FROM "SEC_ROLE_CLAIMS" WHERE "ROLE_ID" IN (SELECT "ROLE_ID" FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names}));
                DELETE FROM "SEC_USER_ROLES" WHERE "ROLE_ID" IN (SELECT "ROLE_ID" FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names}));
                DELETE FROM "SEC_ROLES" WHERE "NORMALIZED_ROLE_NAME" IN ({names});
                """);

            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_DOCUMENTS");
            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_PARTIES");
            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_STEP_NOTES");
            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_APPLICATIONS");

            migrationBuilder.DropIndex(
                name: "UQ_CRM_CLIENT_ROLES_CLIENT_ID_ROLE_CODE_BUSINESS_LINE_CODE",
                table: "CRM_CLIENT_ROLES");

            migrationBuilder.DropColumn(
                name: "BUSINESS_LINE_CODE",
                table: "CRM_CLIENT_ROLES");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CLIENT_ROLES_CLIENT_ID_ROLE_CODE",
                table: "CRM_CLIENT_ROLES",
                columns: new[] { "CLIENT_ID", "ROLE_CODE" },
                unique: true);
        }

        /// <summary>CF_SUPPLIER_APPROVAL: after the manager, a HIGH risk supplier goes to the committee (every member), the others to the contract.</summary>
        public const string SupplierApprovalBpmn = """
            <?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_CFS" targetNamespace="http://finova/workflow">
              <bpmn:process id="Process_CF_SUPPLIER" name="Consumer Finance Supplier Approval" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_ref&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;module_code&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false},{&#34;name&#34;:&#34;risk_rating&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:false}]">
                <bpmn:startEvent id="StartEvent_1" name="Submitted" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:outgoing>Flow_Start</bpmn:outgoing>
                </bpmn:startEvent>
                <bpmn:userTask id="Task_Docs" name="Document check" finova:stepCode="CFS_DOCUMENT_CHECK" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_OPERATIONS" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Start</bpmn:incoming>
                  <bpmn:outgoing>Flow_Docs</bpmn:outgoing>
                  <bpmn:outgoing>Flow_DocsReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Compliance" name="Compliance (AML)" finova:stepCode="CFS_COMPLIANCE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_COMPLIANCE" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Docs</bpmn:incoming>
                  <bpmn:outgoing>Flow_Compliance</bpmn:outgoing>
                  <bpmn:outgoing>Flow_ComplianceReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_SiteVisit" name="Site visit" finova:stepCode="CFS_SITE_VISIT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_SITE_VISIT" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Compliance</bpmn:incoming>
                  <bpmn:outgoing>Flow_Visit</bpmn:outgoing>
                  <bpmn:outgoing>Flow_SiteVisitReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Risk" name="Risk review" finova:stepCode="CFS_RISK_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_RISK" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Visit</bpmn:incoming>
                  <bpmn:outgoing>Flow_Risk</bpmn:outgoing>
                  <bpmn:outgoing>Flow_RiskReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Manager" name="Manager approval" finova:stepCode="CFS_MANAGER_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_MANAGER" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Risk</bpmn:incoming>
                  <bpmn:outgoing>Flow_ManagerHigh</bpmn:outgoing>
                  <bpmn:outgoing>Flow_Manager</bpmn:outgoing>
                  <bpmn:outgoing>Flow_ManagerReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Committee" name="Committee approval" finova:stepCode="CFS_COMMITTEE_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_COMMITTEE" finova:parallelMode="ALL" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_ManagerHigh</bpmn:incoming>
                  <bpmn:outgoing>Flow_Committee</bpmn:outgoing>
                  <bpmn:outgoing>Flow_CommitteeReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:userTask id="Task_Contract" name="Contract signing" finova:stepCode="CFS_CONTRACT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CFS_LEGAL" finova:route="crm-cf-supplier" finova:routeParameters="[{&#34;name&#34;:&#34;applicationId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
                  <bpmn:incoming>Flow_Manager</bpmn:incoming>
                  <bpmn:incoming>Flow_Committee</bpmn:incoming>
                  <bpmn:outgoing>Flow_Contract</bpmn:outgoing>
                  <bpmn:outgoing>Flow_ContractReturn</bpmn:outgoing>
                </bpmn:userTask>
                <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
                  <bpmn:incoming>Flow_Contract</bpmn:incoming>
                  <bpmn:incoming>Flow_DocsReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_ComplianceReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_SiteVisitReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_RiskReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_ManagerReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_CommitteeReturn</bpmn:incoming>
                  <bpmn:incoming>Flow_ContractReturn</bpmn:incoming>
                </bpmn:endEvent>
                <bpmn:sequenceFlow id="Flow_Start" name="" sourceRef="StartEvent_1" targetRef="Task_Docs" finova:outcome="" />
                <bpmn:sequenceFlow id="Flow_Docs" name="ACCEPT" sourceRef="Task_Docs" targetRef="Task_Compliance" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Compliance" name="ACCEPT" sourceRef="Task_Compliance" targetRef="Task_SiteVisit" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Visit" name="ACCEPT" sourceRef="Task_SiteVisit" targetRef="Task_Risk" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Risk" name="ACCEPT" sourceRef="Task_Risk" targetRef="Task_Manager" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_ManagerHigh" name="ACCEPT" sourceRef="Task_Manager" targetRef="Task_Committee" finova:outcome="ACCEPT" finova:conditions="[{&#34;field&#34;:&#34;risk_rating&#34;,&#34;op&#34;:&#34;=&#34;,&#34;value&#34;:&#34;HIGH&#34;}]" />
                <bpmn:sequenceFlow id="Flow_Manager" name="ACCEPT" sourceRef="Task_Manager" targetRef="Task_Contract" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Committee" name="ACCEPT" sourceRef="Task_Committee" targetRef="Task_Contract" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_Contract" name="ACCEPT" sourceRef="Task_Contract" targetRef="EndEvent_1" finova:outcome="ACCEPT" />
                <bpmn:sequenceFlow id="Flow_DocsReturn" name="REJECT" sourceRef="Task_Docs" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_ComplianceReturn" name="REJECT" sourceRef="Task_Compliance" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_SiteVisitReturn" name="REJECT" sourceRef="Task_SiteVisit" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_RiskReturn" name="REJECT" sourceRef="Task_Risk" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_ManagerReturn" name="REJECT" sourceRef="Task_Manager" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_CommitteeReturn" name="REJECT" sourceRef="Task_Committee" targetRef="EndEvent_1" finova:outcome="REJECT" />
                <bpmn:sequenceFlow id="Flow_ContractReturn" name="REJECT" sourceRef="Task_Contract" targetRef="EndEvent_1" finova:outcome="REJECT" />
              </bpmn:process>
              <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_CF_SUPPLIER">
                  <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
                    <dc:Bounds x="100" y="182" width="36" height="36" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Docs_di" bpmnElement="Task_Docs">
                    <dc:Bounds x="180" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Compliance_di" bpmnElement="Task_Compliance">
                    <dc:Bounds x="360" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_SiteVisit_di" bpmnElement="Task_SiteVisit">
                    <dc:Bounds x="540" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Risk_di" bpmnElement="Task_Risk">
                    <dc:Bounds x="720" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Manager_di" bpmnElement="Task_Manager">
                    <dc:Bounds x="900" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Committee_di" bpmnElement="Task_Committee">
                    <dc:Bounds x="990" y="320" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="Task_Contract_di" bpmnElement="Task_Contract">
                    <dc:Bounds x="1080" y="160" width="140" height="80" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
                    <dc:Bounds x="1280" y="182" width="36" height="36" />
                  </bpmndi:BPMNShape>
                  <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start">
                    <di:waypoint x="136" y="200" />
                    <di:waypoint x="180" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Docs_di" bpmnElement="Flow_Docs">
                    <di:waypoint x="320" y="200" />
                    <di:waypoint x="360" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Compliance_di" bpmnElement="Flow_Compliance">
                    <di:waypoint x="500" y="200" />
                    <di:waypoint x="540" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Visit_di" bpmnElement="Flow_Visit">
                    <di:waypoint x="680" y="200" />
                    <di:waypoint x="720" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Risk_di" bpmnElement="Flow_Risk">
                    <di:waypoint x="860" y="200" />
                    <di:waypoint x="900" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ManagerHigh_di" bpmnElement="Flow_ManagerHigh">
                    <di:waypoint x="970" y="240" />
                    <di:waypoint x="970" y="300" />
                    <di:waypoint x="1060" y="300" />
                    <di:waypoint x="1060" y="320" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Manager_di" bpmnElement="Flow_Manager">
                    <di:waypoint x="1040" y="200" />
                    <di:waypoint x="1080" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Committee_di" bpmnElement="Flow_Committee">
                    <di:waypoint x="1130" y="360" />
                    <di:waypoint x="1150" y="360" />
                    <di:waypoint x="1150" y="240" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_Contract_di" bpmnElement="Flow_Contract">
                    <di:waypoint x="1220" y="200" />
                    <di:waypoint x="1280" y="200" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_DocsReturn_di" bpmnElement="Flow_DocsReturn">
                    <di:waypoint x="250" y="160" />
                    <di:waypoint x="250" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ComplianceReturn_di" bpmnElement="Flow_ComplianceReturn">
                    <di:waypoint x="430" y="160" />
                    <di:waypoint x="430" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_SiteVisitReturn_di" bpmnElement="Flow_SiteVisitReturn">
                    <di:waypoint x="610" y="160" />
                    <di:waypoint x="610" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_RiskReturn_di" bpmnElement="Flow_RiskReturn">
                    <di:waypoint x="790" y="160" />
                    <di:waypoint x="790" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ManagerReturn_di" bpmnElement="Flow_ManagerReturn">
                    <di:waypoint x="970" y="160" />
                    <di:waypoint x="970" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_CommitteeReturn_di" bpmnElement="Flow_CommitteeReturn">
                    <di:waypoint x="1060" y="320" />
                    <di:waypoint x="1060" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                  <bpmndi:BPMNEdge id="Flow_ContractReturn_di" bpmnElement="Flow_ContractReturn">
                    <di:waypoint x="1150" y="160" />
                    <di:waypoint x="1150" y="100" />
                    <di:waypoint x="1298" y="100" />
                    <di:waypoint x="1298" y="182" />
                  </bpmndi:BPMNEdge>
                </bpmndi:BPMNPlane>
              </bpmndi:BPMNDiagram>
            </bpmn:definitions>
            """;
    }
}
