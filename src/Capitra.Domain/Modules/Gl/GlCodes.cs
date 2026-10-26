namespace Capitra.Domain.Modules.Gl;

/// <summary>Fixed code lists of the general ledger, as used by the APEX App 120 pages and GL packages.</summary>
public static class GlCodes
{
    public const string SegmentTypeAccount = "ACCOUNT";

    public static readonly string[] SegmentTypes =
        ["COMPANY", "BRANCH", "COST_CENTER", SegmentTypeAccount, "BUSINESS_LINE", "INTERCOMPANY", "PROJECT", "CUSTOMER", "CONTRACT", "FUTURE", "CUSTOM"];

    public const string FormatAlphanumeric = "ALPHANUMERIC";
    public const string FormatNumeric = "NUMERIC";

    public static readonly string[] ValueFormats = [FormatAlphanumeric, FormatNumeric, "CHARACTER"];

    public const string DefaultModeSuggest = "SUGGEST";
    public const string DefaultModeFixed = "FIXED";

    public static readonly string[] AccountTypes =
        ["ASSET", "CONTRA_ASSET", "LIABILITY", "CONTRA_LIABILITY", "EQUITY", "REVENUE", "CONTRA_REVENUE", "EXPENSE", "CONTRA_EXPENSE"];

    public const string Debit = "DR";
    public const string Credit = "CR";

    public static readonly string[] NormalBalances = [Debit, Credit];

    public const string BalanceSheet = "BALANCE_SHEET";
    public const string IncomeStatement = "INCOME_STATEMENT";

    public static readonly string[] FinancialStatementClasses = [BalanceSheet, IncomeStatement];

    public const string CashFlowNone = "NONE";

    public static readonly string[] CashFlowClasses = [CashFlowNone, "OPERATING", "INVESTING", "FINANCING", "CASH_EQUIVALENT"];

    public const string Monthly = "MONTHLY";

    public static readonly string[] CalendarTypes = [Monthly];

    public static readonly string[] PeriodFrequencies = [Monthly];

    public const string YearOpen = "OPEN";

    public static readonly string[] YearStatuses = ["DRAFT", YearOpen, "CLOSED", "PERMANENTLY_CLOSED"];

    public const string PeriodNeverOpened = "NEVER_OPENED";
    public const string PeriodFutureEntry = "FUTURE_ENTRY";
    public const string PeriodOpen = "OPEN";
    public const string PeriodClosed = "CLOSED";
    public const string PeriodPermanentlyClosed = "PERMANENTLY_CLOSED";

    /// <summary>Statuses an accounting period can give its new ledger periods.</summary>
    public static readonly string[] DefaultPeriodStatuses = [PeriodNeverOpened, PeriodFutureEntry, PeriodOpen, PeriodClosed];

    public const string PostingNone = "NONE";
    public const string PostingNormal = "NORMAL";
    public const string PostingAdjustmentOnly = "ADJUSTMENT_ONLY";
    public const string PostingFutureOnly = "FUTURE_ONLY";

    public const string LedgerPrimary = "PRIMARY";

    public static readonly string[] LedgerTypes = [LedgerPrimary, "SECONDARY"];

    public const string LedgerActive = "ACTIVE";

    public static readonly string[] LedgerStatuses = [LedgerActive, "INACTIVE"];

    /// <summary>Normal balance and statement an account type usually has, offered when a new account is created.</summary>
    public static (string NormalBalance, string StatementClass) DefaultsFor(string accountType) => accountType switch
    {
        "ASSET" or "CONTRA_LIABILITY" => (Debit, BalanceSheet),
        "CONTRA_ASSET" or "LIABILITY" or "EQUITY" => (Credit, BalanceSheet),
        "REVENUE" or "CONTRA_EXPENSE" => (Credit, IncomeStatement),
        _ => (Debit, IncomeStatement),
    };
}
