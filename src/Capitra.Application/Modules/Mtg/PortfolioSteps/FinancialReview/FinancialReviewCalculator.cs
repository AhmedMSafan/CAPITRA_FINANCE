namespace Capitra.Application.Modules.Mtg.PortfolioSteps.FinancialReview;

/// <summary>What the reviewer chooses on the financial review.</summary>
public sealed record FinancialReviewInputs(
    string PvMethodCode,
    string MarginBaseCode,
    string AdminBaseCode,
    decimal? AdminFinalAmount, // null uses the calculated amount
    bool IsAdminIncluded,
    decimal? LegalFinalAmount,
    bool IsLegalIncluded,
    decimal? ValuationFinalAmount,
    bool IsValuationIncluded)
{
    public static FinancialReviewInputs Default { get; } = new(
        FinancialReviewCalculator.AnnualMethod, FinancialReviewCalculator.SelectedOutstandingBase,
        FinancialReviewCalculator.SelectedOutstandingBase, null, true, null, true, null, true);
}

/// <summary>The portfolio's pricing terms (financial terms of the portfolio case).</summary>
public sealed record FinancialTerms(
    decimal? PvRate,
    decimal? MarginPct,
    decimal? AdminExpensePct,
    decimal? LegalFeePerUnitAmount,
    decimal? ValuationFeePerUnitAmount);

/// <summary>The contracts kept in the deal, added up.</summary>
public sealed record KeptContracts(decimal OutstandingAmount, decimal FaceValueAmount, int UnitCount);

public sealed record ChequeToValue(long ChequeId, string ChequeNo, string InvestorName, DateOnly DueDate, decimal ChequeAmount);

public sealed record ChequeValueDto(
    long ChequeId, string ChequeNo, string InvestorName, DateOnly DueDate, decimal ChequeAmount, int Days, decimal Months, decimal PvAmount);

/// <summary>Every figure of the financial review, the way APEX FR_SAVE works it out.</summary>
public sealed record FinancialCalculationDto(
    DateOnly PvBaseDate,
    string PvMethodCode,
    decimal PvRate,
    int ChequeCount,
    decimal ChequesFaceAmount,
    decimal ChequesPvAmount,
    decimal SelectedOutstandingAmount,
    decimal SelectedFaceValueAmount,
    string MarginBaseCode,
    decimal MarginPct,
    decimal MarginBaseAmount,
    decimal MarginAmount,
    string AdminBaseCode,
    decimal AdminExpensePct,
    decimal AdminBaseAmount,
    decimal AdminCalcAmount,
    decimal AdminFinalAmount,
    bool IsAdminIncluded,
    int UnitCount,
    decimal LegalFeePerUnitAmount,
    decimal LegalCalcAmount,
    decimal LegalFinalAmount,
    bool IsLegalIncluded,
    decimal ValuationFeePerUnitAmount,
    decimal ValuationCalcAmount,
    decimal ValuationFinalAmount,
    bool IsValuationIncluded,
    decimal TotalChargesAmount,
    decimal ProposedPurchaseAmount,
    decimal ExpectedReturnAmount,
    IReadOnlyList<ChequeValueDto> Cheques);

/// <summary>
/// Prices a portfolio from the cheques of the investors kept in the deal (APEX FR_SAVE):
/// present value of every cheque, less the margin, less the included admin, legal and valuation charges.
/// Amounts are rounded to 2 decimals, half away from zero, at the same points as APEX.
/// </summary>
public static class FinancialReviewCalculator
{
    public const string AnnualMethod = "ANNUAL";
    public const string MonthlyMethod = "MONTHLY";
    public const string SimpleMethod = "SIMPLE";
    public const string SelectedOutstandingBase = "SELECTED_OUTSTANDING";
    public const string SelectedFaceBase = "SELECTED_FACE";
    public const string ChequesPvBase = "CHEQUES_PV";
    public const string ProposedPurchaseBase = "PROPOSED_PURCHASE";

    public static readonly IReadOnlyList<string> PvMethods = [AnnualMethod, MonthlyMethod, SimpleMethod];
    public static readonly IReadOnlyList<string> MarginBases = [SelectedOutstandingBase, ChequesPvBase];
    public static readonly IReadOnlyList<string> AdminBases = [SelectedOutstandingBase, SelectedFaceBase, ChequesPvBase, ProposedPurchaseBase];

    public static FinancialCalculationDto Calculate(
        FinancialReviewInputs inputs, FinancialTerms terms, KeptContracts kept, IReadOnlyList<ChequeToValue> cheques, DateOnly baseDate)
    {
        var rate = terms.PvRate ?? throw new InvalidOperationException("The present value rate is not set.");
        var r = rate / 100m;

        var lines = cheques.Select(c =>
        {
            var due = c.DueDate;
            var days = Math.Max(due.DayNumber - baseDate.DayNumber, 0);
            var months = Math.Max(MonthsBetween(due, baseDate), 0);
            return new ChequeValueDto(c.ChequeId, c.ChequeNo, c.InvestorName, due, c.ChequeAmount, days, Math.Round(months, 4),
                Round(PresentValue(c.ChequeAmount, r, inputs.PvMethodCode, days, months)));
        }).ToList();

        var face = lines.Sum(x => x.ChequeAmount);
        var pv = lines.Sum(x => x.PvAmount);

        var marginPct = terms.MarginPct ?? 0;
        var marginBase = inputs.MarginBaseCode == ChequesPvBase ? pv : kept.OutstandingAmount;
        var margin = Round(marginBase * marginPct / 100);

        var adminPct = terms.AdminExpensePct ?? 0;
        var adminBase = inputs.AdminBaseCode switch
        {
            SelectedFaceBase => kept.FaceValueAmount,
            ChequesPvBase => pv,
            ProposedPurchaseBase => pv - margin,
            _ => kept.OutstandingAmount
        };
        var adminCalc = Round(adminBase * adminPct / 100);
        var legalFee = terms.LegalFeePerUnitAmount ?? 0;
        var legalCalc = Round(legalFee * kept.UnitCount);
        var valuationFee = terms.ValuationFeePerUnitAmount ?? 0;
        var valuationCalc = Round(valuationFee * kept.UnitCount);

        var adminFinal = inputs.AdminFinalAmount is { } a ? Round(a) : adminCalc;
        var legalFinal = inputs.LegalFinalAmount is { } l ? Round(l) : legalCalc;
        var valuationFinal = inputs.ValuationFinalAmount is { } v ? Round(v) : valuationCalc;

        var charges = (inputs.IsAdminIncluded ? adminFinal : 0) + (inputs.IsLegalIncluded ? legalFinal : 0)
                      + (inputs.IsValuationIncluded ? valuationFinal : 0);
        var price = Round(pv - margin - charges);
        var expectedReturn = Round(face - price);

        return new FinancialCalculationDto(
            baseDate, inputs.PvMethodCode, rate, lines.Count, face, pv, kept.OutstandingAmount, kept.FaceValueAmount,
            inputs.MarginBaseCode, marginPct, marginBase, margin,
            inputs.AdminBaseCode, adminPct, adminBase, adminCalc, adminFinal, inputs.IsAdminIncluded,
            kept.UnitCount, legalFee, legalCalc, legalFinal, inputs.IsLegalIncluded,
            valuationFee, valuationCalc, valuationFinal, inputs.IsValuationIncluded,
            charges, price, expectedReturn, lines);
    }

    /// <summary>One cheque discounted to the base date. A rate of zero or less leaves it as it is.</summary>
    public static decimal PresentValue(decimal amount, decimal rate, string method, int days, decimal months)
    {
        if (rate <= 0)
            return amount;
        var factor = method switch
        {
            MonthlyMethod => Math.Pow(1 + (double)rate / 12, (double)months),
            SimpleMethod => 1 + (double)rate * days / 365,
            _ => Math.Pow(1 + (double)rate, days / 365d)
        };
        return amount / (decimal)factor;
    }

    /// <summary>
    /// Oracle MONTHS_BETWEEN: whole months when both days are the same day of the month or both are month ends,
    /// otherwise the difference in days counts as 1/31 of a month each.
    /// </summary>
    public static decimal MonthsBetween(DateOnly later, DateOnly earlier)
    {
        var months = (later.Year - earlier.Year) * 12 + later.Month - earlier.Month;
        var laterIsMonthEnd = later.Day == DateTime.DaysInMonth(later.Year, later.Month);
        var earlierIsMonthEnd = earlier.Day == DateTime.DaysInMonth(earlier.Year, earlier.Month);
        if (later.Day == earlier.Day || (laterIsMonthEnd && earlierIsMonthEnd))
            return months;
        return months + (later.Day - earlier.Day) / 31m;
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
