using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingWriteOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_WRITE_OFFS",
                columns: table => new
                {
                    WRITE_OFF_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DAYS_LATE = table.Column<int>(type: "integer", nullable: false),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_SUSPENSE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LATE_FEE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LATE_FEE_SUSPENSE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INSURANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    REQUESTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DECIDED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DECIDED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DECISION_NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WRITE_OFF_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_WRITE_OFFS", x => x.WRITE_OFF_ID);
                    table.ForeignKey(
                        name: "FK_LND_WRITE_OFFS_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_WRITE_OFFS_LOAN_ID_STATUS_CODE",
                table: "LND_WRITE_OFFS",
                columns: new[] { "LOAN_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_LND_WRITE_OFFS_STATUS_CODE",
                table: "LND_WRITE_OFFS",
                column: "STATUS_CODE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_WRITE_OFFS");
        }
    }
}
