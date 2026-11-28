-- =====================================================================================================================
-- Fix one deal's invoice batch (Ahmed, 7 Nov 2026). One-off data fix: no new function.
--   1. Eligible invoices may not add up to more than the disbursement amount: the latest-due invoices above it become
--      NOT_ELIGIBLE ("Above the disbursement amount"), and the disbursement's gross amount becomes the eligible total.
--      Whole invoices only, so the eligible total can end a little below the target.
--   2. The invoices' tax ID (FCT_INVOICES.DEBTOR_ID, the supplier in reverse factoring) is set to the tax numbers of two
--      existing customers, alternating, and each of them gets an ACTIVE SUPPLIER role.
--
-- HOW TO RUN (pgAdmin, Query Tool on CAPITRA_DB):
--   a) Fill in the four values below. Leave v_apply = false and run: the Messages tab shows what WOULD change.
--   b) If it looks right, set v_apply = true and run again.
-- =====================================================================================================================
DO $$
DECLARE
    v_disbursement_number text    := 'DISB-2026-XXX';  -- the deal
    v_target              numeric := NULL;             -- the disbursement amount; NULL = the disbursement's NET_AMOUNT
    v_supplier_1          text    := NULL;             -- customer name (or part of it); NULL = picked for you
    v_supplier_2          text    := NULL;             -- second customer; NULL = picked for you
    v_apply               boolean := false;            -- false = preview only, true = change the data

    v_disb_id   bigint;
    v_client_id bigint;
    v_net       numeric;
    v_total     numeric;
    v_eligible  numeric := 0;
    v_s1        record;
    v_s2        record;
    r           record;
    n           int := 0;
BEGIN
    SELECT "DISBURSEMENT_ID", "CLIENT_ID", "NET_AMOUNT" INTO v_disb_id, v_client_id, v_net
      FROM "FCT_DISBURSEMENTS" WHERE "DISBURSEMENT_NUMBER" = v_disbursement_number;
    IF v_disb_id IS NULL THEN RAISE EXCEPTION 'Disbursement % was not found.', v_disbursement_number; END IF;
    v_target := coalesce(v_target, v_net);
    IF v_target IS NULL OR v_target <= 0 THEN RAISE EXCEPTION 'Set v_target: the disbursement has no net amount yet.'; END IF;

    SELECT sum(i."GROSS_AMOUNT") INTO v_total
      FROM "FCT_INVOICES" i JOIN "FCT_INVOICE_BATCHES" b ON b."BATCH_ID" = i."BATCH_ID"
     WHERE b."DISBURSEMENT_ID" = v_disb_id AND upper(coalesce(i."STATUS_CODE", '')) IN ('ELIGIBLE', 'APPROVED', '');
    RAISE NOTICE 'Deal %: target (disbursement amount) %, eligible invoices now %', v_disbursement_number, v_target, coalesce(v_total, 0);

    -- 1. eligibility: keep the earliest-due invoices up to the target
    FOR r IN
        SELECT i."INVOICE_ID", i."INVOICE_NUMBER", i."GROSS_AMOUNT", i."DUE_AT"
          FROM "FCT_INVOICES" i JOIN "FCT_INVOICE_BATCHES" b ON b."BATCH_ID" = i."BATCH_ID"
         WHERE b."DISBURSEMENT_ID" = v_disb_id AND upper(coalesce(i."STATUS_CODE", '')) IN ('ELIGIBLE', 'APPROVED', '')
         ORDER BY i."DUE_AT", i."INVOICE_ID"
    LOOP
        IF v_eligible + r."GROSS_AMOUNT" <= v_target THEN
            v_eligible := v_eligible + r."GROSS_AMOUNT";
        ELSE
            n := n + 1;
            RAISE NOTICE '  NOT ELIGIBLE: % (% due %)', r."INVOICE_NUMBER", r."GROSS_AMOUNT", r."DUE_AT"::date;
            IF v_apply THEN
                UPDATE "FCT_INVOICES" SET "STATUS_CODE" = 'NOT_ELIGIBLE', "INELIGIBLE_AMOUNT" = "GROSS_AMOUNT",
                       "INELIGIBLE_REASON" = 'Above the disbursement amount', "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now()
                 WHERE "INVOICE_ID" = r."INVOICE_ID";
            END IF;
        END IF;
    END LOOP;
    RAISE NOTICE '% invoice(s) become not eligible; eligible total becomes %', n, v_eligible;
    IF v_apply THEN
        UPDATE "FCT_DISBURSEMENTS" SET "GROSS_AMOUNT" = v_eligible, "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now()
         WHERE "DISBURSEMENT_ID" = v_disb_id;
    END IF;

    -- 2. suppliers: two existing customers with a numeric tax number, not the client itself
    SELECT "CLIENT_ID", "CLIENT_NAME", "TAX_NUMBER" INTO v_s1 FROM "CRM_CLIENTS"
     WHERE "CLIENT_ID" <> v_client_id AND "TAX_NUMBER" ~ '^[0-9]+$'
       AND (v_supplier_1 IS NULL OR "CLIENT_NAME" ILIKE '%' || v_supplier_1 || '%')
     ORDER BY "CLIENT_ID" LIMIT 1;
    SELECT "CLIENT_ID", "CLIENT_NAME", "TAX_NUMBER" INTO v_s2 FROM "CRM_CLIENTS"
     WHERE "CLIENT_ID" NOT IN (v_client_id, coalesce(v_s1."CLIENT_ID", -1)) AND "TAX_NUMBER" ~ '^[0-9]+$'
       AND (v_supplier_2 IS NULL OR "CLIENT_NAME" ILIKE '%' || v_supplier_2 || '%')
     ORDER BY "CLIENT_ID" LIMIT 1;
    IF v_s1."CLIENT_ID" IS NULL OR v_s2."CLIENT_ID" IS NULL THEN
        RAISE EXCEPTION 'Two customers with a numeric tax number were not found. Check the names, or add a tax number in Customer 360.';
    END IF;
    RAISE NOTICE 'Supplier 1: % (tax %) · Supplier 2: % (tax %)', v_s1."CLIENT_NAME", v_s1."TAX_NUMBER", v_s2."CLIENT_NAME", v_s2."TAX_NUMBER";

    IF v_apply THEN
        WITH numbered AS (
            SELECT i."INVOICE_ID", row_number() OVER (ORDER BY i."DUE_AT", i."INVOICE_ID") AS rn
              FROM "FCT_INVOICES" i JOIN "FCT_INVOICE_BATCHES" b ON b."BATCH_ID" = i."BATCH_ID"
             WHERE b."DISBURSEMENT_ID" = v_disb_id)
        UPDATE "FCT_INVOICES" i
           SET "DEBTOR_ID" = (CASE WHEN n2.rn % 2 = 1 THEN v_s1."TAX_NUMBER" ELSE v_s2."TAX_NUMBER" END)::bigint,
               "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now()
          FROM numbered n2 WHERE n2."INVOICE_ID" = i."INVOICE_ID";

        INSERT INTO "CRM_CLIENT_ROLES" ("CLIENT_ID", "ROLE_CODE", "STATUS_CODE", "ACTIVE_SINCE_DATE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
        SELECT c, 'SUPPLIER', 'ACTIVE', current_date, 'Supplier of ' || v_disbursement_number, 'SCRIPT', now(), 0
          FROM unnest(ARRAY[v_s1."CLIENT_ID", v_s2."CLIENT_ID"]) AS c
        ON CONFLICT ("CLIENT_ID", "ROLE_CODE") DO UPDATE SET "STATUS_CODE" = 'ACTIVE', "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now();
        RAISE NOTICE 'Done. Open Treasury Disburse for % and press Refresh.', v_disbursement_number;
    ELSE
        RAISE NOTICE 'Preview only. Nothing was changed. Set v_apply := true and run again to apply.';
    END IF;
END $$;
