-- =====================================================================================================================
-- Capitra: consumer finance goods categories and the financed item (migration 20261115120000_LendingGoods).
-- Adds the table LND_LKP_GOODS_CATEGORIES with its 12 categories (kept in Administration > Reference Data > Goods Categories), and to
-- LND_LOANS the financed item: GOODS_CATEGORY_CODE, GOODS_DESCRIPTION, GOODS_BRAND_MODEL, MERCHANT_NAME, GOODS_PRICE_AMOUNT,
-- DOWN_PAYMENT_AMOUNT. Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Safe to run again.
-- =====================================================================================================================
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    CREATE TABLE "LND_LKP_GOODS_CATEGORIES" (
        "CODE" character varying(30) NOT NULL,
        "NAME_AR" character varying(100) NOT NULL,
        "NAME_EN" character varying(100) NOT NULL,
        "DESCRIPTION" character varying(500),
        "DISPLAY_ORDER" integer NOT NULL,
        "IS_ACTIVE" boolean NOT NULL,
        "CREATED_BY" character varying(100) NOT NULL,
        "CREATED_AT" timestamp with time zone NOT NULL,
        "UPDATED_BY" character varying(100),
        "UPDATED_AT" timestamp with time zone,
        "ROW_VERSION" integer NOT NULL,
        CONSTRAINT "PK_LND_LKP_GOODS_CATEGORIES" PRIMARY KEY ("CODE")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "GOODS_CATEGORY_CODE" character varying(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "GOODS_DESCRIPTION" character varying(300);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "GOODS_BRAND_MODEL" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "MERCHANT_NAME" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "GOODS_PRICE_AMOUNT" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD "DOWN_PAYMENT_AMOUNT" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    CREATE INDEX "IX_LND_LOANS_GOODS_CATEGORY_CODE" ON "LND_LOANS" ("GOODS_CATEGORY_CODE");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    ALTER TABLE "LND_LOANS" ADD CONSTRAINT "FK_LND_LOANS_LND_LKP_GOODS_CATEGORIES" FOREIGN KEY ("GOODS_CATEGORY_CODE") REFERENCES "LND_LKP_GOODS_CATEGORIES" ("CODE") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    INSERT INTO "LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_EN", "NAME_AR", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('ELECTRONICS', 'Electronics & electrical devices', 'الأجهزة الإلكترونية والكهربائية', 'TV, laptop, tablet, camera, gaming', 10, true, 'SYSTEM', now(), 1),
           ('MOBILE', 'Mobile phones', 'الهواتف المحمولة', 'Phones and smart watches', 20, true, 'SYSTEM', now(), 1),
           ('HOME_APPLIANCES', 'Household appliances', 'الأجهزة المنزلية', 'Fridge, washer, air conditioner, cooker, water heater', 30, true, 'SYSTEM', now(), 1),
           ('FURNITURE', 'Furniture & home fittings', 'الأثاث والمفروشات', 'Bedrooms, living rooms, kitchens, carpets', 40, true, 'SYSTEM', now(), 1),
           ('HOME_FINISHING', 'Home finishing', 'التشطيبات', 'Ceramics, sanitary ware, paint, doors, aluminium', 50, true, 'SYSTEM', now(), 1),
           ('VEHICLES', 'Cars & vehicles', 'السيارات ووسائل النقل', 'New or used cars, motorcycles', 60, true, 'SYSTEM', now(), 1),
           ('FASHION', 'Clothing, shoes, bags & watches', 'الملابس والأحذية والحقائب والساعات', NULL, 70, true, 'SYSTEM', now(), 1),
           ('EDUCATION', 'Education services', 'الخدمات التعليمية', 'School, university and course fees', 80, true, 'SYSTEM', now(), 1),
           ('MEDICAL', 'Medical services', 'الخدمات الطبية', 'Surgery, dental, optical, treatment', 90, true, 'SYSTEM', now(), 1),
           ('TRAVEL', 'Travel & tourism', 'السفر والسياحة', 'Trips, Hajj and Umrah', 100, true, 'SYSTEM', now(), 1),
           ('CLUBS', 'Clubs & sports memberships', 'عضويات الأندية', NULL, 110, true, 'SYSTEM', now(), 1),
           ('OTHER', 'Other durable goods', 'سلع معمرة أخرى', 'Anything not listed, with a description', 120, true, 'SYSTEM', now(), 1)
    ON CONFLICT ("CODE") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261115120000_LendingGoods') THEN
    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261115120000_LendingGoods', '10.0.13');
    END IF;
END $EF$;
COMMIT;
