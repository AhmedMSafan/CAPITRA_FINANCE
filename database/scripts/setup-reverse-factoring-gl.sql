-- =====================================================================================================================
-- Capitra: GL of reverse factoring on Treasury Disburse, agreed with Ahmed, 7 Nov 2026. Kept apart from the factoring rule TRS_DISBURSE.
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again: what exists is kept.
-- Needs: setup-treasury-disburse-gl.sql (product TREASURY_FUNDING) and migrate-reverse-supplier-transfers.sql.
--
-- HOW TO RUN
--   a) Leave v_apply = false and run: the Messages tab shows the two GL accounts it will create and use. Nothing changes.
--   b) If it looks right, set v_apply = true and run again.
--
-- The two GL accounts (leave the codes empty to have them created):
--   reverse factoring receivable    the first free code after 120101 (Factoring Receivable - Principal), same parent and settings
--   reverse factoring borrowings    the first free code after 220101 (Bank Facilities - Short Term), same parent and settings
--   Each gets its NATURAL segment value too, so it can be posted to. To use accounts that already exist, type their codes instead.
--   The business line segment value REVERSE_FACTORING is added (copied from FACTORING) when it is missing.
--
-- It then sets up, under product TREASURY_FUNDING:
--   1. journal category TRS_RF "Reverse Factoring Treasury" under the journal source FACTORING
--   2. rule TRS_RF_DRAWDOWN, posted at Execute (bank lines only), amount BANK_DRAWDOWN:
--        DR our account at that bank (its GL account in Bank Limits) / CR reverse factoring bank borrowings
--   3. rule TRS_RF_TRANSFER, posted when the Confirmed box of a supplier transfer is ticked, amount PAYMENT_AMOUNT
--      (one GL transaction set per supplier):
--        DR reverse factoring receivable / CR the bank account the transfer was paid from
--   4. the actions TRS_RF_DRAWDOWN and TRS_RF_TRANSFER_CONFIRM linked to those rules
-- Example, net 6,208,792.99 from Bank A, three suppliers:
--   Execute:   DR Bank A 6,208,792.99 / CR RF borrowings 6,208,792.99
--   Confirm 1: DR RF receivable 2,500,000.00 / CR Bank A 2,500,000.00   (and the same for suppliers 2 and 3)
-- The accounts can be changed later in Admin Setup > Product Catalog > TREASURY_FUNDING > rules TRS_RF_DRAWDOWN, TRS_RF_TRANSFER.
-- =====================================================================================================================
DO $$
DECLARE
  -- >>> settings <<<
  v_rf_borrowings_code text    := '';      -- reverse factoring bank borrowings ('' = create it after 220101)
  v_rf_receivable_code text    := '';      -- reverse factoring receivable from the buyer ('' = create it after 120101)
  v_apply              boolean := false;   -- false = preview only, true = change the data
  -- >>> end of settings <<<
  v_tpl     record;
  v_seg     bigint;
  v_sv      bigint;
  v_n       bigint;
  v_code    text;
  v_k       int;
  v_bl      record;
  v_by      text := 'SETUP';
  v_from    date := DATE '2020-01-01';
  v_source  bigint;
  v_cat     bigint;
  v_product bigint;
  v_rule    bigint;
  v_recv    bigint;
  v_borr    bigint;
BEGIN
  IF to_regclass('"TRS_SUPPLIER_TRANSFERS"') IS NULL THEN
    RAISE EXCEPTION 'TRS_SUPPLIER_TRANSFERS is missing. Run migrate-reverse-supplier-transfers.sql first. Nothing changed.';
  END IF;
  -- 0. the two GL accounts: given, or created after their factoring templates
  FOR v_k IN 1..2 LOOP
    v_code := trim(CASE v_k WHEN 1 THEN v_rf_receivable_code ELSE v_rf_borrowings_code END);
    IF v_code <> '' THEN
      IF NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = v_code) THEN
        RAISE EXCEPTION 'GL account % was not found. Leave the code empty to have it created. Nothing changed.', v_code;
      END IF;
      RAISE NOTICE '% account: % (existing)', CASE v_k WHEN 1 THEN 'Receivable' ELSE 'Borrowings' END, v_code;
      CONTINUE;
    END IF;
    SELECT * INTO v_tpl FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = CASE v_k WHEN 1 THEN '120101' ELSE '220101' END ORDER BY "ACCOUNT_ID" LIMIT 1;
    IF v_tpl."ACCOUNT_ID" IS NULL THEN
      RAISE EXCEPTION 'GL account % (the template) was not found. Type the account codes at the top instead. Nothing changed.',
        CASE v_k WHEN 1 THEN '120101' ELSE '220101' END;
    END IF;
    -- created by an earlier run of this script: use it
    SELECT "ACCOUNT_CODE" INTO v_code FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tpl."COA_ID"
       AND "ACCOUNT_NAME_EN" = CASE v_k WHEN 1 THEN 'Reverse Factoring Receivable' ELSE 'Reverse Factoring Bank Borrowings' END ORDER BY "ACCOUNT_ID" LIMIT 1;
    IF v_code IS NOT NULL THEN
      RAISE NOTICE '% account: % (created earlier)', CASE v_k WHEN 1 THEN 'Receivable' ELSE 'Borrowings' END, v_code;
      IF v_k = 1 THEN v_rf_receivable_code := v_code; ELSE v_rf_borrowings_code := v_code; END IF;
      CONTINUE;
    END IF;
    SELECT "COA_SEGMENT_ID" INTO v_seg FROM "GL_COA_SEGMENTS" WHERE "COA_ID" = v_tpl."COA_ID" AND "SEGMENT_CODE" = 'NATURAL';
    -- the first free code after the template, in the same block of 100 (1201xx / 2201xx)
    v_code := NULL;
    FOR v_n IN (v_tpl."ACCOUNT_CODE"::bigint + 1)..((v_tpl."ACCOUNT_CODE"::bigint / 100) * 100 + 99) LOOP
      IF NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" WHERE "COA_ID" = v_tpl."COA_ID" AND "ACCOUNT_CODE" = v_n::text)
         AND NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" WHERE "COA_SEGMENT_ID" = v_seg AND "VALUE_CODE" = v_n::text) THEN
        v_code := v_n::text; EXIT;
      END IF;
    END LOOP;
    IF v_code IS NULL THEN
      RAISE EXCEPTION 'No free code after %. Type the account codes at the top instead. Nothing changed.', v_tpl."ACCOUNT_CODE";
    END IF;
    RAISE NOTICE '% account: % "%" will be created (copy of % %, same parent)',
      CASE v_k WHEN 1 THEN 'Receivable' ELSE 'Borrowings' END, v_code,
      CASE v_k WHEN 1 THEN 'Reverse Factoring Receivable' ELSE 'Reverse Factoring Bank Borrowings' END, v_tpl."ACCOUNT_CODE", v_tpl."ACCOUNT_NAME_EN";
    IF v_apply THEN
      v_sv := NULL;
      IF v_seg IS NOT NULL THEN
        INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID", "IS_SUMMARY",
                                             "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
        SELECT v_seg, v_code, CASE v_k WHEN 1 THEN 'Reverse Factoring Receivable' ELSE 'Reverse Factoring Bank Borrowings' END,
               CASE v_k WHEN 1 THEN 'مديونية التخصيم العكسي' ELSE 'اقتراض بنكي - تخصيم عكسي' END, 'Reverse factoring (Treasury Disburse)',
               sv."PARENT_VALUE_ID", false, true, true, coalesce(sv."EFFECTIVE_START_DATE", v_from), coalesce(sv."SORT_ORDER", 10), v_by, now(), 1
          FROM (SELECT 1) x LEFT JOIN "GL_COA_SEGMENT_VALUES" sv ON sv."SEGMENT_VALUE_ID" = v_tpl."ACCOUNT_SEGMENT_VALUE_ID"
        RETURNING "SEGMENT_VALUE_ID" INTO v_sv;
      END IF;
      INSERT INTO "GL_ACCOUNTS" ("COA_ID", "ACCOUNT_CODE", "ACCOUNT_NAME_EN", "ACCOUNT_NAME_AR", "DESCRIPTION", "PARENT_ACCOUNT_ID", "ACCOUNT_SEGMENT_VALUE_ID",
                                 "ACCOUNT_TYPE", "NORMAL_BALANCE", "FINANCIAL_STATEMENT_CLASS", "CASH_FLOW_CLASS", "IS_SUMMARY", "IS_POSTING_ALLOWED",
                                 "IS_CONTROL_ACCOUNT", "IS_RECONCILIATION", "IS_BRANCH_REQUIRED", "IS_COST_CENTER_REQUIRED", "IS_BUSINESS_LINE_REQUIRED",
                                 "IS_MANUAL_JOURNAL_ALLOWED", "IS_SUBLEDGER_POSTING_ALLOWED", "IS_CURRENCY_REVALUATION", "IS_RETAINED_EARNINGS", "IS_ACTIVE",
                                 "EFFECTIVE_START_DATE", "SORT_ORDER", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      VALUES (v_tpl."COA_ID", v_code, CASE v_k WHEN 1 THEN 'Reverse Factoring Receivable' ELSE 'Reverse Factoring Bank Borrowings' END,
              CASE v_k WHEN 1 THEN 'مديونية التخصيم العكسي' ELSE 'اقتراض بنكي - تخصيم عكسي' END, 'Reverse factoring (Treasury Disburse)',
              v_tpl."PARENT_ACCOUNT_ID", v_sv, v_tpl."ACCOUNT_TYPE", v_tpl."NORMAL_BALANCE", v_tpl."FINANCIAL_STATEMENT_CLASS", v_tpl."CASH_FLOW_CLASS",
              false, true, v_tpl."IS_CONTROL_ACCOUNT", v_tpl."IS_RECONCILIATION", v_tpl."IS_BRANCH_REQUIRED", v_tpl."IS_COST_CENTER_REQUIRED",
              v_tpl."IS_BUSINESS_LINE_REQUIRED", v_tpl."IS_MANUAL_JOURNAL_ALLOWED", v_tpl."IS_SUBLEDGER_POSTING_ALLOWED", v_tpl."IS_CURRENCY_REVALUATION",
              false, true, v_tpl."EFFECTIVE_START_DATE", v_tpl."SORT_ORDER", v_by, now(), 1);
    END IF;
    IF v_k = 1 THEN v_rf_receivable_code := v_code; ELSE v_rf_borrowings_code := v_code; END IF;
  END LOOP;

  -- the business line the reverse factoring postings carry
  FOR v_bl IN SELECT s."COA_SEGMENT_ID", f.* FROM "GL_COA_SEGMENTS" s
                JOIN "GL_COA_SEGMENT_VALUES" f ON f."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND f."VALUE_CODE" = 'FACTORING'
               WHERE s."SEGMENT_CODE" = 'BUSINESS_LINE'
                 AND NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" r WHERE r."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND r."VALUE_CODE" = 'REVERSE_FACTORING')
  LOOP
    RAISE NOTICE 'Business line segment value REVERSE_FACTORING will be added (copy of FACTORING)';
    IF v_apply THEN
      INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID", "IS_SUMMARY",
                                           "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER", "REFERENCE_BUSINESS_LINE_CODE",
                                           "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      VALUES (v_bl."COA_SEGMENT_ID", 'REVERSE_FACTORING', 'Reverse Factoring', 'التخصيم العكسي', 'Reverse factoring', v_bl."PARENT_VALUE_ID", false, true, true,
              v_bl."EFFECTIVE_START_DATE", v_bl."SORT_ORDER" + 1, 'REVERSE_FACTORING', v_by, now(), 1);
    END IF;
  END LOOP;

  IF NOT v_apply THEN
    RAISE NOTICE 'Preview only. Nothing was changed. Set v_apply := true and run again to create the accounts and the rules.';
    RETURN;
  END IF;
  SELECT "ACCOUNT_ID" INTO v_recv FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = v_rf_receivable_code ORDER BY "ACCOUNT_ID" LIMIT 1;
  SELECT "ACCOUNT_ID" INTO v_borr FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = v_rf_borrowings_code ORDER BY "ACCOUNT_ID" LIMIT 1;

  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source FACTORING is missing. Nothing changed.';
  END IF;
  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'TREASURY_FUNDING';
  IF v_product IS NULL THEN
    RAISE EXCEPTION 'Product TREASURY_FUNDING is missing. Run setup-treasury-disburse-gl.sql first. Nothing changed.';
  END IF;

  -- 1. journal category
  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'TRS_RF', 'Reverse Factoring Treasury', 'خزينة التخصيم العكسي', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
         'Reverse factoring: bank drawdowns at Execute and the confirmed transfers to the suppliers (Treasury Disburse).', v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'TRS_RF');
  SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'TRS_RF';

  -- 2. rule TRS_RF_DRAWDOWN
  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = 'TRS_RF_DRAWDOWN';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule TRS_RF_DRAWDOWN exists (id %): its GL entries are left as they are', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_source, v_cat, 'TRS_RF_DRAWDOWN', 'Reverse factoring bank drawdown', 'سحب بنكي للتخصيم العكسي', 1, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_rule, NULL, 1, 10, 'D', 'BANK_DRAWDOWN', 'BANK_ACCOUNT', NULL,   'Bank drawdown received in our account', true, v_by, now(), 1),
           (v_rule, NULL, 1, 20, 'C', 'BANK_DRAWDOWN', 'ACCOUNT',      v_borr, 'Reverse factoring bank borrowings',     true, v_by, now(), 1);
    RAISE NOTICE 'rule TRS_RF_DRAWDOWN created (id %)', v_rule;
  END IF;
  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES (v_product, 'TRS_RF_DRAWDOWN', v_rule, true, v_by, now(), 1)
  ON CONFLICT ("ACTION_CODE") DO NOTHING;

  -- 3. rule TRS_RF_TRANSFER
  v_rule := NULL;
  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = 'TRS_RF_TRANSFER';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule TRS_RF_TRANSFER exists (id %): its GL entries are left as they are', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_source, v_cat, 'TRS_RF_TRANSFER', 'Reverse factoring supplier transfer', 'تحويل للمورد - تخصيم عكسي', 1, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;
    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_rule, NULL, 1, 10, 'D', 'PAYMENT_AMOUNT', 'ACCOUNT',      v_recv, 'Reverse factoring receivable', true, v_by, now(), 1),
           (v_rule, NULL, 1, 20, 'C', 'PAYMENT_AMOUNT', 'BANK_ACCOUNT', NULL,   'Transferred to the supplier',  true, v_by, now(), 1);
    RAISE NOTICE 'rule TRS_RF_TRANSFER created (id %)', v_rule;
  END IF;
  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES (v_product, 'TRS_RF_TRANSFER_CONFIRM', v_rule, true, v_by, now(), 1)
  ON CONFLICT ("ACTION_CODE") DO NOTHING;
  RAISE NOTICE 'Done. Execute in reverse factoring now posts TRS_RF_DRAWDOWN; each Confirmed box posts TRS_RF_TRANSFER.';
END $$;

-- what was set up
SELECT x."ACTION_CODE", r."RULE_CODE", c."CATEGORY_CODE", l."JOURNAL_NO", l."LINE_NO", l."DEBIT_CREDIT_CODE", l."ACCOUNT_SOURCE_CODE",
       a."ACCOUNT_CODE", l."AMOUNT_CODE"
  FROM "ADM_PRODUCT_ACTIONS" x
  LEFT JOIN "ADM_PRODUCT_EVENT_RULES" r ON r."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "GL_JOURNAL_CATEGORIES" c ON c."CATEGORY_ID" = r."JOURNAL_CATEGORY_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULE_GL_LINES" l ON l."PRODUCT_EVENT_RULE_ID" = r."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "GL_ACCOUNTS" a ON a."ACCOUNT_ID" = l."ACCOUNT_ID"
 WHERE x."ACTION_CODE" IN ('TRS_RF_DRAWDOWN', 'TRS_RF_TRANSFER_CONFIRM')
 ORDER BY x."ACTION_CODE", l."JOURNAL_NO", l."LINE_NO";
