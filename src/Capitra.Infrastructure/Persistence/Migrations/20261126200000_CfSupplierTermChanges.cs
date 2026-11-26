using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CRM_CF_SUPPLIER_TERM_CHANGES (Ahmed, 26 Nov 2026): changes to an active supplier's terms made without a new approval
    /// (CRM.CF_SUPPLIER.AMEND, ADMIN), one row per field, with the reason.
    /// </summary>
    public partial class CfSupplierTermChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_TERM_CHANGES",
                columns: table => new
                {
                    TERM_CHANGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    FIELD_NAME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NEW_VALUE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OLD_VALUE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_TERM_CHANGES", x => x.TERM_CHANGE_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_TERM_CHANGES_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_TERM_CHANGES_APPLICATION_ID",
                table: "CRM_CF_SUPPLIER_TERM_CHANGES",
                column: "APPLICATION_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_TERM_CHANGES");
        }
    }
}
