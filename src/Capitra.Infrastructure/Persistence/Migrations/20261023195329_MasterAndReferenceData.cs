using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MasterAndReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_LKP_COUNTRY");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "SEC_USER",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_EN",
                table: "ADM_LKP_RISK_LEVEL",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_AR",
                table: "ADM_LKP_RISK_LEVEL",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "ADM_LKP_RISK_LEVEL",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_EN",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_AR",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "ADM_LKP_CURRENCY",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "CODE",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<int>(
                name: "DECIMAL_PLACES",
                table: "ADM_LKP_CURRENCY",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "ADM_LKP_CHEQUE_STATUS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_CHEQUE_STATUS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_DECISION_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_DECISION_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_DOCUMENT_GROUP",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_DOCUMENT_GROUP", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_GUARANTEE_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_GUARANTEE_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_INSURANCE_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_INSURANCE_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_PAYMENT_STATUS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_PAYMENT_STATUS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_DEVELOPER",
                columns: table => new
                {
                    DEVELOPER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVELOPER_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEGAL_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LEGAL_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    COMMERCIAL_REG_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TAX_REG_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    INCORPORATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ADDRESS_TEXT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EMAIL_ADDRESS = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    RISK_LEVEL_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_DEVELOPER", x => x.DEVELOPER_ID);
                    table.ForeignKey(
                        name: "FK_MTG_DEVELOPER_ADM_LKP_RISK_LEVEL",
                        column: x => x.RISK_LEVEL_CODE,
                        principalTable: "ADM_LKP_RISK_LEVEL",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_INVESTOR",
                columns: table => new
                {
                    INVESTOR_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INVESTOR_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FULL_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FULL_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    NATIONAL_ID_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BIRTH_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EMAIL_ADDRESS = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ADDRESS_TEXT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EMPLOYER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DECLARED_INCOME_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_INVESTOR", x => x.INVESTOR_ID);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_APPROVAL_STEP",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_APPROVAL_STEP", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_DD_CHECK_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_DD_CHECK_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_INQUIRY_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_INQUIRY_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_PORTFOLIO_STATUS",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_PORTFOLIO_STATUS", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_UNIT_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_UNIT_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_VALUATION_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_VALUATION_TYPE", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_DOCUMENT_TYPE",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DOCUMENT_GROUP_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DOCUMENT_CATEGORY = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ALLOWED_EXTENSIONS = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MAX_FILE_SIZE_MB = table.Column<int>(type: "integer", nullable: true),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_EXPIRY_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_DOCUMENT_TYPE", x => x.CODE);
                    table.ForeignKey(
                        name: "FK_ADM_LKP_DOCUMENT_TYPE_ADM_LKP_DOCUMENT_GROUP",
                        column: x => x.DOCUMENT_GROUP_CODE,
                        principalTable: "ADM_LKP_DOCUMENT_GROUP",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_DEVELOPER_SIGNATORY",
                columns: table => new
                {
                    DEVELOPER_SIGNATORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVELOPER_ID = table.Column<long>(type: "bigint", nullable: false),
                    SIGNATORY_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NATIONAL_ID_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JOB_TITLE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AUTHORIZATION_REFERENCE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    VALID_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    VALID_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_DEVELOPER_SIGNATORY", x => x.DEVELOPER_SIGNATORY_ID);
                    table.ForeignKey(
                        name: "FK_MTG_DEVELOPER_SIGNATORY_MTG_DEVELOPER",
                        column: x => x.DEVELOPER_ID,
                        principalTable: "MTG_DEVELOPER",
                        principalColumn: "DEVELOPER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PROJECT",
                columns: table => new
                {
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVELOPER_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROJECT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PROJECT_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PROJECT_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LOCATION_TEXT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LAND_AREA = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TITLE_DEED_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BUILDING_LICENSE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BUILDING_LICENSE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PROJECT_COMPLETION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PROJECT_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PROJECT", x => x.PROJECT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PROJECT_MTG_DEVELOPER",
                        column: x => x.DEVELOPER_ID,
                        principalTable: "MTG_DEVELOPER",
                        principalColumn: "DEVELOPER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PROJECT_UNIT",
                columns: table => new
                {
                    PROJECT_UNIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    UNIT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UNIT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BUILDING_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FLOOR_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UNIT_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AREA_SQM = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CONTRACT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MARKET_VALUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LICENSE_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PROJECT_UNIT", x => x.PROJECT_UNIT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PROJECT_UNIT_MTG_LKP_UNIT_TYPE",
                        column: x => x.UNIT_TYPE_CODE,
                        principalTable: "MTG_LKP_UNIT_TYPE",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PROJECT_UNIT_MTG_PROJECT",
                        column: x => x.PROJECT_ID,
                        principalTable: "MTG_PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_LKP_DOCUMENT_TYPE_DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPE",
                column: "DOCUMENT_GROUP_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_DEVELOPER_RISK_LEVEL_CODE",
                table: "MTG_DEVELOPER",
                column: "RISK_LEVEL_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_DEVELOPER_DEVELOPER_CODE",
                table: "MTG_DEVELOPER",
                column: "DEVELOPER_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_DEVELOPER_SIGNATORY_DEVELOPER_ID",
                table: "MTG_DEVELOPER_SIGNATORY",
                column: "DEVELOPER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_INVESTOR_INVESTOR_CODE",
                table: "MTG_INVESTOR",
                column: "INVESTOR_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PROJECT_DEVELOPER_ID",
                table: "MTG_PROJECT",
                column: "DEVELOPER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PROJECT_PROJECT_CODE",
                table: "MTG_PROJECT",
                column: "PROJECT_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PROJECT_UNIT_UNIT_TYPE_CODE",
                table: "MTG_PROJECT_UNIT",
                column: "UNIT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PROJECT_UNIT_PROJECT_ID_UNIT_CODE",
                table: "MTG_PROJECT_UNIT",
                columns: new[] { "PROJECT_ID", "UNIT_CODE" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_LKP_CHEQUE_STATUS");

            migrationBuilder.DropTable(
                name: "ADM_LKP_DECISION_TYPE");

            migrationBuilder.DropTable(
                name: "ADM_LKP_DOCUMENT_TYPE");

            migrationBuilder.DropTable(
                name: "ADM_LKP_GUARANTEE_TYPE");

            migrationBuilder.DropTable(
                name: "ADM_LKP_INSURANCE_TYPE");

            migrationBuilder.DropTable(
                name: "ADM_LKP_PAYMENT_STATUS");

            migrationBuilder.DropTable(
                name: "MTG_DEVELOPER_SIGNATORY");

            migrationBuilder.DropTable(
                name: "MTG_INVESTOR");

            migrationBuilder.DropTable(
                name: "MTG_LKP_APPROVAL_STEP");

            migrationBuilder.DropTable(
                name: "MTG_LKP_DD_CHECK_TYPE");

            migrationBuilder.DropTable(
                name: "MTG_LKP_INQUIRY_TYPE");

            migrationBuilder.DropTable(
                name: "MTG_LKP_PORTFOLIO_STATUS");

            migrationBuilder.DropTable(
                name: "MTG_LKP_VALUATION_TYPE");

            migrationBuilder.DropTable(
                name: "MTG_PROJECT_UNIT");

            migrationBuilder.DropTable(
                name: "ADM_LKP_DOCUMENT_GROUP");

            migrationBuilder.DropTable(
                name: "MTG_LKP_UNIT_TYPE");

            migrationBuilder.DropTable(
                name: "MTG_PROJECT");

            migrationBuilder.DropTable(
                name: "MTG_DEVELOPER");

            migrationBuilder.DropColumn(
                name: "DECIMAL_PLACES",
                table: "ADM_LKP_CURRENCY");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "SEC_USER",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "NAME_EN",
                table: "ADM_LKP_RISK_LEVEL",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_AR",
                table: "ADM_LKP_RISK_LEVEL",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "ADM_LKP_RISK_LEVEL",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "NAME_EN",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NAME_AR",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "ADM_LKP_CURRENCY",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "CODE",
                table: "ADM_LKP_CURRENCY",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.CreateTable(
                name: "ADM_LKP_COUNTRY",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_COUNTRY", x => x.CODE);
                });
        }
    }
}
