using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using PS = Capitra.Application.Modules.Lnd.LndProductTerms.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndLoanRequestDto(long? ClientId, long? ProductId, decimal? PrincipalAmount, int? TermMonths, DateOnly? DisbursementDate, DateOnly? FirstDueDate, string? Notes);

public sealed record LndScheduleLineDto(int InstallmentNo, DateOnly DueDate, decimal OpeningPrincipal, decimal Principal, decimal Interest, decimal Total, decimal ClosingPrincipal);

public sealed record LndLoanPreviewDto(
    int VersionNo, string RepaymentFrequencyCode, string InterestMethodCode, decimal AnnualRatePct, int InstallmentCount, decimal InstallmentAmount,
    decimal TotalInterestAmount, decimal TotalRepayableAmount, DateOnly FirstDueDate, DateOnly MaturityDate, IReadOnlyList<LndScheduleLineDto> Schedule);

public sealed record LndLoanListDto(
    long LoanId, string LoanNumber, long ClientId, string ClientName, string ProductCode, string ProductName, string LicenceCode, string StatusCode,
    decimal PrincipalAmount, int TermMonths, decimal InstallmentAmount, DateOnly DisbursementDate, DateOnly MaturityDate, DateTime CreatedAt);

public sealed record LndInstallmentDto(
    int InstallmentNo, DateOnly DueDate, decimal OpeningPrincipal, decimal PrincipalDue, decimal InterestDue, decimal ChargesDue, decimal InsuranceDue,
    decimal LateFeeDue, decimal TotalDue, decimal TotalPaid, decimal ClosingPrincipal, string StatusCode);

public sealed record LndLoanStatusDto(string? FromStatusCode, string ToStatusCode, string? Reason, string By, DateTime At);

public sealed record LndLoanEventDto(string EventCode, DateOnly EventDate, decimal EventAmount, string? GlTranSetId, string? Notes, string By, DateTime At);

public sealed record LndLoanDto(
    long LoanId, string LoanNumber, long ClientId, string ClientName, long ProductId, string ProductCode, string ProductName, int VersionNo,
    string LicenceCode, string StatusCode, bool IsNonAccrual, int DaysLate, decimal InterestSuspenseAmount, decimal LateFeeSuspenseAmount,
    bool IsWriteOffProposed, DateOnly? LastEodDate, string CurrencyCode, decimal PrincipalAmount, int TermMonths, int InstallmentCount,
    decimal InstallmentAmount, decimal TotalInterestAmount, DateOnly DisbursementDate, DateOnly FirstDueDate, DateOnly MaturityDate,
    string RepaymentFrequencyCode, string InterestMethodCode, decimal AnnualRatePct, string DayCountCode, int LateFeeGraceDays, string LateFeeTypeCode,
    decimal? LateFeeRatePct, decimal? LateFeeFlatAmount, string LateFeeBaseCode, IReadOnlyList<string> PaymentOrder, bool IsInsuranceRequired,
    string? Notes, string CreatedBy, DateTime CreatedAt, int RowVersion,
    IReadOnlyList<LndInstallmentDto> Installments, IReadOnlyList<LndLoanStatusDto> History, IReadOnlyList<LndLoanEventDto> Events);

public sealed record LndOptionDto(long Id, string Code, string Name);

public sealed record LndLoanStatusChangeDto(string? Reason, int? RowVersion);

/// <summary>
/// Retail Lending > Loans: the loan core (design deck, Phase 1). A loan is made from a lending product's version in force, whose terms
/// it copies; its schedule comes from LndSchedule. Only this service changes a loan's status, through the transition table, and every
/// change writes LND_LOAN_STATUS_HIST. Disbursement, GL, receipts and end of day come in the next steps.
/// </summary>
public sealed class LndLoanService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    public static class Statuses
    {
        public const string Approved = "APPROVED";
        public const string Active = "ACTIVE";
        public const string Closed = "CLOSED";
        public const string Cancelled = "CANCELLED";
        public const string WrittenOff = "WRITTEN_OFF";
    }

    /// <summary>The allowed status changes. Disbursement (APPROVED to ACTIVE), closing, write-off and reopening (a receipt reversed) come with their own steps.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> Transitions = new Dictionary<string, string[]>
    {
        [Statuses.Approved] = [Statuses.Active, Statuses.Cancelled],
        [Statuses.Active] = [Statuses.Closed, Statuses.WrittenOff],
        [Statuses.Closed] = [Statuses.Active],
        [Statuses.WrittenOff] = [],
        [Statuses.Cancelled] = [],
    };

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    public async Task<IReadOnlyList<LndLoanListDto>> ListAsync(CancellationToken cancellationToken) =>
        await (from l in db.Set<LndLoan>().AsNoTracking()
               join c in db.Set<CrmClient>().AsNoTracking() on l.ClientId equals c.ClientId
               join p in db.Set<AdmProduct>().AsNoTracking() on l.ProductId equals p.ProductId
               orderby l.LoanId descending
               select new LndLoanListDto(l.LoanId, l.LoanNumber, l.ClientId, c.ClientName, p.ProductCode, p.ProductNameEn, l.LicenceCode, l.StatusCode,
                   l.PrincipalAmount, l.TermMonths, l.InstallmentAmount, l.DisbursementDate, l.MaturityDate, l.CreatedAt))
            .Take(500).ToListAsync(cancellationToken);

    /// <summary>The lending products with a version in force today, for the New Loan form.</summary>
    public async Task<IReadOnlyList<LndOptionDto>> ProductsAsync(CancellationToken cancellationToken)
    {
        var lines = LndProductTerms.Licences.Keys.ToList();
        var products = await db.Set<AdmProduct>().AsNoTracking().Where(x => x.IsActive && lines.Contains(x.BusinessLineCode))
            .OrderBy(x => x.ProductNameEn).ToListAsync(cancellationToken);
        var ids = products.Select(x => x.ProductId).ToList();
        var versions = await db.Set<LndProductVersion>().AsNoTracking().Where(x => ids.Contains(x.ProductId) && x.StatusCode == PS.Approved)
            .ToListAsync(cancellationToken);
        var today = Today;
        return [.. products.Where(p => LndProductTerms.InForce(versions, p.ProductId, today) is not null)
            .Select(p => new LndOptionDto(p.ProductId, p.ProductCode, p.ProductNameEn))];
    }

    /// <summary>Customers whose name, number or national ID contains the text (the first 20).</summary>
    public async Task<IReadOnlyList<LndOptionDto>> ClientsAsync(string? search, CancellationToken cancellationToken)
    {
        var q = db.Set<CrmClient>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToUpperInvariant();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), so the search ignores case in the database.
            q = q.Where(x => x.ClientName.ToUpper().Contains(s) || x.ClientNameAr.Contains(search.Trim()) || x.ClientNumber.ToUpper().Contains(s)
                || (x.NationalId != null && x.NationalId.Contains(s)));
#pragma warning restore CA1862
        }
        return await q.OrderBy(x => x.ClientName).Take(20).Select(x => new LndOptionDto(x.ClientId, x.ClientNumber, x.ClientName)).ToListAsync(cancellationToken);
    }

    /// <summary>The schedule the request would make, without saving anything.</summary>
    public async Task<LndLoanPreviewDto> PreviewAsync(LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var (_, version, amount, term, _, first) = await CheckAsync(dto, cancellationToken);
        return Preview(version, amount, term, first);
    }

    public async Task<long> CreateAsync(LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var (product, version, amount, term, disbursement, first) = await CheckAsync(dto, cancellationToken);
        var lines = LndSchedule.Build(amount, term, version.RepaymentFrequencyCode, version.InterestMethodCode, version.AnnualRatePct, first);
        var loan = new LndLoan
        {
            LoanNumber = "PENDING-" + Guid.NewGuid().ToString("N")[..20].ToUpperInvariant(),
            ClientId = dto.ClientId!.Value, ProductId = product.ProductId, ProductVersionId = version.ProductVersionId,
            LicenceCode = LndProductTerms.Licences[product.BusinessLineCode], StatusCode = Statuses.Approved,
            PrincipalAmount = amount, TermMonths = term, InstallmentCount = lines.Count, InstallmentAmount = lines[0].Total,
            TotalInterestAmount = lines.Sum(x => x.Interest), DisbursementDate = disbursement, FirstDueDate = first, MaturityDate = lines[^1].DueDate,
            RepaymentFrequencyCode = version.RepaymentFrequencyCode, InterestMethodCode = version.InterestMethodCode, AnnualRatePct = version.AnnualRatePct,
            DayCountCode = version.DayCountCode, LateFeeGraceDays = version.LateFeeGraceDays, LateFeeTypeCode = version.LateFeeTypeCode,
            LateFeeRatePct = version.LateFeeRatePct, LateFeeFlatAmount = version.LateFeeFlatAmount, LateFeeBaseCode = version.LateFeeBaseCode,
            PaymentOrder = version.PaymentOrder, IsInsuranceRequired = version.IsInsuranceRequired,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
        };
        db.Set<LndLoan>().Add(loan);
        await db.SaveChangesAsync(cancellationToken);

        loan.LoanNumber = $"LN-{Today.Year}-{loan.LoanId:D7}";
        db.Set<LndInstallment>().AddRange(lines.Select(x => new LndInstallment
        {
            LoanId = loan.LoanId, InstallmentNo = x.InstallmentNo, DueDate = x.DueDate, OpeningPrincipalAmount = x.OpeningPrincipal,
            PrincipalDueAmount = x.Principal, InterestDueAmount = x.Interest, ClosingPrincipalAmount = x.ClosingPrincipal,
        }));
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = loan.LoanId, ToStatusCode = Statuses.Approved, Reason = "Loan created." });
        await db.SaveChangesAsync(cancellationToken);
        return loan.LoanId;
    }

    public async Task<LndLoanDto> GetAsync(long loanId, CancellationToken cancellationToken)
    {
        var l = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var client = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == l.ClientId).Select(x => x.ClientName).SingleAsync(cancellationToken);
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == l.ProductId, cancellationToken);
        var versionNo = await db.Set<LndProductVersion>().AsNoTracking().Where(x => x.ProductVersionId == l.ProductVersionId).Select(x => x.VersionNo).SingleAsync(cancellationToken);
        var installments = await db.Set<LndInstallment>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        var history = await db.Set<LndLoanStatusHist>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.LoanStatusHistId).ToListAsync(cancellationToken);
        var events = await db.Set<LndLoanEvent>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.LoanEventId).ToListAsync(cancellationToken);
        return new LndLoanDto(l.LoanId, l.LoanNumber, l.ClientId, client, l.ProductId, product.ProductCode, product.ProductNameEn, versionNo, l.LicenceCode,
            l.StatusCode, l.IsNonAccrual, l.DaysLate, l.InterestSuspenseAmount, l.LateFeeSuspenseAmount, l.IsWriteOffProposed, l.LastEodDate, l.CurrencyCode, l.PrincipalAmount, l.TermMonths, l.InstallmentCount, l.InstallmentAmount, l.TotalInterestAmount,
            l.DisbursementDate, l.FirstDueDate, l.MaturityDate, l.RepaymentFrequencyCode, l.InterestMethodCode, l.AnnualRatePct, l.DayCountCode,
            l.LateFeeGraceDays, l.LateFeeTypeCode, l.LateFeeRatePct, l.LateFeeFlatAmount, l.LateFeeBaseCode, LndProductTerms.Parts(l.PaymentOrder),
            l.IsInsuranceRequired, l.Notes, l.CreatedBy, l.CreatedAt, l.RowVersion,
            [.. installments.Select(i =>
            {
                var due = i.PrincipalDueAmount + i.InterestDueAmount + i.ChargesDueAmount + i.InsuranceDueAmount + i.LateFeeDueAmount;
                var paid = i.PrincipalPaidAmount + i.InterestPaidAmount + i.ChargesPaidAmount + i.InsurancePaidAmount + i.LateFeePaidAmount;
                return new LndInstallmentDto(i.InstallmentNo, i.DueDate, i.OpeningPrincipalAmount, i.PrincipalDueAmount, i.InterestDueAmount,
                    i.ChargesDueAmount, i.InsuranceDueAmount, i.LateFeeDueAmount, due, paid, i.ClosingPrincipalAmount, i.StatusCode);
            })],
            [.. history.Select(h => new LndLoanStatusDto(h.FromStatusCode, h.ToStatusCode, h.Reason, h.CreatedBy, h.CreatedAt))],
            [.. events.Select(v => new LndLoanEventDto(v.EventCode, v.EventDate, v.EventAmount, v.GlTranSetId, v.Notes, v.CreatedBy, v.CreatedAt))]);
    }

    /// <summary>Cancels a loan that has not been paid out. The schedule is kept as it was, for the record.</summary>
    public Task CancelAsync(long loanId, LndLoanStatusChangeDto dto, CancellationToken cancellationToken) =>
        ChangeStatusAsync(loanId, Statuses.Cancelled,
            string.IsNullOrWhiteSpace(dto.Reason) ? throw BusinessValidationException.For("reason", "Give the reason for the cancellation.") : dto.Reason.Trim(),
            dto.RowVersion, cancellationToken);

    /// <summary>The one place a loan's status changes: the transition must be in the table, and the change is written to the history.</summary>
    public async Task ChangeStatusAsync(long loanId, string toStatus, string? reason, int? rowVersion, CancellationToken cancellationToken)
    {
        var l = await db.Set<LndLoan>().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        if (rowVersion is { } rv && rv != l.RowVersion) throw new ConflictException("Another user has changed this loan. Reload the page.");
        if (!CanMove(l.StatusCode, toStatus)) throw new BusinessRuleException($"A loan cannot move from {l.StatusCode} to {toStatus}.");
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = l.LoanId, FromStatusCode = l.StatusCode, ToStatusCode = toStatus, Reason = reason });
        l.StatusCode = toStatus;
        await db.SaveChangesAsync(cancellationToken);
    }

    public static bool CanMove(string from, string to) => Transitions.TryGetValue(from, out var next) && next.Contains(to);

    /// <summary>The terms a request can be made on, checked against the product version in force on the disbursement date.</summary>
    private async Task<(AdmProduct Product, LndProductVersion Version, decimal Amount, int Term, DateOnly Disbursement, DateOnly First)> CheckAsync(
        LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var e = new Dictionary<string, string[]>();
        if (dto.ClientId is null) e["clientId"] = ["Choose the customer."];
        else if (!await db.Set<CrmClient>().AnyAsync(x => x.ClientId == dto.ClientId, cancellationToken)) e["clientId"] = ["The customer was not found."];
        if (dto.ProductId is null) e["productId"] = ["Choose the product."];
        if (dto.PrincipalAmount is not > 0) e["principalAmount"] = ["Enter the amount."];
        if (dto.TermMonths is not > 0) e["termMonths"] = ["Enter the term in months."];
        var disbursement = dto.DisbursementDate ?? Today;
        if (disbursement < Today) e["disbursementDate"] = ["The disbursement date cannot be before today."];
        if (e.Count > 0) throw new BusinessValidationException(e);

        var product = await db.Set<AdmProduct>().AsNoTracking().SingleOrDefaultAsync(x => x.ProductId == dto.ProductId, cancellationToken);
        if (product is null || !LndProductTerms.Licences.ContainsKey(product.BusinessLineCode) || !product.IsActive)
            throw BusinessValidationException.For("productId", "Choose an active lending product.");
        var versions = await db.Set<LndProductVersion>().AsNoTracking().Where(x => x.ProductId == product.ProductId && x.StatusCode == PS.Approved)
            .ToListAsync(cancellationToken);
        var version = LndProductTerms.InForce(versions, product.ProductId, disbursement)
            ?? throw BusinessValidationException.For("productId", "The product has no approved terms in force on the disbursement date.");

        var amount = Math.Round(dto.PrincipalAmount!.Value, 2);
        var term = dto.TermMonths!.Value;
        var first = dto.FirstDueDate ?? LndSchedule.DefaultFirstDueDate(disbursement, version.RepaymentFrequencyCode);
        e = CheckTerms(version, amount, term, disbursement, first);
        if (e.Count > 0) throw new BusinessValidationException(e);
        return (product, version, amount, term, disbursement, first);
    }

    /// <summary>The amount and term must be within the version's ranges; the first instalment falls after the disbursement, within 3 months.</summary>
    public static Dictionary<string, string[]> CheckTerms(LndProductVersion v, decimal amount, int term, DateOnly disbursement, DateOnly first)
    {
        var e = new Dictionary<string, string[]>();
        if (amount < v.MinAmount || amount > v.MaxAmount) e["principalAmount"] = [$"The amount must be from {v.MinAmount:#,0.##} to {v.MaxAmount:#,0.##}."];
        if (term < v.MinTermMonths || term > v.MaxTermMonths) e["termMonths"] = [$"The term must be from {v.MinTermMonths} to {v.MaxTermMonths} months."];
        if (first <= disbursement) e["firstDueDate"] = ["The first instalment must fall after the disbursement date."];
        else if (first > disbursement.AddMonths(3)) e["firstDueDate"] = ["The first instalment must fall within 3 months of the disbursement."];
        return e;
    }

    public static LndLoanPreviewDto Preview(LndProductVersion v, decimal amount, int term, DateOnly first)
    {
        var lines = LndSchedule.Build(amount, term, v.RepaymentFrequencyCode, v.InterestMethodCode, v.AnnualRatePct, first);
        var interest = lines.Sum(x => x.Interest);
        return new LndLoanPreviewDto(v.VersionNo, v.RepaymentFrequencyCode, v.InterestMethodCode, v.AnnualRatePct, lines.Count, lines[0].Total,
            interest, amount + interest, first, lines[^1].DueDate,
            [.. lines.Select(x => new LndScheduleLineDto(x.InstallmentNo, x.DueDate, x.OpeningPrincipal, x.Principal, x.Interest, x.Total, x.ClosingPrincipal))]);
    }
}
