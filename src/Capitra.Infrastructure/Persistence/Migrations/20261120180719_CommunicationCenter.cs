using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommunicationCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MAIL_SERVER_ID",
                table: "CMC_EMAIL_LOGS",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CMC_MAIL_SERVERS",
                columns: table => new
                {
                    MAIL_SERVER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SERVER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HOST_NAME = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PORT_NO = table.Column<int>(type: "integer", nullable: false),
                    IS_SSL_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PASSWORD_PROTECTED = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FROM_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FROM_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    LAST_TEST_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_TEST_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LAST_TEST_ERROR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CMC_MAIL_SERVERS", x => x.MAIL_SERVER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SYS_DATA_PROTECTION_KEYS",
                columns: table => new
                {
                    DATA_PROTECTION_KEY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FRIENDLY_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KEY_XML = table.Column<string>(type: "text", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYS_DATA_PROTECTION_KEYS", x => x.DATA_PROTECTION_KEY_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CMC_EMAIL_LOGS_ATTEMPTED_AT",
                table: "CMC_EMAIL_LOGS",
                column: "ATTEMPTED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_CMC_EMAIL_LOGS_MAIL_SERVER_ID",
                table: "CMC_EMAIL_LOGS",
                column: "MAIL_SERVER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CMC_MAIL_SERVERS_IS_ACTIVE",
                table: "CMC_MAIL_SERVERS",
                column: "IS_ACTIVE",
                unique: true,
                filter: "\"IS_ACTIVE\"");

            migrationBuilder.AddForeignKey(
                name: "FK_CMC_EMAIL_LOGS_CMC_MAIL_SERVERS",
                table: "CMC_EMAIL_LOGS",
                column: "MAIL_SERVER_ID",
                principalTable: "CMC_MAIL_SERVERS",
                principalColumn: "MAIL_SERVER_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CMC_EMAIL_LOGS_CMC_MAIL_SERVERS",
                table: "CMC_EMAIL_LOGS");

            migrationBuilder.DropTable(
                name: "CMC_MAIL_SERVERS");

            migrationBuilder.DropTable(
                name: "SYS_DATA_PROTECTION_KEYS");

            migrationBuilder.DropIndex(
                name: "IX_CMC_EMAIL_LOGS_ATTEMPTED_AT",
                table: "CMC_EMAIL_LOGS");

            migrationBuilder.DropIndex(
                name: "IX_CMC_EMAIL_LOGS_MAIL_SERVER_ID",
                table: "CMC_EMAIL_LOGS");

            migrationBuilder.DropColumn(
                name: "MAIL_SERVER_ID",
                table: "CMC_EMAIL_LOGS");
        }
    }
}
