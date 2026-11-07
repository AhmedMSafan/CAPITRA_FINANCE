-- =====================================================================================================================
-- Capitra: GL of reverse factoring on Treasury Disburse, agreed with Ahmed, 7 Nov 2026. Kept apart from the factoring rule TRS_DISBURSE.
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again: what exists is kept.
-- Needs: setup-treasury-disburse-gl.sql (product TREASURY_FUNDING) and migrate-reverse-supplier-transfers.sql.
--
-- FILL IN the two account codes at the top first (both must exist in GL_ACCOUNTS):
--   v_rf_borrowings_code  reverse factoring bank borrowings (credited with the bank drawdown at Execute)
--   v_rf_receivable_code  reverse factoring receivable from the buyer (debited when a supplier transfer is confirmed)
--
-- It sets up, under product TREASURY_FUNDING:
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
  -- >>> settings: the GL account codes (fill in) <<<
  v_rf_borrowings_code text := '';   -- reverse factoring bank borrowings
  v_rf_receivable_code text := '';   -- reverse factoring receivable from the buyer
  -- >>> end of settings <<<
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
  IF coalesce(trim(v_rf_borrowings_code), '') = '' OR coalesce(trim(v_rf_receivable_code), '') = '' THEN
    RAISE EXCEPTION 'Fill in v_rf_borrowings_code and v_rf_receivable_code at the top of the script first. Nothing changed.';
  END IF;
  SELECT "ACCOUNT_ID" INTO v_borr FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = trim(v_rf_borrowings_code) ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF v_borr IS NULL THEN
    RAISE EXCEPTION 'GL account % (reverse factoring bank borrowings) was not found. Nothing changed.', v_rf_borrowings_code;
  END IF;
  SELECT "ACCOUNT_ID" INTO v_recv FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = trim(v_rf_receivable_code) ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF v_recv IS NULL THEN
    RAISE EXCEPTION 'GL account % (reverse factoring receivable) was not found. Nothing changed.', v_rf_receivable_code;
  END IF;
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
