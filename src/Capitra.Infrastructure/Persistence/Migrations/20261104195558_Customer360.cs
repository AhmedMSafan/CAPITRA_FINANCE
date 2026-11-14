using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Customer360 : Migration
    {
        internal const string CarryOverRolesSql = """
            INSERT INTO "CRM_CLIENT_ROLES" ("CLIENT_ID", "ROLE_CODE", "STATUS_CODE", "ACTIVE_SINCE_DATE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
            SELECT r."CLIENT_ID", r."ROLE_CODE", 'ACTIVE', CURRENT_DATE, 'Carried over', 'MIGRATION', now(), 0
            FROM (
                SELECT "CLIENT_ID", upper("CLIENT_ROLE_CODE") AS "ROLE_CODE" FROM "CRM_CLIENTS" WHERE "CLIENT_ROLE_CODE" IS NOT NULL
                UNION SELECT "CLIENT_ID", 'SUPPLIER' FROM "CRM_CLIENTS" WHERE "IS_SUPPLIER"
                UNION SELECT DISTINCT a."CLIENT_ID", 'CLIENT' FROM "FCT_AGREEMENTS" a JOIN "CRM_CLIENTS" c ON c."CLIENT_ID" = a."CLIENT_ID"
            ) r
            ON CONFLICT ("CLIENT_ID", "ROLE_CODE") DO NOTHING;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "COMPLIANCE_CHANGED_AT",
                table: "CRM_CLIENTS",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "COMPLIANCE_CHANGED_BY",
                table: "CRM_CLIENTS",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "COMPLIANCE_VERIFIED_AT",
                table: "CRM_CLIENTS",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "COMPLIANCE_VERIFIED_BY",
                table: "CRM_CLIENTS",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IS_PEP",
                table: "CRM_CLIENTS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SANCTIONS_STATUS",
                table: "CRM_CLIENTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CRM_CLIENT_BANK_ACCOUNTS",
                columns: table => new
                {
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    BANK_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BRANCH_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SWIFT_CODE = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    ACCOUNT_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IBAN = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    ACCOUNT_HOLDER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ACCOUNT_HOLDER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_PRIMARY = table.Column<bool>(type: "boolean", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CHANGED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VERIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VERIFICATION_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENT_BANK_ACCOUNTS", x => x.BANK_ACCOUNT_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_BANK_ACCOUNTS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_BANK_ACCOUNTS_CRM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_ID,
                        principalTable: "CRM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CLIENT_COMPLIANCE_HISTORY",
                columns: table => new
                {
                    COMPLIANCE_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RISK_GRADE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    KYC_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    KYC_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    AML_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AML_CHECK_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SANCTIONS_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_PEP = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BLACKLISTED = table.Column<bool>(type: "boolean", nullable: false),
                    BLACKLIST_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NEXT_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENT_COMPLIANCE_HISTORY", x => x.COMPLIANCE_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_COMPLIANCE_HISTORY_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CLIENT_ROLES",
                columns: table => new
                {
                    CLIENT_ROLE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ROLE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ACTIVE_SINCE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENT_ROLES", x => x.CLIENT_ROLE_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_ROLES_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CLIENT_BANK_ACCOUNT_HISTORY",
                columns: table => new
                {
                    BANK_ACCOUNT_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BANK_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BRANCH_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SWIFT_CODE = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    ACCOUNT_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IBAN = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    ACCOUNT_HOLDER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_PRIMARY = table.Column<bool>(type: "boolean", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VERIFICATION_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENT_BANK_ACCOUNT_HISTORY", x => x.BANK_ACCOUNT_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_BANK_ACCOUNT_HISTORY_CRM_CLIENT_BANK_ACCOUNTS",
                        column: x => x.BANK_ACCOUNT_ID,
                        principalTable: "CRM_CLIENT_BANK_ACCOUNTS",
                        principalColumn: "BANK_ACCOUNT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_BANK_ACCOUNT_HISTORY_BANK_ACCOUNT_ID",
                table: "CRM_CLIENT_BANK_ACCOUNT_HISTORY",
                column: "BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_BANK_ACCOUNT_HISTORY_CLIENT_ID",
                table: "CRM_CLIENT_BANK_ACCOUNT_HISTORY",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_BANK_ACCOUNTS_CLIENT_ID_CURRENCY_ID",
                table: "CRM_CLIENT_BANK_ACCOUNTS",
                columns: new[] { "CLIENT_ID", "CURRENCY_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_BANK_ACCOUNTS_CURRENCY_ID",
                table: "CRM_CLIENT_BANK_ACCOUNTS",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_BANK_ACCOUNTS_IBAN",
                table: "CRM_CLIENT_BANK_ACCOUNTS",
                column: "IBAN");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_COMPLIANCE_HISTORY_CLIENT_ID",
                table: "CRM_CLIENT_COMPLIANCE_HISTORY",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_ROLES_ROLE_CODE_STATUS_CODE",
                table: "CRM_CLIENT_ROLES",
                columns: new[] { "ROLE_CODE", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CLIENT_ROLES_CLIENT_ID_ROLE_CODE",
                table: "CRM_CLIENT_ROLES",
                columns: new[] { "CLIENT_ID", "ROLE_CODE" },
                unique: true);

            // the roles the customers already had move to CRM_CLIENT_ROLES, ACTIVE as carried over: the CRM role code (developer,
            // investor), the supplier flag, and CLIENT for every customer with a factoring agreement
            migrationBuilder.Sql(CarryOverRolesSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CRM_CLIENT_BANK_ACCOUNT_HISTORY");

            migrationBuilder.DropTable(
                name: "CRM_CLIENT_COMPLIANCE_HISTORY");

            migrationBuilder.DropTable(
                name: "CRM_CLIENT_ROLES");

            migrationBuilder.DropTable(
                name: "CRM_CLIENT_BANK_ACCOUNTS");

            migrationBuilder.DropColumn(
                name: "COMPLIANCE_CHANGED_AT",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "COMPLIANCE_CHANGED_BY",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "COMPLIANCE_VERIFIED_AT",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "COMPLIANCE_VERIFIED_BY",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "IS_PEP",
                table: "CRM_CLIENTS");

            migrationBuilder.DropColumn(
                name: "SANCTIONS_STATUS",
                table: "CRM_CLIENTS");
        }
    }
}
