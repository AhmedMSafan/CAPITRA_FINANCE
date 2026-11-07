using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReverseSupplierTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TRS_SUPPLIER_TRANSFERS",
                columns: table => new
                {
                    TRANSFER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    FUNDING_ID = table.Column<long>(type: "bigint", nullable: false),
                    SUPPLIER_CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUPPLIER_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TAX_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SUPPLIER_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUPPLIER_BANK_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SUPPLIER_IBAN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SUPPLIER_ACCOUNT_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAID_FROM_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SENT_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BANK_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CONFIRMED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CONFIRMED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_SUPPLIER_TRANSFERS", x => x.TRANSFER_ID);
                    table.ForeignKey(
                        name: "FK_TRS_SUPPLIER_TRANSFERS_CRM_CLIENTS",
                        column: x => x.SUPPLIER_CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUPPLIER_TRANSFERS_CRM_CLIENT_BANK_ACCOUNTS",
                        column: x => x.SUPPLIER_BANK_ACCOUNT_ID,
                        principalTable: "CRM_CLIENT_BANK_ACCOUNTS",
                        principalColumn: "BANK_ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUPPLIER_TRANSFERS_FCT_DISBURSEMENTS",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUPPLIER_TRANSFERS_TRS_BANK_ACCOUNTS",
                        column: x => x.PAID_FROM_ACCOUNT_ID,
                        principalTable: "TRS_BANK_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUPPLIER_TRANSFERS_TRS_DISBURSEMENT_FUNDINGS",
                        column: x => x.FUNDING_ID,
                        principalTable: "TRS_DISBURSEMENT_FUNDINGS",
                        principalColumn: "FUNDING_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUPPLIER_TRANSFERS_DISBURSEMENT_ID_STATUS_CODE",
                table: "TRS_SUPPLIER_TRANSFERS",
                columns: new[] { "DISBURSEMENT_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUPPLIER_TRANSFERS_FUNDING_ID",
                table: "TRS_SUPPLIER_TRANSFERS",
                column: "FUNDING_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUPPLIER_TRANSFERS_PAID_FROM_ACCOUNT_ID",
                table: "TRS_SUPPLIER_TRANSFERS",
                column: "PAID_FROM_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUPPLIER_TRANSFERS_SUPPLIER_BANK_ACCOUNT_ID",
                table: "TRS_SUPPLIER_TRANSFERS",
                column: "SUPPLIER_BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUPPLIER_TRANSFERS_SUPPLIER_CLIENT_ID",
                table: "TRS_SUPPLIER_TRANSFERS",
                column: "SUPPLIER_CLIENT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TRS_SUPPLIER_TRANSFERS");
        }
    }
}
