using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CollectionRepaymentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHQ_CHEQUE_STATUS_HIST",
                columns: table => new
                {
                    CHEQUE_STATUS_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    OLD_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NEW_STATUS_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STATUS_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    BANK_STATEMENT_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CHANGED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHQ_CHEQUE_STATUS_HIST", x => x.CHEQUE_STATUS_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_CHQ_CHEQUE_STATUS_HIST_CHQ_CHEQUES",
                        column: x => x.CHEQUE_ID,
                        principalTable: "CHQ_CHEQUES",
                        principalColumn: "CHEQUE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_REPAYMENT_REQUESTS",
                columns: table => new
                {
                    REPAYMENT_REQUEST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CHEQUES_COUNT = table.Column<int>(type: "integer", nullable: true),
                    CHEQUES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    REMAINING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    REPAYMENT_TRAN_NO = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BANK_TRANSFER = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    CREDIT_BALANCE = table.Column<decimal>(type: "numeric(28,2)", precision: 28, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_REPAYMENT_REQUESTS", x => x.REPAYMENT_REQUEST_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_REPAYMENT_TRANSACTIONS",
                columns: table => new
                {
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REPAYMENT_NUMBER = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    REPAYMENT_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TRANSACTION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    RECEIPT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ALLOCATED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SOURCE_GL_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    BANK_STATEMENT_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EXTERNAL_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCOUNTING_EVENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REVERSAL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REVERSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REVERSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_REPAYMENT_TRANSACTIONS", x => x.REPAYMENT_TXN_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_REPAYMENT_SOURCES",
                columns: table => new
                {
                    REPAYMENT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: false),
                    SOURCE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_GL_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SOURCE_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ALLOCATED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_REPAYMENT_SOURCES", x => x.REPAYMENT_SOURCE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_SOURCES_CHQ_CHEQUES",
                        column: x => x.CHEQUE_ID,
                        principalTable: "CHQ_CHEQUES",
                        principalColumn: "CHEQUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_RE_10F8E45A",
                        column: x => x.REPAYMENT_TXN_ID,
                        principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_SO_9FCB51DD",
                        column: x => x.SOURCE_REPAYMENT_TXN_ID,
                        principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_REPAYMENT_ALLOCATIONS",
                columns: table => new
                {
                    ALLOCATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPAYMENT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    TARGET_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TARGET_ID = table.Column<long>(type: "bigint", nullable: false),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    COMPONENT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ALLOCATION_MODE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    REQUESTED_VALUE = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: false),
                    ALLOCATION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ALLOCATION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REVERSAL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REVERSED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REVERSED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_REPAYMENT_ALLOCATIONS", x => x.ALLOCATION_ID);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_SOURCES",
                        column: x => x.REPAYMENT_SOURCE_ID,
                        principalTable: "FCT_REPAYMENT_SOURCES",
                        principalColumn: "REPAYMENT_SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_TRANSACTIONS",
                        column: x => x.REPAYMENT_TXN_ID,
                        principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_CHEQUE_STATUS_HIST_CHEQUE_ID_STATUS_DATE",
                table: "CHQ_CHEQUE_STATUS_HIST",
                columns: new[] { "CHEQUE_ID", "STATUS_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_ALLOCATIONS_DISBURSEMENT_ID",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_ALLOCATIONS_REPAYMENT_SOURCE_ID",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "REPAYMENT_SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_ALLOCATIONS_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                columns: new[] { "REPAYMENT_TXN_ID", "STATUS_CODE", "ALLOCATION_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_ALLOCATIONS_TARGET_TYPE_TARGET_ID",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                columns: new[] { "TARGET_TYPE", "TARGET_ID", "COMPONENT_CODE", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_CHEQUE_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                columns: new[] { "CHEQUE_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                columns: new[] { "REPAYMENT_TXN_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_SOURCE_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                columns: new[] { "SOURCE_REPAYMENT_TXN_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_REPAYMENT_TRANSACTIONS_CLIENT_ID_LEGAL_ENTITY_ID",
                table: "FCT_REPAYMENT_TRANSACTIONS",
                columns: new[] { "CLIENT_ID", "LEGAL_ENTITY_ID", "VALUE_DATE", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_REPAYMENT_TRANSACTIONS_REPAYMENT_NUMBER",
                table: "FCT_REPAYMENT_TRANSACTIONS",
                column: "REPAYMENT_NUMBER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHQ_CHEQUE_STATUS_HIST");

            migrationBuilder.DropTable(
                name: "FCT_REPAYMENT_ALLOCATIONS");

            migrationBuilder.DropTable(
                name: "FCT_REPAYMENT_REQUESTS");

            migrationBuilder.DropTable(
                name: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.DropTable(
                name: "FCT_REPAYMENT_TRANSACTIONS");
        }
    }
}
