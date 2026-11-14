using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingEndOfDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DAYS_LATE",
                table: "LND_LOANS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "INTEREST_SUSPENSE_AMOUNT",
                table: "LND_LOANS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IS_WRITE_OFF_PROPOSED",
                table: "LND_LOANS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LAST_EOD_DATE",
                table: "LND_LOANS",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LATE_FEE_SUSPENSE_AMOUNT",
                table: "LND_LOANS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "INTEREST_ACCRUED_AMOUNT",
                table: "LND_INSTALLMENTS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LATE_FEE_DAYS",
                table: "LND_INSTALLMENTS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LND_EOD_RUNS",
                columns: table => new
                {
                    EOD_RUN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUN_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LOANS_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CREDIT_APPLIED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_ACCRUED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_SUSPENDED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LATE_FEE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NON_ACCRUAL_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CURED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    WRITE_OFF_PROPOSED_COUNT = table.Column<int>(type: "integer", nullable: false),
                    GL_TRAN_SET_IDS = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FINISHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_EOD_RUNS", x => x.EOD_RUN_ID);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_LND_EOD_RUNS_RUN_DATE",
                table: "LND_EOD_RUNS",
                column: "RUN_DATE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_EOD_RUNS");

            migrationBuilder.DropColumn(
                name: "DAYS_LATE",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "INTEREST_SUSPENSE_AMOUNT",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "IS_WRITE_OFF_PROPOSED",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "LAST_EOD_DATE",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "LATE_FEE_SUSPENSE_AMOUNT",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "INTEREST_ACCRUED_AMOUNT",
                table: "LND_INSTALLMENTS");

            migrationBuilder.DropColumn(
                name: "LATE_FEE_DAYS",
                table: "LND_INSTALLMENTS");
        }
    }
}
