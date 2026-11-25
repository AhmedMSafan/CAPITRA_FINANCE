using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// CRM_RECENT_CLIENTS (Ahmed, 25 Nov 2026): the customers each user opened in Customer 360, shown as "Recently opened" on the
    /// Customers page. One row per user and customer.
    /// </summary>
    public partial class CrmRecentClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_RECENT_CLIENTS",
                columns: table => new
                {
                    RECENT_CLIENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    OPENED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_RECENT_CLIENTS", x => x.RECENT_CLIENT_ID);
                    table.ForeignKey(
                        name: "FK_CRM_RECENT_CLIENTS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_RECENT_CLIENTS_CLIENT_ID",
                table: "CRM_RECENT_CLIENTS",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_RECENT_CLIENTS_USER_NAME_CLIENT_ID",
                table: "CRM_RECENT_CLIENTS",
                columns: new[] { "USER_NAME", "CLIENT_ID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CRM_RECENT_CLIENTS");
        }
    }
}
