-- =====================================================================
-- Capitra: product event rule for the purchased portfolio Disbursement
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5.
-- First run migrate-products-and-charges.sql (creates the tables), unless
-- the API applies migrations on startup and was started with the latest develop.
--
-- It sets up, with the same figures and accounts the Disbursement posts today:
--   1. charge type FEE and the charges ADMIN_EXPENSE, LEGAL_FEE, VALUATION_FEE
--   2. business line MORTGAGE and product MTG_PORTFOLIO_PURCHASING (kept as they are when they exist)
--   3. business event PP_DISBURSE (GL_ACCOUNTING_EVENT_TYPES, source PURCHASED_PORTFOLIO)
--   4. rule PP_DISBURSE: the three charges, the rule GL entries
--      (booking journal 1 and payment journal 2) and each charge's GL entries
--   5. finova:eventCode="PP_DISBURSE" on the Disbursement step of PURCHASED_PORTFOLIO_WF
-- From then on the Disbursement takes its GL entries from the rule.
-- Safe to run again: what exists is kept; a rule that exists is not changed.
-- One transaction: any error changes nothing.
-- =====================================================================
DO $$
DECLARE
  v_by      text := 'SETUP';
  v_from    date := DATE '2020-01-01';   -- rule and charges in effect from
  v_product bigint;
  v_source  bigint;
  v_event   bigint;
  v_rule    bigint;
  v_rc      bigint;
  v_missing text;
  v_n       bigint;
  c         record;
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ADM_PRODUCT_EVENT_RULE_GL_LINES' AND column_name = 'AMOUNT_CODE') THEN
    RAISE EXCEPTION 'The Products and Charges tables are missing or old. Run migrate-products-and-charges.sql first. Nothing changed.';
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ADM_PRODUCT_EVENT_RULE_CHARGES' AND column_name = 'INCOME_RECOGNITION_CODE') THEN
    RAISE EXCEPTION 'The rule charge columns are old. Run migrate-disbursement-rule-charges.sql first. Nothing changed.';
  END IF;

  -- the GL accounts the Disbursement posts to must exist as posting accounts
  SELECT string_agg(code, ', ') INTO v_missing
    FROM unnest(ARRAY['120401', '120402', '120403', '120404', '450102', '450103', '230103']) code
   WHERE NOT EXISTS (SELECT 1 FROM "GL_ACCOUNTS" a WHERE a."ACCOUNT_CODE" = code AND a."IS_ACTIVE");
  IF v_missing IS NOT NULL THEN
    RAISE EXCEPTION 'These GL accounts are missing or inactive in GL_ACCOUNTS: %. Add them to the chart of accounts first. Nothing changed.', v_missing;
  END IF;

  -- 1. charge type and charges (names as on the Financial Review page)
  INSERT INTO "ADM_LKP_CHARGE_TYPES" ("CODE", "NAME_EN", "NAME_AR", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
  VALUES ('FEE', 'Fee', 'رسوم', 'Fees and expenses charged to the customer', 1, true, v_by, now(), 1)
  ON CONFLICT ("CODE") DO NOTHING;

  FOR c IN SELECT * FROM (VALUES
      ('ADMIN_EXPENSE', 'Admin expense', 'المصروفات الإدارية', 'PERCENTAGE'),
      ('LEGAL_FEE', 'Legal fees', 'الرسوم القانونية', 'FIXED'),
      ('VALUATION_FEE', 'Valuation expense', 'مصروفات التقييم', 'FIXED')) AS t(code, en, ar, calc)
  LOOP
    INSERT INTO "ADM_CHARGES" ("CHARGE_CODE", "CHARGE_NAME_EN", "CHARGE_NAME_AR", "CHARGE_TYPE_CODE", "FINANCIAL_NATURE_CODE", "IS_RECURRING",
                               "CALCULATION_TYPE_CODE", "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT c.code, c.en, c.ar, 'FEE', 'REVENUE', false, c.calc, v_from, true, v_by, now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = c.code);
  END LOOP;

  -- 2. product
  SELECT "PRODUCT_ID" INTO v_product FROM "ADM_PRODUCTS" WHERE "PRODUCT_CODE" = 'MTG_PORTFOLIO_PURCHASING';
  IF v_product IS NULL THEN
    -- business line MORTGAGE (the business line segment the Disbursement posts to)
    INSERT INTO "ADM_LKP_BUSINESS_LINES" ("CODE", "NAME_EN", "NAME_AR", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('MORTGAGE', 'Mortgage', 'التمويل العقاري', 1, true, v_by, now(), 1)
    ON CONFLICT ("CODE") DO NOTHING;
    INSERT INTO "ADM_PRODUCTS" ("PRODUCT_CODE", "PRODUCT_NAME_EN", "PRODUCT_NAME_AR", "BUSINESS_LINE_CODE", "PRODUCT_TYPE_CODE",
                                "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "MPP_PV_RATE", "MPP_LATE_FEE_RATE", "MPP_RESERVED_AMOUNT_PCT",
                                "MPP_INVESTOR_PAYMENT_PCT", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('MTG_PORTFOLIO_PURCHASING', 'Mortgage Portfolio Purchasing', 'شراء محافظ التمويل العقاري', 'MORTGAGE', 'PORTFOLIO_PURCHASE',
            v_from, true, 0, 0, 0, 0, v_by, now(), 1)
    RETURNING "PRODUCT_ID" INTO v_product;
    RAISE NOTICE 'product MTG_PORTFOLIO_PURCHASING created: enter its rates on Admin Setup > Mortgage Portfolio Purchasing';
  END IF;

  -- 3. business event PP_DISBURSE of source PURCHASED_PORTFOLIO
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'PURCHASED_PORTFOLIO';
  IF v_source IS NULL THEN
    RAISE EXCEPTION 'GL journal source PURCHASED_PORTFOLIO is missing. Nothing changed.';
  END IF;
  SELECT "EVENT_TYPE_ID" INTO v_event FROM "GL_ACCOUNTING_EVENT_TYPES" WHERE "EVENT_TYPE_CODE" = 'PP_DISBURSE';
  IF v_event IS NULL THEN
    INSERT INTO "GL_ACCOUNTING_EVENT_TYPES" ("SOURCE_ID", "EVENT_TYPE_CODE", "EVENT_TYPE_NAME_EN", "EVENT_TYPE_NAME_AR", "DEFAULT_CATEGORY_ID",
                                             "IS_ACCOUNTING_REQUIRED", "IS_MANUAL_ADJUST_ALLOWED", "IS_ACTIVE", "DESCRIPTION", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_source, 'PP_DISBURSE', 'Purchased portfolio disbursement', 'صرف المحفظة المشتراة',
            (SELECT "CATEGORY_ID" FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'PP_DISBURSE'),
            true, false, true, 'Booking of a purchased portfolio and payment to the developer', v_by, now(), 1)
    RETURNING "EVENT_TYPE_ID" INTO v_event;
    RAISE NOTICE 'business event PP_DISBURSE created';
  END IF;

  -- 4. the rule, its charges and GL entries
  SELECT "PRODUCT_EVENT_RULE_ID" INTO v_rule FROM "ADM_PRODUCT_EVENT_RULES"
   WHERE "PRODUCT_ID" = v_product AND "EVENT_TYPE_ID" = v_event AND "RULE_CODE" = 'PP_DISBURSE';
  IF v_rule IS NOT NULL THEN
    RAISE NOTICE 'rule PP_DISBURSE exists (id %): its charges and GL entries are left as they are', v_rule;
  ELSE
    INSERT INTO "ADM_PRODUCT_EVENT_RULES" ("PRODUCT_ID", "EVENT_TYPE_ID", "RULE_CODE", "RULE_NAME_EN", "RULE_NAME_AR", "PRIORITY_NO",
                                           "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES (v_product, v_event, 'PP_DISBURSE', 'Purchased portfolio disbursement', 'صرف المحفظة المشتراة', 1, v_from, true, v_by, now(), 1)
    RETURNING "PRODUCT_EVENT_RULE_ID" INTO v_rule;

    -- rule GL entries: journal 1 booking, journal 2 payment to the developer
    INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                   "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                   "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT v_rule, NULL, t.j, t.no, t.dc, t.amount, CASE WHEN t.acct IS NULL THEN 'BANK_ACCOUNT' ELSE 'ACCOUNT' END,
           (SELECT "ACCOUNT_ID" FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = t.acct AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1),
           t.descr, true, v_by, now(), 1
      FROM (VALUES
        (1, 10, 'D', 'FACE_VALUE',       '120401', 'Investor cheques - face value'),
        (1, 20, 'C', 'DISCOUNT',         '120402', 'Unearned income - funding recovery'),
        (1, 30, 'C', 'MARGIN_DEFERRED',  '120403', 'Unearned income - margin (pro-rated)'),
        (1, 50, 'C', 'MARGIN_IMMEDIATE', '450102', 'Margin income - recognised on disbursement'),
        (1, 70, 'C', 'PURCHASE_PRICE',   '230103', 'Payable to developer - purchase price'),
        (2, 10, 'D', 'PURCHASE_PRICE',   '230103', 'Payable to developer - settled'),
        (2, 20, 'C', 'PURCHASE_PRICE',   NULL,     'Payment to developer')) AS t(j, no, dc, amount, acct, descr);

    -- charge components and each one's GL entries; the disbursement calculates each amount from the rates entered on the
    -- rule (Admin Setup > Product Event Rules > Charge Components), so enter them there after this script
    FOR c IN SELECT * FROM (VALUES (10, 'ADMIN_EXPENSE', 'PERCENTAGE', 'PRORATE'), (20, 'LEGAL_FEE', 'PER_UNIT', 'IMMEDIATE'),
                                   (30, 'VALUATION_FEE', 'PER_UNIT', 'IMMEDIATE')) AS t(seq, code, method, income)
    LOOP
      INSERT INTO "ADM_PRODUCT_EVENT_RULE_CHARGES" ("PRODUCT_EVENT_RULE_ID", "CHARGE_ID", "SEQUENCE_NO", "CALCULATION_METHOD_CODE", "INCOME_RECOGNITION_CODE",
                                                    "SPREAD_RATE", "DAY_COUNT_BASIS", "ROUNDING_SCALE", "CHARGE_TIMING_CODE", "DUE_DAYS", "TAX_RATE",
                                                    "EFFECTIVE_FROM_DATE", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      SELECT v_rule, "CHARGE_ID", c.seq, c.method, c.income, 0, 365, 2, 'DEDUCT_FROM_DISBURSEMENT', 0, 0, v_from, true, v_by, now(), 1
        FROM "ADM_CHARGES" WHERE "CHARGE_CODE" = c.code
      RETURNING "RULE_CHARGE_ID" INTO v_rc;

      INSERT INTO "ADM_PRODUCT_EVENT_RULE_GL_LINES" ("PRODUCT_EVENT_RULE_ID", "RULE_CHARGE_ID", "JOURNAL_NO", "LINE_NO", "DEBIT_CREDIT_CODE",
                                                     "AMOUNT_CODE", "ACCOUNT_SOURCE_CODE", "ACCOUNT_ID", "DESCRIPTION_TEMPLATE", "IS_ACTIVE",
                                                     "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      SELECT v_rule, v_rc, 1, t.no, 'C', t.amount, 'ACCOUNT',
             (SELECT "ACCOUNT_ID" FROM "GL_ACCOUNTS" WHERE "ACCOUNT_CODE" = t.acct AND "IS_ACTIVE" ORDER BY "ACCOUNT_ID" LIMIT 1),
             t.descr, true, v_by, now(), 1
        FROM (VALUES
          (40, 'CHARGE_DEFERRED',  '120404', 'Unearned income - fees (pro-rated)'),
          (60, 'CHARGE_IMMEDIATE', '450103', 'Fee income - recognised on disbursement')) AS t(no, amount, acct, descr);
    END LOOP;
    RAISE NOTICE 'rule PP_DISBURSE created (id %) with 3 charges and 13 GL entries', v_rule;
  END IF;

  -- 5. the Disbursement step raises PP_DISBURSE (every version of the workflow, so running portfolios use it too)
  UPDATE "WFE_WORKFLOW_DEFINITIONS"
     SET "BPMN_XML" = replace("BPMN_XML", 'finova:stepCode="DISBURSEMENT"', 'finova:stepCode="DISBURSEMENT" finova:eventCode="PP_DISBURSE"'),
         "UPDATED_BY" = v_by, "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "WORKFLOW_CODE" = 'PURCHASED_PORTFOLIO_WF' AND "BPMN_XML" LIKE '%finova:stepCode="DISBURSEMENT"%'
     AND "BPMN_XML" NOT LIKE '%finova:stepCode="DISBURSEMENT" finova:eventCode=%';
  GET DIAGNOSTICS v_n = ROW_COUNT;
  RAISE NOTICE '% workflow version(s) of PURCHASED_PORTFOLIO_WF now raise PP_DISBURSE on Disbursement', v_n;

  RAISE NOTICE 'Setup done. The Disbursement GL tab shows "Entries from product event rule PP_DISBURSE".';
END $$;
