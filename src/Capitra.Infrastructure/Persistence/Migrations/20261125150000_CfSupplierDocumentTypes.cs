using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer Finance supplier documents (Ahmed, 25 Nov 2026): one list per application, added at any time, each with an optional
    /// type. The site visit needs a SITE_VISIT_REPORT and contract signing a SIGNED_AGREEMENT among them.
    /// </summary>
    public partial class CfSupplierDocumentTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DOCUMENT_TYPE_CODE",
                table: "CRM_CF_SUPPLIER_DOCUMENTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DOCUMENT_TYPE_CODE", table: "CRM_CF_SUPPLIER_DOCUMENTS");
        }
    }
}
