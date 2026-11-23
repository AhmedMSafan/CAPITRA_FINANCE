-- One-off sample data for the GL manual screenshots, on an empty test database only (never on a real one):
-- a legal entity, currencies, the account structure with its segments and values, a chart of accounts, the 2026 calendar
-- with its periods, the primary ledger with its periods, and the MANUAL journal source and categories.
-- The journals themselves are made through the application (GL journal entry), so they post as usual.
BEGIN;

INSERT INTO "FIN_CURRENCIES" ("CURRENCY_CODE","CURRENCY_NAME_EN","CURRENCY_NAME_AR","SYMBOL","PRECISION_DIGITS","IS_ACTIVE","CURRENCY_ID","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES ('EGP','Egyptian Pound','جنيه مصري','E£',2,true,1,'SAMPLE',now(),0),('USD','US Dollar','دولار أمريكي','$',2,true,2,'SAMPLE',now(),0)
ON CONFLICT DO NOTHING;

INSERT INTO "FIN_LEGAL_ENTITIES" ("LEGAL_ENTITY_ID","LEGAL_ENTITY_CODE","LEGAL_ENTITY_NAME_EN","LEGAL_ENTITY_NAME_AR","SHORT_NAME","COUNTRY_CODE","BASE_CURRENCY_CODE",
  "START_DATE","STATUS_CODE","IS_PRIMARY","LEGAL_ENTITY_TYPE","IS_CONSOLIDATION","OWNERSHIP_PERCENT","CONSOLIDATION_METHOD","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,'01','Finova Finance S.A.E.','فينوفا للتمويل ش.م.م','Finova','EG','EGP','2020-01-01','ACTIVE',true,'COMPANY',false,100,'FULL','SAMPLE',now(),0);

INSERT INTO "GL_COA_STRUCTURES" ("COA_ID","COA_CODE","COA_NAME_EN","COA_NAME_AR","DESCRIPTION","DELIMITER_CHAR","SEGMENT_COUNT","BALANCING_SEGMENT_NUM","ACCOUNT_SEGMENT_NUM",
  "BRANCH_SEGMENT_NUM","COST_CENTER_SEGMENT_NUM","BUSINESS_LINE_SEGMENT_NUM","INTERCOMPANY_SEGMENT_NUM","IS_DYNAMIC_COMBINATIONS_ALLOWED","IS_COMBINATION_APPROVAL_REQUIRED",
  "IS_ACTIVE","EFFECTIVE_START_DATE","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,'FINOVA_COA','Finova chart of accounts','دليل حسابات فينوفا','Company, branch, cost centre, account, business line and two future segments','-',7,1,4,2,3,5,NULL,true,false,true,'2020-01-01','SAMPLE',now(),0);

INSERT INTO "GL_COA_SEGMENTS" ("COA_SEGMENT_ID","COA_ID","SEGMENT_NUM","SEGMENT_CODE","SEGMENT_NAME_EN","SEGMENT_NAME_AR","SEGMENT_TYPE","MIN_LENGTH","MAX_LENGTH","VALUE_FORMAT",
  "PAD_CHAR","IS_UPPERCASE","IS_REQUIRED","IS_SUMMARY_VALUES_ALLOWED","IS_ENABLED","DISPLAY_SEQ","DESCRIPTION","DEFAULT_VALUE_CODE","DEFAULT_MODE","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES
 (1,1,1,'COMPANY','Company','الشركة','COMPANY',2,2,'NUMERIC','0',true,true,false,true,1,'Which legal entity owns it','01','SUGGEST','SAMPLE',now(),0),
 (2,1,2,'BRANCH','Branch','الفرع','BRANCH',2,5,'ALPHANUMERIC',NULL,true,true,false,true,2,'Where it happened','HQ','SUGGEST','SAMPLE',now(),0),
 (3,1,3,'COST_CENTER','Cost centre','مركز التكلفة','COST_CENTER',4,4,'NUMERIC','0',true,true,false,true,3,'Which department is responsible','1100','SUGGEST','SAMPLE',now(),0),
 (4,1,4,'NATURAL','Natural account','الحساب الطبيعي','ACCOUNT',4,6,'NUMERIC',NULL,true,true,true,true,4,'What it is: asset, liability, equity, income or expense',NULL,'NONE','SAMPLE',now(),0),
 (5,1,5,'BUSINESS_LINE','Business line','خط الأعمال','BUSINESS_LINE',3,20,'ALPHANUMERIC',NULL,true,true,false,true,5,'Which product or business','GENERAL','SUGGEST','SAMPLE',now(),0),
 (6,1,6,'FUTURE1','Future 1','مستقبلي ١','FUTURE',3,3,'NUMERIC','0',true,true,false,true,6,'Reserved, 000 = not used','000','SUGGEST','SAMPLE',now(),0),
 (7,1,7,'FUTURE2','Future 2','مستقبلي ٢','FUTURE',3,3,'NUMERIC','0',true,true,false,true,7,'Reserved, 000 = not used','000','SUGGEST','SAMPLE',now(),0);

-- segment values (the natural account values follow below with the accounts)
INSERT INTO "GL_COA_SEGMENT_VALUES" ("SEGMENT_VALUE_ID","COA_SEGMENT_ID","VALUE_CODE","VALUE_NAME_EN","VALUE_NAME_AR","IS_SUMMARY","IS_POSTING_ALLOWED","IS_ENABLED",
  "EFFECTIVE_START_DATE","SORT_ORDER","REFERENCE_LEGAL_ENTITY_ID","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES
 (1,1,'01','Finova Finance','فينوفا للتمويل',false,true,true,'2020-01-01',1,1,'SAMPLE',now(),0),
 (2,2,'HQ','Head office, Cairo','المركز الرئيسي، القاهرة',false,true,true,'2020-01-01',1,NULL,'SAMPLE',now(),0),
 (3,2,'ALX','Alexandria branch','فرع الإسكندرية',false,true,true,'2020-01-01',2,NULL,'SAMPLE',now(),0),
 (4,2,'GIZA','Giza branch','فرع الجيزة',false,true,true,'2020-01-01',3,NULL,'SAMPLE',now(),0),
 (5,3,'1100','Finance','المالية',false,true,true,'2020-01-01',1,NULL,'SAMPLE',now(),0),
 (6,3,'1200','Operations','العمليات',false,true,true,'2020-01-01',2,NULL,'SAMPLE',now(),0),
 (7,3,'1300','Sales','المبيعات',false,true,true,'2020-01-01',3,NULL,'SAMPLE',now(),0),
 (8,5,'GENERAL','General','عام',false,true,true,'2020-01-01',1,NULL,'SAMPLE',now(),0),
 (9,5,'FACTORING','Factoring','التخصيم',false,true,true,'2020-01-01',2,NULL,'SAMPLE',now(),0),
 (10,5,'MORTGAGE','Mortgage finance','التمويل العقاري',false,true,true,'2020-01-01',3,NULL,'SAMPLE',now(),0),
 (11,5,'CONSUMER','Consumer finance','التمويل الاستهلاكي',false,true,true,'2020-01-01',4,NULL,'SAMPLE',now(),0),
 (12,6,'000','Not used','غير مستخدم',false,true,true,'2020-01-01',1,NULL,'SAMPLE',now(),0),
 (13,7,'000','Not used','غير مستخدم',false,true,true,'2020-01-01',1,NULL,'SAMPLE',now(),0);

-- the chart of accounts: code, parent, name EN/AR, type, normal balance, statement, cash flow, summary
CREATE TEMP TABLE acc (n int, code text, parent text, en text, ar text, typ text, nb text, fs text, cf text, summ boolean, retained boolean DEFAULT false);
INSERT INTO acc VALUES
 (1,'1000',NULL,'Assets','الأصول','ASSET','DR','BALANCE_SHEET','NONE',true),
 (2,'1100','1000','Cash and banks','النقدية والبنوك','ASSET','DR','BALANCE_SHEET','CASH_EQUIVALENT',true),
 (3,'110101','1100','Cash at bank - NBE','نقدية بالبنك - البنك الأهلي','ASSET','DR','BALANCE_SHEET','CASH_EQUIVALENT',false),
 (4,'110102','1100','Cash at bank - CIB','نقدية بالبنك - التجاري الدولي','ASSET','DR','BALANCE_SHEET','CASH_EQUIVALENT',false),
 (5,'1200','1000','Finance receivables','مستحقات التمويل','ASSET','DR','BALANCE_SHEET','OPERATING',true),
 (6,'120101','1200','Factoring receivables','مستحقات التخصيم','ASSET','DR','BALANCE_SHEET','OPERATING',false),
 (7,'120102','1200','Mortgage loans receivable','قروض التمويل العقاري','ASSET','DR','BALANCE_SHEET','OPERATING',false),
 (8,'120103','1200','Consumer loans receivable','قروض التمويل الاستهلاكي','ASSET','DR','BALANCE_SHEET','OPERATING',false),
 (9,'1500','1000','Fixed assets','الأصول الثابتة','ASSET','DR','BALANCE_SHEET','INVESTING',true),
 (10,'150101','1500','Furniture and equipment','الأثاث والمعدات','ASSET','DR','BALANCE_SHEET','INVESTING',false),
 (11,'2000',NULL,'Liabilities','الالتزامات','LIABILITY','CR','BALANCE_SHEET','NONE',true),
 (12,'2100','2000','Bank borrowings','قروض البنوك','LIABILITY','CR','BALANCE_SHEET','FINANCING',true),
 (13,'210101','2100','Bank borrowings - NBE','قروض البنك الأهلي','LIABILITY','CR','BALANCE_SHEET','FINANCING',false),
 (14,'2200','2000','Other liabilities','التزامات أخرى','LIABILITY','CR','BALANCE_SHEET','OPERATING',true),
 (15,'220101','2200','Customer receipts','مقبوضات العملاء','LIABILITY','CR','BALANCE_SHEET','OPERATING',false),
 (16,'220102','2200','Accrued expenses','مصروفات مستحقة','LIABILITY','CR','BALANCE_SHEET','OPERATING',false),
 (17,'3000',NULL,'Equity','حقوق الملكية','EQUITY','CR','BALANCE_SHEET','NONE',true),
 (18,'310101','3000','Share capital','رأس المال','EQUITY','CR','BALANCE_SHEET','FINANCING',false),
 (19,'320101','3000','Retained earnings','الأرباح المرحلة','EQUITY','CR','BALANCE_SHEET','NONE',false),
 (20,'4000',NULL,'Revenue','الإيرادات','REVENUE','CR','INCOME_STATEMENT','NONE',true),
 (21,'410101','4000','Factoring commission income','إيرادات عمولة التخصيم','REVENUE','CR','INCOME_STATEMENT','OPERATING',false),
 (22,'410102','4000','Interest income','إيرادات الفوائد','REVENUE','CR','INCOME_STATEMENT','OPERATING',false),
 (23,'410103','4000','Setup fee income','إيرادات رسوم التأسيس','REVENUE','CR','INCOME_STATEMENT','OPERATING',false),
 (24,'5000',NULL,'Expenses','المصروفات','EXPENSE','DR','INCOME_STATEMENT','NONE',true),
 (25,'510101','5000','Salaries and wages','الأجور والمرتبات','EXPENSE','DR','INCOME_STATEMENT','OPERATING',false),
 (26,'510102','5000','Rent','الإيجار','EXPENSE','DR','INCOME_STATEMENT','OPERATING',false),
 (27,'510103','5000','Interest expense','مصروف الفوائد','EXPENSE','DR','INCOME_STATEMENT','OPERATING',false),
 (28,'510104','5000','Depreciation','الإهلاك','EXPENSE','DR','INCOME_STATEMENT','OPERATING',false);
UPDATE acc SET retained = true WHERE code = '320101';

INSERT INTO "GL_COA_SEGMENT_VALUES" ("SEGMENT_VALUE_ID","COA_SEGMENT_ID","VALUE_CODE","VALUE_NAME_EN","VALUE_NAME_AR","IS_SUMMARY","IS_POSTING_ALLOWED","IS_ENABLED",
  "EFFECTIVE_START_DATE","SORT_ORDER","CREATED_BY","CREATED_AT","ROW_VERSION")
SELECT 100 + n, 4, code, en, ar, summ, NOT summ, true, '2020-01-01', n, 'SAMPLE', now(), 0 FROM acc;
UPDATE "GL_COA_SEGMENT_VALUES" v SET "PARENT_VALUE_ID" = 100 + p.n FROM acc a JOIN acc p ON p.code = a.parent WHERE v."SEGMENT_VALUE_ID" = 100 + a.n;

INSERT INTO "GL_ACCOUNTS" ("ACCOUNT_ID","COA_ID","ACCOUNT_CODE","ACCOUNT_NAME_EN","ACCOUNT_NAME_AR","ACCOUNT_SEGMENT_VALUE_ID","ACCOUNT_TYPE","NORMAL_BALANCE",
  "FINANCIAL_STATEMENT_CLASS","CASH_FLOW_CLASS","IS_SUMMARY","IS_POSTING_ALLOWED","IS_CONTROL_ACCOUNT","IS_RECONCILIATION","IS_BRANCH_REQUIRED","IS_COST_CENTER_REQUIRED",
  "IS_BUSINESS_LINE_REQUIRED","IS_MANUAL_JOURNAL_ALLOWED","IS_SUBLEDGER_POSTING_ALLOWED","IS_CURRENCY_REVALUATION","IS_RETAINED_EARNINGS","IS_ACTIVE","EFFECTIVE_START_DATE",
  "SORT_ORDER","CREATED_BY","CREATED_AT","ROW_VERSION")
SELECT n, 1, code, en, ar, 100 + n, typ, nb, fs, cf, summ, NOT summ, code IN ('120101','120102','120103'), code LIKE '1101%', false, false,
  code LIKE '12%', NOT summ, NOT summ, false, retained, true, '2020-01-01', n, 'SAMPLE', now(), 0 FROM acc;
UPDATE "GL_ACCOUNTS" g SET "PARENT_ACCOUNT_ID" = p.n FROM acc a JOIN acc p ON p.code = a.parent WHERE g."ACCOUNT_ID" = a.n;

-- calendar, fiscal year 2026 and its twelve months: January-September closed, October and November open, December future entry
INSERT INTO "GL_CALENDARS" ("CALENDAR_ID","CALENDAR_CODE","CALENDAR_NAME_EN","CALENDAR_NAME_AR","CALENDAR_TYPE","PERIOD_FREQUENCY","IS_ADJUSTMENT_PERIOD","IS_ACTIVE","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,'FINOVA_CAL','Finova fiscal calendar','التقويم المالي لفينوفا','FISCAL','MONTHLY',false,true,'SAMPLE',now(),0);
INSERT INTO "GL_FISCAL_YEARS" ("FISCAL_YEAR_ID","CALENDAR_ID","FISCAL_YEAR","YEAR_NAME","START_DATE","END_DATE","YEAR_STATUS","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,1,2026,'FY 2026','2026-01-01','2026-12-31','OPEN','SAMPLE',now(),0);
INSERT INTO "GL_ACCOUNTING_PERIODS" ("PERIOD_ID","FISCAL_YEAR_ID","CALENDAR_ID","PERIOD_NO","PERIOD_CODE","PERIOD_NAME_EN","PERIOD_NAME_AR","START_DATE","END_DATE","QUARTER_NO",
  "IS_ADJUSTMENT_PERIOD","PERIOD_STATUS","OPENED_BY","OPENED_AT","CLOSED_BY","CLOSED_AT","CREATED_BY","CREATED_AT","ROW_VERSION")
SELECT m, 1, 1, m, to_char(make_date(2026, m, 1), 'Mon-YYYY'), to_char(make_date(2026, m, 1), 'FMMonth YYYY'), NULL,
  make_date(2026, m, 1), (make_date(2026, m, 1) + interval '1 month - 1 day')::date, (m + 2) / 3, false,
  CASE WHEN m <= 9 THEN 'CLOSED' WHEN m <= 11 THEN 'OPEN' ELSE 'FUTURE_ENTRY' END,
  'admin', make_date(2026, m, 1)::timestamptz, CASE WHEN m <= 9 THEN 'admin' END, CASE WHEN m <= 9 THEN (make_date(2026, m, 1) + interval '1 month 5 days')::timestamptz END,
  'SAMPLE', now(), 0
FROM generate_series(1, 12) m;

INSERT INTO "GL_LEDGERS" ("LEDGER_ID","LEGAL_ENTITY_ID","LEDGER_CODE","LEDGER_NAME_EN","LEDGER_NAME_AR","LEDGER_TYPE","LEDGER_CURRENCY_CODE","CALENDAR_ID","IS_PRIMARY_LEDGER",
  "LEDGER_STATUS","RETAINED_EARNINGS_ACCT_CODE","SUSPENSE_ACCT_CODE","START_DATE","NOTES","COA_ID","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,1,'FINOVA_EGP','Finova primary ledger','الدفتر الرئيسي لفينوفا','PRIMARY','EGP',1,true,'ACTIVE','320101',NULL,'2026-01-01','Primary ledger in Egyptian pounds',1,'SAMPLE',now(),0);
INSERT INTO "GL_LEDGER_OPTIONS" ("LEDGER_OPTION_ID","LEDGER_ID","ALLOW_MANUAL_JOURNALS","REQUIRE_MANUAL_JOURNAL_APPR","ALLOW_SUBLEDGER_JOURNALS","IS_AUTO_POST_SUBLEDGER",
  "ALLOW_FUTURE_PERIOD_ENTRY","ALLOW_BACKDATED_ENTRY","ALLOW_REVERSAL_JOURNALS","IS_ATTACHMENT_REQUIRED","REQUIRE_BALANCED_JOURNAL","ALLOW_STATISTICAL_JOURNALS",
  "MAX_BACKDATE_DAYS","DEFAULT_REVERSAL_METHOD","POSTING_MODE","PERIOD_CLOSE_POLICY","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,1,'Y','Y','Y',true,'Y','Y','Y',false,'Y','N',90,'SWITCH_DR_CR','NORMAL','SOFT','SAMPLE',now(),0);
INSERT INTO "GL_LEDGER_PERIODS" ("LEDGER_PERIOD_ID","LEDGER_ID","PERIOD_ID","PERIOD_STATUS","POSTING_MODE","IS_MANUAL_JOURNAL_ALLOWED","IS_SUBLEDGER_ALLOWED","IS_REVERSAL_ALLOWED",
  "IS_ADJUSTMENT_ALLOWED","OPENED_BY","OPENED_AT","CLOSED_BY","CLOSED_AT","CREATED_BY","CREATED_AT","ROW_VERSION")
SELECT "PERIOD_ID", 1, "PERIOD_ID", "PERIOD_STATUS", CASE WHEN "PERIOD_STATUS" = 'FUTURE_ENTRY' THEN 'FUTURE_ONLY' ELSE 'NORMAL' END, true, true, true, false,
  "OPENED_BY", "OPENED_AT", "CLOSED_BY", "CLOSED_AT", 'SAMPLE', now(), 0 FROM "GL_ACCOUNTING_PERIODS";

INSERT INTO "GL_JOURNAL_SOURCES" ("SOURCE_ID","SOURCE_CODE","SOURCE_NAME_EN","SOURCE_NAME_AR","SOURCE_GROUP_CODE","IS_SYSTEM_SOURCE","IS_MANUAL_ALLOWED","IS_IMPORT_ALLOWED",
  "IS_APPROVAL_REQUIRED","IS_AUTO_POST_ALLOWED","IS_REVERSAL_ALLOWED","IS_DUPLICATE_CHECK","IS_ACTIVE","DESCRIPTION","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,'MANUAL','Manual journal','قيد يدوي','GL',false,true,true,true,false,true,false,true,'Journals entered on the Journal entry page','SAMPLE',now(),0)
ON CONFLICT DO NOTHING;
INSERT INTO "GL_JOURNAL_CATEGORIES" ("CATEGORY_ID","CATEGORY_CODE","CATEGORY_NAME_EN","CATEGORY_NAME_AR","CATEGORY_TYPE","DEFAULT_SOURCE_ID","IS_REVERSAL_REQUIRED","IS_MANUAL_ALLOWED",
  "IS_IMPORT_ALLOWED","IS_ACTIVE","DESCRIPTION","CREATED_BY","CREATED_AT","ROW_VERSION")
VALUES (1,'GENERAL','General','عام','STANDARD',1,false,true,true,true,'Day-to-day journals','SAMPLE',now(),0),
       (2,'ADJUSTMENT','Adjustment','تسوية','STANDARD',1,false,true,true,true,'Corrections and reclassifications','SAMPLE',now(),0),
       (3,'ACCRUAL','Accrual','استحقاق','STANDARD',1,false,true,true,true,'Month-end accruals','SAMPLE',now(),0)
ON CONFLICT DO NOTHING;

-- posting run numbers count per ledger period but share one day: start each period's count apart so two periods posted on the same day do not clash
INSERT INTO "GL_POSTING_SEQUENCE_AUDITS" ("SEQUENCE_SCOPE","LEDGER_ID","LEDGER_PERIOD_ID","LAST_SEQUENCE_NO","GENERATED_REFERENCE","GENERATED_BY","GENERATED_AT","CREATED_BY","CREATED_AT","ROW_VERSION")
SELECT 'POSTING_RUN', 1, "LEDGER_PERIOD_ID", "LEDGER_PERIOD_ID" * 100, 'SAMPLE-START-' || "LEDGER_PERIOD_ID", 'SAMPLE', now(), 'SAMPLE', now(), 0 FROM "GL_LEDGER_PERIODS";

-- identity columns go on after the fixed ids
SELECT setval(pg_get_serial_sequence('"' || t || '"', c), (SELECT max(v) FROM (SELECT 1000 AS v) x), true)
FROM (VALUES ('GL_COA_STRUCTURES','COA_ID'),('GL_COA_SEGMENTS','COA_SEGMENT_ID'),('GL_COA_SEGMENT_VALUES','SEGMENT_VALUE_ID'),('GL_ACCOUNTS','ACCOUNT_ID'),
  ('GL_CALENDARS','CALENDAR_ID'),('GL_FISCAL_YEARS','FISCAL_YEAR_ID'),('GL_ACCOUNTING_PERIODS','PERIOD_ID'),('GL_LEDGERS','LEDGER_ID'),('GL_LEDGER_OPTIONS','LEDGER_OPTION_ID'),
  ('GL_LEDGER_PERIODS','LEDGER_PERIOD_ID'),('GL_JOURNAL_SOURCES','SOURCE_ID'),('GL_JOURNAL_CATEGORIES','CATEGORY_ID'),('FIN_LEGAL_ENTITIES','LEGAL_ENTITY_ID')) s(t, c)
WHERE pg_get_serial_sequence('"' || t || '"', c) IS NOT NULL;
COMMIT;
