using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consumer finance GL (Ahmed, 26 Nov 2026): the accounts 120801-120803, 230801-230802 and 410801-410802 (each copied from an account
    /// of the same kind), the business line segment value, the rules CF_CLAIM and CF_CLAIM_PAYMENT, the GL lines of the four consumer
    /// finance rules, the product action CF_CLAIM_PAYMENT, and the Agreement step linked to CF_CLAIM. Nothing is posted yet: each step
    /// posts its rule when it is built.
    /// </summary>
    public partial class CfFinanceGl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  v_by     text := 'MIGRATION';
                  v_source bigint;
                  v_cat    bigint;
                  v_rule   bigint;
                  v_tmpl   record;
                  v_sv     record;
                  acct     record;
                  prod     record;
                  rl       record;
                  v_ok     boolean := true;
                BEGIN
                  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'CONSUMER_FINANCE';
                  IF v_source IS NULL THEN
                    RAISE NOTICE 'No journal source CONSUMER_FINANCE: consumer finance GL set-up skipped (do it in Admin Setup).';
                    RETURN;
                  END IF;

                  -- GL business line segment value, copied from FACTORING
                  INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID", "IS_SUMMARY",
                                                       "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER", "REFERENCE_BUSINESS_LINE_CODE",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT f."COA_SEGMENT_ID", 'CONSUMER_FINANCE', 'Consumer Finance', 'التمويل الاستهلاكي', 'Consumer finance', f."PARENT_VALUE_ID", false, true, true,
                         f."EFFECTIVE_START_DATE", f."SORT_ORDER" + 6, 'CONSUMER_FINANCE', v_by, now(), 1
                    FROM "GL_COA_SEGMENTS" s
                    JOIN "GL_COA_SEGMENT_VALUES" f ON f."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND f."VALUE_CODE" = 'FACTORING'
                   WHERE s."SEGMENT_CODE" = 'BUSINESS_LINE'
                     AND NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" x WHERE x."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND x."VALUE_CODE" = 'CONSUMER_FINANCE');

                  -- the journal categories of the two new events
                  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT c.code, c.en, c.ar, 'SOURCE_ACCOUNTING', v_source, false, false, true, true, c.descr, v_by, now(), 1
                    FROM (VALUES ('CF_CLAIM', 'Consumer Finance Claim', 'مطالبة التمويل الاستهلاكي', 'The customer''s upfront charges claimed at the agreement.'),
                                 ('CF_CLAIM_PAYMENT', 'Consumer Finance Claim Payment', 'سداد مطالبة التمويل الاستهلاكي', 'The customer paid the upfront claim (charges and the down payment we take).'))
                         AS c(code, en, ar, descr)
                   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" x WHERE x."CATEGORY_CODE" = c.code);

                  -- the accounts, each with the settings and parent of an account of the same kind
                  FOR acct IN SELECT * FROM (VALUES
                      ('120801', 'Consumer Finance Receivable - Principal', 'ذمم التمويل الاستهلاكي - أصل', 'Consumer finances booked at delivery: what the customers owe.', 1208, '120101'),
                      ('120802', 'Consumer Finance Charge Receivable', 'ذمم رسوم التمويل الاستهلاكي', 'Upfront charges claimed from customers and not paid yet.', 1209, '120103'),
                      ('120803', 'Consumer Finance Unearned Fees', 'رسوم التمويل الاستهلاكي غير المكتسبة', 'Fees not earned yet: claimed and unpaid, or added to the finance and released with the schedule.', 1210, '120404'),
                      ('230801', 'Payable to Suppliers - Consumer Finance', 'مستحقات الموردين - التمويل الاستهلاكي', 'What we owe the suppliers for goods delivered on finance.', 2308, '230103'),
                      ('230802', 'Down Payments Held for Suppliers', 'مقدمات محتفظ بها للموردين', 'Down payments we took from customers, paid on to the supplier.', 2309, '230104'),
                      ('410801', 'Consumer Finance Fee Income', 'إيرادات رسوم التمويل الاستهلاكي', 'Administration and other fees of consumer finances.', 4109, '410103'),
                      ('410802', 'Consumer Finance Supplier Commission Income', 'إيرادات عمولات موردي التمويل الاستهلاكي', 'Commission kept from the payments to suppliers.', 4110, '410101'))
                      AS t(code, en, ar, descr, sort, tmpl)
                  LOOP
                    SELECT * INTO v_tmpl FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = acct.tmpl AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1;
                    IF v_tmpl."ACCOUNT_ID" IS NULL THEN
                      RAISE NOTICE 'GL account % to copy is missing: % not added.', acct.tmpl, acct.code;
                      v_ok := false;
                      CONTINUE;
                    END IF;
                    IF EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tmpl."COA_ID" AND "ACCOUNT_CODE" = acct.code) THEN
                      IF NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tmpl."COA_ID" AND "ACCOUNT_CODE" = acct.code AND "ACCOUNT_NAME_EN" = acct.en) THEN
                        RAISE NOTICE 'GL account % is used by another account: consumer finance GL lines not added.', acct.code;
                        v_ok := false;
                      END IF;
                      CONTINUE;
                    END IF;
                    SELECT * INTO v_sv FROM "GL_COA_SEGMENT_VALUES" WHERE "SEGMENT_VALUE_ID" = v_tmpl."ACCOUNT_SEGMENT_VALUE_ID";
                    IF v_sv."SEGMENT_VALUE_ID" IS NOT NULL THEN
                      INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID",
                                                           "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                                           "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                      SELECT v_sv."COA_SEGMENT_ID", acct.code, acct.en, acct.ar, acct.descr, v_sv."PARENT_VALUE_ID", false, true, true, v_sv."EFFECTIVE_START_DATE", acct.sort, v_by, now(), 1
                       WHERE NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" x WHERE x."COA_SEGMENT_ID" = v_sv."COA_SEGMENT_ID" AND x."VALUE_CODE" = acct.code);
                    END IF;
                    INSERT INTO "GL_ACCOUNTS" ("COA_ID", "ACCOUNT_CODE", "ACCOUNT_NAME_EN", "ACCOUNT_NAME_AR", "DESCRIPTION", "PARENT_ACCOUNT_ID",
                                               "ACCOUNT_SEGMENT_VALUE_ID", "ACCOUNT_TYPE", "NORMAL_BALANCE", "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS",
                                               "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_CONTROL_ACCOUNT", "IS_RECONCILIATION", "IS_BRANCH_REQUIRED",
                                               "IS_COST_CENTER_REQUIRED", "IS_BUSINESS_LINE_REQUIRED", "IS_MANUAL_JOURNAL_ALLOWED", "IS_SUBLEDGER_POSTING_ALLOWED",
                                               "IS_CURRENCY_REVALUATION", "IS_RETAINED_EARNINGS", "IS_ACTIVE", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                               "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    VALUES (v_tmpl."COA_ID", acct.code, acct.en, acct.ar, acct.descr, v_tmpl."PARENT_ACCOUNT_ID",
                            (SELECT "SEGMENT_VALUE_ID" FROM "GL_COA_SEGMENT_VALUES" x WHERE x."COA_SEGMENT_ID" = v_sv."COA_SEGMENT_ID" AND x."VALUE_CODE" = acct.code),
                            v_tmpl."ACCOUNT_TYPE", v_tmpl."NORMAL_BALANCE", v_tmpl."FINANCIAL_STATEMENT_CLASS", v_tmpl."CASH_FLOW_CLASS",
                            false, true, v_tmpl."IS_CONTROL_ACCOUNT", v_tmpl."IS_RECONCILIATION", v_tmpl."IS_BRANCH_REQUIRED",
                            v_tmpl."IS_COST_CENTER_REQUIRED", v_tmpl."IS_BUSINESS_LINE_REQUIRED", v_tmpl."IS_MANUAL_JOURNAL_ALLOWED", v_tmpl."IS_SUBLEDGER_POSTING_ALLOWED",
                            v_tmpl."IS_CURRENCY_REVALUATION", false, true, v_tmpl."EFFECTIVE_START_DATE", acct.sort, v_by, now(), 1);
                  END LOOP;
                  IF NOT v_ok THEN
                    RETURN;
                  END IF;

                  FOR prod IN SELECT "PRODUCT_ID" FROM "ADM_PRODUCTS" WHERE "BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE' LOOP
                    -- the two new rules
                    FOR rl IN SELECT * FROM (VALUES ('CF_CLAIM', 'Upfront claim', 'المطالبة المقدمة', 30), ('CF_CLAIM_PAYMENT', 'Upfront claim payment', 'سداد المطالبة المقدمة', 40))
                        AS t(code, en, ar, prio)
                    LOOP
                      IF NOT EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULES" x WHERE x."PRODUCT_ID" = prod."PRODUCT_ID" AND x."RULE_CODE" = rl.code) THEN
                        SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = rl.code;
                        INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR",
                                                               "PRIORITY_NO", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                        VALUES (prod."PRODUCT_ID", v_source, v_cat, rl.code, rl.en, rl.ar, rl.prio, DATE '2026-01-01', true, v_by, now(), 1);
                      END IF;
                    END LOOP;

                    -- the GL lines of the four rules, added only to a rule that has none yet
                    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                    SELECT er."PRODUCT_EVENT_RULE_ID", NULL, gl.jn, gl.no, gl.dc, gl.amount, gl.src,
                           CASE WHEN gl.acc IS NULL THEN NULL ELSE (SELECT ga."ACCOUNT_ID" FROM "GL_ACCOUNTS" ga WHERE ga."ACCOUNT_CODE" = gl.acc AND ga."IS_ACTIVE" ORDER BY ga."ACCOUNT_ID" LIMIT 1) END,
                           gl.descr, true, v_by, now(), 1
                      FROM "ADM_PRODUCT_EVENT_RULES" er
                      JOIN (VALUES
                          ('CF_CLAIM', 1, 10, 'D', 'CLAIM_CHARGES_AMOUNT', 'ACCOUNT', '120802', 'Upfront charges claimed'),
                          ('CF_CLAIM', 1, 20, 'C', 'CLAIM_CHARGES_AMOUNT', 'ACCOUNT', '120803', 'Upfront charges not earned yet'),
                          ('CF_CLAIM_PAYMENT', 1, 10, 'D', 'PAYMENT_AMOUNT', 'BANK_ACCOUNT', NULL, 'Upfront claim paid'),
                          ('CF_CLAIM_PAYMENT', 1, 20, 'C', 'CHARGES_PAID_AMOUNT', 'ACCOUNT', '120802', 'Upfront charges paid'),
                          ('CF_CLAIM_PAYMENT', 1, 30, 'C', 'DOWN_PAYMENT_AMOUNT', 'ACCOUNT', '230802', 'Down payment held for the supplier'),
                          ('CF_CLAIM_PAYMENT', 2, 10, 'D', 'CHARGES_PAID_AMOUNT', 'ACCOUNT', '120803', 'Upfront charges earned'),
                          ('CF_CLAIM_PAYMENT', 2, 20, 'C', 'CHARGES_PAID_AMOUNT', 'ACCOUNT', '410801', 'Consumer finance fee income'),
                          ('CF_DELIVERY', 1, 10, 'D', 'LOAN_AMOUNT', 'ACCOUNT', '120801', 'Consumer finance booked at delivery'),
                          ('CF_DELIVERY', 1, 20, 'C', 'FINANCED_AMOUNT', 'ACCOUNT', '230801', 'Payable to the supplier'),
                          ('CF_DELIVERY', 1, 30, 'C', 'CAPITALISED_CHARGES_AMOUNT', 'ACCOUNT', '120803', 'Charges added to the finance, not earned yet'),
                          ('CF_DELIVERY', 2, 10, 'D', 'DOWN_PAYMENT_HELD_AMOUNT', 'ACCOUNT', '230802', 'Down payment held, now due to the supplier'),
                          ('CF_DELIVERY', 2, 20, 'C', 'DOWN_PAYMENT_HELD_AMOUNT', 'ACCOUNT', '230801', 'Payable to the supplier'),
                          ('CF_SUPPLIER_PAYMENT', 1, 10, 'D', 'PAYABLE_AMOUNT', 'ACCOUNT', '230801', 'Payable to the supplier settled'),
                          ('CF_SUPPLIER_PAYMENT', 1, 20, 'C', 'COMMISSION_AMOUNT', 'ACCOUNT', '410802', 'Supplier commission'),
                          ('CF_SUPPLIER_PAYMENT', 1, 30, 'C', 'NET_PAYMENT_AMOUNT', 'BANK_ACCOUNT', NULL, 'Paid to the supplier'))
                          AS gl(rule, jn, no, dc, amount, src, acc, descr) ON gl.rule = er."RULE_CODE"
                     WHERE er."PRODUCT_ID" = prod."PRODUCT_ID"
                       AND NOT EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" x WHERE x."PRODUCT_EVENT_RULE_ID" = er."PRODUCT_EVENT_RULE_ID");
                  END LOOP;

                  -- recording the claim payment is a button (product action CF_CLAIM_PAYMENT), linked to its rule
                  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
                  SELECT er."PRODUCT_ID", 'CF_CLAIM_PAYMENT', er."PRODUCT_EVENT_RULE_ID", true, v_by, now(), 1
                    FROM "ADM_PRODUCT_EVENT_RULES" er JOIN "ADM_PRODUCTS" pr ON pr."PRODUCT_ID" = er."PRODUCT_ID"
                   WHERE er."RULE_CODE" = 'CF_CLAIM_PAYMENT' AND pr."BUSINESS_LINE_CODE" = 'CONSUMER_FINANCE'
                   ORDER BY (pr."PRODUCT_CODE" = 'CONSUMER_GOODS_FINANCE') DESC, pr."PRODUCT_ID"
                   LIMIT 1
                  ON CONFLICT ("ACTION_CODE") DO NOTHING;
                END $$;
                """);

            // the Agreement step posts the claim (a workflow step links its rule by finova:ruleCode)
            foreach (var table in new[] { "WFE_WORKFLOW_DEFINITIONS", "WFE_DEFINITION_VERSIONS" })
            {
                var filter = table == "WFE_WORKFLOW_DEFINITIONS"
                    ? "\"WORKFLOW_CODE\" = 'CF_FINANCE'"
                    : "\"WORKFLOW_DEFINITION_ID\" IN (SELECT \"WORKFLOW_DEFINITION_ID\" FROM \"WFE_WORKFLOW_DEFINITIONS\" WHERE \"WORKFLOW_CODE\" = 'CF_FINANCE')";
                migrationBuilder.Sql($"""
                    UPDATE "{table}" SET "BPMN_XML" = replace("BPMN_XML", 'finova:stepCode="CF_AGREEMENT" ', 'finova:stepCode="CF_AGREEMENT" finova:ruleCode="CF_CLAIM" ')
                    WHERE {filter} AND "BPMN_XML" NOT LIKE '%finova:ruleCode="CF_CLAIM"%';
                    """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
