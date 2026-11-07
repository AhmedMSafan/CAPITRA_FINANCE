-- =====================================================================================================================
-- Link a deal's invoice tax IDs to existing customers (Ahmed, 7 Nov 2026). One-off data fix: no new function.
--   The invoices keep the tax IDs of the uploaded sheet (FCT_INVOICES.DEBTOR_ID: the supplier in reverse factoring, the
--   debtor in factoring). Each tax ID that is not a customer yet is given to an existing customer (its TAX_NUMBER is set
--   to it), and that customer gets an ACTIVE SUPPLIER role (or DEBTOR in factoring). Then add each one's bank account in
--   Customer 360, and Treasury Disburse shows the customer's name, bills, bank account and amount to receive.
--   Use this instead of part 2 of fix-deal-invoices.sql, which changed the invoices' tax IDs.
--
-- HOW TO RUN (pgAdmin, Query Tool on CAPITRA_DB):
--   a) Fill in the values below. Leave v_apply = false and run: the Messages tab shows what WOULD change, including each
--      customer's old tax number (write it down if you may want it back).
--   b) If it looks right, set v_apply = true and run again.
-- =====================================================================================================================
DO $$
DECLARE
    v_disbursement_number text    := 'DISB-2026-XXX';   -- the deal
    -- customer names (or part of a name), one per tax ID in ascending order (e.g. 111111111, 123456789, 987654321);
    -- NULL = picked for you (an existing customer, not the deal's client)
    v_customers           text[]  := ARRAY[NULL, NULL, NULL]::text[];
    v_role                text    := NULL;              -- NULL = SUPPLIER in reverse factoring, DEBTOR in factoring
    v_apply               boolean := false;             -- false = preview only, true = change the data

    v_disb_id   bigint;
    v_client_id bigint;
    v_reverse   boolean;
    v_used      bigint[] := ARRAY[]::bigint[];
    v_name      text;
    c           record;
    t           record;
    k           int := 0;
BEGIN
    SELECT d."DISBURSEMENT_ID", d."CLIENT_ID",
           upper(coalesce(p."FACTORING_MODE_CODE", '')) = 'REVERSE' OR upper(coalesce(a."FACTORING_TYPE", '')) LIKE '%REVERSE%'
      INTO v_disb_id, v_client_id, v_reverse
      FROM "FCT_DISBURSEMENTS" d
      LEFT JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = d."PRODUCT_ID"
      LEFT JOIN "FCT_AGREEMENTS" a ON a."AGREEMENT_ID" = d."AGREEMENT_ID"
     WHERE d."DISBURSEMENT_NUMBER" = v_disbursement_number;
    IF v_disb_id IS NULL THEN RAISE EXCEPTION 'Disbursement % was not found.', v_disbursement_number; END IF;
    v_role := coalesce(v_role, CASE WHEN v_reverse THEN 'SUPPLIER' ELSE 'DEBTOR' END);
    RAISE NOTICE 'Deal %: % factoring, role to give: %', v_disbursement_number, CASE WHEN v_reverse THEN 'reverse' ELSE 'classic' END, v_role;

    FOR t IN
        SELECT i."DEBTOR_ID"::text AS tax, count(*) AS n, sum(i."GROSS_AMOUNT") AS amount
          FROM "FCT_INVOICES" i
         WHERE i."DEBTOR_ID" IS NOT NULL
           AND (i."DISBURSEMENT_ID" = v_disb_id
                OR i."BATCH_ID" IN (SELECT b."BATCH_ID" FROM "FCT_INVOICE_BATCHES" b WHERE b."DISBURSEMENT_ID" = v_disb_id))
         GROUP BY i."DEBTOR_ID" ORDER BY i."DEBTOR_ID"
    LOOP
        k := k + 1;
        -- already a customer?
        SELECT "CLIENT_ID", "CLIENT_NAME" INTO c FROM "CRM_CLIENTS" WHERE "TAX_NUMBER" = t.tax ORDER BY "CLIENT_ID" LIMIT 1;
        IF c."CLIENT_ID" IS NOT NULL THEN
            RAISE NOTICE 'Tax %: already customer % (% invoices, %). Nothing to change.', t.tax, c."CLIENT_NAME", t.n, t.amount;
            v_used := v_used || c."CLIENT_ID";
        ELSE
            v_name := CASE WHEN k <= coalesce(array_length(v_customers, 1), 0) THEN v_customers[k] END;
            SELECT cl."CLIENT_ID", cl."CLIENT_NAME", cl."TAX_NUMBER" INTO c FROM "CRM_CLIENTS" cl
             WHERE cl."CLIENT_ID" <> v_client_id AND NOT (cl."CLIENT_ID" = ANY (v_used))
               AND (v_name IS NULL OR cl."CLIENT_NAME" ILIKE '%' || v_name || '%')
               -- when picked for you: never a customer whose tax number another invoice of this deal uses
               AND (v_name IS NOT NULL OR cl."TAX_NUMBER" IS NULL OR cl."TAX_NUMBER" NOT IN (
                    SELECT i."DEBTOR_ID"::text FROM "FCT_INVOICES" i JOIN "FCT_INVOICE_BATCHES" b ON b."BATCH_ID" = i."BATCH_ID"
                     WHERE b."DISBURSEMENT_ID" = v_disb_id AND i."DEBTOR_ID" IS NOT NULL))
             ORDER BY cl."CLIENT_ID" LIMIT 1;
            IF c."CLIENT_ID" IS NULL THEN
                RAISE EXCEPTION 'No customer found for tax % (name "%"). Check the name, or add the customer in Customer 360.', t.tax, v_name;
            END IF;
            RAISE NOTICE 'Tax %: customer % (old tax number %) gets tax % and role % (% invoices, %).',
                t.tax, c."CLIENT_NAME", coalesce(c."TAX_NUMBER", 'none'), t.tax, v_role, t.n, t.amount;
            v_used := v_used || c."CLIENT_ID";
            IF v_apply THEN
                UPDATE "CRM_CLIENTS" SET "TAX_NUMBER" = t.tax, "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now() WHERE "CLIENT_ID" = c."CLIENT_ID";
            END IF;
        END IF;
        IF v_apply THEN
            INSERT INTO "CRM_CLIENT_ROLES" ("CLIENT_ID", "ROLE_CODE", "STATUS_CODE", "ACTIVE_SINCE_DATE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
            VALUES (c."CLIENT_ID", v_role, 'ACTIVE', current_date, initcap(v_role) || ' of ' || v_disbursement_number, 'SCRIPT', now(), 0)
            ON CONFLICT ("CLIENT_ID", "ROLE_CODE") DO UPDATE SET "STATUS_CODE" = 'ACTIVE', "UPDATED_BY" = 'SCRIPT', "UPDATED_AT" = now();
        END IF;
    END LOOP;
    IF k = 0 THEN RAISE EXCEPTION 'The invoices of % have no tax IDs.', v_disbursement_number; END IF;

    IF v_apply THEN
        RAISE NOTICE 'Done. Add a bank account for each customer in Customer 360, then open Treasury Disburse for % and press Refresh.', v_disbursement_number;
    ELSE
        RAISE NOTICE 'Preview only. Nothing was changed. Set v_apply := true and run again to apply.';
    END IF;
END $$;
