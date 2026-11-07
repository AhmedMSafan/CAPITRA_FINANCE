-- =====================================================================================================================
-- Clear the invoices and draft cheques already saved on disbursement requests that were never completed, so they can be
-- loaded again with Save Draft (Ahmed, 1 Nov 2026).
--
-- Only requests with no GL posting are touched. Cheques already in the wallet (WALLET_CHEQUE_ID set) are kept.
-- The request itself, its terms and its batch header stay; the batch totals and file name are reset.
--
-- To clear one request only, put its number in the WHERE of the first statement, e.g.
--     AND d."DISBURSEMENT_NUMBER" = 'DISB-2026-646'
-- Step 1 lists what will be cleared. Run it first and check it, then run the whole script.
-- =====================================================================================================================

BEGIN;

CREATE TEMP TABLE clear_requests ON COMMIT DROP AS
SELECT d."DISBURSEMENT_ID", d."DISBURSEMENT_NUMBER"
FROM "FCT_DISBURSEMENTS" d
WHERE COALESCE(d."IS_GL_POSTED", FALSE) = FALSE
  AND d."GL_TRAN_SET_ID" IS NULL
  -- AND d."DISBURSEMENT_NUMBER" = 'DISB-2026-646'
;

-- 1. what will be cleared
SELECT r."DISBURSEMENT_NUMBER",
       (SELECT COUNT(*) FROM "FCT_INVOICES" i WHERE i."DISBURSEMENT_ID" = r."DISBURSEMENT_ID") AS invoices,
       (SELECT COUNT(*) FROM "FCT_DISBURSEMENT_CHEQUES" c
         WHERE c."DISBURSEMENT_ID" = r."DISBURSEMENT_ID" AND c."WALLET_CHEQUE_ID" IS NULL) AS draft_cheques
FROM clear_requests r
ORDER BY r."DISBURSEMENT_ID";

-- 2. the invoices
DELETE FROM "FCT_INVOICES" i USING clear_requests r WHERE i."DISBURSEMENT_ID" = r."DISBURSEMENT_ID";

-- 3. the draft cheques (not the wallet ones)
DELETE FROM "FCT_DISBURSEMENT_CHEQUES" c USING clear_requests r
WHERE c."DISBURSEMENT_ID" = r."DISBURSEMENT_ID" AND c."WALLET_CHEQUE_ID" IS NULL;

-- 4. the batch totals and file name
UPDATE "FCT_INVOICE_BATCHES" b
SET "TOTAL_INVOICES" = 0, "TOTAL_GROSS_AMOUNT" = 0, "TOTAL_NET_AMOUNT" = 0,
    "UPLOAD_FILE_NAME" = NULL, "UPLOAD_FILE_SIZE" = NULL, "UPLOAD_FILE_TYPE" = NULL
FROM clear_requests r
WHERE b."DISBURSEMENT_ID" = r."DISBURSEMENT_ID";

COMMIT;
