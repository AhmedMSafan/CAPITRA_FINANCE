using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScreeningApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "API_CLIENT_ID",
                table: "SCR_INQUIRIES",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CHANNEL_CODE",
                table: "SCR_INQUIRIES",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PORTAL");

            migrationBuilder.CreateTable(
                name: "SCR_API_CLIENTS",
                columns: table => new
                {
                    API_CLIENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MEMBER_COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLIENT_ID = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CLIENT_NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SECRET_HASH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SECRET_HINT = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SECRET_CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REVOKED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REVOKED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LAST_TOKEN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_IP_ADDRESS = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_API_CLIENTS", x => x.API_CLIENT_ID);
                    table.ForeignKey(
                        name: "FK_SCR_API_CLIENTS_SCR_MEMBER_COMPANIES",
                        column: x => x.MEMBER_COMPANY_ID,
                        principalTable: "SCR_MEMBER_COMPANIES",
                        principalColumn: "MEMBER_COMPANY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INQUIRIES_API_CLIENT_ID",
                table: "SCR_INQUIRIES",
                column: "API_CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_API_CLIENTS_MEMBER_COMPANY_ID",
                table: "SCR_API_CLIENTS",
                column: "MEMBER_COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_SCR_API_CLIENTS_CLIENT_ID",
                table: "SCR_API_CLIENTS",
                column: "CLIENT_ID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SCR_INQUIRIES_SCR_API_CLIENTS",
                table: "SCR_INQUIRIES",
                column: "API_CLIENT_ID",
                principalTable: "SCR_API_CLIENTS",
                principalColumn: "API_CLIENT_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SCR_INQUIRIES_SCR_API_CLIENTS",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropTable(
                name: "SCR_API_CLIENTS");

            migrationBuilder.DropIndex(
                name: "IX_SCR_INQUIRIES_API_CLIENT_ID",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "API_CLIENT_ID",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "CHANNEL_CODE",
                table: "SCR_INQUIRIES");
        }
    }
}
