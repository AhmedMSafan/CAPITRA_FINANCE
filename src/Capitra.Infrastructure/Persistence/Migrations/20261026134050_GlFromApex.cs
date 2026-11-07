using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlFromApex : Migration
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
                name: "FIN_BUSINESS_LINES",
                columns: table => new
                {
                    BUSINESS_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BUSINESS_LINE_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BUSINESS_LINE_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MODULE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DESCRIPTION_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_BUSINESS_LINES", x => x.BUSINESS_LINE_ID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_CURRENCIES",
                columns: table => new
                {
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CURRENCY_NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CURRENCY_NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SYMBOL = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PRECISION_DIGITS = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_CURRENCIES", x => x.CURRENCY_CODE);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_STATUSES",
                columns: table => new
                {
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    STATUS_NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    STATUS_NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: true),
                    IS_FINAL = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_STATUSES", x => x.STATUS_CODE);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_RULE_AUDITS",
                columns: table => new
                {
                    RULE_AUDIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OBJECT_TYPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OBJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COLUMN_NAME = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OLD_VALUE = table.Column<string>(type: "text", nullable: true),
                    NEW_VALUE = table.Column<string>(type: "text", nullable: true),
                    CHANGED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CHANGE_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_RULE_AUDITS", x => x.RULE_AUDIT_ID);
                });

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
                    IS_ADJUSTMENT_PERIOD = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "GL_CLOSE_ACCOUNT_GROUPS",
                columns: table => new
                {
                    ACCOUNT_GROUP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCOUNT_GROUP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACCOUNT_GROUP_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ACCOUNT_GROUP_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ACCOUNT_GROUP_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NORMAL_BALANCE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CLOSE_BEHAVIOR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCOUNT_GROUPS", x => x.ACCOUNT_GROUP_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_VALIDATION_SETS",
                columns: table => new
                {
                    VALIDATION_SET_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALIDATION_SET_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VALIDATION_SET_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VALIDATION_SET_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CLOSE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SEVERITY_ON_FAILURE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_VALIDATION_SETS", x => x.VALIDATION_SET_ID);
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
                    BALANCING_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    ACCOUNT_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: false),
                    BRANCH_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    COST_CENTER_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    BUSINESS_LINE_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    INTERCOMPANY_SEGMENT_NUM = table.Column<int>(type: "integer", nullable: true),
                    IS_DYNAMIC_COMBINATIONS_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "GL_EXTERNAL_GL_SYSTEMS",
                columns: table => new
                {
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SYSTEM_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SYSTEM_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SYSTEM_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ORACLE_PRODUCT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EXPORT_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENDPOINT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREDENTIAL_ALIAS = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DEFAULT_INTERFACE_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_DIRECT_DB_INSERT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_FILE_EXPORT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_IMPORT_CONFIRMATION_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_POST_CONFIRMATION_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_EXTERNAL_GL_SYSTEMS", x => x.EXTERNAL_SYSTEM_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_SOURCES",
                columns: table => new
                {
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SOURCE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SOURCE_GROUP_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_SYSTEM_SOURCE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_IMPORT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_APPROVAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_POST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REVERSAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_DUPLICATE_CHECK = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_SOURCES", x => x.SOURCE_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_EXPORT_AUDITS",
                columns: table => new
                {
                    AUDIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AUDIT_OBJECT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AUDIT_OBJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OLD_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NEW_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACTION_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ACTION_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    ACTION_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_EXPORT_AUDITS", x => x.AUDIT_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_CLOSE_TASKS",
                columns: table => new
                {
                    CLOSE_TASK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TASK_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    TASK_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TASK_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TASK_AREA = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_CHECK = table.Column<bool>(type: "boolean", nullable: false),
                    AUTO_CHECK_PACKAGE = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AUTO_CHECK_PROCEDURE = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DEFAULT_RESPONSIBLE_ROLE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_CLOSE_TASKS", x => x.CLOSE_TASK_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_RATE_TYPES",
                columns: table => new
                {
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RATE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RATE_TYPE_NAME_EN = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RATE_TYPE_NAME_AR = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    RATE_SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RATE_PURPOSE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_DEFAULT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ENTRY_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_APPROVAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RATE_TYPES", x => x.RATE_TYPE_ID);
                });

            migrationBuilder.CreateTable(
                name: "GL_SETUP_AUDITS",
                columns: table => new
                {
                    AUDIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ENTITY_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ENTITY_KEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CHANGE_SUMMARY = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OLD_VALUES_JSON = table.Column<string>(type: "text", nullable: true),
                    NEW_VALUES_JSON = table.Column<string>(type: "text", nullable: true),
                    SOURCE_APP_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_PAGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_SESSION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SETUP_AUDITS", x => x.AUDIT_ID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_LEGAL_ENTITIES",
                columns: table => new
                {
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEGAL_ENTITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEGAL_ENTITY_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    LEGAL_ENTITY_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    SHORT_NAME = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    COUNTRY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    BASE_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TAX_REGISTRATION_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    COMMERCIAL_REGISTER_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    REGULATOR_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LEGAL_ADDRESS_EN = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LEGAL_ADDRESS_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EMAIL_ADDRESS = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    WEBSITE_URL = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_PRIMARY = table.Column<bool>(type: "boolean", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PARENT_LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEGAL_ENTITY_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_CONSOLIDATION = table.Column<bool>(type: "boolean", nullable: false),
                    OWNERSHIP_PERCENT = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    CONSOLIDATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_LEGAL_ENTITIES", x => x.LEGAL_ENTITY_ID);
                    table.ForeignKey(
                        name: "FK_FIN_LEGAL_ENTITIES_FIN_CURRENCIES",
                        column: x => x.BASE_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
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
                name: "GL_CLOSE_VALIDATION_RULES",
                columns: table => new
                {
                    VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALIDATION_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RULE_CATEGORY = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RULE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_BLOCKING = table.Column<bool>(type: "boolean", nullable: false),
                    VALIDATION_SQL = table.Column<string>(type: "text", nullable: true),
                    EXPECTED_RESULT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    THRESHOLD_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    THRESHOLD_PERCENT = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    TOLERANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOLERANCE_PERCENT = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_VALIDATION_RULES", x => x.VALIDATION_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_VALIDATION_RULES_GL_CLOSE_VALIDATION_SETS",
                        column: x => x.VALIDATION_SET_ID,
                        principalTable: "GL_CLOSE_VALIDATION_SETS",
                        principalColumn: "VALIDATION_SET_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_COMBINATION_RULES",
                columns: table => new
                {
                    RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    RULE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RULE_SCOPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    VALIDATION_LEVEL = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PRIORITY_NO = table.Column<int>(type: "integer", nullable: false),
                    ERROR_MESSAGE_EN = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ERROR_MESSAGE_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_GL_ACCOUNT_COMBINATION_RULES", x => x.RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATION_RULES_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_HIERARCHIES",
                columns: table => new
                {
                    HIERARCHY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    HIERARCHY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HIERARCHY_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    HIERARCHY_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    HIERARCHY_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_DEFAULT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_HIERARCHIES", x => x.HIERARCHY_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_HIERARCHIES_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
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
                    IS_SUMMARY_VALUES_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DEFAULT_VALUE_CODE = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
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
                name: "GL_ORACLE_GL_CATEGORY_MAPS",
                columns: table => new
                {
                    CATEGORY_MAP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINOVA_CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ORACLE_JE_CATEGORY_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ORACLE_CATEGORY_KEY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_CATEGORY_MAPS", x => x.CATEGORY_MAP_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_CATEGORY_MAPS_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_SOURCE_MAPS",
                columns: table => new
                {
                    SOURCE_MAP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINOVA_SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ORACLE_JE_SOURCE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ORACLE_SOURCE_KEY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_SOURCE_MAPS", x => x.SOURCE_MAP_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_SOURCE_MAPS_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_CATEGORIES",
                columns: table => new
                {
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CATEGORY_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CATEGORY_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CATEGORY_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DEFAULT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_REVERSAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_IMPORT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_CATEGORIES", x => x.CATEGORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_CATEGORIES_GL_JOURNAL_SOURCES",
                        column: x => x.DEFAULT_SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_DAILY_RATES",
                columns: table => new
                {
                    DAILY_RATE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    FROM_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TO_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CONVERSION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(24,12)", precision: 24, scale: 12, nullable: false),
                    INVERSE_RATE = table.Column<decimal>(type: "numeric(24,12)", precision: 24, scale: 12, nullable: true),
                    RATE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    TRIANGULATION_CURRENCY = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    TOLERANCE_PERCENT = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REJECTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REJECTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REJECTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_DAILY_RATES", x => x.DAILY_RATE_ID);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_RATES_FIN_CURRENCIES_FROM_CURRENCY_CODE",
                        column: x => x.FROM_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_RATES_FIN_CURRENCIES_TO_CURRENCY_CODE",
                        column: x => x.TO_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_RATES_FIN_CURRENCIES_TRIANGULATION_CURRENCY",
                        column: x => x.TRIANGULATION_CURRENCY,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_RATES_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RATE_IMPORT_BATCHES",
                columns: table => new
                {
                    IMPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BATCH_NAME = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    IMPORT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TOTAL_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    VALID_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ERROR_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    IMPORTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IMPORTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RATE_IMPORT_BATCHES", x => x.IMPORT_BATCH_ID);
                    table.ForeignKey(
                        name: "FK_GL_RATE_IMPORT_BATCHES_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FIN_BRANCHES",
                columns: table => new
                {
                    BRANCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    BRANCH_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BRANCH_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BRANCH_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PARENT_BRANCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    BRANCH_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COUNTRY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CITY_NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ADDRESS_EN = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ADDRESS_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EMAIL_ADDRESS = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_BRANCHES", x => x.BRANCH_ID);
                    table.ForeignKey(
                        name: "FK_FIN_BRANCHES_FIN_BRANCHES",
                        column: x => x.PARENT_BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FIN_BRANCHES_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGERS",
                columns: table => new
                {
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LEDGER_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LEDGER_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_PRIMARY_LEDGER = table.Column<bool>(type: "boolean", nullable: false),
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
                    COA_ID = table.Column<long>(type: "bigint", nullable: true),
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
                        name: "FK_GL_LEDGERS_FIN_CURRENCIES",
                        column: x => x.LEDGER_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGERS_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
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
                    PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OPENED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OPENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLOSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CLOSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "GL_ACCOUNT_COMBINATION_RULE_LINES",
                columns: table => new
                {
                    RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    SEGMENT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SEGMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ATTRIBUTE_NAME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OPERATOR_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VALUE_FROM = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    VALUE_TO = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    VALUE_LIST = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    INCLUDE_EXCLUDE_FLAG = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_COMBINATION_RULE_LINES", x => x.RULE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATION_RULE_LINES_GL_ACCOUNT_COMBIN_7BB51A9D",
                        column: x => x.RULE_ID,
                        principalTable: "GL_ACCOUNT_COMBINATION_RULES",
                        principalColumn: "RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_EVENT_TYPES",
                columns: table => new
                {
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EVENT_TYPE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    EVENT_TYPE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    DEFAULT_CATEGORY_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_ACCOUNTING_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ADJUST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_EVENT_TYPES", x => x.EVENT_TYPE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_TYPES_GL_JOURNAL_CATEGORIES",
                        column: x => x.DEFAULT_CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_TYPES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_VALIDATION_RULES",
                columns: table => new
                {
                    VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: true),
                    SEVERITY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_VALIDATION_RULES", x => x.VALIDATION_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RULES_GL_JOURNAL_CATEGORIES",
                        column: x => x.CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RULES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RATE_IMPORT_LINES",
                columns: table => new
                {
                    IMPORT_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IMPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    RATE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FROM_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    TO_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CONVERSION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INVERSE_RATE = table.Column<decimal>(type: "numeric(24,12)", precision: 24, scale: 12, nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DAILY_RATE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RATE_IMPORT_LINES", x => x.IMPORT_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_RATE_IMPORT_LINES_GL_DAILY_RATES",
                        column: x => x.DAILY_RATE_ID,
                        principalTable: "GL_DAILY_RATES",
                        principalColumn: "DAILY_RATE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RATE_IMPORT_LINES_GL_RATE_IMPORT_BATCHES",
                        column: x => x.IMPORT_BATCH_ID,
                        principalTable: "GL_RATE_IMPORT_BATCHES",
                        principalColumn: "IMPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FIN_COST_CENTERS",
                columns: table => new
                {
                    COST_CENTER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    BRANCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    COST_CENTER_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COST_CENTER_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    COST_CENTER_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PARENT_COST_CENTER_ID = table.Column<long>(type: "bigint", nullable: true),
                    COST_CENTER_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MANAGER_USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_COST_CENTERS", x => x.COST_CENTER_ID);
                    table.ForeignKey(
                        name: "FK_FIN_COST_CENTERS_FIN_BRANCHES",
                        column: x => x.BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FIN_COST_CENTERS_FIN_COST_CENTERS",
                        column: x => x.PARENT_COST_CENTER_ID,
                        principalTable: "FIN_COST_CENTERS",
                        principalColumn: "COST_CENTER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FIN_COST_CENTERS_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_PROFILES",
                columns: table => new
                {
                    CLOSE_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_PROFILE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CLOSE_PROFILE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CLOSE_PROFILE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CLOSE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLOSE_FREQUENCY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_DEFAULT_PROFILE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_TASK_OWNER_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ATTACHMENT_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_OVERRIDE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_GENERATE_ACCRUALS = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_GENERATE_RECURRING = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_RUN_FX_REVALUATION = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_LOCK_PERIOD = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_PROFILES", x => x.CLOSE_PROFILE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PROFILES_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PROFILES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_FX_RATE_USAGE_LOGS",
                columns: table => new
                {
                    RATE_USAGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DAILY_RATE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_TRANSACTION_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_TRANSACTION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TRANSACTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ENTERED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ACCOUNTED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ACCOUNTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_FX_RATE_USAGE_LOGS", x => x.RATE_USAGE_ID);
                    table.ForeignKey(
                        name: "FK_GL_FX_RATE_USAGE_LOGS_FIN_CURRENCIES_ACCOUNTED_CURRENCY_CODE",
                        column: x => x.ACCOUNTED_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_RATE_USAGE_LOGS_FIN_CURRENCIES_ENTERED_CURRENCY_CODE",
                        column: x => x.ENTERED_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_RATE_USAGE_LOGS_GL_DAILY_RATES",
                        column: x => x.DAILY_RATE_ID,
                        principalTable: "GL_DAILY_RATES",
                        principalColumn: "DAILY_RATE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_RATE_USAGE_LOGS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_NUMBERING_RULES",
                columns: table => new
                {
                    NUMBERING_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PREFIX = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_INCLUDE_YEAR = table.Column<bool>(type: "boolean", nullable: false),
                    IS_INCLUDE_PERIOD = table.Column<bool>(type: "boolean", nullable: false),
                    NEXT_NUMBER = table.Column<int>(type: "integer", nullable: false),
                    NUMBER_LENGTH = table.Column<int>(type: "integer", nullable: false),
                    RESET_FREQUENCY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_NUMBERING_RULES", x => x.NUMBERING_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_NUMBERING_RULES_GL_JOURNAL_CATEGORIES",
                        column: x => x.CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_NUMBERING_RULES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_NUMBERING_RULES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGER_BRANCHES",
                columns: table => new
                {
                    LEDGER_BRANCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    BRANCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_DEFAULT_BRANCH = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_LEDGER_BRANCHES", x => x.LEDGER_BRANCH_ID);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_BRANCHES_FIN_BRANCHES",
                        column: x => x.BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_BRANCHES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_LEDGER_OPTIONS",
                columns: table => new
                {
                    LEDGER_OPTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    ALLOW_MANUAL_JOURNALS = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    REQUIRE_MANUAL_JOURNAL_APPR = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ALLOW_SUBLEDGER_JOURNALS = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    IS_AUTO_POST_SUBLEDGER = table.Column<bool>(type: "boolean", nullable: false),
                    ALLOW_FUTURE_PERIOD_ENTRY = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ALLOW_BACKDATED_ENTRY = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ALLOW_REVERSAL_JOURNALS = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    IS_ATTACHMENT_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    REQUIRE_BALANCED_JOURNAL = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ALLOW_STATISTICAL_JOURNALS = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    MAX_BACKDATE_DAYS = table.Column<int>(type: "integer", nullable: true),
                    DEFAULT_REVERSAL_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PERIOD_CLOSE_POLICY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_LEDGER_OPTIONS", x => x.LEDGER_OPTION_ID);
                    table.ForeignKey(
                        name: "FK_GL_LEDGER_OPTIONS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_LEDGER_MAPS",
                columns: table => new
                {
                    LEDGER_MAP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    ORACLE_LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_LEDGER_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ORACLE_SET_OF_BOOKS_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_CHART_OF_ACCOUNTS_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_LEDGER_CURRENCY = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    IS_DEFAULT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_LEDGER_MAPS", x => x.LEDGER_MAP_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_LEDGER_MAPS_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_LEDGER_MAPS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_CONTROL_RULES",
                columns: table => new
                {
                    PERIOD_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RULE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    MESSAGE_EN = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MESSAGE_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_CONTROL_RULES", x => x.PERIOD_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_CONTROL_RULES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_PROFILES",
                columns: table => new
                {
                    POSTING_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROFILE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PROFILE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PROFILE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    POSTING_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BALANCE_UPDATE_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_AUTO_POST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BATCH_POST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_APPROVAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_UNAPPROVED_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BACKDATED_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_FUTURE_PERIOD_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BALANCED_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_FX_RATE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_UPDATE_DAILY_BALANCE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_CREATE_AUDIT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_NOTIFY_SOURCE = table.Column<bool>(type: "boolean", nullable: false),
                    MAX_RETRY_COUNT = table.Column<int>(type: "integer", nullable: false),
                    PRIORITY = table.Column<int>(type: "integer", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_PROFILES", x => x.POSTING_PROFILE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_PROFILES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_VALIDATION_SETS",
                columns: table => new
                {
                    VALIDATION_SET_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALIDATION_SET_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VALIDATION_SET_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_VALIDATION_SETS", x => x.VALIDATION_SET_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_SETS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RECURRING_JOURNAL_TEMPLATES",
                columns: table => new
                {
                    RECURRING_TEMPLATE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: false),
                    TEMPLATE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    TEMPLATE_NAME = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RECURRENCE_FREQUENCY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NEXT_GENERATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_AUTO_SUBMIT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_POST = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RECURRING_JOURNAL_TEMPLATES", x => x.RECURRING_TEMPLATE_ID);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_TEMPLATES_GL_JOURNAL_CATEGORIES",
                        column: x => x.CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_TEMPLATES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_TEMPLATES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
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
                name: "GL_AUTO_ACCOUNTING_RULE_SETS",
                columns: table => new
                {
                    RULE_SET_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_SET_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RULE_SET_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RULE_SET_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PRODUCT_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    TRANSACTION_TYPE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PRIORITY_NO = table.Column<int>(type: "integer", nullable: false),
                    VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RULE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    APPROVAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATION_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_BALANCING_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_OVERRIDE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ADJUST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    REVERSAL_BEHAVIOR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DUPLICATE_POLICY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DEFAULT_CATEGORY_ID = table.Column<long>(type: "bigint", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_ACCOUNTING_RULE_SETS", x => x.RULE_SET_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_FIN_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_GL_ACCOUNTING_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_ID,
                        principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                        principalColumn: "EVENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_GL_JOURNAL_CATEGORIES",
                        column: x => x.DEFAULT_CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_SETS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_SOURCE_VALUE_DEFINITIONS",
                columns: table => new
                {
                    SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    VALUE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    VALUE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    VALUE_KIND = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALUE_ROLE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DEFAULT_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REPEATABLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_RULE_USAGE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    SOURCE_TABLE_NAME = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SOURCE_COLUMN_NAME = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SOURCE_PATH = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SOURCE_VALUE_DEFINITIONS", x => x.SOURCE_VALUE_ID);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_VALUE_DEFINITIONS_FIN_CURRENCIES",
                        column: x => x.DEFAULT_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_VALUE_DEFINITIONS_GL_ACCOUNTING_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_ID,
                        principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                        principalColumn: "EVENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_VALUE_DEFINITIONS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
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
                    SORT_ORDER = table.Column<long>(type: "bigint", nullable: false),
                    REFERENCE_LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    REFERENCE_BRANCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    REFERENCE_COST_CENTER_ID = table.Column<long>(type: "bigint", nullable: true),
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
                        name: "FK_GL_COA_SEGMENT_VALUES_FIN_BRANCHES",
                        column: x => x.REFERENCE_BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_FIN_COST_CENTERS",
                        column: x => x.REFERENCE_COST_CENTER_ID,
                        principalTable: "FIN_COST_CENTERS",
                        principalColumn: "COST_CENTER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_COA_SEGMENT_VALUES_FIN_LEGAL_ENTITIES",
                        column: x => x.REFERENCE_LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
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
                name: "GL_CLOSE_ACCRUAL_RULES",
                columns: table => new
                {
                    ACCRUAL_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCRUAL_RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACCRUAL_RULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ACCRUAL_RULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CLOSE_PROFILE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACCRUAL_CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACCRUAL_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVERSAL_POLICY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_AUTO_REVERSE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_POST = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCRUAL_RULES", x => x.ACCRUAL_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RULES_GL_CLOSE_PROFILES",
                        column: x => x.CLOSE_PROFILE_ID,
                        principalTable: "GL_CLOSE_PROFILES",
                        principalColumn: "CLOSE_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RULES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_PROFILE_MODULES",
                columns: table => new
                {
                    PROFILE_MODULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_MODULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SOURCE_MODULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_POSTING_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_RECONCILIATION_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    EXPECTED_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_PROFILE_MODULES", x => x.PROFILE_MODULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PROFILE_MODULES_GL_CLOSE_PROFILES",
                        column: x => x.CLOSE_PROFILE_ID,
                        principalTable: "GL_CLOSE_PROFILES",
                        principalColumn: "CLOSE_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_PROFILE_STEPS",
                columns: table => new
                {
                    CLOSE_STEP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    STEP_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    STEP_GROUP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DEPENDENCY_STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_COMPLETE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_EXECUTE = table.Column<bool>(type: "boolean", nullable: false),
                    VALIDATION_SET_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RESPONSIBLE_ROLE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESPONSIBLE_USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXPECTED_DURATION_HOURS = table.Column<int>(type: "integer", nullable: true),
                    ESCALATION_AFTER_HOURS = table.Column<int>(type: "integer", nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_PROFILE_STEPS", x => x.CLOSE_STEP_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PROFILE_STEPS_GL_CLOSE_PROFILES",
                        column: x => x.CLOSE_PROFILE_ID,
                        principalTable: "GL_CLOSE_PROFILES",
                        principalColumn: "CLOSE_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_EXPORT_PROFILES",
                columns: table => new
                {
                    EXPORT_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_MAP_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROFILE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PROFILE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PROFILE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EXPORT_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EXPORT_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DEFAULT_SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DEFAULT_CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_AUTO_SUBMIT_IMPORT = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_RECONCILE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ZERO_DIFFERENCE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    DUPLICATE_CHECK_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVERSAL_HANDLING = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_EXPORT_PROFILES", x => x.EXPORT_PROFILE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_PROFILES_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_PROFILES_GL_ORACLE_GL_LEDGER_MAPS",
                        column: x => x.LEDGER_MAP_ID,
                        principalTable: "GL_ORACLE_GL_LEDGER_MAPS",
                        principalColumn: "LEDGER_MAP_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_CONTROL_RULE_LINES",
                columns: table => new
                {
                    PERIOD_RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PERIOD_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    CONDITION_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CONDITION_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    OPERATOR_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CONDITION_VALUE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_CONTROL_RULE_LINES", x => x.PERIOD_RULE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_CONTROL_RULE_LINES_GL_PERIOD_CONTROL_RULES",
                        column: x => x.PERIOD_RULE_ID,
                        principalTable: "GL_PERIOD_CONTROL_RULES",
                        principalColumn: "PERIOD_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_PROFILE_RULES",
                columns: table => new
                {
                    PROFILE_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_SEQ = table.Column<int>(type: "integer", nullable: false),
                    RULE_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CONDITION_ATTRIBUTE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CONDITION_OPERATOR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CONDITION_VALUE1 = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CONDITION_VALUE2 = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_WARNING = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_PROFILE_RULES", x => x.PROFILE_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_PROFILE_RULES_GL_POSTING_PROFILES",
                        column: x => x.POSTING_PROFILE_ID,
                        principalTable: "GL_POSTING_PROFILES",
                        principalColumn: "POSTING_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_SCHEDULES",
                columns: table => new
                {
                    POSTING_SCHEDULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SCHEDULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SCHEDULE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    POSTING_PROFILE_ID = table.Column<long>(type: "bigint", nullable: true),
                    FREQUENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SCHEDULE_TIME = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DAY_OF_MONTH = table.Column<int>(type: "integer", nullable: true),
                    DAY_OF_WEEK = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LAST_RUN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NEXT_RUN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_SCHEDULES", x => x.POSTING_SCHEDULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SCHEDULES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SCHEDULES_GL_POSTING_PROFILES",
                        column: x => x.POSTING_PROFILE_ID,
                        principalTable: "GL_POSTING_PROFILES",
                        principalColumn: "POSTING_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_VALIDATION_RULES",
                columns: table => new
                {
                    POSTING_VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALIDATION_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RULE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VALIDATION_STAGE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RULE_SEQ = table.Column<int>(type: "integer", nullable: false),
                    IS_STOP_ON_FAILURE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_VALIDATION_RULES", x => x.POSTING_VALIDATION_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RULES_GL_POSTING_VALIDATION_SETS",
                        column: x => x.VALIDATION_SET_ID,
                        principalTable: "GL_POSTING_VALIDATION_SETS",
                        principalColumn: "VALIDATION_SET_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_RUNS",
                columns: table => new
                {
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CLOSE_PROFILE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLOSE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLOSE_PERIOD_NAME = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CLOSE_YEAR_NUMBER = table.Column<int>(type: "integer", nullable: true),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RUN_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    STARTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REOPEN_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CLOSE_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    TOTAL_TASKS = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    COMPLETED_TASKS = table.Column<int>(type: "integer", nullable: true),
                    BLOCKED_TASKS = table.Column<int>(type: "integer", nullable: true),
                    WARNING_COUNT = table.Column<int>(type: "integer", nullable: true),
                    ERROR_COUNT = table.Column<int>(type: "integer", nullable: true),
                    BLOCKER_COUNT = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_RUNS", x => x.CLOSE_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUNS_GL_CLOSE_PROFILES",
                        column: x => x.CLOSE_PROFILE_ID,
                        principalTable: "GL_CLOSE_PROFILES",
                        principalColumn: "CLOSE_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUNS_GL_FISCAL_YEARS",
                        column: x => x.FISCAL_YEAR_ID,
                        principalTable: "GL_FISCAL_YEARS",
                        principalColumn: "FISCAL_YEAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                columns: table => new
                {
                    EXPOSURE_SNAPSHOT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    SNAPSHOT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SNAPSHOT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SNAPSHOT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CURRENCY_EXPOSURE_SNAPSHOTS", x => x.EXPOSURE_SNAPSHOT_ID);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_SNAPSHOTS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_SNAPSHOTS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_SNAPSHOTS_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_BATCHES",
                columns: table => new
                {
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_NO = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    BATCH_NAME = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    BATCH_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    APPROVAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CONTROL_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CONTROL_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACTUAL_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACTUAL_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    JOURNAL_COUNT = table.Column<int>(type: "integer", nullable: false),
                    SOURCE_BATCH_REF = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CANCELLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCEL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_BATCHES", x => x.BATCH_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_BATCHES_GL_JOURNAL_CATEGORIES",
                        column: x => x.CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_BATCHES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_BATCHES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_BATCHES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_ACCESS_EXCEPTIONS",
                columns: table => new
                {
                    PERIOD_EXCEPTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXCEPTION_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EXCEPTION_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ALLOWED_USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ALLOWED_ROLE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    VALID_FROM_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VALID_TO_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MAX_TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    USED_TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    APPROVAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_ACCESS_EXCEPTIONS", x => x.PERIOD_EXCEPTION_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_ACCESS_EXCEPTIONS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_CLOSE_CHECKLISTS",
                columns: table => new
                {
                    CLOSE_CHECKLIST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_TASK_ID = table.Column<long>(type: "bigint", nullable: false),
                    TASK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    RESPONSIBLE_ROLE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ASSIGNED_TO_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WAIVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WAIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WAIVER_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_CLOSE_CHECKLISTS", x => x.CLOSE_CHECKLIST_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_CLOSE_CHECKLISTS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_CLOSE_CHECKLISTS_GL_PERIOD_CLOSE_TASKS",
                        column: x => x.CLOSE_TASK_ID,
                        principalTable: "GL_PERIOD_CLOSE_TASKS",
                        principalColumn: "CLOSE_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_POSTING_WINDOWS",
                columns: table => new
                {
                    POSTING_WINDOW_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_MANUAL_POST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_POST_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_INTERFACE_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    POSTING_CUTOFF_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_REOPENED = table.Column<bool>(type: "boolean", nullable: false),
                    REOPENED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REOPENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REOPEN_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_POSTING_WINDOWS", x => x.POSTING_WINDOW_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_POSTING_WINDOWS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_POSTING_WINDOWS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_SOURCE_STATUSES",
                columns: table => new
                {
                    SOURCE_STATUS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TRANSFER_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UNTRANSFERRED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    UNPOSTED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ERROR_COUNT = table.Column<int>(type: "integer", nullable: false),
                    LAST_TRANSFER_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_CHECKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESPONSIBLE_ROLE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_SOURCE_STATUSES", x => x.SOURCE_STATUS_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_SOURCE_STATUSES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_PERIOD_STATUS_HIST",
                columns: table => new
                {
                    PERIOD_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    OLD_PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NEW_PERIOD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ACTION_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SOURCE_APP_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_PAGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_SESSION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_PERIOD_STATUS_HIST", x => x.PERIOD_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_PERIOD_STATUS_HIST_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_SEQUENCE_AUDITS",
                columns: table => new
                {
                    POSTING_SEQUENCE_AUDIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SEQUENCE_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LAST_SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    GENERATED_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RELATED_TABLE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RELATED_RECORD_ID = table.Column<long>(type: "bigint", nullable: true),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_SEQUENCE_AUDITS", x => x.POSTING_SEQUENCE_AUDIT_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SEQUENCE_AUDITS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SEQUENCE_AUDITS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_RULE_APPROVAL_HIST",
                columns: table => new
                {
                    APPROVAL_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FROM_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TO_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    COMMENTS = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_RULE_APPROVAL_HIST", x => x.APPROVAL_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_APPROVAL_HIST_GL_AUTO_ACCOUNTING_RULE_SETS",
                        column: x => x.RULE_SET_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_SETS",
                        principalColumn: "RULE_SET_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_RULE_TEST_CASES",
                columns: table => new
                {
                    TEST_CASE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    TEST_CASE_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TEST_CASE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    SAMPLE_EVENT_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    EXPECTED_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    EXPECTED_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    TEST_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LAST_RUN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_RUN_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LAST_RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_RULE_TEST_CASES", x => x.TEST_CASE_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_TEST_CASES_GL_AUTO_ACCOUNTING_RULE_SETS",
                        column: x => x.RULE_SET_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_SETS",
                        principalColumn: "RULE_SET_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_ACCOUNTING_CONDITIONS",
                columns: table => new
                {
                    CONDITION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OWNER_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OWNER_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONDITION_GROUP_NO = table.Column<int>(type: "integer", nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    LEFT_SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEFT_VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    LEFT_EXPRESSION = table.Column<string>(type: "text", nullable: true),
                    OPERATOR_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COMPARE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COMPARE_SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMPARE_VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    COMPARE_TEXT_VALUE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    COMPARE_NUMBER_VALUE = table.Column<decimal>(type: "numeric(22,8)", precision: 22, scale: 8, nullable: true),
                    COMPARE_DATE_VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    COMPARE_FROM_NUMBER = table.Column<decimal>(type: "numeric(22,8)", precision: 22, scale: 8, nullable: true),
                    COMPARE_TO_NUMBER = table.Column<decimal>(type: "numeric(22,8)", precision: 22, scale: 8, nullable: true),
                    JOIN_OPERATOR = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_ACCOUNTING_CONDITIONS", x => x.CONDITION_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_CONDITIONS_GL_SOURCE_VALUE_DEFIN_7940F9DB",
                        column: x => x.LEFT_SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_CONDITIONS_GL_SOURCE_VALUE_DEFIN_FAEAE227",
                        column: x => x.COMPARE_SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_COMBINATIONS",
                columns: table => new
                {
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONCATENATED_ACCOUNT = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    DISPLAY_ACCOUNT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BRANCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    COST_CENTER_ID = table.Column<long>(type: "bigint", nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    COMPANY_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BRANCH_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    COST_CENTER_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BUSINESS_LINE_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_SUMMARY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_POSTING_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_MANUAL_CREATED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_DYNAMIC_CREATED = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_VALIDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_COMBINATIONS", x => x.ACCOUNT_COMBINATION_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_FIN_BRANCHES",
                        column: x => x.BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_FIN_COST_CENTERS",
                        column: x => x.COST_CENTER_ID,
                        principalTable: "FIN_COST_CENTERS",
                        principalColumn: "COST_CENTER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_SEGMENT_VALUES_ACCOU_42C89AEE",
                        column: x => x.ACCOUNT_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_SEGMENT_VALUES_BRANC_873F4A31",
                        column: x => x.BRANCH_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_SEGMENT_VALUES_BUSIN_6CCCE636",
                        column: x => x.BUSINESS_LINE_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_SEGMENT_VALUES_COMPA_F40E004F",
                        column: x => x.COMPANY_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_SEGMENT_VALUES_COST__C4D9447D",
                        column: x => x.COST_CENTER_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_HIERARCHY_NODES",
                columns: table => new
                {
                    NODE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HIERARCHY_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARENT_NODE_ID = table.Column<long>(type: "bigint", nullable: true),
                    NODE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NODE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    NODE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    NODE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    DISPLAY_SEQ = table.Column<long>(type: "bigint", nullable: false),
                    SIGN_MULTIPLIER = table.Column<int>(type: "integer", nullable: false),
                    ROLLUP_OPERATOR = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_HIERARCHY_NODES", x => x.NODE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_HIERARCHY_NODES_GL_ACCOUNT_HIERARCHIES",
                        column: x => x.HIERARCHY_ID,
                        principalTable: "GL_ACCOUNT_HIERARCHIES",
                        principalColumn: "HIERARCHY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_HIERARCHY_NODES_GL_ACCOUNT_HIERARCHY_NODES",
                        column: x => x.PARENT_NODE_ID,
                        principalTable: "GL_ACCOUNT_HIERARCHY_NODES",
                        principalColumn: "NODE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_HIERARCHY_NODES_GL_COA_SEGMENT_VALUES",
                        column: x => x.SEGMENT_VALUE_ID,
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
                    SORT_ORDER = table.Column<long>(type: "bigint", nullable: false),
                    EXTERNAL_GL_ACCOUNT_MAP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_EXPORT_BATCHES",
                columns: table => new
                {
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_PROFILE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_BATCH_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EXPORT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EXPORT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GROUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_IMPORT_REQUEST_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXPORT_FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EXPORT_FILE_MIME_TYPE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EXPORT_FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: true),
                    TOTAL_HEADERS = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    TOTAL_LINES = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    TOTAL_ENTERED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TOTAL_ENTERED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TOTAL_ACCOUNTED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TOTAL_ACCOUNTED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    VALIDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VALIDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EXPORTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXPORTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_EXPORT_BATCHES", x => x.EXPORT_BATCH_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_BATCHES_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_BATCHES_GL_ORACLE_GL_EXPORT_PROFILES",
                        column: x => x.EXPORT_PROFILE_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_PROFILES",
                        principalColumn: "EXPORT_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_PERIOD_LOCKS",
                columns: table => new
                {
                    PERIOD_LOCK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    LOCK_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LOCK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LOCK_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UNLOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UNLOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UNLOCK_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_PERIOD_LOCKS", x => x.PERIOD_LOCK_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PERIOD_LOCKS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PERIOD_LOCKS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_PERIOD_LOCKS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_REPORTS",
                columns: table => new
                {
                    CLOSE_REPORT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPORT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    REPORT_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    REPORT_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REPORT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REPORT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_FILE_NAME = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GENERATED_FILE_MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: true),
                    GENERATED_FILE_CLOB = table.Column<string>(type: "text", nullable: true),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REPORT_PARAMETERS_JSON = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_REPORTS", x => x.CLOSE_REPORT_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_REPORTS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_RUN_MODULE_STATUSES",
                columns: table => new
                {
                    RUN_MODULE_STATUS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_MODULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MODULE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RECONCILIATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TOTAL_TRANSACTIONS = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    POSTED_TRANSACTIONS = table.Column<int>(type: "integer", nullable: true),
                    UNPOSTED_TRANSACTIONS = table.Column<int>(type: "integer", nullable: true),
                    ERROR_TRANSACTIONS = table.Column<int>(type: "integer", nullable: true),
                    SOURCE_CONTROL_TOTAL = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    GL_CONTROL_TOTAL = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    VARIANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CERTIFIED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CERTIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SOURCE_REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_RUN_MODULE_STATUSES", x => x.RUN_MODULE_STATUS_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUN_MODULE_STATUSES_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_RUN_TASKS",
                columns: table => new
                {
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_STEP_ID = table.Column<long>(type: "bigint", nullable: true),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TASK_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TASK_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TASK_GROUP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TASK_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DEPENDENCY_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    OWNER_ROLE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OWNER_USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TASK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BLOCKING = table.Column<bool>(type: "boolean", nullable: false),
                    DUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_WAIVER = table.Column<bool>(type: "boolean", nullable: false),
                    WAIVER_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    COMPLETION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_RUN_TASKS", x => x.CLOSE_RUN_TASK_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUN_TASKS_GL_CLOSE_PROFILE_STEPS",
                        column: x => x.CLOSE_STEP_ID,
                        principalTable: "GL_CLOSE_PROFILE_STEPS",
                        principalColumn: "CLOSE_STEP_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUN_TASKS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RUN_TASKS_GL_CLOSE_RUN_TASKS",
                        column: x => x.DEPENDENCY_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_RECURRING_RUNS",
                columns: table => new
                {
                    CLOSE_RECURRING_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    RECURRING_TEMPLATE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    GENERATED_JOURNALS_COUNT = table.Column<int>(type: "integer", nullable: true),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_RECURRING_RUNS", x => x.CLOSE_RECURRING_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RECURRING_RUNS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RECURRING_RUNS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_RECURRING_RUNS_GL_RECURRING_JOURNAL_TEMPLATES",
                        column: x => x.RECURRING_TEMPLATE_ID,
                        principalTable: "GL_RECURRING_JOURNAL_TEMPLATES",
                        principalColumn: "RECURRING_TEMPLATE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_HEADERS",
                columns: table => new
                {
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_NO = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    JOURNAL_NAME = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CONVERSION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONVERSION_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    JOURNAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    APPROVAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_BALANCED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_SYSTEM_GENERATED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REVERSAL = table.Column<bool>(type: "boolean", nullable: false),
                    REVERSAL_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    REVERSAL_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    REVERSAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REVERSED_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORIGINAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    RECURRING_TEMPLATE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_TRANSACTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CANCELLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCEL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_HEADERS", x => x.JOURNAL_HEADER_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_CATEGORIES",
                        column: x => x.CATEGORY_ID,
                        principalTable: "GL_JOURNAL_CATEGORIES",
                        principalColumn: "CATEGORY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_HEADERS_ORIGINAL_HEADER_ID",
                        column: x => x.ORIGINAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_HEADERS_REVERSED_HEADER_ID",
                        column: x => x.REVERSED_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_LEDGER_PERIODS_LEDGER_PERIOD_ID",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_LEDGER_PERIODS_REVERSAL_PERIOD_ID",
                        column: x => x.REVERSAL_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_INTERFACE_BATCHES",
                columns: table => new
                {
                    INTERFACE_BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    INTERFACE_BATCH_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SOURCE_BATCH_REF = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    BATCH_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ERROR_COUNT = table.Column<int>(type: "integer", nullable: false),
                    GENERATED_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    IMPORTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IMPORTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VALIDATED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VALIDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PROCESSED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PROCESSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_INTERFACE_BATCHES", x => x.INTERFACE_BATCH_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_BATCHES_GL_JOURNAL_BATCHES",
                        column: x => x.GENERATED_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_BATCHES_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_BATCHES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_RULE_TEST_RESULTS",
                columns: table => new
                {
                    TEST_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TEST_CASE_ID = table.Column<long>(type: "bigint", nullable: false),
                    RUN_NO = table.Column<int>(type: "integer", nullable: false),
                    RUN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RUN_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    GENERATED_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    DIFFERENCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RESULT_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_RULE_TEST_RESULTS", x => x.TEST_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_TEST_RESULTS_GL_AUTO_RULE_TEST_CASES",
                        column: x => x.TEST_CASE_ID,
                        principalTable: "GL_AUTO_RULE_TEST_CASES",
                        principalColumn: "TEST_CASE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: table => new
                {
                    COMBINATION_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    COA_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    SEGMENT_NUM = table.Column<int>(type: "integer", nullable: false),
                    SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SEGMENT_VALUE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_COMBINATION_SEGMENTS", x => x.COMBINATION_SEGMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATION_SEGMENTS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATION_SEGMENTS_GL_COA_SEGMENTS",
                        column: x => x.COA_SEGMENT_ID,
                        principalTable: "GL_COA_SEGMENTS",
                        principalColumn: "COA_SEGMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATION_SEGMENTS_GL_COA_SEGMENT_VALUES",
                        column: x => x.SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_BALANCE_REBUILD_RUNS",
                columns: table => new
                {
                    BALANCE_REBUILD_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REBUILD_RUN_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    REBUILD_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    FROM_ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TO_ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REBUILD_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    REBUILD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_BALANCE_REBUILD_RUNS", x => x.BALANCE_REBUILD_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_RUNS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ACCRUAL_RULE_LINES",
                columns: table => new
                {
                    ACCRUAL_RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCRUAL_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    DR_CR_FLAG = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ACCOUNT_DERIVATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_LOOKUP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SEGMENT_DERIVATION_JSON = table.Column<string>(type: "text", nullable: true),
                    AMOUNT_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AMOUNT_SOURCE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FIXED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PERCENTAGE_VALUE = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    FORMULA_EXPRESSION = table.Column<string>(type: "text", nullable: true),
                    CURRENCY_DERIVATION = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ROUNDING_RULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LINE_DESCRIPTION_TEMPLATE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCRUAL_RULE_LINES", x => x.ACCRUAL_RULE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RULE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RULE_LINES_GL_CLOSE_ACCRUAL_RULES",
                        column: x => x.ACCRUAL_RULE_ID,
                        principalTable: "GL_CLOSE_ACCRUAL_RULES",
                        principalColumn: "ACCRUAL_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CURRENCY_EXPOSURE_LINES",
                columns: table => new
                {
                    EXPOSURE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPOSURE_SNAPSHOT_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXPOSURE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENTERED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ACCOUNTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FUNCTIONAL_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CURRENCY_EXPOSURE_LINES", x => x.EXPOSURE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_LINES_FIN_CURRENCIES_CURRENCY_CODE",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_LINES_FIN_CURRENCIES_FUNCTIONA_21374110",
                        column: x => x.FUNCTIONAL_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CURRENCY_EXPOSURE_LINES_GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                        column: x => x.EXPOSURE_SNAPSHOT_ID,
                        principalTable: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                        principalColumn: "EXPOSURE_SNAPSHOT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_DAILY_BALANCES",
                columns: table => new
                {
                    DAILY_BALANCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    BALANCE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DAILY_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    DAILY_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CLOSING_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CLOSING_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_DAILY_BALANCES", x => x.DAILY_BALANCE_ID);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_BALANCES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_BALANCES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_DAILY_BALANCES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_FX_REVALUATION_RULES",
                columns: table => new
                {
                    REVALUATION_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    REVALUATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVALUATION_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_BALANCE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GAIN_ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    LOSS_ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_REVERSAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_AUTO_POST = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_FX_REVALUATION_RULES", x => x.REVALUATION_RULE_ID);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RULES_GL_ACCOUNT_COMBINATIONS_GAI_5035AC7D",
                        column: x => x.GAIN_ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RULES_GL_ACCOUNT_COMBINATIONS_LOS_EB242710",
                        column: x => x.LOSS_ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RULES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RULES_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_ACCOUNT_MAPS",
                columns: table => new
                {
                    ACCOUNT_MAP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTERNAL_SYSTEM_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    ORACLE_CODE_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_CONCATENATED_ACCOUNT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SEGMENT1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT4 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT5 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT6 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT7 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT8 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT9 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT10 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT11 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT12 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT13 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT14 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT15 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT16 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT17 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT18 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT19 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT20 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT21 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT22 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT23 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT24 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT25 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT26 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT27 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT28 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT29 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT30 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MAPPING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_ACCOUNT_MAPS", x => x.ACCOUNT_MAP_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_ACCOUNT_MAPS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_ACCOUNT_MAPS_GL_EXTERNAL_GL_SYSTEMS",
                        column: x => x.EXTERNAL_SYSTEM_ID,
                        principalTable: "GL_EXTERNAL_GL_SYSTEMS",
                        principalColumn: "EXTERNAL_SYSTEM_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RECURRING_JOURNAL_LINES",
                columns: table => new
                {
                    RECURRING_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RECURRING_TEMPLATE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ALLOCATION_FORMULA = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RECURRING_JOURNAL_LINES", x => x.RECURRING_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_LINES_GL_RECURRING_JOURNAL_TEMPLATES",
                        column: x => x.RECURRING_TEMPLATE_ID,
                        principalTable: "GL_RECURRING_JOURNAL_TEMPLATES",
                        principalColumn: "RECURRING_TEMPLATE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_YEAR_END_CLOSE_RUNS",
                columns: table => new
                {
                    YEAR_END_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    TARGET_FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: true),
                    RETAINED_EARNINGS_ACCOUNT_COMB_ID = table.Column<long>(type: "bigint", nullable: true),
                    OPENING_BALANCE_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLOSING_JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVENUE_TOTAL = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    EXPENSE_TOTAL = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    NET_PROFIT_LOSS = table.Column<int>(type: "integer", nullable: false),
                    BALANCE_SHEET_TOTAL_DR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    BALANCE_SHEET_TOTAL_CR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    OPENING_BALANCE_TOTAL_DR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    OPENING_BALANCE_TOTAL_CR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_YEAR_END_CLOSE_RUNS", x => x.YEAR_END_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.RETAINED_EARNINGS_ACCOUNT_COMB_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_FISCAL_YEARS_FISCAL_YEAR_ID",
                        column: x => x.FISCAL_YEAR_ID,
                        principalTable: "GL_FISCAL_YEARS",
                        principalColumn: "FISCAL_YEAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_FISCAL_YEARS_TARGET_FISCAL_YEAR_ID",
                        column: x => x.TARGET_FISCAL_YEAR_ID,
                        principalTable: "GL_FISCAL_YEARS",
                        principalColumn: "FISCAL_YEAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_JOURNAL_BATCHES_CLOSING_J_2139CEF2",
                        column: x => x.CLOSING_JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_JOURNAL_BATCHES_OPENING_B_526BC385",
                        column: x => x.OPENING_BALANCE_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                columns: table => new
                {
                    ACCOUNT_LOOKUP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOOKUP_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LOOKUP_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    LOOKUP_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LOOKUP_RESULT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DEFAULT_ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEFAULT_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    NO_MATCH_BEHAVIOR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_ACCOUNT_LOOKUPS", x => x.ACCOUNT_LOOKUP_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUPS_GL_ACCOUNTING_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_ID,
                        principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                        principalColumn: "EVENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUPS_GL_ACCOUNTS",
                        column: x => x.DEFAULT_ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUPS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.DEFAULT_ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUPS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUPS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ACCOUNT_GROUP_LINES",
                columns: table => new
                {
                    ACCOUNT_GROUP_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCOUNT_GROUP_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_CODE_FROM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACCOUNT_CODE_TO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    INCLUDE_EXCLUDE_FLAG = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PRIORITY_NO = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCOUNT_GROUP_LINES", x => x.ACCOUNT_GROUP_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCOUNT_GROUP_LINES_GL_ACCOUNTS",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCOUNT_GROUP_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCOUNT_GROUP_LINES_GL_CLOSE_ACCOUNT_GROUPS",
                        column: x => x.ACCOUNT_GROUP_ID,
                        principalTable: "GL_CLOSE_ACCOUNT_GROUPS",
                        principalColumn: "ACCOUNT_GROUP_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_IMPORT_REQUESTS",
                columns: table => new
                {
                    IMPORT_REQUEST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUEST_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ORACLE_REQUEST_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ORACLE_PROCESS_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REQUEST_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OUTPUT_REFERENCE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LOG_REFERENCE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_IMPORT_REQUESTS", x => x.IMPORT_REQUEST_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_REQUESTS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_RECONCILIATIONS",
                columns: table => new
                {
                    RECONCILIATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    RECONCILIATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FINOVA_TOTAL_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FINOVA_TOTAL_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ORACLE_IMPORTED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ORACLE_IMPORTED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DIFFERENCE_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DIFFERENCE_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RECONCILED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECONCILED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_RECONCILIATIONS", x => x.RECONCILIATION_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_RECONCILIATIONS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ACTION_HIST",
                columns: table => new
                {
                    CLOSE_ACTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACTION_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ACTION_FROM_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACTION_TO_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACTION_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ACTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REFERENCE_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REFERENCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACTION_HIST", x => x.CLOSE_ACTION_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACTION_HIST_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACTION_HIST_GL_CLOSE_RUN_TASKS",
                        column: x => x.CLOSE_RUN_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ATTACHMENTS",
                columns: table => new
                {
                    CLOSE_ATTACHMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLOSE_REPORT_ID = table.Column<long>(type: "bigint", nullable: true),
                    FILE_NAME = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: true),
                    FILE_CLOB = table.Column<string>(type: "text", nullable: true),
                    ATTACHMENT_CATEGORY = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UPLOADED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPLOADED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ATTACHMENTS", x => x.CLOSE_ATTACHMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ATTACHMENTS_GL_CLOSE_REPORTS",
                        column: x => x.CLOSE_REPORT_ID,
                        principalTable: "GL_CLOSE_REPORTS",
                        principalColumn: "CLOSE_REPORT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ATTACHMENTS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ATTACHMENTS_GL_CLOSE_RUN_TASKS",
                        column: x => x.CLOSE_RUN_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_VALIDATION_RESULTS",
                columns: table => new
                {
                    VALIDATION_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_BLOCKING = table.Column<bool>(type: "boolean", nullable: false),
                    RESULT_COUNT = table.Column<int>(type: "integer", nullable: true),
                    RESULT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    THRESHOLD_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VARIANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RESULT_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DETAIL_REFERENCE_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DETAIL_REFERENCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    DETAIL_REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXECUTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXECUTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESOLVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESOLVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESOLUTION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_VALIDATION_RESULTS", x => x.VALIDATION_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_VALIDATION_RESULTS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_VALIDATION_RESULTS_GL_CLOSE_RUN_TASKS",
                        column: x => x.CLOSE_RUN_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_VALIDATION_RESULTS_GL_CLOSE_VALIDATION_RULES",
                        column: x => x.VALIDATION_RULE_ID,
                        principalTable: "GL_CLOSE_VALIDATION_RULES",
                        principalColumn: "VALIDATION_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_BALANCES",
                columns: table => new
                {
                    BALANCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OPENING_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    OPENING_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    PERIOD_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    PERIOD_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CLOSING_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CLOSING_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    LAST_POSTED_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LAST_POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_BALANCES", x => x.BALANCE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_BALANCES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_BALANCES_GL_JOURNAL_HEADERS",
                        column: x => x.LAST_POSTED_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_BALANCES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_BALANCES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_EVENTS",
                columns: table => new
                {
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EVENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EVENT_AMOUNT = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_TRANSACTION_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    EVENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SELECTED_RULE_SET_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_PAYLOAD_JSON = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_EVENTS", x => x.ACCOUNTING_EVENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENTS_GL_ACCOUNTING_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_ID,
                        principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                        principalColumn: "EVENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENTS_GL_AUTO_ACCOUNTING_RULE_SETS",
                        column: x => x.SELECTED_RULE_SET_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_SETS",
                        principalColumn: "RULE_SET_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENTS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENTS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ACCRUAL_RUNS",
                columns: table => new
                {
                    ACCRUAL_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCRUAL_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    REVERSAL_JOURNAL_ID = table.Column<long>(type: "bigint", nullable: true),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TOTAL_DEBIT = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    TOTAL_CREDIT = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    GENERATED_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    REVERSAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCRUAL_RUNS", x => x.ACCRUAL_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RUNS_GL_CLOSE_ACCRUAL_RULES",
                        column: x => x.ACCRUAL_RULE_ID,
                        principalTable: "GL_CLOSE_ACCRUAL_RULES",
                        principalColumn: "ACCRUAL_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RUNS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RUNS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RUNS_GL_JOURNAL_HEADERS_JOURNAL_HEADER_ID",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_RUNS_GL_JOURNAL_HEADERS_REVERSAL_JOURNAL_ID",
                        column: x => x.REVERSAL_JOURNAL_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_APPROVAL_HIST",
                columns: table => new
                {
                    APPROVAL_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPROVAL_STEP_NO = table.Column<int>(type: "integer", nullable: false),
                    APPROVER_ROLE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    APPROVER_USER = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_NOTES = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DELEGATED_FROM_USER = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_APPROVAL_HIST", x => x.APPROVAL_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_APPROVAL_HIST_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_APPROVAL_HIST_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_LINES",
                columns: table => new
                {
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    CONVERSION_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_LINE_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_LINE_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    STATISTICAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_RECONCILIATION = table.Column<bool>(type: "boolean", nullable: false),
                    IS_CLEARED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_LINES", x => x.JOURNAL_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_LINES_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_LINES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_LINES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_EXPORT_HEADERS",
                columns: table => new
                {
                    EXPORT_HEADER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ORACLE_JOURNAL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ORACLE_JOURNAL_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ORACLE_JE_SOURCE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ORACLE_JE_CATEGORY_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PERIOD_NAME = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    HEADER_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ORACLE_JE_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_JE_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    DUPLICATE_KEY = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_EXPORT_HEADERS", x => x.EXPORT_HEADER_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_HEADERS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_HEADERS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_HEADERS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_SOURCE_FEEDBACKS",
                columns: table => new
                {
                    FEEDBACK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ORACLE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ORACLE_JE_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_JE_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FEEDBACK_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    IS_FEEDBACK_SENT = table.Column<bool>(type: "boolean", nullable: false),
                    FEEDBACK_SENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_SOURCE_FEEDBACKS", x => x.FEEDBACK_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_SOURCE_FEEDBACKS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_SOURCE_FEEDBACKS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RECURRING_JOURNAL_RUNS",
                columns: table => new
                {
                    RECURRING_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RECURRING_TEMPLATE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    GENERATION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RECURRING_JOURNAL_RUNS", x => x.RECURRING_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_RUNS_GL_JOURNAL_HEADERS",
                        column: x => x.GENERATED_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RECURRING_JOURNAL_RUNS_GL_RECURRING_JOURNAL_TEMPLATES",
                        column: x => x.RECURRING_TEMPLATE_ID,
                        principalTable: "GL_RECURRING_JOURNAL_TEMPLATES",
                        principalColumn: "RECURRING_TEMPLATE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_BALANCE_REBUILD_LINES",
                columns: table => new
                {
                    BALANCE_REBUILD_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BALANCE_REBUILD_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    OLD_PERIOD_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    OLD_PERIOD_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    NEW_PERIOD_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    NEW_PERIOD_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    REBUILD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_BALANCE_REBUILD_LINES", x => x.BALANCE_REBUILD_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_LINES_GL_BALANCE_REBUILD_RUNS",
                        column: x => x.BALANCE_REBUILD_RUN_ID,
                        principalTable: "GL_BALANCE_REBUILD_RUNS",
                        principalColumn: "BALANCE_REBUILD_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_BALANCE_REBUILD_LINES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_FX_REVALUATION_RUNS",
                columns: table => new
                {
                    REVALUATION_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REVALUATION_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    RUN_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVALUATION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    TOTAL_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    TOTAL_GAIN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_LOSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    REVERSAL_JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REVERSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REVERSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_FX_REVALUATION_RUNS", x => x.REVALUATION_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RUNS_GL_FX_REVALUATION_RULES",
                        column: x => x.REVALUATION_RULE_ID,
                        principalTable: "GL_FX_REVALUATION_RULES",
                        principalColumn: "REVALUATION_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_RUNS_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_OPENING_BALANCE_RUNS",
                columns: table => new
                {
                    OPENING_BALANCE_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YEAR_END_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    TARGET_FISCAL_YEAR_ID = table.Column<long>(type: "bigint", nullable: false),
                    TARGET_LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TOTAL_DR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    TOTAL_CR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    GENERATED_LINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    GENERATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_OPENING_BALANCE_RUNS", x => x.OPENING_BALANCE_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_RUNS_GL_FISCAL_YEARS",
                        column: x => x.TARGET_FISCAL_YEAR_ID,
                        principalTable: "GL_FISCAL_YEARS",
                        principalColumn: "FISCAL_YEAR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_RUNS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.TARGET_LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_RUNS_GL_YEAR_END_CLOSE_RUNS",
                        column: x => x.YEAR_END_RUN_ID,
                        principalTable: "GL_YEAR_END_CLOSE_RUNS",
                        principalColumn: "YEAR_END_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                columns: table => new
                {
                    LOOKUP_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCOUNT_LOOKUP_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRIORITY_NO = table.Column<int>(type: "integer", nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    BRANCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    COST_CENTER_ID = table.Column<long>(type: "bigint", nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PRODUCT_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    TRANSACTION_TYPE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CUSTOMER_TYPE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    PORTFOLIO_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    FUNDING_SOURCE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RESULT_ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_COMPANY_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_BRANCH_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_COST_CENTER_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_BUSINESS_LINE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_FUTURE1_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_FUTURE2_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES", x => x.LOOKUP_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_FIN_BRANCHES",
                        column: x => x.BRANCH_ID,
                        principalTable: "FIN_BRANCHES",
                        principalColumn: "BRANCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_FIN_COST_CENTERS",
                        column: x => x.COST_CENTER_ID,
                        principalTable: "FIN_COST_CENTERS",
                        principalColumn: "COST_CENTER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_FIN_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_FIN_LEGAL_ENTITIES",
                        column: x => x.LEGAL_ENTITY_ID,
                        principalTable: "FIN_LEGAL_ENTITIES",
                        principalColumn: "LEGAL_ENTITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_ACCOUNTING_AC_E0A7AEAB",
                        column: x => x.ACCOUNT_LOOKUP_ID,
                        principalTable: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                        principalColumn: "ACCOUNT_LOOKUP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_ACCOUNTS",
                        column: x => x.RESULT_ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.RESULT_ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_10597DB0",
                        column: x => x.RESULT_COST_CENTER_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_1C09356D",
                        column: x => x.RESULT_FUTURE2_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_5597476C",
                        column: x => x.RESULT_FUTURE1_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_7FE42FFF",
                        column: x => x.RESULT_BUSINESS_LINE_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_85B38799",
                        column: x => x.RESULT_BRANCH_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_GL_COA_SEGMENT_V_EC6D6EE2",
                        column: x => x.RESULT_COMPANY_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_ACCOUNTING_RULE_LINES",
                columns: table => new
                {
                    RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    LINE_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LINE_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    DR_CR_FLAG = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    LINE_ROLE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ACCOUNT_DERIVATION_METHOD = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FIXED_ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    FIXED_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_LOOKUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    AMOUNT_CALCULATION_METHOD = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AMOUNT_SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    AMOUNT_VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    FIXED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FIXED_AMOUNT_CURRENCY = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PERCENTAGE_BASE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    PERCENTAGE_BASE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    PERCENTAGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    FORMULA_EXPRESSION = table.Column<string>(type: "text", nullable: true),
                    MINIMUM_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAXIMUM_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SIGN_HANDLING = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ROUNDING_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ROUNDING_PRECISION = table.Column<int>(type: "integer", nullable: false),
                    CURRENCY_DERIVATION_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CURRENCY_SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    FIXED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    LINE_DESCRIPTION_TEMPLATE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ZERO_AMOUNT_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_ACCOUNTING_RULE_LINES", x => x.RULE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_FIN_CURRENCIES_FIXED__44909B9B",
                        column: x => x.FIXED_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_FIN_CURRENCIES_FIXED__730070DE",
                        column: x => x.FIXED_AMOUNT_CURRENCY,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                        column: x => x.ACCOUNT_LOOKUP_ID,
                        principalTable: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                        principalColumn: "ACCOUNT_LOOKUP_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_ACCOUNTS",
                        column: x => x.FIXED_ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.FIXED_ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_AUTO_ACCOUNTING_RULE_SETS",
                        column: x => x.RULE_SET_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_SETS",
                        principalColumn: "RULE_SET_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_SOURCE_VALUE_DEFIN_094D92C1",
                        column: x => x.PERCENTAGE_BASE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_SOURCE_VALUE_DEFIN_4F0B4677",
                        column: x => x.CURRENCY_SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_SOURCE_VALUE_DEFIN_631B83F2",
                        column: x => x.ACCOUNT_SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_RULE_LINES_GL_SOURCE_VALUE_DEFIN_635DACA9",
                        column: x => x.AMOUNT_SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_EXCEPTIONS",
                columns: table => new
                {
                    CLOSE_EXCEPTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALIDATION_RESULT_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXCEPTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EXCEPTION_TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EXCEPTION_DESCRIPTION = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SEVERITY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EXCEPTION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_BLOCKING = table.Column<bool>(type: "boolean", nullable: false),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TARGET_RESOLUTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESOLVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESOLVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESOLUTION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_WAIVER = table.Column<bool>(type: "boolean", nullable: false),
                    WAIVER_APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WAIVER_APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WAIVER_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_EXCEPTIONS", x => x.CLOSE_EXCEPTION_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_EXCEPTIONS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_EXCEPTIONS_GL_CLOSE_RUN_TASKS",
                        column: x => x.CLOSE_RUN_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_EXCEPTIONS_GL_CLOSE_VALIDATION_RESULTS",
                        column: x => x.VALIDATION_RESULT_ID,
                        principalTable: "GL_CLOSE_VALIDATION_RESULTS",
                        principalColumn: "VALIDATION_RESULT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNTING_EVENT_VALUES",
                columns: table => new
                {
                    EVENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    VALUE_SEQUENCE = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    VALUE_KIND = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AMOUNT_VALUE = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    NUMBER_VALUE = table.Column<decimal>(type: "numeric(22,8)", precision: 22, scale: 8, nullable: true),
                    PERCENT_VALUE = table.Column<decimal>(type: "numeric(12,8)", precision: 12, scale: 8, nullable: true),
                    TEXT_VALUE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DATE_VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JSON_VALUE = table.Column<string>(type: "text", nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNTING_EVENT_VALUES", x => x.EVENT_VALUE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_FIN_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_GL_ACCOUNTING_EVENTS",
                        column: x => x.ACCOUNTING_EVENT_ID,
                        principalTable: "GL_ACCOUNTING_EVENTS",
                        principalColumn: "ACCOUNTING_EVENT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_GL_ACCOUNTS",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_GL_COA_SEGMENT_VALUES",
                        column: x => x.SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNTING_EVENT_VALUES_GL_SOURCE_VALUE_DEFINITIONS",
                        column: x => x.SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_ACCOUNTING_GENERATIONS",
                columns: table => new
                {
                    GENERATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GENERATION_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_SET_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    GENERATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GENERATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GENERATED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TOTAL_ENTERED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    TOTAL_ENTERED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    TOTAL_ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    TOTAL_ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    INTERFACE_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_ACCOUNTING_GENERATIONS", x => x.GENERATION_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATIONS_GL_ACCOUNTING_EVENTS",
                        column: x => x.ACCOUNTING_EVENT_ID,
                        principalTable: "GL_ACCOUNTING_EVENTS",
                        principalColumn: "ACCOUNTING_EVENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATIONS_GL_AUTO_ACCOUNTING_RULE_SETS",
                        column: x => x.RULE_SET_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_SETS",
                        principalColumn: "RULE_SET_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATIONS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATIONS_GL_JOURNAL_INTERFACE_BATCHES",
                        column: x => x.INTERFACE_BATCH_ID,
                        principalTable: "GL_JOURNAL_INTERFACE_BATCHES",
                        principalColumn: "INTERFACE_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATIONS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_ACCRUAL_LINES",
                columns: table => new
                {
                    ACCRUAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACCRUAL_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCRUAL_RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    DR_CR_FLAG = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ENTERED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ENTERED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ACCOUNTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_REFERENCE_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_REFERENCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AMOUNT_SOURCE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AMOUNT_SOURCE_VALUE = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    IS_GENERATED = table.Column<bool>(type: "boolean", nullable: false),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_ACCRUAL_LINES", x => x.ACCRUAL_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_LINES_GL_CLOSE_ACCRUAL_RULE_LINES",
                        column: x => x.ACCRUAL_RULE_LINE_ID,
                        principalTable: "GL_CLOSE_ACCRUAL_RULE_LINES",
                        principalColumn: "ACCRUAL_RULE_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_LINES_GL_CLOSE_ACCRUAL_RUNS",
                        column: x => x.ACCRUAL_RUN_ID,
                        principalTable: "GL_CLOSE_ACCRUAL_RUNS",
                        principalColumn: "ACCRUAL_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_ACCRUAL_LINES_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_ACTION_HIST",
                columns: table => new
                {
                    ACTION_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACTION_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    OLD_STATUS = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    NEW_STATUS = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ACTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ACTION_NOTES = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ACTION_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_ACTION_HIST", x => x.ACTION_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ACTION_HIST_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ACTION_HIST_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ACTION_HIST_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_ATTACHMENTS",
                columns: table => new
                {
                    ATTACHMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FILE_SIZE_BYTES = table.Column<int>(type: "integer", nullable: true),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: true),
                    FILE_URL = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ATTACHMENT_CATEGORY = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_ATTACHMENTS", x => x.ATTACHMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ATTACHMENTS_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ATTACHMENTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_ATTACHMENTS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_COMMENTS",
                columns: table => new
                {
                    COMMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COMMENT_TEXT = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IS_INTERNAL = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_COMMENTS", x => x.COMMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_COMMENTS_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_COMMENTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_COMMENTS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_INTERFACE_LINES",
                columns: table => new
                {
                    INTERFACE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INTERFACE_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_LINE_REF = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    JOURNAL_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CATEGORY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMBINATION_CODE = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    ENTERED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACCOUNTED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    RATE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CONVERSION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONVERSION_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_ERROR_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    VALIDATION_ERROR_MSG = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    GENERATED_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    GENERATED_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_INTERFACE_LINES", x => x.INTERFACE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_LINES_GL_ACCOUNTING_EVENTS",
                        column: x => x.ACCOUNTING_EVENT_ID,
                        principalTable: "GL_ACCOUNTING_EVENTS",
                        principalColumn: "ACCOUNTING_EVENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_LINES_GL_JOURNAL_HEADERS",
                        column: x => x.GENERATED_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_LINES_GL_JOURNAL_INTERFACE_BATCHES",
                        column: x => x.INTERFACE_BATCH_ID,
                        principalTable: "GL_JOURNAL_INTERFACE_BATCHES",
                        principalColumn: "INTERFACE_BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_INTERFACE_LINES_GL_JOURNAL_LINES",
                        column: x => x.GENERATED_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_VALIDATION_RESULTS",
                columns: table => new
                {
                    VALIDATION_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SEVERITY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RESULT_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RESOLVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RESOLVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_VALIDATION_RESULTS", x => x.VALIDATION_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RESULTS_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RESULTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RESULTS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_VALIDATION_RESULTS_GL_JOURNAL_VALIDATION_RULES",
                        column: x => x.VALIDATION_RULE_ID,
                        principalTable: "GL_JOURNAL_VALIDATION_RULES",
                        principalColumn: "VALIDATION_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_RETAINED_EARNINGS_TRANSFERS",
                columns: table => new
                {
                    RETAINED_TRANSFER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YEAR_END_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    RETAINED_EARNINGS_ACCOUNT_COMB_ID = table.Column<long>(type: "bigint", nullable: false),
                    NET_PROFIT_LOSS = table.Column<int>(type: "integer", nullable: false),
                    TRANSFER_DR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TRANSFER_CR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    TRANSFER_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_RETAINED_EARNINGS_TRANSFERS", x => x.RETAINED_TRANSFER_ID);
                    table.ForeignKey(
                        name: "FK_GL_RETAINED_EARNINGS_TRANSFERS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.RETAINED_EARNINGS_ACCOUNT_COMB_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RETAINED_EARNINGS_TRANSFERS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RETAINED_EARNINGS_TRANSFERS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_RETAINED_EARNINGS_TRANSFERS_GL_YEAR_END_CLOSE_RUNS",
                        column: x => x.YEAR_END_RUN_ID,
                        principalTable: "GL_YEAR_END_CLOSE_RUNS",
                        principalColumn: "YEAR_END_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_SOURCE_POSTING_LINKS",
                columns: table => new
                {
                    SOURCE_POSTING_LINK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    REVERSAL_LINK_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SOURCE_POSTING_LINKS", x => x.SOURCE_POSTING_LINK_ID);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_ACCOUNTING_EVENTS",
                        column: x => x.ACCOUNTING_EVENT_ID,
                        principalTable: "GL_ACCOUNTING_EVENTS",
                        principalColumn: "ACCOUNTING_EVENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_SOURCE_POSTING_LINKS_GL_SOURCE_POSTING_LINKS",
                        column: x => x.REVERSAL_LINK_ID,
                        principalTable: "GL_SOURCE_POSTING_LINKS",
                        principalColumn: "SOURCE_POSTING_LINK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_YEAR_END_CLOSE_LINES",
                columns: table => new
                {
                    YEAR_END_CLOSE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YEAR_END_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_GROUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CLOSING_BEHAVIOR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BEGINNING_BALANCE_DR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    BEGINNING_BALANCE_CR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    PERIOD_DEBIT = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    PERIOD_CREDIT = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    ENDING_BALANCE_DR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    ENDING_BALANCE_CR = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    IS_CLOSE_TO_RETAINED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_CARRY_FORWARD = table.Column<bool>(type: "boolean", nullable: false),
                    CLOSING_JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    OPENING_JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_YEAR_END_CLOSE_LINES", x => x.YEAR_END_CLOSE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_LINES_GL_CLOSE_ACCOUNT_GROUPS",
                        column: x => x.ACCOUNT_GROUP_ID,
                        principalTable: "GL_CLOSE_ACCOUNT_GROUPS",
                        principalColumn: "ACCOUNT_GROUP_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_LINES_GL_JOURNAL_LINES_CLOSING_JO_43EA2497",
                        column: x => x.CLOSING_JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_LINES_GL_JOURNAL_LINES_OPENING_JO_E6127A32",
                        column: x => x.OPENING_JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_YEAR_END_CLOSE_LINES_GL_YEAR_END_CLOSE_RUNS",
                        column: x => x.YEAR_END_RUN_ID,
                        principalTable: "GL_YEAR_END_CLOSE_RUNS",
                        principalColumn: "YEAR_END_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_EXPORT_LINES",
                columns: table => new
                {
                    EXPORT_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    ORACLE_CODE_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_CONCATENATED_ACCOUNT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REFERENCE1 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE2 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE3 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE4 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE5 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE6 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE7 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE8 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE9 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE10 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_EXPORT_LINES", x => x.EXPORT_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_LINES_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_EXPORT_LINES_GL_ORACLE_GL_EXPORT_HEADERS",
                        column: x => x.EXPORT_HEADER_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_HEADERS",
                        principalColumn: "EXPORT_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_FX_REVALUATION_LINES",
                columns: table => new
                {
                    REVALUATION_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REVALUATION_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_BALANCE = table.Column<decimal>(type: "numeric(24,6)", precision: 24, scale: 6, nullable: false),
                    ACCOUNTED_BALANCE = table.Column<decimal>(type: "numeric(24,6)", precision: 24, scale: 6, nullable: false),
                    PERIOD_END_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    REVALUED_BALANCE = table.Column<decimal>(type: "numeric(24,6)", precision: 24, scale: 6, nullable: false),
                    GAIN_LOSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GAIN_LOSS_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GAIN_LOSS_ACCOUNT_COMB_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_REFERENCE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SOURCE_REFERENCE_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_FX_REVALUATION_LINES", x => x.REVALUATION_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_LINES_FIN_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_LINES_GL_ACCOUNT_COMBINATIONS_ACC_0E62AD9D",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_LINES_GL_ACCOUNT_COMBINATIONS_GAI_DEF83B41",
                        column: x => x.GAIN_LOSS_ACCOUNT_COMB_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_FX_REVALUATION_LINES_GL_FX_REVALUATION_RUNS",
                        column: x => x.REVALUATION_RUN_ID,
                        principalTable: "GL_FX_REVALUATION_RUNS",
                        principalColumn: "REVALUATION_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_OPENING_BALANCE_LINES",
                columns: table => new
                {
                    OPENING_BALANCE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OPENING_BALANCE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    OPENING_DR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OPENING_CR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SOURCE_ENDING_DR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SOURCE_ENDING_CR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_OPENING_BALANCE_LINES", x => x.OPENING_BALANCE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_LINES_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_OPENING_BALANCE_LINES_GL_OPENING_BALANCE_RUNS",
                        column: x => x.OPENING_BALANCE_RUN_ID,
                        principalTable: "GL_OPENING_BALANCE_RUNS",
                        principalColumn: "OPENING_BALANCE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_RULE_LINE_SEGMENTS",
                columns: table => new
                {
                    RULE_LINE_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    COA_SEGMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DERIVATION_METHOD = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FIXED_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_VALUE_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    LOOKUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXPRESSION_TEXT = table.Column<string>(type: "text", nullable: true),
                    DEFAULT_SEGMENT_VALUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_RULE_LINE_SEGMENTS", x => x.RULE_LINE_SEGMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                        column: x => x.LOOKUP_ID,
                        principalTable: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                        principalColumn: "ACCOUNT_LOOKUP_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_AUTO_ACCOUNTING_RULE_LINES",
                        column: x => x.RULE_LINE_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_LINES",
                        principalColumn: "RULE_LINE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_COA_SEGMENTS",
                        column: x => x.COA_SEGMENT_ID,
                        principalTable: "GL_COA_SEGMENTS",
                        principalColumn: "COA_SEGMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_COA_SEGMENT_VALUES_DE_D80E22FC",
                        column: x => x.DEFAULT_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_COA_SEGMENT_VALUES_FI_65E2839E",
                        column: x => x.FIXED_SEGMENT_VALUE_ID,
                        principalTable: "GL_COA_SEGMENT_VALUES",
                        principalColumn: "SEGMENT_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_RULE_LINE_SEGMENTS_GL_SOURCE_VALUE_DEFINITIONS",
                        column: x => x.SOURCE_VALUE_ID,
                        principalTable: "GL_SOURCE_VALUE_DEFINITIONS",
                        principalColumn: "SOURCE_VALUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_CLOSE_COMMENTS",
                columns: table => new
                {
                    CLOSE_COMMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLOSE_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLOSE_RUN_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLOSE_EXCEPTION_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMMENT_TEXT = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IS_INTERNAL = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLOSE_COMMENTS", x => x.CLOSE_COMMENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_COMMENTS_GL_CLOSE_EXCEPTIONS",
                        column: x => x.CLOSE_EXCEPTION_ID,
                        principalTable: "GL_CLOSE_EXCEPTIONS",
                        principalColumn: "CLOSE_EXCEPTION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_COMMENTS_GL_CLOSE_RUNS",
                        column: x => x.CLOSE_RUN_ID,
                        principalTable: "GL_CLOSE_RUNS",
                        principalColumn: "CLOSE_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_CLOSE_COMMENTS_GL_CLOSE_RUN_TASKS",
                        column: x => x.CLOSE_RUN_TASK_ID,
                        principalTable: "GL_CLOSE_RUN_TASKS",
                        principalColumn: "CLOSE_RUN_TASK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                columns: table => new
                {
                    GENERATED_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GENERATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    DR_CR_FLAG = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMBINATION_CODE = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ENTERED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNTED_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ACCOUNTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RATE_TYPE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CONVERSION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONVERSION_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    AMOUNT_CALCULATION_TRACE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ACCOUNT_DERIVATION_TRACE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_ERROR_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    VALIDATION_ERROR_MSG = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    INTERFACE_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_AUTO_ACCOUNTING_GENERATED_LINES", x => x.GENERATED_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_FIN_CURRENCIES_A_423505EC",
                        column: x => x.ACCOUNTED_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_FIN_CURRENCIES_E_85A2A3EE",
                        column: x => x.ENTERED_CURRENCY_CODE,
                        principalTable: "FIN_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_ACCOUNTS",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_AUTO_ACCOUNTI_67EB98C1",
                        column: x => x.RULE_LINE_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_RULE_LINES",
                        principalColumn: "RULE_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_AUTO_ACCOUNTI_8152D322",
                        column: x => x.GENERATION_ID,
                        principalTable: "GL_AUTO_ACCOUNTING_GENERATIONS",
                        principalColumn: "GENERATION_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_JOURNAL_INTER_9260021F",
                        column: x => x.INTERFACE_LINE_ID,
                        principalTable: "GL_JOURNAL_INTERFACE_LINES",
                        principalColumn: "INTERFACE_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_AUTO_ACCOUNTING_GENERATED_LINES_GL_RATE_TYPES",
                        column: x => x.RATE_TYPE_ID,
                        principalTable: "GL_RATE_TYPES",
                        principalColumn: "RATE_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_INTERFACE_ROWS",
                columns: table => new
                {
                    INTERFACE_ROW_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    GROUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    INTERFACE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ORACLE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEDGER_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SET_OF_BOOKS_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    IS_ACTUAL = table.Column<bool>(type: "boolean", nullable: false),
                    USER_JE_SOURCE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    USER_JE_CATEGORY_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CODE_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    SEGMENT1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT4 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT5 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT6 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT7 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT8 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT9 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT10 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT11 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT12 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT13 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT14 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT15 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT16 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT17 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT18 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT19 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT20 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT21 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT22 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT23 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT24 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT25 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT26 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT27 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT28 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT29 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SEGMENT30 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REFERENCE1 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE2 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE3 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE4 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE5 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE6 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE7 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE8 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE9 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    REFERENCE10 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    ATTRIBUTE_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    ORACLE_INTERFACE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ERROR_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EXPORTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IMPORTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_INTERFACE_ROWS", x => x.INTERFACE_ROW_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_INTERFACE_ROWS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_INTERFACE_ROWS_GL_ORACLE_GL_EXPORT_LINES",
                        column: x => x.EXPORT_LINE_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_LINES",
                        principalColumn: "EXPORT_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_VALIDATION_RESULTS",
                columns: table => new
                {
                    VALIDATION_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXPORT_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALIDATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VALIDATION_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SEVERITY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VALIDATION_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_VALIDATION_RESULTS", x => x.VALIDATION_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_VALIDATION_RESULTS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_VALIDATION_RESULTS_GL_ORACLE_GL_EXPORT_HEADERS",
                        column: x => x.EXPORT_HEADER_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_HEADERS",
                        principalColumn: "EXPORT_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_VALIDATION_RESULTS_GL_ORACLE_GL_EXPORT_LINES",
                        column: x => x.EXPORT_LINE_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_LINES",
                        principalColumn: "EXPORT_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_ORACLE_GL_IMPORT_RESULTS",
                columns: table => new
                {
                    IMPORT_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IMPORT_REQUEST_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    EXPORT_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXPORT_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    INTERFACE_ROW_ID = table.Column<long>(type: "bigint", nullable: true),
                    RESULT_LEVEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ORACLE_JE_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_JE_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ORACLE_JE_LINE_NUM = table.Column<int>(type: "integer", nullable: true),
                    ORACLE_PERIOD_NAME = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ERROR_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ORACLE_GL_IMPORT_RESULTS", x => x.IMPORT_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_RESULTS_GL_ORACLE_GL_EXPORT_BATCHES",
                        column: x => x.EXPORT_BATCH_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_BATCHES",
                        principalColumn: "EXPORT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_RESULTS_GL_ORACLE_GL_EXPORT_HEADERS",
                        column: x => x.EXPORT_HEADER_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_HEADERS",
                        principalColumn: "EXPORT_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_RESULTS_GL_ORACLE_GL_EXPORT_LINES",
                        column: x => x.EXPORT_LINE_ID,
                        principalTable: "GL_ORACLE_GL_EXPORT_LINES",
                        principalColumn: "EXPORT_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_RESULTS_GL_ORACLE_GL_IMPORT_REQUESTS",
                        column: x => x.IMPORT_REQUEST_ID,
                        principalTable: "GL_ORACLE_GL_IMPORT_REQUESTS",
                        principalColumn: "IMPORT_REQUEST_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ORACLE_GL_IMPORT_RESULTS_GL_ORACLE_GL_INTERFACE_ROWS",
                        column: x => x.INTERFACE_ROW_ID,
                        principalTable: "GL_ORACLE_GL_INTERFACE_ROWS",
                        principalColumn: "INTERFACE_ROW_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_BALANCE_IMPACTS",
                columns: table => new
                {
                    BALANCE_IMPACT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    POSTING_RUN_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    BALANCE_BEFORE_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    BALANCE_BEFORE_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    BALANCE_AFTER_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    BALANCE_AFTER_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: true),
                    IMPACT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REVERSAL_IMPACT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_BALANCE_IMPACTS", x => x.BALANCE_IMPACT_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_POSTING_BALANCE_IMPACTS",
                        column: x => x.REVERSAL_IMPACT_ID,
                        principalTable: "GL_POSTING_BALANCE_IMPACTS",
                        principalColumn: "BALANCE_IMPACT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_EXCEPTIONS",
                columns: table => new
                {
                    POSTING_EXCEPTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXCEPTION_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EXCEPTION_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EXCEPTION_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RESOLUTION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESOLVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESOLVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RESOLUTION_NOTES = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_EXCEPTIONS", x => x.POSTING_EXCEPTION_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_EXCEPTIONS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_EXCEPTIONS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_EXCEPTIONS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_LOCKS",
                columns: table => new
                {
                    POSTING_LOCK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOCK_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LOCK_KEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    LOCK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RELEASED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RELEASED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RELEASE_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EXPIRES_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_LOCKS", x => x.POSTING_LOCK_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_LOCKS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_LOCKS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_QUEUES",
                columns: table => new
                {
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QUEUE_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    POSTING_PROFILE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_TRANSACTION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REQUEST_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REQUEST_PRIORITY = table.Column<int>(type: "integer", nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    READY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LOCKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LOCKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HOLD_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ERROR_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RETRY_COUNT = table.Column<int>(type: "integer", nullable: false),
                    NEXT_RETRY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_QUEUES", x => x.QUEUE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_QUEUES_GL_JOURNAL_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_QUEUES_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_QUEUES_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_QUEUES_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_QUEUES_GL_POSTING_PROFILES",
                        column: x => x.POSTING_PROFILE_ID,
                        principalTable: "GL_POSTING_PROFILES",
                        principalColumn: "POSTING_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_RUNS",
                columns: table => new
                {
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    POSTING_RUN_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    POSTING_RUN_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RUN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUBMITTED_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    STARTED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TOTAL_HEADERS = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    POSTED_HEADERS = table.Column<int>(type: "integer", nullable: false),
                    ERROR_HEADERS = table.Column<int>(type: "integer", nullable: false),
                    TOTAL_LINES = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: false),
                    TOTAL_ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    TOTAL_ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    POSTING_PROFILE_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    BALANCE_UPDATE_MODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_RUNS", x => x.POSTING_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_JOURNAL_BATCHES",
                        column: x => x.SUBMITTED_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_JOURNAL_SOURCES",
                        column: x => x.SOURCE_ID,
                        principalTable: "GL_JOURNAL_SOURCES",
                        principalColumn: "SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_POSTING_PROFILES",
                        column: x => x.POSTING_PROFILE_ID,
                        principalTable: "GL_POSTING_PROFILES",
                        principalColumn: "POSTING_PROFILE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUNS_GL_POSTING_QUEUES",
                        column: x => x.QUEUE_ID,
                        principalTable: "GL_POSTING_QUEUES",
                        principalColumn: "QUEUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_RECONCILIATIONS",
                columns: table => new
                {
                    POSTING_RECON_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RECON_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: true),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    RECON_SCOPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JOURNAL_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    JOURNAL_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    IMPACT_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    IMPACT_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    BALANCE_TOTAL_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    BALANCE_TOTAL_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    DIFFERENCE_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    DIFFERENCE_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    RECON_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RECONCILED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECONCILED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_RECONCILIATIONS", x => x.POSTING_RECON_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RECONCILIATIONS_GL_LEDGERS",
                        column: x => x.LEDGER_ID,
                        principalTable: "GL_LEDGERS",
                        principalColumn: "LEDGER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RECONCILIATIONS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RECONCILIATIONS_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_ROLLBACK_RUNS",
                columns: table => new
                {
                    ROLLBACK_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ROLLBACK_RUN_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ROLLBACK_REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ROLLBACK_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ROLLBACK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_ROLLBACK_RUNS", x => x.ROLLBACK_RUN_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_RUNS_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_RUN_LINES",
                columns: table => new
                {
                    POSTING_RUN_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_CODE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PROCESSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_RUN_LINES", x => x.POSTING_RUN_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUN_LINES_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_RUN_LINES_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_SOURCE_FEEDBACKS",
                columns: table => new
                {
                    SOURCE_FEEDBACK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_TRANSACTION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GL_JOURNAL_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_REVERSAL = table.Column<bool>(type: "boolean", nullable: false),
                    FEEDBACK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FEEDBACK_ATTEMPTS = table.Column<int>(type: "integer", nullable: false),
                    LAST_ATTEMPT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PAYLOAD_JSON = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_SOURCE_FEEDBACKS", x => x.SOURCE_FEEDBACK_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SOURCE_FEEDBACKS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SOURCE_FEEDBACKS_GL_POSTING_QUEUES",
                        column: x => x.QUEUE_ID,
                        principalTable: "GL_POSTING_QUEUES",
                        principalColumn: "QUEUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_SOURCE_FEEDBACKS_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_STATUS_EVENTS",
                columns: table => new
                {
                    POSTING_STATUS_EVENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OLD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NEW_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EVENT_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EVENT_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_STATUS_EVENTS", x => x.POSTING_STATUS_EVENT_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_STATUS_EVENTS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_STATUS_EVENTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_STATUS_EVENTS_GL_POSTING_QUEUES",
                        column: x => x.QUEUE_ID,
                        principalTable: "GL_POSTING_QUEUES",
                        principalColumn: "QUEUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_STATUS_EVENTS_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_VALIDATION_RESULTS",
                columns: table => new
                {
                    POSTING_VALIDATION_RESULT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POSTING_RUN_ID = table.Column<long>(type: "bigint", nullable: true),
                    QUEUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: true),
                    POSTING_VALIDATION_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    VALIDATION_STAGE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RESULT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SEVERITY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RESULT_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CHECKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHECKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_VALIDATION_RESULTS", x => x.POSTING_VALIDATION_RESULT_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_JOURNAL_BATCHES",
                        column: x => x.JOURNAL_BATCH_ID,
                        principalTable: "GL_JOURNAL_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_POSTING_QUEUES",
                        column: x => x.QUEUE_ID,
                        principalTable: "GL_POSTING_QUEUES",
                        principalColumn: "QUEUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_POSTING_RUNS",
                        column: x => x.POSTING_RUN_ID,
                        principalTable: "GL_POSTING_RUNS",
                        principalColumn: "POSTING_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_VALIDATION_RESULTS_GL_POSTING_VALIDATION_RULES",
                        column: x => x.POSTING_VALIDATION_RULE_ID,
                        principalTable: "GL_POSTING_VALIDATION_RULES",
                        principalColumn: "POSTING_VALIDATION_RULE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_ROLLBACK_LINES",
                columns: table => new
                {
                    ROLLBACK_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ROLLBACK_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    BALANCE_IMPACT_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ROLLBACK_ACCOUNTED_DR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ROLLBACK_ACCOUNTED_CR = table.Column<decimal>(type: "numeric(22,5)", precision: 22, scale: 5, nullable: false),
                    ROLLBACK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_POSTING_ROLLBACK_LINES", x => x.ROLLBACK_LINE_ID);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_LINES_GL_ACCOUNT_COMBINATIONS",
                        column: x => x.ACCOUNT_COMBINATION_ID,
                        principalTable: "GL_ACCOUNT_COMBINATIONS",
                        principalColumn: "ACCOUNT_COMBINATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_LINES_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_LINES_GL_JOURNAL_LINES",
                        column: x => x.JOURNAL_LINE_ID,
                        principalTable: "GL_JOURNAL_LINES",
                        principalColumn: "JOURNAL_LINE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_LINES_GL_POSTING_BALANCE_IMPACTS",
                        column: x => x.BALANCE_IMPACT_ID,
                        principalTable: "GL_POSTING_BALANCE_IMPACTS",
                        principalColumn: "BALANCE_IMPACT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_POSTING_ROLLBACK_LINES_GL_POSTING_ROLLBACK_RUNS",
                        column: x => x.ROLLBACK_RUN_ID,
                        principalTable: "GL_POSTING_ROLLBACK_RUNS",
                        principalColumn: "ROLLBACK_RUN_ID",
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
                name: "IX_FIN_BRANCHES_LEGAL_ENTITY_ID_IS_ACTIVE",
                table: "FIN_BRANCHES",
                columns: new[] { "LEGAL_ENTITY_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_FIN_BRANCHES_PARENT_BRANCH_ID",
                table: "FIN_BRANCHES",
                column: "PARENT_BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FIN_BRANCHES_LEGAL_ENTITY_ID_BRANCH_CODE",
                table: "FIN_BRANCHES",
                columns: new[] { "LEGAL_ENTITY_ID", "BRANCH_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_FIN_BUSINESS_LINES_BUSINESS_LINE_CODE",
                table: "FIN_BUSINESS_LINES",
                column: "BUSINESS_LINE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FIN_COST_CENTERS_BRANCH_ID",
                table: "FIN_COST_CENTERS",
                column: "BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_COST_CENTERS_LEGAL_ENTITY_ID_BRANCH_ID_IS_ACTIVE",
                table: "FIN_COST_CENTERS",
                columns: new[] { "LEGAL_ENTITY_ID", "BRANCH_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_FIN_COST_CENTERS_PARENT_COST_CENTER_ID",
                table: "FIN_COST_CENTERS",
                column: "PARENT_COST_CENTER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FIN_COST_CENTERS_LEGAL_ENTITY_ID_COST_CENTER_CODE",
                table: "FIN_COST_CENTERS",
                columns: new[] { "LEGAL_ENTITY_ID", "COST_CENTER_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FIN_LEGAL_ENTITIES_BASE_CURRENCY_CODE",
                table: "FIN_LEGAL_ENTITIES",
                column: "BASE_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_FIN_LEGAL_ENTITIES_LEGAL_ENTITY_CODE",
                table: "FIN_LEGAL_ENTITIES",
                column: "LEGAL_ENTITY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNT_BALANCES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_LAST_POSTED_HEADER_ID",
                table: "GL_ACCOUNT_BALANCES",
                column: "LAST_POSTED_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_LEDGER_ID_LEDGER_PERIOD_ID_CURRENCY_CODE",
                table: "GL_ACCOUNT_BALANCES",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_LEDGER_PERIOD_ID",
                table: "GL_ACCOUNT_BALANCES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_BALANCES_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_ACCOUNT_BALANCES",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_RULE_LINES_SEGMENT_CODE_SEGMENT_TYPE",
                table: "GL_ACCOUNT_COMBINATION_RULE_LINES",
                columns: new[] { "SEGMENT_CODE", "SEGMENT_TYPE", "ATTRIBUTE_NAME" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATION_RULE_LINES_RULE_ID_LINE_NO",
                table: "GL_ACCOUNT_COMBINATION_RULE_LINES",
                columns: new[] { "RULE_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_RULES_COA_ID_IS_ACTIVE_PRIORITY_NO",
                table: "GL_ACCOUNT_COMBINATION_RULES",
                columns: new[] { "COA_ID", "IS_ACTIVE", "PRIORITY_NO" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_RULES_RULE_TYPE_RULE_SCOPE",
                table: "GL_ACCOUNT_COMBINATION_RULES",
                columns: new[] { "RULE_TYPE", "RULE_SCOPE", "VALIDATION_LEVEL" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATION_RULES_COA_ID_RULE_CODE",
                table: "GL_ACCOUNT_COMBINATION_RULES",
                columns: new[] { "COA_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_COA_SEGMENT_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: new[] { "COA_SEGMENT_ID", "SEGMENT_VALUE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_SEGMENT_VALUE_CODE",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                column: "SEGMENT_VALUE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                column: "SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATION_SEGMENTS_ACCOUNT_COMBINATION_E4E5497B",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "SEGMENT_NUM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATION_SEGMENTS_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "COA_SEGMENT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_ACCOUNT_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_ACCOUNT_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "ACCOUNT_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_BRANCH_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_BRANCH_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "BRANCH_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_BUSINESS_LINE_CODE_ACCOUNT_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "BUSINESS_LINE_CODE", "ACCOUNT_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_BUSINESS_LINE_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "BUSINESS_LINE_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_COA_ID_IS_ENABLED_VALIDATION_STATUS",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "COA_ID", "IS_ENABLED", "VALIDATION_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_COMPANY_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "COMPANY_SEGMENT_VALUE_ID", "ACCOUNT_SEGMENT_VALUE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_COST_CENTER_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "COST_CENTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_COST_CENTER_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "COST_CENTER_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_LEGAL_ENTITY_ID_BRANCH_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "LEGAL_ENTITY_ID", "BRANCH_ID", "COST_CENTER_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATIONS_COA_ID_CONCATENATED_ACCOUNT",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "COA_ID", "CONCATENATED_ACCOUNT" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_HIERARCHIES_COA_ID_HIERARCHY_TYPE_IS_ACTIVE",
                table: "GL_ACCOUNT_HIERARCHIES",
                columns: new[] { "COA_ID", "HIERARCHY_TYPE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_HIERARCHIES_COA_ID_HIERARCHY_CODE",
                table: "GL_ACCOUNT_HIERARCHIES",
                columns: new[] { "COA_ID", "HIERARCHY_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_HIERARCHY_NODES_ACCOUNT_ID",
                table: "GL_ACCOUNT_HIERARCHY_NODES",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_HIERARCHY_NODES_HIERARCHY_ID_PARENT_NODE_ID",
                table: "GL_ACCOUNT_HIERARCHY_NODES",
                columns: new[] { "HIERARCHY_ID", "PARENT_NODE_ID", "DISPLAY_SEQ" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_HIERARCHY_NODES_PARENT_NODE_ID",
                table: "GL_ACCOUNT_HIERARCHY_NODES",
                column: "PARENT_NODE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_HIERARCHY_NODES_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_HIERARCHY_NODES",
                column: "SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_HIERARCHY_NODES_HIERARCHY_ID_NODE_CODE",
                table: "GL_ACCOUNT_HIERARCHY_NODES",
                columns: new[] { "HIERARCHY_ID", "NODE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_ACCOUNT_LOOKUP_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                columns: new[] { "ACCOUNT_LOOKUP_ID", "IS_ACTIVE", "PRIORITY_NO" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_BRANCH_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_BUSINESS_LINE_CODE",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                columns: new[] { "BUSINESS_LINE_CODE", "PRODUCT_CODE", "TRANSACTION_TYPE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_COST_CENTER_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "COST_CENTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_CURRENCY_CODE",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_LEGAL_ENTITY_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_ACCOUNT_C_F789F181",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_ACCOUNT_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_BRANCH_VALUE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_BRANCH_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_BUSINESS__BDF54380",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_BUSINESS_LINE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_COMPANY_VALUE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_COMPANY_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_COST_CENT_6C77DAE5",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_COST_CENTER_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_FUTURE1_VALUE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_FUTURE1_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_RESULT_FUTURE2_VALUE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES",
                column: "RESULT_FUTURE2_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUPS_DEFAULT_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "DEFAULT_ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUPS_DEFAULT_ACCOUNT_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "DEFAULT_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUPS_EVENT_TYPE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "EVENT_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUPS_LEDGER_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_ACCOUNT_LOOKUPS_SOURCE_ID",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_ACCOUNT_LOOKUPS_LOOKUP_CODE",
                table: "GL_ACCOUNTING_ACCOUNT_LOOKUPS",
                column: "LOOKUP_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_TYPES_DEFAULT_CATEGORY_ID",
                table: "GL_ACCOUNTING_EVENT_TYPES",
                column: "DEFAULT_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_EVENT_TYPES_SOURCE_ID_EVENT_TYPE_CODE",
                table: "GL_ACCOUNTING_EVENT_TYPES",
                columns: new[] { "SOURCE_ID", "EVENT_TYPE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_ACCOUNT_ID",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_ACCOUNTING_EVENT_ID_VALUE_CODE",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                columns: new[] { "ACCOUNTING_EVENT_ID", "VALUE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_CURRENCY_CODE",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                column: "SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENT_VALUES_SOURCE_VALUE_ID",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                column: "SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_EVENT_VALUES_ACCOUNTING_EVENT_ID_VALUE_CODE",
                table: "GL_ACCOUNTING_EVENT_VALUES",
                columns: new[] { "ACCOUNTING_EVENT_ID", "VALUE_CODE", "VALUE_SEQUENCE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENTS_EVENT_TYPE_ID",
                table: "GL_ACCOUNTING_EVENTS",
                column: "EVENT_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENTS_JOURNAL_HEADER_ID",
                table: "GL_ACCOUNTING_EVENTS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENTS_LEDGER_ID_ACCOUNTING_DATE",
                table: "GL_ACCOUNTING_EVENTS",
                columns: new[] { "LEDGER_ID", "ACCOUNTING_DATE", "ACCOUNTING_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENTS_SELECTED_RULE_SET_ID",
                table: "GL_ACCOUNTING_EVENTS",
                column: "SELECTED_RULE_SET_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_EVENTS_SOURCE_MODULE_SOURCE_DOCUMENT_TYPE",
                table: "GL_ACCOUNTING_EVENTS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_EVENTS_LEDGER_ID_EVENT_NO",
                table: "GL_ACCOUNTING_EVENTS",
                columns: new[] { "LEDGER_ID", "EVENT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_EVENTS_SOURCE_ID_SOURCE_DOCUMENT_TYPE",
                table: "GL_ACCOUNTING_EVENTS",
                columns: new[] { "SOURCE_ID", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID", "EVENT_TYPE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_PERIODS_CALENDAR_ID_PERIOD_STATUS_START_DATE",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "CALENDAR_ID", "PERIOD_STATUS", "START_DATE", "END_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTING_PERIODS_FISCAL_YEAR_ID_PERIOD_STATUS",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "FISCAL_YEAR_ID", "PERIOD_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_PERIODS_CALENDAR_ID_PERIOD_CODE",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "CALENDAR_ID", "PERIOD_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNTING_PERIODS_FISCAL_YEAR_ID_PERIOD_NO",
                table: "GL_ACCOUNTING_PERIODS",
                columns: new[] { "FISCAL_YEAR_ID", "PERIOD_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTS_ACCOUNT_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNTS",
                column: "ACCOUNT_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTS_COA_ID_ACCOUNT_TYPE_IS_ACTIVE",
                table: "GL_ACCOUNTS",
                columns: new[] { "COA_ID", "ACCOUNT_TYPE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNTS_FINANCIAL_STATEMENT_CLASS_CASH_FLOW_CLASS",
                table: "GL_ACCOUNTS",
                columns: new[] { "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS" });

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
                name: "IX_GL_AUTO_ACCOUNTING_CONDITIONS_COMPARE_SOURCE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_CONDITIONS",
                column: "COMPARE_SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_CONDITIONS_LEFT_SOURCE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_CONDITIONS",
                column: "LEFT_SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_CONDITIONS_OWNER_TYPE_OWNER_ID_IS_ACTIVE",
                table: "GL_AUTO_ACCOUNTING_CONDITIONS",
                columns: new[] { "OWNER_TYPE", "OWNER_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_CONDITIONS_OWNER_TYPE_OWNER_ID",
                table: "GL_AUTO_ACCOUNTING_CONDITIONS",
                columns: new[] { "OWNER_TYPE", "OWNER_ID", "CONDITION_GROUP_NO", "SEQUENCE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "ACCOUNT_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_ACCOUNT_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_ACCOUNTED_CURRENCY_CODE",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "ACCOUNTED_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_ENTERED_CURRENCY_CODE",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "ENTERED_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_GENERATION_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                columns: new[] { "GENERATION_ID", "VALIDATION_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_INTERFACE_LINE_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "INTERFACE_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_JOURNAL_LINE_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_RATE_TYPE_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATED_LINES_RULE_LINE_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                column: "RULE_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_GENERATED_LINES_GENERATION_ID_LINE_NO",
                table: "GL_AUTO_ACCOUNTING_GENERATED_LINES",
                columns: new[] { "GENERATION_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATIONS_INTERFACE_BATCH_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                column: "INTERFACE_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATIONS_JOURNAL_HEADER_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATIONS_LEDGER_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_GENERATIONS_RULE_SET_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                column: "RULE_SET_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_GENERATIONS_ACCOUNTING_EVENT_ID",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                columns: new[] { "ACCOUNTING_EVENT_ID", "RULE_SET_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_GENERATIONS_GENERATION_NO",
                table: "GL_AUTO_ACCOUNTING_GENERATIONS",
                column: "GENERATION_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_ACCOUNT_LOOKUP_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "ACCOUNT_LOOKUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_ACCOUNT_SOURCE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "ACCOUNT_SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_AMOUNT_SOURCE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "AMOUNT_SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_CURRENCY_SOURCE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "CURRENCY_SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_FIXED_ACCOUNT_COMBINATION_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                columns: new[] { "FIXED_ACCOUNT_COMBINATION_ID", "FIXED_ACCOUNT_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_FIXED_ACCOUNT_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "FIXED_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_FIXED_AMOUNT_CURRENCY",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "FIXED_AMOUNT_CURRENCY");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_FIXED_CURRENCY_CODE",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "FIXED_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_PERCENTAGE_BASE_VALUE_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                column: "PERCENTAGE_BASE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_LINES_RULE_SET_ID_IS_ACTIVE",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                columns: new[] { "RULE_SET_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_RULE_LINES_RULE_SET_ID_LINE_NO",
                table: "GL_AUTO_ACCOUNTING_RULE_LINES",
                columns: new[] { "RULE_SET_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_BUSINESS_LINE_CODE_PRODUCT_CODE",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                columns: new[] { "BUSINESS_LINE_CODE", "PRODUCT_CODE", "TRANSACTION_TYPE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_CURRENCY_CODE",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_DEFAULT_CATEGORY_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                column: "DEFAULT_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_EVENT_TYPE_ID_RULE_STATUS",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                columns: new[] { "EVENT_TYPE_ID", "RULE_STATUS", "APPROVAL_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_LEDGER_ID_EFFECTIVE_FROM_DATE",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                columns: new[] { "LEDGER_ID", "EFFECTIVE_FROM_DATE", "EFFECTIVE_TO_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_ACCOUNTING_RULE_SETS_LEGAL_ENTITY_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_ACCOUNTING_RULE_SETS_SOURCE_ID_EVENT_TYPE_ID",
                table: "GL_AUTO_ACCOUNTING_RULE_SETS",
                columns: new[] { "SOURCE_ID", "EVENT_TYPE_ID", "LEDGER_ID", "RULE_SET_CODE", "VERSION_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_APPROVAL_HIST_RULE_SET_ID",
                table: "GL_AUTO_RULE_APPROVAL_HIST",
                column: "RULE_SET_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_AUDITS_OBJECT_TYPE_OBJECT_ID_CHANGED_AT",
                table: "GL_AUTO_RULE_AUDITS",
                columns: new[] { "OBJECT_TYPE", "OBJECT_ID", "CHANGED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_LINE_SEGMENTS_COA_SEGMENT_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                column: "COA_SEGMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_LINE_SEGMENTS_DEFAULT_SEGMENT_VALUE_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                column: "DEFAULT_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_LINE_SEGMENTS_FIXED_SEGMENT_VALUE_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                column: "FIXED_SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_LINE_SEGMENTS_LOOKUP_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                column: "LOOKUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_AUTO_RULE_LINE_SEGMENTS_SOURCE_VALUE_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                column: "SOURCE_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_RULE_LINE_SEGMENTS_RULE_LINE_ID_COA_SEGMENT_ID",
                table: "GL_AUTO_RULE_LINE_SEGMENTS",
                columns: new[] { "RULE_LINE_ID", "COA_SEGMENT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_RULE_TEST_CASES_RULE_SET_ID_TEST_CASE_CODE",
                table: "GL_AUTO_RULE_TEST_CASES",
                columns: new[] { "RULE_SET_ID", "TEST_CASE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_AUTO_RULE_TEST_RESULTS_TEST_CASE_ID_RUN_NO",
                table: "GL_AUTO_RULE_TEST_RESULTS",
                columns: new[] { "TEST_CASE_ID", "RUN_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_BALANCE_REBUILD_LINES",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "LEDGER_PERIOD_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_LINES_BALANCE_REBUILD_RUN_ID",
                table: "GL_BALANCE_REBUILD_LINES",
                columns: new[] { "BALANCE_REBUILD_RUN_ID", "REBUILD_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_LINES_LEDGER_PERIOD_ID",
                table: "GL_BALANCE_REBUILD_LINES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_RUNS_ACCOUNT_COMBINATION_ID_CURRENCY_CODE",
                table: "GL_BALANCE_REBUILD_RUNS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_RUNS_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_BALANCE_REBUILD_RUNS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "REBUILD_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_BALANCE_REBUILD_RUNS_LEDGER_PERIOD_ID",
                table: "GL_BALANCE_REBUILD_RUNS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_BALANCE_REBUILD_RUNS_REBUILD_RUN_NO",
                table: "GL_BALANCE_REBUILD_RUNS",
                column: "REBUILD_RUN_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CALENDARS_CALENDAR_CODE",
                table: "GL_CALENDARS",
                column: "CALENDAR_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCOUNT_GROUP_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_CLOSE_ACCOUNT_GROUP_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCOUNT_GROUP_LINES_ACCOUNT_GROUP_ID",
                table: "GL_CLOSE_ACCOUNT_GROUP_LINES",
                column: "ACCOUNT_GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCOUNT_GROUP_LINES_ACCOUNT_ID",
                table: "GL_CLOSE_ACCOUNT_GROUP_LINES",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_ACCOUNT_GROUPS_ACCOUNT_GROUP_CODE",
                table: "GL_CLOSE_ACCOUNT_GROUPS",
                column: "ACCOUNT_GROUP_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_CLOSE_ACCRUAL_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_LINES_ACCRUAL_RULE_LINE_ID",
                table: "GL_CLOSE_ACCRUAL_LINES",
                column: "ACCRUAL_RULE_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_LINES_ACCRUAL_RUN_ID_LINE_STATUS",
                table: "GL_CLOSE_ACCRUAL_LINES",
                columns: new[] { "ACCRUAL_RUN_ID", "LINE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_LINES_JOURNAL_LINE_ID",
                table: "GL_CLOSE_ACCRUAL_LINES",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RULE_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_CLOSE_ACCRUAL_RULE_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_ACCRUAL_RULE_LINES_ACCRUAL_RULE_ID_LINE_NO",
                table: "GL_CLOSE_ACCRUAL_RULE_LINES",
                columns: new[] { "ACCRUAL_RULE_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RULES_CLOSE_PROFILE_ID",
                table: "GL_CLOSE_ACCRUAL_RULES",
                column: "CLOSE_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RULES_LEDGER_ID_SOURCE_MODULE_IS_ACTIVE",
                table: "GL_CLOSE_ACCRUAL_RULES",
                columns: new[] { "LEDGER_ID", "SOURCE_MODULE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_ACCRUAL_RULES_ACCRUAL_RULE_CODE",
                table: "GL_CLOSE_ACCRUAL_RULES",
                column: "ACCRUAL_RULE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RUNS_ACCRUAL_RULE_ID",
                table: "GL_CLOSE_ACCRUAL_RUNS",
                column: "ACCRUAL_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RUNS_CLOSE_RUN_ID_RUN_STATUS",
                table: "GL_CLOSE_ACCRUAL_RUNS",
                columns: new[] { "CLOSE_RUN_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RUNS_JOURNAL_BATCH_ID",
                table: "GL_CLOSE_ACCRUAL_RUNS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RUNS_JOURNAL_HEADER_ID",
                table: "GL_CLOSE_ACCRUAL_RUNS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACCRUAL_RUNS_REVERSAL_JOURNAL_ID",
                table: "GL_CLOSE_ACCRUAL_RUNS",
                column: "REVERSAL_JOURNAL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACTION_HIST_CLOSE_RUN_ID_ACTION_AT",
                table: "GL_CLOSE_ACTION_HIST",
                columns: new[] { "CLOSE_RUN_ID", "ACTION_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ACTION_HIST_CLOSE_RUN_TASK_ID",
                table: "GL_CLOSE_ACTION_HIST",
                column: "CLOSE_RUN_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ATTACHMENTS_CLOSE_REPORT_ID",
                table: "GL_CLOSE_ATTACHMENTS",
                column: "CLOSE_REPORT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ATTACHMENTS_CLOSE_RUN_ID",
                table: "GL_CLOSE_ATTACHMENTS",
                column: "CLOSE_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_ATTACHMENTS_CLOSE_RUN_TASK_ID",
                table: "GL_CLOSE_ATTACHMENTS",
                column: "CLOSE_RUN_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_COMMENTS_CLOSE_EXCEPTION_ID",
                table: "GL_CLOSE_COMMENTS",
                column: "CLOSE_EXCEPTION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_COMMENTS_CLOSE_RUN_ID",
                table: "GL_CLOSE_COMMENTS",
                column: "CLOSE_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_COMMENTS_CLOSE_RUN_TASK_ID",
                table: "GL_CLOSE_COMMENTS",
                column: "CLOSE_RUN_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_EXCEPTIONS_CLOSE_RUN_ID_EXCEPTION_STATUS_SEVERITY",
                table: "GL_CLOSE_EXCEPTIONS",
                columns: new[] { "CLOSE_RUN_ID", "EXCEPTION_STATUS", "SEVERITY" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_EXCEPTIONS_CLOSE_RUN_TASK_ID",
                table: "GL_CLOSE_EXCEPTIONS",
                column: "CLOSE_RUN_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_EXCEPTIONS_VALIDATION_RESULT_ID",
                table: "GL_CLOSE_EXCEPTIONS",
                column: "VALIDATION_RESULT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PERIOD_LOCKS_CLOSE_RUN_ID",
                table: "GL_CLOSE_PERIOD_LOCKS",
                column: "CLOSE_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PERIOD_LOCKS_LEDGER_PERIOD_ID_LOCK_STATUS",
                table: "GL_CLOSE_PERIOD_LOCKS",
                columns: new[] { "LEDGER_PERIOD_ID", "LOCK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_PERIOD_LOCKS_LEDGER_ID_LEDGER_PERIOD_ID_LOCK_LEVEL",
                table: "GL_CLOSE_PERIOD_LOCKS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "LOCK_LEVEL", "LOCK_STATUS" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PROFILE_MODULES_SOURCE_MODULE_IS_ACTIVE",
                table: "GL_CLOSE_PROFILE_MODULES",
                columns: new[] { "SOURCE_MODULE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_PROFILE_MODULES_CLOSE_PROFILE_ID_SOURCE_MODULE",
                table: "GL_CLOSE_PROFILE_MODULES",
                columns: new[] { "CLOSE_PROFILE_ID", "SOURCE_MODULE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PROFILE_STEPS_CLOSE_PROFILE_ID_STEP_GROUP_CODE",
                table: "GL_CLOSE_PROFILE_STEPS",
                columns: new[] { "CLOSE_PROFILE_ID", "STEP_GROUP_CODE", "DISPLAY_SEQ" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_PROFILE_STEPS_CLOSE_PROFILE_ID_STEP_CODE",
                table: "GL_CLOSE_PROFILE_STEPS",
                columns: new[] { "CLOSE_PROFILE_ID", "STEP_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PROFILES_LEDGER_ID_CLOSE_TYPE_IS_ACTIVE",
                table: "GL_CLOSE_PROFILES",
                columns: new[] { "LEDGER_ID", "CLOSE_TYPE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_PROFILES_LEGAL_ENTITY_ID",
                table: "GL_CLOSE_PROFILES",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_PROFILES_CLOSE_PROFILE_CODE",
                table: "GL_CLOSE_PROFILES",
                column: "CLOSE_PROFILE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RECURRING_RUNS_CLOSE_RUN_ID",
                table: "GL_CLOSE_RECURRING_RUNS",
                column: "CLOSE_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RECURRING_RUNS_JOURNAL_BATCH_ID",
                table: "GL_CLOSE_RECURRING_RUNS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RECURRING_RUNS_RECURRING_TEMPLATE_ID",
                table: "GL_CLOSE_RECURRING_RUNS",
                column: "RECURRING_TEMPLATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_REPORTS_CLOSE_RUN_ID_REPORT_TYPE_REPORT_STATUS",
                table: "GL_CLOSE_REPORTS",
                columns: new[] { "CLOSE_RUN_ID", "REPORT_TYPE", "REPORT_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_REPORTS_CLOSE_RUN_ID_REPORT_CODE",
                table: "GL_CLOSE_REPORTS",
                columns: new[] { "CLOSE_RUN_ID", "REPORT_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUN_MODULE_STATUSES_CLOSE_RUN_ID_MODULE_STATUS",
                table: "GL_CLOSE_RUN_MODULE_STATUSES",
                columns: new[] { "CLOSE_RUN_ID", "MODULE_STATUS", "POSTING_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_RUN_MODULE_STATUSES_CLOSE_RUN_ID_SOURCE_MODULE",
                table: "GL_CLOSE_RUN_MODULE_STATUSES",
                columns: new[] { "CLOSE_RUN_ID", "SOURCE_MODULE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUN_TASKS_CLOSE_RUN_ID_TASK_STATUS_DISPLAY_SEQ",
                table: "GL_CLOSE_RUN_TASKS",
                columns: new[] { "CLOSE_RUN_ID", "TASK_STATUS", "DISPLAY_SEQ" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUN_TASKS_CLOSE_STEP_ID",
                table: "GL_CLOSE_RUN_TASKS",
                column: "CLOSE_STEP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUN_TASKS_DEPENDENCY_TASK_ID",
                table: "GL_CLOSE_RUN_TASKS",
                column: "DEPENDENCY_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUNS_CLOSE_PROFILE_ID",
                table: "GL_CLOSE_RUNS",
                column: "CLOSE_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUNS_FISCAL_YEAR_ID",
                table: "GL_CLOSE_RUNS",
                column: "FISCAL_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUNS_LEDGER_ID_CLOSE_TYPE_RUN_STATUS",
                table: "GL_CLOSE_RUNS",
                columns: new[] { "LEDGER_ID", "CLOSE_TYPE", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_RUNS_LEDGER_PERIOD_ID_RUN_STATUS",
                table: "GL_CLOSE_RUNS",
                columns: new[] { "LEDGER_PERIOD_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_RUNS_CLOSE_RUN_NO",
                table: "GL_CLOSE_RUNS",
                column: "CLOSE_RUN_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_RUNS_LEDGER_ID_CLOSE_TYPE_LEDGER_PERIOD_ID",
                table: "GL_CLOSE_RUNS",
                columns: new[] { "LEDGER_ID", "CLOSE_TYPE", "LEDGER_PERIOD_ID", "FISCAL_YEAR_ID", "RUN_MODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_VALIDATION_RESULTS_CLOSE_RUN_ID_RESULT_STATUS",
                table: "GL_CLOSE_VALIDATION_RESULTS",
                columns: new[] { "CLOSE_RUN_ID", "RESULT_STATUS", "SEVERITY" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_VALIDATION_RESULTS_CLOSE_RUN_TASK_ID",
                table: "GL_CLOSE_VALIDATION_RESULTS",
                column: "CLOSE_RUN_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_VALIDATION_RESULTS_VALIDATION_RULE_ID",
                table: "GL_CLOSE_VALIDATION_RESULTS",
                column: "VALIDATION_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CLOSE_VALIDATION_RULES_RULE_CATEGORY_SOURCE_MODULE",
                table: "GL_CLOSE_VALIDATION_RULES",
                columns: new[] { "RULE_CATEGORY", "SOURCE_MODULE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_VALIDATION_RULES_VALIDATION_SET_ID_RULE_CODE",
                table: "GL_CLOSE_VALIDATION_RULES",
                columns: new[] { "VALIDATION_SET_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CLOSE_VALIDATION_SETS_VALIDATION_SET_CODE",
                table: "GL_CLOSE_VALIDATION_SETS",
                column: "VALIDATION_SET_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_COA_SEGMENT_ID_IS_ENABLED_VALUE_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                columns: new[] { "COA_SEGMENT_ID", "IS_ENABLED", "VALUE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_PARENT_VALUE_ID",
                table: "GL_COA_SEGMENT_VALUES",
                column: "PARENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_BRANCH_ID",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_BUSINESS_LINE_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_BUSINESS_LINE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_COST_CENTER_ID",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_COST_CENTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENT_VALUES_REFERENCE_LEGAL_ENTITY_ID",
                table: "GL_COA_SEGMENT_VALUES",
                column: "REFERENCE_LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_SEGMENT_VALUES_COA_SEGMENT_ID_VALUE_CODE",
                table: "GL_COA_SEGMENT_VALUES",
                columns: new[] { "COA_SEGMENT_ID", "VALUE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENTS_COA_ID_SEGMENT_TYPE",
                table: "GL_COA_SEGMENTS",
                columns: new[] { "COA_ID", "SEGMENT_TYPE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_COA_SEGMENTS_IS_ENABLED_DISPLAY_SEQ",
                table: "GL_COA_SEGMENTS",
                columns: new[] { "IS_ENABLED", "DISPLAY_SEQ" });

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
                name: "IX_GL_COA_STRUCTURES_IS_ACTIVE_COA_CODE",
                table: "GL_COA_STRUCTURES",
                columns: new[] { "IS_ACTIVE", "COA_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_COA_STRUCTURES_COA_CODE",
                table: "GL_COA_STRUCTURES",
                column: "COA_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_LINES_BUSINESS_LINE_CODE_CURRENCY_CODE",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                columns: new[] { "BUSINESS_LINE_CODE", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_LINES_CURRENCY_CODE",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_LINES_EXPOSURE_SNAPSHOT_ID",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                columns: new[] { "EXPOSURE_SNAPSHOT_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_LINES_FUNCTIONAL_CURRENCY_CODE",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                column: "FUNCTIONAL_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CURRENCY_EXPOSURE_LINES_EXPOSURE_SNAPSHOT_ID_LINE_NO",
                table: "GL_CURRENCY_EXPOSURE_LINES",
                columns: new[] { "EXPOSURE_SNAPSHOT_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_SNAPSHOTS_LEDGER_ID_SNAPSHOT_DATE",
                table: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                columns: new[] { "LEDGER_ID", "SNAPSHOT_DATE", "SNAPSHOT_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_SNAPSHOTS_LEDGER_PERIOD_ID",
                table: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_CURRENCY_EXPOSURE_SNAPSHOTS_RATE_TYPE_ID",
                table: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_CURRENCY_EXPOSURE_SNAPSHOTS_SNAPSHOT_CODE",
                table: "GL_CURRENCY_EXPOSURE_SNAPSHOTS",
                column: "SNAPSHOT_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_BALANCES_ACCOUNT_COMBINATION_ID",
                table: "GL_DAILY_BALANCES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_BALANCES_LEDGER_ID_BALANCE_DATE_CURRENCY_CODE",
                table: "GL_DAILY_BALANCES",
                columns: new[] { "LEDGER_ID", "BALANCE_DATE", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_BALANCES_LEDGER_PERIOD_ID",
                table: "GL_DAILY_BALANCES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_DAILY_BALANCES_LEDGER_ID_BALANCE_DATE",
                table: "GL_DAILY_BALANCES",
                columns: new[] { "LEDGER_ID", "BALANCE_DATE", "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_RATES_CONVERSION_DATE_RATE_TYPE_ID",
                table: "GL_DAILY_RATES",
                columns: new[] { "CONVERSION_DATE", "RATE_TYPE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_RATES_FROM_CURRENCY_CODE_TO_CURRENCY_CODE",
                table: "GL_DAILY_RATES",
                columns: new[] { "FROM_CURRENCY_CODE", "TO_CURRENCY_CODE", "CONVERSION_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_RATES_RATE_STATUS_CONVERSION_DATE",
                table: "GL_DAILY_RATES",
                columns: new[] { "RATE_STATUS", "CONVERSION_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_RATES_TO_CURRENCY_CODE",
                table: "GL_DAILY_RATES",
                column: "TO_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_DAILY_RATES_TRIANGULATION_CURRENCY",
                table: "GL_DAILY_RATES",
                column: "TRIANGULATION_CURRENCY");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_DAILY_RATES_RATE_TYPE_ID_FROM_CURRENCY_CODE",
                table: "GL_DAILY_RATES",
                columns: new[] { "RATE_TYPE_ID", "FROM_CURRENCY_CODE", "TO_CURRENCY_CODE", "CONVERSION_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_EXTERNAL_GL_SYSTEMS_SYSTEM_CODE",
                table: "GL_EXTERNAL_GL_SYSTEMS",
                column: "SYSTEM_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_FISCAL_YEARS_CALENDAR_ID_YEAR_STATUS",
                table: "GL_FISCAL_YEARS",
                columns: new[] { "CALENDAR_ID", "YEAR_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_FISCAL_YEARS_CALENDAR_ID_FISCAL_YEAR",
                table: "GL_FISCAL_YEARS",
                columns: new[] { "CALENDAR_ID", "FISCAL_YEAR" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_RATE_USAGE_LOGS_ACCOUNTED_CURRENCY_CODE",
                table: "GL_FX_RATE_USAGE_LOGS",
                column: "ACCOUNTED_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_RATE_USAGE_LOGS_DAILY_RATE_ID",
                table: "GL_FX_RATE_USAGE_LOGS",
                column: "DAILY_RATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_RATE_USAGE_LOGS_ENTERED_CURRENCY_CODE",
                table: "GL_FX_RATE_USAGE_LOGS",
                column: "ENTERED_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_RATE_USAGE_LOGS_LEDGER_ID",
                table: "GL_FX_RATE_USAGE_LOGS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_RATE_USAGE_LOGS_SOURCE_MODULE_SOURCE_TRANSACTION_TYPE",
                table: "GL_FX_RATE_USAGE_LOGS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_TRANSACTION_TYPE", "SOURCE_TRANSACTION_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_FX_REVALUATION_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_LINES_CURRENCY_CODE_ACCOUNT_COMBINATION_ID",
                table: "GL_FX_REVALUATION_LINES",
                columns: new[] { "CURRENCY_CODE", "ACCOUNT_COMBINATION_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_LINES_GAIN_LOSS_ACCOUNT_COMB_ID",
                table: "GL_FX_REVALUATION_LINES",
                column: "GAIN_LOSS_ACCOUNT_COMB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_LINES_REVALUATION_RUN_ID_LINE_STATUS",
                table: "GL_FX_REVALUATION_LINES",
                columns: new[] { "REVALUATION_RUN_ID", "LINE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_FX_REVALUATION_LINES_REVALUATION_RUN_ID_LINE_NO",
                table: "GL_FX_REVALUATION_LINES",
                columns: new[] { "REVALUATION_RUN_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RULES_GAIN_ACCOUNT_COMBINATION_ID",
                table: "GL_FX_REVALUATION_RULES",
                column: "GAIN_ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RULES_LEDGER_ID_IS_ACTIVE",
                table: "GL_FX_REVALUATION_RULES",
                columns: new[] { "LEDGER_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RULES_LOSS_ACCOUNT_COMBINATION_ID",
                table: "GL_FX_REVALUATION_RULES",
                column: "LOSS_ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RULES_RATE_TYPE_ID",
                table: "GL_FX_REVALUATION_RULES",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_FX_REVALUATION_RULES_LEDGER_ID_RULE_CODE",
                table: "GL_FX_REVALUATION_RULES",
                columns: new[] { "LEDGER_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RUNS_LEDGER_ID_LEDGER_PERIOD_ID_RUN_STATUS",
                table: "GL_FX_REVALUATION_RUNS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RUNS_LEDGER_PERIOD_ID",
                table: "GL_FX_REVALUATION_RUNS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RUNS_RATE_TYPE_ID",
                table: "GL_FX_REVALUATION_RUNS",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_FX_REVALUATION_RUNS_REVALUATION_RULE_ID",
                table: "GL_FX_REVALUATION_RUNS",
                column: "REVALUATION_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_FX_REVALUATION_RUNS_RUN_CODE",
                table: "GL_FX_REVALUATION_RUNS",
                column: "RUN_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ACTION_HIST_BATCH_ID",
                table: "GL_JOURNAL_ACTION_HIST",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ACTION_HIST_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_ACTION_HIST",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ACTION_HIST_JOURNAL_LINE_ID",
                table: "GL_JOURNAL_ACTION_HIST",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_APPROVAL_HIST_BATCH_ID",
                table: "GL_JOURNAL_APPROVAL_HIST",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_APPROVAL_HIST_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_APPROVAL_HIST",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ATTACHMENTS_BATCH_ID",
                table: "GL_JOURNAL_ATTACHMENTS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ATTACHMENTS_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_ATTACHMENTS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_ATTACHMENTS_JOURNAL_LINE_ID",
                table: "GL_JOURNAL_ATTACHMENTS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_CATEGORY_ID",
                table: "GL_JOURNAL_BATCHES",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_LEDGER_ID_LEDGER_PERIOD_ID_BATCH_STATUS",
                table: "GL_JOURNAL_BATCHES",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "BATCH_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_LEDGER_PERIOD_ID",
                table: "GL_JOURNAL_BATCHES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_SOURCE_ID_SOURCE_BATCH_REF",
                table: "GL_JOURNAL_BATCHES",
                columns: new[] { "SOURCE_ID", "SOURCE_BATCH_REF" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_BATCHES_LEDGER_ID_BATCH_NO",
                table: "GL_JOURNAL_BATCHES",
                columns: new[] { "LEDGER_ID", "BATCH_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_CATEGORIES_DEFAULT_SOURCE_ID",
                table: "GL_JOURNAL_CATEGORIES",
                column: "DEFAULT_SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_CATEGORIES_CATEGORY_CODE",
                table: "GL_JOURNAL_CATEGORIES",
                column: "CATEGORY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_COMMENTS_BATCH_ID",
                table: "GL_JOURNAL_COMMENTS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_COMMENTS_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_COMMENTS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_COMMENTS_JOURNAL_LINE_ID",
                table: "GL_JOURNAL_COMMENTS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_ACCOUNTING_DATE",
                table: "GL_JOURNAL_HEADERS",
                column: "ACCOUNTING_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_BATCH_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_CATEGORY_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_LEDGER_ID_LEDGER_PERIOD_ID_JOURNAL_STATUS",
                table: "GL_JOURNAL_HEADERS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "JOURNAL_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_LEDGER_PERIOD_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_ORIGINAL_HEADER_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "ORIGINAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_POSTING_STATUS_APPROVAL_STATUS",
                table: "GL_JOURNAL_HEADERS",
                columns: new[] { "POSTING_STATUS", "APPROVAL_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_RATE_TYPE_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_REVERSAL_PERIOD_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "REVERSAL_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_REVERSED_HEADER_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "REVERSED_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_SOURCE_ID_SOURCE_DOCUMENT_TYPE",
                table: "GL_JOURNAL_HEADERS",
                columns: new[] { "SOURCE_ID", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_HEADERS_LEDGER_ID_JOURNAL_NO",
                table: "GL_JOURNAL_HEADERS",
                columns: new[] { "LEDGER_ID", "JOURNAL_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_BATCHES_GENERATED_BATCH_ID",
                table: "GL_JOURNAL_INTERFACE_BATCHES",
                column: "GENERATED_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_BATCHES_SOURCE_ID",
                table: "GL_JOURNAL_INTERFACE_BATCHES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_INTERFACE_BATCHES_LEDGER_ID_INTERFACE_BATCH_NO",
                table: "GL_JOURNAL_INTERFACE_BATCHES",
                columns: new[] { "LEDGER_ID", "INTERFACE_BATCH_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_JOURNAL_INTERFACE_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_ACCOUNTING_EVENT_ID",
                table: "GL_JOURNAL_INTERFACE_LINES",
                column: "ACCOUNTING_EVENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_GENERATED_HEADER_ID",
                table: "GL_JOURNAL_INTERFACE_LINES",
                column: "GENERATED_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_GENERATED_LINE_ID",
                table: "GL_JOURNAL_INTERFACE_LINES",
                column: "GENERATED_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_INTERFACE_BATCH_ID",
                table: "GL_JOURNAL_INTERFACE_LINES",
                columns: new[] { "INTERFACE_BATCH_ID", "VALIDATION_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_INTERFACE_LINES_SOURCE_MODULE",
                table: "GL_JOURNAL_INTERFACE_LINES",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_INTERFACE_LINES_INTERFACE_BATCH_ID_LINE_NO",
                table: "GL_JOURNAL_INTERFACE_LINES",
                columns: new[] { "INTERFACE_BATCH_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_LINES_ACCOUNT_COMBINATION_ID_CURRENCY_CODE",
                table: "GL_JOURNAL_LINES",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_LINES_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_JOURNAL_LINES",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "ACCOUNT_COMBINATION_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_LINES_LEDGER_PERIOD_ID",
                table: "GL_JOURNAL_LINES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_LINES_JOURNAL_HEADER_ID_LINE_NO",
                table: "GL_JOURNAL_LINES",
                columns: new[] { "JOURNAL_HEADER_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_NUMBERING_RULES_CATEGORY_ID",
                table: "GL_JOURNAL_NUMBERING_RULES",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_NUMBERING_RULES_SOURCE_ID",
                table: "GL_JOURNAL_NUMBERING_RULES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_NUMBERING_RULES_LEDGER_ID_SOURCE_ID_CATEGORY_ID",
                table: "GL_JOURNAL_NUMBERING_RULES",
                columns: new[] { "LEDGER_ID", "SOURCE_ID", "CATEGORY_ID", "JOURNAL_TYPE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_SOURCES_SOURCE_CODE",
                table: "GL_JOURNAL_SOURCES",
                column: "SOURCE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RESULTS_BATCH_ID",
                table: "GL_JOURNAL_VALIDATION_RESULTS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RESULTS_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_VALIDATION_RESULTS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RESULTS_JOURNAL_LINE_ID",
                table: "GL_JOURNAL_VALIDATION_RESULTS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RESULTS_VALIDATION_RULE_ID",
                table: "GL_JOURNAL_VALIDATION_RESULTS",
                column: "VALIDATION_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RULES_CATEGORY_ID",
                table: "GL_JOURNAL_VALIDATION_RULES",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_VALIDATION_RULES_SOURCE_ID",
                table: "GL_JOURNAL_VALIDATION_RULES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_VALIDATION_RULES_RULE_CODE",
                table: "GL_JOURNAL_VALIDATION_RULES",
                column: "RULE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_BRANCHES_BRANCH_ID_IS_ACTIVE",
                table: "GL_LEDGER_BRANCHES",
                columns: new[] { "BRANCH_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGER_BRANCHES_LEDGER_ID_BRANCH_ID",
                table: "GL_LEDGER_BRANCHES",
                columns: new[] { "LEDGER_ID", "BRANCH_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGER_OPTIONS_LEDGER_ID",
                table: "GL_LEDGER_OPTIONS",
                column: "LEDGER_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_PERIODS_LEDGER_ID_OPENED_AT_CLOSED_AT",
                table: "GL_LEDGER_PERIODS",
                columns: new[] { "LEDGER_ID", "OPENED_AT", "CLOSED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_PERIODS_LEDGER_ID_PERIOD_STATUS",
                table: "GL_LEDGER_PERIODS",
                columns: new[] { "LEDGER_ID", "PERIOD_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_LEDGER_PERIODS_PERIOD_ID_PERIOD_STATUS",
                table: "GL_LEDGER_PERIODS",
                columns: new[] { "PERIOD_ID", "PERIOD_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGER_PERIODS_LEDGER_ID_PERIOD_ID",
                table: "GL_LEDGER_PERIODS",
                columns: new[] { "LEDGER_ID", "PERIOD_ID" },
                unique: true);

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
                name: "IX_GL_LEDGERS_LEGAL_ENTITY_ID_LEDGER_STATUS",
                table: "GL_LEDGERS",
                columns: new[] { "LEGAL_ENTITY_ID", "LEDGER_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_LEDGERS_LEDGER_CODE",
                table: "GL_LEDGERS",
                column: "LEDGER_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_OPENING_BALANCE_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_LINES_JOURNAL_LINE_ID",
                table: "GL_OPENING_BALANCE_LINES",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_LINES_OPENING_BALANCE_RUN_ID_LINE_STATUS",
                table: "GL_OPENING_BALANCE_LINES",
                columns: new[] { "OPENING_BALANCE_RUN_ID", "LINE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_OPENING_BALANCE_LINES_OPENING_BALANCE_RUN_ID",
                table: "GL_OPENING_BALANCE_LINES",
                columns: new[] { "OPENING_BALANCE_RUN_ID", "ACCOUNT_COMBINATION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_RUNS_JOURNAL_BATCH_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_RUNS_LEDGER_ID_TARGET_FISCAL_YEAR_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                columns: new[] { "LEDGER_ID", "TARGET_FISCAL_YEAR_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_RUNS_TARGET_FISCAL_YEAR_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                column: "TARGET_FISCAL_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_RUNS_TARGET_LEDGER_PERIOD_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                column: "TARGET_LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_OPENING_BALANCE_RUNS_YEAR_END_RUN_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                column: "YEAR_END_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_OPENING_BALANCE_RUNS_LEDGER_ID_TARGET_FISCAL_YEAR_ID",
                table: "GL_OPENING_BALANCE_RUNS",
                columns: new[] { "LEDGER_ID", "TARGET_FISCAL_YEAR_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_ACCOUNT_MAPS_ACCOUNT_COMBINATION_ID",
                table: "GL_ORACLE_GL_ACCOUNT_MAPS",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_ACCOUNT_MAPS_EXTERNAL_SYSTEM_ID",
                table: "GL_ORACLE_GL_ACCOUNT_MAPS",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "ACCOUNT_COMBINATION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_CATEGORY_MAPS_EXTERNAL_SYSTEM_ID",
                table: "GL_ORACLE_GL_CATEGORY_MAPS",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "FINOVA_CATEGORY_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_BATCHES_EXPORT_PROFILE_ID",
                table: "GL_ORACLE_GL_EXPORT_BATCHES",
                column: "EXPORT_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_BATCHES_EXTERNAL_SYSTEM_ID_EXPORT_STATUS",
                table: "GL_ORACLE_GL_EXPORT_BATCHES",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "EXPORT_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_BATCHES_GROUP_ID",
                table: "GL_ORACLE_GL_EXPORT_BATCHES",
                column: "GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_EXPORT_BATCHES_EXTERNAL_SYSTEM_ID",
                table: "GL_ORACLE_GL_EXPORT_BATCHES",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "EXPORT_BATCH_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_HEADERS_DUPLICATE_KEY",
                table: "GL_ORACLE_GL_EXPORT_HEADERS",
                column: "DUPLICATE_KEY");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_HEADERS_JOURNAL_BATCH_ID",
                table: "GL_ORACLE_GL_EXPORT_HEADERS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_HEADERS_JOURNAL_HEADER_ID",
                table: "GL_ORACLE_GL_EXPORT_HEADERS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_EXPORT_HEADERS_EXPORT_BATCH_ID",
                table: "GL_ORACLE_GL_EXPORT_HEADERS",
                columns: new[] { "EXPORT_BATCH_ID", "JOURNAL_HEADER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_ORACLE_GL_EXPORT_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_LINES_JOURNAL_LINE_ID",
                table: "GL_ORACLE_GL_EXPORT_LINES",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_EXPORT_LINES_EXPORT_HEADER_ID_JOURNAL_LINE_ID",
                table: "GL_ORACLE_GL_EXPORT_LINES",
                columns: new[] { "EXPORT_HEADER_ID", "JOURNAL_LINE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_EXPORT_PROFILES_LEDGER_MAP_ID",
                table: "GL_ORACLE_GL_EXPORT_PROFILES",
                column: "LEDGER_MAP_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_EXPORT_PROFILES_EXTERNAL_SYSTEM_ID_PROFILE_CODE",
                table: "GL_ORACLE_GL_EXPORT_PROFILES",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "PROFILE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_REQUESTS_EXPORT_BATCH_ID",
                table: "GL_ORACLE_GL_IMPORT_REQUESTS",
                column: "EXPORT_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_RESULTS_EXPORT_BATCH_ID_RESULT_STATUS",
                table: "GL_ORACLE_GL_IMPORT_RESULTS",
                columns: new[] { "EXPORT_BATCH_ID", "RESULT_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_RESULTS_EXPORT_HEADER_ID",
                table: "GL_ORACLE_GL_IMPORT_RESULTS",
                column: "EXPORT_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_RESULTS_EXPORT_LINE_ID",
                table: "GL_ORACLE_GL_IMPORT_RESULTS",
                column: "EXPORT_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_RESULTS_IMPORT_REQUEST_ID",
                table: "GL_ORACLE_GL_IMPORT_RESULTS",
                column: "IMPORT_REQUEST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_IMPORT_RESULTS_INTERFACE_ROW_ID",
                table: "GL_ORACLE_GL_IMPORT_RESULTS",
                column: "INTERFACE_ROW_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_INTERFACE_ROWS_EXPORT_BATCH_ID_INTERFACE_STATUS",
                table: "GL_ORACLE_GL_INTERFACE_ROWS",
                columns: new[] { "EXPORT_BATCH_ID", "INTERFACE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_INTERFACE_ROWS_GROUP_ID_ORACLE_STATUS",
                table: "GL_ORACLE_GL_INTERFACE_ROWS",
                columns: new[] { "GROUP_ID", "ORACLE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_INTERFACE_ROWS_EXPORT_LINE_ID",
                table: "GL_ORACLE_GL_INTERFACE_ROWS",
                column: "EXPORT_LINE_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_LEDGER_MAPS_LEDGER_ID",
                table: "GL_ORACLE_GL_LEDGER_MAPS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_LEDGER_MAPS_EXTERNAL_SYSTEM_ID_LEDGER_ID",
                table: "GL_ORACLE_GL_LEDGER_MAPS",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "LEDGER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_RECONCILIATIONS_EXPORT_BATCH_ID",
                table: "GL_ORACLE_GL_RECONCILIATIONS",
                column: "EXPORT_BATCH_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_SOURCE_FEEDBACKS_EXPORT_BATCH_ID",
                table: "GL_ORACLE_GL_SOURCE_FEEDBACKS",
                column: "EXPORT_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_SOURCE_FEEDBACKS_JOURNAL_HEADER_ID",
                table: "GL_ORACLE_GL_SOURCE_FEEDBACKS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_SOURCE_FEEDBACKS_SOURCE_MODULE",
                table: "GL_ORACLE_GL_SOURCE_FEEDBACKS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ORACLE_GL_SOURCE_MAPS_EXTERNAL_SYSTEM_ID",
                table: "GL_ORACLE_GL_SOURCE_MAPS",
                columns: new[] { "EXTERNAL_SYSTEM_ID", "FINOVA_SOURCE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_VALIDATION_RESULTS_EXPORT_BATCH_ID",
                table: "GL_ORACLE_GL_VALIDATION_RESULTS",
                column: "EXPORT_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_VALIDATION_RESULTS_EXPORT_HEADER_ID",
                table: "GL_ORACLE_GL_VALIDATION_RESULTS",
                column: "EXPORT_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ORACLE_GL_VALIDATION_RESULTS_EXPORT_LINE_ID",
                table: "GL_ORACLE_GL_VALIDATION_RESULTS",
                column: "EXPORT_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_ACCESS_EXCEPTIONS_ALLOWED_USER_NAME",
                table: "GL_PERIOD_ACCESS_EXCEPTIONS",
                columns: new[] { "ALLOWED_USER_NAME", "ALLOWED_ROLE_CODE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_ACCESS_EXCEPTIONS_LEDGER_PERIOD_ID_IS_ACTIVE",
                table: "GL_PERIOD_ACCESS_EXCEPTIONS",
                columns: new[] { "LEDGER_PERIOD_ID", "IS_ACTIVE", "APPROVAL_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_ACCESS_EXCEPTIONS_LEDGER_PERIOD_ID_EXCEPTION_CODE",
                table: "GL_PERIOD_ACCESS_EXCEPTIONS",
                columns: new[] { "LEDGER_PERIOD_ID", "EXCEPTION_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CLOSE_CHECKLISTS_CLOSE_TASK_ID_TASK_STATUS",
                table: "GL_PERIOD_CLOSE_CHECKLISTS",
                columns: new[] { "CLOSE_TASK_ID", "TASK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CLOSE_CHECKLISTS_LEDGER_PERIOD_ID_TASK_STATUS",
                table: "GL_PERIOD_CLOSE_CHECKLISTS",
                columns: new[] { "LEDGER_PERIOD_ID", "TASK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_CLOSE_CHECKLISTS_LEDGER_PERIOD_ID_CLOSE_TASK_ID",
                table: "GL_PERIOD_CLOSE_CHECKLISTS",
                columns: new[] { "LEDGER_PERIOD_ID", "CLOSE_TASK_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CLOSE_TASKS_TASK_AREA_IS_ACTIVE_DISPLAY_SEQ",
                table: "GL_PERIOD_CLOSE_TASKS",
                columns: new[] { "TASK_AREA", "IS_ACTIVE", "DISPLAY_SEQ" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_CLOSE_TASKS_TASK_CODE",
                table: "GL_PERIOD_CLOSE_TASKS",
                column: "TASK_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CONTROL_RULE_LINES_PERIOD_RULE_ID_IS_ACTIVE",
                table: "GL_PERIOD_CONTROL_RULE_LINES",
                columns: new[] { "PERIOD_RULE_ID", "IS_ACTIVE", "LINE_NO" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_CONTROL_RULE_LINES_PERIOD_RULE_ID_LINE_NO",
                table: "GL_PERIOD_CONTROL_RULE_LINES",
                columns: new[] { "PERIOD_RULE_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CONTROL_RULES_LEDGER_ID_SOURCE_CODE",
                table: "GL_PERIOD_CONTROL_RULES",
                columns: new[] { "LEDGER_ID", "SOURCE_CODE", "BUSINESS_LINE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_CONTROL_RULES_RULE_TYPE_IS_ACTIVE_DISPLAY_SEQ",
                table: "GL_PERIOD_CONTROL_RULES",
                columns: new[] { "RULE_TYPE", "IS_ACTIVE", "DISPLAY_SEQ" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_CONTROL_RULES_RULE_CODE",
                table: "GL_PERIOD_CONTROL_RULES",
                column: "RULE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_POSTING_WINDOWS_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_PERIOD_POSTING_WINDOWS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_POSTING_WINDOWS_LEDGER_PERIOD_ID",
                table: "GL_PERIOD_POSTING_WINDOWS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_POSTING_WINDOWS_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_PERIOD_POSTING_WINDOWS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "SOURCE_CODE", "CATEGORY_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_SOURCE_STATUSES_LEDGER_PERIOD_ID_SOURCE_STATUS",
                table: "GL_PERIOD_SOURCE_STATUSES",
                columns: new[] { "LEDGER_PERIOD_ID", "SOURCE_STATUS", "TRANSFER_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_SOURCE_STATUSES_SOURCE_CODE_SOURCE_STATUS",
                table: "GL_PERIOD_SOURCE_STATUSES",
                columns: new[] { "SOURCE_CODE", "SOURCE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_PERIOD_SOURCE_STATUSES_LEDGER_PERIOD_ID_SOURCE_CODE",
                table: "GL_PERIOD_SOURCE_STATUSES",
                columns: new[] { "LEDGER_PERIOD_ID", "SOURCE_CODE", "BUSINESS_LINE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_PERIOD_STATUS_HIST_LEDGER_PERIOD_ID",
                table: "GL_PERIOD_STATUS_HIST",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_ACCOUNT_COMBINATION_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "LEDGER_PERIOD_ID", "CURRENCY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_ACCOUNTING_DATE_LEDGER_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                columns: new[] { "ACCOUNTING_DATE", "LEDGER_ID", "ACCOUNT_COMBINATION_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_JOURNAL_BATCH_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_JOURNAL_HEADER_ID_JOURNAL_LINE_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                columns: new[] { "JOURNAL_HEADER_ID", "JOURNAL_LINE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_JOURNAL_LINE_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_LEDGER_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_LEDGER_PERIOD_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_POSTING_RUN_ID_LEDGER_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                columns: new[] { "POSTING_RUN_ID", "LEDGER_ID", "LEDGER_PERIOD_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_POSTING_RUN_LINE_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "POSTING_RUN_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_QUEUE_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "QUEUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_REVERSAL_IMPACT_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "REVERSAL_IMPACT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_EXCEPTIONS_JOURNAL_BATCH_ID",
                table: "GL_POSTING_EXCEPTIONS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_EXCEPTIONS_JOURNAL_HEADER_ID_JOURNAL_LINE_ID",
                table: "GL_POSTING_EXCEPTIONS",
                columns: new[] { "JOURNAL_HEADER_ID", "JOURNAL_LINE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_EXCEPTIONS_JOURNAL_LINE_ID",
                table: "GL_POSTING_EXCEPTIONS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_EXCEPTIONS_POSTING_RUN_ID_QUEUE_ID",
                table: "GL_POSTING_EXCEPTIONS",
                columns: new[] { "POSTING_RUN_ID", "QUEUE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_EXCEPTIONS_QUEUE_ID",
                table: "GL_POSTING_EXCEPTIONS",
                column: "QUEUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_LOCKS_LEDGER_ID_LEDGER_PERIOD_ID_LOCK_STATUS",
                table: "GL_POSTING_LOCKS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "LOCK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_LOCKS_LEDGER_PERIOD_ID",
                table: "GL_POSTING_LOCKS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_LOCKS_POSTING_RUN_ID",
                table: "GL_POSTING_LOCKS",
                column: "POSTING_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_LOCKS_QUEUE_ID_POSTING_RUN_ID",
                table: "GL_POSTING_LOCKS",
                columns: new[] { "QUEUE_ID", "POSTING_RUN_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_LOCKS_LOCK_TYPE_LOCK_KEY_LOCK_STATUS",
                table: "GL_POSTING_LOCKS",
                columns: new[] { "LOCK_TYPE", "LOCK_KEY", "LOCK_STATUS" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_PROFILE_RULES_POSTING_PROFILE_ID_RULE_SCOPE",
                table: "GL_POSTING_PROFILE_RULES",
                columns: new[] { "POSTING_PROFILE_ID", "RULE_SCOPE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_PROFILE_RULES_POSTING_PROFILE_ID_RULE_SEQ",
                table: "GL_POSTING_PROFILE_RULES",
                columns: new[] { "POSTING_PROFILE_ID", "RULE_SEQ" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_PROFILES_LEDGER_ID_SOURCE_CODE_CATEGORY_CODE",
                table: "GL_POSTING_PROFILES",
                columns: new[] { "LEDGER_ID", "SOURCE_CODE", "CATEGORY_CODE", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_PROFILES_POSTING_MODE_IS_ACTIVE_PRIORITY",
                table: "GL_POSTING_PROFILES",
                columns: new[] { "POSTING_MODE", "IS_ACTIVE", "PRIORITY" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_PROFILES_PROFILE_CODE",
                table: "GL_POSTING_PROFILES",
                column: "PROFILE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_BATCH_ID",
                table: "GL_POSTING_QUEUES",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_JOURNAL_HEADER_ID_REQUEST_STATUS",
                table: "GL_POSTING_QUEUES",
                columns: new[] { "JOURNAL_HEADER_ID", "REQUEST_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_LEDGER_ID_LEDGER_PERIOD_ID_REQUEST_STATUS",
                table: "GL_POSTING_QUEUES",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "REQUEST_STATUS", "REQUEST_PRIORITY" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_LEDGER_PERIOD_ID",
                table: "GL_POSTING_QUEUES",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_POSTING_PROFILE_ID",
                table: "GL_POSTING_QUEUES",
                column: "POSTING_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_POSTING_RUN_ID",
                table: "GL_POSTING_QUEUES",
                column: "POSTING_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_QUEUES_SOURCE_MODULE_SOURCE_DOCUMENT_TYPE",
                table: "GL_POSTING_QUEUES",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID", "SOURCE_TRANSACTION_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_QUEUES_QUEUE_NO",
                table: "GL_POSTING_QUEUES",
                column: "QUEUE_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RECONCILIATIONS_LEDGER_ID_LEDGER_PERIOD_ID",
                table: "GL_POSTING_RECONCILIATIONS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "RECON_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RECONCILIATIONS_LEDGER_PERIOD_ID",
                table: "GL_POSTING_RECONCILIATIONS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RECONCILIATIONS_POSTING_RUN_ID_RECON_STATUS",
                table: "GL_POSTING_RECONCILIATIONS",
                columns: new[] { "POSTING_RUN_ID", "RECON_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_RECONCILIATIONS_RECON_NO",
                table: "GL_POSTING_RECONCILIATIONS",
                column: "RECON_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_POSTING_ROLLBACK_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_LINES_BALANCE_IMPACT_ID",
                table: "GL_POSTING_ROLLBACK_LINES",
                column: "BALANCE_IMPACT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_LINES_JOURNAL_HEADER_ID_JOURNAL_LINE_ID",
                table: "GL_POSTING_ROLLBACK_LINES",
                columns: new[] { "JOURNAL_HEADER_ID", "JOURNAL_LINE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_LINES_JOURNAL_LINE_ID",
                table: "GL_POSTING_ROLLBACK_LINES",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_LINES_ROLLBACK_RUN_ID_ROLLBACK_STATUS",
                table: "GL_POSTING_ROLLBACK_LINES",
                columns: new[] { "ROLLBACK_RUN_ID", "ROLLBACK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_ROLLBACK_RUNS_POSTING_RUN_ID_ROLLBACK_STATUS",
                table: "GL_POSTING_ROLLBACK_RUNS",
                columns: new[] { "POSTING_RUN_ID", "ROLLBACK_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_ROLLBACK_RUNS_ROLLBACK_RUN_NO",
                table: "GL_POSTING_ROLLBACK_RUNS",
                column: "ROLLBACK_RUN_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUN_LINES_JOURNAL_HEADER_ID_POSTING_STATUS",
                table: "GL_POSTING_RUN_LINES",
                columns: new[] { "JOURNAL_HEADER_ID", "POSTING_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_RUN_LINES_POSTING_RUN_ID_JOURNAL_HEADER_ID",
                table: "GL_POSTING_RUN_LINES",
                columns: new[] { "POSTING_RUN_ID", "JOURNAL_HEADER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_LEDGER_ID_LEDGER_PERIOD_ID_RUN_STATUS",
                table: "GL_POSTING_RUNS",
                columns: new[] { "LEDGER_ID", "LEDGER_PERIOD_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_LEDGER_PERIOD_ID",
                table: "GL_POSTING_RUNS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_POSTING_PROFILE_ID",
                table: "GL_POSTING_RUNS",
                column: "POSTING_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_QUEUE_ID",
                table: "GL_POSTING_RUNS",
                column: "QUEUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_SOURCE_ID",
                table: "GL_POSTING_RUNS",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_RUNS_SUBMITTED_BATCH_ID",
                table: "GL_POSTING_RUNS",
                column: "SUBMITTED_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_RUNS_LEDGER_ID_POSTING_RUN_NO",
                table: "GL_POSTING_RUNS",
                columns: new[] { "LEDGER_ID", "POSTING_RUN_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SCHEDULES_LEDGER_ID_SOURCE_CODE_IS_ACTIVE",
                table: "GL_POSTING_SCHEDULES",
                columns: new[] { "LEDGER_ID", "SOURCE_CODE", "IS_ACTIVE", "NEXT_RUN_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SCHEDULES_POSTING_PROFILE_ID",
                table: "GL_POSTING_SCHEDULES",
                column: "POSTING_PROFILE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_SCHEDULES_SCHEDULE_CODE",
                table: "GL_POSTING_SCHEDULES",
                column: "SCHEDULE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SEQUENCE_AUDITS_LEDGER_ID",
                table: "GL_POSTING_SEQUENCE_AUDITS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SEQUENCE_AUDITS_LEDGER_PERIOD_ID",
                table: "GL_POSTING_SEQUENCE_AUDITS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SEQUENCE_AUDITS_SEQUENCE_SCOPE_LEDGER_ID",
                table: "GL_POSTING_SEQUENCE_AUDITS",
                columns: new[] { "SEQUENCE_SCOPE", "LEDGER_ID", "LEDGER_PERIOD_ID", "GENERATED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SOURCE_FEEDBACKS_FEEDBACK_STATUS_LAST_ATTEMPT_AT",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                columns: new[] { "FEEDBACK_STATUS", "LAST_ATTEMPT_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SOURCE_FEEDBACKS_JOURNAL_HEADER_ID",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SOURCE_FEEDBACKS_POSTING_RUN_ID",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                column: "POSTING_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SOURCE_FEEDBACKS_QUEUE_ID",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                column: "QUEUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_SOURCE_FEEDBACKS_SOURCE_MODULE_SOURCE_DOCUMENT_ID",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_ID", "POSTING_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_SOURCE_FEEDBACKS_SOURCE_MODULE_SOURCE_DOCUMENT_ID",
                table: "GL_POSTING_SOURCE_FEEDBACKS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_ID", "SOURCE_TRANSACTION_ID", "JOURNAL_HEADER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_STATUS_EVENTS_JOURNAL_BATCH_ID",
                table: "GL_POSTING_STATUS_EVENTS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_STATUS_EVENTS_JOURNAL_HEADER_ID",
                table: "GL_POSTING_STATUS_EVENTS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_STATUS_EVENTS_POSTING_RUN_ID_EVENT_AT",
                table: "GL_POSTING_STATUS_EVENTS",
                columns: new[] { "POSTING_RUN_ID", "EVENT_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_STATUS_EVENTS_QUEUE_ID_EVENT_AT",
                table: "GL_POSTING_STATUS_EVENTS",
                columns: new[] { "QUEUE_ID", "EVENT_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_STATUS_EVENTS_SOURCE_MODULE_SOURCE_DOCUMENT_ID",
                table: "GL_POSTING_STATUS_EVENTS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_ID", "EVENT_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_JOURNAL_BATCH_ID",
                table: "GL_POSTING_VALIDATION_RESULTS",
                column: "JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_JOURNAL_HEADER_ID",
                table: "GL_POSTING_VALIDATION_RESULTS",
                columns: new[] { "JOURNAL_HEADER_ID", "RESULT_STATUS", "SEVERITY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_JOURNAL_LINE_ID",
                table: "GL_POSTING_VALIDATION_RESULTS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_POSTING_RUN_ID_RESULT_STATUS",
                table: "GL_POSTING_VALIDATION_RESULTS",
                columns: new[] { "POSTING_RUN_ID", "RESULT_STATUS", "SEVERITY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_POSTING_VALIDATION_RULE_ID",
                table: "GL_POSTING_VALIDATION_RESULTS",
                column: "POSTING_VALIDATION_RULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RESULTS_QUEUE_ID_RESULT_STATUS",
                table: "GL_POSTING_VALIDATION_RESULTS",
                columns: new[] { "QUEUE_ID", "RESULT_STATUS", "SEVERITY_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_RULES_VALIDATION_SET_ID",
                table: "GL_POSTING_VALIDATION_RULES",
                columns: new[] { "VALIDATION_SET_ID", "VALIDATION_STAGE", "IS_ACTIVE", "RULE_SEQ" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_VALIDATION_RULES_VALIDATION_SET_ID_RULE_CODE",
                table: "GL_POSTING_VALIDATION_RULES",
                columns: new[] { "VALIDATION_SET_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_VALIDATION_SETS_LEDGER_ID",
                table: "GL_POSTING_VALIDATION_SETS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_POSTING_VALIDATION_SETS_VALIDATION_SET_CODE",
                table: "GL_POSTING_VALIDATION_SETS",
                column: "VALIDATION_SET_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RATE_IMPORT_BATCHES_RATE_TYPE_ID",
                table: "GL_RATE_IMPORT_BATCHES",
                column: "RATE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RATE_IMPORT_BATCHES_BATCH_CODE",
                table: "GL_RATE_IMPORT_BATCHES",
                column: "BATCH_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RATE_IMPORT_LINES_DAILY_RATE_ID",
                table: "GL_RATE_IMPORT_LINES",
                column: "DAILY_RATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RATE_IMPORT_LINES_IMPORT_BATCH_ID_VALIDATION_STATUS",
                table: "GL_RATE_IMPORT_LINES",
                columns: new[] { "IMPORT_BATCH_ID", "VALIDATION_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RATE_IMPORT_LINES_IMPORT_BATCH_ID_LINE_NO",
                table: "GL_RATE_IMPORT_LINES",
                columns: new[] { "IMPORT_BATCH_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RATE_TYPES_RATE_TYPE_CODE",
                table: "GL_RATE_TYPES",
                column: "RATE_TYPE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RECURRING_JOURNAL_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_RECURRING_JOURNAL_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RECURRING_JOURNAL_LINES_RECURRING_TEMPLATE_ID_LINE_NO",
                table: "GL_RECURRING_JOURNAL_LINES",
                columns: new[] { "RECURRING_TEMPLATE_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RECURRING_JOURNAL_RUNS_GENERATED_HEADER_ID",
                table: "GL_RECURRING_JOURNAL_RUNS",
                column: "GENERATED_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RECURRING_JOURNAL_RUNS_LEDGER_PERIOD_ID",
                table: "GL_RECURRING_JOURNAL_RUNS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RECURRING_JOURNAL_RUNS_RECURRING_TEMPLATE_ID",
                table: "GL_RECURRING_JOURNAL_RUNS",
                columns: new[] { "RECURRING_TEMPLATE_ID", "LEDGER_PERIOD_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RECURRING_JOURNAL_TEMPLATES_CATEGORY_ID",
                table: "GL_RECURRING_JOURNAL_TEMPLATES",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RECURRING_JOURNAL_TEMPLATES_SOURCE_ID",
                table: "GL_RECURRING_JOURNAL_TEMPLATES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_RECURRING_JOURNAL_TEMPLATES_LEDGER_ID_TEMPLATE_CODE",
                table: "GL_RECURRING_JOURNAL_TEMPLATES",
                columns: new[] { "LEDGER_ID", "TEMPLATE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_RETAINED_EARNINGS_TRANSFERS_JOURNAL_HEADER_ID",
                table: "GL_RETAINED_EARNINGS_TRANSFERS",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RETAINED_EARNINGS_TRANSFERS_JOURNAL_LINE_ID",
                table: "GL_RETAINED_EARNINGS_TRANSFERS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RETAINED_EARNINGS_TRANSFERS_RETAINED_EARNINGS_AC_E4DBD9C5",
                table: "GL_RETAINED_EARNINGS_TRANSFERS",
                column: "RETAINED_EARNINGS_ACCOUNT_COMB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_RETAINED_EARNINGS_TRANSFERS_YEAR_END_RUN_ID",
                table: "GL_RETAINED_EARNINGS_TRANSFERS",
                column: "YEAR_END_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SETUP_AUDITS_ENTITY_NAME_ENTITY_KEY",
                table: "GL_SETUP_AUDITS",
                columns: new[] { "ENTITY_NAME", "ENTITY_KEY" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_ACCOUNTING_EVENT_ID",
                table: "GL_SOURCE_POSTING_LINKS",
                column: "ACCOUNTING_EVENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_JOURNAL_HEADER_ID_JOURNAL_LINE_ID",
                table: "GL_SOURCE_POSTING_LINKS",
                columns: new[] { "JOURNAL_HEADER_ID", "JOURNAL_LINE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_JOURNAL_LINE_ID",
                table: "GL_SOURCE_POSTING_LINKS",
                column: "JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_LEDGER_ID",
                table: "GL_SOURCE_POSTING_LINKS",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_REVERSAL_LINK_ID",
                table: "GL_SOURCE_POSTING_LINKS",
                column: "REVERSAL_LINK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_POSTING_LINKS_SOURCE_MODULE_SOURCE_DOCUMENT_TYPE",
                table: "GL_SOURCE_POSTING_LINKS",
                columns: new[] { "SOURCE_MODULE", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_SOURCE_POSTING_LINKS_SOURCE_ID_SOURCE_DOCUMENT_TYPE",
                table: "GL_SOURCE_POSTING_LINKS",
                columns: new[] { "SOURCE_ID", "SOURCE_DOCUMENT_TYPE", "SOURCE_DOCUMENT_ID", "JOURNAL_HEADER_ID", "JOURNAL_LINE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_VALUE_DEFINITIONS_DEFAULT_CURRENCY_CODE",
                table: "GL_SOURCE_VALUE_DEFINITIONS",
                column: "DEFAULT_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_VALUE_DEFINITIONS_EVENT_TYPE_ID_VALUE_KIND",
                table: "GL_SOURCE_VALUE_DEFINITIONS",
                columns: new[] { "EVENT_TYPE_ID", "VALUE_KIND", "IS_ACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_SOURCE_VALUE_DEFINITIONS_SOURCE_MODULE_VALUE_CODE",
                table: "GL_SOURCE_VALUE_DEFINITIONS",
                columns: new[] { "SOURCE_MODULE", "VALUE_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_SOURCE_VALUE_DEFINITIONS_SOURCE_ID_EVENT_TYPE_ID",
                table: "GL_SOURCE_VALUE_DEFINITIONS",
                columns: new[] { "SOURCE_ID", "EVENT_TYPE_ID", "VALUE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_YEAR_END_CLOSE_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_LINES_ACCOUNT_GROUP_ID",
                table: "GL_YEAR_END_CLOSE_LINES",
                column: "ACCOUNT_GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_LINES_CLOSING_JOURNAL_LINE_ID",
                table: "GL_YEAR_END_CLOSE_LINES",
                column: "CLOSING_JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_LINES_OPENING_JOURNAL_LINE_ID",
                table: "GL_YEAR_END_CLOSE_LINES",
                column: "OPENING_JOURNAL_LINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_LINES_YEAR_END_RUN_ID_CLOSING_BEHAVIOR",
                table: "GL_YEAR_END_CLOSE_LINES",
                columns: new[] { "YEAR_END_RUN_ID", "CLOSING_BEHAVIOR", "LINE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "UQ_GL_YEAR_END_CLOSE_LINES_YEAR_END_RUN_ID",
                table: "GL_YEAR_END_CLOSE_LINES",
                columns: new[] { "YEAR_END_RUN_ID", "ACCOUNT_COMBINATION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_CLOSE_RUN_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "CLOSE_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_CLOSING_JOURNAL_BATCH_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "CLOSING_JOURNAL_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_FISCAL_YEAR_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "FISCAL_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_LEDGER_ID_FISCAL_YEAR_ID_RUN_STATUS",
                table: "GL_YEAR_END_CLOSE_RUNS",
                columns: new[] { "LEDGER_ID", "FISCAL_YEAR_ID", "RUN_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_OPENING_BALANCE_BATCH_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "OPENING_BALANCE_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_RETAINED_EARNINGS_ACCOUNT_COMB_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "RETAINED_EARNINGS_ACCOUNT_COMB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_YEAR_END_CLOSE_RUNS_TARGET_FISCAL_YEAR_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                column: "TARGET_FISCAL_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_YEAR_END_CLOSE_RUNS_LEDGER_ID_FISCAL_YEAR_ID",
                table: "GL_YEAR_END_CLOSE_RUNS",
                columns: new[] { "LEDGER_ID", "FISCAL_YEAR_ID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_POSTING_QUEUES",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "QUEUE_ID",
                principalTable: "GL_POSTING_QUEUES",
                principalColumn: "QUEUE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_POSTING_RUNS",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "POSTING_RUN_ID",
                principalTable: "GL_POSTING_RUNS",
                principalColumn: "POSTING_RUN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_BALANCE_IMPACTS_GL_POSTING_RUN_LINES",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "POSTING_RUN_LINE_ID",
                principalTable: "GL_POSTING_RUN_LINES",
                principalColumn: "POSTING_RUN_LINE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_EXCEPTIONS_GL_POSTING_QUEUES",
                table: "GL_POSTING_EXCEPTIONS",
                column: "QUEUE_ID",
                principalTable: "GL_POSTING_QUEUES",
                principalColumn: "QUEUE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_EXCEPTIONS_GL_POSTING_RUNS",
                table: "GL_POSTING_EXCEPTIONS",
                column: "POSTING_RUN_ID",
                principalTable: "GL_POSTING_RUNS",
                principalColumn: "POSTING_RUN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_LOCKS_GL_POSTING_QUEUES",
                table: "GL_POSTING_LOCKS",
                column: "QUEUE_ID",
                principalTable: "GL_POSTING_QUEUES",
                principalColumn: "QUEUE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_LOCKS_GL_POSTING_RUNS",
                table: "GL_POSTING_LOCKS",
                column: "POSTING_RUN_ID",
                principalTable: "GL_POSTING_RUNS",
                principalColumn: "POSTING_RUN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GL_POSTING_QUEUES_GL_POSTING_RUNS",
                table: "GL_POSTING_QUEUES",
                column: "POSTING_RUN_ID",
                principalTable: "GL_POSTING_RUNS",
                principalColumn: "POSTING_RUN_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GL_LEDGERS_FIN_LEGAL_ENTITIES",
                table: "GL_LEDGERS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_LEDGERS_FIN_CURRENCIES",
                table: "GL_LEDGERS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_QUEUES_GL_JOURNAL_HEADERS",
                table: "GL_POSTING_QUEUES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_JOURNAL_BATCHES_GL_LEDGERS",
                table: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_LEDGER_PERIODS_GL_LEDGERS",
                table: "GL_LEDGER_PERIODS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_PROFILES_GL_LEDGERS",
                table: "GL_POSTING_PROFILES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_QUEUES_GL_LEDGERS",
                table: "GL_POSTING_QUEUES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_RUNS_GL_LEDGERS",
                table: "GL_POSTING_RUNS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_JOURNAL_BATCHES_GL_LEDGER_PERIODS",
                table: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_QUEUES_GL_LEDGER_PERIODS",
                table: "GL_POSTING_QUEUES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_RUNS_GL_LEDGER_PERIODS",
                table: "GL_POSTING_RUNS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_JOURNAL_BATCHES_GL_JOURNAL_SOURCES",
                table: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_JOURNAL_CATEGORIES_GL_JOURNAL_SOURCES",
                table: "GL_JOURNAL_CATEGORIES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_RUNS_GL_JOURNAL_SOURCES",
                table: "GL_POSTING_RUNS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_JOURNAL_BATCHES_GL_JOURNAL_CATEGORIES",
                table: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_QUEUES_GL_JOURNAL_BATCHES",
                table: "GL_POSTING_QUEUES");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_RUNS_GL_JOURNAL_BATCHES",
                table: "GL_POSTING_RUNS");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_POSTING_RUNS_GL_POSTING_QUEUES",
                table: "GL_POSTING_RUNS");

            migrationBuilder.DropTable(
                name: "ADM_LKP_BUSINESS_LINES");

            migrationBuilder.DropTable(
                name: "ADM_LKP_LEGAL_ENTITIES");

            migrationBuilder.DropTable(
                name: "FIN_BUSINESS_LINES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_BALANCES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATION_RULE_LINES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATION_SEGMENTS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_HIERARCHY_NODES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_EVENT_VALUES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_STATUSES");

            migrationBuilder.DropTable(
                name: "GL_AUTO_ACCOUNTING_CONDITIONS");

            migrationBuilder.DropTable(
                name: "GL_AUTO_ACCOUNTING_GENERATED_LINES");

            migrationBuilder.DropTable(
                name: "GL_AUTO_RULE_APPROVAL_HIST");

            migrationBuilder.DropTable(
                name: "GL_AUTO_RULE_AUDITS");

            migrationBuilder.DropTable(
                name: "GL_AUTO_RULE_LINE_SEGMENTS");

            migrationBuilder.DropTable(
                name: "GL_AUTO_RULE_TEST_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_BALANCE_REBUILD_LINES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCOUNT_GROUP_LINES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCRUAL_LINES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACTION_HIST");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ATTACHMENTS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_COMMENTS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_PERIOD_LOCKS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_PROFILE_MODULES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_RECURRING_RUNS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_RUN_MODULE_STATUSES");

            migrationBuilder.DropTable(
                name: "GL_CURRENCY_EXPOSURE_LINES");

            migrationBuilder.DropTable(
                name: "GL_DAILY_BALANCES");

            migrationBuilder.DropTable(
                name: "GL_FX_RATE_USAGE_LOGS");

            migrationBuilder.DropTable(
                name: "GL_FX_REVALUATION_LINES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_ACTION_HIST");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_APPROVAL_HIST");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_ATTACHMENTS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_COMMENTS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_NUMBERING_RULES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_VALIDATION_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_LEDGER_BRANCHES");

            migrationBuilder.DropTable(
                name: "GL_LEDGER_OPTIONS");

            migrationBuilder.DropTable(
                name: "GL_OPENING_BALANCE_LINES");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_ACCOUNT_MAPS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_CATEGORY_MAPS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_EXPORT_AUDITS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_IMPORT_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_RECONCILIATIONS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_SOURCE_FEEDBACKS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_SOURCE_MAPS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_VALIDATION_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_ACCESS_EXCEPTIONS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_CLOSE_CHECKLISTS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_CONTROL_RULE_LINES");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_POSTING_WINDOWS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_SOURCE_STATUSES");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_STATUS_HIST");

            migrationBuilder.DropTable(
                name: "GL_POSTING_EXCEPTIONS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_LOCKS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_PROFILE_RULES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_RECONCILIATIONS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_ROLLBACK_LINES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_SCHEDULES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_SEQUENCE_AUDITS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_SOURCE_FEEDBACKS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_STATUS_EVENTS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_VALIDATION_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_RATE_IMPORT_LINES");

            migrationBuilder.DropTable(
                name: "GL_RECURRING_JOURNAL_LINES");

            migrationBuilder.DropTable(
                name: "GL_RECURRING_JOURNAL_RUNS");

            migrationBuilder.DropTable(
                name: "GL_RETAINED_EARNINGS_TRANSFERS");

            migrationBuilder.DropTable(
                name: "GL_SETUP_AUDITS");

            migrationBuilder.DropTable(
                name: "GL_SOURCE_POSTING_LINKS");

            migrationBuilder.DropTable(
                name: "GL_YEAR_END_CLOSE_LINES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATION_RULES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_HIERARCHIES");

            migrationBuilder.DropTable(
                name: "GL_AUTO_ACCOUNTING_GENERATIONS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_INTERFACE_LINES");

            migrationBuilder.DropTable(
                name: "GL_AUTO_ACCOUNTING_RULE_LINES");

            migrationBuilder.DropTable(
                name: "GL_AUTO_RULE_TEST_CASES");

            migrationBuilder.DropTable(
                name: "GL_BALANCE_REBUILD_RUNS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCRUAL_RULE_LINES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCRUAL_RUNS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_REPORTS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_EXCEPTIONS");

            migrationBuilder.DropTable(
                name: "GL_CURRENCY_EXPOSURE_SNAPSHOTS");

            migrationBuilder.DropTable(
                name: "GL_FX_REVALUATION_RUNS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_VALIDATION_RULES");

            migrationBuilder.DropTable(
                name: "GL_OPENING_BALANCE_RUNS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_IMPORT_REQUESTS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_INTERFACE_ROWS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_CLOSE_TASKS");

            migrationBuilder.DropTable(
                name: "GL_PERIOD_CONTROL_RULES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_BALANCE_IMPACTS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_ROLLBACK_RUNS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_VALIDATION_RULES");

            migrationBuilder.DropTable(
                name: "GL_DAILY_RATES");

            migrationBuilder.DropTable(
                name: "GL_RATE_IMPORT_BATCHES");

            migrationBuilder.DropTable(
                name: "GL_RECURRING_JOURNAL_TEMPLATES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCOUNT_GROUPS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_EVENTS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_INTERFACE_BATCHES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_ACCOUNT_LOOKUPS");

            migrationBuilder.DropTable(
                name: "GL_SOURCE_VALUE_DEFINITIONS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_ACCRUAL_RULES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_VALIDATION_RESULTS");

            migrationBuilder.DropTable(
                name: "GL_FX_REVALUATION_RULES");

            migrationBuilder.DropTable(
                name: "GL_YEAR_END_CLOSE_RUNS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_EXPORT_LINES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_RUN_LINES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_VALIDATION_SETS");

            migrationBuilder.DropTable(
                name: "GL_AUTO_ACCOUNTING_RULE_SETS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_RUN_TASKS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_VALIDATION_RULES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_LINES");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_EXPORT_HEADERS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_EVENT_TYPES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_PROFILE_STEPS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_RUNS");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_VALIDATION_SETS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATIONS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_EXPORT_BATCHES");

            migrationBuilder.DropTable(
                name: "GL_CLOSE_PROFILES");

            migrationBuilder.DropTable(
                name: "GL_COA_SEGMENT_VALUES");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_EXPORT_PROFILES");

            migrationBuilder.DropTable(
                name: "FIN_COST_CENTERS");

            migrationBuilder.DropTable(
                name: "GL_COA_SEGMENTS");

            migrationBuilder.DropTable(
                name: "GL_ORACLE_GL_LEDGER_MAPS");

            migrationBuilder.DropTable(
                name: "FIN_BRANCHES");

            migrationBuilder.DropTable(
                name: "GL_EXTERNAL_GL_SYSTEMS");

            migrationBuilder.DropTable(
                name: "FIN_LEGAL_ENTITIES");

            migrationBuilder.DropTable(
                name: "FIN_CURRENCIES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_HEADERS");

            migrationBuilder.DropTable(
                name: "GL_RATE_TYPES");

            migrationBuilder.DropTable(
                name: "GL_LEDGERS");

            migrationBuilder.DropTable(
                name: "GL_COA_STRUCTURES");

            migrationBuilder.DropTable(
                name: "GL_LEDGER_PERIODS");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNTING_PERIODS");

            migrationBuilder.DropTable(
                name: "GL_FISCAL_YEARS");

            migrationBuilder.DropTable(
                name: "GL_CALENDARS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_SOURCES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_CATEGORIES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_QUEUES");

            migrationBuilder.DropTable(
                name: "GL_POSTING_RUNS");

            migrationBuilder.DropTable(
                name: "GL_POSTING_PROFILES");
        }
    }
}
