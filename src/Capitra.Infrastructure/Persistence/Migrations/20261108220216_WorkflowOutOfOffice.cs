using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowOutOfOffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WFE_OUT_OF_OFFICE_ENTRIES",
                columns: table => new
                {
                    OUT_OF_OFFICE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DELEGATE_TO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WORKFLOW_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TO_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    NOTE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_OUT_OF_OFFICE_ENTRIES", x => x.OUT_OF_OFFICE_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_OUT_OF_OFFICE_ENTRIES_DELEGATE_TO",
                table: "WFE_OUT_OF_OFFICE_ENTRIES",
                column: "DELEGATE_TO");

            migrationBuilder.CreateIndex(
                name: "IX_WFE_OUT_OF_OFFICE_ENTRIES_USER_NAME_IS_ACTIVE",
                table: "WFE_OUT_OF_OFFICE_ENTRIES",
                columns: new[] { "USER_NAME", "IS_ACTIVE" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WFE_OUT_OF_OFFICE_ENTRIES");
        }
    }
}
