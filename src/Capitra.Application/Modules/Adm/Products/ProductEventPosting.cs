namespace Capitra.Application.Modules.Adm.Products;

/// <summary>One active GL line of a product event rule, as a posting reads it.</summary>
public sealed record ProductEventGlLine(
    int JournalNo, int LineNo, int? ChargeSequenceNo, string? ChargeCode, string DebitCreditCode, string AmountCode, bool FromBankAccount,
    string? AccountCode, string? Description);

/// <summary>A charge component of a product event rule in effect, with how it is priced.</summary>
public sealed record ProductEventCharge(
    long RuleChargeId, int SequenceNo, string ChargeCode, string ChargeNameEn, string? ChargeNameAr, string CalculationMethodCode,
    decimal? FixedAmount, decimal? ChargeRate, decimal? PerUnitAmount, string? AmountBaseCode, string IncomeRecognitionCode,
    decimal? MinimumAmount, decimal? MaximumAmount, int RoundingScale, string ChargeTimingCode, decimal TaxRate);

/// <summary>A product event rule on a date: the journal source and category it posts under, its charges and its GL lines.</summary>
public sealed record ProductEventPlan(
    long ProductEventRuleId, string RuleCode, string SourceCode, string CategoryCode, IReadOnlyList<ProductEventCharge> Charges,
    IReadOnlyList<ProductEventGlLine> Lines)
{
    public IEnumerable<string> ChargeCodes => Charges.Select(x => x.ChargeCode);
}

/// <summary>
/// The amount of one charge component: Base is the figure a PERCENTAGE rate was applied to, Problem says why it could not be
/// calculated (the amount is then 0).
/// </summary>
public sealed record ProductEventChargeAmount(decimal Amount, decimal? Base, string? Problem);

/// <summary>A GL line ready for the GL posting API.</summary>
public sealed record ProductEventPostingLine(int JournalNo, string Account, decimal Dr, decimal Cr, string Description);

/// <summary>Turns the GL lines of a product event rule into posting lines with the event's figures.</summary>
public static class ProductEventPosting
{
    /// <summary>
    /// FIXED: the fixed amount; PERCENTAGE: rate % of the base the component names; PER_UNIT: the amount per unit times the
    /// units. The minimum and maximum then apply, and the result is rounded to the component's scale.
    /// </summary>
    public static ProductEventChargeAmount Calculate(ProductEventCharge c, Func<string, decimal?> baseOf, int units)
    {
        decimal? amount = null, b = null;
        string? problem = null;
        switch (c.CalculationMethodCode)
        {
            case "FIXED":
                amount = c.FixedAmount;
                problem = amount is null ? "the rule has no fixed amount" : null;
                break;
            case "PERCENTAGE":
                b = c.AmountBaseCode is null ? null : baseOf(c.AmountBaseCode);
                if (b is null)
                    problem = "the rule has no amount base";
                else if (c.ChargeRate is null)
                    problem = "the rule has no rate";
                else
                    amount = b.Value * c.ChargeRate.Value / 100;
                break;
            case "PER_UNIT":
                amount = c.PerUnitAmount * units;
                problem = amount is null ? "the rule has no amount per unit" : null;
                break;
            default:
                problem = "calculation method " + c.CalculationMethodCode + " cannot be calculated at this event";
                break;
        }

        if (amount is null)
            return new ProductEventChargeAmount(0, b, problem);
        var a = amount.Value;
        if (c.MinimumAmount is { } min && a < min)
            a = min;
        if (c.MaximumAmount is { } max && a > max)
            a = max;
        return new ProductEventChargeAmount(Math.Round(a, Math.Clamp(c.RoundingScale, 0, 6), MidpointRounding.AwayFromZero), b, null);
    }

    /// <summary>
    /// The rule's own lines and its charges' lines in journal and line order; a zero amount gives no line, and lines of one
    /// journal on the same account and side are added together, so one charge per line still posts one line per account.
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> Build(ProductEventPlan plan, Func<ProductEventGlLine, decimal> amountOf, string? bankAccount)
    {
        var result = new List<ProductEventPostingLine>();
        foreach (var line in plan.Lines.OrderBy(x => x.JournalNo).ThenBy(x => x.LineNo).ThenBy(x => x.ChargeSequenceNo.HasValue).ThenBy(x => x.ChargeSequenceNo))
        {
            var amount = amountOf(line);
            if (amount == 0)
                continue;
            var account = line.FromBankAccount ? bankAccount : line.AccountCode;
            if (string.IsNullOrWhiteSpace(account))
                throw new InvalidOperationException($"GL line {line.LineNo} of rule {plan.RuleCode} has no account.");

            var debit = line.DebitCreditCode == "D";
            var at = result.FindIndex(x => x.JournalNo == line.JournalNo && x.Account == account && (x.Dr != 0) == debit);
            if (at >= 0)
            {
                var x = result[at];
                result[at] = x with { Dr = x.Dr + (debit ? amount : 0), Cr = x.Cr + (debit ? 0 : amount) };
            }
            else
            {
                result.Add(new ProductEventPostingLine(line.JournalNo, account, debit ? amount : 0, debit ? 0 : amount,
                    line.Description ?? line.AmountCode));
            }
        }

        return result;
    }
}
