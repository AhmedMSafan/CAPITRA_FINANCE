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

        /// <summary>Read products, charges and product event rules.</summary>
        public const string ProductView = "ADM.PRODUCT.VIEW";

        /// <summary>Create, change and delete products, charges and product event rules.</summary>
        public const string ProductEdit = "ADM.PRODUCT.EDIT";
    }

    public static class Sec
    {
        /// <summary>Create and change users, reset their passwords, create roles and grant roles their permissions.</summary>
        public const string UserAdmin = "SEC.USER.ADMIN";
    }

    public static class Wfe
    {
        /// <summary>Read workflow definitions in the BPMN designer.</summary>
        public const string DefinitionView = "WFE.DEFINITION.VIEW";

        /// <summary>Create, change, release and delete workflow definitions.</summary>
        public const string DefinitionEdit = "WFE.DEFINITION.EDIT";
    }

    public static class Gl
    {
        /// <summary>Read account structures, segments, the chart of accounts, calendars and periods.</summary>
        public const string SetupView = "GL.SETUP.VIEW";

        /// <summary>Change account structures, segments, segment values, accounts, ledgers, calendars and periods.</summary>
        public const string SetupEdit = "GL.SETUP.EDIT";

        /// <summary>Open, close, reopen and permanently close a ledger's periods.</summary>
        public const string PeriodControl = "GL.PERIOD.CONTROL";

        /// <summary>Find journals and read the account ledger and balances.</summary>
        public const string JournalView = "GL.JOURNAL.VIEW";

        /// <summary>Enter, submit, cancel and reverse manual journals.</summary>
        public const string JournalEdit = "GL.JOURNAL.EDIT";

        /// <summary>Approve or reject a journal another user submitted.</summary>
        public const string JournalApprove = "GL.JOURNAL.APPROVE";

        /// <summary>Post approved journals to the account balances.</summary>
        public const string JournalPost = "GL.JOURNAL.POST";
    }

    public static class Chq
    {
        /// <summary>Read the cheques wallet: cheques, upload batches, movements and the action catalog (App 122).</summary>
        public const string View = "CHQ.VIEW";

        /// <summary>Upload cheques, move them and apply their actions (App 122).</summary>
        public const string Edit = "CHQ.EDIT";
    }

    public static class Mff
    {
        /// <summary>Read the mortgage finance applications, loans, collections and reports (App 110).</summary>
        public const string View = "MFF.VIEW";

        /// <summary>Create and change mortgage finance applications, loans and their servicing (App 110).</summary>
        public const string Edit = "MFF.EDIT";
    }

    public static class Fct
    {
        /// <summary>Read the factoring customers, their applications, agreements and limits.</summary>
        public const string View = "FCT.VIEW";

        /// <summary>Create and change factoring applications and start their workflow.</summary>
        public const string Edit = "FCT.EDIT";
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

    /// <summary>Every permission code above, for the role screen.</summary>
    public static IReadOnlyList<string> Catalog { get; } =
        typeof(Permissions).GetNestedTypes()
            .SelectMany(t => t.GetFields())
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal)
            .ToList();
}
