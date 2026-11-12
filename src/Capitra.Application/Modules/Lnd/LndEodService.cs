using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndEodRunDto(
    long EodRunId, DateOnly RunDate, string StatusCode, int LoansCount, decimal CreditAppliedAmount, decimal InterestAccruedAmount,
    decimal InterestSuspendedAmount, decimal LateFeeAmount, int NonAccrualCount, int CuredCount, int WriteOffProposedCount,
    string? GlTranSetIds, string? ErrorMessage, DateTime StartedAt, DateTime? FinishedAt, string CreatedBy);

public sealed record LndEodLoanDto(
    long LoanId, string LoanNumber, string ClientName, string ProductCode, int DaysLate, decimal PrincipalOutstanding,
    decimal InterestSuspenseAmount, decimal LateFeeSuspenseAmount, bool IsNonAccrual, bool IsWriteOffProposed);

public sealed record LndEodPageDto(
    DateOnly Today, DateOnly? LastDoneDate, DateOnly? NextDate, bool CanRun, string? Blocked,
    IReadOnlyList<LndEodRunDto> Runs, IReadOnlyList<LndEodLoanDto> NonAccrual, IReadOnlyList<LndEodLoanDto> WriteOffProposed);

public sealed record LndEodRequestDto(DateOnly? RunDate);

/// <summary>
/// Retail Lending > End of Day (design deck; Ahmed, 11 Nov 2026). One run per business day, in order, never ahead of today.
/// <list type="number">
/// <item>Customer credit balance is applied to what fell due, as a CREDIT_BALANCE receipt of the shared Collection module, one loan at
/// a time (each its own receipt and GL).</item>
/// <item>Then, in one transaction for the day: each active loan's accrual, days late, late fees, non-accrual and suspense, cure and
/// write-off proposal (<see cref="LndEod"/>), the loan events, and one GL transaction set per product from its rule LND_EOD. Closed
/// loans still holding suspense or interest not yet accrued are settled the same day.</item>
/// </list>
/// A day that failed is run again after the cause is fixed; what step 1 applied stays applied.
/// </summary>
public sealed class LndEodService(ICapitraDbContext db, IClock clock, ILndParameters parameters, AdmProductEventRuleService rules,
    ProductActionPosting posting, LndReceiptService receipts)
{
    public const string RuleCode = "LND_EOD";

    public static readonly IReadOnlyList<string> AmountCodes =
    [
        "INTEREST_ACCRUED", "INTEREST_SUSPENDED", "LATE_FEE_CHARGED", "LATE_FEE_SUSPENDED",
        "INTEREST_TO_SUSPENSE", "INTEREST_FROM_SUSPENSE", "LATE_FEE_TO_SUSPENSE", "LATE_FEE_FROM_SUSPENSE",
    ];

    public static class Statuses
    {
        public const string Running = "RUNNING";
        public const string Done = "DONE";
        public const string Failed = "FAILED";
    }

    public async Task<LndEodPageDto> GetAsync(bool canRun, CancellationToken cancellationToken)
    {
        var today = GlTime.Today(clock);
        var runs = await db.Set<LndEodRun>().AsNoTracking().OrderByDescending(x => x.RunDate).Take(60).ToListAsync(cancellationToken);
        var lastDone = await LastDoneAsync(cancellationToken);
        var next = NextDate(lastDone, today);
        var blocked = next is null ? $"End of day is done up to today ({today:yyyy-MM-dd})." : null;
        var loans = await (from l in db.Set<LndLoan>().AsNoTracking()
                           join c in db.Set<CrmClient>().AsNoTracking() on l.ClientId equals c.ClientId
                           join p in db.Set<AdmProduct>().AsNoTracking() on l.ProductId equals p.ProductId
                           where l.StatusCode == St.Active && (l.IsNonAccrual || l.IsWriteOffProposed)
                           orderby l.DaysLate descending, l.LoanNumber
                           select new { l, c.ClientName, p.ProductCode }).ToListAsync(cancellationToken);
        var ids = loans.Select(x => x.l.LoanId).ToList();
        var principal = await db.Set<LndInstallment>().AsNoTracking().Where(x => ids.Contains(x.LoanId))
            .GroupBy(x => x.LoanId).Select(g => new { g.Key, Amount = g.Sum(x => x.PrincipalDueAmount - x.PrincipalPaidAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Amount, cancellationToken);
        var rows = loans.Select(x => new LndEodLoanDto(x.l.LoanId, x.l.LoanNumber, x.ClientName, x.ProductCode, x.l.DaysLate,
            principal.GetValueOrDefault(x.l.LoanId), x.l.InterestSuspenseAmount, x.l.LateFeeSuspenseAmount, x.l.IsNonAccrual, x.l.IsWriteOffProposed)).ToList();
        return new LndEodPageDto(today, lastDone, next, canRun && next is not null, blocked,
            [.. runs.Select(r => new LndEodRunDto(r.EodRunId, r.RunDate, r.StatusCode, r.LoansCount, r.CreditAppliedAmount, r.InterestAccruedAmount,
                r.InterestSuspendedAmount, r.LateFeeAmount, r.NonAccrualCount, r.CuredCount, r.WriteOffProposedCount, r.GlTranSetIds, r.ErrorMessage,
                r.StartedAt, r.FinishedAt, r.CreatedBy))],
            [.. rows.Where(x => x.IsNonAccrual)], [.. rows.Where(x => x.IsWriteOffProposed)]);
    }

    /// <summary>The next business day to run: the day after the last one done, or today for the first run; null when done up to today.</summary>
    public static DateOnly? NextDate(DateOnly? lastDone, DateOnly today)
    {
        var next = lastDone?.AddDays(1) ?? today;
        return next > today ? null : next;
    }

    /// <summary>Runs end of day for one business day (the next one when none is given) and returns the run.</summary>
    public async Task<long> RunAsync(LndEodRequestDto dto, CancellationToken cancellationToken)
    {
        var today = GlTime.Today(clock);
        var lastDone = await LastDoneAsync(cancellationToken);
        var day = dto.RunDate ?? NextDate(lastDone, today)
            ?? throw new BusinessRuleException($"End of day is done up to today ({today:yyyy-MM-dd}).");
        if (day > today) throw BusinessValidationException.For("runDate", "End of day cannot run for a day after today.");
        if (lastDone is { } d && day != d.AddDays(1))
            throw BusinessValidationException.For("runDate", day <= d
                ? $"End of day of {day:yyyy-MM-dd} is done already."
                : $"Run {d.AddDays(1):yyyy-MM-dd} first: days run in order.");

        var nonAccrualDays = await DaysAsync(LndParameterCatalog.Codes.NonAccrualDays, day, cancellationToken);
        var writeOffDays = await DaysAsync(LndParameterCatalog.Codes.WriteOffDays, day, cancellationToken);
        if (nonAccrualDays.Count == 0 || writeOffDays.Count == 0)
            throw new BusinessRuleException($"Set the lending parameters {LndParameterCatalog.Codes.NonAccrualDays} and {LndParameterCatalog.Codes.WriteOffDays} "
                + "in Retail Lending > Parameters before running end of day.");

        // ---- the run row, saved first so a failure is recorded
        var run = await db.Set<LndEodRun>().SingleOrDefaultAsync(x => x.RunDate == day, cancellationToken);
        if (run is { StatusCode: Statuses.Running } && run.StartedAt > clock.UtcNow.AddHours(-1))
            throw new BusinessRuleException($"End of day of {day:yyyy-MM-dd} is running. Wait for it to finish.");
        if (run is null)
        {
            run = new LndEodRun { RunDate = day };
            db.Set<LndEodRun>().Add(run);
        }
        Reset(run);
        run.StartedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        var runId = run.EodRunId;

        try
        {
            var credit = await ApplyCreditAsync(day, cancellationToken);
            db.DiscardChanges();
            await ProcessAsync(runId, day, credit, nonAccrualDays, writeOffDays, cancellationToken);
            return runId;
        }
        catch (Exception ex) when (ex is BusinessRuleException or BusinessValidationException or ConflictException or NotFoundException)
        {
            await FailAsync(runId, ex.Message, cancellationToken);
            throw new BusinessRuleException($"End of day of {day:yyyy-MM-dd} failed, nothing of step 2 was saved: {ex.Message}");
        }
    }

    /// <summary>Step 1: the customer's credit balance pays what fell due by the day, oldest loan first.</summary>
    private async Task<decimal> ApplyCreditAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var due = await (from i in db.Set<LndInstallment>().AsNoTracking()
                         join l in db.Set<LndLoan>().AsNoTracking() on i.LoanId equals l.LoanId
                         where l.StatusCode == St.Active && i.StatusCode != "PAID" && i.DueDate <= day
                         group i by new { l.LoanId, l.ClientId, l.LoanNumber } into g
                         orderby g.Key.LoanId
                         select new
                         {
                             g.Key.LoanId, g.Key.ClientId, g.Key.LoanNumber,
                             Owed = g.Sum(i => i.PrincipalDueAmount - i.PrincipalPaidAmount + i.InterestDueAmount - i.InterestPaidAmount
                                 + i.ChargesDueAmount - i.ChargesPaidAmount + i.InsuranceDueAmount - i.InsurancePaidAmount + i.LateFeeDueAmount - i.LateFeePaidAmount),
                         }).ToListAsync(cancellationToken);
        decimal applied = 0;
        foreach (var d in due.Where(x => x.Owed > 0))
        {
            var credit = await receipts.CreditAsync(d.ClientId, cancellationToken);
            var use = Math.Min(credit, d.Owed);
            if (use <= 0) continue;
            try
            {
                await receipts.ReceiveAsync(d.LoanId, new LndReceiptRequestDto(day,
                    [new LndReceiptSourceDto("CREDIT_BALANCE", null, null, use, null)], $"Credit balance applied by end of day {day:yyyy-MM-dd}."), cancellationToken);
            }
            catch (BusinessRuleException ex)
            {
                throw new BusinessRuleException($"Loan {d.LoanNumber}: the credit balance could not be applied. {ex.Message}");
            }
            applied += use;
        }
        return applied;
    }

    /// <summary>Step 2, in one transaction: every loan's day, the events, GL per product, and the run done.</summary>
    private async Task ProcessAsync(long runId, DateOnly day, decimal creditApplied, Dictionary<string, int> nonAccrualDays,
        Dictionary<string, int> writeOffDays, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var loans = await db.Set<LndLoan>()
            .Where(l => (l.LastEodDate == null || l.LastEodDate < day) && l.DisbursementDate <= day
                && (l.StatusCode == St.Active
                    || (l.StatusCode == St.Closed && (l.InterestSuspenseAmount > 0 || l.LateFeeSuspenseAmount > 0
                        || db.Set<LndInstallment>().Any(i => i.LoanId == l.LoanId && i.InterestAccruedAmount < i.InterestDueAmount)))))
            .OrderBy(l => l.LoanId).ToListAsync(cancellationToken);
        var ids = loans.Select(x => x.LoanId).ToList();
        var schedules = (await db.Set<LndInstallment>().Where(x => ids.Contains(x.LoanId)).ToListAsync(cancellationToken))
            .ToLookup(x => x.LoanId);

        var results = new List<(LndLoan Loan, LndEod.Result Result, LndLoanEvent[] Events)>();
        foreach (var loan in loans)
        {
            if (!nonAccrualDays.TryGetValue(loan.LicenceCode, out var naDays) || !writeOffDays.TryGetValue(loan.LicenceCode, out var woDays))
                throw new BusinessRuleException($"Loan {loan.LoanNumber}: licence {loan.LicenceCode} has no approved {LndParameterCatalog.Codes.NonAccrualDays} "
                    + $"or {LndParameterCatalog.Codes.WriteOffDays} on {day:yyyy-MM-dd}. Set them in Retail Lending > Parameters.");
            var r = LndEod.Process(loan, [.. schedules[loan.LoanId]], day, naDays, woDays);
            var events = Events(loan, r, day);
            db.Set<LndLoanEvent>().AddRange(events);
            results.Add((loan, r, events));
        }

        // ---- GL: one transaction set per product
        var glIds = new List<string>();
        var productIds = results.Select(x => x.Loan.ProductId).Distinct().ToList();
        var products = await db.Set<AdmProduct>().AsNoTracking().Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        foreach (var g in results.GroupBy(x => x.Loan.ProductId).OrderBy(x => products[x.Key].ProductCode))
        {
            var product = products[g.Key];
            var amounts = AmountCodes.ToDictionary(c => c, c => g.Sum(x => x.Result.Amounts[c]));
            if (amounts.Values.All(v => v == 0)) continue;
            var plan = await rules.PlanByCodeAsync(product.ProductCode, RuleCode, day, cancellationToken)
                ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {RuleCode}. Add it in Administration > Product Event Rules: "
                    + "DR interest receivable / CR interest income INTEREST_ACCRUED, and the lines of the late fee and suspense (see setup-lending-gl.sql).");
            var lines = Lines(plan, amounts);
            var currency = g.First().Loan.CurrencyCode;
            var doc = new ProductActionDocument("LND_EOD", runId.ToString(CultureInfo.InvariantCulture), $"EOD-{day:yyyyMMdd}-{product.ProductCode}", day, currency,
                $"End of day {day:yyyy-MM-dd}, product {product.ProductCode}: {g.Count()} loans");
            string glId;
            try
            {
                glId = await posting.PostLinesAsync(plan, lines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                    "Loan end of day", "3100", product.BusinessLineCode, doc, cancellationToken);
            }
            catch (ApexProcessException ex)
            {
                throw new BusinessRuleException($"GL posting of product {product.ProductCode} failed: {GlPostingApi.CleanMessage(ex.Message)}");
            }
            glIds.Add(glId);
            foreach (var e in g.SelectMany(x => x.Events)) e.GlTranSetId = glId;
        }

        var run = await db.Set<LndEodRun>().SingleAsync(x => x.EodRunId == runId, cancellationToken);
        run.StatusCode = Statuses.Done;
        run.LoansCount = results.Count;
        run.CreditAppliedAmount = creditApplied;
        run.InterestAccruedAmount = results.Sum(x => x.Result.InterestAccrued);
        run.InterestSuspendedAmount = results.Sum(x => x.Result.InterestSuspended);
        run.LateFeeAmount = results.Sum(x => x.Result.LateFeeCharged + x.Result.LateFeeSuspended);
        run.NonAccrualCount = results.Count(x => x.Result.WentNonAccrual);
        run.CuredCount = results.Count(x => x.Result.Cured);
        run.WriteOffProposedCount = results.Count(x => x.Result.NewlyProposedForWriteOff);
        run.GlTranSetIds = glIds.Count == 0 ? null : string.Join(", ", glIds);
        run.ErrorMessage = null;
        run.FinishedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    /// <summary>The loan events of the day: interest accrued, late fee, non-accrual, cure, write-off proposal.</summary>
    public static LndLoanEvent[] Events(LndLoan loan, LndEod.Result r, DateOnly day)
    {
        var list = new List<LndLoanEvent>();
        void Add(string code, decimal amount, string notes) => list.Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = code, EventDate = day, EventAmount = amount, SourceDocumentType = "LND_EOD", Notes = notes,
        });
        if (r.InterestAccrued + r.InterestSuspended > 0)
            Add("ACCRUED", r.InterestAccrued + r.InterestSuspended, r.InterestSuspended > 0 ? "Interest accrued to suspense (non-accrual)." : "Interest accrued to income.");
        if (r.LateFeeCharged + r.LateFeeSuspended > 0)
            Add("LATE_FEE", r.LateFeeCharged + r.LateFeeSuspended, $"Late fee, {r.DaysLate} days late" + (r.LateFeeSuspended > 0 ? ", to suspense." : "."));
        if (r.WentNonAccrual)
            Add("NON_ACCRUAL", r.InterestToSuspense + r.LateFeeToSuspense,
                $"{r.DaysLate} days late: non-accrual. Interest {r.InterestToSuspense:#,0.00} and late fees {r.LateFeeToSuspense:#,0.00} moved to suspense.");
        if (r.Cured)
            Add("CURED", r.InterestFromSuspense + r.LateFeeFromSuspense,
                $"No longer late: suspense released to income (interest {r.InterestFromSuspense:#,0.00}, late fees {r.LateFeeFromSuspense:#,0.00}).");
        else if (r.InterestFromSuspense + r.LateFeeFromSuspense > 0)
            Add("SUSPENSE_RELEASED", r.InterestFromSuspense + r.LateFeeFromSuspense, "Paid while in non-accrual: that part of the suspense released to income.");
        if (r.NewlyProposedForWriteOff)
            Add("WRITE_OFF_PROPOSED", 0, $"{r.DaysLate} days late: proposed for write-off.");
        return [.. list];
    }

    /// <summary>
    /// The GL lines of a product's day from its rule LND_EOD: each line takes one of the amount codes of end of day. Lines with the same
    /// account and side in one journal are added together, and each journal must balance.
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> Lines(ProductEventPlan plan, IReadOnlyDictionary<string, decimal> amounts)
    {
        var other = plan.Lines.Where(l => l.ChargeCode is not null || !AmountCodes.Contains(l.AmountCode)).Select(l => l.ChargeCode ?? l.AmountCode).Distinct().ToList();
        if (other.Count > 0) throw new BusinessRuleException($"Rule {plan.RuleCode} posts {string.Join(", ", other)}; end of day gives only {string.Join(", ", AmountCodes)}.");
        var missing = AmountCodes.Where(c => amounts.GetValueOrDefault(c) != 0 && !plan.Lines.Any(l => l.AmountCode == c)).ToList();
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

    /// <summary>A day count parameter for each licence that has one in force (MF, CF); each loan reads its own licence's.</summary>
    private async Task<Dictionary<string, int>> DaysAsync(string code, DateOnly day, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, int>();
        foreach (var scope in new[] { "MF", "CF" })
            if (await parameters.NumberAsync(code, scope, day, cancellationToken) is { } v) result[scope] = (int)v;
        return result;
    }

    private async Task<DateOnly?> LastDoneAsync(CancellationToken cancellationToken) =>
        await db.Set<LndEodRun>().AsNoTracking().Where(x => x.StatusCode == Statuses.Done).MaxAsync(x => (DateOnly?)x.RunDate, cancellationToken);

    private async Task FailAsync(long runId, string message, CancellationToken cancellationToken)
    {
        if (db.CurrentTransaction is { } open) await open.RollbackAsync(cancellationToken);
        db.DiscardChanges();
        var run = await db.Set<LndEodRun>().SingleAsync(x => x.EodRunId == runId, cancellationToken);
        run.StatusCode = Statuses.Failed;
        run.ErrorMessage = message.Length > 2000 ? message[..2000] : message;
        run.FinishedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void Reset(LndEodRun run)
    {
        run.StatusCode = Statuses.Running;
        run.LoansCount = 0;
        run.CreditAppliedAmount = 0;
        run.InterestAccruedAmount = 0;
        run.InterestSuspendedAmount = 0;
        run.LateFeeAmount = 0;
        run.NonAccrualCount = 0;
        run.CuredCount = 0;
        run.WriteOffProposedCount = 0;
        run.GlTranSetIds = null;
        run.ErrorMessage = null;
        run.FinishedAt = null;
    }
}
