-- =====================================================================
-- Capitra: GL accounts and GL entries of the factoring disbursement charges (rule DISBURSEMENT_APPROVED)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Run setup-fct-disbursement-rule.sql before this one.
--
-- Capitra design (agreed with Ahmed, 31 Oct 2026): DR customer receivable, CR fee income, no account repeated.
-- The chart codes accounts by product (4101xx = factoring) and parents them by nature (420000 Fee and Commission Income).
--   1. three new posting accounts under 420000, copied from 410101's settings:
--        410104 Factoring Facility Fee Income
--        410105 Factoring Supplier Fee Income
--        410106 Factoring Opening Account Fee Income
--   2. each charge:  DR 120103 Factoring Client Charge Receivable (exists)
--                    CR its fee income account:
--        DISB_GROSS_COMMISSION   410101 Factoring Commission and Service Fee Income   (exists)
--        HANDLING_PER_DISB       410103 Factoring Handling and Administration Fees    (exists)
--        FACILITY_LIMIT_PERCENT  410104 Factoring Facility Fee Income                 (new)
--        SUPPLIER_FIXED          410105 Factoring Supplier Fee Income                 (new)
--        OPENING_ACCOUNT_FIXED   410106 Factoring Opening Account Fee Income          (new)
--      amount CHARGE_AMOUNT. No tax line: these charges have no tax.
-- Applies to every product with rule DISBURSEMENT_APPROVED (Reverse and both Classic products).
-- Nothing is posted yet: the lines are used when GL is built with the workflow.
-- Safe to run again: an account that exists, or a charge that has GL lines, is left as it is.
-- One transaction: any error changes nothing.
-- =====================================================================
DO $$
DECLARE
  v_by      text := 'SETUP';
  v_missing text;
  v_tmpl    record;
  v_sv      record;
  v_n       int := 0;
  a         record;
  r         record;
BEGIN
  -- the accounts that must exist already
  SELECT string_agg(code, ', ') INTO v_missing
    FROM unnest(ARRAY['120103', '410101', '410103', '420000']) code
   WHERE NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" x WHERE x."ACCOUNT_CODE" = code AND x."IS_ACTIVE");
  IF v_missing IS NOT NULL THEN
    RAISE EXCEPTION 'These GL accounts are missing or inactive: %. Nothing changed.', v_missing;
  END IF;

  -- 1. new fee income accounts, with the settings of 410101 and its segment value
  SELECT * INTO v_tmpl FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = '410101' ORDER BY "ACCOUNT_ID" LIMIT 1;
  SELECT * INTO v_sv FROM "GL_COA_SEGMENT_VALUES" WHERE "SEGMENT_VALUE_ID" = v_tmpl."ACCOUNT_SEGMENT_VALUE_ID";

  FOR a IN SELECT * FROM (VALUES
      ('410104', 'Factoring Facility Fee Income', 'إيرادات رسوم التسهيل - التخصيم', 'Facility fee on the approved factoring limit.', 4104),
      ('410105', 'Factoring Supplier Fee Income', 'إيرادات رسوم المورد - التخصيم', 'Supplier fee of factoring disbursements.', 4105),
      ('410106', 'Factoring Opening Account Fee Income', 'إيرادات رسوم فتح الحساب - التخصيم', 'Opening account fee of factoring clients.', 4106))
      AS t(code, en, ar, descr, sort)
  LOOP
    IF v_sv."SEGMENT_VALUE_ID" IS NOT NULL THEN
      INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID",
                                           "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                           "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      SELECT v_sv."COA_SEGMENT_ID", a.code, a.en, a.ar, a.descr, v_sv."PARENT_VALUE_ID",
             false, true, true, v_sv."EFFECTIVE_START_DATE", a.sort, v_by, now(), 1
       WHERE NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_sv."COA_SEGMENT_ID" AND "VALUE_CODE" = a.code);
    END IF;

    IF EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tmpl."COA_ID" AND "ACCOUNT_CODE" = a.code) THEN
      RAISE NOTICE 'account % exists: left as it is', a.code;
      CONTINUE;
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
            v_tmpl."IS_CURRENCY_REVALUATION", false, true, v_tmpl."EFFECTIVE_START_DATE", a.sort,
            v_by, now(), 1);
    RAISE NOTICE 'account % % added under 420000', a.code, a.en;
  END LOOP;

  -- 2. GL lines of each charge on every DISBURSEMENT_APPROVED rule
  FOR r IN
    SELECT rc."PRODUCT_EVENT_RULE_ID" AS rule_id, rc."RULE_CHARGE_ID" AS rc_id, ch."CHARGE_CODE" AS code, p."PRODUCT_CODE" AS product,
           CASE ch."CHARGE_CODE"
             WHEN 'DISB_GROSS_COMMISSION'  THEN '410101'
             WHEN 'HANDLING_PER_DISB'      THEN '410103'
             WHEN 'FACILITY_LIMIT_PERCENT' THEN '410104'
             WHEN 'SUPPLIER_FIXED'         THEN '410105'
             WHEN 'OPENING_ACCOUNT_FIXED'  THEN '410106'
           END AS income
      FROM "ADM_PRODUCT_EVENT_RULE_CHARGES" rc
      JOIN "ADM_PRODUCT_EVENT_RULES" er ON er."PRODUCT_EVENT_RULE_ID" = rc."PRODUCT_EVENT_RULE_ID"
      JOIN "ADM_PRODUCTS" p             ON p."PRODUCT_ID" = er."PRODUCT_ID"
      JOIN "ADM_CHARGES" ch             ON ch."CHARGE_ID" = rc."CHARGE_ID"
     WHERE er."RULE_CODE" = 'DISBURSEMENT_APPROVED'
       AND ch."CHARGE_CODE" IN ('DISB_GROSS_COMMISSION', 'HANDLING_PER_DISB', 'FACILITY_LIMIT_PERCENT', 'SUPPLIER_FIXED', 'OPENING_ACCOUNT_FIXED')
  LOOP
    IF EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" WHERE "RULE_CHARGE_ID" = r.rc_id) THEN
      RAISE NOTICE '% / %: has GL lines already, left as it is', r.product, r.code;
      CONTINUE;
    END IF;
    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT r.rule_id, r.rc_id, 1, t.no, t.dc, 'CHARGE_AMOUNT', 'ACCOUNT',
           (SELECT "ACCOUNT_ID" FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = t.acct AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1),
           t.descr, true, v_by, now(), 1
      FROM (VALUES (10, 'D', '120103', 'Client charge receivable - ' || r.code),
                   (20, 'C', r.income, 'Factoring fee income - ' || r.code)) AS t(no, dc, acct, descr);
    v_n := v_n + 1;
    RAISE NOTICE '% / %: DR 120103, CR %', r.product, r.code, r.income;
  END LOOP;
  RAISE NOTICE '% charges got their GL lines', v_n;
END $$;

-- what is set up
SELECT p."PRODUCT_CODE", ch."CHARGE_CODE", g."LINE_NO", g."DEBIT_CREDIT_CODE", a."ACCOUNT_CODE", a."ACCOUNT_NAME_EN", g."AMOUNT_CODE"
  FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" g
  JOIN "ADM_PRODUCT_EVENT_RULE_CHARGES" rc ON rc."RULE_CHARGE_ID" = g."RULE_CHARGE_ID"
  JOIN "ADM_PRODUCT_EVENT_RULES" er ON er."PRODUCT_EVENT_RULE_ID" = g."PRODUCT_EVENT_RULE_ID"
  JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = er."PRODUCT_ID"
  JOIN "ADM_CHARGES" ch ON ch."CHARGE_ID" = rc."CHARGE_ID"
  LEFT JOIN "GL_ACCOUNTS" a ON a."ACCOUNT_ID" = g."ACCOUNT_ID"
 WHERE er."RULE_CODE" = 'DISBURSEMENT_APPROVED'
 ORDER BY p."PRODUCT_CODE", rc."SEQUENCE_NO", g."LINE_NO";
