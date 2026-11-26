using Capitra.Application.Common;
using Capitra.Application.Modules.Adm.Products;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// A consumer finance charge: its amount and tax, its rule's default (upfront or added to the schedule) and amount, what Credit
/// decided, and whether that changed the rule's timing or amount.
/// </summary>
public sealed record LndCfChargeDto(string ChargeCode, string ChargeName, string BasisText, decimal Amount, decimal TaxAmount, string TimingCode,
    string DefaultTimingCode, bool IsOverridden, decimal RuleAmount);

/// <summary>The charges of a consumer finance, the part added to the schedule and the part paid upfront; Decided once Credit approved them.</summary>
public sealed record LndCfChargesDto(IReadOnlyList<LndCfChargeDto> Charges, decimal FinancedAmount, decimal AddedToFinance, decimal PaidUpfront, decimal LoanAmount,
    string? Problem, bool Decided = false);

/// <summary>
/// The customer's charges of a consumer finance (Ahmed, 26 Nov 2026) come from the rule of the Charges and claim step (CF_CHARGES).
/// Credit approval decides each one, upfront (the rule's default) or added to the schedule; Charges and claim executes the rule for
/// the upfront ones (claim and GL), and those added to the schedule become part of the principal, with no separate charge.
/// </summary>
public static class LndCfCharges
{
    public const string Upfront = "PAID_UPFRONT";
    public const string Financed = "FINANCE_WITH_PRINCIPAL";

    /// <summary>The rule a consumer finance's charges come from, when its workflow step names none.</summary>
    public const string RuleCode = "CF_CHARGES";

    /// <summary>The rule's default for a charge: added to the schedule only when the rule says so, else upfront.</summary>
    public static string DefaultTiming(ProductEventCharge c) => c.ChargeTimingCode == Financed ? Financed : Upfront;

    /// <summary>
    /// Each charge of the rule worked out on the price (GOODS_PRICE) or the price less the down payment (FINANCED_AMOUNT, LOAN_AMOUNT),
    /// with the timing and amount Credit chose for it (else the rule's; 0 waives it, the tax follows the amount). The loan amount is the
    /// price less the down payment plus the charges (with their tax) added to the schedule.
    /// </summary>
    public static LndCfChargesDto Work(IReadOnlyList<ProductEventCharge> charges, decimal price, decimal down, IReadOnlyDictionary<string, string>? chosen,
        IReadOnlyDictionary<string, decimal>? amounts = null)
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
            var def = DefaultTiming(c);
            var timing = chosen is not null && chosen.TryGetValue(c.ChargeCode, out var t) && t is Upfront or Financed ? t : def;
            var value = amount.Amount;
            if (amounts is not null && amounts.TryGetValue(c.ChargeCode, out var a))
            {
                if (a < 0) return new LndCfChargesDto([], financed, 0, 0, financed, $"The amount of {c.ChargeNameEn} cannot be below zero.");
                value = Money.Round(a, 2);
            }
            var tax = Money.Round(value * c.TaxRate / 100m, 2);
            rows.Add(new LndCfChargeDto(c.ChargeCode, c.ChargeNameEn, Basis(c), value, tax, timing, def, timing != def || value != amount.Amount, amount.Amount));
        }
        return Totals(rows, financed, false);
    }

    /// <summary>The totals of charges already worked out (e.g. those Credit saved).</summary>
    public static LndCfChargesDto Totals(IReadOnlyList<LndCfChargeDto> rows, decimal financed, bool decided)
    {
        var added = rows.Where(x => x.TimingCode == Financed).Sum(x => x.Amount + x.TaxAmount);
        var upfront = rows.Where(x => x.TimingCode == Upfront).Sum(x => x.Amount + x.TaxAmount);
        return new LndCfChargesDto(rows, financed, added, upfront, financed + added, null, decided);
    }

    private static string Basis(ProductEventCharge c) => c.CalculationMethodCode switch
    {
        "PERCENTAGE" => $"{c.ChargeRate:0.##}% of {(c.AmountBaseCode == "GOODS_PRICE" ? "the price" : "the financed amount")}",
        "FIXED" => "Fixed",
        _ => c.CalculationMethodCode.ToLowerInvariant(),
    };
}
