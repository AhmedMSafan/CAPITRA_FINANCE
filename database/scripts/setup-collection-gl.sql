-- =====================================================================================================================
-- Capitra: GL of the cheque collection (Collection and Repayments), agreed with Ahmed, 1 Nov 2026.
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again: what exists is kept.
-- Needs: setup-cheque-wallet-rule.sql (product CHEQUE_WALLET) and setup-charges-payment.sql (account 230104).
--
-- Part 1: the column FCT_REPAYMENT_TRANSACTIONS.GL_TRAN_SET_ID (migration RepaymentGlTranSetId)
-- Part 2: journal category COL_COLLECTED, rule CHQ_CHEQUES_COLLECTED of product CHEQUE_WALLET and the action CHQ_COLLECTION
--         "Cheque collection" linked to it. The rule posts, amount CHEQUE_AMOUNT (the collected cheques):
--           journal 1: DR the deposit bank account (its ERP account code in Treasury) / CR 230104 Customer Unallocated Receipts
-- From then on Apply Bank Results on the Cheques Collection page posts this per client under CAP-COL-<year>-<number> and keeps the
-- ID on the repayment. No rule, a bank account without a GL code, or a GL error: nothing is applied.
-- =====================================================================================================================

-- Part 1 ----------------------------------------------------------------------------------------------------------------
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261101220846_RepaymentGlTranSetId') THEN
    ALTER TABLE "FCT_REPAYMENT_TRANSACTIONS" ADD "GL_TRAN_SET_ID" character varying(40);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261101220846_RepaymentGlTranSetId') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261101220846_RepaymentGlTranSetId', '10.0.13');
    END IF;
END $EF$;
COMMIT;


-- Part 2 ----------------------------------------------------------------------------------------------------------------
DO $$
DECLARE
  v_by      text := 'SETUP';
  v_from    date := DATE '2020-01-01';
  v_source  bigint;
  v_cat     bigint;
  v_product bigint;
  v_rule    bigint;
  v_liab    bigint;
BEGIN
  SELECT "ACCOUNT_ID" INTO v_liab FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = '230104' ORDER BY "ACCOUNT_ID" LIMIT 1;
  IF v_liab IS NULL THEN
    RAISE EXCEPTION 'Account 230104 Customer Unallocated Receipts is missing. Run setup-charges-payment.sql first. Nothing changed.';
  END IF;
  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'CHEQUE_WALLET';
  IF v_product IS NULL THEN
    RAISE EXCEPTION 'Product CHEQUE_WALLET is missing. Run setup-cheque-wallet-rule.sql first. Nothing changed.';
  END IF;
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source FACTORING is missing. Nothing changed.';
  END IF;

  INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'COL', 2026, 'COL', 'Collection and Repayments', 0, 8, true, v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = 'COL');

  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'COL_COLLECTED', 'Cheques Collected', 'تحصيل شيكات', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
         'Client cheques collected by the bank: the money is in the deposit bank account, owed to the client until allocated.', v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'COL_COLLECTED');
  SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'COL_COLLECTED';

  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES"
   WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = 'CHQ_CHEQUES_COLLECTED';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule CHQ_CHEQUES_COLLECTED exists (id %): its GL entries are left as they are', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_source, v_cat, 'CHQ_CHEQUES_COLLECTED', 'Cheques collected', 'تحصيل الشيكات', 2, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;

    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_rule, NULL, 1, 10, 'D', 'CHEQUE_AMOUNT', 'BANK_ACCOUNT', NULL, 'Cheques collected into the bank', true, v_by, now(), 1),
           (v_rule, NULL, 1, 20, 'C', 'CHEQUE_AMOUNT', 'ACCOUNT', v_liab, 'Customer liability: cheques collected', true, v_by, now(), 1);
    RAISE NOTICE 'rule CHQ_CHEQUES_COLLECTED created (id %)', v_rule;
  END IF;

  INSERT INTO "ADM_PRODUCT_ACTIONS" ("PRODUCT_ID", "ACTION_CODE", "PRODUCT_EVENT_RULE_ID", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES (v_product, 'CHQ_COLLECTION', v_rule, true, v_by, now(), 1)
  ON CONFLICT ("ACTION_CODE") DO NOTHING;
  RAISE NOTICE 'action CHQ_COLLECTION: linked to rule % (an existing link was kept)', v_rule;
END $$;

-- what was set up
SELECT p."PRODUCT_CODE", x."ACTION_CODE", r."RULE_CODE", l."JOURNAL_NO", l."LINE_NO", l."DEBIT_CREDIT_CODE", l."ACCOUNT_SOURCE_CODE",
       a."ACCOUNT_CODE", l."AMOUNT_CODE"
  FROM "ADM_PRODUCT_ACTIONS" x
  JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = x."PRODUCT_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULES" r ON r."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "ADM_PRODUCT_EVENT_RULE_GL_LINES" l ON l."PRODUCT_EVENT_RULE_ID" = r."PRODUCT_EVENT_RULE_ID"
  LEFT JOIN "GL_ACCOUNTS" a ON a."ACCOUNT_ID" = l."ACCOUNT_ID"
 WHERE x."ACTION_CODE" IN ('CHW_BATCH_SUBMIT', 'CHQ_COLLECTION')
 ORDER BY x."ACTION_CODE", l."JOURNAL_NO", l."LINE_NO";
