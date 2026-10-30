-- =====================================================================
-- Capitra: remove the migrated product event rules (Ahmed, 30 Oct 2026)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5.
--
-- Keeps only:
--   PP_DISBURSE           the purchased portfolio disbursement rule
--   CHW_CHEQUES_RECEIVED  the cheque batch submission rule (setup-cheque-wallet-rule.sql)
-- Every other rule in ADM_PRODUCT_EVENT_RULES is deleted with its charge components
-- (ADM_PRODUCT_EVENT_RULE_CHARGES) and GL entry lines (ADM_PRODUCT_EVENT_RULE_GL_LINES).
-- Products, charges, charge types, business events and GL accounts are not touched.
--
-- Step 1: run it as it is. It only lists what would be deleted (NOTICE lines in the Messages tab
--         and the result grid). Nothing changes.
-- Step 2: set v_delete to true below and run it again. One transaction: any error changes nothing.
-- =====================================================================
DO $$
DECLARE
  v_delete boolean := false;   -- << set to true to delete
  v_keep   text[]  := ARRAY['PP_DISBURSE', 'CHW_CHEQUES_RECEIVED'];
  v_rules  bigint[];
  v_n      bigint;
  r        record;
BEGIN
  SELECT array_agg("PRODUCT_EVENT_RULE_ID") INTO v_rules FROM "ADM_PRODUCT_EVENT_RULES" WHERE "RULE_CODE" <> ALL (v_keep);
  IF v_rules IS NULL THEN
    RAISE NOTICE 'Nothing to delete: only the kept rules exist.';
    RETURN;
  END IF;

  FOR r IN SELECT x."PRODUCT_EVENT_RULE_ID", x."RULE_CODE", x."RULE_NAME_EN", p."PRODUCT_CODE",
                  (SELECT count(*) FROM "ADM_PRODUCT_EVENT_RULE_CHARGES" c WHERE c."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID") charges,
                  (SELECT count(*) FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" g WHERE g."PRODUCT_EVENT_RULE_ID" = x."PRODUCT_EVENT_RULE_ID") gl_lines
             FROM "ADM_PRODUCT_EVENT_RULES" x JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = x."PRODUCT_ID"
            WHERE x."PRODUCT_EVENT_RULE_ID" = ANY (v_rules) ORDER BY p."PRODUCT_CODE", x."RULE_CODE"
  LOOP
    RAISE NOTICE '% rule % "%" (id %): % charge component(s), % GL line(s)',
      CASE WHEN v_delete THEN 'deleting' ELSE 'would delete' END, r."RULE_CODE", r."RULE_NAME_EN", r."PRODUCT_EVENT_RULE_ID", r.charges, r.gl_lines;
  END LOOP;

  IF NOT v_delete THEN
    RAISE NOTICE 'Preview only: % rule(s) would be deleted. Set v_delete to true and run again to delete them.', cardinality(v_rules);
    RETURN;
  END IF;

  -- a product action linked to a deleted rule loses its link (it is then blocked until a rule is chosen again)
  IF to_regclass('"ADM_PRODUCT_ACTIONS"') IS NOT NULL THEN
    UPDATE "ADM_PRODUCT_ACTIONS" SET "PRODUCT_EVENT_RULE_ID" = NULL, "UPDATED_BY" = 'CLEANUP', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
     WHERE "PRODUCT_EVENT_RULE_ID" = ANY (v_rules);
  END IF;
  -- GL lines first (charge lines point at the rule charges), then charges, then the rules
  DELETE FROM "ADM_PRODUCT_EVENT_RULE_GL_LINES" WHERE "PRODUCT_EVENT_RULE_ID" = ANY (v_rules);
  GET DIAGNOSTICS v_n = ROW_COUNT;
  RAISE NOTICE '% GL line(s) deleted', v_n;
  DELETE FROM "ADM_PRODUCT_EVENT_RULE_CHARGES" WHERE "PRODUCT_EVENT_RULE_ID" = ANY (v_rules);
  GET DIAGNOSTICS v_n = ROW_COUNT;
  RAISE NOTICE '% charge component(s) deleted', v_n;
  DELETE FROM "ADM_PRODUCT_EVENT_RULES" WHERE "PRODUCT_EVENT_RULE_ID" = ANY (v_rules);
  GET DIAGNOSTICS v_n = ROW_COUNT;
  RAISE NOTICE '% rule(s) deleted', v_n;
END $$;

-- the rules there are now (run 1: before; run 2: after)
SELECT p."PRODUCT_CODE", r."RULE_CODE", r."RULE_NAME_EN", r."IS_ACTIVE"
  FROM "ADM_PRODUCT_EVENT_RULES" r JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = r."PRODUCT_ID"
 ORDER BY p."PRODUCT_CODE", r."RULE_CODE";
