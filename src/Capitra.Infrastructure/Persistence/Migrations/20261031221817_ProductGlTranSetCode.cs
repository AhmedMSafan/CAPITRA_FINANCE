using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductGlTranSetCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GL_TRAN_SET_CODE",
                table: "ADM_PRODUCTS",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCTS_GL_TRAN_SET_CODE",
                table: "ADM_PRODUCTS",
                column: "GL_TRAN_SET_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ADM_PRODUCTS_GL_TRAN_SET_CODE",
                table: "ADM_PRODUCTS");

            migrationBuilder.DropColumn(
                name: "GL_TRAN_SET_CODE",
                table: "ADM_PRODUCTS");
        }
    }
}
