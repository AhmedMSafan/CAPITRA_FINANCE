using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_PARAMETER_VALUES",
                columns: table => new
                {
                    PARAMETER_VALUE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PARAMETER_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SCOPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VALUE_TEXT = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DECISION_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REQUEST_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DECIDED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DECIDED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DECISION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_PARAMETER_VALUES", x => x.PARAMETER_VALUE_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_PARAMETER_VALUES_PARAMETER_CODE_SCOPE_CODE_STATUS_CODE",
                table: "LND_PARAMETER_VALUES",
                columns: new[] { "PARAMETER_CODE", "SCOPE_CODE", "STATUS_CODE" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_PARAMETER_VALUES");
        }
    }
}
