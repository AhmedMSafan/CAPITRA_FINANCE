namespace Capitra.Application.Modules.Adm.Products;

/// <summary>One active GL line of a product event rule, as a posting reads it.</summary>
public sealed record ProductEventGlLine(
    int JournalNo, int LineNo, int? ChargeSequenceNo, string? ChargeCode, string DebitCreditCode, string AmountCode, bool FromBankAccount,
    string? AccountCode, string? Description);

/// <summary>The rule that applies to a product and business event on a date: its charges and its GL lines.</summary>
public sealed record ProductEventPlan(
    long ProductEventRuleId, string RuleCode, string EventCode, IReadOnlyList<string> ChargeCodes, IReadOnlyList<ProductEventGlLine> Lines);

/// <summary>A GL line ready for the GL posting API.</summary>
public sealed record ProductEventPostingLine(int JournalNo, string Account, decimal Dr, decimal Cr, string Description);

/// <summary>Turns the GL lines of a product event rule into posting lines with the event's figures.</summary>
public static class ProductEventPosting
{
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
