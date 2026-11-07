using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DisbursementRuleCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AMOUNT_BASE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "INCOME_RECOGNITION_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "PRORATE");

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES",
                columns: table => new
                {
                    DISBURSEMENT_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    CHARGE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHARGE_NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CHARGE_NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RULE_CHARGE_ID = table.Column<long>(type: "bigint", nullable: true),
                    CALCULATION_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AMOUNT_BASE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BASE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CHARGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PER_UNIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UNIT_COUNT = table.Column<int>(type: "integer", nullable: true),
                    CALCULATED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INCOME_RECOGNITION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES", x => x.DISBURSEMENT_CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_DISBURSEMENT_CHARGES_MTG_PURCHASED_PORTFOLIO_6CE5976C",
                        column: x => x.DISBURSEMENT_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_DISBURSEMENT_CHARGES_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_DISBURSEMENT_CHARGES_DISBURSEMENT_ID_CHARGE_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES",
                columns: new[] { "DISBURSEMENT_ID", "CHARGE_CODE" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES");

            migrationBuilder.DropColumn(
                name: "AMOUNT_BASE_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES");

            migrationBuilder.DropColumn(
                name: "INCOME_RECOGNITION_CODE",
                table: "ADM_PRODUCT_EVENT_RULE_CHARGES");
        }
    }
}
