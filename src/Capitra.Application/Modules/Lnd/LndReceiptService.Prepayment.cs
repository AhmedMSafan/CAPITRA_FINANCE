using System.Globalization;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Col;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndPrepaymentLineDto(int InstallmentNo, DateOnly DueDate, decimal Principal, decimal Interest, decimal Total, decimal ClosingPrincipal);

public sealed record LndPrepaymentQuoteDto(
    string Kind, DateOnly Date, decimal DuesAmount, decimal InterestToDate, decimal PrincipalAmount, decimal FeeAmount, decimal TotalAmount,
    decimal PrincipalNotDue, string RecalcCode, decimal FeePct, IReadOnlyList<LndPrepaymentLineDto> NewSchedule, string? Problem);

public sealed record LndPrepaymentRequestDto(string? Kind, decimal? PrincipalAmount, IReadOnlyList<LndReceiptSourceDto>? Sources, string? Notes);

public sealed partial class LndReceiptService
{
    public async Task<LndPrepaymentQuoteDto> QuoteAsync(long loanId, string? kind, decimal? principal, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var schedule = await db.Set<LndInstallment>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        var q = LndPrepayment.Make(loan, schedule, (kind ?? "").Trim().ToUpperInvariant(), principal, GlTime.Today(clock));
        return ToDto(loan, q);
    }

    /// <summary>
    /// Receives a part prepayment or an early settlement on today's date (sources as for a receipt; money beyond the quote stays as
    /// credit). In one transaction: the receipt in the Collection module, the allocations, the instalments (paid, or rebuilt), the
    /// loan (closed on settlement), the loan event, and GL from rule LND_RECEIPT with the fee as PREPAYMENT_FEE_AMOUNT.
    /// </summary>
    public async Task<long> PrepayAsync(long loanId, LndPrepaymentRequestDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var loan = await db.Set<LndLoan>().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var today = GlTime.Today(clock);
        var schedule = await db.Set<LndInstallment>().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        var kind = (dto.Kind ?? "").Trim().ToUpperInvariant();
        var q = LndPrepayment.Make(loan, schedule, kind, dto.PrincipalAmount, today);
        if (q.Problem is not null) throw new BusinessRuleException(q.Problem);
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, cancellationToken);

        var sources = await CheckSourcesAsync(loan, dto.Sources ?? [], cancellationToken);
        var amount = sources.Sum(x => x.Amount);
        if (amount < q.TotalAmount)
            throw BusinessValidationException.For("sources", $"The {(kind == LndPrepayment.Settle ? "settlement" : "prepayment")} needs {q.TotalAmount:#,0.00}; the sources give {amount:#,0.00}.");
        var creditUsed = sources.Where(x => x.Type == "CREDIT_BALANCE").Sum(x => x.Amount);
        if (creditUsed > q.TotalAmount) throw BusinessValidationException.For("sources", "The credit balance used is more than is needed. Use less credit.");

        var txn = new ColRepaymentTransaction
        {
            BusinessLineCode = product.BusinessLineCode, RepaymentNumber = "PENDING-" + Guid.NewGuid().ToString("N").ToUpperInvariant(), ClientId = loan.ClientId,
            LegalEntityId = LegalEntityId, RepaymentMethod = sources.Select(x => x.Type).Distinct().Count() == 1 ? sources[0].Type : "MIXED",
            TransactionDate = today, ValueDate = today, CurrencyCode = loan.CurrencyCode, ReceiptAmount = amount, AllocatedAmount = q.TotalAmount, StatusCode = "POSTED",
            ExternalReference = loan.LoanNumber, Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
        };
        db.Set<ColRepaymentTransaction>().Add(txn);
        await db.SaveChangesAsync(cancellationToken);
        txn.RepaymentNumber = $"RPT-{today:yyyyMM}-{txn.RepaymentTxnId:D8}";

        // ---- the instalments and allocations
        var lines = kind == LndPrepayment.Settle ? Settle(loan, schedule, q, today) : Prepay(schedule, q, today);
        foreach (var l in lines)
            db.Set<LndAllocation>().Add(new LndAllocation
            {
                RepaymentTxnId = txn.RepaymentTxnId, LoanId = loanId, InstallmentId = l.InstallmentId, PartCode = l.PartCode, AllocatedAmount = l.Amount, ValueDate = today,
            });
        if (kind == LndPrepayment.Prepay)
        {
            foreach (var extra in schedule.Where(i => i.DueDate > today).Skip(q.NewSchedule.Count).ToList())
            {
                db.Set<LndInstallment>().Remove(extra);
                schedule.Remove(extra);
            }
            loan.InstallmentCount = schedule.Count;
            loan.MaturityDate = schedule[^1].DueDate;
            if (loan.PrepaymentRecalcCode == "REDUCE_INSTALMENT" && q.NewSchedule.Count > 1) loan.InstallmentAmount = q.NewSchedule[1].Total;
            loan.TotalInterestAmount = schedule.Sum(i => i.InterestDueAmount);
        }

        // ---- sources: credit first, then the new money in the order entered
        var toAllocate = q.TotalAmount;
        foreach (var s in sources.OrderBy(x => x.Type == "CREDIT_BALANCE" ? 0 : 1))
        {
            var used = Math.Min(s.Amount, toAllocate);
            toAllocate -= used;
            if (s.Type == "CREDIT_BALANCE")
            {
                await UseCreditAsync(txn, loan.ClientId, s.Amount, cancellationToken);
                continue;
            }
            db.Set<ColRepaymentSource>().Add(new ColRepaymentSource
            {
                RepaymentTxnId = txn.RepaymentTxnId, SourceType = s.Type, ChequeId = s.ChequeId, SourceReference = s.Reference, SourceAmount = s.Amount, AllocatedAmount = used,
            });
        }

        // ---- GL
        var plan = await rules.PlanByCodeAsync(product.ProductCode, RuleCode, today, cancellationToken)
            ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {RuleCode}.");
        var paid = Parts.ToDictionary(p => p, p => lines.Where(l => l.PartCode == p).Sum(l => l.Amount));
        var banks = sources.Where(x => x.Type != "CREDIT_BALANCE").GroupBy(x => x.GlAccount!).Select(g => (g.Key, g.Sum(x => x.Amount))).ToList();
        var glLines = Lines(plan, paid, banks, creditUsed, amount - q.TotalAmount, null, q.FeeAmount);
        var what = kind == LndPrepayment.Settle ? "early settlement" : "part prepayment";
        var doc = new ProductActionDocument("LND_RECEIPT", txn.RepaymentTxnId.ToString(CultureInfo.InvariantCulture), txn.RepaymentNumber, today, loan.CurrencyCode,
            $"Loan {loan.LoanNumber}: {what} {txn.RepaymentNumber} of {q.TotalAmount:#,0.00}");
        try
        {
            txn.GlTranSetId = await posting.PostLinesAsync(plan, glLines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                "Loan " + what, "3100", product.BusinessLineCode, doc, cancellationToken);
            txn.IsGlPosted = true;
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was received: {GlPostingApi.CleanMessage(ex.Message)}");
        }

        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loanId, EventCode = kind == LndPrepayment.Settle ? "SETTLEMENT" : "PREPAYMENT", EventDate = today, EventAmount = q.TotalAmount,
            GlTranSetId = txn.GlTranSetId, SourceDocumentType = "COL_REPAYMENT", SourceDocumentId = txn.RepaymentTxnId,
            Notes = kind == LndPrepayment.Settle
                ? $"{txn.RepaymentNumber}: settled. Dues {q.DuesAmount:#,0.00}, interest to date {q.InterestToDate:#,0.00}, principal {q.PrincipalAmount:#,0.00}, fee {q.FeeAmount:#,0.00}."
                : $"{txn.RepaymentNumber}: principal {q.PrincipalAmount:#,0.00} paid early, interest to date {q.InterestToDate:#,0.00}, fee {q.FeeAmount:#,0.00}. "
                    + $"{(loan.PrepaymentRecalcCode == "REDUCE_TERM" ? "Shorter term" : "Lower instalment")}: {q.NewSchedule.Count} instalments left.",
        });
        await db.SaveChangesAsync(cancellationToken);
        if (kind == LndPrepayment.Settle)
            await loans.ChangeStatusAsync(loanId, St.Closed, $"Settled early by {txn.RepaymentNumber}.", null, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return txn.RepaymentTxnId;
    }

    /// <summary>Settlement: the dues in the payment order, then the current interest to date and all principal; later interest and not-yet-due charges are not owed.</summary>
    public static List<LndAllocationLine> Settle(LndLoan loan, IReadOnlyList<LndInstallment> schedule, LndPrepayment.Quote q, DateOnly day)
    {
        var owed = schedule.Where(i => i.DueDate <= day && Owed(i).Values.Sum() > 0)
            .Select(i => new LndInstallmentOwed(i.InstallmentId, i.InstallmentNo, i.DueDate, Owed(i))).ToList();
        var (lines, _) = LndAllocator.Allocate(owed, LndProductTerms.Parts(loan.PaymentOrder), q.DuesAmount, day);
        var result = lines.ToList();
        var byId = schedule.ToDictionary(x => x.InstallmentId);
        foreach (var l in lines) LndReplay.Pay(byId[l.InstallmentId], l.PartCode, l.Amount);
        foreach (var i in schedule.Where(i => i.DueDate > day))
        {
            var interest = i.InstallmentNo == q.CurrentInstallmentNo ? q.InterestToDate : 0;
            var principal = i.PrincipalDueAmount - i.PrincipalPaidAmount;
            if (interest > 0) result.Add(new LndAllocationLine(i.InstallmentId, i.InstallmentNo, "INTEREST", interest));
            if (principal > 0) result.Add(new LndAllocationLine(i.InstallmentId, i.InstallmentNo, "PRINCIPAL", principal));
            i.InterestPaidAmount += interest;
            i.InterestDueAmount = i.InterestPaidAmount;
            i.PrincipalPaidAmount = i.PrincipalDueAmount;
            i.ChargesDueAmount = i.ChargesPaidAmount;
            i.InsuranceDueAmount = i.InsurancePaidAmount;
        }
        foreach (var i in schedule) LndReplay.SetStatus(i, day);
        return result;
    }

    /// <summary>Part prepayment: interest to date and the principal paid early on the current instalment, then the rest rebuilt.</summary>
    public static List<LndAllocationLine> Prepay(IReadOnlyList<LndInstallment> schedule, LndPrepayment.Quote q, DateOnly day)
    {
        var rest = schedule.Where(i => i.DueDate > day).OrderBy(i => i.InstallmentNo).ToList();
        var current = rest[0];
        var result = new List<LndAllocationLine>();
        if (q.InterestToDate > 0) result.Add(new LndAllocationLine(current.InstallmentId, current.InstallmentNo, "INTEREST", q.InterestToDate));
        result.Add(new LndAllocationLine(current.InstallmentId, current.InstallmentNo, "PRINCIPAL", q.PrincipalAmount));
        for (var k = 0; k < q.NewSchedule.Count; k++)
        {
            var i = rest[k];
            var line = q.NewSchedule[k];
            if (k == 0)
            {
                i.InterestPaidAmount += q.InterestToDate;
                i.PrincipalPaidAmount += q.PrincipalAmount;
                i.InterestDueAmount = i.InterestPaidAmount + line.Interest;
                i.PrincipalDueAmount = i.PrincipalPaidAmount + line.Principal;
            }
            else
            {
                i.OpeningPrincipalAmount = line.OpeningPrincipal;
                i.InterestDueAmount = line.Interest;
                i.PrincipalDueAmount = line.Principal;
            }
            i.ClosingPrincipalAmount = line.ClosingPrincipal;
            LndReplay.SetStatus(i, day);
        }
        return result;
    }

    private static LndPrepaymentQuoteDto ToDto(LndLoan loan, LndPrepayment.Quote q) =>
        new(q.Kind, q.Date, q.DuesAmount, q.InterestToDate, q.PrincipalAmount, q.FeeAmount, q.TotalAmount, q.PrincipalNotDue, loan.PrepaymentRecalcCode,
            q.Kind == LndPrepayment.Settle ? loan.EarlySettlementFeePct : loan.PrepaymentFeePct,
            [.. q.NewSchedule.Select(l => new LndPrepaymentLineDto(l.InstallmentNo + (q.CurrentInstallmentNo ?? 1) - 1, l.DueDate, l.Principal, l.Interest, l.Total, l.ClosingPrincipal))], q.Problem);
}
