using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlAccountingPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GL_CALENDARS",
                columns: table => new
                {
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CALENDAR_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CALENDAR_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CALENDAR_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CALENDAR_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PERIOD_FREQUENCY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_ADJUSTMENT_PERIOD_USED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CALENDARS", x => x.CALENDAR_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_FISCAL_YEARS",
                columns: table => new
                {
                    FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    FISCAL_YEAR = table.Column<int>(type: "integer", nullable: false),
                    YEAR_NAME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    YEAR_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_FISCAL_YEARS", x => x.FISCAL_YEAR_ID);
                    table.ForeignKey(
                        name: "FK_GL_FISCAL_YEARS_GL_CALENDARS",
                        column: x => x.CALENDAR_ID,
                        principalTable: "GL_CALENDARS",
                        principalColumn: "CALENDAR_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGERS",
                columns: table => new
                {
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEGAL_ENTITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LEDGER_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LEDGER_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_CURRENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    COA_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_PRIMARY = table.Column<bool>(type: "boolean", nullable: false),
                    LEDGER_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RETAINED_EARNINGS_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUSPENSE_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROUNDING_DIFF_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FX_GAIN_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FX_LOSS_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OPENING_BALANCE_ACCT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_LEDGERS", x => x.LEDGER_ID);
                    table.ForeignKey(
                        name: "FK_GL_LEDGERS_ADM_LKP_CURRENCIES",
                        column: x => x.LEDGER_CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCIES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGERS_ADM_LKP_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_CODE,
                        principalTable: "ADM_LKP_LEGAL_ENTITIES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGERS_GL_CALENDARS",
                        column: x => x.CALENDAR_ID,
                        principalTable: "GL_CALENDARS",
                        principalColumn: "CALENDAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGERS_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_PERIODS",
                columns: table => new
                {
                    PERIOD_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERIOD_NO = table.Column<int>(type: "integer", nullable: false),
                    PERIOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PERIOD_NAME_EN = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PERIOD_NAME_AR = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    QUARTER_NO = table.Column<int>(type: "integer", nullable: true),
                    IS_ADJUSTMENT_PERIOD = table.Column<bool>(type: "boolean", nullable: false),
                    DEFAULT_PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_PERIODS", x => x.PERIOD_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_PERIODS_GL_CALENDARS",
                        column: x => x.CALENDAR_ID,
                        principalTable: "GL_CALENDARS",
                        principalColumn: "CALENDAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_PERIODS_GL_FISCAL_YEARS",
                        column: x => x.FISCAL_YEAR_ID,
                        principalTable: "GL_FISCAL_YEARS",
                        principalColumn: "FISCAL_YEAR_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGER_PERIODS",
                columns: table => new
                {
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_MANUAL_JOURNAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_SUBLEDGER_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REVERSAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ADJUSTMENT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    OPENED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OPENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLOSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CLOSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PERMANENTLY_CLOSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PERMANENTLY_CLOSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLOSE_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_LEDGER_PERIODS", x => x.LEDGER_PERIOD_ID);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_PERIODS_GL_ACCOUNTING_PERIODS",
                        column: x => x.PERIOD_ID,
                        principalTable: "GL_ACCOUNTING_PERIODS",
                        principalColumn: "PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_PERIODS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGER_PERIODS_HIST",
                columns: table => new
                {
                    LEDGER_PERIOD_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    OLD_PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NEW_PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_LEDGER_PERIODS_HIST", x => x.LEDGER_PERIOD_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_PERIODS_HIST_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_PERIODS_CALENDAR_ID_START_DATE",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "CALENDAR_ID", "START_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_PERIODS_FISCAL_YEAR_ID",
                table: "GL_ACCOUNTING_PERIODS",
                column: "FISCAL_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_PERIODS_CALENDAR_ID_PERIOD_CODE",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "CALENDAR_ID", "PERIOD_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CALENDARS_CALENDAR_CODE",
                table: "GL_CALENDARS",
                column: "CALENDAR_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_FISCAL_YEARS_CALENDAR_ID_FISCAL_YEAR",
                table: "GL_FISCAL_YEARS",
                columns: new[] { "CALENDAR_ID", "FISCAL_YEAR" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_PERIODS_PERIOD_ID",
                table: "GL_LEDGER_PERIODS",
                column: "PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGER_PERIODS_LEDGER_ID_PERIOD_ID",
                table: "GL_LEDGER_PERIODS",
                columns: new[] { "LEDGER_ID", "PERIOD_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_PERIODS_HIST_LEDGER_PERIOD_ID",
                table: "GL_LEDGER_PERIODS_HIST",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGERS_CALENDAR_ID",
                table: "GL_LEDGERS",
                column: "CALENDAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGERS_COA_ID",
                table: "GL_LEDGERS",
                column: "COA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGERS_LEDGER_CURRENCY_CODE",
                table: "GL_LEDGERS",
                column: "LEDGER_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGERS_LEGAL_ENTITY_CODE",
                table: "GL_LEDGERS",
                column: "LEGAL_ENTITY_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGERS_LEDGER_CODE",
                table: "GL_LEDGERS",
                column: "LEDGER_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GL_LEDGER_PERIODS_HIST");

            migrationBuilder.DropTable(
                name: "GL_LEDGER_PERIODS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_PERIODS");

            migrationBuilder.DropTable(
                name: "GL_LEDGERS");

            migrationBuilder.DropTable(
                name: "GL_FISCAL_YEARS");

            migrationBuilder.DropTable(
                name: "GL_CALENDARS");
        }
    }
}
