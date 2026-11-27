using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// An active supplier's terms change in the same style as the lending terms (Ahmed, 27 Nov 2026): an admin asks for the change
    /// (CRM_CF_SUPPLIER_CHANGE_REQUESTS), another admin approves it, and only then it applies. CRM_CF_SUPPLIER_TERM_CHANGES keeps
    /// the audit trail and now notes the request and who approved; rows made before (applied directly) keep these empty.
    /// </summary>
    public partial class CfSupplierChangeApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_CF_SUPPLIER_CHANGE_REQUESTS",
                columns: table => new
                {
                    CHANGE_REQUEST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APPLICATION_ID = table.Column<long>(type: "bigint", nullable: false),
                    TERMS_JSON = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CHANGES_JSON = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DECIDED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_CRM_CF_SUPPLIER_CHANGE_REQUESTS", x => x.CHANGE_REQUEST_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CF_SUPPLIER_CHANGE_REQUESTS_CRM_CF_SUPPLIER_APPLICATIONS",
                        column: x => x.APPLICATION_ID,
                        principalTable: "CRM_CF_SUPPLIER_APPLICATIONS",
                        principalColumn: "APPLICATION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CF_SUPPLIER_CHANGE_REQUESTS_APPLICATION_ID",
                table: "CRM_CF_SUPPLIER_CHANGE_REQUESTS",
                column: "APPLICATION_ID");

            migrationBuilder.AddColumn<long>(name: "CHANGE_REQUEST_ID", table: "CRM_CF_SUPPLIER_TERM_CHANGES", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "APPROVED_BY", table: "CRM_CF_SUPPLIER_TERM_CHANGES", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "APPROVED_AT", table: "CRM_CF_SUPPLIER_TERM_CHANGES", type: "timestamp with time zone", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CHANGE_REQUEST_ID", table: "CRM_CF_SUPPLIER_TERM_CHANGES");
            migrationBuilder.DropColumn(name: "APPROVED_BY", table: "CRM_CF_SUPPLIER_TERM_CHANGES");
            migrationBuilder.DropColumn(name: "APPROVED_AT", table: "CRM_CF_SUPPLIER_TERM_CHANGES");
            migrationBuilder.DropTable(name: "CRM_CF_SUPPLIER_CHANGE_REQUESTS");
        }
    }
}
