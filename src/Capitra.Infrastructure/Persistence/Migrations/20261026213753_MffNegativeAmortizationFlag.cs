using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MffNegativeAmortizationFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IS_HAS_NEGATIVE_AMORTIZATION",
                table: "MFF_CALCULATION_RUNS",
                newName: "IS_NEGATIVE_AMORTIZATION");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IS_NEGATIVE_AMORTIZATION",
                table: "MFF_CALCULATION_RUNS",
                newName: "IS_HAS_NEGATIVE_AMORTIZATION");
        }
    }
}
