-- =====================================================================================================================
-- Capitra: one GL bank account per bank (agreed with Ahmed, 1 Nov 2026)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again. One block: any error changes nothing.
--
--   1. 110102 "Main Bank Account - EGP" is renamed "Main Bank Account - EGP - NBE"
--   2. two new GL accounts, copied from 110102 (same parent 110000, type and flags), named the same way:
--        110106 Main Bank Account - EGP - CIB
--        110107 Main Bank Account - EGP - Banque Misr
--      each with its NATURAL segment value, as the GL needs
--   3. every EGP bank account in Treasury (TRS_BANK_ACCOUNTS.ERP_ACCOUNT_CODE) takes its bank's GL account:
--        NBE -> 110102, CIB -> 110106, BANQUE_MISR -> 110107
-- A cheque collection then posts DR each bank's GL account / CR 230104 customer liability.
-- =====================================================================================================================

DO $$
DECLARE
  v_by   text := 'SETUP';
  t      "GL_ACCOUNTS"%ROWTYPE;
  tv     "GL_COA_SEGMENT_VALUES"%ROWTYPE;
  b      record;
  v_sv   bigint;
BEGIN
  SELECT * INTO t FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = '110102' ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF t."ACCOUNT_ID" IS NULL THEN
    RAISE EXCEPTION 'GL account 110102 Main Bank Account - EGP was not found. Nothing changed.';
  END IF;
  SELECT * INTO tv FROM "GL_COA_SEGMENT_VALUES" WHERE "SEGMENT_VALUE_ID" = t."ACCOUNT_SEGMENT_VALUE_ID";
  IF tv."SEGMENT_VALUE_ID" IS NULL THEN
    RAISE EXCEPTION 'GL account 110102 has no NATURAL segment value. Nothing changed.';
  END IF;

  -- 1. the current bank account is NBE's
  UPDATE "GL_ACCOUNTS" SET "ACCOUNT_NAME_EN" = 'Main Bank Account - EGP - NBE', "ACCOUNT_NAME_AR" = 'حساب البنك الرئيسي - جنيه - البنك الأهلي المصري',
                           "UPDATED_BY" = v_by, "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "ACCOUNT_ID" = t."ACCOUNT_ID" AND "ACCOUNT_NAME_EN" <> 'Main Bank Account - EGP - NBE';
  UPDATE "GL_COA_SEGMENT_VALUES" SET "VALUE_NAME_EN" = 'Main Bank Account - EGP - NBE', "VALUE_NAME_AR" = 'حساب البنك الرئيسي - جنيه - البنك الأهلي المصري',
                                     "UPDATED_BY" = v_by, "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "SEGMENT_VALUE_ID" = tv."SEGMENT_VALUE_ID" AND "VALUE_NAME_EN" <> 'Main Bank Account - EGP - NBE';

  -- 2. CIB and Banque Misr, copied from 110102
  FOR b IN SELECT * FROM (VALUES
      ('110106', 'Main Bank Account - EGP - CIB', 'حساب البنك الرئيسي - جنيه - البنك التجاري الدولي', 1061),
      ('110107', 'Main Bank Account - EGP - Banque Misr', 'حساب البنك الرئيسي - جنيه - بنك مصر', 1071)) AS x(code, en, ar, sort)
  LOOP
    IF EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = t."COA_ID" AND "ACCOUNT_CODE" = b.code AND "ACCOUNT_NAME_EN" <> b.en) THEN
      RAISE EXCEPTION 'GL account % already exists with another name. Nothing changed.', b.code;
    END IF;

    INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID",
                                         "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                         "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT tv."COA_SEGMENT_ID", b.code, b.en, b.ar, tv."DESCRIPTION", tv."PARENT_VALUE_ID",
           tv."IS_SUMMARY", tv."IS_POSTING_ALLOWED", tv."IS_ENABLED", tv."EFFECTIVE_START_DATE", b.sort, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = tv."COA_SEGMENT_ID" AND "VALUE_CODE" = b.code);
    SELECT "SEGMENT_VALUE_ID" INTO v_sv FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = tv."COA_SEGMENT_ID" AND "VALUE_CODE" = b.code;

    INSERT INTO "GL_ACCOUNTS" ("COA_ID", "ACCOUNT_CODE", "ACCOUNT_NAME_EN", "ACCOUNT_NAME_AR", "DESCRIPTION", "PARENT_ACCOUNT_ID",
                               "ACCOUNT_SEGMENT_VALUE_ID", "ACCOUNT_TYPE", "NORMAL_BALANCE", "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS",
                               "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_CONTROL_ACCOUNT", "IS_RECONCILIATION", "IS_BRANCH_REQUIRED",
                               "IS_COST_CENTER_REQUIRED", "IS_BUSINESS_LINE_REQUIRED", "IS_MANUAL_JOURNAL_ALLOWED", "IS_SUBLEDGER_POSTING_ALLOWED",
                               "IS_CURRENCY_REVALUATION", "IS_RETAINED_EARNINGS", "IS_ACTIVE", "EFFECTIVE_START_DATE", "SORT_ORDER",
                               "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT t."COA_ID", b.code, b.en, b.ar, t."DESCRIPTION", t."PARENT_ACCOUNT_ID",
           v_sv, t."ACCOUNT_TYPE", t."NORMAL_BALANCE", t."FINANCIAL_STATEMENT_CLASS", t."CASH_FLOW_CLASS",
           t."IS_SUMMARY", t."IS_POSTING_ALLOWED", t."IS_CONTROL_ACCOUNT", t."IS_RECONCILIATION", t."IS_BRANCH_REQUIRED",
           t."IS_COST_CENTER_REQUIRED", t."IS_BUSINESS_LINE_REQUIRED", t."IS_MANUAL_JOURNAL_ALLOWED", t."IS_SUBLEDGER_POSTING_ALLOWED",
           t."IS_CURRENCY_REVALUATION", t."IS_RETAINED_EARNINGS", t."IS_ACTIVE", t."EFFECTIVE_START_DATE", b.sort, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = t."COA_ID" AND "ACCOUNT_CODE" = b.code);
  END LOOP;

  -- 3. each EGP bank account posts to its bank's GL account
  UPDATE "TRS_BANK_ACCOUNTS" a
     SET "ERP_ACCOUNT_CODE" = CASE k."BANK_CODE" WHEN 'NBE' THEN '110102' WHEN 'CIB' THEN '110106' WHEN 'BANQUE_MISR' THEN '110107' END,
         "UPDATED_BY" = v_by, "UPDATED_AT" = now(), "ROW_VERSION" = a."ROW_VERSION" + 1
    FROM "TRS_BANKS" k
   WHERE k."BANK_ID" = a."BANK_ID" AND a."CURRENCY_CODE" = 'EGP' AND k."BANK_CODE" IN ('NBE', 'CIB', 'BANQUE_MISR');
END $$;

-- the result: each bank account, its GL account and the check
SELECT a."ACCOUNT_ID", k."BANK_CODE", a."ACCOUNT_NO", a."ACCOUNT_TYPE_CODE", a."ERP_ACCOUNT_CODE" AS gl_account, g."ACCOUNT_NAME_EN" AS gl_account_name,
       CASE WHEN g."ACCOUNT_ID" IS NULL THEN 'NOT IN CHART' WHEN g."IS_POSTING_ALLOWED" AND g."IS_ACTIVE" THEN 'OK' ELSE 'NOT A POSTING ACCOUNT' END AS gl_check
  FROM "TRS_BANK_ACCOUNTS" a
  JOIN "TRS_BANKS" k ON k."BANK_ID" = a."BANK_ID"
  LEFT JOIN "GL_ACCOUNTS" g ON g."ACCOUNT_CODE" = a."ERP_ACCOUNT_CODE"
 ORDER BY k."BANK_CODE", a."ACCOUNT_NO";
