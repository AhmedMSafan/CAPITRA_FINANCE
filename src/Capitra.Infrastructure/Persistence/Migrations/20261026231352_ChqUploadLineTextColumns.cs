using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChqUploadLineTextColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RAW_DUE_DATE",
                table: "CHQ_UPLOAD_LINES",
                newName: "RAW_DUE_DATE_TEXT");

            migrationBuilder.RenameColumn(
                name: "RAW_CHEQUE_AMOUNT",
                table: "CHQ_UPLOAD_LINES",
                newName: "RAW_CHEQUE_AMOUNT_TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RAW_DUE_DATE_TEXT",
                table: "CHQ_UPLOAD_LINES",
                newName: "RAW_DUE_DATE");

            migrationBuilder.RenameColumn(
                name: "RAW_CHEQUE_AMOUNT_TEXT",
                table: "CHQ_UPLOAD_LINES",
                newName: "RAW_CHEQUE_AMOUNT");
        }
    }
}
