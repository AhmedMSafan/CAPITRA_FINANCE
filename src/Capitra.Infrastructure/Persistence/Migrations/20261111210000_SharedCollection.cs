using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SharedCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_COLLECTION_BATCHES",
                table: "FCT_COLLECTION_BATCH_LINES");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_COLLECTION_BATCH_LINES");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_SOURCES",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_ALLOCATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_ALLOCATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_CHQ_CHEQUES",
                table: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_RE_10F8E45A",
                table: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_SO_9FCB51DD",
                table: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_TRANSACTIONS");

            migrationBuilder.RenameTable(
                name: "FCT_REPAYMENT_TRANSACTIONS",
                newName: "COL_REPAYMENT_TRANSACTIONS");

            migrationBuilder.RenameIndex(
                name: "UQ_FCT_REPAYMENT_TRANSACTIONS_REPAYMENT_NUMBER",
                table: "COL_REPAYMENT_TRANSACTIONS",
                newName: "UQ_COL_REPAYMENT_TRANSACTIONS_REPAYMENT_NUMBER");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_REPAYMENT_TRANSACTIONS_CLIENT_ID_LEGAL_ENTITY_ID",
                table: "COL_REPAYMENT_TRANSACTIONS",
                newName: "IX_COL_REPAYMENT_TRANSACTIONS_CLIENT_ID_LEGAL_ENTITY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COL_REPAYMENT_TRANSACTIONS",
                table: "COL_REPAYMENT_TRANSACTIONS",
                column: "REPAYMENT_TXN_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FCT_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.RenameTable(
                name: "FCT_REPAYMENT_SOURCES",
                newName: "COL_REPAYMENT_SOURCES");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_CHEQUE_ID_STATUS_CODE",
                table: "COL_REPAYMENT_SOURCES",
                newName: "IX_COL_REPAYMENT_SOURCES_CHEQUE_ID_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "COL_REPAYMENT_SOURCES",
                newName: "IX_COL_REPAYMENT_SOURCES_REPAYMENT_TXN_ID_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_REPAYMENT_SOURCES_SOURCE_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "COL_REPAYMENT_SOURCES",
                newName: "IX_COL_REPAYMENT_SOURCES_SOURCE_REPAYMENT_TXN_ID_STATUS_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COL_REPAYMENT_SOURCES",
                table: "COL_REPAYMENT_SOURCES",
                column: "REPAYMENT_SOURCE_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FCT_COLLECTION_BATCH_LINES",
                table: "FCT_COLLECTION_BATCH_LINES");

            migrationBuilder.RenameTable(
                name: "FCT_COLLECTION_BATCH_LINES",
                newName: "COL_COLLECTION_BATCH_LINES");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_COLLECTION_BATCH_LINES_CHEQUE_ID",
                table: "COL_COLLECTION_BATCH_LINES",
                newName: "IX_COL_COLLECTION_BATCH_LINES_CHEQUE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_COLLECTION_BATCH_LINES_REPAYMENT_TXN_ID",
                table: "COL_COLLECTION_BATCH_LINES",
                newName: "IX_COL_COLLECTION_BATCH_LINES_REPAYMENT_TXN_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_FCT_COLLECTION_BATCH_LINES_COLLECTION_BATCH_ID_LINE_NO",
                table: "COL_COLLECTION_BATCH_LINES",
                newName: "UQ_COL_COLLECTION_BATCH_LINES_COLLECTION_BATCH_ID_LINE_NO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COL_COLLECTION_BATCH_LINES",
                table: "COL_COLLECTION_BATCH_LINES",
                column: "COLLECTION_BATCH_LINE_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FCT_COLLECTION_BATCHES",
                table: "FCT_COLLECTION_BATCHES");

            migrationBuilder.RenameTable(
                name: "FCT_COLLECTION_BATCHES",
                newName: "COL_COLLECTION_BATCHES");

            migrationBuilder.RenameIndex(
                name: "UQ_FCT_COLLECTION_BATCHES_BATCH_NO",
                table: "COL_COLLECTION_BATCHES",
                newName: "UQ_COL_COLLECTION_BATCHES_BATCH_NO");

            migrationBuilder.RenameIndex(
                name: "IX_FCT_COLLECTION_BATCHES_GL_TRAN_SET_ID",
                table: "COL_COLLECTION_BATCHES",
                newName: "IX_COL_COLLECTION_BATCHES_GL_TRAN_SET_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COL_COLLECTION_BATCHES",
                table: "COL_COLLECTION_BATCHES",
                column: "COLLECTION_BATCH_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FCT_REPAYMENT_REQUESTS",
                table: "FCT_REPAYMENT_REQUESTS");

            migrationBuilder.RenameTable(
                name: "FCT_REPAYMENT_REQUESTS",
                newName: "COL_REPAYMENT_REQUESTS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COL_REPAYMENT_REQUESTS",
                table: "COL_REPAYMENT_REQUESTS",
                column: "REPAYMENT_REQUEST_ID");

            migrationBuilder.AddColumn<string>(
                name: "BUSINESS_LINE_CODE",
                table: "COL_REPAYMENT_TRANSACTIONS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "FACTORING");

            migrationBuilder.CreateIndex(
                name: "IX_COL_REPAYMENT_TRANSACTIONS_BUSINESS_LINE_CODE",
                table: "COL_REPAYMENT_TRANSACTIONS",
                column: "BUSINESS_LINE_CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_COL_COLLECTION_BATCH_LINES_COL_COLLECTION_BATCHES",
                table: "COL_COLLECTION_BATCH_LINES",
                column: "COLLECTION_BATCH_ID",
                principalTable: "COL_COLLECTION_BATCHES",
                principalColumn: "COLLECTION_BATCH_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_COL_COLLECTION_BATCH_LINES_COL_REPAYMENT_TRANSACTIONS",
                table: "COL_COLLECTION_BATCH_LINES",
                column: "REPAYMENT_TXN_ID",
                principalTable: "COL_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_COL_REPAYMENT_SOURCES",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_SOURCE_ID",
                principalTable: "COL_REPAYMENT_SOURCES",
                principalColumn: "REPAYMENT_SOURCE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_COL_REPAYMENT_TRANSACTIONS",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_TXN_ID",
                principalTable: "COL_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_COL_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "REPAYMENT_SOURCE_ID",
                principalTable: "COL_REPAYMENT_SOURCES",
                principalColumn: "REPAYMENT_SOURCE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_COL_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "REPAYMENT_TXN_ID",
                principalTable: "COL_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_CHQ_CHEQUES",
                table: "COL_REPAYMENT_SOURCES",
                column: "CHEQUE_ID",
                principalTable: "CHQ_CHEQUES",
                principalColumn: "CHEQUE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_COL_REPAYMENT_TRANSACTIONS_RE_E00F5DA6",
                table: "COL_REPAYMENT_SOURCES",
                column: "REPAYMENT_TXN_ID",
                principalTable: "COL_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_COL_REPAYMENT_TRANSACTIONS_SO_EC080C4A",
                table: "COL_REPAYMENT_SOURCES",
                column: "SOURCE_REPAYMENT_TXN_ID",
                principalTable: "COL_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_COL_COLLECTION_BATCH_LINES_COL_COLLECTION_BATCHES",
                table: "COL_COLLECTION_BATCH_LINES");

            migrationBuilder.DropForeignKey(
                name: "FK_COL_COLLECTION_BATCH_LINES_COL_REPAYMENT_TRANSACTIONS",
                table: "COL_COLLECTION_BATCH_LINES");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_COL_REPAYMENT_SOURCES",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_COL_REPAYMENT_TRANSACTIONS",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_COL_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_ALLOCATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_COL_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_ALLOCATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_CHQ_CHEQUES",
                table: "COL_REPAYMENT_SOURCES");

            migrationBuilder.DropForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_COL_REPAYMENT_TRANSACTIONS_RE_E00F5DA6",
                table: "COL_REPAYMENT_SOURCES");

            migrationBuilder.DropForeignKey(
                name: "FK_COL_REPAYMENT_SOURCES_COL_REPAYMENT_TRANSACTIONS_SO_EC080C4A",
                table: "COL_REPAYMENT_SOURCES");

            migrationBuilder.DropIndex(
                name: "IX_COL_REPAYMENT_TRANSACTIONS_BUSINESS_LINE_CODE",
                table: "COL_REPAYMENT_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "BUSINESS_LINE_CODE",
                table: "COL_REPAYMENT_TRANSACTIONS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COL_REPAYMENT_TRANSACTIONS",
                table: "COL_REPAYMENT_TRANSACTIONS");

            migrationBuilder.RenameTable(
                name: "COL_REPAYMENT_TRANSACTIONS",
                newName: "FCT_REPAYMENT_TRANSACTIONS");

            migrationBuilder.RenameIndex(
                name: "UQ_COL_REPAYMENT_TRANSACTIONS_REPAYMENT_NUMBER",
                table: "FCT_REPAYMENT_TRANSACTIONS",
                newName: "UQ_FCT_REPAYMENT_TRANSACTIONS_REPAYMENT_NUMBER");

            migrationBuilder.RenameIndex(
                name: "IX_COL_REPAYMENT_TRANSACTIONS_CLIENT_ID_LEGAL_ENTITY_ID",
                table: "FCT_REPAYMENT_TRANSACTIONS",
                newName: "IX_FCT_REPAYMENT_TRANSACTIONS_CLIENT_ID_LEGAL_ENTITY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_TRANSACTIONS",
                column: "REPAYMENT_TXN_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COL_REPAYMENT_SOURCES",
                table: "COL_REPAYMENT_SOURCES");

            migrationBuilder.RenameTable(
                name: "COL_REPAYMENT_SOURCES",
                newName: "FCT_REPAYMENT_SOURCES");

            migrationBuilder.RenameIndex(
                name: "IX_COL_REPAYMENT_SOURCES_CHEQUE_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                newName: "IX_FCT_REPAYMENT_SOURCES_CHEQUE_ID_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_COL_REPAYMENT_SOURCES_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                newName: "IX_FCT_REPAYMENT_SOURCES_REPAYMENT_TXN_ID_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_COL_REPAYMENT_SOURCES_SOURCE_REPAYMENT_TXN_ID_STATUS_CODE",
                table: "FCT_REPAYMENT_SOURCES",
                newName: "IX_FCT_REPAYMENT_SOURCES_SOURCE_REPAYMENT_TXN_ID_STATUS_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FCT_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_SOURCES",
                column: "REPAYMENT_SOURCE_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COL_COLLECTION_BATCH_LINES",
                table: "COL_COLLECTION_BATCH_LINES");

            migrationBuilder.RenameTable(
                name: "COL_COLLECTION_BATCH_LINES",
                newName: "FCT_COLLECTION_BATCH_LINES");

            migrationBuilder.RenameIndex(
                name: "IX_COL_COLLECTION_BATCH_LINES_CHEQUE_ID",
                table: "FCT_COLLECTION_BATCH_LINES",
                newName: "IX_FCT_COLLECTION_BATCH_LINES_CHEQUE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_COL_COLLECTION_BATCH_LINES_REPAYMENT_TXN_ID",
                table: "FCT_COLLECTION_BATCH_LINES",
                newName: "IX_FCT_COLLECTION_BATCH_LINES_REPAYMENT_TXN_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_COL_COLLECTION_BATCH_LINES_COLLECTION_BATCH_ID_LINE_NO",
                table: "FCT_COLLECTION_BATCH_LINES",
                newName: "UQ_FCT_COLLECTION_BATCH_LINES_COLLECTION_BATCH_ID_LINE_NO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FCT_COLLECTION_BATCH_LINES",
                table: "FCT_COLLECTION_BATCH_LINES",
                column: "COLLECTION_BATCH_LINE_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COL_COLLECTION_BATCHES",
                table: "COL_COLLECTION_BATCHES");

            migrationBuilder.RenameTable(
                name: "COL_COLLECTION_BATCHES",
                newName: "FCT_COLLECTION_BATCHES");

            migrationBuilder.RenameIndex(
                name: "UQ_COL_COLLECTION_BATCHES_BATCH_NO",
                table: "FCT_COLLECTION_BATCHES",
                newName: "UQ_FCT_COLLECTION_BATCHES_BATCH_NO");

            migrationBuilder.RenameIndex(
                name: "IX_COL_COLLECTION_BATCHES_GL_TRAN_SET_ID",
                table: "FCT_COLLECTION_BATCHES",
                newName: "IX_FCT_COLLECTION_BATCHES_GL_TRAN_SET_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FCT_COLLECTION_BATCHES",
                table: "FCT_COLLECTION_BATCHES",
                column: "COLLECTION_BATCH_ID");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COL_REPAYMENT_REQUESTS",
                table: "COL_REPAYMENT_REQUESTS");

            migrationBuilder.RenameTable(
                name: "COL_REPAYMENT_REQUESTS",
                newName: "FCT_REPAYMENT_REQUESTS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FCT_REPAYMENT_REQUESTS",
                table: "FCT_REPAYMENT_REQUESTS",
                column: "REPAYMENT_REQUEST_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_COLLECTION_BATCHES",
                table: "FCT_COLLECTION_BATCH_LINES",
                column: "COLLECTION_BATCH_ID",
                principalTable: "FCT_COLLECTION_BATCHES",
                principalColumn: "COLLECTION_BATCH_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_COLLECTION_BATCH_LINES_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_COLLECTION_BATCH_LINES",
                column: "REPAYMENT_TXN_ID",
                principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_SOURCES",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_SOURCE_ID",
                principalTable: "FCT_REPAYMENT_SOURCES",
                principalColumn: "REPAYMENT_SOURCE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_TXN_ID",
                principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_SOURCES",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "REPAYMENT_SOURCE_ID",
                principalTable: "FCT_REPAYMENT_SOURCES",
                principalColumn: "REPAYMENT_SOURCE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_ALLOCATIONS_FCT_REPAYMENT_TRANSACTIONS",
                table: "FCT_REPAYMENT_ALLOCATIONS",
                column: "REPAYMENT_TXN_ID",
                principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_CHQ_CHEQUES",
                table: "FCT_REPAYMENT_SOURCES",
                column: "CHEQUE_ID",
                principalTable: "CHQ_CHEQUES",
                principalColumn: "CHEQUE_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_RE_10F8E45A",
                table: "FCT_REPAYMENT_SOURCES",
                column: "REPAYMENT_TXN_ID",
                principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FCT_REPAYMENT_SOURCES_FCT_REPAYMENT_TRANSACTIONS_SO_9FCB51DD",
                table: "FCT_REPAYMENT_SOURCES",
                column: "SOURCE_REPAYMENT_TXN_ID",
                principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                principalColumn: "REPAYMENT_TXN_ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
