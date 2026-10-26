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

    /// <summary>Normal balance and statement an account type usually has, offered when a new account is created.</summary>
    public static (string NormalBalance, string StatementClass) DefaultsFor(string accountType) => accountType switch
    {
        "ASSET" or "CONTRA_LIABILITY" => (Debit, BalanceSheet),
        "CONTRA_ASSET" or "LIABILITY" or "EQUITY" => (Credit, BalanceSheet),
        "REVENUE" or "CONTRA_EXPENSE" => (Credit, IncomeStatement),
        _ => (Debit, IncomeStatement),
    };
}
