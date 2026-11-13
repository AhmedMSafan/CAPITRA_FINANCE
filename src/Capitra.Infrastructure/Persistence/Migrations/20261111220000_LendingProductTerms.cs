using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingProductTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_PRODUCT_VERSIONS",
                columns: table => new
                {
                    PRODUCT_VERSION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false),
                    VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MIN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MIN_TERM_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    MAX_TERM_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    REPAYMENT_FREQUENCY_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    INTEREST_METHOD_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ANNUAL_RATE_PCT = table.Column<long>(type: "numeric(9,6)", nullable: false),
                    DAY_COUNT_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LATE_FEE_GRACE_DAYS = table.Column<int>(type: "integer", nullable: false),
                    LATE_FEE_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LATE_FEE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    LATE_FEE_FLAT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    LATE_FEE_BASE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PAYMENT_ORDER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IS_INSURANCE_REQUIRED = table.Column<bool>(type: "boolean", nullable: false),
                    CHANGE_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    REQUESTED_BY = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    REQUESTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_LND_PRODUCT_VERSIONS", x => x.PRODUCT_VERSION_ID);
                    table.ForeignKey(
                        name: "FK_LND_PRODUCT_VERSIONS_ADM_PRODUCTS",
                        column: x => x.PRODUCT_ID,
                        principalTable: "ADM_PRODUCTS",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_PRODUCT_VERSIONS_PRODUCT_ID_STATUS_CODE",
                table: "LND_PRODUCT_VERSIONS",
                columns: new[] { "PRODUCT_ID", "STATUS_CODE", "EFFECTIVE_FROM_DATE" });

            migrationBuilder.CreateIndex(
                name: "UQ_LND_PRODUCT_VERSIONS_PRODUCT_ID_VERSION_NO",
                table: "LND_PRODUCT_VERSIONS",
                columns: new[] { "PRODUCT_ID", "VERSION_NO" },
                unique: true);

            // The business lines of Retail Lending, so lending products can be created in the Product Catalog.
            migrationBuilder.Sql("""
                INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('MICROFINANCE', 'Microfinance', 'التمويل متناهي الصغر', 10, true, 'SYSTEM', now(), 1),
                       ('CONSUMER_FINANCE', 'Consumer Finance', 'التمويل الاستهلاكي', 11, true, 'SYSTEM', now(), 1)
                ON CONFLICT ("CODE") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LND_PRODUCT_VERSIONS");
        }
    }
}
