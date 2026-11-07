using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MtgFromApex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The App 105 tables are rebuilt with every APEX column; their rows are reloaded from the APEX export.
            migrationBuilder.Sql("TRUNCATE \"ADM_LKP_CHEQUE_STATUSES\", \"ADM_LKP_DECISION_TYPES\", \"ADM_LKP_DOCUMENT_GROUPS\", \"ADM_LKP_DOCUMENT_TYPES\", \"ADM_LKP_GUARANTEE_TYPES\", \"ADM_LKP_INSURANCE_TYPES\", \"ADM_LKP_PAYMENT_STATUSES\", \"ADM_LKP_RISK_LEVELS\", \"MTG_ACCOUNT_PAYMENTS\", \"MTG_AML_RESULTS\", \"MTG_DEVELOPERS\", \"MTG_DEVELOPER_SIGNATORIES\", \"MTG_INQUIRY_RESULTS\", \"MTG_INVESTORS\", \"MTG_LEGAL_REVIEWS\", \"MTG_LKP_APPROVAL_STEPS\", \"MTG_LKP_DD_CHECK_TYPES\", \"MTG_LKP_INQUIRY_TYPES\", \"MTG_LKP_PORTFOLIO_STATUSES\", \"MTG_LKP_UNIT_TYPES\", \"MTG_LKP_VALUATION_TYPES\", \"MTG_PROJECTS\", \"MTG_PROJECT_UNITS\", \"MTG_PURCHASED_PORTFOLIOS\", \"MTG_PURCHASED_PORTFOLIO_ACCOUNTS\", \"MTG_PURCHASED_PORTFOLIO_CHEQUES\", \"MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS\", \"MTG_PURCHASED_PORTFOLIO_IMPORTS\", \"MTG_PURCHASED_PORTFOLIO_STATUS_HIST\", \"MTG_VALUATIONS\";");

            migrationBuilder.DropForeignKey(
                name: "FK_ADM_LKP_DOCUMENT_TYPES_ADM_LKP_DOCUMENT_GROUPS",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_ADM_LKP_PAYMENT_STATUSES",
                table: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPERS_ADM_LKP_RISK_LEVELS",
                table: "MTG_DEVELOPERS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPES",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_LEGAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEWS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPES",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPES",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES");


















            migrationBuilder.DropColumn(
                name: "MARKET_VALUE_AMOUNT",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropColumn(
                name: "ADMIN_EXPENSE_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "APPROVED_DATE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "BOARD_DECISION_CODE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "LEGAL_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "MARGIN_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "RESERVED_AMOUNT_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "SELECTED_FACE_VALUE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "SELECTED_OUTSTANDING_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "SUBMITTED_DATE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "ERROR_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "IMPORTED_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "WORKBOOK_CONTENT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "ADMIN_BASE_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "ADMIN_EXPENSE_PCT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "LEGAL_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "MARGIN_PCT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "PROPOSED_PURCHASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "UNIT_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "VALUATION_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_VALUATION_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_VALUATION_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_UNIT_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_UNIT_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_PORTFOLIO_STATUSES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_PORTFOLIO_STATUSES");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropColumn(
                name: "RESULT_TEXT",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_INSURANCE_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_INSURANCE_TYPES");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "NAME_AR",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_DOCUMENT_GROUPS");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DOCUMENT_GROUPS");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_DECISION_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DECISION_TYPES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_CURRENCIES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_CURRENCIES");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.RenameColumn(
                name: "RECOMMENDED_VALUE_AMOUNT",
                table: "MTG_VALUATIONS",
                newName: "RECOMMENDED_VALUE");

            migrationBuilder.RenameColumn(
                name: "PROJECT_UNIT_ID",
                table: "MTG_VALUATIONS",
                newName: "UNIT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_PROJECT_UNIT_ID",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "VALUATION_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "VALUATION_FEE_PER_UNIT");

            migrationBuilder.RenameColumn(
                name: "SELECTED_PAID_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "LEGAL_FEE_PER_UNIT");

            migrationBuilder.RenameColumn(
                name: "PROPOSED_PURCHASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "SELECTED_PAID_VALUE");

            migrationBuilder.RenameColumn(
                name: "OUTSTANDING_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "SELECTED_OUTSTANDING_VALUE");

            migrationBuilder.RenameColumn(
                name: "FACE_VALUE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "SELECTED_FACE_VALUE");

            migrationBuilder.RenameColumn(
                name: "TO_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "TO_STATUS");

            migrationBuilder.RenameColumn(
                name: "FROM_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "FROM_STATUS");

            migrationBuilder.RenameColumn(
                name: "PORTFOLIO_STATUS_HISTORY_ID",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "STATUS_HIST_ID");

            migrationBuilder.RenameColumn(
                name: "VALID_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "VALID_COUNT");

            migrationBuilder.RenameColumn(
                name: "TOTAL_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "ERROR_COUNT");

            migrationBuilder.RenameColumn(
                name: "IMPORT_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "VALIDATION_STATUS");

            migrationBuilder.RenameColumn(
                name: "PORTFOLIO_IMPORT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "STAGING_ID");

            migrationBuilder.RenameColumn(
                name: "PV_METHOD_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "MARGIN_BASE");

            migrationBuilder.RenameColumn(
                name: "MARGIN_BASE_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "ADMIN_BASE");

            migrationBuilder.RenameColumn(
                name: "FINANCIAL_REVIEW_ID",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "REVIEW_ID");

            migrationBuilder.RenameColumn(
                name: "PROJECT_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "PAID_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "PAID_VALUE");

            migrationBuilder.RenameColumn(
                name: "OUTSTANDING_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "OUTSTANDING_VALUE");

            migrationBuilder.RenameColumn(
                name: "CONTRACT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "CONTRACT_VALUE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_ACCOUNTS_PROJECT_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "IX_MTG_PP_ACCOUNTS_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "MARKET_VALUE_AMOUNT",
                table: "MTG_PROJECT_UNITS",
                newName: "CURRENT_MARKET_VALUE");

            migrationBuilder.RenameColumn(
                name: "CONTRACT_AMOUNT",
                table: "MTG_PROJECT_UNITS",
                newName: "CONTRACT_VALUE");

            migrationBuilder.RenameColumn(
                name: "PROJECT_UNIT_ID",
                table: "MTG_PROJECT_UNITS",
                newName: "UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "VALUATION_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "VALUATION_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "VALUATION_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "UNIT_TYPE_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "UNIT_TYPE_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "UNIT_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "STATUS_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "STATUS_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "STATUS_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_INQUIRY_TYPES",
                newName: "INQUIRY_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_INQUIRY_TYPES",
                newName: "INQUIRY_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_DD_CHECK_TYPES",
                newName: "CHECK_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_DD_CHECK_TYPES",
                newName: "CHECK_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "STEP_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "STEP_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "SEQUENCE_NO");

            migrationBuilder.RenameColumn(
                name: "NATIONAL_ID_NO",
                table: "MTG_INVESTORS",
                newName: "NATIONAL_ID");

            migrationBuilder.RenameColumn(
                name: "DECLARED_INCOME_AMOUNT",
                table: "MTG_INVESTORS",
                newName: "DECLARED_INCOME");

            migrationBuilder.RenameColumn(
                name: "BIRTH_DATE",
                table: "MTG_INVESTORS",
                newName: "DATE_OF_BIRTH_DATE");

            migrationBuilder.RenameColumn(
                name: "SUBJECT_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS",
                newName: "SUBJECT_TYPE");

            migrationBuilder.RenameColumn(
                name: "INQUIRY_RESULT_ID",
                table: "MTG_INQUIRY_RESULTS",
                newName: "INQUIRY_ID");

            migrationBuilder.RenameColumn(
                name: "NATIONAL_ID_NO",
                table: "MTG_DEVELOPER_SIGNATORIES",
                newName: "NATIONAL_ID");

            migrationBuilder.RenameColumn(
                name: "DEVELOPER_SIGNATORY_ID",
                table: "MTG_DEVELOPER_SIGNATORIES",
                newName: "SIGNATORY_ID");

            migrationBuilder.RenameColumn(
                name: "SUBJECT_TYPE_CODE",
                table: "MTG_AML_RESULTS",
                newName: "SUBJECT_TYPE");

            migrationBuilder.RenameColumn(
                name: "AML_RESULT_CODE",
                table: "MTG_AML_RESULTS",
                newName: "RESULT_CODE");

            migrationBuilder.RenameColumn(
                name: "AML_RESULT_ID",
                table: "MTG_AML_RESULTS",
                newName: "AML_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_AML_RESULTS",
                newName: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_SUBJECT_ID");

            migrationBuilder.RenameColumn(
                name: "ACCOUNT_PAYMENT_ID",
                table: "MTG_ACCOUNT_PAYMENTS",
                newName: "PAYMENT_ID");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_RISK_LEVELS",
                newName: "RISK_LEVEL_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_RISK_LEVELS",
                newName: "RISK_LEVEL_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_PAYMENT_STATUSES",
                newName: "PAYMENT_STATUS_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_PAYMENT_STATUSES",
                newName: "PAYMENT_STATUS_AR");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "INSURANCE_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "INSURANCE_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "INSURANCE_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_GUARANTEE_TYPES",
                newName: "GUARANTEE_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_GUARANTEE_TYPES",
                newName: "GUARANTEE_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_DOCUMENT_TYPES",
                newName: "DOCUMENT_TYPE_NAME");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                newName: "DOCUMENT_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "GROUP_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "GROUP_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "GROUP_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "DECISION_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "DECISION_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "DECISION_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_CURRENCIES",
                newName: "CURRENCY_NAME_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_CURRENCIES",
                newName: "CURRENCY_NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "ADM_LKP_CURRENCIES",
                newName: "CURRENCY_CODE");

            migrationBuilder.RenameColumn(
                name: "NAME_EN",
                table: "ADM_LKP_CHEQUE_STATUSES",
                newName: "CHEQUE_STATUS_EN");

            migrationBuilder.RenameColumn(
                name: "NAME_AR",
                table: "ADM_LKP_CHEQUE_STATUSES",
                newName: "CHEQUE_STATUS_AR");

            migrationBuilder.AlterColumn<string>(
                name: "VALUER_NAME",
                table: "MTG_VALUATIONS",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "VALUATION_DATE",
                table: "MTG_VALUATIONS",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "FACILITIES_STATUS",
                table: "MTG_VALUATIONS",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MARKET_VALUE",
                table: "MTG_VALUATIONS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "REPORT_DOCUMENT_ID",
                table: "MTG_VALUATIONS",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SELECTED_ACCOUNT_COUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "REJECTION_REASON",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PV_RATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LATE_FEE_RATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BOARD_DECISION_NO",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ADMIN_EXPENSE_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "APPROVED_AT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BOARD_DECISION",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CURRENT_APPROVAL_STEP",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FACE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IS_ACTIVE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MARGIN_PERCENTAGE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OUTSTANDING_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PROPOSED_PURCHASE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RESERVED_AMOUNT_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SUBMITTED_AT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WORKFLOW_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WORKFLOW_TASK_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CHANGED_BY",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CHANGE_AT",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TOTAL_ROWS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "numeric(28,2)",
                precision: 28,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VALIDATION_JSON",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "WF_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<decimal>(
                name: "VALUATION_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "VALUATION_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TOTAL_CHARGES_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PV_RATE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PV_BASE_DATE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<decimal>(
                name: "MARGIN_BASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MARGIN_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "LEGAL_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "LEGAL_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "EXPECTED_RETURN_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "CHEQUE_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "CHEQUES_PV_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CHEQUES_FACE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_BASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "ADMIN_EXPENSE_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LEGAL_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MARGIN_PERCENTAGE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PROPOSED_PURCHASE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PV_METHOD",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UNITS_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VALUATION_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PORTFOLIO_ACCOUNT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DUE_DATE",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "CHEQUE_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_VALUATION_TYPES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "INQUIRY_TYPE_CODE",
                table: "MTG_LKP_INQUIRY_TYPES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_INQUIRY_TYPES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CHECK_TYPE_CODE",
                table: "MTG_LKP_DD_CHECK_TYPES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_DD_CHECK_TYPES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "STEP_CODE",
                table: "MTG_LKP_APPROVAL_STEPS",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LEGAL_RECOMMENDATION",
                table: "MTG_LEGAL_REVIEWS",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LEGAL_FINDINGS",
                table: "MTG_LEGAL_REVIEWS",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SUBJECT_ID",
                table: "MTG_INQUIRY_RESULTS",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RESULT_DETAILS",
                table: "MTG_INQUIRY_RESULTS",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "INQUIRY_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "RESULT_CODE",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_DEVELOPERS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_AML_RESULTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "RESULT_DETAILS",
                table: "MTG_AML_RESULTS",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PAYMENT_STATUS_CODE",
                table: "MTG_ACCOUNT_PAYMENTS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "ADM_LKP_RISK_LEVELS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SORT_ORDER",
                table: "ADM_LKP_RISK_LEVELS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PAYMENT_STATUS_CODE",
                table: "ADM_LKP_PAYMENT_STATUSES",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GUARANTEE_TYPE_CODE",
                table: "ADM_LKP_GUARANTEE_TYPES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "ADM_LKP_GUARANTEE_TYPES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DOCUMENT_TYPE_ID",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "DOCUMENT_TYPE_NAME_AR",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SORT_ORDER",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_SEQ",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CHEQUE_STATUS_CODE",
                table: "ADM_LKP_CHEQUE_STATUSES",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_LKP_INQUIRY_TYPES",
                column: "INQUIRY_TYPE_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPES",
                table: "MTG_LKP_DD_CHECK_TYPES",
                column: "CHECK_TYPE_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_LKP_APPROVAL_STEPS",
                column: "STEP_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS",
                column: "RISK_LEVEL_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES",
                column: "PAYMENT_STATUS_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPES",
                table: "ADM_LKP_GUARANTEE_TYPES",
                column: "GUARANTEE_TYPE_CODE");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_TYPE_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "DOCUMENT_TYPE_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPES",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "DOCUMENT_TYPE_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES",
                column: "CHEQUE_STATUS_CODE");

            migrationBuilder.CreateTable(
                name: "MTG_ACCOUNT_SNAPSHOTS",
                columns: table => new
                {
                    SNAPSHOT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AS_OF_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PORTFOLIO_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    OUTSTANDING_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ARREARS_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NEXT_DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    PAYMENT_STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_ACCOUNT_SNAPSHOTS", x => x.SNAPSHOT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_ACCOUNT_SNAPSHOTS_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                        column: x => x.PORTFOLIO_ACCOUNT_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                        principalColumn: "PORTFOLIO_ACCOUNT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_DUE_DILIGENCES",
                columns: table => new
                {
                    DD_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    CHECK_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SUBJECT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SUBJECT_ID = table.Column<long>(type: "bigint", nullable: true),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    COMPLETION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RESULT_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RISK_LEVEL_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IS_EXCEPTION = table.Column<bool>(type: "boolean", nullable: false),
                    FINDINGS = table.Column<string>(type: "text", nullable: true),
                    RECOMMENDATION = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_DUE_DILIGENCES", x => x.DD_ID);
                    table.ForeignKey(
                        name: "FK_MTG_DUE_DILIGENCES_ADM_LKP_RISK_LEVELS",
                        column: x => x.RISK_LEVEL_CODE,
                        principalTable: "ADM_LKP_RISK_LEVELS",
                        principalColumn: "RISK_LEVEL_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_DUE_DILIGENCES_MTG_LKP_DD_CHECK_TYPES",
                        column: x => x.CHECK_TYPE_CODE,
                        principalTable: "MTG_LKP_DD_CHECK_TYPES",
                        principalColumn: "CHECK_TYPE_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_DUE_DILIGENCES_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_FINANCIAL_STUDIES",
                columns: table => new
                {
                    FIN_STUDY_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_FACE_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OUTSTANDING_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PROPOSED_PURCHASE_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TERM_MONTHS = table.Column<int>(type: "integer", nullable: true),
                    EXPECTED_RETURN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EXPECTED_RETURN_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ADMIN_EXPENSE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NET_RETURN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHEQUE_TOTAL_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHEQUE_COVERAGE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    DEPOSIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DEPOSIT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    FINANCIAL_COMMENTS = table.Column<string>(type: "text", nullable: true),
                    PREPARED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PREPARED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    APPROVED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_FINANCIAL_STUDIES", x => x.FIN_STUDY_ID);
                    table.ForeignKey(
                        name: "FK_MTG_FINANCIAL_STUDIES_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_LKP_PORTFOLIO_PAYMENT_METHODS",
                columns: table => new
                {
                    PAYMENT_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PAYMENT_METHOD_NAME_AR = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PAYMENT_METHOD_NAME_EN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DISPLAY_SEQ = table.Column<int>(type: "integer", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_LKP_PORTFOLIO_PAYMENT_METHODS", x => x.PAYMENT_METHOD_CODE);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_APPROVALS",
                columns: table => new
                {
                    APPROVAL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SEQUENCE_NO = table.Column<int>(type: "integer", nullable: false),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ASSIGNED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DECISION_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DECISION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    COMMENTS = table.Column<string>(type: "text", nullable: true),
                    RETURN_TO_STEP_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_APPROVALS", x => x.APPROVAL_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_APPROVALS_ADM_LKP_DECISION_TYPES",
                        column: x => x.DECISION_CODE,
                        principalTable: "ADM_LKP_DECISION_TYPES",
                        principalColumn: "DECISION_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_APPROVALS_MTG_LKP_APPROVAL_STEPS_RETURN_TO_STEP_CODE",
                        column: x => x.RETURN_TO_STEP_CODE,
                        principalTable: "MTG_LKP_APPROVAL_STEPS",
                        principalColumn: "STEP_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_APPROVALS_MTG_LKP_APPROVAL_STEPS_STEP_CODE",
                        column: x => x.STEP_CODE,
                        principalTable: "MTG_LKP_APPROVAL_STEPS",
                        principalColumn: "STEP_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_APPROVALS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS",
                columns: table => new
                {
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    TRAN_SET_ID = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DISBURSEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: true),
                    BANK_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: true),
                    BANK_GL_ACCOUNT = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAYMENT_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PV_METHOD = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PV_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PV_BASE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CHEQUE_COUNT = table.Column<int>(type: "integer", nullable: true),
                    CONTRACT_COUNT = table.Column<int>(type: "integer", nullable: true),
                    FACE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PV_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FUNDING_RECOVERY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MARGIN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ADMIN_FR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ADMIN_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LEGAL_FR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LEGAL_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VALUATION_FR_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VALUATION_FINAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BOARD_PRICE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PAYABLE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CLIENTS_CREATED = table.Column<int>(type: "integer", nullable: true),
                    CLIENTS_MATCHED = table.Column<int>(type: "integer", nullable: true),
                    GL_BATCH_NO = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    GL_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    GL_RESPONSE = table.Column<string>(type: "text", nullable: true),
                    WORKFLOW_TASK_ID = table.Column<long>(type: "bigint", nullable: true),
                    COMMENTS = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    STATUS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MARGIN_TREATMENT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ADMIN_TREATMENT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LEGAL_TREATMENT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VALUATION_TREATMENT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MARGIN_DEFERRED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MARGIN_IMMEDIATE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEES_DEFERRED_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEES_IMMEDIATE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS", x => x.DISBURSEMENT_ID);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                columns: table => new
                {
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    ENTITY_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENTITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    DOCUMENT_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DOCUMENT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ISSUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FILE_NAME = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MIME_TYPE = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FILE_BLOB = table.Column<byte[]>(type: "bytea", nullable: true),
                    FILE_URL = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IS_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VERIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DOCUMENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_DOCUMENTS", x => x.DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_DOCUMENTS_ADM_LKP_DOCUMENT_TYPES",
                        column: x => x.DOCUMENT_TYPE_CODE,
                        principalTable: "ADM_LKP_DOCUMENT_TYPES",
                        principalColumn: "DOCUMENT_TYPE_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_DOCUMENTS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES",
                columns: table => new
                {
                    INCOME_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CHEQUE_COUNT = table.Column<int>(type: "integer", nullable: true),
                    FUNDING_RECOVERY_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MARGIN_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEES_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    POSTING_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TRAN_SET_ID = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    POSTED_ON_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POSTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES", x => x.INCOME_LINE_ID);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_MEMOS",
                columns: table => new
                {
                    MEMO_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    VERSION_NO = table.Column<int>(type: "integer", nullable: false),
                    MEMO_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    EXECUTIVE_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    DEVELOPER_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    PROJECT_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    INVESTOR_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    FINANCIAL_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    LEGAL_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    VALUATION_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    GUARANTEE_SUMMARY = table.Column<string>(type: "text", nullable: true),
                    RECOMMENDATION = table.Column<string>(type: "text", nullable: true),
                    MEMO_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PREPARED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PREPARED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_MEMOS", x => x.MEMO_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_MEMOS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES",
                columns: table => new
                {
                    SCHEDULE_LINE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PORTFOLIO_ACCOUNT_ID = table.Column<long>(type: "bigint", nullable: false),
                    INVESTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_CLIENT_CREATED = table.Column<bool>(type: "boolean", nullable: false),
                    SOURCE_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    WALLET_CHEQUE_ID = table.Column<long>(type: "bigint", nullable: true),
                    LINE_NO = table.Column<int>(type: "integer", nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CHEQUE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PV_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DAYS_FROM_DISBURSEMENT = table.Column<int>(type: "integer", nullable: true),
                    OPENING_BALANCE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PRINCIPAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    INTEREST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CLOSING_BALANCE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MARGIN_RELEASE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FEE_RELEASE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EIR_RATE = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES", x => x.SCHEDULE_LINE_ID);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_SNAPSHOTS",
                columns: table => new
                {
                    SNAPSHOT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AS_OF_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    OUTSTANDING_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PURCHASE_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CHEQUE_COVERAGE = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    DOCUMENT_COMPLETION_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    OPEN_EXCEPTION_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CURRENT_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_SNAPSHOTS", x => x.SNAPSHOT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_SNAPSHOTS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS",
                columns: table => new
                {
                    PORTFOLIO_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    YEAR_NO = table.Column<int>(type: "integer", nullable: false),
                    LAST_NO = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS", x => new { x.PORTFOLIO_NO, x.YEAR_NO });
                });

            migrationBuilder.CreateTable(
                name: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS",
                columns: table => new
                {
                    SETTLEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    CONTRACT_EXECUTION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CUT_OFF_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ACQUISITION_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SETTLEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ACCOUNTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    APPROVED_PURCHASE_VALUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PAYMENT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ACQUISITION_COST_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY_CODE = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PAYMENT_METHOD_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BANK_GL_ACCOUNT_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    BANK_ACCOUNT_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TRANSFER_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CHEQUE_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CHEQUE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    SELLER_PAYMENT_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SETTLEMENT_STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ACCOUNTING_STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GL_BATCH_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GL_JOURNAL_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NOTES = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SETTLED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SETTLED_ON_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_PURCHASED_PORTFOLIO_SETTLEMENTS", x => x.SETTLEMENT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_PP_SETTLEMENTS_ADM_LKP_CURRENCIES",
                        column: x => x.CURRENCY_CODE,
                        principalTable: "ADM_LKP_CURRENCIES",
                        principalColumn: "CURRENCY_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_SETTLEMENTS_MTG_LKP_PORTFOLIO_PAYMENT_METHODS",
                        column: x => x.PAYMENT_METHOD_CODE,
                        principalTable: "MTG_LKP_PORTFOLIO_PAYMENT_METHODS",
                        principalColumn: "PAYMENT_METHOD_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_PP_SETTLEMENTS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_GUARANTEES",
                columns: table => new
                {
                    GUARANTEE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    GUARANTEE_TYPE_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GUARANTOR_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    GUARANTOR_ID = table.Column<long>(type: "bigint", nullable: true),
                    GUARANTEE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GUARANTEE_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    EFFECTIVE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    IS_VERIFIED = table.Column<bool>(type: "boolean", nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VERIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_GUARANTEES", x => x.GUARANTEE_ID);
                    table.ForeignKey(
                        name: "FK_MTG_GUARANTEES_ADM_LKP_GUARANTEE_TYPES",
                        column: x => x.GUARANTEE_TYPE_CODE,
                        principalTable: "ADM_LKP_GUARANTEE_TYPES",
                        principalColumn: "GUARANTEE_TYPE_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_GUARANTEES_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_GUARANTEES_MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        column: x => x.DOCUMENT_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_INSURANCES",
                columns: table => new
                {
                    INSURANCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    UNIT_ID = table.Column<long>(type: "bigint", nullable: true),
                    INSURANCE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    INSURANCE_COMPANY = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    POLICY_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUM_INSURED = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VALID_FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    VALID_TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    IS_ASSIGNED_TO_COMPANY = table.Column<bool>(type: "boolean", nullable: false),
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_INSURANCES", x => x.INSURANCE_ID);
                    table.ForeignKey(
                        name: "FK_MTG_INSURANCES_ADM_LKP_INSURANCE_TYPES",
                        column: x => x.INSURANCE_TYPE_CODE,
                        principalTable: "ADM_LKP_INSURANCE_TYPES",
                        principalColumn: "INSURANCE_TYPE_CODE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_INSURANCES_MTG_PROJECT_UNITS",
                        column: x => x.UNIT_ID,
                        principalTable: "MTG_PROJECT_UNITS",
                        principalColumn: "UNIT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_INSURANCES_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_INSURANCES_MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        column: x => x.DOCUMENT_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MTG_SECURITY_DEPOSITS",
                columns: table => new
                {
                    DEPOSIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PORTFOLIO_ID = table.Column<long>(type: "bigint", nullable: false),
                    BANK_NAME = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ACCOUNT_REFERENCE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DEPOSIT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DEPOSIT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    IS_BLOCKED = table.Column<bool>(type: "boolean", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    RELEASE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DOCUMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTG_SECURITY_DEPOSITS", x => x.DEPOSIT_ID);
                    table.ForeignKey(
                        name: "FK_MTG_SECURITY_DEPOSITS_MTG_PURCHASED_PORTFOLIOS",
                        column: x => x.PORTFOLIO_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIOS",
                        principalColumn: "PORTFOLIO_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MTG_SECURITY_DEPOSITS_MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        column: x => x.DOCUMENT_ID,
                        principalTable: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                        principalColumn: "DOCUMENT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTG_VALUATIONS_REPORT_DOCUMENT_ID",
                table: "MTG_VALUATIONS",
                column: "REPORT_DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_BOARD_DECISION",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "BOARD_DECISION");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_CURRENT_APPROVAL_STEP",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "CURRENT_APPROVAL_STEP");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_WORKFLOW_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "WORKFLOW_INSTANCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_WORKFLOW_TASK_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "WORKFLOW_TASK_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_STATUS_HIST_FROM_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "FROM_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_STATUS_HIST_TO_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "TO_STATUS");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID_WF_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                columns: new[] { "PORTFOLIO_ID", "WF_INSTANCE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_ACCOUNTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                columns: new[] { "PORTFOLIO_ID", "SELLER_CONTRACT_NO" });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_INVESTOR_ID_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                columns: new[] { "PORTFOLIO_ID", "INVESTOR_ID", "UNIT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PROJECT_UNITS_PROJECT_ID",
                table: "MTG_PROJECT_UNITS",
                column: "PROJECT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_LEGAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEWS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID",
                table: "MTG_INQUIRY_RESULTS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INQUIRY_RESULTS_RISK_LEVEL_CODE",
                table: "MTG_INQUIRY_RESULTS",
                column: "RISK_LEVEL_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_AML_RESULTS_PORTFOLIO_ID",
                table: "MTG_AML_RESULTS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_TYPE_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "DOCUMENT_TYPE_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_ACCOUNT_SNAPSHOTS_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_ACCOUNT_SNAPSHOTS",
                column: "PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_ACCOUNT_SNAPSHOTS_AS_OF_DATE_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_ACCOUNT_SNAPSHOTS",
                columns: new[] { "AS_OF_DATE", "PORTFOLIO_ACCOUNT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_DUE_DILIGENCES_CHECK_TYPE_CODE",
                table: "MTG_DUE_DILIGENCES",
                column: "CHECK_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_DUE_DILIGENCES_PORTFOLIO_ID",
                table: "MTG_DUE_DILIGENCES",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_DUE_DILIGENCES_RISK_LEVEL_CODE",
                table: "MTG_DUE_DILIGENCES",
                column: "RISK_LEVEL_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_FINANCIAL_STUDIES_PORTFOLIO_ID",
                table: "MTG_FINANCIAL_STUDIES",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_GUARANTEES_DOCUMENT_ID",
                table: "MTG_GUARANTEES",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_GUARANTEES_GUARANTEE_TYPE_CODE",
                table: "MTG_GUARANTEES",
                column: "GUARANTEE_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_GUARANTEES_PORTFOLIO_ID",
                table: "MTG_GUARANTEES",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INSURANCES_DOCUMENT_ID",
                table: "MTG_INSURANCES",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INSURANCES_INSURANCE_TYPE_CODE",
                table: "MTG_INSURANCES",
                column: "INSURANCE_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INSURANCES_PORTFOLIO_ID",
                table: "MTG_INSURANCES",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INSURANCES_UNIT_ID",
                table: "MTG_INSURANCES",
                column: "UNIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_APPROVALS_DECISION_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_APPROVALS",
                column: "DECISION_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_APPROVALS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_APPROVALS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_APPROVALS_RETURN_TO_STEP_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_APPROVALS",
                column: "RETURN_TO_STEP_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_APPROVALS_STEP_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_APPROVALS",
                column: "STEP_CODE");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_DISBURSEMENTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS",
                column: "PORTFOLIO_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_DISBURSEMENTS_TRAN_SET_ID",
                table: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS",
                column: "TRAN_SET_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_DOCUMENTS_DOCUMENT_TYPE_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                column: "DOCUMENT_TYPE_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_DOCUMENTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_INCOME_SCHEDULES_DISBURSEMENT_ID_DUE_DATE",
                table: "MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES",
                columns: new[] { "DISBURSEMENT_ID", "DUE_DATE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_MEMOS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_MEMOS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_MEMOS_PORTFOLIO_ID_VERSION_NO",
                table: "MTG_PURCHASED_PORTFOLIO_MEMOS",
                columns: new[] { "PORTFOLIO_ID", "VERSION_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_REPAYMENT_SCHEDULES_DISBURSEMENT_ID_SOURCE_CHEQUE_ID",
                table: "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES",
                columns: new[] { "DISBURSEMENT_ID", "SOURCE_CHEQUE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_SETTLEMENTS_CURRENCY_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS",
                column: "CURRENCY_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_SETTLEMENTS_PAYMENT_METHOD_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS",
                column: "PAYMENT_METHOD_CODE");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_SETTLEMENTS_SETTLEMENT_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS",
                columns: new[] { "SETTLEMENT_STATUS_CODE", "ACCOUNTING_STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_SETTLEMENTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS",
                column: "PORTFOLIO_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_PP_SNAPSHOTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_SNAPSHOTS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_SNAPSHOTS_AS_OF_DATE_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_SNAPSHOTS",
                columns: new[] { "AS_OF_DATE", "PORTFOLIO_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_TRAN_SET_COUNTERS_PORTFOLIO_NO_YEAR_NO",
                table: "MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS",
                columns: new[] { "PORTFOLIO_NO", "YEAR_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_SECURITY_DEPOSITS_DOCUMENT_ID",
                table: "MTG_SECURITY_DEPOSITS",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MTG_SECURITY_DEPOSITS_PORTFOLIO_ID",
                table: "MTG_SECURITY_DEPOSITS",
                column: "PORTFOLIO_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_ADM_LKP_PAYMENT_STATUSES",
                table: "MTG_ACCOUNT_PAYMENTS",
                column: "PAYMENT_STATUS_CODE",
                principalTable: "ADM_LKP_PAYMENT_STATUSES",
                principalColumn: "PAYMENT_STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_DEVELOPERS_ADM_LKP_RISK_LEVELS",
                table: "MTG_DEVELOPERS",
                column: "RISK_LEVEL_CODE",
                principalTable: "ADM_LKP_RISK_LEVELS",
                principalColumn: "RISK_LEVEL_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_ADM_LKP_RISK_LEVELS",
                table: "MTG_INQUIRY_RESULTS",
                column: "RISK_LEVEL_CODE",
                principalTable: "ADM_LKP_RISK_LEVELS",
                principalColumn: "RISK_LEVEL_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_INQUIRY_RESULTS",
                column: "INQUIRY_TYPE_CODE",
                principalTable: "MTG_LKP_INQUIRY_TYPES",
                principalColumn: "INQUIRY_TYPE_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "CHEQUE_STATUS_CODE",
                principalTable: "ADM_LKP_CHEQUE_STATUSES",
                principalColumn: "CHEQUE_STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_LKP_PORTFOLIO_STATUSES_FROM_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "FROM_STATUS",
                principalTable: "MTG_LKP_PORTFOLIO_STATUSES",
                principalColumn: "STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_LKP_PORTFOLIO_STATUSES_TO_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "TO_STATUS",
                principalTable: "MTG_LKP_PORTFOLIO_STATUSES",
                principalColumn: "STATUS_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_ADM_LKP_DECISION_TYPES",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "BOARD_DECISION",
                principalTable: "ADM_LKP_DECISION_TYPES",
                principalColumn: "DECISION_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "CURRENT_APPROVAL_STEP",
                principalTable: "MTG_LKP_APPROVAL_STEPS",
                principalColumn: "STEP_CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                table: "MTG_VALUATIONS",
                column: "REPORT_DOCUMENT_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                principalColumn: "DOCUMENT_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_ADM_LKP_PAYMENT_STATUSES",
                table: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPERS_ADM_LKP_RISK_LEVELS",
                table: "MTG_DEVELOPERS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_ADM_LKP_RISK_LEVELS",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_LKP_PORTFOLIO_STATUSES_FROM_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_LKP_PORTFOLIO_STATUSES_TO_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_ADM_LKP_DECISION_TYPES",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PURCHASED_PORTFOLIO_DOCUMENTS",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropTable(
                name: "MTG_ACCOUNT_SNAPSHOTS");

            migrationBuilder.DropTable(
                name: "MTG_DUE_DILIGENCES");

            migrationBuilder.DropTable(
                name: "MTG_FINANCIAL_STUDIES");

            migrationBuilder.DropTable(
                name: "MTG_GUARANTEES");

            migrationBuilder.DropTable(
                name: "MTG_INSURANCES");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_APPROVALS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_MEMOS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_SETTLEMENTS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_SNAPSHOTS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS");

            migrationBuilder.DropTable(
                name: "MTG_SECURITY_DEPOSITS");

            migrationBuilder.DropTable(
                name: "MTG_LKP_PORTFOLIO_PAYMENT_METHODS");

            migrationBuilder.DropTable(
                name: "MTG_PURCHASED_PORTFOLIO_DOCUMENTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_VALUATIONS_REPORT_DOCUMENT_ID",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_BOARD_DECISION",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_CURRENT_APPROVAL_STEP",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_WORKFLOW_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_WORKFLOW_TASK_ID",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PP_STATUS_HIST_FROM_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PP_STATUS_HIST_TO_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID_WF_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PP_ACCOUNTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_INVESTOR_ID_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_PROJECT_UNITS_PROJECT_ID",
                table: "MTG_PROJECT_UNITS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPES",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_LEGAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEWS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_INQUIRY_RESULTS_RISK_LEVEL_CODE",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropIndex(
                name: "IX_MTG_AML_RESULTS_PORTFOLIO_ID",
                table: "MTG_AML_RESULTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPES",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_TYPE_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPES",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropIndex(
                name: "UQ_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_TYPE_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.DropColumn(
                name: "MARKET_VALUE",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropColumn(
                name: "REPORT_DOCUMENT_ID",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropColumn(
                name: "ADMIN_EXPENSE_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "APPROVED_AT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "BOARD_DECISION",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "CURRENT_APPROVAL_STEP",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "FACE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "IS_ACTIVE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "MARGIN_PERCENTAGE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "OUTSTANDING_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "PROPOSED_PURCHASE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "RESERVED_AMOUNT_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "SUBMITTED_AT",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "WORKFLOW_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "WORKFLOW_TASK_ID",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropColumn(
                name: "CHANGED_BY",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropColumn(
                name: "CHANGE_AT",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropColumn(
                name: "TOTAL_ROWS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "VALIDATION_JSON",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "WF_INSTANCE_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropColumn(
                name: "ADMIN_EXPENSE_PERCENT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "LEGAL_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "MARGIN_PERCENTAGE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "PROPOSED_PURCHASE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "PV_METHOD",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "UNITS_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "VALUATION_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_VALUATION_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_PORTFOLIO_STATUSES");

            migrationBuilder.DropColumn(
                name: "INQUIRY_TYPE_CODE",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropColumn(
                name: "CHECK_TYPE_CODE",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropColumn(
                name: "STEP_CODE",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropColumn(
                name: "RESULT_CODE",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropColumn(
                name: "RISK_LEVEL_CODE",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropColumn(
                name: "PAYMENT_STATUS_CODE",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropColumn(
                name: "GUARANTEE_TYPE_CODE",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropColumn(
                name: "DOCUMENT_TYPE_ID",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "DOCUMENT_TYPE_NAME_AR",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "DISPLAY_SEQ",
                table: "ADM_LKP_DOCUMENT_GROUPS");

            migrationBuilder.DropColumn(
                name: "CHEQUE_STATUS_CODE",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.RenameColumn(
                name: "UNIT_ID",
                table: "MTG_VALUATIONS",
                newName: "PROJECT_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "RECOMMENDED_VALUE",
                table: "MTG_VALUATIONS",
                newName: "RECOMMENDED_VALUE_AMOUNT");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_UNIT_ID",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_PROJECT_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "VALUATION_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "VALUATION_FEE_PER_UNIT_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "SELECTED_PAID_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "PROPOSED_PURCHASE_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "SELECTED_OUTSTANDING_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "OUTSTANDING_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "SELECTED_FACE_VALUE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "FACE_VALUE_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "LEGAL_FEE_PER_UNIT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "SELECTED_PAID_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "TO_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "TO_STATUS_CODE");

            migrationBuilder.RenameColumn(
                name: "FROM_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "FROM_STATUS_CODE");

            migrationBuilder.RenameColumn(
                name: "STATUS_HIST_ID",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "PORTFOLIO_STATUS_HISTORY_ID");

            migrationBuilder.RenameColumn(
                name: "VALID_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "VALID_ROW_COUNT");

            migrationBuilder.RenameColumn(
                name: "VALIDATION_STATUS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "IMPORT_STATUS_CODE");

            migrationBuilder.RenameColumn(
                name: "ERROR_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "TOTAL_ROW_COUNT");

            migrationBuilder.RenameColumn(
                name: "STAGING_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "PORTFOLIO_IMPORT_ID");

            migrationBuilder.RenameColumn(
                name: "MARGIN_BASE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "PV_METHOD_CODE");

            migrationBuilder.RenameColumn(
                name: "ADMIN_BASE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "MARGIN_BASE_CODE");

            migrationBuilder.RenameColumn(
                name: "REVIEW_ID",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "FINANCIAL_REVIEW_ID");

            migrationBuilder.RenameColumn(
                name: "UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "PROJECT_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "PAID_VALUE",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "PAID_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "OUTSTANDING_VALUE",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "OUTSTANDING_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "CONTRACT_VALUE",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "CONTRACT_AMOUNT");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_ACCOUNTS_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "IX_MTG_PP_ACCOUNTS_PROJECT_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "CURRENT_MARKET_VALUE",
                table: "MTG_PROJECT_UNITS",
                newName: "MARKET_VALUE_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "CONTRACT_VALUE",
                table: "MTG_PROJECT_UNITS",
                newName: "CONTRACT_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "UNIT_ID",
                table: "MTG_PROJECT_UNITS",
                newName: "PROJECT_UNIT_ID");

            migrationBuilder.RenameColumn(
                name: "VALUATION_NAME_EN",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "VALUATION_NAME_AR",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "VALUATION_TYPE_CODE",
                table: "MTG_LKP_VALUATION_TYPES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "UNIT_TYPE_NAME_EN",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "UNIT_TYPE_NAME_AR",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "UNIT_TYPE_CODE",
                table: "MTG_LKP_UNIT_TYPES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "STATUS_NAME_EN",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "STATUS_NAME_AR",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "STATUS_CODE",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "INQUIRY_NAME_EN",
                table: "MTG_LKP_INQUIRY_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "INQUIRY_NAME_AR",
                table: "MTG_LKP_INQUIRY_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CHECK_NAME_EN",
                table: "MTG_LKP_DD_CHECK_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "CHECK_NAME_AR",
                table: "MTG_LKP_DD_CHECK_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "STEP_NAME_EN",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "STEP_NAME_AR",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "SEQUENCE_NO",
                table: "MTG_LKP_APPROVAL_STEPS",
                newName: "DISPLAY_ORDER");

            migrationBuilder.RenameColumn(
                name: "NATIONAL_ID",
                table: "MTG_INVESTORS",
                newName: "NATIONAL_ID_NO");

            migrationBuilder.RenameColumn(
                name: "DECLARED_INCOME",
                table: "MTG_INVESTORS",
                newName: "DECLARED_INCOME_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "DATE_OF_BIRTH_DATE",
                table: "MTG_INVESTORS",
                newName: "BIRTH_DATE");

            migrationBuilder.RenameColumn(
                name: "SUBJECT_TYPE",
                table: "MTG_INQUIRY_RESULTS",
                newName: "SUBJECT_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "INQUIRY_ID",
                table: "MTG_INQUIRY_RESULTS",
                newName: "INQUIRY_RESULT_ID");

            migrationBuilder.RenameColumn(
                name: "NATIONAL_ID",
                table: "MTG_DEVELOPER_SIGNATORIES",
                newName: "NATIONAL_ID_NO");

            migrationBuilder.RenameColumn(
                name: "SIGNATORY_ID",
                table: "MTG_DEVELOPER_SIGNATORIES",
                newName: "DEVELOPER_SIGNATORY_ID");

            migrationBuilder.RenameColumn(
                name: "SUBJECT_TYPE",
                table: "MTG_AML_RESULTS",
                newName: "SUBJECT_TYPE_CODE");

            migrationBuilder.RenameColumn(
                name: "RESULT_CODE",
                table: "MTG_AML_RESULTS",
                newName: "AML_RESULT_CODE");

            migrationBuilder.RenameColumn(
                name: "AML_ID",
                table: "MTG_AML_RESULTS",
                newName: "AML_RESULT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_SUBJECT_ID",
                table: "MTG_AML_RESULTS",
                newName: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID");

            migrationBuilder.RenameColumn(
                name: "PAYMENT_ID",
                table: "MTG_ACCOUNT_PAYMENTS",
                newName: "ACCOUNT_PAYMENT_ID");

            migrationBuilder.RenameColumn(
                name: "RISK_LEVEL_NAME_EN",
                table: "ADM_LKP_RISK_LEVELS",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "RISK_LEVEL_NAME_AR",
                table: "ADM_LKP_RISK_LEVELS",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "PAYMENT_STATUS_EN",
                table: "ADM_LKP_PAYMENT_STATUSES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "PAYMENT_STATUS_AR",
                table: "ADM_LKP_PAYMENT_STATUSES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "INSURANCE_NAME_EN",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "INSURANCE_NAME_AR",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "INSURANCE_TYPE_CODE",
                table: "ADM_LKP_INSURANCE_TYPES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "GUARANTEE_NAME_EN",
                table: "ADM_LKP_GUARANTEE_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "GUARANTEE_NAME_AR",
                table: "ADM_LKP_GUARANTEE_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "DOCUMENT_TYPE_NAME",
                table: "ADM_LKP_DOCUMENT_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "DOCUMENT_TYPE_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "GROUP_NAME_EN",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "GROUP_NAME_AR",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "DECISION_NAME_EN",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "DECISION_NAME_AR",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "DECISION_CODE",
                table: "ADM_LKP_DECISION_TYPES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "CURRENCY_NAME_EN",
                table: "ADM_LKP_CURRENCIES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "CURRENCY_NAME_AR",
                table: "ADM_LKP_CURRENCIES",
                newName: "NAME_AR");

            migrationBuilder.RenameColumn(
                name: "CURRENCY_CODE",
                table: "ADM_LKP_CURRENCIES",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "CHEQUE_STATUS_EN",
                table: "ADM_LKP_CHEQUE_STATUSES",
                newName: "NAME_EN");

            migrationBuilder.RenameColumn(
                name: "CHEQUE_STATUS_AR",
                table: "ADM_LKP_CHEQUE_STATUSES",
                newName: "NAME_AR");

            migrationBuilder.AlterColumn<string>(
                name: "VALUER_NAME",
                table: "MTG_VALUATIONS",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "VALUATION_DATE",
                table: "MTG_VALUATIONS",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FACILITIES_STATUS",
                table: "MTG_VALUATIONS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MARKET_VALUE_AMOUNT",
                table: "MTG_VALUATIONS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "SELECTED_ACCOUNT_COUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "REJECTION_REASON",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PV_RATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "LATE_FEE_RATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BOARD_DECISION_NO",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ADMIN_EXPENSE_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "APPROVED_DATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BOARD_DECISION_CODE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LEGAL_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MARGIN_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RESERVED_AMOUNT_PCT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SELECTED_FACE_VALUE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SELECTED_OUTSTANDING_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SUBMITTED_DATE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ERROR_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IMPORTED_ROW_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "WORKBOOK_CONTENT",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                type: "bytea",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "VALUATION_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "VALUATION_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TOTAL_CHARGES_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PV_RATE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PV_BASE_DATE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MARGIN_BASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MARGIN_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LEGAL_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LEGAL_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EXPECTED_RETURN_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CHEQUE_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CHEQUES_PV_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CHEQUES_FACE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_FINAL_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_CALC_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADMIN_BASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ADMIN_BASE_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ADMIN_EXPENSE_PCT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LEGAL_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MARGIN_PCT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PROPOSED_PURCHASE_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UNIT_COUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VALUATION_FEE_PER_UNIT_AMOUNT",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<long>(
                name: "PORTFOLIO_ACCOUNT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DUE_DATE",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CHEQUE_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_VALUATION_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_VALUATION_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_UNIT_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_UNIT_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "MTG_LKP_INQUIRY_TYPES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_INQUIRY_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_INQUIRY_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "MTG_LKP_DD_CHECK_TYPES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_DD_CHECK_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "MTG_LKP_DD_CHECK_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "MTG_LKP_APPROVAL_STEPS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "MTG_LKP_APPROVAL_STEPS",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LEGAL_RECOMMENDATION",
                table: "MTG_LEGAL_REVIEWS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LEGAL_FINDINGS",
                table: "MTG_LEGAL_REVIEWS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SUBJECT_ID",
                table: "MTG_INQUIRY_RESULTS",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RESULT_DETAILS",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "INQUIRY_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "RESULT_TEXT",
                table: "MTG_INQUIRY_RESULTS",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_DEVELOPERS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RISK_LEVEL_CODE",
                table: "MTG_AML_RESULTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RESULT_DETAILS",
                table: "MTG_AML_RESULTS",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PAYMENT_STATUS_CODE",
                table: "MTG_ACCOUNT_PAYMENTS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "ADM_LKP_RISK_LEVELS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_RISK_LEVELS",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_RISK_LEVELS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "ADM_LKP_PAYMENT_STATUSES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_PAYMENT_STATUSES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_PAYMENT_STATUSES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_INSURANCE_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_INSURANCE_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "ADM_LKP_GUARANTEE_TYPES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_GUARANTEE_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_GUARANTEE_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NAME_AR",
                table: "ADM_LKP_DOCUMENT_TYPES",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_DECISION_TYPES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_DECISION_TYPES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_CURRENCIES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_CURRENCIES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "ADM_LKP_CHEQUE_STATUSES",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "ADM_LKP_CHEQUE_STATUSES",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DISPLAY_ORDER",
                table: "ADM_LKP_CHEQUE_STATUSES",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_LKP_INQUIRY_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPES",
                table: "MTG_LKP_DD_CHECK_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_LKP_APPROVAL_STEPS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPES",
                table: "ADM_LKP_GUARANTEE_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPES",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES",
                column: "CODE");

            migrationBuilder.InsertData(
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                columns: new[] { "CODE", "CREATED_AT", "CREATED_BY", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "NAME_AR", "NAME_EN", "ROW_VERSION", "UPDATED_AT", "UPDATED_BY" },
                values: new object[,]
                {
                    { "AML_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 3, true, "تم قبول مراجعة مكافحة غسل الأموال", "AML Accepted", 1, null, null },
                    { "AML_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 4, true, "تم رفض مراجعة مكافحة غسل الأموال", "AML Rejected", 1, null, null },
                    { "BOARD_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 16, true, "تمت موافقة مجلس الإدارة", "Board Review Accepted", 1, null, null },
                    { "BOARD_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 17, true, "تم رفض مجلس الإدارة", "Board Review Rejected", 1, null, null },
                    { "CHEQUE_COVERAGE_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 12, true, "تم قبول تغطية الشيكات", "Cheque Coverage Accepted", 1, null, null },
                    { "CHEQUE_COVERAGE_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 13, true, "تم رفض تغطية الشيكات", "Cheque Coverage Rejected", 1, null, null },
                    { "COVERAGE_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 10, true, "تم قبول مراجعة التغطية", "Coverage Review Accepted", 1, null, null },
                    { "COVERAGE_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 11, true, "تم رفض مراجعة التغطية", "Coverage Review Rejected", 1, null, null },
                    { "DATA_LOADING_COMPLETED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 2, true, "اكتمل تحميل البيانات", "Data Loading Completed", 1, null, null },
                    { "DRAFT", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 1, true, "مسودة", "Draft", 1, null, null },
                    { "FINANCIAL_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 14, true, "تم قبول المراجعة المالية", "Financial Review Accepted", 1, null, null },
                    { "FINANCIAL_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 15, true, "تم رفض المراجعة المالية", "Financial Review Rejected", 1, null, null },
                    { "INQUIRY_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 5, true, "تم قبول الاستعلامات", "Inquiry Accepted", 1, null, null },
                    { "INQUIRY_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 6, true, "تم رفض الاستعلامات", "Inquiry Rejected", 1, null, null },
                    { "LEGAL_REVIEW_ACCEPTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 7, true, "تم قبول المراجعة القانونية", "Legal Review Accepted", 1, null, null },
                    { "LEGAL_REVIEW_REJECTED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 8, true, "تم رفض المراجعة القانونية", "Legal Review Rejected", 1, null, null },
                    { "VALUATION_COMPLETED", new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, 9, true, "اكتمل التقييم", "Valuation Completed", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                column: "PORTFOLIO_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                columns: new[] { "PORTFOLIO_ID", "SELLER_CONTRACT_NO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MTG_LEGAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEWS",
                column: "PORTFOLIO_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS",
                columns: new[] { "PORTFOLIO_ID", "SUBJECT_TYPE_CODE", "SUBJECT_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "DOCUMENT_GROUP_CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_ADM_LKP_DOCUMENT_TYPES_ADM_LKP_DOCUMENT_GROUPS",
                table: "ADM_LKP_DOCUMENT_TYPES",
                column: "DOCUMENT_GROUP_CODE",
                principalTable: "ADM_LKP_DOCUMENT_GROUPS",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_ADM_LKP_PAYMENT_STATUSES",
                table: "MTG_ACCOUNT_PAYMENTS",
                column: "PAYMENT_STATUS_CODE",
                principalTable: "ADM_LKP_PAYMENT_STATUSES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_DEVELOPERS_ADM_LKP_RISK_LEVELS",
                table: "MTG_DEVELOPERS",
                column: "RISK_LEVEL_CODE",
                principalTable: "ADM_LKP_RISK_LEVELS",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_INQUIRY_RESULTS",
                column: "INQUIRY_TYPE_CODE",
                principalTable: "MTG_LKP_INQUIRY_TYPES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "CHEQUE_STATUS_CODE",
                principalTable: "ADM_LKP_CHEQUE_STATUSES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
