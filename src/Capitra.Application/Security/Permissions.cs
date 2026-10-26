namespace Capitra.Application.Security;

/// <summary>
/// Permission codes, {MODULE}.{AREA}.{ACTION} (naming standard, section 7).
/// Permissions are granted to roles as role claims of type <see cref="ClaimType"/>.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    /// <summary>Grants every permission. Given to the seeded administrator role only.</summary>
    public const string All = "*";

    public static class Adm
    {
        /// <summary>Create, change and delete lookup values. Reading lookups only needs a signed-in user.</summary>
        public const string LookupEdit = "ADM.LOOKUP.EDIT";
    }

    public static class Gl
    {
        /// <summary>Read account structures, segments, the chart of accounts, calendars and periods.</summary>
        public const string SetupView = "GL.SETUP.VIEW";

        /// <summary>Change account structures, segments, segment values and accounts.</summary>
        public const string SetupEdit = "GL.SETUP.EDIT";
    }

    public static class Mtg
    {
        /// <summary>Read developers, signatories, projects, units and investors.</summary>
        public const string MasterDataView = "MTG.MASTER_DATA.VIEW";

        /// <summary>Create, change and delete developers, signatories, projects, units and investors.</summary>
        public const string MasterDataEdit = "MTG.MASTER_DATA.EDIT";

        /// <summary>Read portfolio cases, their accounts and payment history.</summary>
        public const string PortfolioView = "MTG.PORTFOLIO.VIEW";

        /// <summary>Create, change and delete portfolio cases, accounts and payments.</summary>
        public const string PortfolioEdit = "MTG.PORTFOLIO.EDIT";

        /// <summary>Workflow step "Excel Import &amp; Validation" (page 208): validate and import the seller workbook.</summary>
        public const string StepExcelImport = "MTG.STEP.EXCEL_IMPORT";

        /// <summary>Workflow step "AML Review" (page 209): record checks and deal decisions, accept or reject.</summary>
        public const string StepAmlReview = "MTG.STEP.AML_REVIEW";

        /// <summary>Workflow step "Portfolio Inquiries" (page 210): record inquiries and deal decisions, complete or reject.</summary>
        public const string StepInquiries = "MTG.STEP.INQUIRIES";

        /// <summary>Workflow step "Portfolio Legal Review" (page 211): save the legal checklist and deal decisions, accept or reject.</summary>
        public const string StepLegalReview = "MTG.STEP.LEGAL_REVIEW";

        /// <summary>Workflow step "Portfolio Valuation" (page 401): record valuations and complete the step.</summary>
        public const string StepValuation = "MTG.STEP.VALUATION";

        /// <summary>Workflow step "Portfolio Coverage Review" (page 212): apply the coverage rule, decide investors, accept or reject.</summary>
        public const string StepCoverageReview = "MTG.STEP.COVERAGE_REVIEW";

        /// <summary>Workflow step "Portfolio Cheque Coverage Review" (page 213): enter investors' cheques, accept or reject.</summary>
        public const string StepChequeCoverage = "MTG.STEP.CHEQUE_COVERAGE";

        /// <summary>Workflow step "Portfolio Financial Review" (page 214): price the portfolio, accept or reject.</summary>
        public const string StepFinancialReview = "MTG.STEP.FINANCIAL_REVIEW";

        /// <summary>Workflow step "Portfolio Board Review" (page 215): record the board decision, accept or reject.</summary>
        public const string StepBoardReview = "MTG.STEP.BOARD_REVIEW";
    }
}
