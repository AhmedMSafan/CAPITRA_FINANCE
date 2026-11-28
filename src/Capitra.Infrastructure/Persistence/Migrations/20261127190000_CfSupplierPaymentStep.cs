using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CF_FINANCE step 6, Supplier payment (Ahmed, 27 Nov 2026): the payment to the supplier of a consumer finance on LND_LOANS (its
    /// commission and the reason when Treasury changed it, the bank account, the reference, who paid and when, the GL), all nullable.
    /// </summary>
    public partial class CfSupplierPaymentStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "SUPPLIER_COMMISSION_AMOUNT", table: "LND_LOANS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
            migrationBuilder.AddColumn<string>(name: "SUPPLIER_COMMISSION_REASON", table: "LND_LOANS", type: "character varying(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "SUPPLIER_PAID_AT", table: "LND_LOANS", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<string>(name: "SUPPLIER_PAID_BY", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<long>(name: "SUPPLIER_PAYMENT_BANK_ACCOUNT_ID", table: "LND_LOANS", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "SUPPLIER_PAYMENT_GL_TRAN_SET_ID", table: "LND_LOANS", type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "SUPPLIER_PAYMENT_REFERENCE", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var c in new[] { "SUPPLIER_COMMISSION_AMOUNT", "SUPPLIER_COMMISSION_REASON", "SUPPLIER_PAID_AT", "SUPPLIER_PAID_BY",
                         "SUPPLIER_PAYMENT_BANK_ACCOUNT_ID", "SUPPLIER_PAYMENT_GL_TRAN_SET_ID", "SUPPLIER_PAYMENT_REFERENCE" })
                migrationBuilder.DropColumn(name: c, table: "LND_LOANS");
        }
    }
}
