using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingPrepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_PREPAYMENT_ALLOWED",
                table: "LND_PRODUCT_VERSIONS",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PREPAYMENT_RECALC_CODE",
                table: "LND_PRODUCT_VERSIONS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "REDUCE_INSTALMENT");

            migrationBuilder.AddColumn<decimal>(
                name: "PREPAYMENT_FEE_PCT",
                table: "LND_PRODUCT_VERSIONS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EARLY_SETTLEMENT_FEE_PCT",
                table: "LND_PRODUCT_VERSIONS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IS_PREPAYMENT_ALLOWED",
                table: "LND_LOANS",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PREPAYMENT_RECALC_CODE",
                table: "LND_LOANS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "REDUCE_INSTALMENT");

            migrationBuilder.AddColumn<decimal>(
                name: "PREPAYMENT_FEE_PCT",
                table: "LND_LOANS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EARLY_SETTLEMENT_FEE_PCT",
                table: "LND_LOANS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_PREPAYMENT_ALLOWED",
                table: "LND_PRODUCT_VERSIONS");

            migrationBuilder.DropColumn(
                name: "PREPAYMENT_RECALC_CODE",
                table: "LND_PRODUCT_VERSIONS");

            migrationBuilder.DropColumn(
                name: "PREPAYMENT_FEE_PCT",
                table: "LND_PRODUCT_VERSIONS");

            migrationBuilder.DropColumn(
                name: "EARLY_SETTLEMENT_FEE_PCT",
                table: "LND_PRODUCT_VERSIONS");

            migrationBuilder.DropColumn(
                name: "IS_PREPAYMENT_ALLOWED",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "PREPAYMENT_RECALC_CODE",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "PREPAYMENT_FEE_PCT",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "EARLY_SETTLEMENT_FEE_PCT",
                table: "LND_LOANS");
        }
    }
}
