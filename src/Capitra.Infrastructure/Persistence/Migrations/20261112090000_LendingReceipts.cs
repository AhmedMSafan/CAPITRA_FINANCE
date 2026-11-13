using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_ALLOCATIONS",
                columns: table => new
                {
                    ALLOCATION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: false),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PART_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ALLOCATED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_ALLOCATIONS", x => x.ALLOCATION_ID);
                    table.ForeignKey(
                        name: "FK_LND_ALLOCATIONS_COL_REPAYMENT_TRANSACTIONS",
                        column: x => x.REPAYMENT_TXN_ID,
                        principalTable: "COL_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LND_ALLOCATIONS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LND_ALLOCATIONS_LND_INSTALLMENTS",
                        column: x => x.INSTALLMENT_ID,
                        principalTable: "LND_INSTALLMENTS",
                        principalColumn: "INSTALLMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_ALLOCATIONS_INSTALLMENT_ID",
                table: "LND_ALLOCATIONS",
                column: "INSTALLMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_ALLOCATIONS_LOAN_ID_STATUS_CODE",
                table: "LND_ALLOCATIONS",
                columns: new[] { "LOAN_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_LND_ALLOCATIONS_REPAYMENT_TXN_ID",
                table: "LND_ALLOCATIONS",
                column: "REPAYMENT_TXN_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_ALLOCATIONS");
        }
    }
}
