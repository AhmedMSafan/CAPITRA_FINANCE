using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer finance (Ahmed, 27 Nov 2026): Credit approval keeps the product rate fixed and adds or takes off a margin for the finance;
    /// LND_LOANS.RATE_MARGIN_PCT keeps it (ANNUAL_RATE_PCT is the product rate plus the margin).
    /// </summary>
    public partial class CfCreditRateMargin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "RATE_MARGIN_PCT", table: "LND_LOANS", type: "numeric(9,6)", precision: 9, scale: 6, nullable: false, defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RATE_MARGIN_PCT", table: "LND_LOANS");
        }
    }
}
