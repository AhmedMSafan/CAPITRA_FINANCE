using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChqStatusForeignKeysOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CHQ_CHEQUES_CHQ_LKP_STATUSES",
                table: "CHQ_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_NEW_STATUS_CODE",
                table: "CHQ_EVENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_OLD_STATUS_CODE",
                table: "CHQ_EVENTS");

            migrationBuilder.DropIndex(
                name: "IX_CHQ_EVENTS_NEW_STATUS_CODE",
                table: "CHQ_EVENTS");

            migrationBuilder.DropIndex(
                name: "IX_CHQ_EVENTS_OLD_STATUS_CODE",
                table: "CHQ_EVENTS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_NEW_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "NEW_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CHQ_EVENTS_OLD_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "OLD_STATUS_CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_CHQ_CHEQUES_CHQ_LKP_STATUSES",
                table: "CHQ_CHEQUES",
                column: "STATUS_CODE",
                principalTable: "CHQ_LKP_STATUSES",
                principalColumn: "STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_NEW_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "NEW_STATUS_CODE",
                principalTable: "CHQ_LKP_STATUSES",
                principalColumn: "STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CHQ_EVENTS_CHQ_LKP_STATUSES_OLD_STATUS_CODE",
                table: "CHQ_EVENTS",
                column: "OLD_STATUS_CODE",
                principalTable: "CHQ_LKP_STATUSES",
                principalColumn: "STATUS_CODE",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
