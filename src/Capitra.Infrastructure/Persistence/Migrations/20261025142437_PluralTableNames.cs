using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PluralTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ADM_LKP_DOCUMENT_TYPE_ADM_LKP_DOCUMENT_GROUP",
                table: "ADM_LKP_DOCUMENT_TYPE");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENT_ADM_LKP_PAYMENT_STATUS",
                table: "MTG_ACCOUNT_PAYMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENT_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_ACCOUNT_PAYMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_AML_RESULT_MTG_PORTFOLIO",
                table: "MTG_AML_RESULT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPER_ADM_LKP_RISK_LEVEL",
                table: "MTG_DEVELOPER");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPER_SIGNATORY_MTG_DEVELOPER",
                table: "MTG_DEVELOPER_SIGNATORY");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULT_MTG_LKP_INQUIRY_TYPE",
                table: "MTG_INQUIRY_RESULT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULT_MTG_PORTFOLIO",
                table: "MTG_INQUIRY_RESULT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_LEGAL_REVIEW_MTG_PORTFOLIO",
                table: "MTG_LEGAL_REVIEW");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_ADM_LKP_CURRENCY",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_DEVELOPER",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_LKP_PORTFOLIO_STATUS",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_PROJECT",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_INVESTOR",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PROJECT_UNIT",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_ADM_LKP_CHEQUE_STATUS",
                table: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_FINANCIAL_REVIEW_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_IMPORT_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_IMPORT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PORTFOLIO_STATUS_HISTORY_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_STATUS_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECT_MTG_DEVELOPER",
                table: "MTG_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECT_UNIT_MTG_LKP_UNIT_TYPE",
                table: "MTG_PROJECT_UNIT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECT_UNIT_MTG_PROJECT",
                table: "MTG_PROJECT_UNIT");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATION_MTG_LKP_VALUATION_TYPE",
                table: "MTG_VALUATION");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATION_MTG_PORTFOLIO",
                table: "MTG_VALUATION");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATION_MTG_PROJECT",
                table: "MTG_VALUATION");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATION_MTG_PROJECT_UNIT",
                table: "MTG_VALUATION");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_ROLE_CLAIM_SEC_ROLE",
                table: "SEC_ROLE_CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_CLAIM_SEC_USER",
                table: "SEC_USER_CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_LOGIN_SEC_USER",
                table: "SEC_USER_LOGIN");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_PASSKEY_SEC_USER",
                table: "SEC_USER_PASSKEY");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_ROLE_SEC_ROLE",
                table: "SEC_USER_ROLE");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_ROLE_SEC_USER",
                table: "SEC_USER_ROLE");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_TOKEN_SEC_USER",
                table: "SEC_USER_TOKEN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_TOKEN",
                table: "SEC_USER_TOKEN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_ROLE",
                table: "SEC_USER_ROLE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_PASSKEY",
                table: "SEC_USER_PASSKEY");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_LOGIN",
                table: "SEC_USER_LOGIN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_CLAIM",
                table: "SEC_USER_CLAIM");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER",
                table: "SEC_USER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_ROLE_CLAIM",
                table: "SEC_ROLE_CLAIM");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_ROLE",
                table: "SEC_ROLE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_VALUATION",
                table: "MTG_VALUATION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PROJECT_UNIT",
                table: "MTG_PROJECT_UNIT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PROJECT",
                table: "MTG_PROJECT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO_STATUS_HISTORY",
                table: "MTG_PORTFOLIO_STATUS_HISTORY");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO_IMPORT",
                table: "MTG_PORTFOLIO_IMPORT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO_FINANCIAL_REVIEW",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO_CHEQUE",
                table: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_VALUATION_TYPE",
                table: "MTG_LKP_VALUATION_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_UNIT_TYPE",
                table: "MTG_LKP_UNIT_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_PORTFOLIO_STATUS",
                table: "MTG_LKP_PORTFOLIO_STATUS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPE",
                table: "MTG_LKP_INQUIRY_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPE",
                table: "MTG_LKP_DD_CHECK_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEP",
                table: "MTG_LKP_APPROVAL_STEP");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LEGAL_REVIEW",
                table: "MTG_LEGAL_REVIEW");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_INVESTOR",
                table: "MTG_INVESTOR");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_INQUIRY_RESULT",
                table: "MTG_INQUIRY_RESULT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_DEVELOPER_SIGNATORY",
                table: "MTG_DEVELOPER_SIGNATORY");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_DEVELOPER",
                table: "MTG_DEVELOPER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_AML_RESULT",
                table: "MTG_AML_RESULT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_ACCOUNT_PAYMENT",
                table: "MTG_ACCOUNT_PAYMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVEL",
                table: "ADM_LKP_RISK_LEVEL");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUS",
                table: "ADM_LKP_PAYMENT_STATUS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_INSURANCE_TYPE",
                table: "ADM_LKP_INSURANCE_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPE",
                table: "ADM_LKP_GUARANTEE_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPE",
                table: "ADM_LKP_DOCUMENT_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_GROUP",
                table: "ADM_LKP_DOCUMENT_GROUP");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DECISION_TYPE",
                table: "ADM_LKP_DECISION_TYPE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CURRENCY",
                table: "ADM_LKP_CURRENCY");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_COUNTRY",
                table: "ADM_LKP_COUNTRY");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUS",
                table: "ADM_LKP_CHEQUE_STATUS");

            migrationBuilder.RenameTable(
                name: "SEC_USER_TOKEN",
                newName: "SEC_USER_TOKENS");

            migrationBuilder.RenameTable(
                name: "SEC_USER_ROLE",
                newName: "SEC_USER_ROLES");

            migrationBuilder.RenameTable(
                name: "SEC_USER_PASSKEY",
                newName: "SEC_USER_PASSKEYS");

            migrationBuilder.RenameTable(
                name: "SEC_USER_LOGIN",
                newName: "SEC_USER_LOGINS");

            migrationBuilder.RenameTable(
                name: "SEC_USER_CLAIM",
                newName: "SEC_USER_CLAIMS");

            migrationBuilder.RenameTable(
                name: "SEC_USER",
                newName: "SEC_USERS");

            migrationBuilder.RenameTable(
                name: "SEC_ROLE_CLAIM",
                newName: "SEC_ROLE_CLAIMS");

            migrationBuilder.RenameTable(
                name: "SEC_ROLE",
                newName: "SEC_ROLES");

            migrationBuilder.RenameTable(
                name: "MTG_VALUATION",
                newName: "MTG_VALUATIONS");

            migrationBuilder.RenameTable(
                name: "MTG_PROJECT_UNIT",
                newName: "MTG_PROJECT_UNITS");

            migrationBuilder.RenameTable(
                name: "MTG_PROJECT",
                newName: "MTG_PROJECTS");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO_STATUS_HISTORY",
                newName: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO_IMPORT",
                newName: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                newName: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO_CHEQUE",
                newName: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO_ACCOUNT",
                newName: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.RenameTable(
                name: "MTG_PORTFOLIO",
                newName: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_VALUATION_TYPE",
                newName: "MTG_LKP_VALUATION_TYPES");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_UNIT_TYPE",
                newName: "MTG_LKP_UNIT_TYPES");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_PORTFOLIO_STATUS",
                newName: "MTG_LKP_PORTFOLIO_STATUSES");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_INQUIRY_TYPE",
                newName: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_DD_CHECK_TYPE",
                newName: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_APPROVAL_STEP",
                newName: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.RenameTable(
                name: "MTG_LEGAL_REVIEW",
                newName: "MTG_LEGAL_REVIEWS");

            migrationBuilder.RenameTable(
                name: "MTG_INVESTOR",
                newName: "MTG_INVESTORS");

            migrationBuilder.RenameTable(
                name: "MTG_INQUIRY_RESULT",
                newName: "MTG_INQUIRY_RESULTS");

            migrationBuilder.RenameTable(
                name: "MTG_DEVELOPER_SIGNATORY",
                newName: "MTG_DEVELOPER_SIGNATORIES");

            migrationBuilder.RenameTable(
                name: "MTG_DEVELOPER",
                newName: "MTG_DEVELOPERS");

            migrationBuilder.RenameTable(
                name: "MTG_AML_RESULT",
                newName: "MTG_AML_RESULTS");

            migrationBuilder.RenameTable(
                name: "MTG_ACCOUNT_PAYMENT",
                newName: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_RISK_LEVEL",
                newName: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_PAYMENT_STATUS",
                newName: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_INSURANCE_TYPE",
                newName: "ADM_LKP_INSURANCE_TYPES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_GUARANTEE_TYPE",
                newName: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DOCUMENT_TYPE",
                newName: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DOCUMENT_GROUP",
                newName: "ADM_LKP_DOCUMENT_GROUPS");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DECISION_TYPE",
                newName: "ADM_LKP_DECISION_TYPES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_CURRENCY",
                newName: "ADM_LKP_CURRENCIES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_COUNTRY",
                newName: "ADM_LKP_COUNTRIES");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_CHEQUE_STATUS",
                newName: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_ROLE_ROLE_ID",
                table: "SEC_USER_ROLES",
                newName: "IX_SEC_USER_ROLES_ROLE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_PASSKEY_USER_ID",
                table: "SEC_USER_PASSKEYS",
                newName: "IX_SEC_USER_PASSKEYS_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_LOGIN_USER_ID",
                table: "SEC_USER_LOGINS",
                newName: "IX_SEC_USER_LOGINS_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_CLAIM_USER_ID",
                table: "SEC_USER_CLAIMS",
                newName: "IX_SEC_USER_CLAIMS_USER_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_SEC_USER_NORMALIZED_USER_NAME",
                table: "SEC_USERS",
                newName: "UQ_SEC_USERS_NORMALIZED_USER_NAME");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_NORMALIZED_EMAIL",
                table: "SEC_USERS",
                newName: "IX_SEC_USERS_NORMALIZED_EMAIL");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_ROLE_CLAIM_ROLE_ID",
                table: "SEC_ROLE_CLAIMS",
                newName: "IX_SEC_ROLE_CLAIMS_ROLE_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_SEC_ROLE_NORMALIZED_ROLE_NAME",
                table: "SEC_ROLES",
                newName: "UQ_SEC_ROLES_NORMALIZED_ROLE_NAME");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATION_VALUATION_TYPE_CODE",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_VALUATION_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATION_PROJECT_UNIT_ID",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_PROJECT_UNIT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATION_PROJECT_ID",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_PROJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATION_PORTFOLIO_ID",
                table: "MTG_VALUATIONS",
                newName: "IX_MTG_VALUATIONS_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PROJECT_UNIT_PROJECT_ID_UNIT_CODE",
                table: "MTG_PROJECT_UNITS",
                newName: "UQ_MTG_PROJECT_UNITS_PROJECT_ID_UNIT_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PROJECT_UNIT_UNIT_TYPE_CODE",
                table: "MTG_PROJECT_UNITS",
                newName: "IX_MTG_PROJECT_UNITS_UNIT_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PROJECT_PROJECT_CODE",
                table: "MTG_PROJECTS",
                newName: "UQ_MTG_PROJECTS_PROJECT_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PROJECT_DEVELOPER_ID",
                table: "MTG_PROJECTS",
                newName: "IX_MTG_PROJECTS_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_STATUS_HISTORY_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "IX_MTG_PP_STATUS_HIST_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PORTFOLIO_IMPORT_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PORTFOLIO_FINANCIAL_REVIEW_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "UQ_MTG_PP_FINANCIAL_REVIEWS_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ID",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                newName: "IX_MTG_PP_CHEQUES_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                newName: "IX_MTG_PP_CHEQUES_PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_CHEQUE_CHEQUE_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                newName: "IX_MTG_PP_CHEQUES_CHEQUE_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_PROJECT_UNIT_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "IX_MTG_PP_ACCOUNTS_PROJECT_UNIT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_ACCOUNT_INVESTOR_ID",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "IX_MTG_PP_ACCOUNTS_INVESTOR_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PORTFOLIO_PORTFOLIO_NO",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "UQ_MTG_PURCHASED_PORTFOLIOS_PORTFOLIO_NO");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_PROJECT_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "IX_MTG_PURCHASED_PORTFOLIOS_PROJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_PORTFOLIO_STATUS_CODE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "IX_MTG_PURCHASED_PORTFOLIOS_PORTFOLIO_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_DEVELOPER_ID",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "IX_MTG_PURCHASED_PORTFOLIOS_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PORTFOLIO_CURRENCY_CODE",
                table: "MTG_PURCHASED_PORTFOLIOS",
                newName: "IX_MTG_PURCHASED_PORTFOLIOS_CURRENCY_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_LEGAL_REVIEW_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEWS",
                newName: "UQ_MTG_LEGAL_REVIEWS_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_INVESTOR_INVESTOR_CODE",
                table: "MTG_INVESTORS",
                newName: "UQ_MTG_INVESTORS_INVESTOR_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_INQUIRY_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_INQUIRY_RESULTS",
                newName: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_INQUIRY_RESULT_INQUIRY_TYPE_CODE",
                table: "MTG_INQUIRY_RESULTS",
                newName: "IX_MTG_INQUIRY_RESULTS_INQUIRY_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_DEVELOPER_SIGNATORY_DEVELOPER_ID",
                table: "MTG_DEVELOPER_SIGNATORIES",
                newName: "IX_MTG_DEVELOPER_SIGNATORIES_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_DEVELOPER_DEVELOPER_CODE",
                table: "MTG_DEVELOPERS",
                newName: "UQ_MTG_DEVELOPERS_DEVELOPER_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_DEVELOPER_RISK_LEVEL_CODE",
                table: "MTG_DEVELOPERS",
                newName: "IX_MTG_DEVELOPERS_RISK_LEVEL_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_AML_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_AML_RESULTS",
                newName: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_ACCOUNT_PAYMENT_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_ACCOUNT_PAYMENTS",
                newName: "IX_MTG_ACCOUNT_PAYMENTS_PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_ACCOUNT_PAYMENT_PAYMENT_STATUS_CODE",
                table: "MTG_ACCOUNT_PAYMENTS",
                newName: "IX_MTG_ACCOUNT_PAYMENTS_PAYMENT_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_ADM_LKP_DOCUMENT_TYPE_DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPES",
                newName: "IX_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_GROUP_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_TOKENS",
                table: "SEC_USER_TOKENS",
                columns: new[] { "USER_ID", "LOGIN_PROVIDER", "TOKEN_NAME" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_ROLES",
                table: "SEC_USER_ROLES",
                columns: new[] { "USER_ID", "ROLE_ID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_PASSKEYS",
                table: "SEC_USER_PASSKEYS",
                column: "CREDENTIAL_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_LOGINS",
                table: "SEC_USER_LOGINS",
                columns: new[] { "LOGIN_PROVIDER", "PROVIDER_KEY" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_CLAIMS",
                table: "SEC_USER_CLAIMS",
                column: "USER_CLAIM_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USERS",
                table: "SEC_USERS",
                column: "USER_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_ROLE_CLAIMS",
                table: "SEC_ROLE_CLAIMS",
                column: "ROLE_CLAIM_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_ROLES",
                table: "SEC_ROLES",
                column: "ROLE_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_VALUATIONS",
                table: "MTG_VALUATIONS",
                column: "VALUATION_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PROJECT_UNITS",
                table: "MTG_PROJECT_UNITS",
                column: "PROJECT_UNIT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PROJECTS",
                table: "MTG_PROJECTS",
                column: "PROJECT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "PORTFOLIO_STATUS_HISTORY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_IMPORTS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                column: "PORTFOLIO_IMPORT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                column: "FINANCIAL_REVIEW_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_CHEQUES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "CHEQUE_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                column: "PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "PORTFOLIO_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_VALUATION_TYPES",
                table: "MTG_LKP_VALUATION_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_UNIT_TYPES",
                table: "MTG_LKP_UNIT_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_PORTFOLIO_STATUSES",
                table: "MTG_LKP_PORTFOLIO_STATUSES",
                column: "CODE");

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
                name: "PK_MTG_LEGAL_REVIEWS",
                table: "MTG_LEGAL_REVIEWS",
                column: "LEGAL_REVIEW_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_INVESTORS",
                table: "MTG_INVESTORS",
                column: "INVESTOR_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_INQUIRY_RESULTS",
                table: "MTG_INQUIRY_RESULTS",
                column: "INQUIRY_RESULT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_DEVELOPER_SIGNATORIES",
                table: "MTG_DEVELOPER_SIGNATORIES",
                column: "DEVELOPER_SIGNATORY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_DEVELOPERS",
                table: "MTG_DEVELOPERS",
                column: "DEVELOPER_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_AML_RESULTS",
                table: "MTG_AML_RESULTS",
                column: "AML_RESULT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_ACCOUNT_PAYMENTS",
                table: "MTG_ACCOUNT_PAYMENTS",
                column: "ACCOUNT_PAYMENT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_INSURANCE_TYPES",
                table: "ADM_LKP_INSURANCE_TYPES",
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
                name: "PK_ADM_LKP_DOCUMENT_GROUPS",
                table: "ADM_LKP_DOCUMENT_GROUPS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DECISION_TYPES",
                table: "ADM_LKP_DECISION_TYPES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CURRENCIES",
                table: "ADM_LKP_CURRENCIES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_COUNTRIES",
                table: "ADM_LKP_COUNTRIES",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES",
                column: "CODE");

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
                name: "FK_MTG_ACCOUNT_PAYMENTS_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_ACCOUNT_PAYMENTS",
                column: "PORTFOLIO_ACCOUNT_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                principalColumn: "PORTFOLIO_ACCOUNT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_DEVELOPER_SIGNATORIES_MTG_DEVELOPERS",
                table: "MTG_DEVELOPER_SIGNATORIES",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPERS",
                principalColumn: "DEVELOPER_ID",
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
                name: "FK_MTG_INQUIRY_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_INQUIRY_RESULTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_LEGAL_REVIEWS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_LEGAL_REVIEWS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECT_UNITS_MTG_LKP_UNIT_TYPES",
                table: "MTG_PROJECT_UNITS",
                column: "UNIT_TYPE_CODE",
                principalTable: "MTG_LKP_UNIT_TYPES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECT_UNITS_MTG_PROJECTS",
                table: "MTG_PROJECT_UNITS",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECTS",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECTS_MTG_DEVELOPERS",
                table: "MTG_PROJECTS",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPERS",
                principalColumn: "DEVELOPER_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_INVESTORS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                column: "INVESTOR_ID",
                principalTable: "MTG_INVESTORS",
                principalColumn: "INVESTOR_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_PROJECT_UNITS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                column: "PROJECT_UNIT_ID",
                principalTable: "MTG_PROJECT_UNITS",
                principalColumn: "PROJECT_UNIT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "CHEQUE_STATUS_CODE",
                principalTable: "ADM_LKP_CHEQUE_STATUSES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_CHEQUES_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_CHEQUES_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                column: "PORTFOLIO_ACCOUNT_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                principalColumn: "PORTFOLIO_ACCOUNT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_FINANCIAL_REVIEWS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_ADM_LKP_CURRENCIES",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "CURRENCY_CODE",
                principalTable: "ADM_LKP_CURRENCIES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_DEVELOPERS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPERS",
                principalColumn: "DEVELOPER_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_LKP_PORTFOLIO_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "PORTFOLIO_STATUS_CODE",
                principalTable: "MTG_LKP_PORTFOLIO_STATUSES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_PROJECTS",
                table: "MTG_PURCHASED_PORTFOLIOS",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECTS",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_LKP_VALUATION_TYPES",
                table: "MTG_VALUATIONS",
                column: "VALUATION_TYPE_CODE",
                principalTable: "MTG_LKP_VALUATION_TYPES",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PROJECTS",
                table: "MTG_VALUATIONS",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECTS",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PROJECT_UNITS",
                table: "MTG_VALUATIONS",
                column: "PROJECT_UNIT_ID",
                principalTable: "MTG_PROJECT_UNITS",
                principalColumn: "PROJECT_UNIT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_VALUATIONS",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PURCHASED_PORTFOLIOS",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_ROLE_CLAIMS_SEC_ROLES",
                table: "SEC_ROLE_CLAIMS",
                column: "ROLE_ID",
                principalTable: "SEC_ROLES",
                principalColumn: "ROLE_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_CLAIMS_SEC_USERS",
                table: "SEC_USER_CLAIMS",
                column: "USER_ID",
                principalTable: "SEC_USERS",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_LOGINS_SEC_USERS",
                table: "SEC_USER_LOGINS",
                column: "USER_ID",
                principalTable: "SEC_USERS",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_PASSKEYS_SEC_USERS",
                table: "SEC_USER_PASSKEYS",
                column: "USER_ID",
                principalTable: "SEC_USERS",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_ROLES_SEC_ROLES",
                table: "SEC_USER_ROLES",
                column: "ROLE_ID",
                principalTable: "SEC_ROLES",
                principalColumn: "ROLE_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_ROLES_SEC_USERS",
                table: "SEC_USER_ROLES",
                column: "USER_ID",
                principalTable: "SEC_USERS",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_TOKENS_SEC_USERS",
                table: "SEC_USER_TOKENS",
                column: "USER_ID",
                principalTable: "SEC_USERS",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ADM_LKP_DOCUMENT_TYPES_ADM_LKP_DOCUMENT_GROUPS",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_ADM_LKP_PAYMENT_STATUSES",
                table: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENTS_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_AML_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_AML_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPER_SIGNATORIES_MTG_DEVELOPERS",
                table: "MTG_DEVELOPER_SIGNATORIES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_DEVELOPERS_ADM_LKP_RISK_LEVELS",
                table: "MTG_DEVELOPERS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_INQUIRY_RESULTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_LEGAL_REVIEWS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_LEGAL_REVIEWS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECT_UNITS_MTG_LKP_UNIT_TYPES",
                table: "MTG_PROJECT_UNITS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECT_UNITS_MTG_PROJECTS",
                table: "MTG_PROJECT_UNITS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PROJECTS_MTG_DEVELOPERS",
                table: "MTG_PROJECTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_INVESTORS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_PROJECT_UNITS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_ACCOUNTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_CHEQUES_ADM_LKP_CHEQUE_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_CHEQUES_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_CHEQUES_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_FINANCIAL_REVIEWS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_IMPORTS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PP_STATUS_HIST_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_ADM_LKP_CURRENCIES",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_DEVELOPERS",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_LKP_PORTFOLIO_STATUSES",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_PURCHASED_PORTFOLIOS_MTG_PROJECTS",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_LKP_VALUATION_TYPES",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PROJECTS",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PROJECT_UNITS",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_MTG_VALUATIONS_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_ROLE_CLAIMS_SEC_ROLES",
                table: "SEC_ROLE_CLAIMS");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_CLAIMS_SEC_USERS",
                table: "SEC_USER_CLAIMS");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_LOGINS_SEC_USERS",
                table: "SEC_USER_LOGINS");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_PASSKEYS_SEC_USERS",
                table: "SEC_USER_PASSKEYS");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_ROLES_SEC_ROLES",
                table: "SEC_USER_ROLES");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_ROLES_SEC_USERS",
                table: "SEC_USER_ROLES");

            migrationBuilder.DropForeignKey(
                name: "FK_SEC_USER_TOKENS_SEC_USERS",
                table: "SEC_USER_TOKENS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USERS",
                table: "SEC_USERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_TOKENS",
                table: "SEC_USER_TOKENS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_ROLES",
                table: "SEC_USER_ROLES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_PASSKEYS",
                table: "SEC_USER_PASSKEYS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_LOGINS",
                table: "SEC_USER_LOGINS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_USER_CLAIMS",
                table: "SEC_USER_CLAIMS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_ROLES",
                table: "SEC_ROLES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SEC_ROLE_CLAIMS",
                table: "SEC_ROLE_CLAIMS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_VALUATIONS",
                table: "MTG_VALUATIONS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIOS",
                table: "MTG_PURCHASED_PORTFOLIOS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                table: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_IMPORTS",
                table: "MTG_PURCHASED_PORTFOLIO_IMPORTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                table: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_CHEQUES",
                table: "MTG_PURCHASED_PORTFOLIO_CHEQUES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                table: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PROJECTS",
                table: "MTG_PROJECTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_PROJECT_UNITS",
                table: "MTG_PROJECT_UNITS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_VALUATION_TYPES",
                table: "MTG_LKP_VALUATION_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_UNIT_TYPES",
                table: "MTG_LKP_UNIT_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_PORTFOLIO_STATUSES",
                table: "MTG_LKP_PORTFOLIO_STATUSES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPES",
                table: "MTG_LKP_INQUIRY_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPES",
                table: "MTG_LKP_DD_CHECK_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEPS",
                table: "MTG_LKP_APPROVAL_STEPS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_LEGAL_REVIEWS",
                table: "MTG_LEGAL_REVIEWS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_INVESTORS",
                table: "MTG_INVESTORS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_INQUIRY_RESULTS",
                table: "MTG_INQUIRY_RESULTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_DEVELOPERS",
                table: "MTG_DEVELOPERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_DEVELOPER_SIGNATORIES",
                table: "MTG_DEVELOPER_SIGNATORIES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_AML_RESULTS",
                table: "MTG_AML_RESULTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MTG_ACCOUNT_PAYMENTS",
                table: "MTG_ACCOUNT_PAYMENTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVELS",
                table: "ADM_LKP_RISK_LEVELS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUSES",
                table: "ADM_LKP_PAYMENT_STATUSES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_INSURANCE_TYPES",
                table: "ADM_LKP_INSURANCE_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPES",
                table: "ADM_LKP_GUARANTEE_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPES",
                table: "ADM_LKP_DOCUMENT_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_GROUPS",
                table: "ADM_LKP_DOCUMENT_GROUPS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_DECISION_TYPES",
                table: "ADM_LKP_DECISION_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CURRENCIES",
                table: "ADM_LKP_CURRENCIES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_COUNTRIES",
                table: "ADM_LKP_COUNTRIES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUSES",
                table: "ADM_LKP_CHEQUE_STATUSES");

            migrationBuilder.RenameTable(
                name: "SEC_USERS",
                newName: "SEC_USER");

            migrationBuilder.RenameTable(
                name: "SEC_USER_TOKENS",
                newName: "SEC_USER_TOKEN");

            migrationBuilder.RenameTable(
                name: "SEC_USER_ROLES",
                newName: "SEC_USER_ROLE");

            migrationBuilder.RenameTable(
                name: "SEC_USER_PASSKEYS",
                newName: "SEC_USER_PASSKEY");

            migrationBuilder.RenameTable(
                name: "SEC_USER_LOGINS",
                newName: "SEC_USER_LOGIN");

            migrationBuilder.RenameTable(
                name: "SEC_USER_CLAIMS",
                newName: "SEC_USER_CLAIM");

            migrationBuilder.RenameTable(
                name: "SEC_ROLES",
                newName: "SEC_ROLE");

            migrationBuilder.RenameTable(
                name: "SEC_ROLE_CLAIMS",
                newName: "SEC_ROLE_CLAIM");

            migrationBuilder.RenameTable(
                name: "MTG_VALUATIONS",
                newName: "MTG_VALUATION");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIOS",
                newName: "MTG_PORTFOLIO");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIO_STATUS_HIST",
                newName: "MTG_PORTFOLIO_STATUS_HISTORY");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIO_IMPORTS",
                newName: "MTG_PORTFOLIO_IMPORT");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS",
                newName: "MTG_PORTFOLIO_FINANCIAL_REVIEW");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIO_CHEQUES",
                newName: "MTG_PORTFOLIO_CHEQUE");

            migrationBuilder.RenameTable(
                name: "MTG_PURCHASED_PORTFOLIO_ACCOUNTS",
                newName: "MTG_PORTFOLIO_ACCOUNT");

            migrationBuilder.RenameTable(
                name: "MTG_PROJECTS",
                newName: "MTG_PROJECT");

            migrationBuilder.RenameTable(
                name: "MTG_PROJECT_UNITS",
                newName: "MTG_PROJECT_UNIT");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_VALUATION_TYPES",
                newName: "MTG_LKP_VALUATION_TYPE");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_UNIT_TYPES",
                newName: "MTG_LKP_UNIT_TYPE");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_PORTFOLIO_STATUSES",
                newName: "MTG_LKP_PORTFOLIO_STATUS");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_INQUIRY_TYPES",
                newName: "MTG_LKP_INQUIRY_TYPE");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_DD_CHECK_TYPES",
                newName: "MTG_LKP_DD_CHECK_TYPE");

            migrationBuilder.RenameTable(
                name: "MTG_LKP_APPROVAL_STEPS",
                newName: "MTG_LKP_APPROVAL_STEP");

            migrationBuilder.RenameTable(
                name: "MTG_LEGAL_REVIEWS",
                newName: "MTG_LEGAL_REVIEW");

            migrationBuilder.RenameTable(
                name: "MTG_INVESTORS",
                newName: "MTG_INVESTOR");

            migrationBuilder.RenameTable(
                name: "MTG_INQUIRY_RESULTS",
                newName: "MTG_INQUIRY_RESULT");

            migrationBuilder.RenameTable(
                name: "MTG_DEVELOPERS",
                newName: "MTG_DEVELOPER");

            migrationBuilder.RenameTable(
                name: "MTG_DEVELOPER_SIGNATORIES",
                newName: "MTG_DEVELOPER_SIGNATORY");

            migrationBuilder.RenameTable(
                name: "MTG_AML_RESULTS",
                newName: "MTG_AML_RESULT");

            migrationBuilder.RenameTable(
                name: "MTG_ACCOUNT_PAYMENTS",
                newName: "MTG_ACCOUNT_PAYMENT");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_RISK_LEVELS",
                newName: "ADM_LKP_RISK_LEVEL");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_PAYMENT_STATUSES",
                newName: "ADM_LKP_PAYMENT_STATUS");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_INSURANCE_TYPES",
                newName: "ADM_LKP_INSURANCE_TYPE");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_GUARANTEE_TYPES",
                newName: "ADM_LKP_GUARANTEE_TYPE");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DOCUMENT_TYPES",
                newName: "ADM_LKP_DOCUMENT_TYPE");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DOCUMENT_GROUPS",
                newName: "ADM_LKP_DOCUMENT_GROUP");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_DECISION_TYPES",
                newName: "ADM_LKP_DECISION_TYPE");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_CURRENCIES",
                newName: "ADM_LKP_CURRENCY");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_COUNTRIES",
                newName: "ADM_LKP_COUNTRY");

            migrationBuilder.RenameTable(
                name: "ADM_LKP_CHEQUE_STATUSES",
                newName: "ADM_LKP_CHEQUE_STATUS");

            migrationBuilder.RenameIndex(
                name: "UQ_SEC_USERS_NORMALIZED_USER_NAME",
                table: "SEC_USER",
                newName: "UQ_SEC_USER_NORMALIZED_USER_NAME");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USERS_NORMALIZED_EMAIL",
                table: "SEC_USER",
                newName: "IX_SEC_USER_NORMALIZED_EMAIL");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_ROLES_ROLE_ID",
                table: "SEC_USER_ROLE",
                newName: "IX_SEC_USER_ROLE_ROLE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_PASSKEYS_USER_ID",
                table: "SEC_USER_PASSKEY",
                newName: "IX_SEC_USER_PASSKEY_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_LOGINS_USER_ID",
                table: "SEC_USER_LOGIN",
                newName: "IX_SEC_USER_LOGIN_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_USER_CLAIMS_USER_ID",
                table: "SEC_USER_CLAIM",
                newName: "IX_SEC_USER_CLAIM_USER_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_SEC_ROLES_NORMALIZED_ROLE_NAME",
                table: "SEC_ROLE",
                newName: "UQ_SEC_ROLE_NORMALIZED_ROLE_NAME");

            migrationBuilder.RenameIndex(
                name: "IX_SEC_ROLE_CLAIMS_ROLE_ID",
                table: "SEC_ROLE_CLAIM",
                newName: "IX_SEC_ROLE_CLAIM_ROLE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_VALUATION_TYPE_CODE",
                table: "MTG_VALUATION",
                newName: "IX_MTG_VALUATION_VALUATION_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_PROJECT_UNIT_ID",
                table: "MTG_VALUATION",
                newName: "IX_MTG_VALUATION_PROJECT_UNIT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_PROJECT_ID",
                table: "MTG_VALUATION",
                newName: "IX_MTG_VALUATION_PROJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_VALUATIONS_PORTFOLIO_ID",
                table: "MTG_VALUATION",
                newName: "IX_MTG_VALUATION_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PURCHASED_PORTFOLIOS_PORTFOLIO_NO",
                table: "MTG_PORTFOLIO",
                newName: "UQ_MTG_PORTFOLIO_PORTFOLIO_NO");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_PROJECT_ID",
                table: "MTG_PORTFOLIO",
                newName: "IX_MTG_PORTFOLIO_PROJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_PORTFOLIO_STATUS_CODE",
                table: "MTG_PORTFOLIO",
                newName: "IX_MTG_PORTFOLIO_PORTFOLIO_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_DEVELOPER_ID",
                table: "MTG_PORTFOLIO",
                newName: "IX_MTG_PORTFOLIO_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PURCHASED_PORTFOLIOS_CURRENCY_CODE",
                table: "MTG_PORTFOLIO",
                newName: "IX_MTG_PORTFOLIO_CURRENCY_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_STATUS_HIST_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_STATUS_HISTORY",
                newName: "IX_MTG_PORTFOLIO_STATUS_HISTORY_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PP_IMPORTS_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_IMPORT",
                newName: "UQ_MTG_PORTFOLIO_IMPORT_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PP_FINANCIAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                newName: "UQ_MTG_PORTFOLIO_FINANCIAL_REVIEW_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_CHEQUES_PORTFOLIO_ID",
                table: "MTG_PORTFOLIO_CHEQUE",
                newName: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_CHEQUES_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_PORTFOLIO_CHEQUE",
                newName: "IX_MTG_PORTFOLIO_CHEQUE_PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_CHEQUES_CHEQUE_STATUS_CODE",
                table: "MTG_PORTFOLIO_CHEQUE",
                newName: "IX_MTG_PORTFOLIO_CHEQUE_CHEQUE_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PP_ACCOUNTS_PORTFOLIO_ID_SELLER_CONTRACT_NO",
                table: "MTG_PORTFOLIO_ACCOUNT",
                newName: "UQ_MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ID_SELLER_CONTRACT_NO");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_ACCOUNTS_PROJECT_UNIT_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                newName: "IX_MTG_PORTFOLIO_ACCOUNT_PROJECT_UNIT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PP_ACCOUNTS_INVESTOR_ID",
                table: "MTG_PORTFOLIO_ACCOUNT",
                newName: "IX_MTG_PORTFOLIO_ACCOUNT_INVESTOR_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PROJECTS_PROJECT_CODE",
                table: "MTG_PROJECT",
                newName: "UQ_MTG_PROJECT_PROJECT_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PROJECTS_DEVELOPER_ID",
                table: "MTG_PROJECT",
                newName: "IX_MTG_PROJECT_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_PROJECT_UNITS_PROJECT_ID_UNIT_CODE",
                table: "MTG_PROJECT_UNIT",
                newName: "UQ_MTG_PROJECT_UNIT_PROJECT_ID_UNIT_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_PROJECT_UNITS_UNIT_TYPE_CODE",
                table: "MTG_PROJECT_UNIT",
                newName: "IX_MTG_PROJECT_UNIT_UNIT_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_LEGAL_REVIEWS_PORTFOLIO_ID",
                table: "MTG_LEGAL_REVIEW",
                newName: "UQ_MTG_LEGAL_REVIEW_PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_INVESTORS_INVESTOR_CODE",
                table: "MTG_INVESTOR",
                newName: "UQ_MTG_INVESTOR_INVESTOR_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_INQUIRY_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE",
                table: "MTG_INQUIRY_RESULT",
                newName: "IX_MTG_INQUIRY_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_INQUIRY_RESULTS_INQUIRY_TYPE_CODE",
                table: "MTG_INQUIRY_RESULT",
                newName: "IX_MTG_INQUIRY_RESULT_INQUIRY_TYPE_CODE");

            migrationBuilder.RenameIndex(
                name: "UQ_MTG_DEVELOPERS_DEVELOPER_CODE",
                table: "MTG_DEVELOPER",
                newName: "UQ_MTG_DEVELOPER_DEVELOPER_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_DEVELOPERS_RISK_LEVEL_CODE",
                table: "MTG_DEVELOPER",
                newName: "IX_MTG_DEVELOPER_RISK_LEVEL_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_DEVELOPER_SIGNATORIES_DEVELOPER_ID",
                table: "MTG_DEVELOPER_SIGNATORY",
                newName: "IX_MTG_DEVELOPER_SIGNATORY_DEVELOPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_AML_RESULTS_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID",
                table: "MTG_AML_RESULT",
                newName: "IX_MTG_AML_RESULT_PORTFOLIO_ID_SUBJECT_TYPE_CODE_SUBJECT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_ACCOUNT_PAYMENTS_PORTFOLIO_ACCOUNT_ID",
                table: "MTG_ACCOUNT_PAYMENT",
                newName: "IX_MTG_ACCOUNT_PAYMENT_PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MTG_ACCOUNT_PAYMENTS_PAYMENT_STATUS_CODE",
                table: "MTG_ACCOUNT_PAYMENT",
                newName: "IX_MTG_ACCOUNT_PAYMENT_PAYMENT_STATUS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_ADM_LKP_DOCUMENT_TYPES_DOCUMENT_GROUP_CODE",
                table: "ADM_LKP_DOCUMENT_TYPE",
                newName: "IX_ADM_LKP_DOCUMENT_TYPE_DOCUMENT_GROUP_CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER",
                table: "SEC_USER",
                column: "USER_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_TOKEN",
                table: "SEC_USER_TOKEN",
                columns: new[] { "USER_ID", "LOGIN_PROVIDER", "TOKEN_NAME" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_ROLE",
                table: "SEC_USER_ROLE",
                columns: new[] { "USER_ID", "ROLE_ID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_PASSKEY",
                table: "SEC_USER_PASSKEY",
                column: "CREDENTIAL_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_LOGIN",
                table: "SEC_USER_LOGIN",
                columns: new[] { "LOGIN_PROVIDER", "PROVIDER_KEY" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_USER_CLAIM",
                table: "SEC_USER_CLAIM",
                column: "USER_CLAIM_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_ROLE",
                table: "SEC_ROLE",
                column: "ROLE_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SEC_ROLE_CLAIM",
                table: "SEC_ROLE_CLAIM",
                column: "ROLE_CLAIM_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_VALUATION",
                table: "MTG_VALUATION",
                column: "VALUATION_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO",
                column: "PORTFOLIO_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO_STATUS_HISTORY",
                table: "MTG_PORTFOLIO_STATUS_HISTORY",
                column: "PORTFOLIO_STATUS_HISTORY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO_IMPORT",
                table: "MTG_PORTFOLIO_IMPORT",
                column: "PORTFOLIO_IMPORT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO_FINANCIAL_REVIEW",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                column: "FINANCIAL_REVIEW_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO_CHEQUE",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "CHEQUE_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PORTFOLIO_ACCOUNT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PROJECT",
                table: "MTG_PROJECT",
                column: "PROJECT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_PROJECT_UNIT",
                table: "MTG_PROJECT_UNIT",
                column: "PROJECT_UNIT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_VALUATION_TYPE",
                table: "MTG_LKP_VALUATION_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_UNIT_TYPE",
                table: "MTG_LKP_UNIT_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_PORTFOLIO_STATUS",
                table: "MTG_LKP_PORTFOLIO_STATUS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_INQUIRY_TYPE",
                table: "MTG_LKP_INQUIRY_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_DD_CHECK_TYPE",
                table: "MTG_LKP_DD_CHECK_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LKP_APPROVAL_STEP",
                table: "MTG_LKP_APPROVAL_STEP",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_LEGAL_REVIEW",
                table: "MTG_LEGAL_REVIEW",
                column: "LEGAL_REVIEW_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_INVESTOR",
                table: "MTG_INVESTOR",
                column: "INVESTOR_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_INQUIRY_RESULT",
                table: "MTG_INQUIRY_RESULT",
                column: "INQUIRY_RESULT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_DEVELOPER",
                table: "MTG_DEVELOPER",
                column: "DEVELOPER_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_DEVELOPER_SIGNATORY",
                table: "MTG_DEVELOPER_SIGNATORY",
                column: "DEVELOPER_SIGNATORY_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_AML_RESULT",
                table: "MTG_AML_RESULT",
                column: "AML_RESULT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MTG_ACCOUNT_PAYMENT",
                table: "MTG_ACCOUNT_PAYMENT",
                column: "ACCOUNT_PAYMENT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_RISK_LEVEL",
                table: "ADM_LKP_RISK_LEVEL",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_PAYMENT_STATUS",
                table: "ADM_LKP_PAYMENT_STATUS",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_INSURANCE_TYPE",
                table: "ADM_LKP_INSURANCE_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_GUARANTEE_TYPE",
                table: "ADM_LKP_GUARANTEE_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_TYPE",
                table: "ADM_LKP_DOCUMENT_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DOCUMENT_GROUP",
                table: "ADM_LKP_DOCUMENT_GROUP",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_DECISION_TYPE",
                table: "ADM_LKP_DECISION_TYPE",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CURRENCY",
                table: "ADM_LKP_CURRENCY",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_COUNTRY",
                table: "ADM_LKP_COUNTRY",
                column: "CODE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ADM_LKP_CHEQUE_STATUS",
                table: "ADM_LKP_CHEQUE_STATUS",
                column: "CODE");

            migrationBuilder.AddForeignKey(
                name: "FK_ADM_LKP_DOCUMENT_TYPE_ADM_LKP_DOCUMENT_GROUP",
                table: "ADM_LKP_DOCUMENT_TYPE",
                column: "DOCUMENT_GROUP_CODE",
                principalTable: "ADM_LKP_DOCUMENT_GROUP",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENT_ADM_LKP_PAYMENT_STATUS",
                table: "MTG_ACCOUNT_PAYMENT",
                column: "PAYMENT_STATUS_CODE",
                principalTable: "ADM_LKP_PAYMENT_STATUS",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_ACCOUNT_PAYMENT_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_ACCOUNT_PAYMENT",
                column: "PORTFOLIO_ACCOUNT_ID",
                principalTable: "MTG_PORTFOLIO_ACCOUNT",
                principalColumn: "PORTFOLIO_ACCOUNT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_AML_RESULT_MTG_PORTFOLIO",
                table: "MTG_AML_RESULT",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_DEVELOPER_ADM_LKP_RISK_LEVEL",
                table: "MTG_DEVELOPER",
                column: "RISK_LEVEL_CODE",
                principalTable: "ADM_LKP_RISK_LEVEL",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_DEVELOPER_SIGNATORY_MTG_DEVELOPER",
                table: "MTG_DEVELOPER_SIGNATORY",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPER",
                principalColumn: "DEVELOPER_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_INQUIRY_RESULT_MTG_LKP_INQUIRY_TYPE",
                table: "MTG_INQUIRY_RESULT",
                column: "INQUIRY_TYPE_CODE",
                principalTable: "MTG_LKP_INQUIRY_TYPE",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_INQUIRY_RESULT_MTG_PORTFOLIO",
                table: "MTG_INQUIRY_RESULT",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_LEGAL_REVIEW_MTG_PORTFOLIO",
                table: "MTG_LEGAL_REVIEW",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_ADM_LKP_CURRENCY",
                table: "MTG_PORTFOLIO",
                column: "CURRENCY_CODE",
                principalTable: "ADM_LKP_CURRENCY",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_DEVELOPER",
                table: "MTG_PORTFOLIO",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPER",
                principalColumn: "DEVELOPER_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_LKP_PORTFOLIO_STATUS",
                table: "MTG_PORTFOLIO",
                column: "PORTFOLIO_STATUS_CODE",
                principalTable: "MTG_LKP_PORTFOLIO_STATUS",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_MTG_PROJECT",
                table: "MTG_PORTFOLIO",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECT",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_INVESTOR",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "INVESTOR_ID",
                principalTable: "MTG_INVESTOR",
                principalColumn: "INVESTOR_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_ACCOUNT_MTG_PROJECT_UNIT",
                table: "MTG_PORTFOLIO_ACCOUNT",
                column: "PROJECT_UNIT_ID",
                principalTable: "MTG_PROJECT_UNIT",
                principalColumn: "PROJECT_UNIT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_ADM_LKP_CHEQUE_STATUS",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "CHEQUE_STATUS_CODE",
                principalTable: "ADM_LKP_CHEQUE_STATUS",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_CHEQUE_MTG_PORTFOLIO_ACCOUNT",
                table: "MTG_PORTFOLIO_CHEQUE",
                column: "PORTFOLIO_ACCOUNT_ID",
                principalTable: "MTG_PORTFOLIO_ACCOUNT",
                principalColumn: "PORTFOLIO_ACCOUNT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_FINANCIAL_REVIEW_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_FINANCIAL_REVIEW",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_IMPORT_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_IMPORT",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PORTFOLIO_STATUS_HISTORY_MTG_PORTFOLIO",
                table: "MTG_PORTFOLIO_STATUS_HISTORY",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECT_MTG_DEVELOPER",
                table: "MTG_PROJECT",
                column: "DEVELOPER_ID",
                principalTable: "MTG_DEVELOPER",
                principalColumn: "DEVELOPER_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECT_UNIT_MTG_LKP_UNIT_TYPE",
                table: "MTG_PROJECT_UNIT",
                column: "UNIT_TYPE_CODE",
                principalTable: "MTG_LKP_UNIT_TYPE",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_PROJECT_UNIT_MTG_PROJECT",
                table: "MTG_PROJECT_UNIT",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECT",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATION_MTG_LKP_VALUATION_TYPE",
                table: "MTG_VALUATION",
                column: "VALUATION_TYPE_CODE",
                principalTable: "MTG_LKP_VALUATION_TYPE",
                principalColumn: "CODE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATION_MTG_PORTFOLIO",
                table: "MTG_VALUATION",
                column: "PORTFOLIO_ID",
                principalTable: "MTG_PORTFOLIO",
                principalColumn: "PORTFOLIO_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATION_MTG_PROJECT",
                table: "MTG_VALUATION",
                column: "PROJECT_ID",
                principalTable: "MTG_PROJECT",
                principalColumn: "PROJECT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTG_VALUATION_MTG_PROJECT_UNIT",
                table: "MTG_VALUATION",
                column: "PROJECT_UNIT_ID",
                principalTable: "MTG_PROJECT_UNIT",
                principalColumn: "PROJECT_UNIT_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_ROLE_CLAIM_SEC_ROLE",
                table: "SEC_ROLE_CLAIM",
                column: "ROLE_ID",
                principalTable: "SEC_ROLE",
                principalColumn: "ROLE_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_CLAIM_SEC_USER",
                table: "SEC_USER_CLAIM",
                column: "USER_ID",
                principalTable: "SEC_USER",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_LOGIN_SEC_USER",
                table: "SEC_USER_LOGIN",
                column: "USER_ID",
                principalTable: "SEC_USER",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_PASSKEY_SEC_USER",
                table: "SEC_USER_PASSKEY",
                column: "USER_ID",
                principalTable: "SEC_USER",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_ROLE_SEC_ROLE",
                table: "SEC_USER_ROLE",
                column: "ROLE_ID",
                principalTable: "SEC_ROLE",
                principalColumn: "ROLE_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_ROLE_SEC_USER",
                table: "SEC_USER_ROLE",
                column: "USER_ID",
                principalTable: "SEC_USER",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SEC_USER_TOKEN_SEC_USER",
                table: "SEC_USER_TOKEN",
                column: "USER_ID",
                principalTable: "SEC_USER",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
