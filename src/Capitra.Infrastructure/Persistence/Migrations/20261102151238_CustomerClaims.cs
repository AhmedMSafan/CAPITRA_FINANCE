using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENT_CHARGE_SOURCES");

            migrationBuilder.AddColumn<string>(
                name: "CLAIM_STATUS_CODE",
                table: "FCT_DISBURSEMENTS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NOT_CREATED");

            migrationBuilder.CreateTable(
                name: "FCT_CUSTOMER_CLAIMS",
                columns: table => new
                {
                    CLAIM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLAIM_NUMBER = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: true),
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLAIM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAID_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_CUSTOMER_CLAIMS", x => x.CLAIM_ID);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIMS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIMS_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_CUSTOMER_CLAIM_LINES",
                columns: table => new
                {
                    CLAIM_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLAIM_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    CHARGE_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CHARGE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CHARGE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAID_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_CUSTOMER_CLAIM_LINES", x => x.CLAIM_LINE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIM_LINES_FCT_CUSTOMER_CLAIMS",
                        column: x => x.CLAIM_ID,
                        principalTable: "FCT_CUSTOMER_CLAIMS",
                        principalColumn: "CLAIM_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                columns: table => new
                {
                    CLAIM_PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLAIM_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPAYMENT_TXN_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPAYMENT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_CUSTOMER_CLAIM_PAYMENTS", x => x.CLAIM_PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_CUSTOMER_CLAIMS",
                        column: x => x.CLAIM_ID,
                        principalTable: "FCT_CUSTOMER_CLAIMS",
                        principalColumn: "CLAIM_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_SOURCES",
                        column: x => x.REPAYMENT_SOURCE_ID,
                        principalTable: "FCT_REPAYMENT_SOURCES",
                        principalColumn: "REPAYMENT_SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_CUSTOMER_CLAIM_PAYMENTS_FCT_REPAYMENT_TRANSACTIONS",
                        column: x => x.REPAYMENT_TXN_ID,
                        principalTable: "FCT_REPAYMENT_TRANSACTIONS",
                        principalColumn: "REPAYMENT_TXN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_CUSTOMER_CLAIM_LINES_CLAIM_ID_CHARGE_CODE",
                table: "FCT_CUSTOMER_CLAIM_LINES",
                columns: new[] { "CLAIM_ID", "CHARGE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CUSTOMER_CLAIM_PAYMENTS_CLAIM_ID",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "CLAIM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CUSTOMER_CLAIM_PAYMENTS_REPAYMENT_SOURCE_ID",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CUSTOMER_CLAIM_PAYMENTS_REPAYMENT_TXN_ID",
                table: "FCT_CUSTOMER_CLAIM_PAYMENTS",
                column: "REPAYMENT_TXN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CUSTOMER_CLAIMS_CLIENT_ID_STATUS_CODE",
                table: "FCT_CUSTOMER_CLAIMS",
                columns: new[] { "CLIENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_CUSTOMER_CLAIMS_DISBURSEMENT_ID",
                table: "FCT_CUSTOMER_CLAIMS",
                column: "DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_CUSTOMER_CLAIMS_CLAIM_NUMBER",
                table: "FCT_CUSTOMER_CLAIMS",
                column: "CLAIM_NUMBER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_CUSTOMER_CLAIM_LINES");

            migrationBuilder.DropTable(
                name: "FCT_CUSTOMER_CLAIM_PAYMENTS");

            migrationBuilder.DropTable(
                name: "FCT_CUSTOMER_CLAIMS");

            migrationBuilder.DropColumn(
                name: "CLAIM_STATUS_CODE",
                table: "FCT_DISBURSEMENTS");

            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENT_CHARGE_SOURCES",
                columns: table => new
                {
                    DISBURSEMENT_CHARGE_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPAYMENT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_DISBURSEMENT_CHARGE_SOURCES", x => x.DISBURSEMENT_CHARGE_SOURCE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHARGE_SOURCES_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHARGE_SOURCES_FCT_REPAYMENT_SOURCES",
                        column: x => x.REPAYMENT_SOURCE_ID,
                        principalTable: "FCT_REPAYMENT_SOURCES",
                        principalColumn: "REPAYMENT_SOURCE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHARGE_SOURCES_REPAYMENT_SOURCE_ID",
                table: "FCT_DISBURSEMENT_CHARGE_SOURCES",
                column: "REPAYMENT_SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_DISBURSEMENT_CHARGE_SOURCES_DISBURSEMENT_ID",
                table: "FCT_DISBURSEMENT_CHARGE_SOURCES",
                columns: new[] { "DISBURSEMENT_ID", "REPAYMENT_SOURCE_ID" },
                unique: true);
        }
    }
}
