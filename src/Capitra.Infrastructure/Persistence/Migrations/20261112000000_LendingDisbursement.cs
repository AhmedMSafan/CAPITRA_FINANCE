using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingDisbursement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_DISBURSEMENTS",
                columns: table => new
                {
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAID_FROM_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PAYOUT_METHOD_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAYEE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PAYEE_ACCOUNT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NET_PAYOUT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAID_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    REQUESTED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DECIDED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DECIDED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DECISION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_DISBURSEMENTS", x => x.DISBURSEMENT_ID);
                    table.ForeignKey(
                        name: "FK_LND_DISBURSEMENTS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LND_DISBURSEMENTS_TRS_BANK_ACCOUNTS",
                        column: x => x.PAID_FROM_ACCOUNT_ID,
                        principalTable: "TRS_BANK_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LND_DISBURSEMENT_CHARGES",
                columns: table => new
                {
                    DISBURSEMENT_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHARGE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHARGE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CHARGE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INCOME_RECOGNITION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_DISBURSEMENT_CHARGES", x => x.DISBURSEMENT_CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_LND_DISBURSEMENT_CHARGES_LND_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "LND_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LND_LOAN_EVENTS",
                columns: table => new
                {
                    LOAN_EVENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EVENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EVENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SOURCE_DOCUMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SOURCE_DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LOAN_EVENTS", x => x.LOAN_EVENT_ID);
                    table.ForeignKey(
                        name: "FK_LND_LOAN_EVENTS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_DISBURSEMENTS_LOAN_ID_STATUS_CODE",
                table: "LND_DISBURSEMENTS",
                columns: new[] { "LOAN_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_LND_DISBURSEMENTS_PAID_FROM_ACCOUNT_ID",
                table: "LND_DISBURSEMENTS",
                column: "PAID_FROM_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_DISBURSEMENTS_STATUS_CODE",
                table: "LND_DISBURSEMENTS",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_LND_DISBURSEMENT_CHARGES_DISBURSEMENT_ID",
                table: "LND_DISBURSEMENT_CHARGES",
                column: "DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOAN_EVENTS_GL_TRAN_SET_ID",
                table: "LND_LOAN_EVENTS",
                column: "GL_TRAN_SET_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOAN_EVENTS_LOAN_ID_EVENT_DATE",
                table: "LND_LOAN_EVENTS",
                columns: new[] { "LOAN_ID", "EVENT_DATE" });

            // the GL transaction set IDs of Retail Lending postings, CAP-LND-yyyy-nnnnnnnn
            migrationBuilder.Sql("""
                INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT 'LND', 2026, 'LND', 'Retail Lending', 'التمويل للأفراد', 0, 8, true, 'SYSTEM', now(), 1
                WHERE NOT EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = 'LND');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_DISBURSEMENT_CHARGES");

            migrationBuilder.DropTable(
                name: "LND_LOAN_EVENTS");

            migrationBuilder.DropTable(
                name: "LND_DISBURSEMENTS");
        }
    }
}
