using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DisbursementDraftCheques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENT_CHARGE_SOURCES",
                columns: table => new
                {
                    DISBURSEMENT_CHARGE_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    REPAYMENT_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENT_CHEQUES",
                columns: table => new
                {
                    DISBURSEMENT_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    BATCH_ID = table.Column<long>(type: "bigint", nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHEQUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    ISSUER_BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ISSUER_BANK_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ISSUER_BANK_BRANCH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DRAWER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DRAWER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DRAWER_NATIONAL_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DRAWER_ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CUSTOMER_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CUSTOMER_NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CUSTOMER_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DEPOSIT_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    RECEIVED_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DEPOSIT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WALLET_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_DISBURSEMENT_CHEQUES", x => x.DISBURSEMENT_CHEQUE_ID);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHEQUES_CHQ_CHEQUES",
                        column: x => x.WALLET_CHEQUE_ID,
                        principalTable: "CHQ_CHEQUES",
                        principalColumn: "CHEQUE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENT_CHEQUES_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
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

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHEQUES_DISBURSEMENT_ID_STATUS_CODE",
                table: "FCT_DISBURSEMENT_CHEQUES",
                columns: new[] { "DISBURSEMENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENT_CHEQUES_WALLET_CHEQUE_ID",
                table: "FCT_DISBURSEMENT_CHEQUES",
                column: "WALLET_CHEQUE_ID");

            // the cheques uploaded on page 705 before this change went straight into the wallet: they are kept there and listed on their
            // request as already loaded (IN_WALLET), so Complete does not load them again
            migrationBuilder.Sql(@"
INSERT INTO ""FCT_DISBURSEMENT_CHEQUES"" (""DISBURSEMENT_ID"", ""BATCH_ID"", ""CHEQUE_NO"", ""CHEQUE_DATE"", ""DUE_DATE"", ""ISSUER_BANK_NAME"",
    ""ISSUER_BANK_CODE"", ""ISSUER_BANK_BRANCH"", ""DRAWER_NAME_EN"", ""DRAWER_NAME_AR"", ""DRAWER_NATIONAL_ID"", ""DRAWER_ACCOUNT_NO"",
    ""CHEQUE_AMOUNT"", ""CURRENCY_CODE"", ""CUSTOMER_ID"", ""CUSTOMER_NAME_EN"", ""CUSTOMER_NAME_AR"", ""DEPOSIT_BANK_ACCOUNT_ID"",
    ""RECEIVED_DATE"", ""DEPOSIT_DATE"", ""STATUS_CODE"", ""NOTES"", ""WALLET_CHEQUE_ID"", ""CREATED_BY"", ""CREATED_AT"", ""ROW_VERSION"")
SELECT d.""DISBURSEMENT_ID"", c.""BATCH_ID"", c.""CHEQUE_NO"", c.""CHEQUE_AT""::date, c.""DUE_DATE"", c.""ISSUER_BANK_NAME"",
       c.""ISSUER_BANK_CODE"", c.""ISSUER_BANK_BRANCH"", c.""DRAWER_NAME_EN"", c.""DRAWER_NAME_AR"", c.""DRAWER_NATIONAL_ID"", c.""DRAWER_ACCOUNT_NO"",
       c.""CHEQUE_AMOUNT"", c.""CURRENCY_CODE"", c.""CUSTOMER_ID"", c.""CUSTOMER_NAME_EN"", c.""CUSTOMER_NAME_AR"", c.""DEPOSIT_BANK_ACCOUNT_ID"",
       c.""RECEIVED_AT""::date, c.""DEPOSIT_AT""::date,
       CASE WHEN c.""STATUS_CODE"" IN ('CANCELLED', 'REPLACED') THEN 'CANCELLED' ELSE 'IN_WALLET' END, c.""NOTES"", c.""CHEQUE_ID"",
       'MIGRATION', now(), 1
  FROM ""CHQ_CHEQUES"" c
  JOIN ""FCT_DISBURSEMENTS"" d ON d.""DISBURSEMENT_ID""::text = c.""SOURCE_REF_ID""
 WHERE c.""SOURCE_REF_TYPE"" = 'DISBURSEMENT'
   AND NOT EXISTS (SELECT 1 FROM ""FCT_DISBURSEMENT_CHEQUES"" x WHERE x.""WALLET_CHEQUE_ID"" = c.""CHEQUE_ID"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENT_CHARGE_SOURCES");

            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENT_CHEQUES");
        }
    }
}
