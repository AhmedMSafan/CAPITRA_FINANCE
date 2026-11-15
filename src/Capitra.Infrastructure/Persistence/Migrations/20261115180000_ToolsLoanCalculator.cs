using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ToolsLoanCalculator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TLS_LOAN_SCENARIOS",
                columns: table => new
                {
                    SCENARIO_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SCENARIO_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PARENT_SCENARIO_ID = table.Column<long>(type: "bigint", nullable: true),
                    RATE_INDEX_ID = table.Column<long>(type: "bigint", nullable: true),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CONTRACT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TENOR_VALUE = table.Column<int>(type: "integer", nullable: false),
                    TENOR_UNIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SCHEDULE_MODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PRINCIPAL_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INTEREST_PAYMENT_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DAY_COUNT_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PRINCIPAL_PAYMENT_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    INTEREST_PAYMENT_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PRINCIPAL_FREQUENCY_VALUE = table.Column<int>(type: "integer", nullable: true),
                    PRINCIPAL_FREQUENCY_UNIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PRINCIPAL_DATE_RULE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INTEREST_FREQUENCY_VALUE = table.Column<int>(type: "integer", nullable: true),
                    INTEREST_FREQUENCY_UNIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INTEREST_DATE_RULE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    COMBINED_FREQUENCY_VALUE = table.Column<int>(type: "integer", nullable: true),
                    COMBINED_FREQUENCY_UNIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    COMBINED_DATE_RULE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TLS_LOAN_SCENARIOS", x => x.SCENARIO_ID);
                });

            migrationBuilder.CreateTable(
                name: "TLS_LOAN_SCENARIO_RATES",
                columns: table => new
                {
                    SCENARIO_RATE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SCENARIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    EFFECTIVE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    INTEREST_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TLS_LOAN_SCENARIO_RATES", x => x.SCENARIO_RATE_ID);
                    table.ForeignKey(
                        name: "FK_TLS_LOAN_SCENARIO_RATES_TLS_LOAN_SCENARIOS",
                        column: x => x.SCENARIO_ID,
                        principalTable: "TLS_LOAN_SCENARIOS",
                        principalColumn: "SCENARIO_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TLS_LOAN_SCENARIO_LINES",
                columns: table => new
                {
                    SCHEDULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SCENARIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    ACCRUAL_START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ACCRUAL_END_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    OPENING_PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CLOSING_PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    DAY_COUNT_FRACTION = table.Column<decimal>(type: "numeric(18,12)", precision: 18, scale: 12, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TLS_LOAN_SCENARIO_LINES", x => x.SCHEDULE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_TLS_LOAN_SCENARIO_LINES_TLS_LOAN_SCENARIOS",
                        column: x => x.SCENARIO_ID,
                        principalTable: "TLS_LOAN_SCENARIOS",
                        principalColumn: "SCENARIO_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_TLS_LOAN_SCENARIOS_CREATED_BY_SCENARIO_NAME",
                table: "TLS_LOAN_SCENARIOS",
                columns: new[] { "CREATED_BY", "SCENARIO_NAME" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TLS_LOAN_SCENARIO_RATES_SCENARIO_ID_EFFECTIVE_DATE",
                table: "TLS_LOAN_SCENARIO_RATES",
                columns: new[] { "SCENARIO_ID", "EFFECTIVE_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TLS_LOAN_SCENARIO_LINES_SCENARIO_ID_INSTALLMENT_NO",
                table: "TLS_LOAN_SCENARIO_LINES",
                columns: new[] { "SCENARIO_ID", "INSTALLMENT_NO" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TLS_LOAN_SCENARIO_LINES");

            migrationBuilder.DropTable(
                name: "TLS_LOAN_SCENARIO_RATES");

            migrationBuilder.DropTable(
                name: "TLS_LOAN_SCENARIOS");
        }
    }
}
