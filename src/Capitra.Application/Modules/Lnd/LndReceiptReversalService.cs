using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Col;
using Capitra.Domain.Modules.Gl;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndReceiptReversalRequestDto(string? Reason);

public sealed record LndReplayPreviewDto(long RepaymentTxnId, string RepaymentNumber, DateOnly ValueDate, decimal ReceiptAmount, decimal AllocatedBefore,
    decimal AllocatedAfter, decimal CreditKeptBefore, decimal CreditKeptAfter);

public sealed record LndReversalPreviewDto(long RepaymentTxnId, string RepaymentNumber, string Kind, decimal ReceiptAmount, decimal AllocatedAmount,
    string? Blocked, bool ReopensLoan, IReadOnlyList<LndReplayPreviewDto> Replayed);

/// <summary>
/// Reversing a customer receipt on a retail loan, with replay (design deck: receipt reversal with replay; e.g. a deposit booked twice, or
/// the wrong customer). In one transaction:
/// <list type="number">
/// <item>The receipt becomes REVERSED with its reason: its sources are released (a cheque can be used again; credit balance it used goes
/// back to the receipts it came from) and its GL transaction set is reversed (posted journals get a REV- journal, others are cancelled).</item>
/// <item>Replay: the receipts after it (by value date, then entry) are applied again in order with the same money. They can only apply
/// more than before; the difference comes from what they kept as customer credit, and GL posts the change with rule LND_RECEIPT (DR
/// customer credit CREDIT_USED_AMOUNT, CR each part paid).</item>
/// <item>A closed loan that now owes again becomes ACTIVE. On a written-off loan only recoveries can be reversed; nothing is replayed.</item>
/// </list>
/// Blocked when the credit the receipt (or a replayed one) kept was used since by another receipt: reverse that one first. Late fees
/// and days late of end of day are not worked out again for past days; the next end of day takes the loan as it now stands.
/// </summary>
public sealed class LndReceiptReversalService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser, AdmProductEventRuleService rules,
    ProductActionPosting posting, GlPostingApi gl, LndLoanService loans)
{
    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private sealed record Plan(
        LndLoan Loan, ColRepaymentTransaction Txn, bool Recovery, List<LndInstallment> Schedule, List<ColRepaymentTransaction> Later,
        IReadOnlyList<LndReplay.Result> Replay, List<LndAllocation> Undone, string? Blocked);

    public async Task<LndReversalPreviewDto> PreviewAsync(long loanId, long txnId, CancellationToken cancellationToken)
    {
        var p = await PlanAsync(loanId, txnId, cancellationToken);
        db.DiscardChanges();
        return new LndReversalPreviewDto(p.Txn.RepaymentTxnId, p.Txn.RepaymentNumber, p.Recovery ? "RECOVERY" : "RECEIPT", p.Txn.ReceiptAmount, p.Txn.AllocatedAmount,
            p.Blocked, p.Loan.StatusCode == St.Closed && p.Schedule.Any(i => i.StatusCode != "PAID"),
            [.. p.Later.Zip(p.Replay).Select(x => new LndReplayPreviewDto(x.First.RepaymentTxnId, x.First.RepaymentNumber, x.First.ValueDate, x.First.ReceiptAmount,
                x.First.AllocatedAmount, x.Second.Allocated, x.First.ReceiptAmount - x.First.AllocatedAmount, x.First.ReceiptAmount - x.Second.Allocated))]);
    }

    public async Task ReverseAsync(long loanId, long txnId, LndReceiptReversalRequestDto dto, CancellationToken cancellationToken)
    {
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? throw BusinessValidationException.For("reason", "Give the reason for the reversal.") : dto.Reason.Trim();
        if (reason.Length > 500) throw BusinessValidationException.For("reason", "The reason is longer than 500 characters.");
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var p = await PlanAsync(loanId, txnId, cancellationToken);
        if (p.Blocked is not null) throw new BusinessRuleException(p.Blocked);
        var today = GlTime.Today(clock);
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == p.Loan.ProductId, cancellationToken);

        // ---- 1. the receipt itself
        var t = p.Txn;
        t.StatusCode = "REVERSED";
        t.ReversalReason = reason;
        t.ReversedBy = User;
        t.ReversedAt = clock.UtcNow;
        await ReleaseSourcesAsync(t, cancellationToken);
        var revJournals = await ReverseGlAsync(t.GlTranSetId, today, $"Reversal of receipt {t.RepaymentNumber}: {reason}", cancellationToken);
        foreach (var a in p.Undone) a.StatusCode = "REVERSED";
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loanId, EventCode = p.Recovery ? "RECOVERY_REVERSED" : "RECEIPT_REVERSED", EventDate = today,
            EventAmount = p.Recovery ? -t.AllocatedAmount : t.ReceiptAmount, GlTranSetId = t.GlTranSetId, SourceDocumentType = "COL_REPAYMENT", SourceDocumentId = t.RepaymentTxnId,
            Notes = Cut($"{t.RepaymentNumber} of {t.ReceiptAmount:#,0.00} reversed by {User}: {reason}. GL: {(revJournals.Count == 0 ? "nothing posted" : string.Join(", ", revJournals))}.", 1000),
        });

        // ---- 2. replay of the later receipts
        foreach (var (later, r) in p.Later.Zip(p.Replay))
        {
            foreach (var l in r.Lines)
                db.Set<LndAllocation>().Add(new LndAllocation
                {
                    RepaymentTxnId = later.RepaymentTxnId, LoanId = loanId, InstallmentId = l.InstallmentId, PartCode = l.PartCode, AllocatedAmount = l.Amount, ValueDate = later.ValueDate,
                });
            if (!r.Changed) continue;
            var more = r.Allocated - later.AllocatedAmount;
            await TakeKeptCreditAsync(later, more, cancellationToken);
            later.AllocatedAmount = r.Allocated;
            var adjustGl = await AdjustGlAsync(product, p.Loan, later, r.Delta, today, cancellationToken);
            db.Set<LndLoanEvent>().Add(new LndLoanEvent
            {
                LoanId = loanId, EventCode = "RECEIPT_REPLAYED", EventDate = today, EventAmount = r.Allocated, GlTranSetId = adjustGl,
                SourceDocumentType = "COL_REPAYMENT", SourceDocumentId = later.RepaymentTxnId,
                Notes = $"{later.RepaymentNumber} applied again after the reversal of {t.RepaymentNumber}: applied {r.Allocated:#,0.00} (was {r.Allocated - more:#,0.00}), "
                    + $"credit kept {later.ReceiptAmount - r.Allocated:#,0.00}.",
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        if (p.Loan.StatusCode == St.Closed && p.Schedule.Any(i => i.StatusCode != "PAID"))
            await loans.ChangeStatusAsync(loanId, St.Active, Cut($"Receipt {t.RepaymentNumber} reversed: the loan owes again.", 500), null, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    /// <summary>Works out the reversal and replay on tracked entities (nothing saved).</summary>
    private async Task<Plan> PlanAsync(long loanId, long txnId, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        if (await db.Set<LndLoanEvent>().AsNoTracking().AnyAsync(x => x.LoanId == loanId && x.SourceDocumentId == txnId
                && (x.EventCode == "PREPAYMENT" || x.EventCode == "SETTLEMENT"), cancellationToken))
            throw new BusinessRuleException("A part prepayment or early settlement changed the schedule and cannot be reversed here.");
        var ev = await db.Set<LndLoanEvent>().AsNoTracking()
            .Where(x => x.LoanId == loanId && x.SourceDocumentId == txnId && (x.EventCode == "RECEIPT" || x.EventCode == "RECOVERY")).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Receipt {txnId} is not a receipt of this loan.");
        var txn = await db.Set<ColRepaymentTransaction>().SingleAsync(x => x.RepaymentTxnId == txnId, cancellationToken);
        var recovery = ev.EventCode == "RECOVERY";
        var schedule = await db.Set<LndInstallment>().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);

        string? blocked = txn.StatusCode != "POSTED" ? $"Receipt {txn.RepaymentNumber} is {txn.StatusCode.ToLowerInvariant()}; only a posted receipt can be reversed."
            : loan.StatusCode == St.WrittenOff && !recovery ? "The loan is written off: only recoveries received after the write-off can be reversed."
            : loan.StatusCode is not (St.Active or St.Closed or St.WrittenOff) ? $"The loan is {loan.StatusCode.ToLowerInvariant()}."
            : await CreditUsedByAsync(txnId, cancellationToken) is { } user ? $"The credit kept by {txn.RepaymentNumber} was used by receipt {user}. Reverse {user} first."
            : await EarlyPaidSinceAsync(loanId, txn.ValueDate, cancellationToken)
                ? "A part prepayment or early settlement was made on or after this receipt's value date and rebuilt the schedule: it cannot be reversed here."
            : null;
        if (blocked is not null || recovery)
            return new Plan(loan, txn, recovery, schedule, [], [], [], blocked);

        // ---- take the receipt and the later ones off the instalments
        var receiptIds = await db.Set<LndLoanEvent>().AsNoTracking()
            .Where(x => x.LoanId == loanId && x.EventCode == "RECEIPT" && x.SourceDocumentId != null).Select(x => x.SourceDocumentId!.Value).ToListAsync(cancellationToken);
        var later = (await db.Set<ColRepaymentTransaction>().Where(x => receiptIds.Contains(x.RepaymentTxnId) && x.StatusCode == "POSTED"
                && x.RepaymentTxnId != txnId && (x.ValueDate > txn.ValueDate || (x.ValueDate == txn.ValueDate && x.RepaymentTxnId > txnId)))
            .ToListAsync(cancellationToken)).OrderBy(x => x.ValueDate).ThenBy(x => x.RepaymentTxnId).ToList();
        var ids = later.Select(x => x.RepaymentTxnId).Append(txnId).ToList();
        var allocations = await db.Set<LndAllocation>().Where(x => ids.Contains(x.RepaymentTxnId) && x.StatusCode == "ACTIVE").ToListAsync(cancellationToken);
        var byId = schedule.ToDictionary(x => x.InstallmentId);
        foreach (var a in allocations) LndReplay.Pay(byId[a.InstallmentId], a.PartCode, -a.AllocatedAmount);
        foreach (var i in schedule) LndReplay.SetStatus(i, null);

        var replay = LndReplay.Run(schedule, LndProductTerms.Parts(loan.PaymentOrder),
            [.. later.Select(l => new LndReplay.Receipt(l.RepaymentTxnId, l.ValueDate, l.ReceiptAmount,
                LndReceiptService.Parts.ToDictionary(p => p, p => allocations.Where(a => a.RepaymentTxnId == l.RepaymentTxnId && a.PartCode == p).Sum(a => a.AllocatedAmount))))]);

        // a later receipt pays more only from what it kept as credit and nobody used since
        foreach (var (l, r) in later.Zip(replay))
        {
            var more = r.Allocated - l.AllocatedAmount;
            if (more <= 0) continue;
            var free = await db.Set<ColRepaymentSource>().Where(s => s.RepaymentTxnId == l.RepaymentTxnId && s.StatusCode == "ACTIVE" && s.SourceType != "CREDIT_BALANCE")
                .SumAsync(s => s.SourceAmount - s.AllocatedAmount, cancellationToken);
            if (free < more)
            {
                blocked = $"Replaying receipt {l.RepaymentNumber} needs {more:#,0.00} of the credit it kept, but only {free:#,0.00} is left: the rest was used by another "
                    + "receipt. Reverse that receipt first.";
                break;
            }
        }
        return new Plan(loan, txn, false, schedule, later, replay, allocations.Where(a => a.RepaymentTxnId == txnId || later.Any(l => l.RepaymentTxnId == a.RepaymentTxnId)).ToList(), blocked);
    }

    /// <summary>Whether a part prepayment or early settlement of the loan has a value date on or after the day.</summary>
    private Task<bool> EarlyPaidSinceAsync(long loanId, DateOnly day, CancellationToken cancellationToken) =>
        (from e in db.Set<LndLoanEvent>().AsNoTracking()
         join t in db.Set<ColRepaymentTransaction>().AsNoTracking() on e.SourceDocumentId equals (long?)t.RepaymentTxnId
         where e.LoanId == loanId && (e.EventCode == "PREPAYMENT" || e.EventCode == "SETTLEMENT") && t.StatusCode == "POSTED" && t.ValueDate >= day
         select e).AnyAsync(cancellationToken);

    /// <summary>The receipt that used credit kept by this one, if any (still posted).</summary>
    private async Task<string?> CreditUsedByAsync(long txnId, CancellationToken cancellationToken) =>
        await (from s in db.Set<ColRepaymentSource>().AsNoTracking()
               join t in db.Set<ColRepaymentTransaction>().AsNoTracking() on s.RepaymentTxnId equals t.RepaymentTxnId
               where s.SourceRepaymentTxnId == txnId && s.StatusCode == "ACTIVE" && t.StatusCode == "POSTED"
               select t.RepaymentNumber).FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// The receipt's sources are released: bank deposits and cheques stop counting (a cheque is free again); credit it used goes back to
    /// the receipts it came from.
    /// </summary>
    private async Task ReleaseSourcesAsync(ColRepaymentTransaction t, CancellationToken cancellationToken)
    {
        var sources = await db.Set<ColRepaymentSource>().Where(x => x.RepaymentTxnId == t.RepaymentTxnId && x.StatusCode == "ACTIVE").ToListAsync(cancellationToken);
        foreach (var s in sources)
        {
            s.StatusCode = "REVERSED";
            if (s.SourceType != "CREDIT_BALANCE" || s.SourceRepaymentTxnId is not { } fromId) continue;
            var from = await db.Set<ColRepaymentTransaction>().SingleAsync(x => x.RepaymentTxnId == fromId, cancellationToken);
            from.AllocatedAmount -= s.SourceAmount;
            var back = s.SourceAmount;
            foreach (var o in (await db.Set<ColRepaymentSource>().Where(x => x.RepaymentTxnId == fromId && x.StatusCode == "ACTIVE" && x.SourceType != "CREDIT_BALANCE")
                         .ToListAsync(cancellationToken)).OrderByDescending(x => x.RepaymentSourceId))
            {
                if (back <= 0) break;
                var give = Math.Min(back, o.AllocatedAmount);
                o.AllocatedAmount -= give;
                back -= give;
            }
        }
    }

    /// <summary>A replayed receipt pays more: that much of what it kept as credit is now allocated, its own sources first in order.</summary>
    private async Task TakeKeptCreditAsync(ColRepaymentTransaction t, decimal more, CancellationToken cancellationToken)
    {
        if (more <= 0) return;
        foreach (var s in (await db.Set<ColRepaymentSource>().Where(x => x.RepaymentTxnId == t.RepaymentTxnId && x.StatusCode == "ACTIVE" && x.SourceType != "CREDIT_BALANCE")
                     .ToListAsync(cancellationToken)).OrderBy(x => x.RepaymentSourceId))
        {
            if (more <= 0) break;
            var take = Math.Min(more, s.SourceAmount - s.AllocatedAmount);
            s.AllocatedAmount += take;
            more -= take;
        }
        if (more > 0) throw new BusinessRuleException($"Receipt {t.RepaymentNumber} has not enough credit kept to be applied again. Reload the page.");
    }

    /// <summary>GL of a replayed receipt that now pays differently: DR customer credit / CR each part paid more (or the reverse).</summary>
    private async Task<string?> AdjustGlAsync(AdmProduct product, LndLoan loan, ColRepaymentTransaction t, IReadOnlyDictionary<string, decimal> delta, DateOnly today,
        CancellationToken cancellationToken)
    {
        var plan = await rules.PlanByCodeAsync(product.ProductCode, LndReceiptService.RuleCode, today, cancellationToken)
            ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {LndReceiptService.RuleCode}.");
        var lines = AdjustmentLines(plan, delta);
        if (lines.Count == 0) return null;
        var doc = new ProductActionDocument("LND_RECEIPT", t.RepaymentTxnId.ToString(CultureInfo.InvariantCulture), t.RepaymentNumber + "-R", today, loan.CurrencyCode,
            $"Loan {loan.LoanNumber}: receipt {t.RepaymentNumber} applied again after a reversal");
        try
        {
            return await posting.PostLinesAsync(plan, lines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                "Loan receipt replay", "3100", product.BusinessLineCode, doc, cancellationToken);
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting of the replay of {t.RepaymentNumber} failed, nothing was reversed: {GlPostingApi.CleanMessage(ex.Message)}");
        }
    }

    /// <summary>
    /// The GL lines of a replay from the receipt rule: parts paid more are DR customer credit (CREDIT_USED_AMOUNT) / CR the part; parts
    /// paid less the other way round. Lines on one account and journal are netted.
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> AdjustmentLines(ProductEventPlan plan, IReadOnlyDictionary<string, decimal> delta)
    {
        if (!plan.Lines.Any(l => l.AmountCode == "CREDIT_USED_AMOUNT"))
            throw new BusinessRuleException($"Rule {plan.RuleCode} has no CREDIT_USED_AMOUNT line (DR customer credit), needed to apply a receipt again.");
        var plus = delta.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);
        var minus = delta.Where(x => x.Value < 0).ToDictionary(x => x.Key, x => -x.Value);
        var all = new List<ProductEventPostingLine>();
        if (plus.Count > 0) all.AddRange(LndReceiptService.Lines(plan, plus, [], plus.Values.Sum(), 0));
        if (minus.Count > 0) all.AddRange(LndReceiptService.Lines(plan, minus, [], minus.Values.Sum(), 0).Select(l => l with { Dr = l.Cr, Cr = l.Dr }));
        return [.. all.GroupBy(l => (l.JournalNo, l.Account))
            .Select(g => (g.First(), Net: g.Sum(x => x.Dr) - g.Sum(x => x.Cr)))
            .Where(x => x.Net != 0)
            .Select(x => x.Item1 with { Dr = x.Net > 0 ? x.Net : 0, Cr = x.Net < 0 ? -x.Net : 0 })];
    }

    /// <summary>Reverses each journal of a GL transaction set: posted ones get a REV- journal on the day, the others are cancelled.</summary>
    private async Task<List<string>> ReverseGlAsync(string? tranSetId, DateOnly day, string reason, CancellationToken cancellationToken)
    {
        var done = new List<string>();
        if (string.IsNullOrWhiteSpace(tranSetId)) return done;
        var journals = await db.Set<GlJournalHeader>().AsNoTracking()
            .Where(h => h.ReferenceNo == tranSetId && !h.IsReversal && h.JournalStatus != "CANCELLED").OrderBy(h => h.JournalHeaderId).ToListAsync(cancellationToken);
        try
        {
            foreach (var h in journals)
            {
                if (h.PostingStatus == "POSTED")
                {
                    await gl.ReverseJournalAsync(h.JournalHeaderId, day, Cut(reason, 4000), cancellationToken);
                    done.Add("REV-" + h.JournalNo);
                }
                else
                {
                    await gl.CancelJournalAsync(h.JournalHeaderId, Cut(reason, 4000), cancellationToken);
                    done.Add(h.JournalNo + " cancelled");
                }
            }
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL reversal failed, nothing was reversed: {GlPostingApi.CleanMessage(ex.Message)}");
        }
        return done;
    }

    private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;
}
