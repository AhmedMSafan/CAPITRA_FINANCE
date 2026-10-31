using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrmTrsFctTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRM_CLIENTS",
                columns: table => new
                {
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIENT_NUMBER = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CLIENT_REF_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CLIENT_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CLIENT_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SHORT_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SHORT_NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TRADE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TRADE_NAME_AR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CLIENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LEGAL_STRUCTURE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LEGAL_STRUCTURE_AR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REGISTRATION_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TAX_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VAT_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NATIONAL_ID = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    UNIFIED_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DATE_OF_INCORPORATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DATE_OF_REGISTRATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LICENSE_NUMBER = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LICENSE_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    COUNTRY_ID = table.Column<long>(type: "bigint", nullable: false),
                    REGION_ID = table.Column<long>(type: "bigint", nullable: true),
                    CITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    INDUSTRY_ID = table.Column<long>(type: "bigint", nullable: true),
                    SECTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    BASE_CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLIENT_CATEGORY = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CLIENT_SEGMENT = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RISK_GRADE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ANNUAL_TURNOVER = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    NUMBER_OF_EMPLOYEES = table.Column<long>(type: "bigint", nullable: true),
                    CAPITAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    WEBSITE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MOBILE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FAX_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RELATIONSHIP_MANAGER = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RM_EMPLOYEE_ID = table.Column<long>(type: "bigint", nullable: true),
                    ONBOARDING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FIRST_FACILITY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NEXT_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    KYC_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    KYC_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    AML_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AML_CHECK_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_BLACKLISTED = table.Column<bool>(type: "boolean", nullable: false),
                    BLACKLIST_REASON = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BLACKLIST_REASON_AR = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IS_RELATED_PARTY = table.Column<bool>(type: "boolean", nullable: false),
                    PARENT_CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    GROUP_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    GROUP_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    GROUP_NAME_AR = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LOGO_URL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTES = table.Column<string>(type: "text", nullable: true),
                    NOTES_AR = table.Column<string>(type: "text", nullable: true),
                    ATTRIBUTE1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ATTRIBUTE5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    SUPPLIER_BANK_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUPPLIER_BANK_ACCOUNT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_SUPPLIER = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRM_CLIENTS", x => x.CLIENT_ID);
                    table.ForeignKey(
                        name: "FK_CRM_CLIENTS_CRM_CLIENTS",
                        column: x => x.PARENT_CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_BANKS",
                columns: table => new
                {
                    BANK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    BANK_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BANK_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    BANK_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    CBE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SWIFT_BIC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FRA_REGISTRATION_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RELATIONSHIP_MANAGER_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RELATIONSHIP_MANAGER_EMAIL = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    RELATIONSHIP_MANAGER_PHONE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    KYC_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    KYC_REVIEW_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    KYC_EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_BANKS", x => x.BANK_ID);
                });

            migrationBuilder.CreateTable(
                name: "TRS_CALENDARS",
                columns: table => new
                {
                    CALENDAR_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    CALENDAR_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    COUNTRY_CODE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    IS_BUSINESS_DAY = table.Column<bool>(type: "boolean", nullable: false),
                    HOLIDAY_NAME_EN = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    HOLIDAY_NAME_AR = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    IS_CBE_NON_WORKING = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_CALENDARS", x => x.CALENDAR_ID);
                });

            migrationBuilder.CreateTable(
                name: "TRS_FACILITY_RUNOFF_OPTIONS",
                columns: table => new
                {
                    RUNOFF_OPTIONS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUNOFF_CASE_ID = table.Column<long>(type: "bigint", nullable: false),
                    INTEREST_DAY_BASIS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REPAYMENT_FREQUENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AMORTIZATION_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BANK_COLLECTION_POLICY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCRUAL_FREQUENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RATE_MODE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ADJUSTMENT_POSTING_MODE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IS_GENERATE_ACCOUNTING = table.Column<bool>(type: "boolean", nullable: false),
                    IS_MIRROR_FACTORING_RUNOFF = table.Column<bool>(type: "boolean", nullable: false),
                    IS_INCLUDE_FEES = table.Column<bool>(type: "boolean", nullable: false),
                    IS_NO_BANK_LATE_FEE = table.Column<bool>(type: "boolean", nullable: false),
                    PREVIEW_TARGET_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PREVIEW_COLLECTION_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_FACILITY_RUNOFF_OPTIONS", x => x.RUNOFF_OPTIONS_ID);
                });

            migrationBuilder.CreateTable(
                name: "FCT_DISBURSEMENTS",
                columns: table => new
                {
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DISBURSEMENT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DISBURSEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    FUNDING_REQUEST_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    AGREEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    DISBURSEMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DISBURSEMENT_TYPE_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CURRENCY_ID = table.Column<long>(type: "bigint", nullable: false),
                    GROSS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FEE_DEDUCTED = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    INTEREST_DEDUCTED = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    TAX_DEDUCTED = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    OTHER_DEDUCTIONS = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    NET_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    BASE_CURRENCY_ID = table.Column<long>(type: "bigint", nullable: true),
                    EXCHANGE_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    BASE_NET_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PAYMENT_METHOD = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAYMENT_METHOD_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAYMENT_CHANNEL = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAYMENT_CHANNEL_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    BENEFICIARY_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BENEFICIARY_BANK = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BENEFICIARY_IBAN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BENEFICIARY_SWIFT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BENEFICIARY_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REMITTING_BANK = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    REMITTING_ACCOUNT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    REMITTING_SWIFT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VALUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXECUTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXECUTION_TIME_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAYMENT_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BANK_REFERENCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SWIFT_REFERENCE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TRANSACTION_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_CONFIRMED = table.Column<bool>(type: "boolean", nullable: false),
                    CONFIRMATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CONFIRMATION_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CONFIRMED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IS_RECONCILED = table.Column<bool>(type: "boolean", nullable: false),
                    RECONCILIATION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RECONCILIATION_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RECONCILED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IS_REVERSED = table.Column<bool>(type: "boolean", nullable: false),
                    REVERSAL_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    REVERSAL_REASON = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REVERSAL_REASON_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    REVERSED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REVERSAL_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ORIGINAL_DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_GL_POSTED = table.Column<bool>(type: "boolean", nullable: false),
                    GL_POSTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    GL_JOURNAL_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PROCESSED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AUTHORIZED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AUTHORIZATION_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SECOND_AUTHORIZED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SECOND_AUTH_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    STATUS_ID = table.Column<long>(type: "bigint", nullable: true),
                    PAYMENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAYMENT_STATUS_AR = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FAILURE_REASON = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FAILURE_REASON_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RETRY_COUNT = table.Column<int>(type: "integer", nullable: true),
                    LAST_RETRY_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REMARKS = table.Column<string>(type: "text", nullable: true),
                    REMARKS_AR = table.Column<string>(type: "text", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    LIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    REPAYMENT_START_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TENOR_MONTH = table.Column<int>(type: "integer", nullable: true),
                    INDEX_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SPREAD_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    BASE_DAY_COUNT_BASIS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PRINCIPAL_REPAYMENT_SCHEDULE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PRINCIPAL_REPAYMENT_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PRINCIPAL_DISTRIBUTION_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    INTEREST_REPAYMENT_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ADVANCE_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PRODUCT_ID = table.Column<long>(type: "bigint", nullable: true),
                    LEGAL_ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    APPROVED_INVOICE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    APPROVED_ADVANCE_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    PROCESSING_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SOURCE_FUND_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_PROCESSED = table.Column<bool>(type: "boolean", nullable: false),
                    SOURCE_BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    FUNDING_MAPPING_ID = table.Column<long>(type: "bigint", nullable: true),
                    TRANCHE_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUBLIMIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    FACILITY_POLICY_CHECK_ID = table.Column<long>(type: "bigint", nullable: true),
                    FACILITY_RESERVATION_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FACILITY_RESERVED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FACILITY_RESERVATION_KEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FACILITY_RESERVED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TREASURY_DRAWDOWN_ID = table.Column<long>(type: "bigint", nullable: true),
                    POLICY_BUSINESS_LINE_MEMBER_ID = table.Column<long>(type: "bigint", nullable: true),
                    POLICY_PRODUCT_MEMBER_ID = table.Column<long>(type: "bigint", nullable: true),
                    POLICY_ACTIVITY_MEMBER_ID = table.Column<long>(type: "bigint", nullable: true),
                    POLICY_ASSET_MEMBER_ID = table.Column<long>(type: "bigint", nullable: true),
                    POLICY_CLIENT_SEGMENT_MEMBER_ID = table.Column<long>(type: "bigint", nullable: true),
                    FACILITY_RATE_ID = table.Column<long>(type: "bigint", nullable: true),
                    FACILITY_INTEREST_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    BANK_SCHEDULE_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BANK_SCHEDULE_INSTALLMENT_COUNT = table.Column<int>(type: "integer", nullable: true),
                    BANK_SCHEDULE_TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FCT_DISBURSEMENTS", x => x.DISBURSEMENT_ID);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENTS_CRM_CLIENTS",
                        column: x => x.CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCT_DISBURSEMENTS_FCT_DISBURSEMENTS",
                        column: x => x.ORIGINAL_DISBURSEMENT_ID,
                        principalTable: "FCT_DISBURSEMENTS",
                        principalColumn: "DISBURSEMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_BANK_ACCOUNTS",
                columns: table => new
                {
                    ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARENT_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ACCOUNT_REF = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ACCOUNT_NO = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IBAN = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ACCOUNT_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PURPOSE_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    PURPOSE_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    OPENING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CLOSING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ERP_ACCOUNT_CODE = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AUTHORIZED_SIGNATORIES_JSON = table.Column<string>(type: "text", nullable: true),
                    CURRENT_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_BANK_ACCOUNTS", x => x.ACCOUNT_ID);
                    table.ForeignKey(
                        name: "FK_TRS_BANK_ACCOUNTS_TRS_BANKS",
                        column: x => x.BANK_ID,
                        principalTable: "TRS_BANKS",
                        principalColumn: "BANK_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_BANK_ACCOUNTS_TRS_BANK_ACCOUNTS",
                        column: x => x.PARENT_ACCOUNT_ID,
                        principalTable: "TRS_BANK_ACCOUNTS",
                        principalColumn: "ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_CREDIT_FACILITIES",
                columns: table => new
                {
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACILITY_REF = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACILITY_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    APPROVED_LIMIT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    OUTSTANDING_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    AVAILABLE_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    INTEREST_RATE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BASE_RATE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MARGIN_BPS = table.Column<decimal>(type: "numeric(9,3)", precision: 9, scale: 3, nullable: true),
                    ALL_IN_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    GRACE_PERIOD_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    AMORTIZATION_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SECURITY_DESCRIPTION_EN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SECURITY_DESCRIPTION_AR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FACILITY_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FRA_REGISTRATION_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TERM_SHEET_DOC_ID = table.Column<long>(type: "bigint", nullable: true),
                    SIGNED_CONTRACT_DOC_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_REVOLVING = table.Column<bool>(type: "boolean", nullable: false),
                    NFA_UTILIZED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_DAY_BASIS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    REPAYMENT_FREQUENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BANK_COLLECTION_POLICY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ACCRUAL_FREQUENCY_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ADJUSTMENT_POSTING_MODE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RESERVED_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_CREDIT_FACILITIES", x => x.FACILITY_ID);
                    table.ForeignKey(
                        name: "FK_TRS_CREDIT_FACILITIES_TRS_BANKS",
                        column: x => x.BANK_ID,
                        principalTable: "TRS_BANKS",
                        principalColumn: "BANK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_FACILITY_CALENDAR_DAYS",
                columns: table => new
                {
                    CALENDAR_DAY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: true),
                    CALENDAR_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    IS_WORKING_DAY = table.Column<bool>(type: "boolean", nullable: false),
                    HOLIDAY_NAME_EN = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    HOLIDAY_NAME_AR = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_FACILITY_CALENDAR_DAYS", x => x.CALENDAR_DAY_ID);
                    table.ForeignKey(
                        name: "FK_TRS_FACILITY_CALENDAR_DAYS_TRS_BANKS",
                        column: x => x.BANK_ID,
                        principalTable: "TRS_BANKS",
                        principalColumn: "BANK_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_FACILITY_REPAYMENT_PLANS",
                columns: table => new
                {
                    REPAYMENT_PLAN_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUNOFF_CASE_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    DRAWDOWN_ID = table.Column<long>(type: "bigint", nullable: true),
                    REPAYMENT_RULE_ID = table.Column<long>(type: "bigint", nullable: true),
                    INSTALLMENT_NO = table.Column<int>(type: "integer", nullable: false),
                    CONTRACTUAL_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    MANUAL_BANK_COLLECTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    BANK_COLLECTION_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PERIOD_START_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PERIOD_END_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    OPENING_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    PRINCIPAL_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    INTEREST_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FEE_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CLOSING_BALANCE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    PLAN_SOURCE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PLAN_STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DAY_COUNT = table.Column<int>(type: "integer", nullable: true),
                    ANNUAL_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CLIENT_INTEREST_RATE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CLIENT_INTEREST_DUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_FACILITY_REPAYMENT_PLANS", x => x.REPAYMENT_PLAN_ID);
                    table.ForeignKey(
                        name: "FK_TRS_FACILITY_REPAYMENT_PLANS_TRS_CREDIT_FACILITIES",
                        column: x => x.FACILITY_ID,
                        principalTable: "TRS_CREDIT_FACILITIES",
                        principalColumn: "FACILITY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_CITY_ID",
                table: "CRM_CLIENTS",
                column: "CITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_COUNTRY_ID",
                table: "CRM_CLIENTS",
                column: "COUNTRY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_GROUP_CODE",
                table: "CRM_CLIENTS",
                column: "GROUP_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_INDUSTRY_ID",
                table: "CRM_CLIENTS",
                column: "INDUSTRY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_KYC_STATUS_KYC_EXPIRY_DATE",
                table: "CRM_CLIENTS",
                columns: new[] { "KYC_STATUS", "KYC_EXPIRY_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_PARENT_CLIENT_ID",
                table: "CRM_CLIENTS",
                column: "PARENT_CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_RM_EMPLOYEE_ID",
                table: "CRM_CLIENTS",
                column: "RM_EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_SECTOR_ID",
                table: "CRM_CLIENTS",
                column: "SECTOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_STATUS_ID",
                table: "CRM_CLIENTS",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CLIENTS_TAX_NUMBER",
                table: "CRM_CLIENTS",
                column: "TAX_NUMBER");

            migrationBuilder.CreateIndex(
                name: "UQ_CRM_CLIENTS_CLIENT_NUMBER",
                table: "CRM_CLIENTS",
                column: "CLIENT_NUMBER",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_AGREEMENT_ID",
                table: "FCT_DISBURSEMENTS",
                column: "AGREEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_BANK_ACCOUNT_ID",
                table: "FCT_DISBURSEMENTS",
                column: "BANK_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_BANK_REFERENCE",
                table: "FCT_DISBURSEMENTS",
                column: "BANK_REFERENCE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_BASE_CURRENCY_ID",
                table: "FCT_DISBURSEMENTS",
                column: "BASE_CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_CLIENT_ID",
                table: "FCT_DISBURSEMENTS",
                column: "CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_CLIENT_ID_CURRENCY_ID_DISBURSEMENT_DATE",
                table: "FCT_DISBURSEMENTS",
                columns: new[] { "CLIENT_ID", "CURRENCY_ID", "DISBURSEMENT_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_CURRENCY_ID",
                table: "FCT_DISBURSEMENTS",
                column: "CURRENCY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_DISBURSEMENT_DATE",
                table: "FCT_DISBURSEMENTS",
                column: "DISBURSEMENT_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_DISBURSEMENT_TYPE",
                table: "FCT_DISBURSEMENTS",
                column: "DISBURSEMENT_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_EXECUTION_DATE",
                table: "FCT_DISBURSEMENTS",
                column: "EXECUTION_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_FACILITY_ID_SUBLIMIT_ID",
                table: "FCT_DISBURSEMENTS",
                columns: new[] { "FACILITY_ID", "SUBLIMIT_ID", "FACILITY_RESERVATION_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_FUNDING_REQUEST_ID",
                table: "FCT_DISBURSEMENTS",
                column: "FUNDING_REQUEST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_IS_CONFIRMED",
                table: "FCT_DISBURSEMENTS",
                column: "IS_CONFIRMED");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_IS_GL_POSTED",
                table: "FCT_DISBURSEMENTS",
                column: "IS_GL_POSTED");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_IS_RECONCILED",
                table: "FCT_DISBURSEMENTS",
                column: "IS_RECONCILED");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_IS_REVERSED",
                table: "FCT_DISBURSEMENTS",
                column: "IS_REVERSED");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_ORIGINAL_DISBURSEMENT_ID",
                table: "FCT_DISBURSEMENTS",
                column: "ORIGINAL_DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_PAYMENT_CHANNEL",
                table: "FCT_DISBURSEMENTS",
                column: "PAYMENT_CHANNEL");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_PAYMENT_METHOD",
                table: "FCT_DISBURSEMENTS",
                column: "PAYMENT_METHOD");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_PAYMENT_REFERENCE",
                table: "FCT_DISBURSEMENTS",
                column: "PAYMENT_REFERENCE");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_PAYMENT_STATUS",
                table: "FCT_DISBURSEMENTS",
                column: "PAYMENT_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_STATUS_ID",
                table: "FCT_DISBURSEMENTS",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_TRANSACTION_ID",
                table: "FCT_DISBURSEMENTS",
                column: "TRANSACTION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FCT_DISBURSEMENTS_VALUE_DATE",
                table: "FCT_DISBURSEMENTS",
                column: "VALUE_DATE");

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_DISBURSEMENTS_DISBURSEMENT_NUMBER",
                table: "FCT_DISBURSEMENTS",
                column: "DISBURSEMENT_NUMBER",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_FCT_DISBURSEMENTS_FACILITY_RESERVATION_KEY",
                table: "FCT_DISBURSEMENTS",
                column: "FACILITY_RESERVATION_KEY",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_ACCOUNTS_COMPANY_ID_BANK_ID_ACCOUNT_TYPE_CODE",
                table: "TRS_BANK_ACCOUNTS",
                columns: new[] { "COMPANY_ID", "BANK_ID", "ACCOUNT_TYPE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_ACCOUNTS_IBAN",
                table: "TRS_BANK_ACCOUNTS",
                column: "IBAN");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_ACCOUNTS_PARENT_ACCOUNT_ID",
                table: "TRS_BANK_ACCOUNTS",
                column: "PARENT_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_BANK_ACCOUNTS_BANK_ID_ACCOUNT_NO",
                table: "TRS_BANK_ACCOUNTS",
                columns: new[] { "BANK_ID", "ACCOUNT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_BANK_ACCOUNTS_COMPANY_ID_ACCOUNT_REF",
                table: "TRS_BANK_ACCOUNTS",
                columns: new[] { "COMPANY_ID", "ACCOUNT_REF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANKS_BANK_NAME_AR",
                table: "TRS_BANKS",
                column: "BANK_NAME_AR");

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_BANKS_COMPANY_ID_BANK_CODE",
                table: "TRS_BANKS",
                columns: new[] { "COMPANY_ID", "BANK_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_BANKS_SWIFT_BIC",
                table: "TRS_BANKS",
                column: "SWIFT_BIC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_CALENDARS_CALENDAR_DATE_IS_BUSINESS_DAY",
                table: "TRS_CALENDARS",
                columns: new[] { "CALENDAR_DATE", "IS_BUSINESS_DAY" });

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_CALENDARS_COMPANY_ID_CALENDAR_DATE",
                table: "TRS_CALENDARS",
                columns: new[] { "COMPANY_ID", "CALENDAR_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_CREDIT_FACILITIES_BANK_ID_FACILITY_TYPE_CODE",
                table: "TRS_CREDIT_FACILITIES",
                columns: new[] { "BANK_ID", "FACILITY_TYPE_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_CREDIT_FACILITIES_COMPANY_ID_FACILITY_STATUS",
                table: "TRS_CREDIT_FACILITIES",
                columns: new[] { "COMPANY_ID", "FACILITY_STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_CREDIT_FACILITIES_MATURITY_DATE",
                table: "TRS_CREDIT_FACILITIES",
                column: "MATURITY_DATE");

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_CREDIT_FACILITIES_COMPANY_ID_FACILITY_REF",
                table: "TRS_CREDIT_FACILITIES",
                columns: new[] { "COMPANY_ID", "FACILITY_REF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_FACILITY_CALENDAR_DAYS_CALENDAR_DATE_IS_WORKING_DAY",
                table: "TRS_FACILITY_CALENDAR_DAYS",
                columns: new[] { "CALENDAR_DATE", "IS_WORKING_DAY" });

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_FACILITY_CALENDAR_DAYS_BANK_ID_CALENDAR_DATE",
                table: "TRS_FACILITY_CALENDAR_DAYS",
                columns: new[] { "BANK_ID", "CALENDAR_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_FACILITY_REPAYMENT_PLANS_DRAWDOWN_ID",
                table: "TRS_FACILITY_REPAYMENT_PLANS",
                columns: new[] { "DRAWDOWN_ID", "BANK_COLLECTION_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_FACILITY_REPAYMENT_PLANS_FACILITY_ID",
                table: "TRS_FACILITY_REPAYMENT_PLANS",
                columns: new[] { "FACILITY_ID", "BANK_COLLECTION_DATE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_FACILITY_REPAYMENT_PLANS_RUNOFF_CASE_ID",
                table: "TRS_FACILITY_REPAYMENT_PLANS",
                columns: new[] { "RUNOFF_CASE_ID", "CONTRACTUAL_DUE_DATE" });

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_FACILITY_REPAYMENT_PLANS_RUNOFF_CASE_ID_INSTALLMENT_NO",
                table: "TRS_FACILITY_REPAYMENT_PLANS",
                columns: new[] { "RUNOFF_CASE_ID", "INSTALLMENT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_FACILITY_RUNOFF_OPTIONS_RUNOFF_CASE_ID",
                table: "TRS_FACILITY_RUNOFF_OPTIONS",
                column: "RUNOFF_CASE_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FCT_DISBURSEMENTS");

            migrationBuilder.DropTable(
                name: "TRS_BANK_ACCOUNTS");

            migrationBuilder.DropTable(
                name: "TRS_CALENDARS");

            migrationBuilder.DropTable(
                name: "TRS_FACILITY_CALENDAR_DAYS");

            migrationBuilder.DropTable(
                name: "TRS_FACILITY_REPAYMENT_PLANS");

            migrationBuilder.DropTable(
                name: "TRS_FACILITY_RUNOFF_OPTIONS");

            migrationBuilder.DropTable(
                name: "CRM_CLIENTS");

            migrationBuilder.DropTable(
                name: "TRS_CREDIT_FACILITIES");

            migrationBuilder.DropTable(
                name: "TRS_BANKS");
        }
    }
}
