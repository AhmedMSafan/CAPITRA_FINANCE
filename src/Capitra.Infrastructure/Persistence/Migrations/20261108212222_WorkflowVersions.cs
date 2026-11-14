using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DEFINITION_VERSION_ID",
                table: "WFE_WORKFLOW_INSTANCES",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WFE_DEFINITION_VERSIONS",
                columns: table => new
                {
                    DEFINITION_VERSION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_DEFINITION_ID = table.Column<long>(type: "bigint", nullable: false),
                    VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    BPMN_XML = table.Column<string>(type: "text", nullable: false),
                    RELEASED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RELEASED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_DEFINITION_VERSIONS", x => x.DEFINITION_VERSION_ID);
                    table.ForeignKey(
                        name: "FK_WFE_DEFINITION_VERSIONS_WFE_WORKFLOW_DEFINITIONS",
                        column: x => x.WORKFLOW_DEFINITION_ID,
                        principalTable: "WFE_WORKFLOW_DEFINITIONS",
                        principalColumn: "WORKFLOW_DEFINITION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_WORKFLOW_INSTANCES_DEFINITION_VERSION_ID",
                table: "WFE_WORKFLOW_INSTANCES",
                column: "DEFINITION_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_DEFINITION_VERSIONS_WORKFLOW_DEFINITION_ID_VERSION_NO",
                table: "WFE_DEFINITION_VERSIONS",
                columns: new[] { "WORKFLOW_DEFINITION_ID", "VERSION_NO" },
                unique: true);

            // every workflow's BPMN as it is today becomes its first kept version, and the instances already there run on it,
            // so a later change of the workflow no longer moves them
            migrationBuilder.Sql("""
                INSERT INTO "WFE_DEFINITION_VERSIONS" ("WORKFLOW_DEFINITION_ID", "VERSION_NO", "BPMN_XML", "RELEASED_AT", "RELEASED_BY",
                    "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                SELECT d."WORKFLOW_DEFINITION_ID",
                       CASE WHEN d."VERSION_NO" ~ '^[0-9]{1,6}$' AND d."VERSION_NO"::int > 0 THEN d."VERSION_NO"::int ELSE 1 END,
                       d."BPMN_XML", COALESCE(d."UPDATED_AT", d."CREATED_AT"), COALESCE(d."UPDATED_BY", d."CREATED_BY"),
                       'SYSTEM', now(), 1
                FROM "WFE_WORKFLOW_DEFINITIONS" d
                WHERE NOT EXISTS (SELECT 1 FROM "WFE_DEFINITION_VERSIONS" v WHERE v."WORKFLOW_DEFINITION_ID" = d."WORKFLOW_DEFINITION_ID");
                UPDATE "WFE_WORKFLOW_DEFINITIONS" d
                SET "VERSION_NO" = v."VERSION_NO"::text
                FROM "WFE_DEFINITION_VERSIONS" v WHERE v."WORKFLOW_DEFINITION_ID" = d."WORKFLOW_DEFINITION_ID";
                UPDATE "WFE_WORKFLOW_INSTANCES" i
                SET "DEFINITION_VERSION_ID" = v."DEFINITION_VERSION_ID"
                FROM "WFE_DEFINITION_VERSIONS" v
                WHERE v."WORKFLOW_DEFINITION_ID" = i."WORKFLOW_DEFINITION_ID" AND i."DEFINITION_VERSION_ID" IS NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_WFE_WORKFLOW_INSTANCES_WFE_DEFINITION_VERSIONS",
                table: "WFE_WORKFLOW_INSTANCES",
                column: "DEFINITION_VERSION_ID",
                principalTable: "WFE_DEFINITION_VERSIONS",
                principalColumn: "DEFINITION_VERSION_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WFE_WORKFLOW_INSTANCES_WFE_DEFINITION_VERSIONS",
                table: "WFE_WORKFLOW_INSTANCES");

            migrationBuilder.DropTable(
                name: "WFE_DEFINITION_VERSIONS");

            migrationBuilder.DropIndex(
                name: "IX_WFE_WORKFLOW_INSTANCES_DEFINITION_VERSION_ID",
                table: "WFE_WORKFLOW_INSTANCES");

            migrationBuilder.DropColumn(
                name: "DEFINITION_VERSION_ID",
                table: "WFE_WORKFLOW_INSTANCES");
        }
    }
}
