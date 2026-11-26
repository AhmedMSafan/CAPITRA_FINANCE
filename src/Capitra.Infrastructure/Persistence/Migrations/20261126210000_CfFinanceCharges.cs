using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer finance charges (Ahmed, 26 Nov 2026): LND_LOAN_CHARGES keeps each finance's charges, paid upfront or added to the
    /// finance, and a sample set-up of the rules CF_DELIVERY and CF_SUPPLIER_PAYMENT is seeded on the consumer finance products.
    /// </summary>
    public partial class CfFinanceCharges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LND_LOAN_CHARGES",
                columns: table => new
                {
                    LOAN_CHARGE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CHARGE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHARGE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CHARGE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LOAN_ID = table.Column<long>(type: "bigint", nullable: false),
                    TAX_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TIMING_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LND_LOAN_CHARGES", x => x.LOAN_CHARGE_ID);
                    table.ForeignKey(
                        name: "FK_LND_LOAN_CHARGES_LND_LOANS",
                        column: x => x.LOAN_ID,
                        principalTable: "LND_LOANS",
                        principalColumn: "LOAN_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LND_LOAN_CHARGES_LOAN_ID",
                table: "LND_LOAN_CHARGES",
                column: "LOAN_ID");

            // sample consumer finance set-up (Ahmed, 26 Nov 2026), to be edited in Administration: a journal source and two categories
            // (copied from FACTORING's), the charges CF_ADMIN_FEE (1% of the financed amount, paid upfront) and CF_SUPPLIER_COMMISSION
            // (the supplier's agreed %, deducted from the supplier payment), and the rules CF_DELIVERY and CF_SUPPLIER_PAYMENT on every
            // consumer finance product. Their GL lines come with the steps that post them.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  v_by text := 'MIGRATION';
                  v_source bigint; v_cat_del bigint; v_cat_pay bigint; v_fee bigint; v_comm bigint; v_rule bigint; p record;
                BEGIN
                  INSERT INTO "GL_JOURNAL_SOURCES" ("SOURCE_CODE", "SOURCE_NAME_EN", "SOURCE_NAME_AR", "SOURCE_GROUP_CODE", "DESCRIPTION", "IS_SYSTEM_SOURCE",
                                                    "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED",
                                                    "IS_DUPLICATE_CHECK", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT 'CONSUMER_FINANCE', 'Consumer Finance', 'التمويل الاستهلاكي', "SOURCE_GROUP_CODE", 'Consumer finance: delivery and supplier payment', "IS_SYSTEM_SOURCE",
                         "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED", "IS_DUPLICATE_CHECK", true,
                         v_by, now(), 1
                    FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING'
                     AND NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'CONSUMER_FINANCE');
                  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'CONSUMER_FINANCE';
                  IF v_source IS NULL THEN
                    RAISE NOTICE 'No journal source FACTORING to copy: consumer finance rules skipped (add them in Administration).';
                    RETURN;
                  END IF;
                  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT c.code, c.en, c.ar, 'SOURCE_ACCOUNTING', v_source, false, false, true, true, c.descr, v_by, now(), 1
                    FROM (VALUES ('CF_DELIVERY', 'Consumer Finance Delivery', 'تسليم التمويل الاستهلاكي', 'A consumer finance booked when the customer received the goods.'),
                                 ('CF_SUPPLIER_PAYMENT', 'Consumer Finance Supplier Payment', 'السداد للمورد', 'The payment to the supplier, less its commission.')) AS c(code, en, ar, descr)
                   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" x WHERE x."CATEGORY_CODE" = c.code);
                  SELECT "CATEGORY_ID" INTO v_cat_del FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'CF_DELIVERY';
                  SELECT "CATEGORY_ID" INTO v_cat_pay FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'CF_SUPPLIER_PAYMENT';

                  -- the charges, each with the type of an existing fee / commission
                  INSERT INTO "ADM_CHARGES" ("CHARGE_CODE", "CHARGE_NAME_EN", "CHARGE_NAME_AR", "CHARGE_TYPE_CODE", "FINANCIAL_NATURE_CODE", "IS_RECURRING",
                                             "CALCULATION_TYPE_CODE", "DEFAULT_RATE", "DESCRIPTION", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT c.code, c.en, c.ar, (SELECT "CHARGE_TYPE_CODE" FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = c.tmpl
                                               UNION ALL SELECT "CHARGE_TYPE_CODE" FROM "ADM_CHARGES" LIMIT 1),
                         'REVENUE', false, 'PERCENTAGE', c.rate, c.descr, DATE '2026-01-01', true, v_by, now(), 1
                    FROM (VALUES ('CF_ADMIN_FEE', 'Administration fee', 'رسوم إدارية', 1.0, 'HANDLING_PER_DISB', 'Consumer finance administration fee paid by the customer.'),
                                 ('CF_SUPPLIER_COMMISSION', 'Supplier commission', 'عمولة المورد', 0.0, 'DISB_GROSS_COMMISSION', 'Commission kept from the payment to the supplier; its rate is the supplier''s agreed %.'))
                         AS c(code, en, ar, rate, tmpl, descr)
                   WHERE NOT EXISTS (SELECT 1 FROM "ADM_CHARGES" x WHERE x."CHARGE_CODE" = c.code)
                     AND EXISTS (SELECT 1 FROM "ADM_CHARGES");
                  SELECT "CHARGE_ID" INTO v_fee FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = 'CF_ADMIN_FEE';
                  SELECT "CHARGE_ID" INTO v_comm FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = 'CF_SUPPLIER_COMMISSION';
                  IF v_fee IS NULL OR v_comm IS NULL THEN
                    RAISE NOTICE 'No charge to copy a type from: consumer finance charges skipped (add them in Administration).';
                    RETURN;
                  END IF;

                  FOR p IN SELECT "PRODUCT_ID" FROM "ADM_PRODUCTS" WHERE "BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE' LOOP
                    IF NOT EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = p."PRODUCT_ID" AND "RULE_CODE" = 'CF_DELIVERY') THEN
                      INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR",
                                                             "PRIORITY_NO", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      VALUES (p."PRODUCT_ID", v_source, v_cat_del, 'CF_DELIVERY', 'Finance booked at delivery', 'قيد التمويل عند التسليم', 10, DATE '2026-01-01', true, v_by, now(), 1)
                      RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
                      INSERT INTO "ADM_PRODUCT_EVENT_RULE_CHARGES" ("PRODUCT_EVENT_RULE_ID", "CHARGE_ID", "SEQUENCE_NO", "CALCULATION_METHOD_CODE", "CHARGE_RATE",
                                                                    "AMOUNT_BASE_CODE", "INCOME_RECOGNITION_CODE", "SPREAD_RATE", "DAY_COUNT_BASIS", "ROUNDING_SCALE",
                                                                    "CHARGE_TIMING_CODE", "DUE_DAYS", "TAX_RATE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE",
                                                                    "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      VALUES (v_rule, v_fee, 10, 'PERCENTAGE', 1.0, 'FINANCED_AMOUNT', 'PRORATE', 0, 365, 2, 'PAID_UPFRONT', 0, 0, DATE '2026-01-01', true, v_by, now(), 1);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = p."PRODUCT_ID" AND "RULE_CODE" = 'CF_SUPPLIER_PAYMENT') THEN
                      INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR",
                                                             "PRIORITY_NO", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      VALUES (p."PRODUCT_ID", v_source, v_cat_pay, 'CF_SUPPLIER_PAYMENT', 'Supplier payment', 'السداد للمورد', 20, DATE '2026-01-01', true, v_by, now(), 1)
                      RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
                      INSERT INTO "ADM_PRODUCT_EVENT_RULE_CHARGES" ("PRODUCT_EVENT_RULE_ID", "CHARGE_ID", "SEQUENCE_NO", "CALCULATION_METHOD_CODE", "CHARGE_RATE",
                                                                    "AMOUNT_BASE_CODE", "INCOME_RECOGNITION_CODE", "SPREAD_RATE", "DAY_COUNT_BASIS", "ROUNDING_SCALE",
                                                                    "CHARGE_TIMING_CODE", "DUE_DAYS", "TAX_RATE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE",
                                                                    "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      VALUES (v_rule, v_comm, 10, 'PERCENTAGE', 0, 'FINANCED_AMOUNT', 'IMMEDIATE', 0, 365, 2, 'DEDUCT_FROM_DISBURSEMENT', 0, 0, DATE '2026-01-01', true, v_by, now(), 1);
                    END IF;
                  END LOOP;
                END $$;
                """);

            // the workflow's delivery and supplier payment steps post these rules (a workflow step links its rule by finova:ruleCode)
            foreach (var table in new[] { "WFE_WORKFLOW_DEFINITIONS", "WFE_DEFINITION_VERSIONS" })
            {
                var filter = table == "WFE_WORKFLOW_DEFINITIONS"
                    ? "\"WORKFLOW_CODE\" = 'CF_FINANCE'"
                    : "\"WORKFLOW_DEFINITION_ID\" IN (SELECT \"WORKFLOW_DEFINITION_ID\" FROM \"WFE_WORKFLOW_DEFINITIONS\" WHERE \"WORKFLOW_CODE\" = 'CF_FINANCE')";
                migrationBuilder.Sql($"""
                    UPDATE "{table}" SET "BPMN_XML" = replace(replace("BPMN_XML",
                        'finova:stepCode="CF_DELIVERY" ', 'finova:stepCode="CF_DELIVERY" finova:ruleCode="CF_DELIVERY" '),
                        'finova:stepCode="CF_SUPPLIER_PAYMENT" ', 'finova:stepCode="CF_SUPPLIER_PAYMENT" finova:ruleCode="CF_SUPPLIER_PAYMENT" ')
                    WHERE {filter} AND "BPMN_XML" NOT LIKE '%finova:ruleCode="CF_DELIVERY"%';
                    """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LND_LOAN_CHARGES");
        }
    }
}
