-- =====================================================================
-- Capitra: GL of the cheque batch submission (Cheques Wallet) from a product rule
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5.
-- First run migrate-cheque-batch-gl.sql (creates GL_TRAN_SET_SEQUENCES and ADM_PRODUCT_ACTIONS),
-- unless the API applies migrations on startup and was started with the latest develop.
--
-- It sets up (agreed with Ahmed, 30 Oct 2026):
--   1. memorandum (off-balance-sheet) accounts in the chart of accounts:
--        900000 Memorandum Accounts (summary)
--        910101 Cheques Received in Custody      (debit)
--        910102 Clients' Cheques in Custody      (credit)
--      FINANCIAL_STATEMENT_CLASS OFF_BALANCE_SHEET: in the trial balance, not in the balance sheet or income statement
--   2. journal category CHW_RECEIVED and business event CHW_CHEQUES_RECEIVED (source FACTORING)
--   3. product CHEQUE_WALLET "Cheques Wallet"
--   4. rule CHW_CHEQUES_RECEIVED: Dr 910101 / Cr 910102, amount CHEQUE_AMOUNT (the batch's cheques)
--   5. the product action CHW_BATCH_SUBMIT "Cheque batch submission" linked to that rule
-- From then on Submit to the wallet posts these entries under CAP-CHW-<year>-<number>,
-- in the same transaction as the cheques: no rule or a GL error means nothing is saved.
-- Safe to run again: what exists is kept. One transaction: any error changes nothing.
-- =====================================================================
DO $$
DECLARE
  v_by       text := 'SETUP';
  v_from     date := DATE '2020-01-01';   -- rule in effect from
  v_coa      bigint;
  v_natural  bigint;
  v_sv_root  bigint;
  v_acc_root bigint;
  v_source   bigint;
  v_cat      bigint;
  v_event    bigint;
  v_product  bigint;
  v_rule     bigint;
  a          record;
BEGIN
  IF to_regclass('"ADM_PRODUCT_ACTIONS"') IS NULL OR to_regclass('"GL_TRAN_SET_SEQUENCES"') IS NULL THEN
    RAISE EXCEPTION 'ADM_PRODUCT_ACTIONS or GL_TRAN_SET_SEQUENCES is missing. Run migrate-cheque-batch-gl.sql first. Nothing changed.';
  END IF;

  -- the chart of accounts the other accounts are in, and its natural account segment
  SELECT "COA_ID" INTO v_coa FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = '999999';
  SELECT "COA_SEGMENT_ID" INTO v_natural FROM "GL_COA_SEGMENTS" WHERE "COA_ID" = v_coa AND "SEGMENT_CODE" = 'NATURAL';
  IF v_coa IS NULL OR v_natural IS NULL THEN
    RAISE EXCEPTION 'The chart of accounts (account 999999 and its NATURAL segment) was not found. Nothing changed.';
  END IF;

  -- 1. segment values and accounts
  FOR a IN SELECT * FROM (VALUES
      ('900000', 'Memorandum Accounts', 'الحسابات النظامية', 'Off-balance-sheet memorandum accounts: items held in custody, not money of the company.',
       true, 'ASSET', 'DR', 9000),
      ('910101', 'Cheques Received in Custody', 'شيكات برسم التحصيل في العهدة', 'Memo: client cheques received in the cheques wallet, not yet collected.',
       false, 'ASSET', 'DR', 9010),
      ('910102', 'Clients'' Cheques in Custody', 'عملاء - شيكات في العهدة', 'Memo contra: the clients the cheques in custody belong to.',
       false, 'LIABILITY', 'CR', 9020)) AS t(code, en, ar, descr, summary, typ, nb, sort)
  LOOP
    INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID",
                                         "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER",
                                         "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT v_natural, a.code, a.en, a.ar, a.descr, CASE WHEN a.summary THEN NULL ELSE v_sv_root END,
           a.summary, NOT a.summary, true, DATE '2026-01-01', a.sort, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_natural AND "VALUE_CODE" = a.code);

    INSERT INTO "GL_ACCOUNTS" ("COA_ID", "ACCOUNT_CODE", "ACCOUNT_NAME_EN", "ACCOUNT_NAME_AR", "DESCRIPTION", "PARENT_ACCOUNT_ID",
                               "ACCOUNT_SEGMENT_VALUE_ID", "ACCOUNT_TYPE", "NORMAL_BALANCE", "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS",
                               "IS_SUMMARY", "IS_POSTING_ALLOWED", "IS_CONTROL_ACCOUNT", "IS_RECONCILIATION", "IS_BRANCH_REQUIRED",
                               "IS_COST_CENTER_REQUIRED", "IS_BUSINESS_LINE_REQUIRED", "IS_MANUAL_JOURNAL_ALLOWED", "IS_SUBLEDGER_POSTING_ALLOWED",
                               "IS_CURRENCY_REVALUATION", "IS_RETAINED_EARNINGS", "IS_ACTIVE", "EFFECTIVE_START_DATE", "SORT_ORDER",
                               "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT v_coa, a.code, a.en, a.ar, a.descr, CASE WHEN a.summary THEN NULL ELSE v_acc_root END,
           (SELECT "SEGMENT_VALUE_ID" FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_natural AND "VALUE_CODE" = a.code),
           a.typ, a.nb, 'OFF_BALANCE_SHEET', 'NONE',
           a.summary, NOT a.summary, false, false, NOT a.summary,
           NOT a.summary, NOT a.summary, false, NOT a.summary,
           false, false, true, DATE '2026-01-01', a.sort, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_coa AND "ACCOUNT_CODE" = a.code);

    IF a.summary THEN
      SELECT "SEGMENT_VALUE_ID" INTO v_sv_root FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_natural AND "VALUE_CODE" = a.code;
      SELECT "ACCOUNT_ID" INTO v_acc_root FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_coa AND "ACCOUNT_CODE" = a.code;
    END IF;
  END LOOP;

  -- 2. journal category and business event, source FACTORING
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source FACTORING is missing. Nothing changed.';
  END IF;
  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'CHW_RECEIVED', 'Cheques Received in Wallet', 'استلام شيكات في المحفظة', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
         'Memo entries of client cheques received in the cheques wallet (cheque batch submission).', v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'CHW_RECEIVED');
  SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'CHW_RECEIVED';

  SELECT "EVENT_TYPE_ID" INTO v_event FROM "GL_ACCOUNTING_EVENT_TYPES" WHERE "EVENT_TYPE_CODE" = 'CHW_CHEQUES_RECEIVED';
  IF v_event IS NULL THEN
    INSERT INTO "GL_ACCOUNTING_EVENT_TYPES" ("SOURCE_ID", "EVENT_TYPE_CODE", "EVENT_TYPE_NAME_EN", "EVENT_TYPE_NAME_AR", "DEFAULT_CATEGORY_ID",
                                             "IS_ACCOUNTING_REQUIRED", "IS_MANUAL_ADJUST_ALLOWED", "IS_ACTIVE", "DESCRIPTION", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_source, 'CHW_CHEQUES_RECEIVED', 'Cheques received in wallet', 'استلام شيكات في المحفظة', v_cat,
            true, false, true, 'Client cheques of a cheque batch enter the cheques wallet (not yet collected).', v_by, now(), 1)
    RETURNING "EVENT_TYPE_ID" INTO v_event;
    RAISE NOTICE 'business event CHW_CHEQUES_RECEIVED created';
  END IF;

  -- 3. product CHEQUE_WALLET
  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'CHEQUE_WALLET';
  IF v_product IS NULL THEN
    INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('FACTORING', 'Factoring', 'التخصيم', 2, true, v_by, now(), 1)
    ON CONFLICT ("CODE") DO NOTHING;
    INSERT INTO "ADM_PRODUCTS" ("PRODUCT_CODE", "PRODUCT_NAME_EN", "PRODUCT_NAME_AR", "BUSINESS_LINE_CODE", "PRODUCT_TYPE_CODE",
                                "DEFAULT_CURRENCY_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "MPP_PV_RATE", "MPP_LATE_FEE_RATE",
                                "MPP_RESERVED_AMOUNT_PCT", "MPP_INVESTOR_PAYMENT_PCT", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('CHEQUE_WALLET', 'Cheques Wallet', 'محفظة الشيكات', 'FACTORING', 'CHEQUE_WALLET', 'EGP', v_from, true, 0, 0, 0, 0, v_by, now(), 1)
    RETURNING "PRODUCT_ID" INTO v_product;
    RAISE NOTICE 'product CHEQUE_WALLET created';
  END IF;

  -- 4. the rule and its GL entries
  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES"
   WHERE "PRODUCT_ID" = v_product AND "EVENT_TYPE_ID" = v_event AND "RULE_CODE" = 'CHW_CHEQUES_RECEIVED';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule CHW_CHEQUES_RECEIVED exists (id %): its GL entries are left as they are', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "EVENT_TYPE_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_event, 'CHW_CHEQUES_RECEIVED', 'Cheques received in wallet', 'استلام شيكات في المحفظة', 1, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;

    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT v_rule, NULL, 1, t.no, t.dc, 'CHEQUE_AMOUNT', 'ACCOUNT',
           (SELECT "ACCOUNT_ID" FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_coa AND "ACCOUNT_CODE" = t.acct),
           t.descr, true, v_by, now(), 1
      FROM (VALUES
        (10, 'D', '910101', 'Cheques received in custody'),
        (20, 'C', '910102', 'Clients'' cheques in custody')) AS t(no, dc, acct, descr);
    RAISE NOTICE 'rule CHW_CHEQUES_RECEIVED created (id %)', v_rule;
  END IF;

  -- 5. the product action Cheque batch submission
  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES (v_product, 'CHW_BATCH_SUBMIT', v_rule, true, v_by, now(), 1)
  ON CONFLICT ("ACTION_CODE") DO NOTHING;
  RAISE NOTICE 'action CHW_BATCH_SUBMIT: linked to rule % (an existing link was kept)', v_rule;
END $$;

-- what was set up
SELECT p."PRODUCT_CODE", x."ACTION_CODE", r."RULE_CODE", x."IS_ACTIVE"
  FROM "ADM_PRODUCT_ACTIONS" x
  JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = x."PRODUCT_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULES" r ON r."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID";
