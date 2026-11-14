using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowFormsAndDataSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WFE_DATA_SOURCES",
                columns: table => new
                {
                    DATA_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SOURCE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SOURCE_NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_DATA_SOURCES", x => x.DATA_SOURCE_ID);
                });

            migrationBuilder.CreateTable(
                name: "WFE_FORMS",
                columns: table => new
                {
                    FORM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FORM_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FORM_NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    WORKFLOW_DEFINITION_ID = table.Column<long>(type: "bigint", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LAYOUT_JSON = table.Column<string>(type: "text", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_FORMS", x => x.FORM_ID);
                    table.ForeignKey(
                        name: "FK_WFE_FORMS_WFE_WORKFLOW_DEFINITIONS",
                        column: x => x.WORKFLOW_DEFINITION_ID,
                        principalTable: "WFE_WORKFLOW_DEFINITIONS",
                        principalColumn: "WORKFLOW_DEFINITION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WFE_DATA_SOURCE_ITEMS",
                columns: table => new
                {
                    DATA_SOURCE_ITEM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DATA_SOURCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    ITEM_VALUE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ITEM_LABEL = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_DATA_SOURCE_ITEMS", x => x.DATA_SOURCE_ITEM_ID);
                    table.ForeignKey(
                        name: "FK_WFE_DATA_SOURCE_ITEMS_WFE_DATA_SOURCES",
                        column: x => x.DATA_SOURCE_ID,
                        principalTable: "WFE_DATA_SOURCES",
                        principalColumn: "DATA_SOURCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_DATA_SOURCE_ITEMS_DATA_SOURCE_ID_ITEM_VALUE",
                table: "WFE_DATA_SOURCE_ITEMS",
                columns: new[] { "DATA_SOURCE_ID", "ITEM_VALUE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_DATA_SOURCES_SOURCE_CODE",
                table: "WFE_DATA_SOURCES",
                column: "SOURCE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WFE_FORMS_WORKFLOW_DEFINITION_ID",
                table: "WFE_FORMS",
                column: "WORKFLOW_DEFINITION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_FORMS_FORM_CODE",
                table: "WFE_FORMS",
                column: "FORM_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WFE_DATA_SOURCE_ITEMS");

            migrationBuilder.DropTable(
                name: "WFE_FORMS");

            migrationBuilder.DropTable(
                name: "WFE_DATA_SOURCES");
        }
    }
}
