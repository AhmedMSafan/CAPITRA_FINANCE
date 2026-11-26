using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// A consumer finance is bought from an active supplier (Ahmed, 26 Nov 2026): LND_LOANS keeps the supplier agreement and a copy
    /// of its terms (down payment collector and minimum, commission, payment and delivery days).
    /// </summary>
    public partial class CfFinanceSupplier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "DOWN_PAYMENT_COLLECTOR_CODE", table: "LND_LOANS", type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<long>(name: "SUPPLIER_APPLICATION_ID", table: "LND_LOANS", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "SUPPLIER_COMMISSION_PCT", table: "LND_LOANS", type: "numeric(9,6)", precision: 9, scale: 6, nullable: true);
            migrationBuilder.AddColumn<int>(name: "SUPPLIER_DELIVERY_DAYS", table: "LND_LOANS", type: "integer", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "SUPPLIER_MIN_DOWN_PAYMENT_PCT", table: "LND_LOANS", type: "numeric(9,6)", precision: 9, scale: 6, nullable: true);
            migrationBuilder.AddColumn<int>(name: "SUPPLIER_PAYMENT_DAYS", table: "LND_LOANS", type: "integer", nullable: true);

            migrationBuilder.CreateIndex(name: "IX_LND_LOANS_SUPPLIER_APPLICATION_ID", table: "LND_LOANS", column: "SUPPLIER_APPLICATION_ID");
            migrationBuilder.AddForeignKey(
                name: "FK_LND_LOANS_CRM_CF_SUPPLIER_APPLICATIONS",
                table: "LND_LOANS",
                column: "SUPPLIER_APPLICATION_ID",
                principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                principalColumn: "APPLICATION_ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_LND_LOANS_CRM_CF_SUPPLIER_APPLICATIONS", table: "LND_LOANS");
            migrationBuilder.DropIndex(name: "IX_LND_LOANS_SUPPLIER_APPLICATION_ID", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "DOWN_PAYMENT_COLLECTOR_CODE", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "SUPPLIER_APPLICATION_ID", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "SUPPLIER_COMMISSION_PCT", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "SUPPLIER_DELIVERY_DAYS", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "SUPPLIER_MIN_DOWN_PAYMENT_PCT", table: "LND_LOANS");
            migrationBuilder.DropColumn(name: "SUPPLIER_PAYMENT_DAYS", table: "LND_LOANS");
        }
    }
}
