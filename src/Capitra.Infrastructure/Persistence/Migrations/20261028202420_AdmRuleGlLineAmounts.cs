using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdmRuleGlLineAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_GL_LINES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.AlterColumn<long>(
                name: "ACCOUNT_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "ACCOUNT_SOURCE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ACCOUNT");

            migrationBuilder.AddColumn<string>(
                name: "AMOUNT_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "JOURNAL_NO",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_GL_LINES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                columns: new[] { "PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_GL_LINES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.DropColumn(
                name: "ACCOUNT_SOURCE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.DropColumn(
                name: "AMOUNT_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.DropColumn(
                name: "JOURNAL_NO",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.AlterColumn<long>(
                name: "ACCOUNT_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_GL_LINES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                columns: new[] { "PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "LINE_NO" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
