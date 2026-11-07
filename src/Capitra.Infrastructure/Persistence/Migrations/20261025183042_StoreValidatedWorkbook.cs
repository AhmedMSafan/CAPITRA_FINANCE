using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreValidatedWorkbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "WORKBOOK_CONTENT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WORKBOOK_CONTENT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");
        }
    }
}
