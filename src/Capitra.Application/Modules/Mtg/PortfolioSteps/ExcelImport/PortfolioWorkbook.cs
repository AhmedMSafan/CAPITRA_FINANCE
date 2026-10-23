using System.Globalization;
using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;

/// <summary>One worksheet of the seller workbook and its columns, in order.</summary>
public sealed record WorkbookSheetDto(string Sheet, IReadOnlyList<string> Columns);

/// <summary>
/// The seller workbook of APEX page 208: three worksheets, a header row, then one row per record.
/// Every sheet starts with the portfolio number and the project code so a workbook cannot be loaded into the wrong case.
/// </summary>
internal static class PortfolioWorkbook
{
    public const string InvestorsSheet = "INVESTORS";
    public const string UnitsSheet = "UNITS";
    public const string AccountsSheet = "ACCOUNTS";

    public const int PortfolioNo = 0;
    public const int ProjectCode = 1;

    public static class Investor
    {
        public const int Code = 2, NameAr = 3, NameEn = 4, NationalId = 5, BirthDate = 6, Phone = 7, Email = 8,
            Address = 9, Employer = 10, DeclaredIncome = 11, ActiveFlag = 12;
    }

    public static class Unit
    {
        public const int Code = 2, TypeCode = 3, BuildingNo = 4, FloorNo = 5, UnitNo = 6, AreaSqm = 7, ContractValue = 8,
            MarketValue = 9, LicenseStatus = 10, ActiveFlag = 11;
    }

    public static class Account
    {
        public const int SourceContractNo = 2, InvestorCode = 3, UnitCode = 4, BookingDate = 5, ContractDate = 6,
            ContractValue = 7, PaidValue = 8, OutstandingValue = 9, InstallmentAmount = 10, InstallmentFrequency = 11,
            LastPaymentDate = 12, RegularPaymentMonths = 13, ArrearsAmount = 14, AccountStatus = 15;
    }

    public static readonly IReadOnlyList<WorkbookSheetDto> Format =
    [
        new(InvestorsSheet,
        [
            "PORTFOLIO_NO", "PROJECT_CODE", "INVESTOR_CODE", "FULL_NAME_AR", "FULL_NAME_EN", "NATIONAL_ID", "DATE_OF_BIRTH",
            "PHONE_NO", "EMAIL_ADDRESS", "ADDRESS", "EMPLOYER_NAME", "DECLARED_INCOME", "ACTIVE_FLAG"
        ]),
        new(UnitsSheet,
        [
            "PORTFOLIO_NO", "PROJECT_CODE", "UNIT_CODE", "UNIT_TYPE_CODE", "BUILDING_NO", "FLOOR_NO", "UNIT_NO", "AREA_SQM",
            "CONTRACT_VALUE", "CURRENT_MARKET_VALUE", "LICENSE_STATUS", "ACTIVE_FLAG"
        ]),
        new(AccountsSheet,
        [
            "PORTFOLIO_NO", "PROJECT_CODE", "SOURCE_CONTRACT_NO", "INVESTOR_CODE", "UNIT_CODE", "BOOKING_DATE", "CONTRACT_DATE",
            "CONTRACT_VALUE", "PAID_VALUE", "OUTSTANDING_VALUE", "INSTALLMENT_AMOUNT", "INSTALLMENT_FREQUENCY",
            "LAST_PAYMENT_DATE", "REGULAR_PAYMENT_MONTHS", "ARREARS_AMOUNT", "ACCOUNT_STATUS"
        ])
    ];

    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy"];

    /// <summary>A code as APEX compares it: trimmed and upper case.</summary>
    public static string? Code(WorkbookRow row, int column) => Text(row, column)?.ToUpperInvariant();

    public static string? Text(WorkbookRow row, int column) =>
        string.IsNullOrWhiteSpace(row[column]) ? null : row[column]!.Trim();

    /// <summary>A number with thousands separators allowed. Blank is null; anything else unreadable adds an error.</summary>
    public static decimal? Number(WorkbookRow row, int column, string label, List<string> errors)
    {
        var text = Text(row, column)?.Replace(",", "", StringComparison.Ordinal);
        if (text is null)
            return null;
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return value;
        errors.Add($"{label} is not a number.");
        return null;
    }

    public static DateOnly? Date(WorkbookRow row, int column, string label, List<string> errors)
    {
        var text = Text(row, column);
        if (text is null)
            return null;
        if (DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
            return value;
        errors.Add($"{label} is not a date (use YYYY-MM-DD).");
        return null;
    }

    /// <summary>ACTIVE_FLAG: N makes the record inactive; blank or anything else keeps it active, as APEX did (nvl(flag,'Y')).</summary>
    public static bool IsActive(WorkbookRow row, int column) =>
        !string.Equals(Text(row, column), "N", StringComparison.OrdinalIgnoreCase);
}
