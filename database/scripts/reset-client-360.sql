-- =====================================================================================================================
-- Capitra: reset the Customer 360 of chosen clients (Ahmed, 28 Nov 2026)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Put the client numbers in the list below, then press F5.
--
-- Kept: the client itself (number, name, type, contact details, IDs, addresses).
-- Removed: its roles, related parties, bank accounts (and their history), compliance history, recent-client entries,
--   treasury bank rules about it, and consumer finance supplier applications (with documents, notes and changes).
-- Reset: compliance profile back to new (KYC, AML and sanctions PENDING; not PEP, not blacklisted; no risk grade,
--   review dates or verification), not a supplier, no role code, no first facility date.
-- Safe stop: if the client still has deals (finances, factoring offers, agreements, limits, disbursements, claims,
--   invoice batches or supplier transfers) nothing is changed: run reset-all-portfolios.sql first.
-- This cannot be undone.
-- =====================================================================================================================
BEGIN;

CREATE TEMP TABLE reset_clients ON COMMIT DROP AS
  SELECT "CLIENT_ID" FROM "CRM_CLIENTS"
   WHERE "CLIENT_NUMBER" IN ('CLT-2024-001', 'CLT-2024-002');   -- <== the clients to reset

DO $$
DECLARE n int; deals text;
BEGIN
  SELECT count(*) INTO n FROM reset_clients;
  IF n = 0 THEN RAISE EXCEPTION 'None of the client numbers was found. Nothing was changed.'; END IF;
  SELECT string_agg(t, ', ') INTO deals FROM (
    SELECT 'finances' t WHERE EXISTS (SELECT 1 FROM "LND_LOANS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'factoring offers' WHERE EXISTS (SELECT 1 FROM "FCT_OFFERS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'factoring agreements' WHERE EXISTS (SELECT 1 FROM "FCT_AGREEMENTS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'credit limits' WHERE EXISTS (SELECT 1 FROM "FCT_CREDIT_LIMITS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'disbursements' WHERE EXISTS (SELECT 1 FROM "FCT_DISBURSEMENTS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'customer claims' WHERE EXISTS (SELECT 1 FROM "FCT_CUSTOMER_CLAIMS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'invoice batches' WHERE EXISTS (SELECT 1 FROM "FCT_INVOICE_BATCHES" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
    UNION ALL SELECT 'supplier transfers' WHERE EXISTS (SELECT 1 FROM "TRS_SUPPLIER_TRANSFERS" WHERE "SUPPLIER_CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
  ) x;
  IF deals IS NOT NULL THEN
    RAISE EXCEPTION 'These clients still have %. Run reset-all-portfolios.sql first. Nothing was changed.', deals;
  END IF;
END $$;

-- consumer finance supplier applications (documents, notes, term changes, change requests first)
DELETE FROM "CRM_CF_SUPPLIER_STEP_NOTES"      WHERE "APPLICATION_ID" IN (SELECT "APPLICATION_ID" FROM "CRM_CF_SUPPLIER_APPLICATIONS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients));
DELETE FROM "CRM_CF_SUPPLIER_DOCUMENTS"       WHERE "APPLICATION_ID" IN (SELECT "APPLICATION_ID" FROM "CRM_CF_SUPPLIER_APPLICATIONS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients));
DELETE FROM "CRM_CF_SUPPLIER_TERM_CHANGES"    WHERE "APPLICATION_ID" IN (SELECT "APPLICATION_ID" FROM "CRM_CF_SUPPLIER_APPLICATIONS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients));
DELETE FROM "CRM_CF_SUPPLIER_CHANGE_REQUESTS" WHERE "APPLICATION_ID" IN (SELECT "APPLICATION_ID" FROM "CRM_CF_SUPPLIER_APPLICATIONS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients));
DELETE FROM "CRM_CF_SUPPLIER_APPLICATIONS"    WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);

-- roles, parties, bank accounts, compliance history, recent clients, treasury bank rules about the client
DELETE FROM "CRM_CLIENT_ROLES"                WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "CRM_CLIENT_PARTIES"              WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "CRM_CLIENT_BANK_ACCOUNT_HISTORY" WHERE "BANK_ACCOUNT_ID" IN (SELECT "BANK_ACCOUNT_ID" FROM "CRM_CLIENT_BANK_ACCOUNTS" WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients))
                                                 OR "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "CRM_CLIENT_BANK_ACCOUNTS"        WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "CRM_CLIENT_COMPLIANCE_HISTORY"   WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "CRM_RECENT_CLIENTS"              WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);
DELETE FROM "TRS_BANK_RULES"                  WHERE "SUBJECT_CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);

-- the compliance profile back to new; not a supplier; no role
UPDATE "CRM_CLIENTS" SET
       "KYC_STATUS" = 'PENDING', "KYC_EXPIRY_DATE" = NULL,
       "AML_STATUS" = 'PENDING', "AML_CHECK_DATE" = NULL,
       "SANCTIONS_STATUS" = 'PENDING', "IS_PEP" = false,
       "IS_BLACKLISTED" = false, "BLACKLIST_REASON" = NULL, "BLACKLIST_REASON_AR" = NULL,
       "RISK_GRADE" = NULL, "LAST_REVIEW_DATE" = NULL, "NEXT_REVIEW_DATE" = NULL,
       "COMPLIANCE_CHANGED_BY" = NULL, "COMPLIANCE_CHANGED_AT" = NULL,
       "COMPLIANCE_VERIFIED_BY" = NULL, "COMPLIANCE_VERIFIED_AT" = NULL,
       "IS_SUPPLIER" = false, "SUPPLIER_BANK_ID" = NULL, "SUPPLIER_BANK_ACCOUNT" = NULL,
       "CLIENT_ROLE_CODE" = NULL, "FIRST_FACILITY_DATE" = NULL,
       "UPDATED_BY" = 'RESET', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
 WHERE "CLIENT_ID" IN (SELECT "CLIENT_ID" FROM reset_clients);

COMMIT;
