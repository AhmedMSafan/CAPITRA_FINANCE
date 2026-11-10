using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowSla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DUE_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ESCALATED_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESCALATED_FROM",
                table: "WFE_INBOX",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ESCALATE_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OVERDUE_NOTIFIED_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "REMINDED_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "REMIND_AT",
                table: "WFE_INBOX",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WFE_ALERTS",
                columns: table => new
                {
                    ALERT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ALERT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INBOX_ITEM_ID = table.Column<long>(type: "bigint", nullable: true),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: true),
                    MESSAGE = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ALERT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    READ_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_ALERTS", x => x.ALERT_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_ALERTS_INBOX_ITEM_ID",
                table: "WFE_ALERTS",
                column: "INBOX_ITEM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WFE_ALERTS_USER_NAME_READ_AT",
                table: "WFE_ALERTS",
                columns: new[] { "USER_NAME", "READ_AT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WFE_ALERTS");

            migrationBuilder.DropColumn(
                name: "DUE_AT",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "ESCALATED_AT",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "ESCALATED_FROM",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "ESCALATE_AT",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "OVERDUE_NOTIFIED_AT",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "REMINDED_AT",
                table: "WFE_INBOX");

            migrationBuilder.DropColumn(
                name: "REMIND_AT",
                table: "WFE_INBOX");
        }
    }
}
