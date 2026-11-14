using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FctAgreementDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_LKP_DOCUMENT_STATUS_CODES",
                columns: table => new
                {
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STATUS_CODE_VALUE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    STATUS_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_DOCUMENT_STATUS_CODES", x => x.STATUS_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_AGREEMENT_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DOC_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    DOCUMENT_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DOCUMENT_TITLE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DOCUMENT_TITLE_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DOCUMENT_DESC = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DOCUMENT_DESC_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FILE_EXTENSION = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FILE_SIZE_BYTES = table.Column<int>(type: "integer", nullable: true),
                    MIME_TYPE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FILE_CONTENT = table.Column<byte[]>(type: "bytea", nullable: true),
                    FILE_PATH = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    STORAGE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ISSUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXECUTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTARIZATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTARIZATION_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VERSION_NUMBER = table.Column<int>(type: "integer", nullable: true),
                    IS_LATEST_VERSION = table.Column<bool>(type: "boolean", nullable: false),
                    PREVIOUS_VERSION_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_EXECUTED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ORIGINAL_RECEIVED = table.Column<bool>(type: "boolean", nullable: false),
                    ORIGINAL_RECEIVED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VERIFICATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_CONFIDENTIAL = table.Column<bool>(type: "boolean", nullable: false),
                    REMARKS = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REMARKS_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_AGREEMENT_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DOCUMENTS_ADM_LKP_DOCUMENT_STATUS_CODES",
                        column: x => x.STATUS_ID,
                        principalTable: "ADM_LKP_DOCUMENT_STATUS_CODES",
                        principalColumn: "STATUS_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DOCUMENTS_ADM_LKP_DOCUMENT_TYPES",
                        column: x => x.DOC_TYPE_ID,
                        principalTable: "ADM_LKP_DOCUMENT_TYPES",
                        principalColumn: "DOCUMENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DOCUMENTS_FCT_AGREEMENTS",
                        column: x => x.AGREEMENT_ID,
                        principalTable: "FCT_AGREEMENTS",
                        principalColumn: "AGREEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_AGREEMENT_DOCUMENTS_FCT_AGREEMENT_DOCUMENTS",
                        column: x => x.PREVIOUS_VERSION_ID,
                        principalTable: "FCT_AGREEMENT_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_LKP_DOCUMENT_STATUS_CODES_STATUS_CODE_VALUE",
                table: "ADM_LKP_DOCUMENT_STATUS_CODES",
                column: "STATUS_CODE_VALUE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DOCUMENTS_AGREEMENT_ID",
                table: "FCT_AGREEMENT_DOCUMENTS",
                column: "AGREEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DOCUMENTS_DOC_TYPE_ID",
                table: "FCT_AGREEMENT_DOCUMENTS",
                column: "DOC_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DOCUMENTS_EXPIRY_DATE",
                table: "FCT_AGREEMENT_DOCUMENTS",
                column: "EXPIRY_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DOCUMENTS_PREVIOUS_VERSION_ID",
                table: "FCT_AGREEMENT_DOCUMENTS",
                column: "PREVIOUS_VERSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_AGREEMENT_DOCUMENTS_STATUS_ID",
                table: "FCT_AGREEMENT_DOCUMENTS",
                column: "STATUS_ID");

            // FINOVA_FINANCEING's document statuses (in its order, so a new table gives them its ids 1 to 3) and its four factoring document types. The types go into
            // the shared ADM_LKP_DOCUMENT_TYPES under GROUP_CODE FACTORING, so the factoring pages list only these.
            migrationBuilder.Sql("""
                INSERT INTO "ADM_LKP_DOCUMENT_STATUS_CODES" ("STATUS_CODE_VALUE", "STATUS_NAME", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('RECEIVED', 'Received', 'Document received', TRUE, 'SYSTEM', now(), 0),
                       ('VERIFIED', 'Verified', 'Document verified', TRUE, 'SYSTEM', now(), 0),
                       ('REJECTED', 'Rejected', 'Document rejected', TRUE, 'SYSTEM', now(), 0)
                ON CONFLICT DO NOTHING;

                INSERT INTO "ADM_LKP_DOCUMENT_TYPES" ("DOCUMENT_TYPE_CODE", "DOCUMENT_TYPE_NAME", "IS_MANDATORY", "SORT_ORDER", "IS_ACTIVE", "IS_EXPIRY_REQUIRED", "GROUP_CODE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('AGREEMENT', 'Agreement', FALSE, 1, TRUE, FALSE, 'FACTORING', 'SYSTEM', now(), 0),
                       ('SUPPORTING_DOCUMENT', 'Supporting Document', FALSE, 2, TRUE, FALSE, 'FACTORING', 'SYSTEM', now(), 0),
                       ('FINANCIAL_STATEMENT', 'Financial Statement', FALSE, 3, TRUE, FALSE, 'FACTORING', 'SYSTEM', now(), 0),
                       ('OTHER', 'Other', FALSE, 4, TRUE, FALSE, 'FACTORING', 'SYSTEM', now(), 0)
                ON CONFLICT ("DOCUMENT_TYPE_CODE") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "ADM_LKP_DOCUMENT_TYPES" WHERE "GROUP_CODE" = 'FACTORING' AND "DOCUMENT_TYPE_CODE" IN ('AGREEMENT', 'SUPPORTING_DOCUMENT', 'FINANCIAL_STATEMENT', 'OTHER');""");

            migrationBuilder.DropTable(
                name: "FCT_AGREEMENT_DOCUMENTS");

            migrationBuilder.DropTable(
                name: "ADM_LKP_DOCUMENT_STATUS_CODES");
        }
    }
}
