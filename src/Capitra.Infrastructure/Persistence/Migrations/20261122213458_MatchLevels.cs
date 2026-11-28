using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatchLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BIRTH_MATCH_CODE",
                table: "SCR_INQUIRY_HITS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_DIGITS_DIFFERENT",
                table: "SCR_INQUIRY_HITS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ID_MATCH_CODE",
                table: "SCR_INQUIRY_HITS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ID_SCORE_PCT",
                table: "SCR_INQUIRY_HITS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NATIONALITY_MATCH_CODE",
                table: "SCR_INQUIRY_HITS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_MATCH_MIN_DIGITS",
                table: "SCR_INQUIRIES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NAME_MATCH_MIN",
                table: "SCR_INQUIRIES",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BIRTH_MATCH_CODE",
                table: "SCR_INQUIRY_HITS");

            migrationBuilder.DropColumn(
                name: "ID_DIGITS_DIFFERENT",
                table: "SCR_INQUIRY_HITS");

            migrationBuilder.DropColumn(
                name: "ID_MATCH_CODE",
                table: "SCR_INQUIRY_HITS");

            migrationBuilder.DropColumn(
                name: "ID_SCORE_PCT",
                table: "SCR_INQUIRY_HITS");

            migrationBuilder.DropColumn(
                name: "NATIONALITY_MATCH_CODE",
                table: "SCR_INQUIRY_HITS");

            migrationBuilder.DropColumn(
                name: "ID_MATCH_MIN_DIGITS",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "NAME_MATCH_MIN",
                table: "SCR_INQUIRIES");
        }
    }
}
