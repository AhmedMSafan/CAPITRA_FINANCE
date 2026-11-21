using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScreeningBillingGl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ACCOUNTING_DATE",
                table: "SCR_INVOICES",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CANCEL_GL_TRAN_SET_ID",
                table: "SCR_INVOICES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GL_TRAN_SET_ID",
                table: "SCR_INVOICES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PAID_AMOUNT",
                table: "SCR_INVOICES",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SCR_INVOICE_PAYMENTS",
                columns: table => new
                {
                    PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    INVOICE_ID = table.Column<long>(type: "bigint", nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    REFERENCE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GL_TRAN_SET_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCR_INVOICE_PAYMENTS", x => x.PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_SCR_INVOICE_PAYMENTS_SCR_INVOICES",
                        column: x => x.INVOICE_ID,
                        principalTable: "SCR_INVOICES",
                        principalColumn: "INVOICE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCR_INVOICE_PAYMENTS_TRS_BANK_ACCOUNTS",
                        column: x => x.BANK_ACCOUNT_ID,
                        principalTable: "TRS_BANK_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INVOICE_PAYMENTS_BANK_ACCOUNT_ID",
                table: "SCR_INVOICE_PAYMENTS",
                column: "BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SCR_INVOICE_PAYMENTS_INVOICE_ID",
                table: "SCR_INVOICE_PAYMENTS",
                column: "INVOICE_ID");

            // Screening's product, GL accounts and rules (Ahmed, 22 Nov 2026), added only where missing and only where the chart of
            // accounts has what they are copied from; a database without the chart (a new test database) just gets the product.
            //   business line SCREENING (product lookup and GL segment value, copied from FACTORING)
            //   journal source SCREENING (copied from FACTORING), categories SCR_INVOICE and SCR_PAYMENT
            //   GL accounts 120701 Screening Fees Receivable (like 120103), 410701 Screening User Fee Income and
            //   410702 Screening Inquiry Fee Income (like 410101)
            //   product SCR_SERVICES (GL transaction set code SCR), rules SCR_INVOICE and SCR_PAYMENT, linked to the actions
            //   SCR_INVOICE_ISSUE:   DR 120701 INVOICE_AMOUNT / CR 410701 USER_FEE_AMOUNT / CR 410702 INQUIRY_FEE_AMOUNT
            //   SCR_INVOICE_PAYMENT: DR the bank account's GL account PAYMENT_AMOUNT / CR 120701 PAYMENT_AMOUNT
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  v_by      text := 'MIGRATION';
                  v_product bigint;
                  v_source  bigint;
                  v_cat_inv bigint;
                  v_cat_pay bigint;
                  v_tmpl    record;
                  v_sv      record;
                  a         record;
                  v_ok      boolean := true;
                  v_rule    bigint;
                BEGIN
                  -- business line of the product catalog
                  INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  VALUES ('SCREENING', 'Screening', 'الفحص', 'Screening service of the Federation member companies', 20, true, v_by, now(), 1)
                  ON CONFLICT ("CODE") DO NOTHING;

                  -- the product, with its GL transaction set counter CAP-SCR-{year}-...
                  INSERT INTO "ADM_PRODUCTS" ("PRODUCT_CODE", "PRODUCT_NAME_EN", "PRODUCT_NAME_AR", "BUSINESS_LINE_CODE", "PRODUCT_TYPE_CODE", "GL_TRAN_SET_CODE",
                                              "DEFAULT_CURRENCY_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "MPP_PV_RATE", "MPP_LATE_FEE_RATE",
                                              "MPP_RESERVED_AMOUNT_PCT", "MPP_INVESTOR_PAYMENT_PCT", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT 'SCR_SERVICES', 'Screening services', 'خدمات الفحص', 'SCREENING', 'SERVICE',
                         CASE WHEN EXISTS (SELECT 1 FROM "ADM_PRODUCTS" WHERE "GL_TRAN_SET_CODE" = 'SCR') OR EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = 'SCR')
                              THEN NULL ELSE 'SCR' END,
                         (SELECT "CURRENCY_CODE" FROM "ADM_LKP_CURRENCIES" WHERE "CURRENCY_CODE" = 'EGP'), DATE '2026-01-01', true, 0, 0, 0, 0, v_by, now(), 1
                   WHERE NOT EXISTS (SELECT 1 FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'SCR_SERVICES');
                  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'SCR_SERVICES';
                  INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT 'SCR', extract(year FROM now())::int, 'SCREENING', 'Screening services', 'خدمات الفحص', 0, 8, true, v_by, now(), 1
                   WHERE NOT EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = 'SCR');

                  -- GL business line segment value, copied from FACTORING
                  INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID", "IS_SUMMARY",
                                                       "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER", "REFERENCE_BUSINESS_LINE_CODE",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT f."COA_SEGMENT_ID", 'SCREENING', 'Screening', 'الفحص', 'Screening services', f."PARENT_VALUE_ID", false, true, true, f."EFFECTIVE_START_DATE",
                         f."SORT_ORDER" + 5, 'SCREENING', v_by, now(), 1
                    FROM "GL_COA_SEGMENTS" s
                    JOIN "GL_COA_SEGMENT_VALUES" f ON f."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND f."VALUE_CODE" = 'FACTORING'
                   WHERE s."SEGMENT_CODE" = 'BUSINESS_LINE'
                     AND NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" r WHERE r."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND r."VALUE_CODE" = 'SCREENING');

                  -- journal source and categories
                  INSERT INTO "GL_JOURNAL_SOURCES" ("SOURCE_CODE", "SOURCE_NAME_EN", "SOURCE_NAME_AR", "SOURCE_GROUP_CODE", "DESCRIPTION", "IS_SYSTEM_SOURCE",
                                                    "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED",
                                                    "IS_DUPLICATE_CHECK", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT 'SCREENING', 'Screening', 'الفحص', "SOURCE_GROUP_CODE", 'Screening invoices and their payments', "IS_SYSTEM_SOURCE",
                         "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED", "IS_DUPLICATE_CHECK", true,
                         v_by, now(), 1
                    FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING'
                     AND NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'SCREENING');
                  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'SCREENING';
                  IF v_source IS NULL THEN
                    RAISE NOTICE 'No journal source FACTORING to copy: screening GL set-up skipped (do it in Admin Setup).';
                    RETURN;
                  END IF;
                  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT c.code, c.en, c.ar, 'SOURCE_ACCOUNTING', v_source, false, false, true, true, c.descr, v_by, now(), 1
                    FROM (VALUES ('SCR_INVOICE', 'Screening Invoice', 'فاتورة الفحص', 'A screening invoice: fees receivable against user and inquiry fee income.'),
                                 ('SCR_PAYMENT', 'Screening Payment', 'سداد فاتورة الفحص', 'Money a member company paid against a screening invoice.')) AS c(code, en, ar, descr)
                   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" x WHERE x."CATEGORY_CODE" = c.code);
                  SELECT "CATEGORY_ID" INTO v_cat_inv FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'SCR_INVOICE';
                  SELECT "CATEGORY_ID" INTO v_cat_pay FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'SCR_PAYMENT';

                  -- GL accounts, each with the settings and parent of its template
                  FOR a IN SELECT * FROM (VALUES
                      ('120701', 'Screening Fees Receivable', 'ذمم رسوم الفحص', 'Screening fees invoiced to member companies and not paid yet.', 1207, '120103'),
                      ('410701', 'Screening User Fee Income', 'إيرادات رسوم مستخدمي الفحص', 'Monthly fees of the member companies'' screening users.', 4107, '410101'),
                      ('410702', 'Screening Inquiry Fee Income', 'إيرادات رسوم استعلامات الفحص', 'Fees of the member companies'' screening inquiries.', 4108, '410101'))
                      AS t(code, en, ar, descr, sort, tmpl)
                  LOOP
                    SELECT * INTO v_tmpl FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = a.tmpl AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1;
                    IF v_tmpl."ACCOUNT_ID" IS NULL THEN
                      RAISE NOTICE 'GL account % to copy is missing: % not added.', a.tmpl, a.code;
                      v_ok := false;
                      CONTINUE;
                    END IF;
                    IF EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tmpl."COA_ID" AND "ACCOUNT_CODE" = a.code) THEN
                      IF NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tmpl."COA_ID" AND "ACCOUNT_CODE" = a.code AND "ACCOUNT_NAME_EN" = a.en) THEN
                        RAISE NOTICE 'GL account % is used by another account: screening rules not added.', a.code;
                        v_ok := false;
                      END IF;
                      CONTINUE;
                    END IF;
                    SELECT * INTO v_sv FROM "GL_COA_SEGMENT_VALUES" WHERE "SEGMENT_VALUE_ID" = v_tmpl."ACCOUNT_SEGMENT_VALUE_ID";
                    IF v_sv."SEGMENT_VALUE_ID" IS NOT NULL THEN
                      INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID",
                                                           "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                                           "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      SELECT v_sv."COA_SEGMENT_ID", a.code, a.en, a.ar, a.descr, v_sv."PARENT_VALUE_ID", false, true, true, v_sv."EFFECTIVE_START_DATE", a.sort, v_by, now(), 1
                       WHERE NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_sv."COA_SEGMENT_ID" AND "VALUE_CODE" = a.code);
                    END IF;
                    INSERT INTO "GL_ACCOUNTS" ("COA_ID", "ACCOUNT_CODE", "ACCOUNT_NAME_EN", "ACCOUNT_NAME_AR", "DESCRIPTION", "PARENT_ACCOUNT_ID",
                                               "ACCOUNT_SEGMENT_VALUE_ID", "ACCOUNT_TYPE", "NORMAL_BALANCE", "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS",
                                               "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_CONTROL_ACCOUNT", "IS_RECONCILIATION", "IS_BRANCH_REQUIRED",
                                               "IS_COST_CENTER_REQUIRED", "IS_BUSINESS_LINE_REQUIRED", "IS_MANUAL_JOURNAL_ALLOWED", "IS_SUBLEDGER_POSTING_ALLOWED",
                                               "IS_CURRENCY_REVALUATION", "IS_RETAINED_EARNINGS", "IS_ACTIVE", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                               "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    VALUES (v_tmpl."COA_ID", a.code, a.en, a.ar, a.descr, v_tmpl."PARENT_ACCOUNT_ID",
                            (SELECT "SEGMENT_VALUE_ID" FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_sv."COA_SEGMENT_ID" AND "VALUE_CODE" = a.code),
                            v_tmpl."ACCOUNT_TYPE", v_tmpl."NORMAL_BALANCE", v_tmpl."FINANCIAL_STATEMENT_CLASS", v_tmpl."CASH_FLOW_CLASS",
                            false, true, v_tmpl."IS_CONTROL_ACCOUNT", v_tmpl."IS_RECONCILIATION", v_tmpl."IS_BRANCH_REQUIRED",
                            v_tmpl."IS_COST_CENTER_REQUIRED", v_tmpl."IS_BUSINESS_LINE_REQUIRED", v_tmpl."IS_MANUAL_JOURNAL_ALLOWED", v_tmpl."IS_SUBLEDGER_POSTING_ALLOWED",
                            v_tmpl."IS_CURRENCY_REVALUATION", false, true, v_tmpl."EFFECTIVE_START_DATE", a.sort, v_by, now(), 1);
                  END LOOP;
                  IF NOT v_ok THEN
                    RETURN;
                  END IF;

                  -- the rules and their GL lines, linked to the actions
                  FOR a IN SELECT * FROM (VALUES ('SCR_INVOICE', 'Screening invoice', 'فاتورة الفحص', 'SCR_INVOICE_ISSUE'),
                                                 ('SCR_PAYMENT', 'Screening invoice payment', 'سداد فاتورة الفحص', 'SCR_INVOICE_PAYMENT')) AS t(code, en, ar, action)
                  LOOP
                    SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = a.code;
                    IF v_rule IS NULL THEN
                      INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR",
                                                             "PRIORITY_NO", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      VALUES (v_product, v_source, CASE a.code WHEN 'SCR_INVOICE' THEN v_cat_inv ELSE v_cat_pay END, a.code, a.en, a.ar, 10, DATE '2026-01-01', true,
                              v_by, now(), 1)
                      RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
                      INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                                     "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                                     "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      SELECT v_rule, NULL, 1, l.no, l.dc, l.amount, l.src,
                             CASE WHEN l.acct IS NULL THEN NULL ELSE (SELECT "ACCOUNT_ID" FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = l.acct AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1) END,
                             l.descr, true, v_by, now(), 1
                        FROM (VALUES ('SCR_INVOICE', 10, 'D', 'INVOICE_AMOUNT', 'ACCOUNT', '120701', 'Screening fees receivable'),
                                     ('SCR_INVOICE', 20, 'C', 'USER_FEE_AMOUNT', 'ACCOUNT', '410701', 'Screening user fees'),
                                     ('SCR_INVOICE', 30, 'C', 'INQUIRY_FEE_AMOUNT', 'ACCOUNT', '410702', 'Screening inquiry fees'),
                                     ('SCR_PAYMENT', 10, 'D', 'PAYMENT_AMOUNT', 'BANK_ACCOUNT', NULL, 'Screening invoice payment received'),
                                     ('SCR_PAYMENT', 20, 'C', 'PAYMENT_AMOUNT', 'ACCOUNT', '120701', 'Screening fees receivable paid')) AS l(rule, no, dc, amount, src, acct, descr)
                       WHERE l.rule = a.code;
                    END IF;
                    INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    VALUES (v_product, a.action, v_rule, true, v_by, now(), 1)
                    ON CONFLICT ("ACTION_CODE") DO UPDATE SET "PRODUCT_EVENT_RULE_ID" = EXCLUDED."PRODUCT_EVENT_RULE_ID", "UPDATED_BY" = v_by, "UPDATED_AT" = now()
                     WHERE "ADM_PRODUCT_ACTIONS"."PRODUCT_EVENT_RULE_ID" IS NULL;
                  END LOOP;
                EXCEPTION WHEN OTHERS THEN
                  -- never stop the application from starting: the set-up is left out whole, and is done in Admin Setup instead
                  RAISE WARNING 'Screening GL set-up skipped: %', SQLERRM;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SCR_INVOICE_PAYMENTS");

            migrationBuilder.DropColumn(
                name: "ACCOUNTING_DATE",
                table: "SCR_INVOICES");

            migrationBuilder.DropColumn(
                name: "CANCEL_GL_TRAN_SET_ID",
                table: "SCR_INVOICES");

            migrationBuilder.DropColumn(
                name: "GL_TRAN_SET_ID",
                table: "SCR_INVOICES");

            migrationBuilder.DropColumn(
                name: "PAID_AMOUNT",
                table: "SCR_INVOICES");
        }
    }
}
