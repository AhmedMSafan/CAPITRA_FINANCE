-- =====================================================================
-- Capitra: product event rule for the factoring disbursement charges (Process Disbursement Request)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5.
-- First run migrate-disbursement-process.sql, unless the API applies migrations on startup
-- and was started with the latest develop.
--
-- It sets up, with FINOVA's own figures (FNV_PRODUCT_EVENT_RULES of event DISBURSEMENT_APPROVED):
--   1. charge type FEE and the charges OPENING_ACCOUNT_FIXED, FACILITY_LIMIT_PERCENT, SUPPLIER_FIXED,
--      DISB_GROSS_COMMISSION, HANDLING_PER_DISB
--   2. business line FACTORING and FINOVA's factoring products (kept as they are when they exist):
--        REVERSE_FACTORING, CLASSIC_FACTORING_R, CLASSIC_FACTORING_NR
--   3. journal category FCT_DISBURSEMENT under the journal source FACTORING (the source must exist)
--   4. on each product, rule DISBURSEMENT_APPROVED with FINOVA's charges of that product:
--        REVERSE_FACTORING:      opening 30,000 fixed; facility 0.3% of the approved limit; supplier 20,000 fixed;
--                                commission 0.75% of the gross invoice amount; handling 600 fixed
--        CLASSIC_FACTORING_R/NR: commission 0.75% of the gross invoice amount; handling 200 fixed
--      all PAID_UPFRONT, due on the disbursement date, no tax. No GL entries yet: they come with GL.
--   5. finova:ruleCode="DISBURSEMENT_APPROVED" on the Invoice Upload step of DISBURSEMENT_REQUEST_WF
-- Safe to run again: what exists is kept; a rule that exists is not changed.
-- One transaction: any error changes nothing.
-- =====================================================================
DO $$
DECLARE
  v_by      text := 'SETUP';
  v_source  bigint;
  v_cat     bigint;
  v_product bigint;
  v_rule    bigint;
  p         record;
  c         record;
BEGIN
  IF to_regclass('"FCT_DISBURSEMENT_CHARGES"') IS NULL THEN
    RAISE EXCEPTION 'FCT_DISBURSEMENT_CHARGES is missing. Run migrate-disbursement-process.sql first. Nothing changed.';
  END IF;

  -- 1. charge type and charges
  INSERT INTO "ADM_LKP_CHARGE_TYPES" ("CODE", "NAME_EN", "NAME_AR", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES ('FEE', 'Fee', 'رسوم', 'Fees and expenses charged to the customer', 1, true, v_by, now(), 1)
  ON CONFLICT ("CODE") DO NOTHING;

  FOR c IN SELECT * FROM (VALUES
      ('OPENING_ACCOUNT_FIXED', 'Opening account fee - fixed', 'FIXED'),
      ('FACILITY_LIMIT_PERCENT', 'Facility fee - percent of approved limit', 'PERCENTAGE'),
      ('SUPPLIER_FIXED', 'Supplier fee - fixed', 'FIXED'),
      ('DISB_GROSS_COMMISSION', 'Commission on disbursement invoice gross amount', 'PERCENTAGE'),
      ('HANDLING_PER_DISB', 'Handling fee per paid disbursement', 'FIXED')) AS t(code, en, calc)
  LOOP
    INSERT INTO "ADM_CHARGES" ("CHARGE_CODE", "CHARGE_NAME_EN", "CHARGE_TYPE_CODE", "FINANCIAL_NATURE_CODE", "IS_RECURRING",
                               "CALCULATION_TYPE_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT c.code, c.en, 'FEE', 'REVENUE', false, c.calc, DATE '2024-01-01', true, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = c.code);
  END LOOP;

  -- 2. business line and products (FNV_PRODUCTS)
  INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES ('FACTORING', 'Factoring', 'التخصيم', 2, true, v_by, now(), 1)
  ON CONFLICT ("CODE") DO NOTHING;
  INSERT INTO "ADM_PRODUCTS" ("PRODUCT_CODE", "PRODUCT_NAME_EN", "BUSINESS_LINE_CODE", "PRODUCT_TYPE_CODE", "FACTORING_MODE_CODE", "IS_RECOURSE",
                              "DEFAULT_CURRENCY_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "MPP_PV_RATE", "MPP_LATE_FEE_RATE",
                              "MPP_RESERVED_AMOUNT_PCT", "MPP_INVESTOR_PAYMENT_PCT", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT t.code, t.en, 'FACTORING', 'FACTORING', t.mode, t.recourse, 'EGP', DATE '2026-07-28', true, 0, 0, 0, 0, v_by, now(), 1
    FROM (VALUES ('REVERSE_FACTORING', 'Reverse Factoring', 'REVERSE', NULL::boolean),
                 ('CLASSIC_FACTORING_R', 'Classic Factoring - Recourse', 'CLASSIC', true),
                 ('CLASSIC_FACTORING_NR', 'Classic Factoring - Non-Recourse', 'CLASSIC', false)) AS t(code, en, mode, recourse)
   WHERE NOT EXISTS (SELECT 1 FROM "ADM_PRODUCTS" x WHERE x."PRODUCT_CODE" = t.code);

  -- 3. journal source FACTORING and category FCT_DISBURSEMENT
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source FACTORING is missing. Nothing changed.';
  END IF;
  INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                       "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                       "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  SELECT 'FCT_DISBURSEMENT', 'Factoring Disbursement', 'صرف التخصيم', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
         'Charges and entries of a factoring disbursement request.', v_by, now(), 1
   WHERE NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'FCT_DISBURSEMENT');
  SELECT "CATEGORY_ID" INTO v_cat FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'FCT_DISBURSEMENT';

  -- 4. one rule per product with FINOVA's charges of that product
  FOR p IN SELECT DISTINCT product FROM (VALUES ('REVERSE_FACTORING'), ('CLASSIC_FACTORING_R'), ('CLASSIC_FACTORING_NR')) AS t(product)
  LOOP
    SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = p.product;
    SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_ID" = v_product AND "RULE_CODE" = 'DISBURSEMENT_APPROVED';
    IF v_rule IS NOT NULL THEN
      RAISE NOTICE 'rule DISBURSEMENT_APPROVED of % exists (id %): left as it is', p.product, v_rule;
      CONTINUE;
    END IF;
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "JOURNAL_SOURCE_ID", "JOURNAL_CATEGORY_ID", "RULE_CODE", "RULE_NAME_EN", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_source, v_cat, 'DISBURSEMENT_APPROVED', 'Disbursement Approved', 10, DATE '2024-01-01', true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;

    INSERT INTO "ADM_PRODUCT_EVENT_RULE_CHARGES" ("PRODUCT_EVENT_RULE_ID", "CHARGE_ID", "SEQUENCE_NO", "CALCULATION_METHOD_CODE", "FIXED_AMOUNT",
                                                  "CHARGE_RATE", "AMOUNT_BASE_CODE", "SPREAD_RATE", "DAY_COUNT_BASIS", "ROUNDING_SCALE",
                                                  "CHARGE_TIMING_CODE", "DUE_DAYS", "TAX_RATE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE",
                                                  "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT v_rule, ch."CHARGE_ID", t.seq, t.method, t.fixed, t.rate, t.base, 0, t.days, 2, 'PAID_UPFRONT', 0, 0, DATE '2024-01-01', true, v_by, now(), 1
      FROM (VALUES
        ('REVERSE_FACTORING',    10, 'OPENING_ACCOUNT_FIXED',  'FIXED',      30000::numeric, NULL::numeric, NULL, 360),
        ('REVERSE_FACTORING',    20, 'FACILITY_LIMIT_PERCENT', 'PERCENTAGE', NULL,           0.3,           'APPROVED_LIMIT', 360),
        ('REVERSE_FACTORING',    30, 'SUPPLIER_FIXED',         'FIXED',      20000,          NULL,          NULL, 360),
        ('REVERSE_FACTORING',    40, 'DISB_GROSS_COMMISSION',  'PERCENTAGE', NULL,           0.75,          'DISBURSEMENT_GROSS_INVOICE_AMOUNT', 365),
        ('REVERSE_FACTORING',    50, 'HANDLING_PER_DISB',      'FIXED',      600,            NULL,          NULL, 360),
        ('CLASSIC_FACTORING_R',  10, 'DISB_GROSS_COMMISSION',  'PERCENTAGE', NULL,           0.75,          'DISBURSEMENT_GROSS_INVOICE_AMOUNT', 365),
        ('CLASSIC_FACTORING_R',  20, 'HANDLING_PER_DISB',      'FIXED',      200,            NULL,          NULL, 365),
        ('CLASSIC_FACTORING_NR', 10, 'DISB_GROSS_COMMISSION',  'PERCENTAGE', NULL,           0.75,          'DISBURSEMENT_GROSS_INVOICE_AMOUNT', 365),
        ('CLASSIC_FACTORING_NR', 20, 'HANDLING_PER_DISB',      'FIXED',      200,            NULL,          NULL, 365))
        AS t(product, seq, code, method, fixed, rate, base, days)
      JOIN "ADM_CHARGES" ch ON ch."CHARGE_CODE" = t.code
     WHERE t.product = p.product;
    RAISE NOTICE 'rule DISBURSEMENT_APPROVED of % created (id %)', p.product, v_rule;
  END LOOP;

  -- 5. the Invoice Upload step points at the rule
  UPDATE "WFE_WORKFLOW_DEFINITIONS"
     SET "BPMN_XML" = replace("BPMN_XML", 'finova:stepCode="INVOICE_UPLOAD" ', 'finova:stepCode="INVOICE_UPLOAD" finova:ruleCode="DISBURSEMENT_APPROVED" ')
   WHERE "WORKFLOW_CODE" = 'DISBURSEMENT_REQUEST_WF' AND "BPMN_XML" NOT LIKE '%finova:ruleCode="DISBURSEMENT_APPROVED"%';
END $$;
