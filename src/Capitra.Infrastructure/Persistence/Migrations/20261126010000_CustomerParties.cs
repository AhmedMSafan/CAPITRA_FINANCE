using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// One place for KYC (Ahmed, 26 Nov 2026): the owners, managers and authorised signatories belong to the customer
    /// (CRM_CLIENT_PARTIES, Customer 360 > Compliance) instead of each Consumer Finance supplier application. The existing ones are
    /// copied with their screening, and CRM_CF_SUPPLIER_PARTIES is dropped.
    /// </summary>
    public partial class CustomerParties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_CLIENT_PARTIES",
                columns: table => new
                {
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NATIONAL_ID_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NATIONALITY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OWNERSHIP_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PARTY_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SCREENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SCREENED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SCREENING_RESULT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENT_PARTIES", x => x.PARTY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENT_PARTIES_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENT_PARTIES_CLIENT_ID",
                table: "CRM_CLIENT_PARTIES",
                column: "CLIENT_ID");

            // the owners and signatories entered on supplier applications become the customer's
            migrationBuilder.Sql(@"
INSERT INTO ""CRM_CLIENT_PARTIES"" (""CLIENT_ID"", ""PARTY_TYPE_CODE"", ""FULL_NAME"", ""NATIONAL_ID_NO"", ""NATIONALITY"", ""OWNERSHIP_PCT"",
    ""SCREENING_RESULT_CODE"", ""SCREENED_BY"", ""SCREENED_AT"", ""CREATED_AT"", ""CREATED_BY"", ""UPDATED_AT"", ""UPDATED_BY"", ""ROW_VERSION"")
SELECT a.""CLIENT_ID"", p.""PARTY_TYPE_CODE"", p.""FULL_NAME"", p.""NATIONAL_ID_NO"", p.""NATIONALITY"", p.""OWNERSHIP_PCT"",
    p.""SCREENING_RESULT_CODE"", p.""SCREENED_BY"", p.""SCREENED_AT"", p.""CREATED_AT"", p.""CREATED_BY"", p.""UPDATED_AT"", p.""UPDATED_BY"", 0
FROM ""CRM_CF_SUPPLIER_PARTIES"" p
JOIN ""CRM_CF_SUPPLIER_APPLICATIONS"" a ON a.""APPLICATION_ID"" = p.""APPLICATION_ID""
ORDER BY p.""PARTY_ID"";");

            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_PARTIES");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_PARTIES",
                columns: table => new
                {
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NATIONAL_ID_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NATIONALITY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OWNERSHIP_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PARTY_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SCREENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SCREENED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SCREENING_RESULT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_PARTIES", x => x.PARTY_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_PARTIES_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_PARTIES_APPLICATION_ID",
                table: "CRM_CF_SUPPLIER_PARTIES",
                column: "APPLICATION_ID");

            migrationBuilder.DropTable(name: "CRM_CLIENT_PARTIES");
        }
    }
}
