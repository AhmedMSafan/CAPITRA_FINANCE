using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MffFromApex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MFF_AUDIT_LOGS",
                columns: table => new
                {
                    AUDIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TABLE_NAME = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RECORD_KEY = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ACTION_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OLD_DATA_JSON = table.Column<string>(type: "text", nullable: true),
                    NEW_DATA_JSON = table.Column<string>(type: "text", nullable: true),
                    ACTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ACTION_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    APP_ID = table.Column<long>(type: "bigint", nullable: true),
                    PAGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SESSION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CLIENT_IP = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_AUDIT_LOGS", x => x.AUDIT_ID);
                });

            migrationBuilder.CreateTable(
                name: "MFF_HOUSING_ANNOUNCEMENTS",
                columns: table => new
                {
                    ANNOUNCEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ANNOUNCEMENT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ANNOUNCEMENT_NAME_AR = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ISSUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CLOSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_HOUSING_ANNOUNCEMENTS", x => x.ANNOUNCEMENT_ID);
                });

            migrationBuilder.CreateTable(
                name: "MFF_INTEGRATION_LOGS",
                columns: table => new
                {
                    INTEGRATION_LOG_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INTERFACE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DIRECTION = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    REFERENCE_KEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REQUEST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RESPONSE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    INTEGRATION_LOG_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HTTP_STATUS = table.Column<int>(type: "integer", nullable: true),
                    REQUEST_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    RESPONSE_PAYLOAD = table.Column<string>(type: "text", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_INTEGRATION_LOGS", x => x.INTEGRATION_LOG_ID);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_ACCOUNT_EVENTS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_ACCOUNT_EVENTS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_APPLICATION_STATUSES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_APPLICATION_STATUSES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_DECISION_CODES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_DECISION_CODES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_DELINQUENCY_STAGES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_DELINQUENCY_STAGES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_DOCUMENT_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_DOCUMENT_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_EMPLOYMENT_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_EMPLOYMENT_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_EVENT_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_EVENT_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_GOVERNORATES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_GOVERNORATES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_INCOME_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_INCOME_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_MARITAL_STATUSES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_MARITAL_STATUSES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_ORG_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_ORG_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_PARTY_ROLES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_PARTY_ROLES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_PAYMENT_STATUSES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_PAYMENT_STATUSES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_PROPERTY_REG_STATUSES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_PROPERTY_REG_STATUSES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_REGULATORS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_REGULATORS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_SOURCE_CHANNELS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_SOURCE_CHANNELS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_SUPPORT_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_SUPPORT_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_USER_ROLES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_USER_ROLES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_CITYS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GOVERNORATE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_CITYS", x => x.CODE);
                    table.ForeignKey(
                        name: "FK_MFF_LKP_CITYS_MFF_LKP_GOVERNORATES",
                        column: x => x.GOVERNORATE_CODE,
                        principalTable: "MFF_LKP_GOVERNORATES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PERSONS",
                columns: table => new
                {
                    PERSON_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NATIONAL_ID = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FULL_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BIRTH_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GENDER_CODE = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    MARITAL_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MOBILE_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ADDRESS_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EMPLOYMENT_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EMPLOYER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HIRE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TAX_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COMMERCIAL_REG_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PERSONS", x => x.PERSON_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PERSONS_MFF_LKP_EMPLOYMENT_TYPES",
                        column: x => x.EMPLOYMENT_TYPE_CODE,
                        principalTable: "MFF_LKP_EMPLOYMENT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PERSONS_MFF_LKP_MARITAL_STATUSES",
                        column: x => x.MARITAL_STATUS_CODE,
                        principalTable: "MFF_LKP_MARITAL_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_ORGANIZATIONS",
                columns: table => new
                {
                    ORG_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ORG_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ORG_NAME_AR = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LEGACY_NAME_AR = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ORG_NAME_EN = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ORG_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    REGULATOR_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LICENSE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LICENSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    BANK_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TAX_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ADDRESS_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_ORGANIZATIONS", x => x.ORG_ID);
                    table.ForeignKey(
                        name: "FK_MFF_ORGANIZATIONS_MFF_LKP_ORG_TYPES",
                        column: x => x.ORG_TYPE_CODE,
                        principalTable: "MFF_LKP_ORG_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_ORGANIZATIONS_MFF_LKP_REGULATORS",
                        column: x => x.REGULATOR_CODE,
                        principalTable: "MFF_LKP_REGULATORS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LKP_STATUS_TRANSITIONS",
                columns: table => new
                {
                    FROM_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TO_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ROLE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LKP_STATUS_TRANSITIONS", x => new { x.FROM_STATUS_CODE, x.TO_STATUS_CODE });
                    table.ForeignKey(
                        name: "FK_MFF_LKP_STATUS_TRANSITIONS_MFF_LKP_APPLICATION_STAT_A02ACA4E",
                        column: x => x.FROM_STATUS_CODE,
                        principalTable: "MFF_LKP_APPLICATION_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LKP_STATUS_TRANSITIONS_MFF_LKP_APPLICATION_STAT_F920E979",
                        column: x => x.TO_STATUS_CODE,
                        principalTable: "MFF_LKP_APPLICATION_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LKP_STATUS_TRANSITIONS_MFF_LKP_USER_ROLES",
                        column: x => x.ROLE_CODE,
                        principalTable: "MFF_LKP_USER_ROLES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_HOUSING_PROJECTS",
                columns: table => new
                {
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROJECT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ANNOUNCEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    PROJECT_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GOVERNORATE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CITY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DISTRICT_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NEIGHBORHOOD_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SITE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DEFAULT_AREA_SQM = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_HOUSING_PROJECTS", x => x.PROJECT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_HOUSING_PROJECTS_MFF_HOUSING_ANNOUNCEMENTS",
                        column: x => x.ANNOUNCEMENT_ID,
                        principalTable: "MFF_HOUSING_ANNOUNCEMENTS",
                        principalColumn: "ANNOUNCEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_HOUSING_PROJECTS_MFF_LKP_CITYS",
                        column: x => x.CITY_CODE,
                        principalTable: "MFF_LKP_CITYS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_HOUSING_PROJECTS_MFF_LKP_GOVERNORATES",
                        column: x => x.GOVERNORATE_CODE,
                        principalTable: "MFF_LKP_GOVERNORATES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_GL_ACCOUNT_MAPS",
                columns: table => new
                {
                    MAP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DEBIT_ACCOUNT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREDIT_ACCOUNT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_GL_ACCOUNT_MAPS", x => x.MAP_ID);
                    table.ForeignKey(
                        name: "FK_MFF_GL_ACCOUNT_MAPS_MFF_LKP_ACCOUNT_EVENTS",
                        column: x => x.EVENT_CODE,
                        principalTable: "MFF_LKP_ACCOUNT_EVENTS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_GL_ACCOUNT_MAPS_MFF_ORGANIZATIONS",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROTOCOLS",
                columns: table => new
                {
                    PROTOCOL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROTOCOL_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PROTOCOL_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FUND_ORG_ID = table.Column<long>(type: "bigint", nullable: false),
                    SIGNED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LEGAL_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROTOCOLS", x => x.PROTOCOL_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROTOCOLS_MFF_ORGANIZATIONS",
                        column: x => x.FUND_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROPERTIES",
                columns: table => new
                {
                    PROPERTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SELLER_PERSON_ID = table.Column<long>(type: "bigint", nullable: true),
                    GOVERNORATE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CITY_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DISTRICT_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NEIGHBORHOOD_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BUILDING_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FLOOR_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UNIT_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AREA_SQM = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ADDRESS_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REGISTRATION_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REGISTRY_REFERENCE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SALE_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    APPRAISED_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAINTENANCE_DEPOSIT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROPERTIES", x => x.PROPERTY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROPERTIES_MFF_HOUSING_PROJECTS",
                        column: x => x.PROJECT_ID,
                        principalTable: "MFF_HOUSING_PROJECTS",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROPERTIES_MFF_LKP_CITYS",
                        column: x => x.CITY_CODE,
                        principalTable: "MFF_LKP_CITYS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROPERTIES_MFF_LKP_GOVERNORATES",
                        column: x => x.GOVERNORATE_CODE,
                        principalTable: "MFF_LKP_GOVERNORATES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROPERTIES_MFF_LKP_PROPERTY_REG_STATUSES",
                        column: x => x.REGISTRATION_STATUS_CODE,
                        principalTable: "MFF_LKP_PROPERTY_REG_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROPERTIES_MFF_PERSONS",
                        column: x => x.SELLER_PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROGRAM_VERSIONS",
                columns: table => new
                {
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROTOCOL_ID = table.Column<long>(type: "bigint", nullable: true),
                    PROGRAM_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VERSION_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PROGRAM_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IS_OPERATIONAL = table.Column<bool>(type: "boolean", nullable: false),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROGRAM_VERSIONS", x => x.PROGRAM_VERSION_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_VERSIONS_MFF_PROTOCOLS",
                        column: x => x.PROTOCOL_ID,
                        principalTable: "MFF_PROTOCOLS",
                        principalColumn: "PROTOCOL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROTOCOL_PARTIES",
                columns: table => new
                {
                    PROTOCOL_PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROTOCOL_ID = table.Column<long>(type: "bigint", nullable: false),
                    ORG_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARTY_ROLE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROTOCOL_PARTIES", x => x.PROTOCOL_PARTY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROTOCOL_PARTIES_MFF_ORGANIZATIONS",
                        column: x => x.ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROTOCOL_PARTIES_MFF_PROTOCOLS",
                        column: x => x.PROTOCOL_ID,
                        principalTable: "MFF_PROTOCOLS",
                        principalColumn: "PROTOCOL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_IMPORT_BATCHES",
                columns: table => new
                {
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_CHANNEL_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RECEIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: true),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: true),
                    FUND_ORG_ID = table.Column<long>(type: "bigint", nullable: true),
                    ROW_COUNT = table.Column<int>(type: "integer", nullable: false),
                    VALID_COUNT = table.Column<int>(type: "integer", nullable: false),
                    ERROR_COUNT = table.Column<int>(type: "integer", nullable: false),
                    BATCH_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FILE_CHECKSUM = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_IMPORT_BATCHES", x => x.BATCH_ID);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_BATCHES_MFF_LKP_SOURCE_CHANNELS",
                        column: x => x.SOURCE_CHANNEL_CODE,
                        principalTable: "MFF_LKP_SOURCE_CHANNELS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_BATCHES_MFF_ORGANIZATIONS_FINANCIER_ID",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_BATCHES_MFF_ORGANIZATIONS_FUND_ORG_ID",
                        column: x => x.FUND_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_BATCHES_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROGRAM_DOC_REQUIREMENTS",
                columns: table => new
                {
                    REQUIREMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    DOCUMENT_TYPE_CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PARTY_ROLE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    EMPLOYMENT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    VALIDITY_DAYS = table.Column<int>(type: "integer", nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROGRAM_DOC_REQUIREMENTS", x => x.REQUIREMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_DOC_REQUIREMENTS_MFF_LKP_DOCUMENT_TYPES",
                        column: x => x.DOCUMENT_TYPE_CODE,
                        principalTable: "MFF_LKP_DOCUMENT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_DOC_REQUIREMENTS_MFF_LKP_EMPLOYMENT_TYPES",
                        column: x => x.EMPLOYMENT_TYPE_CODE,
                        principalTable: "MFF_LKP_EMPLOYMENT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_DOC_REQUIREMENTS_MFF_LKP_PARTY_ROLES",
                        column: x => x.PARTY_ROLE_CODE,
                        principalTable: "MFF_LKP_PARTY_ROLES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_DOC_REQUIREMENTS_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROGRAM_PARAMS",
                columns: table => new
                {
                    PARAM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: true),
                    PARAM_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PARAM_NAME_AR = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    VALUE_NUMBER = table.Column<decimal>(type: "numeric(24,6)", precision: 24, scale: 6, nullable: true),
                    VALUE_TEXT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UNIT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROGRAM_PARAMS", x => x.PARAM_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_PARAMS_MFF_ORGANIZATIONS",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_PARAMS_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PROGRAM_PARAM_HIST",
                columns: table => new
                {
                    PARAM_HIST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PARAM_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINANCIER_ORG_ID = table.Column<long>(type: "bigint", nullable: true),
                    PARAM_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OLD_VALUE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NEW_VALUE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CHANGED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHANGE_REASON_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PROGRAM_PARAM_HIST", x => x.PARAM_HIST_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_PARAM_HIST_MFF_ORGANIZATIONS",
                        column: x => x.FINANCIER_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_PARAM_HIST_MFF_PROGRAM_PARAMS",
                        column: x => x.PARAM_ID,
                        principalTable: "MFF_PROGRAM_PARAMS",
                        principalColumn: "PARAM_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PROGRAM_PARAM_HIST_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_ACCOUNTING_ENTRIES",
                columns: table => new
                {
                    ENTRY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    EVENT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENTRY_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    GL_ACCOUNT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DEBIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREDIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CORE_JOURNAL_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DESCRIPTION_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_ACCOUNTING_ENTRIES", x => x.ENTRY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_ACCOUNTING_ENTRIES_MFF_LKP_ACCOUNT_EVENTS",
                        column: x => x.EVENT_CODE,
                        principalTable: "MFF_LKP_ACCOUNT_EVENTS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPEALS",
                columns: table => new
                {
                    APPEAL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    APPEAL_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    REJECTION_DECISION_ID = table.Column<long>(type: "bigint", nullable: true),
                    REASON_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DECISION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DECISION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DECISION_REASON_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPEALS", x => x.APPEAL_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPEALS_MFF_LKP_DECISION_CODES",
                        column: x => x.DECISION_CODE,
                        principalTable: "MFF_LKP_DECISION_CODES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPLICATION_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERSON_ID = table.Column<long>(type: "bigint", nullable: true),
                    DOCUMENT_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FILE_SIZE_BYTES = table.Column<int>(type: "integer", nullable: true),
                    FILE_CONTENT = table.Column<byte[]>(type: "bytea", nullable: true),
                    DOCUMENT_NO = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ISSUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RECEIVED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    VERIFICATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VERIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REJECTION_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPLICATION_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_DOCUMENTS_MFF_LKP_DOCUMENT_TYPES",
                        column: x => x.DOCUMENT_TYPE_CODE,
                        principalTable: "MFF_LKP_DOCUMENT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_DOCUMENTS_MFF_PERSONS",
                        column: x => x.PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPLICATION_PARTIES",
                columns: table => new
                {
                    APPLICATION_PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERSON_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARTY_ROLE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IS_INCOME_INCLUDED = table.Column<bool>(type: "boolean", nullable: false),
                    OWNERSHIP_SHARE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPLICATION_PARTIES", x => x.APPLICATION_PARTY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_PARTIES_MFF_LKP_PARTY_ROLES",
                        column: x => x.PARTY_ROLE_CODE,
                        principalTable: "MFF_LKP_PARTY_ROLES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_PARTIES_MFF_PERSONS",
                        column: x => x.PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_INCOME_SOURCES",
                columns: table => new
                {
                    INCOME_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_PARTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    INCOME_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EMPLOYER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MONTHLY_NET_INCOME = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ANNUAL_NET_INCOME = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    VERIFICATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_INCOME_SOURCES", x => x.INCOME_ID);
                    table.ForeignKey(
                        name: "FK_MFF_INCOME_SOURCES_MFF_APPLICATION_PARTIES",
                        column: x => x.APPLICATION_PARTY_ID,
                        principalTable: "MFF_APPLICATION_PARTIES",
                        principalColumn: "APPLICATION_PARTY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_INCOME_SOURCES_MFF_LKP_INCOME_TYPES",
                        column: x => x.INCOME_TYPE_CODE,
                        principalTable: "MFF_LKP_INCOME_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPLICATION_PROPERTYS",
                columns: table => new
                {
                    APP_PROPERTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROPERTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUESTED_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_SELECTED = table.Column<bool>(type: "boolean", nullable: false),
                    SELECTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPLICATION_PROPERTYS", x => x.APP_PROPERTY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_PROPERTYS_MFF_PROPERTIES",
                        column: x => x.PROPERTY_ID,
                        principalTable: "MFF_PROPERTIES",
                        principalColumn: "PROPERTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPLICATION_STATUS_HIST",
                columns: table => new
                {
                    STATUS_HIST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    OLD_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NEW_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CHANGED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ROLE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    REMARKS_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPLICATION_STATUS_HIST", x => x.STATUS_HIST_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_STATUS_HIST_MFF_LKP_APPLICATION_STA_9F8B368C",
                        column: x => x.OLD_STATUS_CODE,
                        principalTable: "MFF_LKP_APPLICATION_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_STATUS_HIST_MFF_LKP_APPLICATION_STA_DE3D331C",
                        column: x => x.NEW_STATUS_CODE,
                        principalTable: "MFF_LKP_APPLICATION_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATION_STATUS_HIST_MFF_LKP_USER_ROLES",
                        column: x => x.ROLE_CODE,
                        principalTable: "MFF_LKP_USER_ROLES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPLICATIONS",
                columns: table => new
                {
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IMPORT_STAGING_ID = table.Column<long>(type: "bigint", nullable: true),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: false),
                    FUND_ORG_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_CHANNEL_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PRIMARY_PERSON_ID = table.Column<long>(type: "bigint", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    APPLICATION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    FUND_FILE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ELIGIBILITY_CERT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ELIGIBILITY_CERT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ELIGIBILITY_CERT_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REQUESTED_FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    APPROVED_FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_SUBSIDY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROPERTY_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DOWNPAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MONTHLY_INCOME_TOTAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    REJECTION_REASON = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CURRENT_OWNER_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BRANCH_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPLICATIONS", x => x.APPLICATION_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_LKP_APPLICATION_STATUSES",
                        column: x => x.STATUS_CODE,
                        principalTable: "MFF_LKP_APPLICATION_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_LKP_SOURCE_CHANNELS",
                        column: x => x.SOURCE_CHANNEL_CODE,
                        principalTable: "MFF_LKP_SOURCE_CHANNELS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_ORGANIZATIONS_FINANCIER_ID",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_ORGANIZATIONS_FUND_ORG_ID",
                        column: x => x.FUND_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_PERSONS",
                        column: x => x.PRIMARY_PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPLICATIONS_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_APPRAISALS",
                columns: table => new
                {
                    APPRAISAL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROPERTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    APPRAISER_ORG_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPRAISER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LICENSE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REQUEST_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REPORT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    MARKET_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FORCED_SALE_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ACCEPTED_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    REPORT_DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPRAISAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_APPRAISALS", x => x.APPRAISAL_ID);
                    table.ForeignKey(
                        name: "FK_MFF_APPRAISALS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPRAISALS_MFF_APPLICATION_DOCUMENTS",
                        column: x => x.REPORT_DOCUMENT_ID,
                        principalTable: "MFF_APPLICATION_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPRAISALS_MFF_ORGANIZATIONS",
                        column: x => x.APPRAISER_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_APPRAISALS_MFF_PROPERTIES",
                        column: x => x.PROPERTY_ID,
                        principalTable: "MFF_PROPERTIES",
                        principalColumn: "PROPERTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CALCULATION_RUNS",
                columns: table => new
                {
                    CALC_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    PROGRAM_VERSION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: true),
                    RUN_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INPUT_INCOME = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INPUT_PROPERTY_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INPUT_DOWNPAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MORTGAGE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    TERM_YEARS = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    PAYMENT_GROWTH_YEARS = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    WAGE_INCREASE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_PAYMENT_INCOME_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MIN_DOWNPAYMENT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_DOWNPAYMENT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_SUBSIDY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DISCOUNT_FACTOR_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INPUT_MONTHLY_SUBSIDY = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INPUT_SUBSIDY_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    OUT_BASIC_MORTGAGE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_MAX_MORTGAGE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_PV_BUYDOWN_SUBSIDY = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_DOWNPAYMENT_ASSIST = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_TOTAL_SUBSIDY = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_MORTGAGE_LOAN = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_INITIAL_PAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_LTV_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    OUT_BORROWER_DOWNPAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_MAX_PROPERTY_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_PROPERTY_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OUT_MIN_PROPERTY_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_HAS_NEGATIVE_AMORTIZATION = table.Column<bool>(type: "boolean", nullable: false),
                    WARNING_MESSAGE_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CALC_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CALCULATION_RUNS", x => x.CALC_RUN_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CALCULATION_RUNS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CALCULATION_RUNS_MFF_ORGANIZATIONS",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CALCULATION_RUNS_MFF_PROGRAM_VERSIONS",
                        column: x => x.PROGRAM_VERSION_ID,
                        principalTable: "MFF_PROGRAM_VERSIONS",
                        principalColumn: "PROGRAM_VERSION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CONTRACTS",
                columns: table => new
                {
                    CONTRACT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONTRACT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CONTRACT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CONTRACT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    BUYER_PERSON_ID = table.Column<long>(type: "bigint", nullable: false),
                    SELLER_PERSON_ID = table.Column<long>(type: "bigint", nullable: true),
                    FINANCIER_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROPERTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROPERTY_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_SIGNED = table.Column<bool>(type: "boolean", nullable: false),
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CONTRACT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CONTRACTS", x => x.CONTRACT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_APPLICATION_DOCUMENTS",
                        column: x => x.DOCUMENT_ID,
                        principalTable: "MFF_APPLICATION_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_ORGANIZATIONS",
                        column: x => x.FINANCIER_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_PERSONS_BUYER_PERSON_ID",
                        column: x => x.BUYER_PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_PERSONS_SELLER_PERSON_ID",
                        column: x => x.SELLER_PERSON_ID,
                        principalTable: "MFF_PERSONS",
                        principalColumn: "PERSON_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CONTRACTS_MFF_PROPERTIES",
                        column: x => x.PROPERTY_ID,
                        principalTable: "MFF_PROPERTIES",
                        principalColumn: "PROPERTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CREDIT_DECISIONS",
                columns: table => new
                {
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    DECISION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DECISION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    APPROVED_FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    APPROVED_TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    APPROVED_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CONDITIONS_TEXT = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DECIDED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    COMMITTEE_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CREDIT_DECISIONS", x => x.DECISION_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_DECISIONS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_DECISIONS_MFF_LKP_DECISION_CODES",
                        column: x => x.DECISION_CODE,
                        principalTable: "MFF_LKP_DECISION_CODES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CREDIT_INQUIRIES",
                columns: table => new
                {
                    INQUIRY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    BUREAU_ORG_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUEST_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RESPONSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREDIT_SCORE = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RESULT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REPORT_DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CREDIT_INQUIRIES", x => x.INQUIRY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_INQUIRIES_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_INQUIRIES_MFF_APPLICATION_DOCUMENTS",
                        column: x => x.REPORT_DOCUMENT_ID,
                        principalTable: "MFF_APPLICATION_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_INQUIRIES_MFF_ORGANIZATIONS",
                        column: x => x.BUREAU_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CREDIT_STUDIES",
                columns: table => new
                {
                    CREDIT_STUDY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    ANALYST_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    STUDY_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    VERIFIED_INCOME_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MONTHLY_OBLIGATIONS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DISPOSABLE_INCOME_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROPOSED_FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PROPOSED_TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    PROPOSED_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INSTALLMENT_INCOME_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LTV_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RECOMMENDATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CREDIT_STUDIES", x => x.CREDIT_STUDY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_STUDIES_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_CREDIT_STUDIES_MFF_LKP_DECISION_CODES",
                        column: x => x.RECOMMENDATION_CODE,
                        principalTable: "MFF_LKP_DECISION_CODES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_FINANCE_OFFERS",
                columns: table => new
                {
                    OFFER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    OFFER_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OFFER_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    ANNUAL_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    INITIAL_INSTALLMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DOWNPAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SUBSIDY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LTV_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    OFFER_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCEPTED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_FINANCE_OFFERS", x => x.OFFER_ID);
                    table.ForeignKey(
                        name: "FK_MFF_FINANCE_OFFERS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_IMPORT_STAGINGS",
                columns: table => new
                {
                    STAGING_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_ROW_NO = table.Column<int>(type: "integer", nullable: false),
                    APPLICANT_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    APPLICANT_NATIONAL_ID = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SPOUSE_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SPOUSE_NATIONAL_ID = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MARITAL_STATUS_RAW = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MONTHLY_INCOME = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CUSTOMER_DOWNPAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_SUBSIDY = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UNIT_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAINTENANCE_DEPOSIT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FINANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ANNOUNCEMENT_RAW = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PROJECT_RAW = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GOVERNORATE_RAW = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CITY_RAW = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISTRICT_RAW = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AREA_SQM = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    NEIGHBORHOOD_RAW = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BUILDING_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FLOOR_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UNIT_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SITE_RAW = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MOBILE_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VALIDATION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_IMPORT_STAGINGS", x => x.STAGING_ID);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_STAGINGS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_IMPORT_STAGINGS_MFF_IMPORT_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "MFF_IMPORT_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LEGAL_REVIEWS",
                columns: table => new
                {
                    LEGAL_REVIEW_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROPERTY_ID = table.Column<long>(type: "bigint", nullable: true),
                    REVIEWER_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RESULT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_REGISTRATION_FEASIBLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MORTGAGE_FEASIBLE = table.Column<bool>(type: "boolean", nullable: false),
                    ISSUES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LEGAL_REVIEWS", x => x.LEGAL_REVIEW_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LEGAL_REVIEWS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LEGAL_REVIEWS_MFF_PROPERTIES",
                        column: x => x.PROPERTY_ID,
                        principalTable: "MFF_PROPERTIES",
                        principalColumn: "PROPERTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_REVIEW_CHECKS",
                columns: table => new
                {
                    CHECK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHECK_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHECK_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RESULT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CHECKED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CHECKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_REVIEW_CHECKS", x => x.CHECK_ID);
                    table.ForeignKey(
                        name: "FK_MFF_REVIEW_CHECKS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_SUBSIDY_APPROVALS",
                columns: table => new
                {
                    SUBSIDY_APPROVAL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    SUPPORT_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    APPROVAL_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    INSTALLMENT_SUPPORT_NPV = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DOWNPAYMENT_SUPPORT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_SUPPORT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_DEDICATED_ACCOUNT = table.Column<bool>(type: "boolean", nullable: false),
                    APPROVAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FUND_NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_SUBSIDY_APPROVALS", x => x.SUBSIDY_APPROVAL_ID);
                    table.ForeignKey(
                        name: "FK_MFF_SUBSIDY_APPROVALS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_SUBSIDY_APPROVALS_MFF_LKP_SUPPORT_TYPES",
                        column: x => x.SUPPORT_TYPE_CODE,
                        principalTable: "MFF_LKP_SUPPORT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_WORK_ASSIGNMENTS",
                columns: table => new
                {
                    ASSIGNMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    ASSIGNMENT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ASSIGNED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ASSIGNED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ASSIGNMENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_WORK_ASSIGNMENTS", x => x.ASSIGNMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_WORK_ASSIGNMENTS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_CALCULATION_SCHEDULES",
                columns: table => new
                {
                    CALC_SCHEDULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CALC_RUN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    DUE_MONTH_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    OPENING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BORROWER_PAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SUBSIDY_PAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BULLET_PAYMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CLOSING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_CALCULATION_SCHEDULES", x => x.CALC_SCHEDULE_ID);
                    table.ForeignKey(
                        name: "FK_MFF_CALCULATION_SCHEDULES_MFF_CALCULATION_RUNS",
                        column: x => x.CALC_RUN_ID,
                        principalTable: "MFF_CALCULATION_RUNS",
                        principalColumn: "CALC_RUN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LOANS",
                columns: table => new
                {
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONTRACT_ID = table.Column<long>(type: "bigint", nullable: false),
                    LOAN_ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DISBURSEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ANNUAL_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    TERM_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    INITIAL_INSTALLMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSTALLMENT_FREQUENCY = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LOAN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OUTSTANDING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NEXT_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DELINQUENT_DAYS = table.Column<int>(type: "integer", nullable: false),
                    CORE_SYSTEM_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LOANS", x => x.LOAN_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LOANS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LOANS_MFF_CONTRACTS",
                        column: x => x.CONTRACT_ID,
                        principalTable: "MFF_CONTRACTS",
                        principalColumn: "CONTRACT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_DELINQUENCIES",
                columns: table => new
                {
                    DELINQUENCY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STAGE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DAYS_PAST_DUE = table.Column<int>(type: "integer", nullable: false),
                    OVERDUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OVERDUE_INSTALLMENTS = table.Column<int>(type: "integer", nullable: false),
                    DELINQUENCY_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COLLECTION_ACTION_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_DELINQUENCIES", x => x.DELINQUENCY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_DELINQUENCIES_MFF_LKP_DELINQUENCY_STAGES",
                        column: x => x.STAGE_CODE,
                        principalTable: "MFF_LKP_DELINQUENCY_STAGES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_DELINQUENCIES_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_EARLY_SETTLEMENTS",
                columns: table => new
                {
                    SETTLEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUEST_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    SETTLEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SETTLEMENT_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PRINCIPAL_SETTLED = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FINANCE_PCT_SETTLED = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    SUBSIDY_REFUND_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SUBSIDY_REFUND_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EARLY_SETTLEMENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NOTES_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_EARLY_SETTLEMENTS", x => x.SETTLEMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_EARLY_SETTLEMENTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_INSTALLMENTS",
                columns: table => new
                {
                    INSTALLMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    OPENING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PRINCIPAL_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INTEREST_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSURANCE_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEES_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SUBSIDY_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BORROWER_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_DUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PAYMENT_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PAID_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CLOSING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_INSTALLMENTS", x => x.INSTALLMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_INSTALLMENTS_MFF_LKP_PAYMENT_STATUSES",
                        column: x => x.PAYMENT_STATUS_CODE,
                        principalTable: "MFF_LKP_PAYMENT_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_INSTALLMENTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_INSURANCE_POLICIES",
                columns: table => new
                {
                    POLICY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSURER_ORG_ID = table.Column<long>(type: "bigint", nullable: false),
                    POLICY_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    POLICY_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PREMIUM_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSURANCE_POLICY_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_INSURANCE_POLICIES", x => x.POLICY_ID);
                    table.ForeignKey(
                        name: "FK_MFF_INSURANCE_POLICIES_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_INSURANCE_POLICIES_MFF_ORGANIZATIONS",
                        column: x => x.INSURER_ORG_ID,
                        principalTable: "MFF_ORGANIZATIONS",
                        principalColumn: "ORG_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LEGAL_ACTIONS",
                columns: table => new
                {
                    LEGAL_ACTION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACTION_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACTION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CASE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LEGAL_ACTION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OUTCOME_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NEXT_ACTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LEGAL_ACTIONS", x => x.LEGAL_ACTION_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LEGAL_ACTIONS_MFF_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "MFF_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LEGAL_ACTIONS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LOAN_EVENTS",
                columns: table => new
                {
                    LOAN_EVENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    REPORTED_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    LOAN_EVENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DETAILS_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RESOLUTION_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LOAN_EVENTS", x => x.LOAN_EVENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LOAN_EVENTS_MFF_LKP_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_CODE,
                        principalTable: "MFF_LKP_EVENT_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_LOAN_EVENTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LOAN_MONTHLY_SNAPSHOTS",
                columns: table => new
                {
                    SNAPSHOT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SNAPSHOT_MONTH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LOAN_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OUTSTANDING_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OVERDUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DAYS_PAST_DUE = table.Column<int>(type: "integer", nullable: false),
                    SUBSIDY_BALANCE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LAST_PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LOAN_MONTHLY_SNAPSHOTS", x => x.SNAPSHOT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LOAN_MONTHLY_SNAPSHOTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_LOAN_RESTRUCTURINGS",
                columns: table => new
                {
                    RESTRUCTURING_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUEST_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DECISION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REASON_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OLD_TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    NEW_TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    OLD_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    NEW_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    OLD_INSTALLMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NEW_INSTALLMENT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CAPITALIZED_ARREARS = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LOAN_RESTRUCTURING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NOTES_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_LOAN_RESTRUCTURINGS", x => x.RESTRUCTURING_ID);
                    table.ForeignKey(
                        name: "FK_MFF_LOAN_RESTRUCTURINGS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_SUBSIDY_ACCOUNTS",
                columns: table => new
                {
                    SUBSIDY_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OPEN_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    OPENING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENT_BALANCE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CORE_SYSTEM_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_SUBSIDY_ACCOUNTS", x => x.SUBSIDY_ACCOUNT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_SUBSIDY_ACCOUNTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_PAYMENTS",
                columns: table => new
                {
                    PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAYMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PAYMENT_CHANNEL = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ALLOCATED_PRINCIPAL = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ALLOCATED_INTEREST = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ALLOCATED_FEES = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ALLOCATED_INSURANCE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_PAYMENTS", x => x.PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_PAYMENTS_MFF_INSTALLMENTS",
                        column: x => x.INSTALLMENT_ID,
                        principalTable: "MFF_INSTALLMENTS",
                        principalColumn: "INSTALLMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PAYMENTS_MFF_LKP_PAYMENT_STATUSES",
                        column: x => x.STATUS_CODE,
                        principalTable: "MFF_LKP_PAYMENT_STATUSES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_PAYMENTS_MFF_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "MFF_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MFF_SUBSIDY_MOVEMENTS",
                columns: table => new
                {
                    MOVEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SUBSIDY_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    MOVEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MOVEMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INSTALLMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DESCRIPTION_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFF_SUBSIDY_MOVEMENTS", x => x.MOVEMENT_ID);
                    table.ForeignKey(
                        name: "FK_MFF_SUBSIDY_MOVEMENTS_MFF_INSTALLMENTS",
                        column: x => x.INSTALLMENT_ID,
                        principalTable: "MFF_INSTALLMENTS",
                        principalColumn: "INSTALLMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MFF_SUBSIDY_MOVEMENTS_MFF_SUBSIDY_ACCOUNTS",
                        column: x => x.SUBSIDY_ACCOUNT_ID,
                        principalTable: "MFF_SUBSIDY_ACCOUNTS",
                        principalColumn: "SUBSIDY_ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MFF_ACCOUNTING_ENTRIES_APPLICATION_ID",
                table: "MFF_ACCOUNTING_ENTRIES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_ACCOUNTING_ENTRIES_EVENT_CODE",
                table: "MFF_ACCOUNTING_ENTRIES",
                column: "EVENT_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_ACCOUNTING_ENTRIES_LOAN_ID",
                table: "MFF_ACCOUNTING_ENTRIES",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPEALS_APPLICATION_ID",
                table: "MFF_APPEALS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPEALS_DECISION_CODE",
                table: "MFF_APPEALS",
                column: "DECISION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPEALS_REJECTION_DECISION_ID",
                table: "MFF_APPEALS",
                column: "REJECTION_DECISION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_DOCUMENTS_APPLICATION_ID",
                table: "MFF_APPLICATION_DOCUMENTS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_DOCUMENTS_DOCUMENT_TYPE_CODE",
                table: "MFF_APPLICATION_DOCUMENTS",
                column: "DOCUMENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_DOCUMENTS_PERSON_ID",
                table: "MFF_APPLICATION_DOCUMENTS",
                column: "PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_PARTIES_APPLICATION_ID",
                table: "MFF_APPLICATION_PARTIES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_PARTIES_PARTY_ROLE_CODE",
                table: "MFF_APPLICATION_PARTIES",
                column: "PARTY_ROLE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_PARTIES_PERSON_ID",
                table: "MFF_APPLICATION_PARTIES",
                column: "PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_APPLICATION_PARTIES_APPLICATION_ID_PERSON_ID",
                table: "MFF_APPLICATION_PARTIES",
                columns: new[] { "APPLICATION_ID", "PERSON_ID", "PARTY_ROLE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_PROPERTYS_APPLICATION_ID",
                table: "MFF_APPLICATION_PROPERTYS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_PROPERTYS_PROPERTY_ID",
                table: "MFF_APPLICATION_PROPERTYS",
                column: "PROPERTY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_APPLICATION_PROPERTYS_APPLICATION_ID_PROPERTY_ID",
                table: "MFF_APPLICATION_PROPERTYS",
                columns: new[] { "APPLICATION_ID", "PROPERTY_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_STATUS_HIST_APPLICATION_ID",
                table: "MFF_APPLICATION_STATUS_HIST",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_STATUS_HIST_NEW_STATUS_CODE",
                table: "MFF_APPLICATION_STATUS_HIST",
                column: "NEW_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_STATUS_HIST_OLD_STATUS_CODE",
                table: "MFF_APPLICATION_STATUS_HIST",
                column: "OLD_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATION_STATUS_HIST_ROLE_CODE",
                table: "MFF_APPLICATION_STATUS_HIST",
                column: "ROLE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_FINANCIER_ID",
                table: "MFF_APPLICATIONS",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_FUND_ORG_ID",
                table: "MFF_APPLICATIONS",
                column: "FUND_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_IMPORT_STAGING_ID",
                table: "MFF_APPLICATIONS",
                column: "IMPORT_STAGING_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_PRIMARY_PERSON_ID",
                table: "MFF_APPLICATIONS",
                column: "PRIMARY_PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_PROGRAM_VERSION_ID",
                table: "MFF_APPLICATIONS",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_SOURCE_CHANNEL_CODE",
                table: "MFF_APPLICATIONS",
                column: "SOURCE_CHANNEL_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPLICATIONS_STATUS_CODE",
                table: "MFF_APPLICATIONS",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_APPLICATIONS_APPLICATION_NO",
                table: "MFF_APPLICATIONS",
                column: "APPLICATION_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPRAISALS_APPLICATION_ID",
                table: "MFF_APPRAISALS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPRAISALS_APPRAISER_ORG_ID",
                table: "MFF_APPRAISALS",
                column: "APPRAISER_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPRAISALS_PROPERTY_ID",
                table: "MFF_APPRAISALS",
                column: "PROPERTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_APPRAISALS_REPORT_DOCUMENT_ID",
                table: "MFF_APPRAISALS",
                column: "REPORT_DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CALCULATION_RUNS_APPLICATION_ID",
                table: "MFF_CALCULATION_RUNS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CALCULATION_RUNS_FINANCIER_ID",
                table: "MFF_CALCULATION_RUNS",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CALCULATION_RUNS_PROGRAM_VERSION_ID",
                table: "MFF_CALCULATION_RUNS",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CALCULATION_SCHEDULES_CALC_RUN_ID",
                table: "MFF_CALCULATION_SCHEDULES",
                column: "CALC_RUN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_CALCULATION_SCHEDULES_CALC_RUN_ID_INSTALLMENT_NO",
                table: "MFF_CALCULATION_SCHEDULES",
                columns: new[] { "CALC_RUN_ID", "INSTALLMENT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_APPLICATION_ID",
                table: "MFF_CONTRACTS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_BUYER_PERSON_ID",
                table: "MFF_CONTRACTS",
                column: "BUYER_PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_DOCUMENT_ID",
                table: "MFF_CONTRACTS",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_FINANCIER_ID",
                table: "MFF_CONTRACTS",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_PROPERTY_ID",
                table: "MFF_CONTRACTS",
                column: "PROPERTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CONTRACTS_SELLER_PERSON_ID",
                table: "MFF_CONTRACTS",
                column: "SELLER_PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_CONTRACTS_CONTRACT_NO",
                table: "MFF_CONTRACTS",
                column: "CONTRACT_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_DECISIONS_APPLICATION_ID",
                table: "MFF_CREDIT_DECISIONS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_DECISIONS_DECISION_CODE",
                table: "MFF_CREDIT_DECISIONS",
                column: "DECISION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_INQUIRIES_APPLICATION_ID",
                table: "MFF_CREDIT_INQUIRIES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_INQUIRIES_BUREAU_ORG_ID",
                table: "MFF_CREDIT_INQUIRIES",
                column: "BUREAU_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_INQUIRIES_REPORT_DOCUMENT_ID",
                table: "MFF_CREDIT_INQUIRIES",
                column: "REPORT_DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_STUDIES_APPLICATION_ID",
                table: "MFF_CREDIT_STUDIES",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_CREDIT_STUDIES_RECOMMENDATION_CODE",
                table: "MFF_CREDIT_STUDIES",
                column: "RECOMMENDATION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_DELINQUENCIES_LOAN_ID",
                table: "MFF_DELINQUENCIES",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_DELINQUENCIES_STAGE_CODE",
                table: "MFF_DELINQUENCIES",
                column: "STAGE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_EARLY_SETTLEMENTS_LOAN_ID",
                table: "MFF_EARLY_SETTLEMENTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_FINANCE_OFFERS_APPLICATION_ID",
                table: "MFF_FINANCE_OFFERS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_FINANCE_OFFERS_OFFER_NO",
                table: "MFF_FINANCE_OFFERS",
                column: "OFFER_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_GL_ACCOUNT_MAPS_EVENT_CODE",
                table: "MFF_GL_ACCOUNT_MAPS",
                column: "EVENT_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_GL_ACCOUNT_MAPS_FINANCIER_ID",
                table: "MFF_GL_ACCOUNT_MAPS",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_GL_ACCOUNT_MAPS_FINANCIER_ID_EVENT_CODE",
                table: "MFF_GL_ACCOUNT_MAPS",
                columns: new[] { "FINANCIER_ID", "EVENT_CODE", "EFFECTIVE_FROM_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_HOUSING_ANNOUNCEMENTS_ANNOUNCEMENT_CODE",
                table: "MFF_HOUSING_ANNOUNCEMENTS",
                column: "ANNOUNCEMENT_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_HOUSING_PROJECTS_ANNOUNCEMENT_ID",
                table: "MFF_HOUSING_PROJECTS",
                column: "ANNOUNCEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_HOUSING_PROJECTS_CITY_CODE",
                table: "MFF_HOUSING_PROJECTS",
                column: "CITY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_HOUSING_PROJECTS_GOVERNORATE_CODE",
                table: "MFF_HOUSING_PROJECTS",
                column: "GOVERNORATE_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_HOUSING_PROJECTS_PROJECT_CODE",
                table: "MFF_HOUSING_PROJECTS",
                column: "PROJECT_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_BATCHES_FINANCIER_ID",
                table: "MFF_IMPORT_BATCHES",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_BATCHES_FUND_ORG_ID",
                table: "MFF_IMPORT_BATCHES",
                column: "FUND_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_BATCHES_PROGRAM_VERSION_ID",
                table: "MFF_IMPORT_BATCHES",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_BATCHES_SOURCE_CHANNEL_CODE",
                table: "MFF_IMPORT_BATCHES",
                column: "SOURCE_CHANNEL_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_STAGINGS_APPLICATION_ID",
                table: "MFF_IMPORT_STAGINGS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_IMPORT_STAGINGS_BATCH_ID",
                table: "MFF_IMPORT_STAGINGS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_IMPORT_STAGINGS_BATCH_ID_SOURCE_ROW_NO",
                table: "MFF_IMPORT_STAGINGS",
                columns: new[] { "BATCH_ID", "SOURCE_ROW_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INCOME_SOURCES_APPLICATION_PARTY_ID",
                table: "MFF_INCOME_SOURCES",
                column: "APPLICATION_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INCOME_SOURCES_INCOME_TYPE_CODE",
                table: "MFF_INCOME_SOURCES",
                column: "INCOME_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INSTALLMENTS_LOAN_ID",
                table: "MFF_INSTALLMENTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INSTALLMENTS_PAYMENT_STATUS_CODE",
                table: "MFF_INSTALLMENTS",
                column: "PAYMENT_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_INSTALLMENTS_LOAN_ID_INSTALLMENT_NO",
                table: "MFF_INSTALLMENTS",
                columns: new[] { "LOAN_ID", "INSTALLMENT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INSURANCE_POLICIES_INSURER_ORG_ID",
                table: "MFF_INSURANCE_POLICIES",
                column: "INSURER_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_INSURANCE_POLICIES_LOAN_ID",
                table: "MFF_INSURANCE_POLICIES",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_INSURANCE_POLICIES_POLICY_NO",
                table: "MFF_INSURANCE_POLICIES",
                column: "POLICY_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LEGAL_ACTIONS_APPLICATION_ID",
                table: "MFF_LEGAL_ACTIONS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LEGAL_ACTIONS_LOAN_ID",
                table: "MFF_LEGAL_ACTIONS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LEGAL_REVIEWS_APPLICATION_ID",
                table: "MFF_LEGAL_REVIEWS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LEGAL_REVIEWS_PROPERTY_ID",
                table: "MFF_LEGAL_REVIEWS",
                column: "PROPERTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LKP_CITYS_GOVERNORATE_CODE",
                table: "MFF_LKP_CITYS",
                column: "GOVERNORATE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LKP_STATUS_TRANSITIONS_FROM_STATUS_CODE",
                table: "MFF_LKP_STATUS_TRANSITIONS",
                column: "FROM_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LKP_STATUS_TRANSITIONS_ROLE_CODE",
                table: "MFF_LKP_STATUS_TRANSITIONS",
                column: "ROLE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LKP_STATUS_TRANSITIONS_TO_STATUS_CODE",
                table: "MFF_LKP_STATUS_TRANSITIONS",
                column: "TO_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_LKP_STATUS_TRANSITIONS_FROM_STATUS_CODE_TO_STATUS_CODE",
                table: "MFF_LKP_STATUS_TRANSITIONS",
                columns: new[] { "FROM_STATUS_CODE", "TO_STATUS_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOAN_EVENTS_EVENT_TYPE_CODE",
                table: "MFF_LOAN_EVENTS",
                column: "EVENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOAN_EVENTS_LOAN_ID",
                table: "MFF_LOAN_EVENTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOAN_MONTHLY_SNAPSHOTS_LOAN_ID",
                table: "MFF_LOAN_MONTHLY_SNAPSHOTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_LOAN_MONTHLY_SNAPSHOTS_SNAPSHOT_MONTH_DATE_LOAN_ID",
                table: "MFF_LOAN_MONTHLY_SNAPSHOTS",
                columns: new[] { "SNAPSHOT_MONTH_DATE", "LOAN_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOAN_RESTRUCTURINGS_LOAN_ID",
                table: "MFF_LOAN_RESTRUCTURINGS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOANS_APPLICATION_ID",
                table: "MFF_LOANS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_LOANS_CONTRACT_ID",
                table: "MFF_LOANS",
                column: "CONTRACT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_LOANS_LOAN_ACCOUNT_NO",
                table: "MFF_LOANS",
                column: "LOAN_ACCOUNT_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_ORGANIZATIONS_ORG_TYPE_CODE",
                table: "MFF_ORGANIZATIONS",
                column: "ORG_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_ORGANIZATIONS_REGULATOR_CODE",
                table: "MFF_ORGANIZATIONS",
                column: "REGULATOR_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_ORGANIZATIONS_ORG_CODE",
                table: "MFF_ORGANIZATIONS",
                column: "ORG_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PAYMENTS_INSTALLMENT_ID",
                table: "MFF_PAYMENTS",
                column: "INSTALLMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PAYMENTS_LOAN_ID",
                table: "MFF_PAYMENTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PAYMENTS_STATUS_CODE",
                table: "MFF_PAYMENTS",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PERSONS_EMPLOYMENT_TYPE_CODE",
                table: "MFF_PERSONS",
                column: "EMPLOYMENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PERSONS_MARITAL_STATUS_CODE",
                table: "MFF_PERSONS",
                column: "MARITAL_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PERSONS_NATIONAL_ID",
                table: "MFF_PERSONS",
                column: "NATIONAL_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_DOC_REQUIREMENTS_DOCUMENT_TYPE_CODE",
                table: "MFF_PROGRAM_DOC_REQUIREMENTS",
                column: "DOCUMENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_DOC_REQUIREMENTS_EMPLOYMENT_TYPE_CODE",
                table: "MFF_PROGRAM_DOC_REQUIREMENTS",
                column: "EMPLOYMENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_DOC_REQUIREMENTS_PARTY_ROLE_CODE",
                table: "MFF_PROGRAM_DOC_REQUIREMENTS",
                column: "PARTY_ROLE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_DOC_REQUIREMENTS_PROGRAM_VERSION_ID",
                table: "MFF_PROGRAM_DOC_REQUIREMENTS",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PROGRAM_DOC_REQUIREMENTS_PROGRAM_VERSION_ID",
                table: "MFF_PROGRAM_DOC_REQUIREMENTS",
                columns: new[] { "PROGRAM_VERSION_ID", "DOCUMENT_TYPE_CODE", "PARTY_ROLE_CODE", "EMPLOYMENT_TYPE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_PARAM_HIST_FINANCIER_ORG_ID",
                table: "MFF_PROGRAM_PARAM_HIST",
                column: "FINANCIER_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_PARAM_HIST_PARAM_ID",
                table: "MFF_PROGRAM_PARAM_HIST",
                column: "PARAM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_PARAM_HIST_PROGRAM_VERSION_ID",
                table: "MFF_PROGRAM_PARAM_HIST",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_PARAMS_FINANCIER_ID",
                table: "MFF_PROGRAM_PARAMS",
                column: "FINANCIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_PARAMS_PROGRAM_VERSION_ID",
                table: "MFF_PROGRAM_PARAMS",
                column: "PROGRAM_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PROGRAM_PARAMS_PROGRAM_VERSION_ID_FINANCIER_ID",
                table: "MFF_PROGRAM_PARAMS",
                columns: new[] { "PROGRAM_VERSION_ID", "FINANCIER_ID", "PARAM_CODE", "EFFECTIVE_FROM_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROGRAM_VERSIONS_PROTOCOL_ID",
                table: "MFF_PROGRAM_VERSIONS",
                column: "PROTOCOL_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PROGRAM_VERSIONS_PROGRAM_CODE_VERSION_NO",
                table: "MFF_PROGRAM_VERSIONS",
                columns: new[] { "PROGRAM_CODE", "VERSION_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROPERTIES_CITY_CODE",
                table: "MFF_PROPERTIES",
                column: "CITY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROPERTIES_GOVERNORATE_CODE",
                table: "MFF_PROPERTIES",
                column: "GOVERNORATE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROPERTIES_PROJECT_ID",
                table: "MFF_PROPERTIES",
                column: "PROJECT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROPERTIES_REGISTRATION_STATUS_CODE",
                table: "MFF_PROPERTIES",
                column: "REGISTRATION_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROPERTIES_SELLER_PERSON_ID",
                table: "MFF_PROPERTIES",
                column: "SELLER_PERSON_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROTOCOL_PARTIES_ORG_ID",
                table: "MFF_PROTOCOL_PARTIES",
                column: "ORG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROTOCOL_PARTIES_PROTOCOL_ID",
                table: "MFF_PROTOCOL_PARTIES",
                column: "PROTOCOL_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PROTOCOL_PARTIES_PROTOCOL_ID_ORG_ID",
                table: "MFF_PROTOCOL_PARTIES",
                columns: new[] { "PROTOCOL_ID", "ORG_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_PROTOCOLS_FUND_ORG_ID",
                table: "MFF_PROTOCOLS",
                column: "FUND_ORG_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_PROTOCOLS_PROTOCOL_CODE",
                table: "MFF_PROTOCOLS",
                column: "PROTOCOL_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_REVIEW_CHECKS_APPLICATION_ID",
                table: "MFF_REVIEW_CHECKS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_SUBSIDY_ACCOUNTS_LOAN_ID",
                table: "MFF_SUBSIDY_ACCOUNTS",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MFF_SUBSIDY_ACCOUNTS_ACCOUNT_NO",
                table: "MFF_SUBSIDY_ACCOUNTS",
                column: "ACCOUNT_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MFF_SUBSIDY_APPROVALS_APPLICATION_ID",
                table: "MFF_SUBSIDY_APPROVALS",
                column: "APPLICATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_SUBSIDY_APPROVALS_SUPPORT_TYPE_CODE",
                table: "MFF_SUBSIDY_APPROVALS",
                column: "SUPPORT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_SUBSIDY_MOVEMENTS_INSTALLMENT_ID",
                table: "MFF_SUBSIDY_MOVEMENTS",
                column: "INSTALLMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_SUBSIDY_MOVEMENTS_SUBSIDY_ACCOUNT_ID",
                table: "MFF_SUBSIDY_MOVEMENTS",
                column: "SUBSIDY_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MFF_WORK_ASSIGNMENTS_APPLICATION_ID",
                table: "MFF_WORK_ASSIGNMENTS",
                column: "APPLICATION_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_ACCOUNTING_ENTRIES_MFF_APPLICATIONS",
                table: "MFF_ACCOUNTING_ENTRIES",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_ACCOUNTING_ENTRIES_MFF_LOANS",
                table: "MFF_ACCOUNTING_ENTRIES",
                column: "LOAN_ID",
                principalTable: "MFF_LOANS",
                principalColumn: "LOAN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPEALS_MFF_APPLICATIONS",
                table: "MFF_APPEALS",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPEALS_MFF_CREDIT_DECISIONS",
                table: "MFF_APPEALS",
                column: "REJECTION_DECISION_ID",
                principalTable: "MFF_CREDIT_DECISIONS",
                principalColumn: "DECISION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPLICATION_DOCUMENTS_MFF_APPLICATIONS",
                table: "MFF_APPLICATION_DOCUMENTS",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPLICATION_PARTIES_MFF_APPLICATIONS",
                table: "MFF_APPLICATION_PARTIES",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPLICATION_PROPERTYS_MFF_APPLICATIONS",
                table: "MFF_APPLICATION_PROPERTYS",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPLICATION_STATUS_HIST_MFF_APPLICATIONS",
                table: "MFF_APPLICATION_STATUS_HIST",
                column: "APPLICATION_ID",
                principalTable: "MFF_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MFF_APPLICATIONS_MFF_IMPORT_STAGINGS",
                table: "MFF_APPLICATIONS",
                column: "IMPORT_STAGING_ID",
                principalTable: "MFF_IMPORT_STAGINGS",
                principalColumn: "STAGING_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MFF_IMPORT_STAGINGS_MFF_APPLICATIONS",
                table: "MFF_IMPORT_STAGINGS");

            migrationBuilder.DropTable(
                name: "MFF_ACCOUNTING_ENTRIES");

            migrationBuilder.DropTable(
                name: "MFF_APPEALS");

            migrationBuilder.DropTable(
                name: "MFF_APPLICATION_PROPERTYS");

            migrationBuilder.DropTable(
                name: "MFF_APPLICATION_STATUS_HIST");

            migrationBuilder.DropTable(
                name: "MFF_APPRAISALS");

            migrationBuilder.DropTable(
                name: "MFF_AUDIT_LOGS");

            migrationBuilder.DropTable(
                name: "MFF_CALCULATION_SCHEDULES");

            migrationBuilder.DropTable(
                name: "MFF_CREDIT_INQUIRIES");

            migrationBuilder.DropTable(
                name: "MFF_CREDIT_STUDIES");

            migrationBuilder.DropTable(
                name: "MFF_DELINQUENCIES");

            migrationBuilder.DropTable(
                name: "MFF_EARLY_SETTLEMENTS");

            migrationBuilder.DropTable(
                name: "MFF_FINANCE_OFFERS");

            migrationBuilder.DropTable(
                name: "MFF_GL_ACCOUNT_MAPS");

            migrationBuilder.DropTable(
                name: "MFF_INCOME_SOURCES");

            migrationBuilder.DropTable(
                name: "MFF_INSURANCE_POLICIES");

            migrationBuilder.DropTable(
                name: "MFF_INTEGRATION_LOGS");

            migrationBuilder.DropTable(
                name: "MFF_LEGAL_ACTIONS");

            migrationBuilder.DropTable(
                name: "MFF_LEGAL_REVIEWS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_STATUS_TRANSITIONS");

            migrationBuilder.DropTable(
                name: "MFF_LOAN_EVENTS");

            migrationBuilder.DropTable(
                name: "MFF_LOAN_MONTHLY_SNAPSHOTS");

            migrationBuilder.DropTable(
                name: "MFF_LOAN_RESTRUCTURINGS");

            migrationBuilder.DropTable(
                name: "MFF_PAYMENTS");

            migrationBuilder.DropTable(
                name: "MFF_PROGRAM_DOC_REQUIREMENTS");

            migrationBuilder.DropTable(
                name: "MFF_PROGRAM_PARAM_HIST");

            migrationBuilder.DropTable(
                name: "MFF_PROTOCOL_PARTIES");

            migrationBuilder.DropTable(
                name: "MFF_REVIEW_CHECKS");

            migrationBuilder.DropTable(
                name: "MFF_SUBSIDY_APPROVALS");

            migrationBuilder.DropTable(
                name: "MFF_SUBSIDY_MOVEMENTS");

            migrationBuilder.DropTable(
                name: "MFF_WORK_ASSIGNMENTS");

            migrationBuilder.DropTable(
                name: "MFF_CREDIT_DECISIONS");

            migrationBuilder.DropTable(
                name: "MFF_CALCULATION_RUNS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_DELINQUENCY_STAGES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_ACCOUNT_EVENTS");

            migrationBuilder.DropTable(
                name: "MFF_APPLICATION_PARTIES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_INCOME_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_USER_ROLES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_EVENT_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_PROGRAM_PARAMS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_SUPPORT_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_INSTALLMENTS");

            migrationBuilder.DropTable(
                name: "MFF_SUBSIDY_ACCOUNTS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_DECISION_CODES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_PARTY_ROLES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropTable(
                name: "MFF_LOANS");

            migrationBuilder.DropTable(
                name: "MFF_CONTRACTS");

            migrationBuilder.DropTable(
                name: "MFF_APPLICATION_DOCUMENTS");

            migrationBuilder.DropTable(
                name: "MFF_PROPERTIES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_HOUSING_PROJECTS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_PROPERTY_REG_STATUSES");

            migrationBuilder.DropTable(
                name: "MFF_HOUSING_ANNOUNCEMENTS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_CITYS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_GOVERNORATES");

            migrationBuilder.DropTable(
                name: "MFF_APPLICATIONS");

            migrationBuilder.DropTable(
                name: "MFF_IMPORT_STAGINGS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_APPLICATION_STATUSES");

            migrationBuilder.DropTable(
                name: "MFF_PERSONS");

            migrationBuilder.DropTable(
                name: "MFF_IMPORT_BATCHES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_EMPLOYMENT_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_MARITAL_STATUSES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_SOURCE_CHANNELS");

            migrationBuilder.DropTable(
                name: "MFF_PROGRAM_VERSIONS");

            migrationBuilder.DropTable(
                name: "MFF_PROTOCOLS");

            migrationBuilder.DropTable(
                name: "MFF_ORGANIZATIONS");

            migrationBuilder.DropTable(
                name: "MFF_LKP_ORG_TYPES");

            migrationBuilder.DropTable(
                name: "MFF_LKP_REGULATORS");
        }
    }
}
