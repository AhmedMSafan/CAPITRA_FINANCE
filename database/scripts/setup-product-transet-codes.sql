-- =====================================================================
-- Capitra: GL Tran Set Code of each product (ADM_PRODUCTS.GL_TRAN_SET_CODE)
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again.
--
-- Part 1 adds the column (the API does the same on startup with the latest develop).
-- Part 2 sets each product's 3-letter code (agreed with Ahmed, 31 Oct 2026) and gives each code its counter in
-- GL_TRAN_SET_SEQUENCES, so its postings are numbered CAP-{code}-{year}-{number}, e.g. CAP-FRF-2026-00000001:
--   REVERSE_FACTORING          FRF  Factoring - Reverse                (new counter)
--   CLASSIC_FACTORING_R        FCR  Factoring - Classic with recourse   (new counter)
--   CLASSIC_FACTORING_NR       FCN  Factoring - Classic non-recourse    (new counter)
--   CHEQUE_WALLET              CHW  Cheques Wallet                      (counter exists: numbering goes on)
--   MTG_PORTFOLIO_PURCHASING   MPA  Mortgage Portfolio Acquisition      (counter exists)
-- The codes do not repeat the module counters FCT, COL and GLM. The Product Catalog shows the code read-only.
-- =====================================================================

-- Part 1: the column
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261031221817_ProductGlTranSetCode') THEN
    ALTER TABLE "ADM_PRODUCTS" ADD "GL_TRAN_SET_CODE" character varying(3);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261031221817_ProductGlTranSetCode') THEN
    CREATE UNIQUE INDEX "UQ_ADM_PRODUCTS_GL_TRAN_SET_CODE" ON "ADM_PRODUCTS" ("GL_TRAN_SET_CODE");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261031221817_ProductGlTranSetCode') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261031221817_ProductGlTranSetCode', '10.0.13');
    END IF;
END $EF$;
COMMIT;


-- Part 2: the codes and their counters
START TRANSACTION;
DO $$
DECLARE
  p record;
BEGIN
  FOR p IN SELECT * FROM (VALUES
      ('REVERSE_FACTORING',        'FRF', 'FCT', 'Factoring - Reverse',              'التخصيم العكسي'),
      ('CLASSIC_FACTORING_R',      'FCR', 'FCT', 'Factoring - Classic with recourse', 'التخصيم التقليدي بحق الرجوع'),
      ('CLASSIC_FACTORING_NR',     'FCN', 'FCT', 'Factoring - Classic non-recourse',  'التخصيم التقليدي بدون حق الرجوع'),
      ('CHEQUE_WALLET',            'CHW', 'FCT', 'Cheques Wallet',                    'محفظة الشيكات'),
      ('MTG_PORTFOLIO_PURCHASING', 'MPA', 'MTG', 'Mortgage Portfolio Acquisition',    'شراء محافظ التمويل العقاري'))
      AS t(product, code, app, en, ar)
  LOOP
    IF EXISTS (SELECT 1 FROM "ADM_PRODUCTS" WHERE "GL_TRAN_SET_CODE" = p.code AND "PRODUCT_CODE" <> p.product) THEN
      RAISE EXCEPTION 'Code % is already used by another product. Nothing changed.', p.code;
    END IF;
    UPDATE "ADM_PRODUCTS" SET "GL_TRAN_SET_CODE" = p.code, "UPDATED_BY" = 'SETUP', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
     WHERE "PRODUCT_CODE" = p.product AND "GL_TRAN_SET_CODE" IS DISTINCT FROM p.code;
    IF NOT FOUND THEN
      RAISE NOTICE '%: % (no change, or product not found)', p.product, p.code;
    ELSE
      RAISE NOTICE '%: % set', p.product, p.code;
    END IF;

    INSERT INTO "GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE",
                                         "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    SELECT p.code, extract(year FROM now())::int, p.app, p.en, p.ar, 0, 8, true, 'SETUP', now(), 1
     WHERE NOT EXISTS (SELECT 1 FROM "GL_TRAN_SET_SEQUENCES" WHERE "MODULE_CODE" = p.code);
  END LOOP;
END $$;
COMMIT;

-- what is set up
SELECT p."PRODUCT_CODE", p."PRODUCT_NAME_EN", p."GL_TRAN_SET_CODE", s."YEAR_NO", s."LAST_NO",
       'CAP-' || p."GL_TRAN_SET_CODE" || '-' || s."YEAR_NO" || '-' || lpad((s."LAST_NO" + 1)::text, 8, '0') AS "NEXT_ID"
  FROM "ADM_PRODUCTS" p
  LEFT JOIN "GL_TRAN_SET_SEQUENCES" s ON s."MODULE_CODE" = p."GL_TRAN_SET_CODE"
 ORDER BY p."PRODUCT_CODE", s."YEAR_NO" DESC;
