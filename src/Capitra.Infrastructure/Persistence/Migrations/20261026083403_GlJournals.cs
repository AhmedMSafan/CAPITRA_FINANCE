using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlJournals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_COMBINATIONS",
                columns: table => new
                {
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COA_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONCATENATED_ACCOUNT = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
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
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_ACCOUNTS",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GL_ACCOUNT_COMBINATIONS_GL_COA_STRUCTURES",
                        column: x => x.COA_ID,
                        principalTable: "GL_COA_STRUCTURES",
                        principalColumn: "COA_ID",
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
                    IS_MANUAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
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
                    IS_SYSTEM_SOURCE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MANUAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_APPROVAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REVERSAL_ALLOWED = table.Column<bool>(type: "boolean", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
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
                name: "GL_JOURNAL_BATCHES",
                columns: table => new
                {
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    BATCH_NO = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    BATCH_NAME = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CATEGORY_ID = table.Column<long>(type: "bigint", nullable: false),
                    BATCH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    SOURCE_BATCH_REF = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    CURRENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JOURNAL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TOTAL_DR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_CR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_REVERSAL = table.Column<bool>(type: "boolean", nullable: false),
                    REVERSED_HEADER_ID = table.Column<long>(type: "bigint", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    SOURCE_DOCUMENT_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SUBMITTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUBMITTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    APPROVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POSTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CANCELLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCEL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REJECT_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LAST_POST_ERROR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                        name: "FK_GL_JOURNAL_HEADERS_GL_JOURNAL_HEADERS",
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
                        name: "FK_GL_JOURNAL_HEADERS_GL_LEDGER_PERIODS",
                        column: x => x.LEDGER_PERIOD_ID,
                        principalTable: "GL_LEDGER_PERIODS",
                        principalColumn: "LEDGER_PERIOD_ID",
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
                    CURRENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PERIOD_DR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PERIOD_CR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                name: "GL_JOURNAL_HEADERS_HIST",
                columns: table => new
                {
                    JOURNAL_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OLD_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NEW_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACTION_REASON = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_JOURNAL_HEADERS_HIST", x => x.JOURNAL_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_GL_JOURNAL_HEADERS_HIST_GL_JOURNAL_HEADERS",
                        column: x => x.JOURNAL_HEADER_ID,
                        principalTable: "GL_JOURNAL_HEADERS",
                        principalColumn: "JOURNAL_HEADER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GL_JOURNAL_LINES",
                columns: table => new
                {
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENTERED_DR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ENTERED_CR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LINE_DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REFERENCE_NO = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "GL_POSTING_BALANCE_IMPACTS",
                columns: table => new
                {
                    BALANCE_IMPACT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOURNAL_HEADER_ID = table.Column<long>(type: "bigint", nullable: false),
                    JOURNAL_LINE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEDGER_PERIOD_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCOUNT_COMBINATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNTED_DR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNTED_CR = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BALANCE_BEFORE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BALANCE_AFTER = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                });

            migrationBuilder.InsertData(
                table: "GL_JOURNAL_CATEGORIES",
                columns: new[] { "CATEGORY_ID", "CATEGORY_CODE", "CATEGORY_NAME_AR", "CATEGORY_NAME_EN", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "IS_ACTIVE", "IS_MANUAL_ALLOWED", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { 1L, "STANDARD", "قيد عادي", "Standard", new DateTime(2026, 10, 26, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, true, true, 1, null, null },
                    { 2L, "ADJUSTMENT", "قيد تسوية", "Adjustment", new DateTime(2026, 10, 26, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, true, true, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "GL_JOURNAL_SOURCES",
                columns: new[] { "SOURCE_ID", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "IS_ACTIVE", "IS_APPROVAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_REVERSAL_ALLOWED", "IS_SYSTEM_SOURCE", "ROW_VERSION", "SOURCE_CODE", "SOURCE_NAME_AR", "SOURCE_NAME_EN", "UPDATED_AT", "UPDATED_BY" },
                values: new object[] { 1L, new DateTime(2026, 10, 26, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", "Journals entered on the journal entry page.", true, true, true, true, true, 1, "MANUAL", "يدوي", "Manual", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNT_BALANCES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_BALANCES_LAST_POSTED_HEADER_ID",
                table: "GL_ACCOUNT_BALANCES",
                column: "LAST_POSTED_HEADER_ID");

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
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_COA_SEGMENT_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                column: "COA_SEGMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_SEGMENT_NUM",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: new[] { "SEGMENT_NUM", "SEGMENT_VALUE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATION_SEGMENTS_SEGMENT_VALUE_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                column: "SEGMENT_VALUE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATION_SEGMENTS_ACCOUNT_COMBINATION_ID",
                table: "GL_ACCOUNT_COMBINATION_SEGMENTS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "SEGMENT_NUM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_COMBINATIONS_ACCOUNT_ID",
                table: "GL_ACCOUNT_COMBINATIONS",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_ACCOUNT_COMBINATIONS_COA_ID_CONCATENATED_ACCOUNT",
                table: "GL_ACCOUNT_COMBINATIONS",
                columns: new[] { "COA_ID", "CONCATENATED_ACCOUNT" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_CATEGORY_ID",
                table: "GL_JOURNAL_BATCHES",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_LEDGER_ID",
                table: "GL_JOURNAL_BATCHES",
                column: "LEDGER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_SOURCE_BATCH_REF",
                table: "GL_JOURNAL_BATCHES",
                column: "SOURCE_BATCH_REF");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_BATCHES_SOURCE_ID",
                table: "GL_JOURNAL_BATCHES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_BATCHES_BATCH_NO",
                table: "GL_JOURNAL_BATCHES",
                column: "BATCH_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_CATEGORIES_CATEGORY_CODE",
                table: "GL_JOURNAL_CATEGORIES",
                column: "CATEGORY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_BATCH_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "BATCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_CATEGORY_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_JOURNAL_STATUS",
                table: "GL_JOURNAL_HEADERS",
                column: "JOURNAL_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_LEDGER_ID_ACCOUNTING_DATE",
                table: "GL_JOURNAL_HEADERS",
                columns: new[] { "LEDGER_ID", "ACCOUNTING_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_LEDGER_PERIOD_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "LEDGER_PERIOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_REVERSED_HEADER_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "REVERSED_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_SOURCE_ID",
                table: "GL_JOURNAL_HEADERS",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_HEADERS_JOURNAL_NO",
                table: "GL_JOURNAL_HEADERS",
                column: "JOURNAL_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_HEADERS_HIST_JOURNAL_HEADER_ID",
                table: "GL_JOURNAL_HEADERS_HIST",
                column: "JOURNAL_HEADER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_JOURNAL_LINES_ACCOUNT_COMBINATION_ID",
                table: "GL_JOURNAL_LINES",
                column: "ACCOUNT_COMBINATION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_LINES_JOURNAL_HEADER_ID_LINE_NO",
                table: "GL_JOURNAL_LINES",
                columns: new[] { "JOURNAL_HEADER_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_GL_JOURNAL_SOURCES_SOURCE_CODE",
                table: "GL_JOURNAL_SOURCES",
                column: "SOURCE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_ACCOUNT_COMBINATION_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                columns: new[] { "ACCOUNT_COMBINATION_ID", "ACCOUNTING_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_GL_POSTING_BALANCE_IMPACTS_JOURNAL_HEADER_ID",
                table: "GL_POSTING_BALANCE_IMPACTS",
                column: "JOURNAL_HEADER_ID");

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

            // The seeded rows carry their own keys; move the identities past them so new rows do not collide.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('"GL_JOURNAL_SOURCES"', 'SOURCE_ID'), (SELECT MAX("SOURCE_ID") FROM "GL_JOURNAL_SOURCES"));""");
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('"GL_JOURNAL_CATEGORIES"', 'CATEGORY_ID'), (SELECT MAX("CATEGORY_ID") FROM "GL_JOURNAL_CATEGORIES"));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_BALANCES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATION_SEGMENTS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_HEADERS_HIST");

            migrationBuilder.DropTable(
                name: "GL_POSTING_BALANCE_IMPACTS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_LINES");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COMBINATIONS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_HEADERS");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_BATCHES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_CATEGORIES");

            migrationBuilder.DropTable(
                name: "GL_JOURNAL_SOURCES");
        }
    }
}
