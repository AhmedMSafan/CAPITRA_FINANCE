using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Tls;
using Microsoft.EntityFrameworkCore;
using PS = Capitra.Application.Modules.Lnd.LndProductTerms.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndLoanRequestDto(long? ClientId, long? ProductId, decimal? PrincipalAmount, int? TermMonths, DateOnly? DisbursementDate, DateOnly? FirstDueDate, string? Notes,
    string? GoodsCategoryCode = null, string? GoodsDescription = null, string? GoodsBrandModel = null, string? MerchantName = null,
    decimal? GoodsPriceAmount = null, decimal? DownPaymentAmount = null, long? ScenarioId = null);

/// <summary>The financed item of a consumer finance loan.</summary>
public sealed record LndGoodsDto(string CategoryCode, string CategoryName, string Description, string? BrandModel, string? MerchantName,
    decimal PriceAmount, decimal DownPaymentAmount);

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
    bool IsPrepaymentAllowed, string PrepaymentRecalcCode, decimal PrepaymentFeePct, decimal EarlySettlementFeePct,
    string? Notes, string CreatedBy, DateTime CreatedAt, int RowVersion,
    IReadOnlyList<LndInstallmentDto> Installments, IReadOnlyList<LndLoanStatusDto> History, IReadOnlyList<LndLoanEventDto> Events,
    LndGoodsDto? Goods = null);

public sealed record LndOptionDto(long Id, string Code, string Name, string? LicenceCode = null);

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
            .Select(p => new LndOptionDto(p.ProductId, p.ProductCode, p.ProductNameEn, LndProductTerms.Licences[p.BusinessLineCode]))];
    }

    /// <summary>The active goods categories, for the financed item of a consumer finance loan.</summary>
    public async Task<IReadOnlyList<LndOptionDto>> GoodsCategoriesAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);
        return [.. rows.Select(x => new LndOptionDto(0, x.Code, x.NameEn))];
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
        var t = await CheckAsync(dto, cancellationToken);
        return Preview(t.Version, t.Amount, t.Lines, t.RatePct);
    }

    public async Task<long> CreateAsync(LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var t = await CheckAsync(dto, cancellationToken);
        var (product, version, amount, term, disbursement, first, lines) = (t.Product, t.Version, t.Amount, t.Term, t.Disbursement, t.First, t.Lines);
        var loan = new LndLoan
        {
            LoanNumber = "PENDING-" + Guid.NewGuid().ToString("N")[..20].ToUpperInvariant(),
            ClientId = dto.ClientId!.Value, ProductId = product.ProductId, ProductVersionId = version.ProductVersionId,
            LicenceCode = LndProductTerms.Licences[product.BusinessLineCode], StatusCode = Statuses.Approved,
            PrincipalAmount = amount, TermMonths = term, InstallmentCount = lines.Count, InstallmentAmount = lines[0].Total,
            TotalInterestAmount = lines.Sum(x => x.Interest), DisbursementDate = disbursement, FirstDueDate = first, MaturityDate = lines[^1].DueDate,
            RepaymentFrequencyCode = version.RepaymentFrequencyCode, InterestMethodCode = version.InterestMethodCode, AnnualRatePct = t.RatePct,
            DayCountCode = t.DayCountCode ?? version.DayCountCode, LateFeeGraceDays = version.LateFeeGraceDays, LateFeeTypeCode = version.LateFeeTypeCode,
            LateFeeRatePct = version.LateFeeRatePct, LateFeeFlatAmount = version.LateFeeFlatAmount, LateFeeBaseCode = version.LateFeeBaseCode,
            PaymentOrder = version.PaymentOrder, IsInsuranceRequired = version.IsInsuranceRequired, IsPrepaymentAllowed = version.IsPrepaymentAllowed,
            PrepaymentRecalcCode = version.PrepaymentRecalcCode, PrepaymentFeePct = version.PrepaymentFeePct, EarlySettlementFeePct = version.EarlySettlementFeePct,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
        };
        if (loan.LicenceCode == "CF")
        {
            loan.GoodsCategoryCode = dto.GoodsCategoryCode!.Trim().ToUpperInvariant();
            loan.GoodsDescription = dto.GoodsDescription!.Trim();
            loan.GoodsBrandModel = Blank(dto.GoodsBrandModel);
            loan.MerchantName = Blank(dto.MerchantName);
            loan.GoodsPriceAmount = Math.Round(dto.GoodsPriceAmount!.Value, 2);
            loan.DownPaymentAmount = Math.Round(dto.DownPaymentAmount ?? 0, 2);
        }
        db.Set<LndLoan>().Add(loan);
        await db.SaveChangesAsync(cancellationToken);

        loan.LoanNumber = $"LN-{Today.Year}-{loan.LoanId:D7}";
        db.Set<LndInstallment>().AddRange(lines.Select(x => new LndInstallment
        {
            LoanId = loan.LoanId, InstallmentNo = x.InstallmentNo, DueDate = x.DueDate, OpeningPrincipalAmount = x.OpeningPrincipal,
            PrincipalDueAmount = x.Principal, InterestDueAmount = x.Interest, ClosingPrincipalAmount = x.ClosingPrincipal,
        }));
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = loan.LoanId, ToStatusCode = Statuses.Approved, Reason = t.ScenarioName is null ? "Loan created." : $"Created from the saved scenario '{t.ScenarioName}'." });
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
            l.IsInsuranceRequired, l.IsPrepaymentAllowed, l.PrepaymentRecalcCode, l.PrepaymentFeePct, l.EarlySettlementFeePct, l.Notes, l.CreatedBy, l.CreatedAt, l.RowVersion,
            [.. installments.Select(i =>
            {
                var due = i.PrincipalDueAmount + i.InterestDueAmount + i.ChargesDueAmount + i.InsuranceDueAmount + i.LateFeeDueAmount;
                var paid = i.PrincipalPaidAmount + i.InterestPaidAmount + i.ChargesPaidAmount + i.InsurancePaidAmount + i.LateFeePaidAmount;
                return new LndInstallmentDto(i.InstallmentNo, i.DueDate, i.OpeningPrincipalAmount, i.PrincipalDueAmount, i.InterestDueAmount,
                    i.ChargesDueAmount, i.InsuranceDueAmount, i.LateFeeDueAmount, due, paid, i.ClosingPrincipalAmount, i.StatusCode);
            })],
            [.. history.Select(h => new LndLoanStatusDto(h.FromStatusCode, h.ToStatusCode, h.Reason, h.CreatedBy, h.CreatedAt))],
            [.. events.Select(v => new LndLoanEventDto(v.EventCode, v.EventDate, v.EventAmount, v.GlTranSetId, v.Notes, v.CreatedBy, v.CreatedAt))],
            l.GoodsCategoryCode is null ? null : new LndGoodsDto(l.GoodsCategoryCode,
                await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.Code == l.GoodsCategoryCode).Select(x => x.NameEn).SingleOrDefaultAsync(cancellationToken) ?? l.GoodsCategoryCode,
                l.GoodsDescription ?? "", l.GoodsBrandModel, l.MerchantName, l.GoodsPriceAmount ?? 0, l.DownPaymentAmount ?? 0));
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

    /// <summary>What a request is made on: the product version in force, the amount, term and dates, and the schedule.</summary>
    private sealed record Terms(
        AdmProduct Product, LndProductVersion Version, decimal Amount, int Term, DateOnly Disbursement, DateOnly First,
        IReadOnlyList<LndScheduleLine> Lines, decimal RatePct, string? DayCountCode, string? ScenarioName);

    /// <summary>
    /// The terms a request can be made on, checked against the product version in force on the disbursement date. With a saved
    /// scenario of the Finance Calculator (Ahmed, 15 Nov 2026) the amount, dates, rate and schedule are the scenario's; the product,
    /// when not given, is the consumer finance product with terms in force.
    /// </summary>
    private async Task<Terms> CheckAsync(LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var e = new Dictionary<string, string[]>();
        TlsScenario? scenario = null;
        if (dto.ScenarioId is { } scenarioId)
        {
            scenario = await ScenarioAsync(scenarioId, cancellationToken);
            if (scenario is null) e["scenarioId"] = ["The saved scenario was not found among yours."];
        }
        if (dto.ClientId is null) e["clientId"] = ["Choose the customer."];
        else if (!await db.Set<CrmClient>().AnyAsync(x => x.ClientId == dto.ClientId, cancellationToken)) e["clientId"] = ["The customer was not found."];
        if (dto.ScenarioId is null)
        {
            if (dto.ProductId is null) e["productId"] = ["Choose the product."];
            if (dto.PrincipalAmount is not > 0) e["principalAmount"] = ["Enter the amount."];
            if (dto.TermMonths is not > 0) e["termMonths"] = ["Enter the term in months."];
        }
        var disbursement = scenario?.Scenario.ContractDate ?? dto.DisbursementDate ?? Today;
        if (disbursement < Today)
            e[scenario is null ? "disbursementDate" : "scenarioId"] = [scenario is null ? "The disbursement date cannot be before today."
                : "The scenario's contract date is before today. Change it in the Finance Calculator and save the scenario again."];
        if (e.Count > 0) throw new BusinessValidationException(e);

        var product = dto.ProductId is { } productId
            ? await db.Set<AdmProduct>().AsNoTracking().SingleOrDefaultAsync(x => x.ProductId == productId, cancellationToken)
            : await ConsumerProductAsync(disbursement, cancellationToken);
        if (product is null || !LndProductTerms.Licences.ContainsKey(product.BusinessLineCode) || !product.IsActive)
            throw BusinessValidationException.For("productId", dto.ProductId is null
                ? "No consumer finance product has approved terms in force." : "Choose an active lending product.");
        var versions = await db.Set<LndProductVersion>().AsNoTracking().Where(x => x.ProductId == product.ProductId && x.StatusCode == PS.Approved)
            .ToListAsync(cancellationToken);
        var version = LndProductTerms.InForce(versions, product.ProductId, disbursement)
            ?? throw BusinessValidationException.For("productId", "The product has no approved terms in force on the disbursement date.");

        IReadOnlyList<LndScheduleLine> lines;
        decimal amount, rate;
        int term;
        DateOnly first;
        string? dayCount = null;
        if (scenario is not null)
        {
            e = CheckScenario(scenario.Scenario, scenario.Rates, scenario.Lines);
            if (e.Count > 0) throw new BusinessValidationException(e);
            var s = scenario.Scenario;
            amount = s.PrincipalAmount;
            term = TermMonths(s.ContractDate, s.MaturityDate);
            rate = scenario.Rates[0].InterestRatePct;
            dayCount = DayCount(s.DayCountMethod);
            lines = [.. scenario.Lines.Select(x => new LndScheduleLine(x.InstallmentNo, x.PaymentDate, x.OpeningPrincipalAmount, x.PrincipalAmount,
                x.InterestAmount, x.ClosingPrincipalAmount))];
            first = lines[0].DueDate;
        }
        else
        {
            amount = Math.Round(dto.PrincipalAmount!.Value, 2);
            term = dto.TermMonths!.Value;
            rate = version.AnnualRatePct;
            first = dto.FirstDueDate ?? LndSchedule.DefaultFirstDueDate(disbursement, version.RepaymentFrequencyCode);
            lines = [];
        }

        if (LndProductTerms.Licences[product.BusinessLineCode] == "CF")
        {
            var categories = await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.IsActive).Select(x => x.Code).ToListAsync(cancellationToken);
            e = CheckGoods(dto, amount, categories);
            if (e.Count > 0) throw new BusinessValidationException(e);
        }
        e = CheckTerms(version, amount, term, disbursement, first);
        if (scenario is not null && e.Count > 0)
            e = new Dictionary<string, string[]> { ["scenarioId"] = [.. e.Values.SelectMany(x => x).Select(x => "Scenario: " + x)] };
        if (e.Count > 0) throw new BusinessValidationException(e);
        if (scenario is null) lines = LndSchedule.Build(amount, term, version.RepaymentFrequencyCode, version.InterestMethodCode, version.AnnualRatePct, first);
        return new Terms(product, version, amount, term, disbursement, first, lines, rate, dayCount, scenario?.Scenario.ScenarioName);
    }

    private sealed record TlsScenario(TlsLoanScenario Scenario, List<TlsLoanScenarioRate> Rates, List<TlsLoanScenarioLine> Lines);

    /// <summary>A saved scenario of the Finance Calculator, only if the user created it.</summary>
    private async Task<TlsScenario?> ScenarioAsync(long scenarioId, CancellationToken cancellationToken)
    {
        var owner = currentUser.UserName ?? "SYSTEM";
        var s = await db.Set<TlsLoanScenario>().AsNoTracking().SingleOrDefaultAsync(x => x.ScenarioId == scenarioId && x.CreatedBy == owner, cancellationToken);
        if (s is null) return null;
        var rates = await db.Set<TlsLoanScenarioRate>().AsNoTracking().Where(x => x.ScenarioId == scenarioId).OrderBy(x => x.EffectiveDate).ToListAsync(cancellationToken);
        var lines = await db.Set<TlsLoanScenarioLine>().AsNoTracking().Where(x => x.ScenarioId == scenarioId).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        return new TlsScenario(s, rates, lines);
    }

    /// <summary>The user's saved scenarios of the Finance Calculator, for the New Finance page.</summary>
    public async Task<IReadOnlyList<LndOptionDto>> ScenariosAsync(CancellationToken cancellationToken)
    {
        var owner = currentUser.UserName ?? "SYSTEM";
        return await db.Set<TlsLoanScenario>().AsNoTracking().Where(x => x.CreatedBy == owner).OrderBy(x => x.ScenarioName)
            .Select(x => new LndOptionDto(x.ScenarioId, x.ScenarioName, x.ScenarioName)).ToListAsync(cancellationToken);
    }

    /// <summary>The active consumer finance product with approved terms in force on the day (the first by name).</summary>
    private async Task<AdmProduct?> ConsumerProductAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var products = await db.Set<AdmProduct>().AsNoTracking()
            .Where(x => x.IsActive && x.BusinessLineCode == LndProductTerms.BusinessLines.ConsumerFinance).OrderBy(x => x.ProductNameEn).ToListAsync(cancellationToken);
        var ids = products.Select(x => x.ProductId).ToList();
        var versions = await db.Set<LndProductVersion>().AsNoTracking().Where(x => ids.Contains(x.ProductId) && x.StatusCode == PS.Approved).ToListAsync(cancellationToken);
        return products.FirstOrDefault(p => LndProductTerms.InForce(versions, p.ProductId, day) is not null);
    }

    /// <summary>
    /// A saved scenario can make a finance only if every payment carries principal and interest together (one invoice: principal and
    /// interest on the same dates) and one rate runs for the whole term.
    /// </summary>
    public static Dictionary<string, string[]> CheckScenario(TlsLoanScenario s, IReadOnlyList<TlsLoanScenarioRate> rates, IReadOnlyList<TlsLoanScenarioLine> lines)
    {
        var problems = new List<string>();
        if (lines.Count == 0) problems.Add("The scenario has no schedule. Open it in the Finance Calculator and save it again.");
        if (rates.Count == 0) problems.Add("The scenario has no rate. Open it in the Finance Calculator and save it again.");
        if (rates.Select(x => x.InterestRatePct).Distinct().Count() > 1) problems.Add("The scenario changes its rate during the term; a finance uses one rate for the whole term.");
        var rate = rates.Count > 0 ? rates[0].InterestRatePct : 0;
        if (lines.Any(x => x.PrincipalAmount <= 0 || (rate > 0 && x.InterestAmount <= 0)))
            problems.Add("Principal and interest are not paid on the same dates. Use a scenario where every payment has both (Combined Principal + Interest).");
        var e = new Dictionary<string, string[]>();
        if (problems.Count > 0) e["scenarioId"] = [.. problems];
        return e;
    }

    /// <summary>Whole months from the contract date to maturity, a part month counting as one.</summary>
    public static int TermMonths(DateOnly from, DateOnly to)
    {
        var months = (to.Year - from.Year) * 12 + to.Month - from.Month;
        return to.Day > from.Day ? months + 1 : months;
    }

    /// <summary>The calculator's day count as the loan's (used by end of day): 30-day bases are THIRTY_360, ACT/ACT is ACT_365.</summary>
    public static string DayCount(string method) => method switch
    {
        "ACT_360" => "ACT_360",
        "30_360" or "30_365" or "30_ACT" => "THIRTY_360",
        _ => "ACT_365",
    };

    /// <summary>
    /// The financed item of a consumer finance loan: an active category, a description, the price, and a down payment below it; the
    /// loan amount is the price less the down payment.
    /// </summary>
    public static Dictionary<string, string[]> CheckGoods(LndLoanRequestDto dto, decimal amount, IReadOnlyCollection<string> activeCategories)
    {
        var e = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.GoodsCategoryCode)) e["goodsCategoryCode"] = ["Choose what is financed."];
        else if (!activeCategories.Contains(dto.GoodsCategoryCode.Trim().ToUpperInvariant())) e["goodsCategoryCode"] = ["The goods category is not active."];
        if (string.IsNullOrWhiteSpace(dto.GoodsDescription)) e["goodsDescription"] = ["Describe the financed item."];
        else if (dto.GoodsDescription.Trim().Length > 300) e["goodsDescription"] = ["The description is longer than 300 characters."];
        if (dto.GoodsBrandModel?.Trim().Length > 200) e["goodsBrandModel"] = ["The brand / model is longer than 200 characters."];
        if (dto.MerchantName?.Trim().Length > 200) e["merchantName"] = ["The merchant is longer than 200 characters."];
        var down = Math.Round(dto.DownPaymentAmount ?? 0, 2);
        if (dto.GoodsPriceAmount is not > 0) e["goodsPriceAmount"] = ["Enter the item's price."];
        else if (down < 0 || down >= dto.GoodsPriceAmount) e["downPaymentAmount"] = ["The down payment must be from 0 to less than the price."];
        else if (amount != Math.Round(dto.GoodsPriceAmount.Value, 2) - down)
            e["principalAmount"] = [$"The amount must be the price less the down payment ({Math.Round(dto.GoodsPriceAmount.Value, 2) - down:#,0.00})."];
        return e;
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

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

    public static LndLoanPreviewDto Preview(LndProductVersion v, decimal amount, IReadOnlyList<LndScheduleLine> lines, decimal ratePct)
    {
        var interest = lines.Sum(x => x.Interest);
        return new LndLoanPreviewDto(v.VersionNo, v.RepaymentFrequencyCode, v.InterestMethodCode, ratePct, lines.Count, lines[0].Total,
            interest, amount + interest, lines[0].DueDate, lines[^1].DueDate,
            [.. lines.Select(x => new LndScheduleLineDto(x.InstallmentNo, x.DueDate, x.OpeningPrincipal, x.Principal, x.Interest, x.Total, x.ClosingPrincipal))]);
    }
}
