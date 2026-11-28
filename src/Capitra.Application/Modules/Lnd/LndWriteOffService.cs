using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndWriteOffFiguresDto(
    int DaysLate, decimal PrincipalAmount, decimal InterestAmount, decimal InterestSuspenseAmount, decimal LateFeeAmount,
    decimal LateFeeSuspenseAmount, decimal ChargesAmount, decimal InsuranceAmount, decimal TotalAmount);

public sealed record LndWriteOffDto(
    long WriteOffId, string StatusCode, string Reason, LndWriteOffFiguresDto Figures, string RequestedBy, DateTime RequestedAt,
    string? DecidedBy, DateTime? DecidedAt, string? DecisionNotes, DateOnly? WriteOffDate, string? GlTranSetId,
    bool CanApprove, string? ApproveBlockedReason, bool CanWithdraw);

public sealed record LndWriteOffsPageDto(
    long LoanId, string LoanStatusCode, bool IsWriteOffProposed, bool IsNonAccrual, DateOnly? LastEodDate, DateOnly Today,
    LndWriteOffFiguresDto Figures, bool CanRequest, string? RequestBlockedReason, IReadOnlyList<LndWriteOffDto> Requests);

public sealed record LndWriteOffRequestDto(string? Reason);

public sealed record LndWriteOffDecisionDto(string? Notes);

/// <summary>
/// Writing off a retail loan (design deck: write-off after Y days late; Ahmed, 11 Nov 2026: in this phase). End of day proposes loans
/// at LND.WRITE_OFF_DAYS; a user asks for the write-off with a reason, and another user approves or rejects it (four eyes). On approval,
/// in one transaction: the amounts are worked out again for the day, GL posts the product's rule LND_WRITE_OFF, the unpaid instalments
/// and the loan become WRITTEN_OFF, and the suspense is cleared. Interest not accrued yet (future periods) is never on the books, so it
/// is not written off.
/// </summary>
public sealed class LndWriteOffService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser, AdmProductEventRuleService rules,
    ProductActionPosting posting, LndLoanService loans)
{
    public const string RuleCode = "LND_WRITE_OFF";

    public static readonly IReadOnlyList<string> AmountCodes =
    [
        "PRINCIPAL_WRITTEN_OFF", "INTEREST_WRITTEN_OFF", "INTEREST_SUSPENSE_WRITTEN_OFF", "LATE_FEE_WRITTEN_OFF",
        "LATE_FEE_SUSPENSE_WRITTEN_OFF", "CHARGES_WRITTEN_OFF", "INSURANCE_WRITTEN_OFF",
    ];

    public static class Statuses
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
    }

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// What a write-off on the day takes off the books: all principal still owed; the interest and late fees accrued and not paid, split
    /// between what is in suspense and what was taken to income; and the charges and insurance of instalments due by the day.
    /// </summary>
    public static LndWriteOffFiguresDto Figures(LndLoan loan, IReadOnlyList<LndInstallment> schedule, DateOnly day)
    {
        var principal = schedule.Sum(i => i.PrincipalDueAmount - i.PrincipalPaidAmount);
        var interest = LndEod.UnpaidAccrued(schedule);
        var interestSusp = Math.Min(loan.InterestSuspenseAmount, interest);
        var fee = LndEod.UnpaidLateFee(schedule);
        var feeSusp = Math.Min(loan.LateFeeSuspenseAmount, fee);
        var due = schedule.Where(i => i.DueDate <= day).ToList();
        var charges = due.Sum(i => i.ChargesDueAmount - i.ChargesPaidAmount);
        var insurance = due.Sum(i => i.InsuranceDueAmount - i.InsurancePaidAmount);
        return new LndWriteOffFiguresDto(LndEod.DaysLate(schedule, day), principal, interest - interestSusp, interestSusp, fee - feeSusp, feeSusp,
            charges, insurance, principal + interest + fee + charges + insurance);
    }

    public static IReadOnlyDictionary<string, decimal> Amounts(LndWriteOffFiguresDto f) => new Dictionary<string, decimal>
    {
        ["PRINCIPAL_WRITTEN_OFF"] = f.PrincipalAmount, ["INTEREST_WRITTEN_OFF"] = f.InterestAmount,
        ["INTEREST_SUSPENSE_WRITTEN_OFF"] = f.InterestSuspenseAmount, ["LATE_FEE_WRITTEN_OFF"] = f.LateFeeAmount,
        ["LATE_FEE_SUSPENSE_WRITTEN_OFF"] = f.LateFeeSuspenseAmount, ["CHARGES_WRITTEN_OFF"] = f.ChargesAmount,
        ["INSURANCE_WRITTEN_OFF"] = f.InsuranceAmount,
    };

    public async Task<LndWriteOffsPageDto> GetAsync(long loanId, bool canRequest, bool canApprove, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var today = GlTime.Today(clock);
        var schedule = await db.Set<LndInstallment>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        var requests = await db.Set<LndWriteOff>().AsNoTracking().Where(x => x.LoanId == loanId).OrderByDescending(x => x.WriteOffId).ToListAsync(cancellationToken);
        var pending = requests.Any(x => x.StatusCode == Statuses.Pending);
        var blocked = loan.StatusCode != St.Active ? "Only an active loan can be written off."
            : pending ? "A write-off request is waiting for a decision." : null;
        return new LndWriteOffsPageDto(loanId, loan.StatusCode, loan.IsWriteOffProposed, loan.IsNonAccrual, loan.LastEodDate, today,
            Figures(loan, schedule, today), canRequest && blocked is null, blocked,
            [.. requests.Select(r =>
            {
                var approveBlocked = r.StatusCode != Statuses.Pending ? null
                    : SameUser(r.RequestedBy) ? "You asked for this write-off. Another user must approve it."
                    : loan.StatusCode != St.Active ? "The loan is no longer active." : null;
                return new LndWriteOffDto(r.WriteOffId, r.StatusCode, r.Reason, ToFigures(r), r.RequestedBy, r.RequestedAt, r.DecidedBy, r.DecidedAt,
                    r.DecisionNotes, r.WriteOffDate, r.GlTranSetId, canApprove && r.StatusCode == Statuses.Pending && approveBlocked is null, approveBlocked,
                    r.StatusCode == Statuses.Pending && SameUser(r.RequestedBy));
            })]);
    }

    public async Task<long> RequestAsync(long loanId, LndWriteOffRequestDto dto, CancellationToken cancellationToken)
    {
        var reason = Blank(dto.Reason) ?? throw BusinessValidationException.For("reason", "Give the reason for the write-off.");
        if (reason.Length > 1000) throw BusinessValidationException.For("reason", "The reason is longer than 1,000 characters.");
        var loan = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        if (loan.StatusCode != St.Active) throw new BusinessRuleException("Only an active loan can be written off.");
        if (await db.Set<LndWriteOff>().AnyAsync(x => x.LoanId == loanId && x.StatusCode == Statuses.Pending, cancellationToken))
            throw new BusinessRuleException("A write-off request for this loan is already waiting for a decision.");
        var schedule = await db.Set<LndInstallment>().AsNoTracking().Where(x => x.LoanId == loanId).ToListAsync(cancellationToken);
        var w = new LndWriteOff { LoanId = loanId, Reason = reason, RequestedBy = User, RequestedAt = clock.UtcNow };
        Apply(w, Figures(loan, schedule, GlTime.Today(clock)));
        db.Set<LndWriteOff>().Add(w);
        await db.SaveChangesAsync(cancellationToken);
        return w.WriteOffId;
    }

    public async Task ApproveAsync(long loanId, long writeOffId, LndWriteOffDecisionDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var w = await PendingAsync(loanId, writeOffId, cancellationToken);
        if (SameUser(w.RequestedBy)) throw new BusinessRuleException("You asked for this write-off. Another user must approve it.");
        var loan = await db.Set<LndLoan>().SingleAsync(x => x.LoanId == loanId, cancellationToken);
        if (loan.StatusCode != St.Active) throw new BusinessRuleException("The loan is no longer active. Reject the request.");
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, cancellationToken);
        var schedule = await db.Set<LndInstallment>().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        var today = GlTime.Today(clock);
        var figures = Figures(loan, schedule, today);

        string? glId = null;
        if (figures.TotalAmount > 0)
        {
            var plan = await rules.PlanByCodeAsync(product.ProductCode, RuleCode, today, cancellationToken)
                ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {RuleCode}. Add it in Administration > Product Event Rules "
                    + "(see setup-lending-gl.sql).");
            var lines = LndGl.Lines(plan, Amounts(figures), AmountCodes, "a write-off");
            var doc = new ProductActionDocument("LND_WRITE_OFF", w.WriteOffId.ToString(CultureInfo.InvariantCulture), $"WO-{loan.LoanNumber}", today,
                loan.CurrencyCode, $"Write-off of loan {loan.LoanNumber}: {w.Reason}");
            try
            {
                glId = await posting.PostLinesAsync(plan, lines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                    "Loan write-off", "3100", product.BusinessLineCode, doc, cancellationToken);
            }
            catch (ApexProcessException ex)
            {
                throw new BusinessRuleException($"GL posting failed: {GlPostingApi.CleanMessage(ex.Message)}");
            }
        }

        foreach (var i in schedule.Where(i => i.StatusCode != "PAID")) i.StatusCode = "WRITTEN_OFF";
        loan.InterestSuspenseAmount = 0;
        loan.LateFeeSuspenseAmount = 0;
        loan.IsNonAccrual = false;
        loan.IsWriteOffProposed = false;
        loan.DaysLate = figures.DaysLate;
        Apply(w, figures);
        w.StatusCode = Statuses.Approved;
        w.DecidedBy = User;
        w.DecidedAt = clock.UtcNow;
        w.DecisionNotes = Blank(dto.Notes);
        w.WriteOffDate = today;
        w.GlTranSetId = glId;
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loanId, EventCode = "WRITTEN_OFF", EventDate = today, EventAmount = figures.TotalAmount, GlTranSetId = glId,
            SourceDocumentType = "LND_WRITE_OFF", SourceDocumentId = w.WriteOffId,
            Notes = Cut($"Written off: principal {figures.PrincipalAmount:#,0.00}, interest {figures.InterestAmount + figures.InterestSuspenseAmount:#,0.00}, "
                + $"late fees {figures.LateFeeAmount + figures.LateFeeSuspenseAmount:#,0.00}. {w.Reason}", 1000),
        });
        await db.SaveChangesAsync(cancellationToken);
        await loans.ChangeStatusAsync(loanId, St.WrittenOff, Cut($"Write-off {w.WriteOffId}: {w.Reason}", 500), null, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task RejectAsync(long loanId, long writeOffId, LndWriteOffDecisionDto dto, CancellationToken cancellationToken)
    {
        var w = await PendingAsync(loanId, writeOffId, cancellationToken);
        if (SameUser(w.RequestedBy)) throw new BusinessRuleException("You asked for this write-off. Withdraw it instead.");
        w.StatusCode = Statuses.Rejected;
        w.DecidedBy = User;
        w.DecidedAt = clock.UtcNow;
        w.DecisionNotes = Blank(dto.Notes) ?? throw BusinessValidationException.For("notes", "Give the reason for the rejection.");
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WithdrawAsync(long loanId, long writeOffId, CancellationToken cancellationToken)
    {
        var w = await PendingAsync(loanId, writeOffId, cancellationToken);
        if (!SameUser(w.RequestedBy)) throw new BusinessRuleException("Only the user who asked for the write-off can withdraw it.");
        w.StatusCode = Statuses.Withdrawn;
        w.DecidedBy = User;
        w.DecidedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<LndWriteOff> PendingAsync(long loanId, long writeOffId, CancellationToken cancellationToken)
    {
        var w = await db.Set<LndWriteOff>().SingleOrDefaultAsync(x => x.WriteOffId == writeOffId && x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Write-off request {writeOffId} was not found.");
        if (w.StatusCode != Statuses.Pending) throw new BusinessRuleException($"The write-off request is {w.StatusCode.ToLowerInvariant()} already.");
        return w;
    }

    private static void Apply(LndWriteOff w, LndWriteOffFiguresDto f)
    {
        w.DaysLate = f.DaysLate;
        w.PrincipalAmount = f.PrincipalAmount;
        w.InterestAmount = f.InterestAmount;
        w.InterestSuspenseAmount = f.InterestSuspenseAmount;
        w.LateFeeAmount = f.LateFeeAmount;
        w.LateFeeSuspenseAmount = f.LateFeeSuspenseAmount;
        w.ChargesAmount = f.ChargesAmount;
        w.InsuranceAmount = f.InsuranceAmount;
        w.TotalAmount = f.TotalAmount;
    }

    private static LndWriteOffFiguresDto ToFigures(LndWriteOff w) => new(w.DaysLate, w.PrincipalAmount, w.InterestAmount, w.InterestSuspenseAmount,
        w.LateFeeAmount, w.LateFeeSuspenseAmount, w.ChargesAmount, w.InsuranceAmount, w.TotalAmount);

    private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
