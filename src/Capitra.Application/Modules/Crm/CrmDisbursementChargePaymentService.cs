using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>A payment source of the customer with balance left (FCT_REPAYMENT_SOURCES of an active repayment).</summary>
public sealed record CrmChargeSourceDto(
    long RepaymentSourceId, string RepaymentNumber, string SourceType, string? Reference, DateOnly ValueDate, decimal SourceAmount, decimal UsedAmount,
    decimal AvailableAmount);

/// <summary>One payment of a charge from a source (FCT_REPAYMENT_ALLOCATIONS, target CHARGE).</summary>
public sealed record CrmChargePaymentRowDto(
    long AllocationId, string RuleCode, string RepaymentNumber, string SourceType, string? Reference, DateOnly AllocationDate, decimal Amount);

/// <summary>The Charges Payment section of the Invoice Upload page.</summary>
public sealed record CrmChargePaymentDto(
    decimal TotalCharges, decimal PaidAmount, decimal OutstandingAmount, string StatusCode, string? GlTranSetId, DateOnly? GlPostingDate,
    IReadOnlyList<CrmChargeSourceDto> Sources, IReadOnlyList<CrmChargePaymentRowDto> Payments, IReadOnlyList<long> ChosenSourceIds);

/// <summary>
/// Charges Payment (agreed with Ahmed, 31 Oct 2026), in place of FINOVA's client claim and page 713 for the disbursement charges:
/// the user pays the request's calculated charges from the customer's payment sources (collected cheques, bank transfers), each
/// payment an FCT_REPAYMENT_ALLOCATIONS row of target CHARGE that uses up the source's balance. Complete of the Invoice Upload
/// step needs every charge paid; it then posts the charges' GL (journal 1 DR customer receivable / CR fee income, journal 2
/// DR customer unallocated receipts / CR customer receivable) under the product's transaction set ID, kept on the request.
/// </summary>
public sealed class CrmDisbursementChargePaymentService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    public const string TargetCharge = "CHARGE";

    private string User => currentUser.UserName ?? "SYSTEM";

    /// <summary>Charges that are owed: calculated, not cancelled or reversed, with an amount.</summary>
    internal static bool Owed(FctDisbursementCharge c) => c.TotalAmount > 0 && c.StatusCode is not ("CANCELLED" or "REVERSED");

    internal static decimal Outstanding(FctDisbursementCharge c) => Math.Max(c.TotalAmount - c.CollectedAmount - c.WaivedAmount, 0);

    public async Task<CrmChargePaymentDto> GetAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        var charges = (await db.Set<FctDisbursementCharge>().AsNoTracking().Where(x => x.DisbursementId == disbursementId).ToListAsync(cancellationToken))
            .Where(Owed).ToList();
        var total = charges.Sum(x => x.TotalAmount);
        var outstanding = charges.Sum(Outstanding);
        var paid = total - outstanding;
        var status = total == 0 ? "NO_CHARGES" : outstanding == 0 ? "PAID" : paid > 0 ? "PARTIALLY_PAID" : "NOT_PAID";

        var sources = await (from s in db.Set<FctRepaymentSource>().AsNoTracking()
                             join t in db.Set<FctRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
                             join c in db.Set<ChqCheque>() on s.ChequeId equals c.ChequeId into cs
                             from c in cs.DefaultIfEmpty()
                             where t.ClientId == d.ClientId && s.StatusCode == "ACTIVE" && t.StatusCode != "REVERSED" && t.StatusCode != "CANCELLED"
                                   && s.SourceAmount > s.AllocatedAmount
                             orderby t.ValueDate, s.RepaymentSourceId
                             select new CrmChargeSourceDto(s.RepaymentSourceId, t.RepaymentNumber, s.SourceType,
                                 c != null ? c.ChequeNo : s.SourceReference ?? t.BankStatementReference ?? t.ExternalReference, t.ValueDate,
                                 s.SourceAmount, s.AllocatedAmount, s.SourceAmount - s.AllocatedAmount)).ToListAsync(cancellationToken);

        var payments = await (from a in db.Set<FctRepaymentAllocation>().AsNoTracking()
                              join ch in db.Set<FctDisbursementCharge>() on a.TargetId equals ch.DisbursementChargeId
                              join s in db.Set<FctRepaymentSource>() on a.RepaymentSourceId equals s.RepaymentSourceId
                              join t in db.Set<FctRepaymentTransaction>() on a.RepaymentTxnId equals t.RepaymentTxnId
                              join c in db.Set<ChqCheque>() on s.ChequeId equals c.ChequeId into cs
                              from c in cs.DefaultIfEmpty()
                              where a.DisbursementId == disbursementId && a.TargetType == TargetCharge && a.StatusCode == "ACTIVE"
                              orderby a.AllocationId
                              select new CrmChargePaymentRowDto(a.AllocationId, ch.RuleCode, t.RepaymentNumber, s.SourceType,
                                  c != null ? c.ChequeNo : s.SourceReference ?? t.BankStatementReference, a.AllocationDate, a.AllocationAmount))
            .ToListAsync(cancellationToken);

        var chosen = await db.Set<FctDisbursementChargeSource>().AsNoTracking().Where(x => x.DisbursementId == disbursementId)
            .OrderBy(x => x.SortOrder).Select(x => x.RepaymentSourceId).ToListAsync(cancellationToken);
        return new CrmChargePaymentDto(total, paid, outstanding, status, d.GlTranSetId, d.GlPostingDate, sources, payments, chosen);
    }

    /// <summary>Pay Charges: the smaller of the source's balance and the outstanding charges, paid charge by charge in order.</summary>
    public async Task<CrmChargePaymentDto> PayAsync(long disbursementId, long repaymentSourceId, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var d = await OpenRequestAsync(disbursementId, cancellationToken);
        await PayCoreAsync(d, repaymentSourceId, true, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    /// <summary>
    /// Pays the outstanding charges from one source, inside the caller's transaction; returns the amount paid. With strict false a source
    /// that is used up or no longer active pays nothing instead of refusing (Complete goes on to the next chosen source).
    /// </summary>
    internal async Task<decimal> PayCoreAsync(FctDisbursement d, long repaymentSourceId, bool strict, CancellationToken cancellationToken)
    {
        var disbursementId = d.DisbursementId;
        var source = await db.Set<FctRepaymentSource>().FirstOrDefaultAsync(x => x.RepaymentSourceId == repaymentSourceId, cancellationToken)
            ?? throw new NotFoundException($"Payment source {repaymentSourceId} was not found.");
        var txn = await db.Set<FctRepaymentTransaction>().FirstAsync(x => x.RepaymentTxnId == source.RepaymentTxnId, cancellationToken);
        if (txn.ClientId != d.ClientId)
            throw new BusinessRuleException("The payment source belongs to another customer.");
        var active = source.StatusCode == "ACTIVE" && txn.StatusCode is not ("REVERSED" or "CANCELLED");
        if (!active && strict) throw new BusinessRuleException("The payment source is not active.");
        var available = active ? source.SourceAmount - source.AllocatedAmount : 0;
        if (available <= 0 && strict) throw new BusinessRuleException("The payment source has no balance left.");

        var charges = (await db.Set<FctDisbursementCharge>().Where(x => x.DisbursementId == disbursementId).OrderBy(x => x.DisbursementChargeId)
            .ToListAsync(cancellationToken)).Where(x => Owed(x) && Outstanding(x) > 0).ToList();
        if (charges.Count == 0 && strict) throw new BusinessRuleException("The charges of this request are already paid.");
        if (available <= 0 || charges.Count == 0) return 0;
        var paidNow = 0m;

        var today = GlTime.Today(clock);
        foreach (var c in charges)
        {
            if (available <= 0) break;
            var amount = Math.Min(available, Outstanding(c));
            db.Add(new FctRepaymentAllocation
            {
                RepaymentTxnId = txn.RepaymentTxnId,
                RepaymentSourceId = source.RepaymentSourceId,
                TargetType = TargetCharge,
                TargetId = c.DisbursementChargeId,
                DisbursementId = disbursementId,
                ComponentCode = TargetCharge,
                AllocationMode = "AMOUNT",
                RequestedValue = amount,
                AllocationAmount = amount,
                AllocationDate = today,
                StatusCode = "ACTIVE",
            });
            Apply(c, source, txn, amount);
            available -= amount;
            paidNow += amount;
        }

        await db.SaveChangesAsync(cancellationToken);
        await TxnStatusAsync(txn, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return paidNow;
    }

    /// <summary>Reverse a payment: the amount goes back to its source and the charge is owed again.</summary>
    public async Task<CrmChargePaymentDto> ReverseAsync(long disbursementId, long allocationId, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        await OpenRequestAsync(disbursementId, cancellationToken);
        var a = await db.Set<FctRepaymentAllocation>().FirstOrDefaultAsync(
                    x => x.AllocationId == allocationId && x.DisbursementId == disbursementId && x.TargetType == TargetCharge, cancellationToken)
            ?? throw new NotFoundException($"Payment {allocationId} of this request was not found.");
        if (a.StatusCode != "ACTIVE") throw new BusinessRuleException("This payment is already reversed.");
        var c = await db.Set<FctDisbursementCharge>().FirstAsync(x => x.DisbursementChargeId == a.TargetId, cancellationToken);
        var source = await db.Set<FctRepaymentSource>().FirstAsync(x => x.RepaymentSourceId == a.RepaymentSourceId, cancellationToken);
        var txn = await db.Set<FctRepaymentTransaction>().FirstAsync(x => x.RepaymentTxnId == a.RepaymentTxnId, cancellationToken);

        a.StatusCode = "REVERSED";
        a.ReversalReason = "Reversed on the Invoice Upload page";
        a.ReversedBy = User;
        a.ReversedAt = clock.UtcNow;
        Apply(c, source, txn, -a.AllocationAmount);
        await db.SaveChangesAsync(cancellationToken);
        await TxnStatusAsync(txn, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    private async Task<FctDisbursement> OpenRequestAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        if (d.IsGlPosted)
            throw new BusinessRuleException($"The charges are posted to GL ({d.GlTranSetId}); their payments can no longer change.");
        return d;
    }

    private static void Apply(FctDisbursementCharge c, FctRepaymentSource s, FctRepaymentTransaction t, decimal amount)
    {
        c.CollectedAmount += amount;
        c.StatusCode = c.CollectedAmount + c.WaivedAmount >= c.TotalAmount ? "COLLECTED" : c.CollectedAmount > 0 ? "PARTIALLY_COLLECTED" : "CALCULATED";
        s.AllocatedAmount += amount;
        t.AllocatedAmount += amount;
    }

    /// <summary>The repayment's status from what its sources hold and how much of it is used.</summary>
    private async Task TxnStatusAsync(FctRepaymentTransaction t, CancellationToken cancellationToken)
    {
        var held = await db.Set<FctRepaymentSource>().Where(x => x.RepaymentTxnId == t.RepaymentTxnId && x.StatusCode == "ACTIVE")
            .SumAsync(x => (decimal?)x.SourceAmount, cancellationToken) ?? 0;
        t.StatusCode = t.AllocatedAmount <= 0 ? "DRAFT" : t.AllocatedAmount >= held ? "ALLOCATED" : "PARTIALLY_ALLOCATED";
    }
}
