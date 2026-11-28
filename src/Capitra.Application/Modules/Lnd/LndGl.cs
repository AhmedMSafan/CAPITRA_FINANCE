using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;

namespace Capitra.Application.Modules.Lnd;

public static class LndGl
{
    /// <summary>
    /// GL lines from a product's rule for a lending event whose lines are all fixed accounts (end of day, write-off): each line takes one
    /// of the event's amount codes. Lines with the same account and side in one journal are added together, and each journal must balance.
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> Lines(ProductEventPlan plan, IReadOnlyDictionary<string, decimal> amounts,
        IReadOnlyList<string> codes, string what)
    {
        var other = plan.Lines.Where(l => l.ChargeCode is not null || !codes.Contains(l.AmountCode)).Select(l => l.ChargeCode ?? l.AmountCode).Distinct().ToList();
        if (other.Count > 0) throw new BusinessRuleException($"Rule {plan.RuleCode} posts {string.Join(", ", other)}; {what} gives only {string.Join(", ", codes)}.");
        var missing = codes.Where(c => amounts.GetValueOrDefault(c) != 0 && !plan.Lines.Any(l => l.AmountCode == c)).ToList();
        if (missing.Count > 0) throw new BusinessRuleException($"Rule {plan.RuleCode} has no lines for {string.Join(", ", missing)}. Add a debit and a credit line for each.");

        var result = new List<ProductEventPostingLine>();
        foreach (var l in plan.Lines.OrderBy(x => x.JournalNo).ThenBy(x => x.LineNo))
        {
            var value = amounts.GetValueOrDefault(l.AmountCode);
            if (value == 0) continue;
            if (l.FromBankAccount || string.IsNullOrWhiteSpace(l.AccountCode))
                throw new BusinessRuleException($"GL line {l.LineNo} of rule {plan.RuleCode} ({l.AmountCode}) has no account.");
            var debit = l.DebitCreditCode == "D";
            var at = result.FindIndex(x => x.JournalNo == l.JournalNo && x.Account == l.AccountCode && (x.Dr != 0) == debit);
            if (at >= 0) result[at] = result[at] with { Dr = result[at].Dr + (debit ? value : 0), Cr = result[at].Cr + (debit ? 0 : value) };
            else result.Add(new ProductEventPostingLine(l.JournalNo, l.AccountCode, debit ? value : 0, debit ? 0 : value, l.Description ?? l.AmountCode));
        }
        foreach (var j in result.GroupBy(x => x.JournalNo))
            if (j.Sum(x => x.Dr) != j.Sum(x => x.Cr))
                throw new BusinessRuleException($"Journal {j.Key} of rule {plan.RuleCode} does not balance: debit {j.Sum(x => x.Dr):#,0.00}, credit {j.Sum(x => x.Cr):#,0.00}. Check its lines.");
        return result;
    }
}
