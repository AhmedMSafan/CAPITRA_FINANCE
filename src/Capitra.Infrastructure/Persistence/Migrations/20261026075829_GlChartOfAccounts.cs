using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlChartOfAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_LKP_BUSINESS_LINES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_BUSINESS_LINES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_LEGAL_ENTITIES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BASE_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    COUNTRY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_LEGAL_ENTITIES", x => x.CODE);
                    table.ForeignKey(
                        name: "FK_ADM_LKP_LEGAL_ENTITIES_ADM_LKP_COUNTRIES",
                        column: x => x.COUNTRY_CODE,
                        principalTable: "ADM_LKP_COUNTRIES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_LKP_LEGAL_ENTITIES_ADM_LKP_CURRENCIES",
                        column: x => x.BASE_CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCIES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_COA_STRUCTURES",
                columns: table => new
                {
                    COA_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COA_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    COA_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DELIMITER_CHAR = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SEGMENT_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: false),
                    BALANCING_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    BRANCH_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    COST_CENTER_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    BUSINESS_LINE_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    INTERCOMPANY_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    IS_DYNAMIC_COMBINATION_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_COMBINATION_APPROVAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_COA_STRUCTURES", x => x.COA_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_COA_SEGMENTS",
                columns: table => new
                {
                    COA_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    SEGMENT_NUM = table.Column<int>(type: "integer", nullable: false),
                    SEGMENT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEGMENT_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SEGMENT_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SEGMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MIN_LENGTH = table.Column<int>(type: "integer", nullable: false),
                    MAX_LENGTH = table.Column<int>(type: "integer", nullable: false),
                    VALUE_FORMAT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAD_CHAR = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    IS_UPPERCASE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_SUMMARY_VALUE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DEFAULT_VALUE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DEFAULT_MODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_COA_SEGMENTS", x => x.COA_SEGMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENTS_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_COA_SEGMENT_VALUES",
                columns: table => new
                {
                    SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    VALUE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALUE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    VALUE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PARENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_SUMMARY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_POSTING_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    REFERENCE_LEGAL_ENTITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    REFERENCE_BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_COA_SEGMENT_VALUES", x => x.SEGMENT_VALUE_ID);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_ADM_LKP_BUSINESS_LINES",
                        column: x => x.REFERENCE_BUSINESS_LINE_CODE,
                        principalTable: "ADM_LKP_BUSINESS_LINES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_ADM_LKP_LEGAL_ENTITIES",
                        column: x => x.REFERENCE_LEGAL_ENTITY_CODE,
                        principalTable: "ADM_LKP_LEGAL_ENTITIES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_GL_COA_SEGMENTS",
                        column: x => x.COA_SEGMENT_ID,
                        principalTable: "GL_COA_SEGMENTS",
                        principalColumn: "COA_SEGMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_GL_COA_SEGMENT_VALUES",
                        column: x => x.PARENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTS",
                columns: table => new
                {
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNT_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    ACCOUNT_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PARENT_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NORMAL_BALANCE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    FINANCIAL_STATEMENT_CLASS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CASH_FLOW_CLASS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_SUMMARY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_POSTING_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_CONTROL_ACCOUNT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_RECONCILIATION = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BRANCH_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_COST_CENTER_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BUSINESS_LINE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_JOURNAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_SUBLEDGER_POSTING_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_CURRENCY_REVALUATION = table.Column<bool>(type: "boolean", nullable: false),
                    IS_RETAINED_EARNINGS = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTS", x => x.ACCOUNT_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTS_GL_ACCOUNTS",
                        column: x => x.PARENT_ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTS_GL_COA_SEGMENT_VALUES",
                        column: x => x.ACCOUNT_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTS_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_LKP_LEGAL_ENTITIES_BASE_CURRENCY_CODE",
                table: "ADM_LKP_LEGAL_ENTITIES",
                column: "BASE_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_LKP_LEGAL_ENTITIES_COUNTRY_CODE",
                table: "ADM_LKP_LEGAL_ENTITIES",
                column: "COUNTRY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTS_ACCOUNT_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNTS",
                column: "ACCOUNT_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTS_PARENT_ACCOUNT_ID",
                table: "GL_ACCOUNTS",
                column: "PARENT_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTS_COA_ID_ACCOUNT_CODE",
                table: "GL_ACCOUNTS",
                columns: new[] { "COA_ID", "ACCOUNT_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_PARENT_VALUE_ID",
                table: "GL_COA_SEGMENT_VALUES",
                column: "PARENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_BUSINESS_LINE_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_BUSINESS_LINE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_LEGAL_ENTITY_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_LEGAL_ENTITY_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_SEGMENT_VALUES_COA_SEGMENT_ID_VALUE_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                columns: new[] { "COA_SEGMENT_ID", "VALUE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_SEGMENTS_COA_ID_SEGMENT_CODE",
                table: "GL_COA_SEGMENTS",
                columns: new[] { "COA_ID", "SEGMENT_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_SEGMENTS_COA_ID_SEGMENT_NUM",
                table: "GL_COA_SEGMENTS",
                columns: new[] { "COA_ID", "SEGMENT_NUM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_STRUCTURES_COA_CODE",
                table: "GL_COA_STRUCTURES",
                column: "COA_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GL_ACCOUNTS");

            migrationBuilder.DropTable(
                name: "GL_COA_SEGMENT_VALUES");

            migrationBuilder.DropTable(
                name: "ADM_LKP_BUSINESS_LINES");

            migrationBuilder.DropTable(
                name: "ADM_LKP_LEGAL_ENTITIES");

            migrationBuilder.DropTable(
                name: "GL_COA_SEGMENTS");

            migrationBuilder.DropTable(
                name: "GL_COA_STRUCTURES");
        }
    }
}
