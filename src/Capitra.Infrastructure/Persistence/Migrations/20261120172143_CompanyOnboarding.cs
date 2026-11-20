using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompanyOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SCR_COMPANY_LICENCES",
                columns: table => new
                {
                    LICENCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    MAX_USERS = table.Column<int>(type: "integer", nullable: false),
                    VALID_FROM = table.Column<DateOnly>(type: "date", nullable: false),
                    VALID_TO = table.Column<DateOnly>(type: "date", nullable: true),
                    YEARLY_INQUIRY_QUOTA = table.Column<int>(type: "integer", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_COMPANY_LICENCES", x => x.LICENCE_ID);
                    table.ForeignKey(
                        name: "FK_SCR_COMPANY_LICENCES_SCR_MEMBER_COMPANIES",
                        column: x => x.MEMBER_COMPANY_ID,
                        principalTable: "SCR_MEMBER_COMPANIES",
                        principalColumn: "MEMBER_COMPANY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SEC_EMAIL_LOGS",
                columns: table => new
                {
                    EMAIL_LOG_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_ID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    TO_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    KIND_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBJECT = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_TEXT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ATTEMPTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_EMAIL_LOGS", x => x.EMAIL_LOG_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_COMPANY_LICENCES_MEMBER_COMPANY_ID",
                table: "SCR_COMPANY_LICENCES",
                column: "MEMBER_COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_EMAIL_LOGS_USER_ID",
                table: "SEC_EMAIL_LOGS",
                column: "USER_ID");

            // Every existing company gets an opening licence, so nobody loses access on upgrade: its users in use today (active,
            // not the company administrator), from the day it was created, open-ended and without an inquiry quota. Nothing is deleted.
            migrationBuilder.Sql("""
                INSERT INTO "SCR_COMPANY_LICENCES"
                    ("MEMBER_COMPANY_ID", "MAX_USERS", "VALID_FROM", "VALID_TO", "YEARLY_INQUIRY_QUOTA", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT c."MEMBER_COMPANY_ID",
                       (SELECT COUNT(DISTINCT u."USER_ID")::int
                          FROM "SEC_USERS" u
                         WHERE u."MEMBER_COMPANY_ID" = c."MEMBER_COMPANY_ID" AND u."IS_ACTIVE"
                           AND NOT EXISTS (SELECT 1 FROM "SEC_USER_ROLES" ur JOIN "SEC_ROLES" r ON r."ROLE_ID" = ur."ROLE_ID"
                                            WHERE ur."USER_ID" = u."USER_ID" AND r."NORMALIZED_ROLE_NAME" = 'MEMBER_COMPANY_ADMIN')),
                       c."CREATED_AT"::date, NULL, NULL, 'Opening licence: the users the company had when licences were introduced.',
                       'SYSTEM', now(), 0
                  FROM "SCR_MEMBER_COMPANIES" c
                 WHERE NOT EXISTS (SELECT 1 FROM "SCR_COMPANY_LICENCES" l WHERE l."MEMBER_COMPANY_ID" = c."MEMBER_COMPANY_ID");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SCR_COMPANY_LICENCES");

            migrationBuilder.DropTable(
                name: "SEC_EMAIL_LOGS");
        }
    }
}
