-- =====================================================================================================================
-- Capitra: GL of the Treasury disbursement (Treasury Disburse page, Execute), agreed with Ahmed, 5 Nov 2026.
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again: what exists is kept.
-- Needs: migrate-treasury-payment.sql (or the API started with the latest develop).
--
-- FILL IN THE TWO ACCOUNT CODES BELOW FIRST (v_receivable_code and v_borrowings_code). Both must exist in GL_ACCOUNTS.
--
-- It sets up:
--   1. the GL transaction set counter TRS: postings are numbered CAP-TRS-<year>-<number>
--   2. journal category TRS_DISBURSE under the journal source FACTORING
--   3. product TREASURY_FUNDING "Treasury Funding" (business line FACTORING)
--   4. rule TRS_DISBURSE. The bank money comes into our account at that bank and the client is paid from it:
--        journal 1 (bank drawdown, bank lines only), amount BANK_DRAWDOWN:
--          DR our account at that bank (its GL account in Bank Limits) / CR bank borrowings (v_borrowings_code)
--        journal 2 (payment to the client, every line, own funds included), amount PAYMENT_AMOUNT:
--          DR factoring receivable (v_receivable_code) / CR the paying bank account (own funds: the main operating account)
--   5. the product action TRS_DISBURSE_EXECUTE "Treasury disbursement" linked to that rule
-- Example, net 12,000,000 from Bank A 2,000,000 and Bank B 10,000,000:
--   journal 1: DR Bank A 2,000,000, DR Bank B 10,000,000 / CR borrowings 12,000,000
--   journal 2: DR receivable 12,000,000 / CR Bank A 2,000,000, CR Bank B 10,000,000
-- The accounts can be changed later in Admin Setup > Product Catalog > TREASURY_FUNDING > rule TRS_DISBURSE.
-- Until the action is linked, Execute refuses with a message and saves nothing.
-- =====================================================================================================================
DO $$
DECLARE
  -- >>> settings: the GL account codes (Ahmed to give them) <<<
  v_receivable_code text := '';   -- Factoring receivable (debited with the amount paid to the client)
  v_borrowings_code text := '';   -- Bank borrowings (credited with the bank drawdown)
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
  IF to_regclass('"TRS_DISBURSEMENT_FUNDINGS"') IS NULL
     OR NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'TRS_DISBURSEMENT_FUNDINGS' AND column_name = 'GL_TRAN_SET_ID') THEN
    RAISE EXCEPTION 'TRS_DISBURSEMENT_FUNDINGS.GL_TRAN_SET_ID is missing. Run migrate-treasury-payment.sql first. Nothing changed.';
  END IF;
  IF coalesce(trim(v_receivable_code), '') = '' OR coalesce(trim(v_borrowings_code), '') = '' THEN
    RAISE EXCEPTION 'Fill in v_receivable_code and v_borrowings_code at the top of the script first. Nothing changed.';
  END IF;
  SELECT "ACCOUNT_ID" INTO v_recv FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = trim(v_receivable_code) ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF v_recv IS NULL THEN
    RAISE EXCEPTION 'GL account % (factoring receivable) was not found. Nothing changed.', v_receivable_code;
  END IF;
  SELECT "ACCOUNT_ID" INTO v_borr FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = trim(v_borrowings_code) ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF v_borr IS NULL THEN
    RAISE EXCEPTION 'GL account % (bank borrowings) was not found. Nothing changed.', v_borrowings_code;
  END IF;
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source FACTORING is missing. Nothing changed.';
  END IF;

  -- 1. counter
  INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'TRS', extract(year FROM now())::int, 'TRS', 'Treasury', 0, 8, true, v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = 'TRS');

  -- 2. journal category
  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'TRS_DISBURSE', 'Treasury Disbursement', 'صرف الخزينة', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
         'Bank drawdowns into our accounts and the payment of the disbursement to the client (Treasury Disburse, Execute).', v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'TRS_DISBURSE');
  SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'TRS_DISBURSE';

  -- 3. product
  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'TREASURY_FUNDING';
  IF v_product IS NULL THEN
    INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('FACTORING', 'Factoring', 'التخصيم', 2, true, v_by, now(), 1)
    ON CONFLICT ("CODE") DO NOTHING;
    INSERT INTO "ADM_PRODUCTS" ("PRODUCT_CODE", "PRODUCT_NAME_EN", "PRODUCT_NAME_AR", "BUSINESS_LINE_CODE", "PRODUCT_TYPE_CODE",
                                "DEFAULT_CURRENCY_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "MPP_PV_RATE", "MPP_LATE_FEE_RATE",
                                "MPP_RESERVED_AMOUNT_PCT", "MPP_INVESTOR_PAYMENT_PCT", "GL_TRAN_SET_CODE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('TREASURY_FUNDING', 'Treasury Funding', 'تمويل الخزينة', 'FACTORING', 'TREASURY_FUNDING', 'EGP', v_from, true, 0, 0, 0, 0,
            CASE WHEN EXISTS (SELECT 1 FROM "ADM_PRODUCTS" WHERE "GL_TRAN_SET_CODE" = 'TRS') THEN NULL ELSE 'TRS' END, v_by, now(), 1)
    RETURNING "PRODUCT_ID" INTO v_product;
    RAISE NOTICE 'product TREASURY_FUNDING created';
  END IF;

  -- 4. rule and its GL entries
  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES"
   WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = 'TRS_DISBURSE';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule TRS_DISBURSE exists (id %): its GL entries are left as they are (change them in Admin Setup)', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_source, v_cat, 'TRS_DISBURSE', 'Treasury disbursement', 'صرف الخزينة', 1, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;

    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_rule, NULL, 1, 10, 'D', 'BANK_DRAWDOWN',  'BANK_ACCOUNT', NULL,   'Bank drawdown received in our account', true, v_by, now(), 1),
           (v_rule, NULL, 1, 20, 'C', 'BANK_DRAWDOWN',  'ACCOUNT',      v_borr, 'Bank borrowings',                       true, v_by, now(), 1),
           (v_rule, NULL, 2, 10, 'D', 'PAYMENT_AMOUNT', 'ACCOUNT',      v_recv, 'Factoring receivable',                  true, v_by, now(), 1),
           (v_rule, NULL, 2, 20, 'C', 'PAYMENT_AMOUNT', 'BANK_ACCOUNT', NULL,   'Paid to the client',                    true, v_by, now(), 1);
    RAISE NOTICE 'rule TRS_DISBURSE created (id %)', v_rule;
  END IF;

  -- 5. the action
  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES (v_product, 'TRS_DISBURSE_EXECUTE', v_rule, true, v_by, now(), 1)
  ON CONFLICT ("ACTION_CODE") DO NOTHING;
  RAISE NOTICE 'action TRS_DISBURSE_EXECUTE: linked to rule % (an existing link was kept)', v_rule;
END $$;

-- what was set up, and the GL account of each Treasury bank account (a bank line needs one)
SELECT p."PRODUCT_CODE", x."ACTION_CODE", r."RULE_CODE", l."JOURNAL_NO", l."LINE_NO", l."DEBIT_CREDIT_CODE", l."ACCOUNT_SOURCE_CODE",
       a."ACCOUNT_CODE", l."AMOUNT_CODE"
  FROM "ADM_PRODUCT_ACTIONS" x
  JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = x."PRODUCT_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULES" r ON r."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULE_GL_LINES" l ON l."PRODUCT_EVENT_RULE_ID" = r."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "GL_ACCOUNTS" a ON a."ACCOUNT_ID" = l."ACCOUNT_ID"
 WHERE x."ACTION_CODE" = 'TRS_DISBURSE_EXECUTE'
 ORDER BY l."JOURNAL_NO", l."LINE_NO";

SELECT b."BANK_CODE", t."ACCOUNT_REF", t."ACCOUNT_TYPE_CODE", t."CURRENCY_CODE", t."ERP_ACCOUNT_CODE", t."IS_ACTIVE"
  FROM "TRS_BANK_ACCOUNTS" t JOIN "TRS_BANKS" b ON b."BANK_ID" = t."BANK_ID"
 ORDER BY b."BANK_CODE", t."ACCOUNT_TYPE_CODE", t."ACCOUNT_ID";
