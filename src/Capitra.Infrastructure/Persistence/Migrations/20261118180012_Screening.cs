using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Screening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MEMBER_COMPANY_ID",
                table: "SEC_USERS",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SCR_LETTERS",
                columns: table => new
                {
                    LETTER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LETTER_NO = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SENDER_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SENDER_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CHANNEL_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LETTER_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RECEIVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_LETTERS", x => x.LETTER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SCR_MEMBER_COMPANIES",
                columns: table => new
                {
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LICENCE_NO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CONTACT_EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_MEMBER_COMPANIES", x => x.MEMBER_COMPANY_ID);
                });

            migrationBuilder.CreateTable(
                name: "SCR_PARTIES",
                columns: table => new
                {
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PARTY_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IDENTITY_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IDENTITY_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NATIONALITY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    BIRTH_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_PARTIES", x => x.PARTY_ID);
                });

            migrationBuilder.CreateTable(
                name: "SCR_DECISIONS",
                columns: table => new
                {
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DECISION_REF = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OFFICIAL_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DECISION_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PARENT_DECISION_ID = table.Column<long>(type: "bigint", nullable: true),
                    LETTER_ID = table.Column<long>(type: "bigint", nullable: true),
                    SCOPE_TEXT = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EFFECTIVE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RETURN_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ENTERED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ENTERED_BY_PERSON = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    REVIEWED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    REVIEWED_BY_PERSON = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    REVIEWED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    APPROVED_BY_PERSON = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PUBLISHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REGISTRY_VERSION_NO = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_DECISIONS", x => x.DECISION_ID);
                    table.ForeignKey(
                        name: "FK_SCR_DECISIONS_SCR_DECISIONS",
                        column: x => x.PARENT_DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCR_DECISIONS_SCR_LETTERS",
                        column: x => x.LETTER_ID,
                        principalTable: "SCR_LETTERS",
                        principalColumn: "LETTER_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_LETTER_FILES",
                columns: table => new
                {
                    LETTER_FILE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LETTER_ID = table.Column<long>(type: "bigint", nullable: false),
                    FILE_NAME = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FILE_SIZE_BYTES = table.Column<long>(type: "bigint", nullable: false),
                    SHA256_HASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_LETTER_FILES", x => x.LETTER_FILE_ID);
                    table.ForeignKey(
                        name: "FK_SCR_LETTER_FILES_SCR_LETTERS",
                        column: x => x.LETTER_ID,
                        principalTable: "SCR_LETTERS",
                        principalColumn: "LETTER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SCR_INQUIRIES",
                columns: table => new
                {
                    INQUIRY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INQUIRY_REF = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SUBJECT_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SUBJECT_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IDENTITY_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IDENTITY_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BIRTH_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NATIONALITY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    PURPOSE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PURPOSE_TEXT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CASE_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RESULT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REGISTRY_VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    SCREENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_INQUIRIES", x => x.INQUIRY_ID);
                    table.ForeignKey(
                        name: "FK_SCR_INQUIRIES_SCR_MEMBER_COMPANIES",
                        column: x => x.MEMBER_COMPANY_ID,
                        principalTable: "SCR_MEMBER_COMPANIES",
                        principalColumn: "MEMBER_COMPANY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_PARTY_ALIASES",
                columns: table => new
                {
                    PARTY_ALIAS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    ALIAS_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_PARTY_ALIASES", x => x.PARTY_ALIAS_ID);
                    table.ForeignKey(
                        name: "FK_SCR_PARTY_ALIASES_SCR_PARTIES",
                        column: x => x.PARTY_ID,
                        principalTable: "SCR_PARTIES",
                        principalColumn: "PARTY_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SCR_DECISION_PARTIES",
                columns: table => new
                {
                    DECISION_PARTY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_DECISION_PARTIES", x => x.DECISION_PARTY_ID);
                    table.ForeignKey(
                        name: "FK_SCR_DECISION_PARTIES_SCR_DECISIONS",
                        column: x => x.DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SCR_DECISION_PARTIES_SCR_PARTIES",
                        column: x => x.PARTY_ID,
                        principalTable: "SCR_PARTIES",
                        principalColumn: "PARTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_REGISTRY_VERSIONS",
                columns: table => new
                {
                    REGISTRY_VERSION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false),
                    PUBLISHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PUBLISHED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_REGISTRY_VERSIONS", x => x.REGISTRY_VERSION_ID);
                    table.ForeignKey(
                        name: "FK_SCR_REGISTRY_VERSIONS_SCR_DECISIONS",
                        column: x => x.DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCR_INQUIRY_HITS",
                columns: table => new
                {
                    INQUIRY_HIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INQUIRY_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARTY_ID = table.Column<long>(type: "bigint", nullable: false),
                    DECISION_ID = table.Column<long>(type: "bigint", nullable: false),
                    MATCH_BASIS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NAME_SCORE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MATCHED_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ENTRY_STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_INQUIRY_HITS", x => x.INQUIRY_HIT_ID);
                    table.ForeignKey(
                        name: "FK_SCR_INQUIRY_HITS_SCR_DECISIONS",
                        column: x => x.DECISION_ID,
                        principalTable: "SCR_DECISIONS",
                        principalColumn: "DECISION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCR_INQUIRY_HITS_SCR_INQUIRIES",
                        column: x => x.INQUIRY_ID,
                        principalTable: "SCR_INQUIRIES",
                        principalColumn: "INQUIRY_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SCR_INQUIRY_HITS_SCR_PARTIES",
                        column: x => x.PARTY_ID,
                        principalTable: "SCR_PARTIES",
                        principalColumn: "PARTY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USERS_MEMBER_COMPANY_ID",
                table: "SEC_USERS",
                column: "MEMBER_COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISION_PARTIES_PARTY_ID",
                table: "SCR_DECISION_PARTIES",
                column: "PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_DECISION_PARTIES_DECISION_ID_PARTY_ID",
                table: "SCR_DECISION_PARTIES",
                columns: new[] { "DECISION_ID", "PARTY_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISIONS_LETTER_ID",
                table: "SCR_DECISIONS",
                column: "LETTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISIONS_PARENT_DECISION_ID",
                table: "SCR_DECISIONS",
                column: "PARENT_DECISION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISIONS_STATUS_CODE",
                table: "SCR_DECISIONS",
                column: "STATUS_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_DECISIONS_WORKFLOW_INSTANCE_ID",
                table: "SCR_DECISIONS",
                column: "WORKFLOW_INSTANCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_DECISIONS_DECISION_REF",
                table: "SCR_DECISIONS",
                column: "DECISION_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRIES_MEMBER_COMPANY_ID_SCREENED_AT",
                table: "SCR_INQUIRIES",
                columns: new[] { "MEMBER_COMPANY_ID", "SCREENED_AT" });

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_INQUIRIES_INQUIRY_REF",
                table: "SCR_INQUIRIES",
                column: "INQUIRY_REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRY_HITS_DECISION_ID",
                table: "SCR_INQUIRY_HITS",
                column: "DECISION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRY_HITS_INQUIRY_ID",
                table: "SCR_INQUIRY_HITS",
                column: "INQUIRY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRY_HITS_PARTY_ID",
                table: "SCR_INQUIRY_HITS",
                column: "PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_LETTER_FILES_LETTER_ID",
                table: "SCR_LETTER_FILES",
                column: "LETTER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_LETTERS_LETTER_NO",
                table: "SCR_LETTERS",
                column: "LETTER_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_MEMBER_COMPANIES_COMPANY_CODE",
                table: "SCR_MEMBER_COMPANIES",
                column: "COMPANY_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_PARTIES_PARTY_TYPE_CODE_IDENTITY_NO",
                table: "SCR_PARTIES",
                columns: new[] { "PARTY_TYPE_CODE", "IDENTITY_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCR_PARTY_ALIASES_PARTY_ID",
                table: "SCR_PARTY_ALIASES",
                column: "PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_REGISTRY_VERSIONS_DECISION_ID",
                table: "SCR_REGISTRY_VERSIONS",
                column: "DECISION_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_REGISTRY_VERSIONS_VERSION_NO",
                table: "SCR_REGISTRY_VERSIONS",
                column: "VERSION_NO",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USERS_SCR_MEMBER_COMPANIES",
                table: "SEC_USERS",
                column: "MEMBER_COMPANY_ID",
                principalTable: "SCR_MEMBER_COMPANIES",
                principalColumn: "MEMBER_COMPANY_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USERS_SCR_MEMBER_COMPANIES",
                table: "SEC_USERS");

            migrationBuilder.DropTable(
                name: "SCR_DECISION_PARTIES");

            migrationBuilder.DropTable(
                name: "SCR_INQUIRY_HITS");

            migrationBuilder.DropTable(
                name: "SCR_LETTER_FILES");

            migrationBuilder.DropTable(
                name: "SCR_PARTY_ALIASES");

            migrationBuilder.DropTable(
                name: "SCR_REGISTRY_VERSIONS");

            migrationBuilder.DropTable(
                name: "SCR_INQUIRIES");

            migrationBuilder.DropTable(
                name: "SCR_PARTIES");

            migrationBuilder.DropTable(
                name: "SCR_DECISIONS");

            migrationBuilder.DropTable(
                name: "SCR_MEMBER_COMPANIES");

            migrationBuilder.DropTable(
                name: "SCR_LETTERS");

            migrationBuilder.DropIndex(
                name: "IX_SEC_USERS_MEMBER_COMPANY_ID",
                table: "SEC_USERS");

            migrationBuilder.DropColumn(
                name: "MEMBER_COMPANY_ID",
                table: "SEC_USERS");
        }
    }
}
