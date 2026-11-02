using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepaymentScheduleTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "INTEREST_FIRST_DUE_CODE",
                table: "FCT_DISBURSEMENTS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "INTEREST_FREQUENCY_MONTHS",
                table: "FCT_DISBURSEMENTS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "INTEREST_START_DATE",
                table: "FCT_DISBURSEMENTS",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRINCIPAL_FIRST_DUE_CODE",
                table: "FCT_DISBURSEMENTS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PRINCIPAL_FREQUENCY_MONTHS",
                table: "FCT_DISBURSEMENTS",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENT_CUSTOM_SCHEDULES",
                columns: table => new
                {
                    CUSTOM_SCHEDULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_DISBURSEMENT_CUSTOM_SCHEDULES", x => x.CUSTOM_SCHEDULE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CUSTOM_SCHEDULES_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_DISBURSEMENT_CUSTOM_SCHEDULES_DISBURSEMENT_ID_DUE_DATE",
                table: "FCT_DISBURSEMENT_CUSTOM_SCHEDULES",
                columns: new[] { "DISBURSEMENT_ID", "DUE_DATE" },
                unique: true);
        
            // the cheque-based options are gone (Ahmed, 2 Nov 2026): requests not completed yet take at maturity and equal amounts
            migrationBuilder.Sql("""
                UPDATE "FCT_DISBURSEMENTS" SET "PRINCIPAL_REPAYMENT_METHOD" = 'AT_MATURITY'
                 WHERE "PRINCIPAL_REPAYMENT_METHOD" IN ('CHEQUE_DUE_DATE', 'CHEQUES_DUE_DATE') AND NOT "IS_GL_POSTED";
                UPDATE "FCT_DISBURSEMENTS" SET "PRINCIPAL_DISTRIBUTION_METHOD" = 'EQUAL_AMOUNT'
                 WHERE "PRINCIPAL_DISTRIBUTION_METHOD" IN ('CHEQUE_AMOUNT', 'CHEQUES_AMOUNT', 'CHEQUES_PRO_RATA') AND NOT "IS_GL_POSTED";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENT_CUSTOM_SCHEDULES");

            migrationBuilder.DropColumn(
                name: "INTEREST_FIRST_DUE_CODE",
                table: "FCT_DISBURSEMENTS");

            migrationBuilder.DropColumn(
                name: "INTEREST_FREQUENCY_MONTHS",
                table: "FCT_DISBURSEMENTS");

            migrationBuilder.DropColumn(
                name: "INTEREST_START_DATE",
                table: "FCT_DISBURSEMENTS");

            migrationBuilder.DropColumn(
                name: "PRINCIPAL_FIRST_DUE_CODE",
                table: "FCT_DISBURSEMENTS");

            migrationBuilder.DropColumn(
                name: "PRINCIPAL_FREQUENCY_MONTHS",
                table: "FCT_DISBURSEMENTS");
        }
    }
}
