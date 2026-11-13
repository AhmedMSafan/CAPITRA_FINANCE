using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// Replaying a loan's receipts after one of them is reversed (design deck: receipt reversal with replay). The reversed receipt and every
/// receipt after it (by value date, then entry) are taken off the instalments; the later ones are then applied again, in their order,
/// with the same money and value date. Without the reversed money the loan owes more at each step, so a later receipt can only apply
/// more than before: what it kept as customer credit pays the difference.
/// </summary>
public static class LndReplay
{
    public sealed record Receipt(long TxnId, DateOnly ValueDate, decimal Amount, IReadOnlyDictionary<string, decimal> OldPaid);

    public sealed record Result(long TxnId, IReadOnlyList<LndAllocationLine> Lines, decimal Left, IReadOnlyDictionary<string, decimal> Delta)
    {
        public decimal Allocated => Lines.Sum(x => x.Amount);

        public bool Changed => Delta.Values.Any(v => v != 0);
    }

    /// <summary>Adds (or, negative, takes off) a part paid on an instalment.</summary>
    public static void Pay(LndInstallment i, string part, decimal amount)
    {
        switch (part)
        {
            case "LATE_FEE": i.LateFeePaidAmount += amount; break;
            case "CHARGES": i.ChargesPaidAmount += amount; break;
            case "INSURANCE": i.InsurancePaidAmount += amount; break;
            case "INTEREST": i.InterestPaidAmount += amount; break;
            default: i.PrincipalPaidAmount += amount; break;
        }
    }

    /// <summary>The instalment's status from what it still owes: PAID (on the day given), PART_PAID or DUE.</summary>
    public static void SetStatus(LndInstallment i, DateOnly? paidOn)
    {
        var rest = LndReceiptService.Owed(i).Values.Sum();
        var paid = i.PrincipalPaidAmount + i.InterestPaidAmount + i.ChargesPaidAmount + i.InsurancePaidAmount + i.LateFeePaidAmount;
        i.StatusCode = rest == 0 ? "PAID" : paid > 0 ? "PART_PAID" : "DUE";
        i.PaidDate = rest == 0 ? paidOn ?? i.PaidDate : null;
    }

    /// <summary>Applies the receipts again, in order, to the schedule (changed in place) and says what each one now pays.</summary>
    public static IReadOnlyList<Result> Run(IReadOnlyList<LndInstallment> schedule, IReadOnlyList<string> order, IReadOnlyList<Receipt> receipts)
    {
        var results = new List<Result>();
        var byId = schedule.ToDictionary(x => x.InstallmentId);
        foreach (var r in receipts)
        {
            var owed = schedule.OrderBy(x => x.InstallmentNo).Where(x => LndReceiptService.Owed(x).Values.Sum() > 0)
                .Select(i => new LndInstallmentOwed(i.InstallmentId, i.InstallmentNo, i.DueDate, LndReceiptService.Owed(i))).ToList();
            var (lines, left) = LndAllocator.Allocate(owed, order, r.Amount, r.ValueDate);
            foreach (var l in lines) Pay(byId[l.InstallmentId], l.PartCode, l.Amount);
            foreach (var id in lines.Select(x => x.InstallmentId).Distinct()) SetStatus(byId[id], r.ValueDate);
            var delta = LndReceiptService.Parts.ToDictionary(p => p, p => lines.Where(l => l.PartCode == p).Sum(l => l.Amount) - r.OldPaid.GetValueOrDefault(p));
            results.Add(new Result(r.TxnId, lines, left, delta));
        }
        return results;
    }
}
