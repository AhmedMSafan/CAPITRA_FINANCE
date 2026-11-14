using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MtgDocumentRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTG_LKP_DOCUMENT_REQUIREMENTS",
                columns: table => new
                {
                    REQUIREMENT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IS_MANDATORY = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ORIGINAL_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_VERIFIED_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    MAX_AGE_DAYS = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_DOCUMENT_REQUIREMENTS", x => x.REQUIREMENT_CODE);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_LKP_DOCUMENT_REQUIREMENTS");
        }
    }
}
