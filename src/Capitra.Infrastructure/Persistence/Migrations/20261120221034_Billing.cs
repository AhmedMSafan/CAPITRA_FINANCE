using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BILLING_CYCLE_CODE",
                table: "SCR_MEMBER_COMPANIES",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "MONTHLY");

            migrationBuilder.AddColumn<long>(
                name: "PRICE_PLAN_ID",
                table: "SCR_MEMBER_COMPANIES",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SCR_PRICE_PLANS",
                columns: table => new
                {
                    PRICE_PLAN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PLAN_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    USER_MONTHLY_FEE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INQUIRY_FEE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_PRICE_PLANS", x => x.PRICE_PLAN_ID);
                });

            migrationBuilder.CreateTable(
                name: "SCR_USER_ACTIVITY",
                columns: table => new
                {
                    USER_ACTIVITY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    USER_ID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ACTIVE_FROM = table.Column<DateOnly>(type: "date", nullable: false),
                    ACTIVE_TO = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_USER_ACTIVITY", x => x.USER_ACTIVITY_ID);
                    table.ForeignKey(
                        name: "FK_SCR_USER_ACTIVITY_SCR_MEMBER_COMPANIES",
                        column: x => x.MEMBER_COMPANY_ID,
                        principalTable: "SCR_MEMBER_COMPANIES",
                        principalColumn: "MEMBER_COMPANY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_INVOICES",
                columns: table => new
                {
                    INVOICE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INVOICE_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    PERIOD_FROM = table.Column<DateOnly>(type: "date", nullable: false),
                    PERIOD_TO = table.Column<DateOnly>(type: "date", nullable: false),
                    PRICE_PLAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    PLAN_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    USER_MONTHLY_FEE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INQUIRY_FEE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    USER_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    USER_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INQUIRY_COUNT = table.Column<int>(type: "integer", nullable: false),
                    INQUIRY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ISSUED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ISSUED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CANCELLED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CANCELLED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CANCEL_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_INVOICES", x => x.INVOICE_ID);
                    table.ForeignKey(
                        name: "FK_SCR_INVOICES_SCR_MEMBER_COMPANIES",
                        column: x => x.MEMBER_COMPANY_ID,
                        principalTable: "SCR_MEMBER_COMPANIES",
                        principalColumn: "MEMBER_COMPANY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCR_INVOICES_SCR_PRICE_PLANS",
                        column: x => x.PRICE_PLAN_ID,
                        principalTable: "SCR_PRICE_PLANS",
                        principalColumn: "PRICE_PLAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_INVOICE_LINES",
                columns: table => new
                {
                    INVOICE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INVOICE_ID = table.Column<long>(type: "bigint", nullable: false),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MONTHS_TEXT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    USER_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    USER_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INQUIRY_COUNT = table.Column<int>(type: "integer", nullable: false),
                    INQUIRY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LINE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_INVOICE_LINES", x => x.INVOICE_LINE_ID);
                    table.ForeignKey(
                        name: "FK_SCR_INVOICE_LINES_SCR_INVOICES",
                        column: x => x.INVOICE_ID,
                        principalTable: "SCR_INVOICES",
                        principalColumn: "INVOICE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_MEMBER_COMPANIES_PRICE_PLAN_ID",
                table: "SCR_MEMBER_COMPANIES",
                column: "PRICE_PLAN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_INVOICE_LINES_INVOICE_ID_LINE_NO",
                table: "SCR_INVOICE_LINES",
                columns: new[] { "INVOICE_ID", "LINE_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INVOICES_MEMBER_COMPANY_ID_PERIOD_FROM",
                table: "SCR_INVOICES",
                columns: new[] { "MEMBER_COMPANY_ID", "PERIOD_FROM" });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INVOICES_PRICE_PLAN_ID",
                table: "SCR_INVOICES",
                column: "PRICE_PLAN_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_INVOICES_INVOICE_NO",
                table: "SCR_INVOICES",
                column: "INVOICE_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_PRICE_PLANS_PLAN_CODE",
                table: "SCR_PRICE_PLANS",
                column: "PLAN_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_USER_ACTIVITY_MEMBER_COMPANY_ID_ACTIVE_FROM",
                table: "SCR_USER_ACTIVITY",
                columns: new[] { "MEMBER_COMPANY_ID", "ACTIVE_FROM" });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_USER_ACTIVITY_USER_ID",
                table: "SCR_USER_ACTIVITY",
                column: "USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SCR_MEMBER_COMPANIES_SCR_PRICE_PLANS",
                table: "SCR_MEMBER_COMPANIES",
                column: "PRICE_PLAN_ID",
                principalTable: "SCR_PRICE_PLANS",
                principalColumn: "PRICE_PLAN_ID",
                onDelete: ReferentialAction.Restrict);

            // Billing history starts with the company accounts usable today (active, activated): each from the day it was made
            // (Cairo date). From now on the application opens and closes these periods itself. Nothing is changed or deleted.
            migrationBuilder.Sql("""
                INSERT INTO "SCR_USER_ACTIVITY" ("MEMBER_COMPANY_ID", "USER_ID", "USER_NAME", "ACTIVE_FROM", "ACTIVE_TO", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT u."MEMBER_COMPANY_ID", u."USER_ID", u."USER_NAME", (u."CREATED_AT" AT TIME ZONE 'Africa/Cairo')::date, NULL, 'SYSTEM', now(), 0
                  FROM "SEC_USERS" u
                 WHERE u."MEMBER_COMPANY_ID" IS NOT NULL AND u."IS_ACTIVE" AND u."PASSWORD_HASH" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SCR_MEMBER_COMPANIES_SCR_PRICE_PLANS",
                table: "SCR_MEMBER_COMPANIES");

            migrationBuilder.DropTable(
                name: "SCR_INVOICE_LINES");

            migrationBuilder.DropTable(
                name: "SCR_USER_ACTIVITY");

            migrationBuilder.DropTable(
                name: "SCR_INVOICES");

            migrationBuilder.DropTable(
                name: "SCR_PRICE_PLANS");

            migrationBuilder.DropIndex(
                name: "IX_SCR_MEMBER_COMPANIES_PRICE_PLAN_ID",
                table: "SCR_MEMBER_COMPANIES");

            migrationBuilder.DropColumn(
                name: "BILLING_CYCLE_CODE",
                table: "SCR_MEMBER_COMPANIES");

            migrationBuilder.DropColumn(
                name: "PRICE_PLAN_ID",
                table: "SCR_MEMBER_COMPANIES");
        }
    }
}
