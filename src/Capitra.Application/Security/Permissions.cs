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

        /// <summary>Approve or reject the lending terms of a product version (four eyes: never one's own version).</summary>
        public const string ProductApprove = "ADM.PRODUCT.APPROVE";

        /// <summary>Read the module settings (Administration > Module Settings).</summary>
        public const string SettingsView = "ADM.SETTINGS.VIEW";

        /// <summary>Ask for, approve, reject and withdraw module setting changes. A change always needs a second user's approval.</summary>
        public const string SettingsEdit = "ADM.SETTINGS.EDIT";
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

    public static class Crm
    {
        /// <summary>Read the CRM clients.</summary>
        public const string View = "CRM.VIEW";

        /// <summary>Create and change CRM clients.</summary>
        public const string Edit = "CRM.EDIT";

        /// <summary>Verify a customer's compliance profile and bank accounts (four eyes: never one's own change).</summary>
        public const string Verify = "CRM.VERIFY";
    }

    public static class Trs
    {
        /// <summary>Read the Treasury bank limits, tranches, sublimits and bank rules.</summary>
        public const string View = "TRS.VIEW";

        /// <summary>Create and change facilities, tranches, sublimits and bank rules.</summary>
        public const string Edit = "TRS.EDIT";

        /// <summary>Verify a bank rule (four eyes: never one's own change).</summary>
        public const string Verify = "TRS.VERIFY";
    }

    public static class Collection
    {
        /// <summary>Read the customer repayment transactions and the due factoring cheques (App 136).</summary>
        public const string View = "COLLECTION.VIEW";

        /// <summary>Record bank transfers and apply the bank's cheque results (App 136).</summary>
        public const string Edit = "COLLECTION.EDIT";
    }

    public static class Tls
    {
        /// <summary>Tools > Loan Calculator: calculate schedules and keep one's own saved scenarios.</summary>
        public const string Calculator = "TLS.CALCULATOR.USE";
    }

    public static class Lnd
    {
        /// <summary>Read the Retail Lending parameters, their history and the go-live check.</summary>
        public const string ParametersView = "LND.PARAMETERS.VIEW";

        /// <summary>Ask for a new parameter value, and withdraw one's own request.</summary>
        public const string ParametersEdit = "LND.PARAMETERS.EDIT";

        /// <summary>Approve or reject a parameter value (compliance; four eyes: never one's own request).</summary>
        public const string ParametersApprove = "LND.PARAMETERS.APPROVE";

        /// <summary>Read retail loans and their schedules.</summary>
        public const string LoanView = "LND.LOAN.VIEW";

        /// <summary>Create retail loans from a lending product's terms, and cancel one not yet paid out.</summary>
        public const string LoanEdit = "LND.LOAN.EDIT";

        /// <summary>Pay a loan out, or reject a payout (four eyes: never one's own request).</summary>
        public const string LoanDisburse = "LND.LOAN.DISBURSE";

        /// <summary>Receive a customer's payment on a loan (bank deposit, collected cheque or credit balance).</summary>
        public const string LoanReceive = "LND.LOAN.RECEIVE";

        /// <summary>Was: run Retail Lending end of day. End of Day is its own application now (<see cref="Eod"/>); roles that had it were given EOD.RUN.</summary>
        public const string EodRun = "LND.EOD.RUN";

        /// <summary>Ask for a loan to be written off, and withdraw one's own request.</summary>
        public const string WriteOffRequest = "LND.WRITE_OFF.REQUEST";

        /// <summary>Approve or reject a write-off (four eyes: never one's own request).</summary>
        public const string WriteOffApprove = "LND.WRITE_OFF.APPROVE";

        /// <summary>Reverse a customer receipt or recovery on a retail loan; the later receipts are applied again.</summary>
        public const string ReceiptReverse = "LND.RECEIPT.REVERSE";
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

    /// <summary>Screening (the Federation Final List, merged from FINOVA): federation staff and member companies.</summary>
    public static class Scr
    {
        /// <summary>Register incoming letters and attach their scans (intake).</summary>
        public const string LetterEdit = "SCR.LETTER.EDIT";

        /// <summary>Read letters, parties, decisions and the Final List (federation staff and auditors).</summary>
        public const string DecisionView = "SCR.DECISION.VIEW";

        /// <summary>Register parties and enter, change, submit and cancel decisions (data entry).</summary>
        public const string DecisionEdit = "SCR.DECISION.EDIT";

        /// <summary>Workflow step "Review decision" (SCR_REVIEW): never one's own decision.</summary>
        public const string DecisionReview = "SCR.DECISION.REVIEW";

        /// <summary>Workflow step "Approve and publish" (SCR_APPROVAL): never a decision one entered or reviewed.</summary>
        public const string DecisionApprove = "SCR.DECISION.APPROVE";

        /// <summary>Create and suspend member companies and manage any company's user accounts.</summary>
        public const string CompanyAdmin = "SCR.COMPANY.ADMIN";

        /// <summary>Manage the user accounts of one's own member company (company administrator).</summary>
        public const string CompanyUsers = "SCR.COMPANY.USERS";

        /// <summary>Screen people and companies against the Final List, and read one's company's inquiries (member company user).</summary>
        public const string InquiryCreate = "SCR.INQUIRY.CREATE";

        /// <summary>Read every member company's inquiries (federation audit).</summary>
        public const string InquiryAudit = "SCR.INQUIRY.AUDIT";

        /// <summary>Billing: price plans, each company's plan and invoicing period, issuing and cancelling invoices.</summary>
        public const string BillingManage = "SCR.BILLING.MANAGE";

        /// <summary>Billing: read invoices and what each company owes.</summary>
        public const string BillingView = "SCR.BILLING.VIEW";

        /// <summary>The sign-in log: every sign-in attempt, and per session the inquiries made in it.</summary>
        public const string AccessLogView = "SCR.ACCESS_LOG.VIEW";
    }

    /// <summary>Communication Center: the mail servers every e-mail goes through, and the log of what was sent.</summary>
    /// <summary>End of Day: the night batch of every application.</summary>
    public static class Eod
    {
        /// <summary>Read the End of Day: the days run, each job's result, and the jobs and their settings.</summary>
        public const string View = "EOD.VIEW";

        /// <summary>Run the End of Day for today or up to a day (administrators).</summary>
        public const string Run = "EOD.RUN";

        /// <summary>Switch jobs on or off, set their order and their settings (administrators).</summary>
        public const string JobsEdit = "EOD.JOBS.EDIT";
    }

    public static class Cmc
    {
        /// <summary>Add and change mail servers, send test e-mails and choose the server in use.</summary>
        public const string MailServerEdit = "CMC.MAIL_SERVER.EDIT";

        /// <summary>Read the log of every e-mail the system sent or tried to send.</summary>
        public const string EmailLogView = "CMC.EMAIL_LOG.VIEW";
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
