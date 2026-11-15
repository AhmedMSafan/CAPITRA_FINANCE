using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LendingGoods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_LKP_GOODS_CATEGORIES",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LKP_GOODS_CATEGORIES", x => x.CODE);
                });

            migrationBuilder.AddColumn<string>(
                name: "GOODS_CATEGORY_CODE",
                table: "LND_LOANS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GOODS_DESCRIPTION",
                table: "LND_LOANS",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GOODS_BRAND_MODEL",
                table: "LND_LOANS",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MERCHANT_NAME",
                table: "LND_LOANS",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GOODS_PRICE_AMOUNT",
                table: "LND_LOANS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DOWN_PAYMENT_AMOUNT",
                table: "LND_LOANS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOANS_GOODS_CATEGORY_CODE",
                table: "LND_LOANS",
                column: "GOODS_CATEGORY_CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_LND_LOANS_LND_LKP_GOODS_CATEGORIES",
                table: "LND_LOANS",
                column: "GOODS_CATEGORY_CODE",
                principalTable: "LND_LKP_GOODS_CATEGORIES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            // The goods categories of consumer finance, after the categories the FRA reports.
            migrationBuilder.Sql("""
                INSERT INTO "LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_EN", "NAME_AR", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                VALUES ('ELECTRONICS', 'Electronics & electrical devices', 'الأجهزة الإلكترونية والكهربائية', 'TV, laptop, tablet, camera, gaming', 10, true, 'SYSTEM', now(), 1),
                       ('MOBILE', 'Mobile phones', 'الهواتف المحمولة', 'Phones and smart watches', 20, true, 'SYSTEM', now(), 1),
                       ('HOME_APPLIANCES', 'Household appliances', 'الأجهزة المنزلية', 'Fridge, washer, air conditioner, cooker, water heater', 30, true, 'SYSTEM', now(), 1),
                       ('FURNITURE', 'Furniture & home fittings', 'الأثاث والمفروشات', 'Bedrooms, living rooms, kitchens, carpets', 40, true, 'SYSTEM', now(), 1),
                       ('HOME_FINISHING', 'Home finishing', 'التشطيبات', 'Ceramics, sanitary ware, paint, doors, aluminium', 50, true, 'SYSTEM', now(), 1),
                       ('VEHICLES', 'Cars & vehicles', 'السيارات ووسائل النقل', 'New or used cars, motorcycles', 60, true, 'SYSTEM', now(), 1),
                       ('FASHION', 'Clothing, shoes, bags & watches', 'الملابس والأحذية والحقائب والساعات', NULL, 70, true, 'SYSTEM', now(), 1),
                       ('EDUCATION', 'Education services', 'الخدمات التعليمية', 'School, university and course fees', 80, true, 'SYSTEM', now(), 1),
                       ('MEDICAL', 'Medical services', 'الخدمات الطبية', 'Surgery, dental, optical, treatment', 90, true, 'SYSTEM', now(), 1),
                       ('TRAVEL', 'Travel & tourism', 'السفر والسياحة', 'Trips, Hajj and Umrah', 100, true, 'SYSTEM', now(), 1),
                       ('CLUBS', 'Clubs & sports memberships', 'عضويات الأندية', NULL, 110, true, 'SYSTEM', now(), 1),
                       ('OTHER', 'Other durable goods', 'سلع معمرة أخرى', 'Anything not listed, with a description', 120, true, 'SYSTEM', now(), 1)
                ON CONFLICT ("CODE") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LND_LOANS_LND_LKP_GOODS_CATEGORIES",
                table: "LND_LOANS");

            migrationBuilder.DropTable(
                name: "LND_LKP_GOODS_CATEGORIES");

            migrationBuilder.DropIndex(
                name: "IX_LND_LOANS_GOODS_CATEGORY_CODE",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "GOODS_CATEGORY_CODE",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "GOODS_DESCRIPTION",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "GOODS_BRAND_MODEL",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "MERCHANT_NAME",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "GOODS_PRICE_AMOUNT",
                table: "LND_LOANS");

            migrationBuilder.DropColumn(
                name: "DOWN_PAYMENT_AMOUNT",
                table: "LND_LOANS");
        }
    }
}
