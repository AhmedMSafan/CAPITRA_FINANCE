using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TreasuryPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GL_TRAN_SET_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PAID_FROM_ACCOUNT_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_PAID_FROM_ACCOUNT_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                column: "PAID_FROM_ACCOUNT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_BANK_ACCOUNTS",
                table: "TRS_DISBURSEMENT_FUNDINGS",
                column: "PAID_FROM_ACCOUNT_ID",
                principalTable: "TRS_BANK_ACCOUNTS",
                principalColumn: "ACCOUNT_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TRS_DISBURSEMENT_FUNDINGS_TRS_BANK_ACCOUNTS",
                table: "TRS_DISBURSEMENT_FUNDINGS");

            migrationBuilder.DropIndex(
                name: "IX_TRS_DISBURSEMENT_FUNDINGS_PAID_FROM_ACCOUNT_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS");

            migrationBuilder.DropColumn(
                name: "GL_TRAN_SET_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS");

            migrationBuilder.DropColumn(
                name: "PAID_FROM_ACCOUNT_ID",
                table: "TRS_DISBURSEMENT_FUNDINGS");
        }
    }
}
