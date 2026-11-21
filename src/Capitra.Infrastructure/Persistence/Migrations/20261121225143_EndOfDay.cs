using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EndOfDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EOD_DAYS",
                columns: table => new
                {
                    DAY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BUSINESS_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FINISHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RUN_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOD_DAYS", x => x.DAY_ID);
                });

            migrationBuilder.CreateTable(
                name: "EOD_JOBS",
                columns: table => new
                {
                    JOB_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOB_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MODULE_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOD_JOBS", x => x.JOB_ID);
                });

            migrationBuilder.CreateTable(
                name: "EOD_DAY_JOBS",
                columns: table => new
                {
                    DAY_JOB_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DAY_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOB_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ITEMS_COUNT = table.Column<int>(type: "integer", nullable: false),
                    SUMMARY = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ATTEMPTS = table.Column<int>(type: "integer", nullable: false),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FINISHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RUN_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOD_DAY_JOBS", x => x.DAY_JOB_ID);
                    table.ForeignKey(
                        name: "FK_EOD_DAY_JOBS_EOD_DAYS",
                        column: x => x.DAY_ID,
                        principalTable: "EOD_DAYS",
                        principalColumn: "DAY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EOD_JOB_PARAMETERS",
                columns: table => new
                {
                    JOB_PARAMETER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOB_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARAMETER_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PARAMETER_VALUE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOD_JOB_PARAMETERS", x => x.JOB_PARAMETER_ID);
                    table.ForeignKey(
                        name: "FK_EOD_JOB_PARAMETERS_EOD_JOBS",
                        column: x => x.JOB_ID,
                        principalTable: "EOD_JOBS",
                        principalColumn: "JOB_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_EOD_DAY_JOBS_DAY_ID_JOB_CODE",
                table: "EOD_DAY_JOBS",
                columns: new[] { "DAY_ID", "JOB_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EOD_DAYS_BUSINESS_DATE",
                table: "EOD_DAYS",
                column: "BUSINESS_DATE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EOD_JOB_PARAMETERS_JOB_ID_PARAMETER_CODE",
                table: "EOD_JOB_PARAMETERS",
                columns: new[] { "JOB_ID", "PARAMETER_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EOD_JOBS_JOB_CODE",
                table: "EOD_JOBS",
                column: "JOB_CODE",
                unique: true);

            // the jobs of the night, in order (what each does is in the code; new jobs add their row themselves)
            migrationBuilder.Sql("""
                INSERT INTO "EOD_JOBS" ("JOB_CODE", "MODULE_CODE", "NAME_EN", "NAME_AR", "SEQUENCE_NO", "IS_ENABLED", "CREATED_BY", "CREATED_AT", "ROW_VERSION") VALUES
                ('LND_DAILY', 'LND', 'Retail Lending: daily accrual and arrears', 'التمويل الاستهلاكي: الاستحقاق اليومي والمتأخرات', 10, true, 'MIGRATION', now(), 0),
                ('SCR_LICENCE_EXPIRY', 'SCR', 'Screening: licence expiry notices', 'الفحص: تنبيهات انتهاء الترخيص', 20, true, 'MIGRATION', now(), 0),
                ('SCR_INVOICES', 'SCR', 'Screening: period-end invoices', 'الفحص: فواتير نهاية الفترة', 30, true, 'MIGRATION', now(), 0),
                ('SCR_HOUSEKEEPING', 'SCR', 'Screening: sign-in housekeeping', 'الفحص: تنظيم سجل الدخول', 40, true, 'MIGRATION', now(), 0)
                ON CONFLICT ("JOB_CODE") DO NOTHING;
                """);

            // Retail Lending's end of day history becomes the End of Day's: each lending day done is a day done, with its lending job
            migrationBuilder.Sql("""
                INSERT INTO "EOD_DAYS" ("BUSINESS_DATE", "STATUS_CODE", "STARTED_AT", "FINISHED_AT", "RUN_BY", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT r."RUN_DATE", 'DONE', r."STARTED_AT", r."FINISHED_AT", r."CREATED_BY", 'MIGRATION', now(), 0
                FROM "LND_EOD_RUNS" r
                WHERE r."STATUS_CODE" = 'DONE'
                  AND NOT EXISTS (SELECT 1 FROM "EOD_DAYS" d WHERE d."BUSINESS_DATE" = r."RUN_DATE");

                INSERT INTO "EOD_DAY_JOBS" ("DAY_ID", "JOB_CODE", "SEQUENCE_NO", "STATUS_CODE", "ITEMS_COUNT", "SUMMARY", "ATTEMPTS", "STARTED_AT", "FINISHED_AT", "RUN_BY", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT d."DAY_ID", 'LND_DAILY', 10, 'DONE', r."LOANS_COUNT",
                       r."LOANS_COUNT" || ' finances: interest accrued ' || to_char(r."INTEREST_ACCRUED_AMOUNT", 'FM999,999,999,990.00')
                         || ', late fees ' || to_char(r."LATE_FEE_AMOUNT", 'FM999,999,999,990.00') || ' (run in Retail Lending before the End of Day application).',
                       1, r."STARTED_AT", r."FINISHED_AT", r."CREATED_BY", 'MIGRATION', now(), 0
                FROM "LND_EOD_RUNS" r
                JOIN "EOD_DAYS" d ON d."BUSINESS_DATE" = r."RUN_DATE"
                WHERE r."STATUS_CODE" = 'DONE'
                  AND NOT EXISTS (SELECT 1 FROM "EOD_DAY_JOBS" j WHERE j."DAY_ID" = d."DAY_ID" AND j."JOB_CODE" = 'LND_DAILY');
                """);

            // roles that ran Retail Lending's end of day run the End of Day application
            migrationBuilder.Sql("""
                INSERT INTO "SEC_ROLE_CLAIMS" ("ROLE_ID", "CLAIM_TYPE", "CLAIM_VALUE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT DISTINCT c."ROLE_ID", 'permission', p.code, 'MIGRATION', now(), 0
                FROM "SEC_ROLE_CLAIMS" c
                CROSS JOIN (VALUES ('EOD.VIEW'), ('EOD.RUN'), ('EOD.JOBS.EDIT')) AS p(code)
                WHERE c."CLAIM_TYPE" = 'permission' AND c."CLAIM_VALUE" = 'LND.EOD.RUN'
                  AND NOT EXISTS (SELECT 1 FROM "SEC_ROLE_CLAIMS" x WHERE x."ROLE_ID" = c."ROLE_ID" AND x."CLAIM_TYPE" = 'permission' AND x."CLAIM_VALUE" = p.code);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EOD_DAY_JOBS");

            migrationBuilder.DropTable(
                name: "EOD_JOB_PARAMETERS");

            migrationBuilder.DropTable(
                name: "EOD_DAYS");

            migrationBuilder.DropTable(
                name: "EOD_JOBS");
        }
    }
}
