using Capitra.Application.Common;
using Capitra.Application.Modules.Adm.Products;

namespace Capitra.Application.Modules.Lnd;

/// <summary>A consumer finance charge as New Finance shows it: its amount and tax, and whether it is paid upfront or added to the finance.</summary>
public sealed record LndCfChargeDto(string ChargeCode, string ChargeName, string BasisText, decimal Amount, decimal TaxAmount, string TimingCode);

/// <summary>The charges of a consumer finance for a price and down payment, the part added to the finance and the part paid upfront.</summary>
public sealed record LndCfChargesDto(IReadOnlyList<LndCfChargeDto> Charges, decimal FinancedAmount, decimal AddedToFinance, decimal PaidUpfront, decimal LoanAmount,
    string? Problem);

public sealed record LndCfChargesRequest(decimal? GoodsPriceAmount, decimal? DownPaymentAmount, IReadOnlyDictionary<string, string>? ChargeTimings);

/// <summary>
/// The customer's charges of a consumer finance (Ahmed, 26 Nov 2026) come from the product's rule CF_DELIVERY and are decided on New
/// Finance, before the schedule: each one is paid upfront (the default) or added to the finance, so Credit approves the real amount.
/// </summary>
public static class LndCfCharges
{
    public const string RuleCode = "CF_DELIVERY";
    public const string Upfront = "PAID_UPFRONT";
    public const string Financed = "FINANCE_WITH_PRINCIPAL";

    /// <summary>
    /// Each charge of the rule worked out on the price (GOODS_PRICE) or the price less the down payment (FINANCED_AMOUNT, LOAN_AMOUNT),
    /// with the timing chosen for it (else the rule's, upfront unless the rule adds it to the finance). The loan amount is the price
    /// less the down payment plus the charges (with their tax) added to the finance.
    /// </summary>
    public static LndCfChargesDto Work(IReadOnlyList<ProductEventCharge> charges, decimal price, decimal down, IReadOnlyDictionary<string, string>? chosen)
    {
        var financed = Money.Round(price - down, 2);
        decimal? BaseOf(string code) => code switch
        {
            "GOODS_PRICE" => price,
            "FINANCED_AMOUNT" or "LOAN_AMOUNT" => financed,
            _ => null,
        };
        var rows = new List<LndCfChargeDto>();
        foreach (var c in charges)
        {
            var amount = ProductEventPosting.Calculate(c, BaseOf, 1);
            if (amount.Problem is not null)
                return new LndCfChargesDto([], financed, 0, 0, financed, $"Charge {c.ChargeCode} cannot be worked out: {amount.Problem}.");
            var timing = chosen is not null && chosen.TryGetValue(c.ChargeCode, out var t) && t is Upfront or Financed ? t
                : c.ChargeTimingCode == Financed ? Financed : Upfront;
            var tax = Money.Round(amount.Amount * c.TaxRate / 100m, 2);
            rows.Add(new LndCfChargeDto(c.ChargeCode, c.ChargeNameEn, Basis(c), amount.Amount, tax, timing));
        }
        var added = rows.Where(x => x.TimingCode == Financed).Sum(x => x.Amount + x.TaxAmount);
        var upfront = rows.Where(x => x.TimingCode == Upfront).Sum(x => x.Amount + x.TaxAmount);
        return new LndCfChargesDto(rows, financed, added, upfront, financed + added, null);
    }

    private static string Basis(ProductEventCharge c) => c.CalculationMethodCode switch
    {
        "PERCENTAGE" => $"{c.ChargeRate:0.##}% of {(c.AmountBaseCode == "GOODS_PRICE" ? "the price" : "the financed amount")}",
        "FIXED" => "Fixed",
        _ => c.CalculationMethodCode.ToLowerInvariant(),
    };
}
