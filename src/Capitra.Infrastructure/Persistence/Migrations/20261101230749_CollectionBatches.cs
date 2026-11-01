using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CollectionBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FCT_COLLECTION_BATCHES",
                columns: table => new
                {
                    COLLECTION_BATCH_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BATCH_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    COLLECTED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    COLLECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BOUNCED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    BOUNCED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    REJECTED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    REJECTED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    REPAYMENTS_COUNT = table.Column<int>(type: "integer", nullable: false),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_COLLECTION_BATCHES", x => x.COLLECTION_BATCH_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_COLLECTION_BATCH_LINES",
                columns: table => new
                {
                    COLLECTION_BATCH_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COLLECTION_BATCH_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHEQUE_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CUSTOMER_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DEPOSIT_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    RESULT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    STATUS_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    BANK_STATEMENT_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    REASON_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_COLLECTION_BATCH_LINES", x => x.COLLECTION_BATCH_LINE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_COLLECTION_BATCHES",
                        column: x => x.COLLECTION_BATCH_ID,
                        principalTable: "FCT_COLLECTION_BATCHES",
                        principalColumn: "COLLECTION_BATCH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_REPAYMENT_TRANSACTIONS",
                        column: x => x.REPAYMENT_TXN_ID,
                        principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_COLLECTION_BATCH_LINES_CHEQUE_ID",
                table: "FCT_COLLECTION_BATCH_LINES",
                column: "CHEQUE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_COLLECTION_BATCH_LINES_REPAYMENT_TXN_ID",
                table: "FCT_COLLECTION_BATCH_LINES",
                column: "REPAYMENT_TXN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_COLLECTION_BATCH_LINES_COLLECTION_BATCH_ID_LINE_NO",
                table: "FCT_COLLECTION_BATCH_LINES",
                columns: new[] { "COLLECTION_BATCH_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_COLLECTION_BATCHES_GL_TRAN_SET_ID",
                table: "FCT_COLLECTION_BATCHES",
                column: "GL_TRAN_SET_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_COLLECTION_BATCHES_BATCH_NO",
                table: "FCT_COLLECTION_BATCHES",
                column: "BATCH_NO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_COLLECTION_BATCH_LINES");

            migrationBuilder.DropTable(
                name: "FCT_COLLECTION_BATCHES");
        }
    }
}
