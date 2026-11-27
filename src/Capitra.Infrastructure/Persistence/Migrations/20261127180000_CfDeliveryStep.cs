using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CF_FINANCE step 5, Delivery and customer receipt (Ahmed, 27 Nov 2026): the delivery of a consumer finance (date, delivery note,
    /// serial, GL of its booking, the supplier payment due date) on LND_LOANS, all nullable, and LND_LOAN_DOCUMENTS, a finance's
    /// documents (the signed delivery note), kept like the supplier documents.
    /// </summary>
    public partial class CfDeliveryStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_LOAN_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DOCUMENT_TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DOCUMENT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FILE_NAME = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FILE_SIZE_BYTES = table.Column<long>(type: "bigint", nullable: false),
                    SHA256_HASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LOAN_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_LND_LOAN_DOCUMENTS_LND_LOANS_LOAN_ID",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOAN_DOCUMENTS_LOAN_ID",
                table: "LND_LOAN_DOCUMENTS",
                column: "LOAN_ID");

            migrationBuilder.AddColumn<DateOnly>(name: "DELIVERY_DATE", table: "LND_LOANS", type: "date", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DELIVERY_NOTE_NO", table: "LND_LOANS", type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "DELIVERY_SERIAL_NO", table: "LND_LOANS", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "DELIVERY_GL_TRAN_SET_ID", table: "LND_LOANS", type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<DateOnly>(name: "SUPPLIER_PAYMENT_DUE_DATE", table: "LND_LOANS", type: "date", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var c in new[] { "DELIVERY_DATE", "DELIVERY_NOTE_NO", "DELIVERY_SERIAL_NO", "DELIVERY_GL_TRAN_SET_ID", "SUPPLIER_PAYMENT_DUE_DATE" })
                migrationBuilder.DropColumn(name: c, table: "LND_LOANS");
            migrationBuilder.DropTable(name: "LND_LOAN_DOCUMENTS");
        }
    }
}
