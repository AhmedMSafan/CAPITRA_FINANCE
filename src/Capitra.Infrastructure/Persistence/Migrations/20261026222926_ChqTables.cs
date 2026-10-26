using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChqTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHQ_ACTION_CATALOGS",
                columns: table => new
                {
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACTION_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ACTION_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EVENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TARGET_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ALLOWED_FROM_STATUS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ALLOWED_FROM_LOCATION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_DEPOSIT_BATCH_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_BANK_REF_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REJECTION_REASON_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_NOTES_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    PACKAGE_PROCEDURE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_ACTION_CATALOGS", x => x.ACTION_CODE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_BANK_CHARGES",
                columns: table => new
                {
                    CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHARGE_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHARGE_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHARGE_DESCRIPTION_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CHARGE_DESCRIPTION_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CHARGE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CHARGED_BY_BANK = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CHARGE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IS_RECOVERED_FROM_CUSTOMER = table.Column<bool>(type: "boolean", nullable: false),
                    RECOVERY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RECOVERY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_DEBIT_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GL_CREDIT_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_POST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_BANK_CHARGES", x => x.CHARGE_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_BANK_STATEMENTS",
                columns: table => new
                {
                    STATEMENT_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STATEMENT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    STATEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TRANSACTION_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TRANSACTION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    BANK_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MATCHED_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    RECON_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RECON_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RECON_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECON_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CLOSING_BALANCE = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_BANK_STATEMENTS", x => x.STATEMENT_LINE_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_DEPOSIT_BATCHES",
                columns: table => new
                {
                    DEPOSIT_BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    DEPOSIT_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    BATCH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TOTAL_CHEQUES = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    COLLECTED_COUNT = table.Column<int>(type: "integer", nullable: true),
                    COLLECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    REJECTED_COUNT = table.Column<int>(type: "integer", nullable: true),
                    REJECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    WITHDRAWN_COUNT = table.Column<int>(type: "integer", nullable: true),
                    WITHDRAWN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PENDING_COUNT = table.Column<int>(type: "integer", nullable: true),
                    PENDING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BATCH_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLEARED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BANK_BATCH_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IS_BANK_CONFIRMED = table.Column<bool>(type: "boolean", nullable: false),
                    BANK_CONFIRMED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_POST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_DEPOSIT_BATCHES", x => x.DEPOSIT_BATCH_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_EVENT_GROUPS",
                columns: table => new
                {
                    MOVEMENT_GROUP_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EVENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EFFECTIVE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DEPOSIT_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    TO_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MOVEMENT_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_EVENT_GROUPS", x => x.MOVEMENT_GROUP_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_LKP_EVENT_TYPES",
                columns: table => new
                {
                    EVENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_DESC_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EVENT_DESC_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_LKP_EVENT_TYPES", x => x.EVENT_TYPE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_LKP_ISSUER_BANKS",
                columns: table => new
                {
                    BANK_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BANK_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BANK_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_LKP_ISSUER_BANKS", x => x.BANK_CODE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_LKP_LOCATIONS",
                columns: table => new
                {
                    LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LOCATION_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LOCATION_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_LKP_LOCATIONS", x => x.LOCATION_CODE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_LKP_REJECTION_REASONS",
                columns: table => new
                {
                    REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_LKP_REJECTION_REASONS", x => x.REASON_CODE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_LKP_STATUSES",
                columns: table => new
                {
                    STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IS_TERMINAL = table.Column<bool>(type: "boolean", nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_LKP_STATUSES", x => x.STATUS_CODE);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_REPLACEMENTS",
                columns: table => new
                {
                    REPLACEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REPLACEMENT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ORIGINAL_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    TRIGGER_EVENT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TRIGGER_EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SETTLEMENT_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SETTLEMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ORIGINAL_CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NEW_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    TRANSFER_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    TRANSFER_TXN_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TRANSFER_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    INTERNAL_FUND_SOURCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    INTERNAL_APPROVAL_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SETTLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REPLACEMENT_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PAYMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_POST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_REPLACEMENTS", x => x.REPLACEMENT_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_UPLOAD_BATCHES",
                columns: table => new
                {
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BATCH_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MIME_TYPE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CUSTOMER_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CUSTOMER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CUSTOMER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DRAWER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DRAWER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SOURCE_REF_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_REF_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CHEQUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RECEIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DEFAULT_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DEFAULT_CUSTODIAN_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TOTAL_ROWS = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    VALID_ROWS = table.Column<int>(type: "integer", nullable: true),
                    ERROR_ROWS = table.Column<int>(type: "integer", nullable: true),
                    SUBMITTED_ROWS = table.Column<int>(type: "integer", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_UPLOAD_BATCHES", x => x.BATCH_ID);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_CHEQUES",
                columns: table => new
                {
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHEQUE_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHEQUE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ISSUER_BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ISSUER_BANK_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ISSUER_BANK_BRANCH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ISSUER_ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DRAWER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DRAWER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DRAWER_NATIONAL_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CUSTOMER_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CUSTOMER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CUSTOMER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DRAWDOWN_ID = table.Column<long>(type: "bigint", nullable: true),
                    SCHEDULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEPOSIT_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SOURCE_MODULE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SOURCE_REF_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SOURCE_REF_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECEIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DEPOSIT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COLLECTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REJECTION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WITHDRAWAL_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    REJECTION_REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REJECTION_NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_REPLACED = table.Column<bool>(type: "boolean", nullable: false),
                    REPLACEMENT_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEPOSIT_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_POST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAYMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DRAWER_ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PARTY_TAX_ID = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CURRENT_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CURRENT_CUSTODIAN_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LAST_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    LAST_EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEPOSIT_BANK_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_CHEQUES", x => x.CHEQUE_ID);
                    table.ForeignKey(
                        name: "FK_CHQ_CHEQUES_CHQ_LKP_LOCATIONS",
                        column: x => x.CURRENT_LOCATION_CODE,
                        principalTable: "CHQ_LKP_LOCATIONS",
                        principalColumn: "LOCATION_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHQ_CHEQUES_CHQ_LKP_REJECTION_REASONS",
                        column: x => x.REJECTION_REASON_CODE,
                        principalTable: "CHQ_LKP_REJECTION_REASONS",
                        principalColumn: "REASON_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHQ_CHEQUES_CHQ_LKP_STATUSES",
                        column: x => x.STATUS_CODE,
                        principalTable: "CHQ_LKP_STATUSES",
                        principalColumn: "STATUS_CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_EVENTS",
                columns: table => new
                {
                    EVENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    DEPOSIT_BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    EVENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EFFECTIVE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EVENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    REJECTION_REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BANK_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NEW_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OLD_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GL_POST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GL_DEBIT_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GL_CREDIT_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MOVEMENT_GROUP_ID = table.Column<long>(type: "bigint", nullable: true),
                    FROM_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TO_LOCATION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FROM_CUSTODIAN_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TO_CUSTODIAN_USER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HANDOVER_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECEIVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECEIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MOVEMENT_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SOURCE_PAGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_APP_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_SESSION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_EVENTS", x => x.EVENT_ID);
                    table.ForeignKey(
                        name: "FK_CHQ_EVENTS_CHQ_DEPOSIT_BATCHES",
                        column: x => x.DEPOSIT_BATCH_ID,
                        principalTable: "CHQ_DEPOSIT_BATCHES",
                        principalColumn: "DEPOSIT_BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHQ_EVENTS_CHQ_LKP_REJECTION_REASONS",
                        column: x => x.REJECTION_REASON_CODE,
                        principalTable: "CHQ_LKP_REJECTION_REASONS",
                        principalColumn: "REASON_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_NEW_STATUS_CODE",
                        column: x => x.NEW_STATUS_CODE,
                        principalTable: "CHQ_LKP_STATUSES",
                        principalColumn: "STATUS_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_OLD_STATUS_CODE",
                        column: x => x.OLD_STATUS_CODE,
                        principalTable: "CHQ_LKP_STATUSES",
                        principalColumn: "STATUS_CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CHQ_UPLOAD_LINES",
                columns: table => new
                {
                    LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    RAW_SR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_CLIENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_PARTY_TAX_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_DUE_DATE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_CHEQUE_AMOUNT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RAW_ISSUER_BANK_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CLIENT_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PARTY_TAX_ID = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ISSUER_BANK_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ISSUER_BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LINE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TARGET_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    TARGET_CHEQUE_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_UPLOAD_LINES", x => x.LINE_ID);
                    table.ForeignKey(
                        name: "FK_CHQ_UPLOAD_LINES_CHQ_UPLOAD_BATCHES",
                        column: x => x.BATCH_ID,
                        principalTable: "CHQ_UPLOAD_BATCHES",
                        principalColumn: "BATCH_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_CHARGES_CHARGE_AT",
                table: "CHQ_BANK_CHARGES",
                column: "CHARGE_AT");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_CHARGES_CHEQUE_ID",
                table: "CHQ_BANK_CHARGES",
                column: "CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_CHARGES_IS_GL_POSTED",
                table: "CHQ_BANK_CHARGES",
                column: "IS_GL_POSTED");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_CHARGES_LEGAL_ENTITY_ID",
                table: "CHQ_BANK_CHARGES",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_BANK_CHARGES_CHARGE_REF",
                table: "CHQ_BANK_CHARGES",
                column: "CHARGE_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_STATEMENTS_BANK_ACCOUNT_ID",
                table: "CHQ_BANK_STATEMENTS",
                column: "BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_STATEMENTS_LEGAL_ENTITY_ID",
                table: "CHQ_BANK_STATEMENTS",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_STATEMENTS_MATCHED_CHEQUE_ID",
                table: "CHQ_BANK_STATEMENTS",
                column: "MATCHED_CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_BANK_STATEMENTS_RECON_STATUS",
                table: "CHQ_BANK_STATEMENTS",
                column: "RECON_STATUS");

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_BANK_STATEMENTS_STATEMENT_REF_BANK_ACCOUNT_ID",
                table: "CHQ_BANK_STATEMENTS",
                columns: new[] { "STATEMENT_REF", "BANK_ACCOUNT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_CURRENT_LOCATION_CODE",
                table: "CHQ_CHEQUES",
                column: "CURRENT_LOCATION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_CUSTOMER_ID",
                table: "CHQ_CHEQUES",
                column: "CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_DEPOSIT_BANK_ACCOUNT_ID",
                table: "CHQ_CHEQUES",
                column: "DEPOSIT_BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_DEPOSIT_BATCH_ID",
                table: "CHQ_CHEQUES",
                column: "DEPOSIT_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_DRAWDOWN_ID",
                table: "CHQ_CHEQUES",
                column: "DRAWDOWN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_DUE_DATE",
                table: "CHQ_CHEQUES",
                column: "DUE_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_ISSUER_BANK_CODE_CHEQUE_NO",
                table: "CHQ_CHEQUES",
                columns: new[] { "ISSUER_BANK_CODE", "CHEQUE_NO" });

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_LEGAL_ENTITY_ID",
                table: "CHQ_CHEQUES",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_REJECTION_REASON_CODE",
                table: "CHQ_CHEQUES",
                column: "REJECTION_REASON_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_SCHEDULE_ID",
                table: "CHQ_CHEQUES",
                column: "SCHEDULE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_SOURCE_MODULE_SOURCE_REF_ID",
                table: "CHQ_CHEQUES",
                columns: new[] { "SOURCE_MODULE", "SOURCE_REF_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUES_STATUS_CODE",
                table: "CHQ_CHEQUES",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_CHEQUES_CHEQUE_NO_ISSUER_BANK_CODE",
                table: "CHQ_CHEQUES",
                columns: new[] { "CHEQUE_NO", "ISSUER_BANK_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_CHEQUES_CHEQUE_REF",
                table: "CHQ_CHEQUES",
                column: "CHEQUE_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_DEPOSIT_BATCHES_BATCH_DATE",
                table: "CHQ_DEPOSIT_BATCHES",
                column: "BATCH_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_DEPOSIT_BATCHES_BATCH_STATUS",
                table: "CHQ_DEPOSIT_BATCHES",
                column: "BATCH_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_DEPOSIT_BATCHES_DEPOSIT_BANK_ACCOUNT_ID",
                table: "CHQ_DEPOSIT_BATCHES",
                column: "DEPOSIT_BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_DEPOSIT_BATCHES_LEGAL_ENTITY_ID",
                table: "CHQ_DEPOSIT_BATCHES",
                column: "LEGAL_ENTITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_DEPOSIT_BATCHES_BATCH_REF",
                table: "CHQ_DEPOSIT_BATCHES",
                column: "BATCH_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_CHEQUE_ID",
                table: "CHQ_EVENTS",
                column: "CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_DEPOSIT_BATCH_ID",
                table: "CHQ_EVENTS",
                column: "DEPOSIT_BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_EVENT_TYPE",
                table: "CHQ_EVENTS",
                column: "EVENT_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_IS_GL_POSTED",
                table: "CHQ_EVENTS",
                column: "IS_GL_POSTED");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_MOVEMENT_GROUP_ID",
                table: "CHQ_EVENTS",
                column: "MOVEMENT_GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_NEW_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "NEW_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_OLD_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "OLD_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_REJECTION_REASON_CODE",
                table: "CHQ_EVENTS",
                column: "REJECTION_REASON_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_TO_LOCATION_CODE",
                table: "CHQ_EVENTS",
                column: "TO_LOCATION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_REPLACEMENTS_NEW_CHEQUE_ID",
                table: "CHQ_REPLACEMENTS",
                column: "NEW_CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_REPLACEMENTS_ORIGINAL_CHEQUE_ID",
                table: "CHQ_REPLACEMENTS",
                column: "ORIGINAL_CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_REPLACEMENTS_REPLACEMENT_STATUS",
                table: "CHQ_REPLACEMENTS",
                column: "REPLACEMENT_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_REPLACEMENTS_SETTLED_AT",
                table: "CHQ_REPLACEMENTS",
                column: "SETTLED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_REPLACEMENTS_SETTLEMENT_METHOD",
                table: "CHQ_REPLACEMENTS",
                column: "SETTLEMENT_METHOD");

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_REPLACEMENTS_REPLACEMENT_REF",
                table: "CHQ_REPLACEMENTS",
                column: "REPLACEMENT_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_UPLOAD_BATCHES_BATCH_NO",
                table: "CHQ_UPLOAD_BATCHES",
                column: "BATCH_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_UPLOAD_LINES_BATCH_ID_LINE_STATUS",
                table: "CHQ_UPLOAD_LINES",
                columns: new[] { "BATCH_ID", "LINE_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_UPLOAD_LINES_CHEQUE_NO_ISSUER_BANK_CODE",
                table: "CHQ_UPLOAD_LINES",
                columns: new[] { "CHEQUE_NO", "ISSUER_BANK_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_CHQ_UPLOAD_LINES_BATCH_ID_LINE_NO",
                table: "CHQ_UPLOAD_LINES",
                columns: new[] { "BATCH_ID", "LINE_NO" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHQ_ACTION_CATALOGS");

            migrationBuilder.DropTable(
                name: "CHQ_BANK_CHARGES");

            migrationBuilder.DropTable(
                name: "CHQ_BANK_STATEMENTS");

            migrationBuilder.DropTable(
                name: "CHQ_CHEQUES");

            migrationBuilder.DropTable(
                name: "CHQ_EVENT_GROUPS");

            migrationBuilder.DropTable(
                name: "CHQ_EVENTS");

            migrationBuilder.DropTable(
                name: "CHQ_LKP_EVENT_TYPES");

            migrationBuilder.DropTable(
                name: "CHQ_LKP_ISSUER_BANKS");

            migrationBuilder.DropTable(
                name: "CHQ_REPLACEMENTS");

            migrationBuilder.DropTable(
                name: "CHQ_UPLOAD_LINES");

            migrationBuilder.DropTable(
                name: "CHQ_LKP_LOCATIONS");

            migrationBuilder.DropTable(
                name: "CHQ_DEPOSIT_BATCHES");

            migrationBuilder.DropTable(
                name: "CHQ_LKP_REJECTION_REASONS");

            migrationBuilder.DropTable(
                name: "CHQ_LKP_STATUSES");

            migrationBuilder.DropTable(
                name: "CHQ_UPLOAD_BATCHES");
        }
    }
}
