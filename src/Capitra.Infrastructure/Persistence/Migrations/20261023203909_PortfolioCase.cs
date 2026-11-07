using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO",
                columns: table => new
                {
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DEVELOPER_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROJECT_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUEST_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PORTFOLIO_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    COMMENTS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FACE_VALUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OUTSTANDING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PROPOSED_PURCHASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EXPECTED_RETURN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ADMIN_EXPENSE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    COVERAGE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RESERVED_AMOUNT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LATE_FEE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MARGIN_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ADMIN_EXPENSE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LEGAL_FEE_PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VALUATION_FEE_PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO", x => x.PORTFOLIO_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_ADM_LKP_CURRENCY",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCY",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_MTG_DEVELOPER",
                        column: x => x.DEVELOPER_ID,
                        principalTable: "MTG_DEVELOPER",
                        principalColumn: "DEVELOPER_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_MTG_LKP_PORTFOLIO_STATUS",
                        column: x => x.PORTFOLIO_STATUS_CODE,
                        principalTable: "MTG_LKP_PORTFOLIO_STATUS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_MTG_PROJECT",
                        column: x => x.PROJECT_ID,
                        principalTable: "MTG_PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO_ACCOUNT",
                columns: table => new
                {
                    PORTFOLIO_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    INVESTOR_ID = table.Column<long>(type: "bigint", nullable: false),
                    PROJECT_UNIT_ID = table.Column<long>(type: "bigint", nullable: false),
                    BOOKING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONTRACT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONTRACT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OUTSTANDING_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INSTALLMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INSTALLMENT_FREQUENCY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LAST_PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REGULAR_PAYMENT_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    ARREARS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ACCOUNT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO_ACCOUNT", x => x.PORTFOLIO_ACCOUNT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_INVESTOR",
                        column: x => x.INVESTOR_ID,
                        principalTable: "MTG_INVESTOR",
                        principalColumn: "INVESTOR_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PROJECT_UNIT",
                        column: x => x.PROJECT_UNIT_ID,
                        principalTable: "MTG_PROJECT_UNIT",
                        principalColumn: "PROJECT_UNIT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PORTFOLIO_STATUS_HISTORY",
                columns: table => new
                {
                    PORTFOLIO_STATUS_HISTORY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    FROM_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TO_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    COMMENTS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PORTFOLIO_STATUS_HISTORY", x => x.PORTFOLIO_STATUS_HISTORY_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PORTFOLIO_STATUS_HISTORY_MTG_PORTFOLIO",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PORTFOLIO",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_ACCOUNT_PAYMENT",
                columns: table => new
                {
                    ACCOUNT_PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAYMENT_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DAYS_LATE = table.Column<int>(type: "integer", nullable: false),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_ACCOUNT_PAYMENT", x => x.ACCOUNT_PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_ACCOUNT_PAYMENT_ADM_LKP_PAYMENT_STATUS",
                        column: x => x.PAYMENT_STATUS_CODE,
                        principalTable: "ADM_LKP_PAYMENT_STATUS",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_ACCOUNT_PAYMENT_MTG_PORTFOLIO_ACCOUNT",
                        column: x => x.PORTFOLIO_ACCOUNT_ID,
                        principalTable: "MTG_PORTFOLIO_ACCOUNT",
                        principalColumn: "PORTFOLIO_ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[] { "DRAFT", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 1, true, "مسودة", "Draft", 1, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_ACCOUNT_PAYMENT_PAYMENT_STATUS_CODE",
                table: "MTG_ACCOUNT_PAYMENT",
                column: "PAYMENT_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_ACCOUNT_PAYMENT_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_ACCOUNT_PAYMENT",
                column: "PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_CURRENCY_CODE",
                table: "MTG_PORTFOLIO",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_DEVELOPER_ID",
                table: "MTG_PORTFOLIO",
                column: "DEVELOPER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_PORTFOLIO_STATUS_CODE",
                table: "MTG_PORTFOLIO",
                column: "PORTFOLIO_STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_PROJECT_ID",
                table: "MTG_PORTFOLIO",
                column: "PROJECT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PORTFOLIO_PORTFOLIO_NO",
                table: "MTG_PORTFOLIO",
                column: "PORTFOLIO_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_INVESTOR_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "INVESTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_PROJECT_UNIT_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PROJECT_UNIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PORTFOLIO_STATUS_HISTORY_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_STATUS_HISTORY",
                column: "PORTFOLIO_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_ACCOUNT_PAYMENT");

            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO_STATUS_HISTORY");

            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropTable(
                name: "MTG_PORTFOLIO");

            migrationBuilder.DeleteData(
                table: "MTG_LKP_PORTFOLIO_STATUS",
                keyColumn: "CODE",
                keyValue: "DRAFT");
        }
    }
}
