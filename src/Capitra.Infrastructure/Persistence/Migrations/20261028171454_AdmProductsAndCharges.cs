using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdmProductsAndCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_LKP_CHARGE_TYPES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_CHARGE_TYPES", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_PRODUCTS",
                columns: table => new
                {
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PRODUCT_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PRODUCT_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BUSINESS_LINE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PRODUCT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FACTORING_MODE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IS_RECOURSE = table.Column<bool>(type: "boolean", nullable: true),
                    DEFAULT_CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    MPP_PV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MPP_LATE_FEE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MPP_RESERVED_AMOUNT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    MPP_INVESTOR_PAYMENT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_PRODUCTS", x => x.PRODUCT_ID);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCTS_ADM_LKP_BUSINESS_LINES",
                        column: x => x.BUSINESS_LINE_CODE,
                        principalTable: "ADM_LKP_BUSINESS_LINES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCTS_ADM_LKP_CURRENCIES",
                        column: x => x.DEFAULT_CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADM_CHARGES",
                columns: table => new
                {
                    CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHARGE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHARGE_NAME_EN = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CHARGE_NAME_AR = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CHARGE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FINANCIAL_NATURE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IS_RECURRING = table.Column<bool>(type: "boolean", nullable: false),
                    CALCULATION_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DEFAULT_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DEFAULT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_CHARGES", x => x.CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_ADM_CHARGES_ADM_LKP_CHARGE_TYPES",
                        column: x => x.CHARGE_TYPE_CODE,
                        principalTable: "ADM_LKP_CHARGE_TYPES",
                        principalColumn: "CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_CHARGES_ADM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADM_PRODUCT_EVENT_RULES",
                columns: table => new
                {
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE_ID = table.Column<long>(type: "bigint", nullable: false),
                    WORKFLOW_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    STEP_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RULE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RULE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RULE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PRIORITY_NO = table.Column<int>(type: "integer", nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_PRODUCT_EVENT_RULES", x => x.PRODUCT_EVENT_RULE_ID);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULES_ADM_PRODUCTS",
                        column: x => x.PRODUCT_ID,
                        principalTable: "ADM_PRODUCTS",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULES_GL_ACCOUNTING_EVENT_TYPES",
                        column: x => x.EVENT_TYPE_ID,
                        principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                        principalColumn: "EVENT_TYPE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                columns: table => new
                {
                    RULE_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHARGE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    CALCULATION_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FIXED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CHARGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SPREAD_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    DAY_COUNT_BASIS = table.Column<int>(type: "integer", nullable: true),
                    MINIMUM_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MAXIMUM_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ROUNDING_SCALE = table.Column<int>(type: "integer", nullable: false),
                    CHARGE_TIMING_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DUE_DAYS = table.Column<int>(type: "integer", nullable: false),
                    TAX_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    EFFECTIVE_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EFFECTIVE_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_PRODUCT_EVENT_RULE_CHARGES", x => x.RULE_CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULE_CHARGES_ADM_CHARGES",
                        column: x => x.CHARGE_ID,
                        principalTable: "ADM_CHARGES",
                        principalColumn: "CHARGE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULE_CHARGES_ADM_PRODUCT_EVENT_RULES",
                        column: x => x.PRODUCT_EVENT_RULE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULES",
                        principalColumn: "PRODUCT_EVENT_RULE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                columns: table => new
                {
                    RULE_GL_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PRODUCT_EVENT_RULE_ID = table.Column<long>(type: "bigint", nullable: false),
                    RULE_CHARGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LINE_NO = table.Column<int>(type: "integer", nullable: false),
                    DEBIT_CREDIT_CODE = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    DESCRIPTION_TEMPLATE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_PRODUCT_EVENT_RULE_GL_LINES", x => x.RULE_GL_LINE_ID);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULE_GL_LINES_ADM_PRODUCT_EVENT_RULES",
                        column: x => x.PRODUCT_EVENT_RULE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULES",
                        principalColumn: "PRODUCT_EVENT_RULE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULE_GL_LINES_ADM_PRODUCT_EVENT_R_C89D088D",
                        column: x => x.RULE_CHARGE_ID,
                        principalTable: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                        principalColumn: "RULE_CHARGE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ADM_PRODUCT_EVENT_RULE_GL_LINES_GL_ACCOUNTS",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "GL_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_CHARGES_CHARGE_TYPE_CODE",
                table: "ADM_CHARGES",
                column: "CHARGE_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_CHARGES_CURRENCY_CODE",
                table: "ADM_CHARGES",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_CHARGES_CHARGE_CODE",
                table: "ADM_CHARGES",
                column: "CHARGE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULE_CHARGES_CHARGE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                column: "CHARGE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_CHARGES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                columns: new[] { "PRODUCT_EVENT_RULE_ID", "CHARGE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULE_GL_LINES_ACCOUNT_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                column: "ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULE_GL_LINES_RULE_CHARGE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                column: "RULE_CHARGE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULE_GL_LINES_PRODUCT_EVENT_RULE_ID",
                table: "ADM_PRODUCT_EVENT_RULE_GL_LINES",
                columns: new[] { "PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "LINE_NO" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "EVENT_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_WORKFLOW_CODE_STEP_CODE",
                table: "ADM_PRODUCT_EVENT_RULES",
                columns: new[] { "PRODUCT_ID", "WORKFLOW_CODE", "STEP_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_EVENT_TYPE_ID_RULE_CODE",
                table: "ADM_PRODUCT_EVENT_RULES",
                columns: new[] { "PRODUCT_ID", "EVENT_TYPE_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCTS_BUSINESS_LINE_CODE",
                table: "ADM_PRODUCTS",
                column: "BUSINESS_LINE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCTS_DEFAULT_CURRENCY_CODE",
                table: "ADM_PRODUCTS",
                column: "DEFAULT_CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCTS_IS_ACTIVE_BUSINESS_LINE_CODE",
                table: "ADM_PRODUCTS",
                columns: new[] { "IS_ACTIVE", "BUSINESS_LINE_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCTS_PRODUCT_CODE",
                table: "ADM_PRODUCTS",
                column: "PRODUCT_CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_PRODUCT_EVENT_RULE_GL_LINES");

            migrationBuilder.DropTable(
                name: "ADM_PRODUCT_EVENT_RULE_CHARGES");

            migrationBuilder.DropTable(
                name: "ADM_CHARGES");

            migrationBuilder.DropTable(
                name: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropTable(
                name: "ADM_LKP_CHARGE_TYPES");

            migrationBuilder.DropTable(
                name: "ADM_PRODUCTS");
        }
    }
}
