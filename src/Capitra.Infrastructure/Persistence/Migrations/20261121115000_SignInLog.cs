using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SignInLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RECEIVED_AT",
                table: "SCR_INQUIRIES",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RESPONDED_AT",
                table: "SCR_INQUIRIES",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SESSION_ID",
                table: "SCR_INQUIRIES",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SEC_SIGN_INS",
                columns: table => new
                {
                    SIGN_IN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SESSION_ID = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    USER_ID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: true),
                    ATTEMPTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RESULT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FAILURE_TEXT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IP_ADDRESS = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    USER_AGENT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EXPIRES_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SIGNED_OUT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_SIGN_INS", x => x.SIGN_IN_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRIES_SESSION_ID",
                table: "SCR_INQUIRIES",
                column: "SESSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_SIGN_INS_ATTEMPTED_AT",
                table: "SEC_SIGN_INS",
                column: "ATTEMPTED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_SIGN_INS_MEMBER_COMPANY_ID_ATTEMPTED_AT",
                table: "SEC_SIGN_INS",
                columns: new[] { "MEMBER_COMPANY_ID", "ATTEMPTED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_SEC_SIGN_INS_USER_ID",
                table: "SEC_SIGN_INS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SEC_SIGN_INS_SESSION_ID",
                table: "SEC_SIGN_INS",
                column: "SESSION_ID",
                unique: true,
                filter: "\"SESSION_ID\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SEC_SIGN_INS");

            migrationBuilder.DropIndex(
                name: "IX_SCR_INQUIRIES_SESSION_ID",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "RECEIVED_AT",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "RESPONDED_AT",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "SESSION_ID",
                table: "SCR_INQUIRIES");
        }
    }
}
