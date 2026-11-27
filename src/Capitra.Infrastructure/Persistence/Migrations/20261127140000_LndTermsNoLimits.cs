using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Lending terms without limits (Ahmed, 27 Nov 2026): a product has one set of terms with no amount or term limits, so the four limit
    /// columns go. Grace days are 0 where there is no late fee (they only mean something with one).
    /// </summary>
    public partial class LndTermsNoLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "LND_PRODUCT_VERSIONS" SET "LATE_FEE_GRACE_DAYS" = 0 WHERE "LATE_FEE_TYPE_CODE" = 'NONE' AND "LATE_FEE_GRACE_DAYS" <> 0;
                """);
            migrationBuilder.DropColumn(name: "MIN_AMOUNT", table: "LND_PRODUCT_VERSIONS");
            migrationBuilder.DropColumn(name: "MAX_AMOUNT", table: "LND_PRODUCT_VERSIONS");
            migrationBuilder.DropColumn(name: "MIN_TERM_MONTHS", table: "LND_PRODUCT_VERSIONS");
            migrationBuilder.DropColumn(name: "MAX_TERM_MONTHS", table: "LND_PRODUCT_VERSIONS");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "MIN_AMOUNT", table: "LND_PRODUCT_VERSIONS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "MAX_AMOUNT", table: "LND_PRODUCT_VERSIONS", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<int>(name: "MIN_TERM_MONTHS", table: "LND_PRODUCT_VERSIONS", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "MAX_TERM_MONTHS", table: "LND_PRODUCT_VERSIONS", type: "integer", nullable: false, defaultValue: 0);
        }
    }
}
