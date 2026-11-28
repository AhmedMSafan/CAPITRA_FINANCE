using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScreeningStepDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SCR_DECISION_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DOCUMENT_TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FILE_SIZE_BYTES = table.Column<long>(type: "bigint", nullable: false),
                    SHA256_HASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_DECISION_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_SCR_DECISION_DOCUMENTS_SCR_DECISIONS",
                        column: x => x.DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_DECISION_STEP_NOTES",
                columns: table => new
                {
                    STEP_NOTE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STEP_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    COMMENT_TEXT = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DONE_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DONE_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_DECISION_STEP_NOTES", x => x.STEP_NOTE_ID);
                    table.ForeignKey(
                        name: "FK_SCR_DECISION_STEP_NOTES_SCR_DECISIONS",
                        column: x => x.DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISION_DOCUMENTS_DECISION_ID_STEP_CODE",
                table: "SCR_DECISION_DOCUMENTS",
                columns: new[] { "DECISION_ID", "STEP_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISION_STEP_NOTES_DECISION_ID",
                table: "SCR_DECISION_STEP_NOTES",
                column: "DECISION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_DECISION_STEP_NOTES_STEP_INSTANCE_ID",
                table: "SCR_DECISION_STEP_NOTES",
                column: "STEP_INSTANCE_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SCR_DECISION_DOCUMENTS");

            migrationBuilder.DropTable(
                name: "SCR_DECISION_STEP_NOTES");
        }
    }
}
