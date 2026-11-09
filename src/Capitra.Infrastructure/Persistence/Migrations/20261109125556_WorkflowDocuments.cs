using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "WORKFLOW_DEFINITION_ID",
                table: "WFE_FORMS",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "FIELDS_JSON",
                table: "WFE_FORMS",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FIELDS_JSON",
                table: "WFE_FORMS");

            migrationBuilder.AlterColumn<long>(
                name: "WORKFLOW_DEFINITION_ID",
                table: "WFE_FORMS",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
