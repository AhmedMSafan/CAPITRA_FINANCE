-- =====================================================================================================================
-- Capitra: GL setup of Retail Lending (microfinance and consumer finance), for the loan payout (rule LND_DISBURSE).
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again: what exists is kept.
-- Needs: migrate-lending-disbursement.sql.
--
-- HOW TO RUN
--   a) Leave v_apply = false and run: the Messages tab shows what will be added. Nothing changes.
--   b) If it looks right, set v_apply = true and run again.
--
-- It adds, when missing:
--   1. the business line segment values MICROFINANCE and CONSUMER_FINANCE (copied from FACTORING), so lending postings carry them
--   2. the GL journal source LENDING (copied from FACTORING) and its journal category LND_DISBURSEMENT "Loan Disbursement"
--
-- Then, for each lending product (Administration > Product Catalog, business line Microfinance or Consumer Finance), add in
-- Administration > Product Event Rules a rule with code LND_DISBURSE, journal source LENDING, category LND_DISBURSEMENT:
--   journal 1, line 1: DEBIT  the loans receivable account                  amount PRINCIPAL_AMOUNT
--   journal 1, line 2: CREDIT account source "GL account of the bank account" amount NET_PAYOUT_AMOUNT
--   and for each charge taken at payout (timing "Deduct From Disbursement", e.g. an admin fee of 2% on LOAN_AMOUNT):
--   journal 1:         CREDIT fee income (CHARGE_IMMEDIATE) or unearned fee income (CHARGE_DEFERRED)
-- Example, a loan of 50,000 with a 2% admin fee earned at once:
--   DR Loans receivable 50,000.00 / CR Bank 49,000.00 / CR Fee income 1,000.00
-- =====================================================================================================================
DO $$
DECLARE
  v_apply  boolean := false;   -- false = preview only, true = change the data
  v_by     text    := 'SYSTEM';
  v_bl     record;
  v_line   text;
  v_source bigint;
BEGIN
  -- 1. business lines of the GL
  FOREACH v_line IN ARRAY ARRAY['MICROFINANCE', 'CONSUMER_FINANCE'] LOOP
    FOR v_bl IN SELECT s."COA_SEGMENT_ID", f.* FROM "GL_COA_SEGMENTS" s
                  JOIN "GL_COA_SEGMENT_VALUES" f ON f."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND f."VALUE_CODE" = 'FACTORING'
                 WHERE s."SEGMENT_CODE" = 'BUSINESS_LINE'
                   AND NOT EXISTS (SELECT 1 FROM "GL_COA_SEGMENT_VALUES" r WHERE r."COA_SEGMENT_ID" = s."COA_SEGMENT_ID" AND r."VALUE_CODE" = v_line)
    LOOP
      RAISE NOTICE 'Business line segment value % will be added (copy of FACTORING)', v_line;
      IF v_apply THEN
        INSERT INTO "GL_COA_SEGMENT_VALUES" ("COA_SEGMENT_ID", "VALUE_CODE", "VALUE_NAME_EN", "VALUE_NAME_AR", "DESCRIPTION", "PARENT_VALUE_ID", "IS_SUMMARY",
                                             "IS_POSTING_ALLOWED", "IS_ENABLED", "EFFECTIVE_START_DATE", "SORT_ORDER", "REFERENCE_BUSINESS_LINE_CODE",
                                             "CREATED_BY", "CREATED_AT", "ROW_VERSION")
        VALUES (v_bl."COA_SEGMENT_ID", v_line,
                CASE v_line WHEN 'MICROFINANCE' THEN 'Microfinance' ELSE 'Consumer Finance' END,
                CASE v_line WHEN 'MICROFINANCE' THEN 'التمويل متناهي الصغر' ELSE 'التمويل الاستهلاكي' END,
                'Retail Lending', v_bl."PARENT_VALUE_ID", false, true, true, v_bl."EFFECTIVE_START_DATE", v_bl."SORT_ORDER" + 2, v_line, v_by, now(), 1);
      END IF;
    END LOOP;
  END LOOP;

  -- 2. journal source LENDING and category LND_DISBURSEMENT
  IF NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'LENDING') THEN
    RAISE NOTICE 'Journal source LENDING will be added (copy of FACTORING)';
    IF v_apply THEN
      INSERT INTO "GL_JOURNAL_SOURCES" ("SOURCE_CODE", "SOURCE_NAME_EN", "SOURCE_NAME_AR", "SOURCE_GROUP_CODE", "DESCRIPTION", "IS_SYSTEM_SOURCE",
                                        "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED",
                                        "IS_DUPLICATE_CHECK", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      SELECT 'LENDING', 'Retail Lending', 'التمويل للأفراد', "SOURCE_GROUP_CODE", 'Microfinance and consumer finance postings', "IS_SYSTEM_SOURCE",
             "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_APPROVAL_REQUIRED", "IS_AUTO_POST_ALLOWED", "IS_REVERSAL_ALLOWED", "IS_DUPLICATE_CHECK", true,
             v_by, now(), 1
        FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'FACTORING';
    END IF;
  END IF;
  SELECT "SOURCE_ID" INTO v_source FROM "GL_JOURNAL_SOURCES" WHERE "SOURCE_CODE" = 'LENDING';

  IF NOT EXISTS (SELECT 1 FROM "GL_JOURNAL_CATEGORIES" WHERE "CATEGORY_CODE" = 'LND_DISBURSEMENT') THEN
    RAISE NOTICE 'Journal category LND_DISBURSEMENT will be added';
    IF v_apply THEN
      IF v_source IS NULL THEN
        RAISE EXCEPTION 'Journal source FACTORING is missing, so LENDING could not be copied from it. Nothing changed.';
      END IF;
      INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_CODE", "CATEGORY_NAME_EN", "CATEGORY_NAME_AR", "CATEGORY_TYPE", "DEFAULT_SOURCE_ID",
                                           "IS_REVERSAL_REQUIRED", "IS_MANUAL_ALLOWED", "IS_IMPORT_ALLOWED", "IS_ACTIVE", "DESCRIPTION",
                                           "CREATED_BY", "CREATED_AT", "ROW_VERSION")
      VALUES ('LND_DISBURSEMENT', 'Loan Disbursement', 'صرف التمويل', 'SOURCE_ACCOUNTING', v_source, false, false, true, true,
              'A retail loan paid out: loans receivable against the bank account, less the charges taken.', v_by, now(), 1);
    END IF;
  END IF;

  IF NOT v_apply THEN
    RAISE NOTICE 'Preview only. Nothing was changed. Set v_apply := true and run again.';
  ELSE
    RAISE NOTICE 'Done. Now add the rule LND_DISBURSE to each lending product (see the header).';
  END IF;
END $$;
