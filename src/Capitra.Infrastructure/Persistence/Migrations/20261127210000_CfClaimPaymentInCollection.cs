using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer finance claims are paid in Collection › Customer Payments (Ahmed, 27 Nov 2026), like the factoring claims: the money is
    /// received in Collection first (DR bank / CR 230104 Customer Unallocated Receipts), then the claim is paid from that payment source.
    /// LND_CF_CLAIM_PAYMENTS keeps the repayment and source it was paid from (nullable). Rule CF_CLAIM_PAYMENT's debit line moves from the
    /// bank account chosen to 230104 (the same account as factoring); payments already posted are not touched.
    /// </summary>
    public partial class CfClaimPaymentInCollection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "REPAYMENT_TXN_ID", table: "LND_CF_CLAIM_PAYMENTS", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "REPAYMENT_SOURCE_ID", table: "LND_CF_CLAIM_PAYMENTS", type: "bigint", nullable: true);
            migrationBuilder.Sql("""
                UPDATE "ADM_PRODUCT_EVENT_RULE_GL_LINES" gl
                   SET "ACCOUNT_SOURCE_CODE" = 'ACCOUNT',
                       "ACCOUNT_ID" = (SELECT a."ACCOUNT_ID" FROM "GL_ACCOUNTS" a WHERE a."ACCOUNT_CODE" = '230104' AND a."IS_ACTIVE" ORDER BY a."ACCOUNT_ID" LIMIT 1),
                       "DESCRIPTION_TEMPLATE" = 'Upfront claim paid from the customer''s receipts',
                       "UPDATED_BY" = 'MIGRATION', "UPDATED_AT" = now()
                  FROM "ADM_PRODUCT_EVENT_RULES" er
                 WHERE er."PRODUCT_EVENT_RULE_ID" = gl."PRODUCT_EVENT_RULE_ID" AND er."RULE_CODE" = 'CF_CLAIM_PAYMENT'
                   AND gl."AMOUNT_CODE" = 'PAYMENT_AMOUNT' AND gl."ACCOUNT_SOURCE_CODE" = 'BANK_ACCOUNT'
                   AND EXISTS (SELECT 1 FROM "GL_ACCOUNTS" a WHERE a."ACCOUNT_CODE" = '230104' AND a."IS_ACTIVE");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "REPAYMENT_TXN_ID", table: "LND_CF_CLAIM_PAYMENTS");
            migrationBuilder.DropColumn(name: "REPAYMENT_SOURCE_ID", table: "LND_CF_CLAIM_PAYMENTS");
        }
    }
}
