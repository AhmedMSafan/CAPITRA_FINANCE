namespace Capitra.Application.Modules.Lnd;

/// <summary>What an instalment still owes of each part (LATE_FEE, CHARGES, INSURANCE, INTEREST, PRINCIPAL).</summary>
public sealed record LndInstallmentOwed(long InstallmentId, int InstallmentNo, DateOnly DueDate, IReadOnlyDictionary<string, decimal> Owed);

/// <summary>One part of one instalment paid by a receipt.</summary>
public sealed record LndAllocationLine(long InstallmentId, int InstallmentNo, string PartCode, decimal Amount);

/// <summary>
/// The allocation engine of retail loans (Ahmed, 4 Nov 2026: late fee, charges, insurance, interest, principal; the order is a product
/// setting copied to the loan). A receipt pays the oldest instalment first and, within it, the parts in the loan's payment order, then
/// the next instalment. Only instalments due on or before the value date are paid: money beyond them stays on the receipt as the
/// customer's credit balance, used when the next instalments fall due (proposed default for early payments, design deck).
/// </summary>
public static class LndAllocator
{
    public static (IReadOnlyList<LndAllocationLine> Lines, decimal Left) Allocate(
        IEnumerable<LndInstallmentOwed> installments, IReadOnlyList<string> order, decimal amount, DateOnly valueDate)
    {
        var lines = new List<LndAllocationLine>();
        var left = amount;
        foreach (var i in installments.Where(x => x.DueDate <= valueDate).OrderBy(x => x.InstallmentNo))
        {
            foreach (var part in order)
            {
                if (left <= 0) return (lines, 0);
                var owed = i.Owed.GetValueOrDefault(part);
                if (owed <= 0) continue;
                var pay = Math.Min(owed, left);
                lines.Add(new LndAllocationLine(i.InstallmentId, i.InstallmentNo, part, pay));
                left -= pay;
            }
        }
        return (lines, left);
    }
}
