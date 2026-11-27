using Capitra.Application.Abstractions;
using Capitra.Application.Common;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Modules.Tls;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Tls;
using Microsoft.EntityFrameworkCore;
using PS = Capitra.Application.Modules.Lnd.LndProductTerms.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndLoanRequestDto(long? ClientId, long? ProductId, decimal? PrincipalAmount, int? TermMonths, DateOnly? DisbursementDate, DateOnly? FirstDueDate, string? Notes,
    string? GoodsCategoryCode = null, string? GoodsDescription = null, string? GoodsBrandModel = null, string? MerchantName = null,
    decimal? GoodsPriceAmount = null, decimal? DownPaymentAmount = null, long? ScenarioId = null, decimal? RatePct = null,
    TlsScenarioRequest? Calculation = null, long? SupplierApplicationId = null);

/// <summary>
/// An active Consumer Finance supplier agreement, offered on New Finance with its terms (Ahmed, 26 Nov 2026): active application and
/// supplier role, and valid today.
/// </summary>
public sealed record LndCfSupplierOptionDto(
    long ApplicationId, string ApplicationRef, long ClientId, string ClientNumber, string SupplierName, IReadOnlyList<string> GoodsCategoryCodes,
    decimal MinDownPaymentPct, string DownPaymentCollectorCode, decimal CommissionPct, int PaymentDays, int DeliveryDays,
    DateOnly ValidFromDate, DateOnly? ValidToDate);

/// <summary>A saved Finance Calculator scenario offered on New Finance, with its amount and rate.</summary>
public sealed record LndScenarioOptionDto(long Id, string Name, decimal PrincipalAmount, decimal RatePct, string ScheduleMode, string PrincipalMethod);

/// <summary>The financed item of a consumer finance loan.</summary>
public sealed record LndGoodsDto(string CategoryCode, string CategoryName, string Description, string? BrandModel, string? MerchantName,
    decimal PriceAmount, decimal DownPaymentAmount, long? SupplierApplicationId = null, string? DownPaymentCollectorCode = null,
    decimal? SupplierMinDownPaymentPct = null, decimal? SupplierCommissionPct = null, int? SupplierPaymentDays = null, int? SupplierDeliveryDays = null);

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
    LndGoodsDto? Goods = null, IReadOnlyList<LndCfChargeDto>? Charges = null, LndCfClaimDto? Claim = null, decimal RateMarginPct = 0,
    LndCfDeliveryDto? Delivery = null, LndCfSupplierPaidDto? SupplierPayment = null);

/// <summary>A consumer finance's supplier payment (step 6): commission (and why it changed), net paid, reference, who and when, GL.</summary>
public sealed record LndCfSupplierPaidDto(decimal CommissionAmount, string? CommissionReason, decimal NetAmount, string? Reference, DateTime PaidAt, string? PaidBy, string? GlTranSetId);

/// <summary>A consumer finance's delivery (step 5): the day, the delivery note, the serial, the GL of the booking, when the supplier is paid by.</summary>
public sealed record LndCfDeliveryDto(DateOnly DeliveryDate, string? DeliveryNoteNo, string? SerialNo, string? GlTranSetId, DateOnly? SupplierPaymentDueDate);

/// <summary>
/// The upfront claim of a consumer finance (Charges and claim step): the upfront charges and the down payment we take, OPEN until the
/// customer pays it (PAID), or NONE when nothing is paid upfront.
/// </summary>
public sealed record LndCfClaimDto(string StatusCode, decimal ChargesAmount, decimal DownPaymentAmount, decimal TotalAmount, string? GlTranSetId,
    DateTime? PaidAt, string? PaidBy, string? PaymentMethodCode, string? PaymentReference, string? PaymentGlTranSetId);

public sealed record LndOptionDto(long Id, string Code, string Name, string? LicenceCode = null);

public sealed record LndLoanStatusChangeDto(string? Reason, int? RowVersion);

/// <summary>
/// Retail Lending > Loans: the loan core (design deck, Phase 1). A loan is made from a lending product's version in force, whose terms
/// it copies; its schedule comes from LndSchedule. Only this service changes a loan's status, through the transition table, and every
/// change writes LND_LOAN_STATUS_HIST. Disbursement, GL, receipts and end of day come in the next steps.
/// </summary>
public sealed class LndLoanService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeRuntime runtime, IScrUserProfiles profiles, ILndParameters parameters)
{
    public static class Statuses
    {
        /// <summary>A consumer finance in its CF_FINANCE workflow, before credit approval.</summary>
        public const string InApproval = "IN_APPROVAL";
        public const string Approved = "APPROVED";
        /// <summary>Rejected in its workflow.</summary>
        public const string Rejected = "REJECTED";
        public const string Active = "ACTIVE";
        public const string Closed = "CLOSED";
        public const string Cancelled = "CANCELLED";
        public const string WrittenOff = "WRITTEN_OFF";
    }

    /// <summary>The allowed status changes. Disbursement (APPROVED to ACTIVE), closing, write-off and reopening (a receipt reversed) come with their own steps.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> Transitions = new Dictionary<string, string[]>
    {
        [Statuses.InApproval] = [Statuses.Approved, Statuses.Rejected, Statuses.Cancelled],
        [Statuses.Approved] = [Statuses.Active, Statuses.Cancelled],
        [Statuses.Rejected] = [],
        [Statuses.Active] = [Statuses.Closed, Statuses.WrittenOff],
        [Statuses.Closed] = [Statuses.Active],
        [Statuses.WrittenOff] = [],
        [Statuses.Cancelled] = [],
    };

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private DateOnly Today => CairoToday(clock);

    public static DateOnly CairoToday(IClock clock) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

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

    /// <summary>The supplier agreements a consumer finance can be bought under today (New Finance), by supplier name.</summary>
    public async Task<IReadOnlyList<LndCfSupplierOptionDto>> CfSuppliersAsync(CancellationToken cancellationToken)
    {
        var rows = await (
            from a in ActiveSupplierAgreements(db, Today)
            join c in db.Set<CrmClient>().AsNoTracking() on a.ClientId equals c.ClientId
            orderby c.ClientName
            select new { a, c.ClientNumber, c.ClientName }).ToListAsync(cancellationToken);
        return [.. rows.Select(x => Option(x.a, x.ClientNumber, x.ClientName))];
    }

    /// <summary>Supplier agreements a finance can be bought under on a day: active application and supplier role, valid that day.</summary>
    public static IQueryable<CrmCfSupplierApplication> ActiveSupplierAgreements(ICapitraDbContext db, DateOnly day) =>
        from a in db.Set<CrmCfSupplierApplication>().AsNoTracking()
        join r in db.Set<CrmClientRole>().AsNoTracking() on a.ClientRoleId equals r.ClientRoleId
        where a.StatusCode == CrmCfSupplierStatus.Active && r.StatusCode == CrmClientRole.Active
            && a.ValidFromDate <= day && (a.ValidToDate == null || a.ValidToDate >= day)
        select a;

    private static LndCfSupplierOptionDto Option(CrmCfSupplierApplication a, string clientNumber, string clientName) =>
        new(a.ApplicationId, a.ApplicationRef, a.ClientId, clientNumber, clientName,
            [.. a.GoodsCategoryCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
            a.MinDownPaymentPct, a.DownPaymentCollectorCode, a.CommissionPct, a.PaymentDays, a.DeliveryDays, a.ValidFromDate, a.ValidToDate);

    /// <summary>
    /// The supplier of a consumer finance (Ahmed, 26 Nov 2026): an active agreement valid today, a goods category it may sell, and a
    /// down payment of at least its minimum % of the price.
    /// </summary>
    public static Dictionary<string, string[]> CheckSupplier(LndLoanRequestDto dto, LndCfSupplierOptionDto? supplier)
    {
        var e = new Dictionary<string, string[]>();
        if (supplier is null)
        {
            e["supplierApplicationId"] = [dto.SupplierApplicationId is null ? "Choose the supplier." : "The supplier is not active, or its agreement is not valid today."];
            return e;
        }
        var category = dto.GoodsCategoryCode?.Trim().ToUpperInvariant();
        if (category is not null && !supplier.GoodsCategoryCodes.Contains(category))
            e["goodsCategoryCode"] = [$"{supplier.SupplierName} may not sell this goods category."];
        if (dto.GoodsPriceAmount is > 0)
        {
            var min = Money.Round(dto.GoodsPriceAmount.Value * supplier.MinDownPaymentPct / 100m, 2);
            if (Money.Round(dto.DownPaymentAmount ?? 0, 2) < min)
                e["downPaymentAmount"] = [$"The down payment must be at least {supplier.MinDownPaymentPct:0.##}% of the price ({min:#,0.00})."];
        }
        return e;
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
        // a consumer finance from a supplier goes through CF_FINANCE: creating it submits it for credit approval
        // its charges are decided at Credit approval and its schedule is built at Schedule and contract: the instalment, interest and
        // maturity saved now are indicative only
        var workflow = t.Supplier is not null;
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
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
            loan.GoodsPriceAmount = Money.Round(dto.GoodsPriceAmount!.Value, 2);
            loan.DownPaymentAmount = Money.Round(dto.DownPaymentAmount ?? 0, 2);
            // the supplier agreement's terms are copied, so a later change to it does not change this finance
            var sup = t.Supplier!;
            loan.SupplierApplicationId = sup.ApplicationId;
            loan.MerchantName = sup.SupplierName;
            loan.DownPaymentCollectorCode = sup.DownPaymentCollectorCode;
            loan.SupplierMinDownPaymentPct = sup.MinDownPaymentPct;
            loan.SupplierCommissionPct = sup.CommissionPct;
            loan.SupplierPaymentDays = sup.PaymentDays;
            loan.SupplierDeliveryDays = sup.DeliveryDays;
            var profile = await profiles.ByUserNameAsync(User, cancellationToken);
            loan.EnteredByPerson = ScrDuties.PersonKey(profile?.EmployeeNo, User);
            loan.StatusCode = Statuses.InApproval;
        }
        db.Set<LndLoan>().Add(loan);
        await db.SaveChangesAsync(cancellationToken);

        loan.LoanNumber = $"LN-{Today.Year}-{loan.LoanId:D7}";
        if (!workflow)
            db.Set<LndInstallment>().AddRange(Installments(loan.LoanId, lines));
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = loan.LoanId, ToStatusCode = loan.StatusCode, Reason = History(t) });
        await db.SaveChangesAsync(cancellationToken);
        if (workflow)
        {
            var run = await runtime.StartAsync(LndCfFinanceRules.WorkflowCode, loan.LoanNumber, LndCfFinanceService.Variables(loan), User, cancellationToken);
            loan.WorkflowInstanceId = run.WorkflowInstanceId;
            await db.SaveChangesAsync(cancellationToken);
        }
        await tx.CommitAsync(cancellationToken);
        return loan.LoanId;
    }

    /// <summary>The instalments of a schedule, as saved for a loan.</summary>
    public static IEnumerable<LndInstallment> Installments(long loanId, IReadOnlyList<LndScheduleLine> lines) => lines.Select(x => new LndInstallment
    {
        LoanId = loanId, InstallmentNo = x.InstallmentNo, DueDate = x.DueDate, OpeningPrincipalAmount = x.OpeningPrincipal,
        PrincipalDueAmount = x.Principal, InterestDueAmount = x.Interest, ClosingPrincipalAmount = x.ClosingPrincipal,
    });

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
                l.GoodsDescription ?? "", l.GoodsBrandModel, l.MerchantName, l.GoodsPriceAmount ?? 0, l.DownPaymentAmount ?? 0, l.SupplierApplicationId,
                l.DownPaymentCollectorCode, l.SupplierMinDownPaymentPct, l.SupplierCommissionPct, l.SupplierPaymentDays, l.SupplierDeliveryDays),
            await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loanId).OrderBy(x => x.LoanChargeId)
                .Select(x => new LndCfChargeDto(x.ChargeCode, x.ChargeName, "", x.ChargeAmount, x.TaxAmount, x.TimingCode,
                    x.TimingCode, x.IsOverridden, x.ChargeAmount))
                .ToListAsync(cancellationToken),
            l.ClaimStatusCode is null ? null : new LndCfClaimDto(l.ClaimStatusCode, l.ClaimChargesAmount ?? 0, l.ClaimDownPaymentAmount ?? 0,
                (l.ClaimChargesAmount ?? 0) + (l.ClaimDownPaymentAmount ?? 0), l.ClaimGlTranSetId, l.ClaimPaidAt, l.ClaimPaidBy, l.ClaimPaymentMethodCode,
                l.ClaimPaymentReference, l.ClaimPaymentGlTranSetId),
            l.RateMarginPct,
            l.DeliveryDate is { } delivered ? new LndCfDeliveryDto(delivered, l.DeliveryNoteNo, l.DeliverySerialNo, l.DeliveryGlTranSetId, l.SupplierPaymentDueDate) : null,
            l.SupplierPaidAt is { } paidAt ? new LndCfSupplierPaidDto(l.SupplierCommissionAmount ?? 0, l.SupplierCommissionReason,
                LndCfFinanceRules.SupplierDue(l).PayableAmount - (l.SupplierCommissionAmount ?? 0), l.SupplierPaymentReference, paidAt, l.SupplierPaidBy,
                l.SupplierPaymentGlTranSetId) : null);
    }

    /// <summary>Cancels a loan that has not been paid out. The schedule is kept as it was, for the record.</summary>
    public async Task CancelAsync(long loanId, LndLoanStatusChangeDto dto, CancellationToken cancellationToken)
    {
        if (await db.Set<LndLoan>().AnyAsync(x => x.LoanId == loanId && x.WorkflowInstanceId != null, cancellationToken))
            throw new BusinessRuleException("This finance is in its workflow: reject it at the open step instead.");
        await ChangeStatusAsync(loanId, Statuses.Cancelled,
            string.IsNullOrWhiteSpace(dto.Reason) ? throw BusinessValidationException.For("reason", "Give the reason for the cancellation.") : dto.Reason.Trim(),
            dto.RowVersion, cancellationToken);
    }

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
        IReadOnlyList<LndScheduleLine> Lines, decimal RatePct, string? DayCountCode, string? ScenarioName, decimal? ScenarioRatePct = null,
        LndCfSupplierOptionDto? Supplier = null);

    /// <summary>
    /// The terms a request can be made on, checked against the product version in force on the disbursement date. With a saved
    /// scenario of the Finance Calculator (Ahmed, 15 Nov 2026) the amount, dates, rate and schedule are the scenario's; the product,
    /// when not given, is the consumer finance product with terms in force.
    /// </summary>
    private async Task<Terms> CheckAsync(LndLoanRequestDto dto, CancellationToken cancellationToken)
    {
        var e = new Dictionary<string, string[]>();
        LndCfSupplierOptionDto? supplier = null;
        TlsScenario? scenario = null;
        if (dto.ScenarioId is { } scenarioId)
        {
            scenario = await ScenarioAsync(scenarioId, cancellationToken);
            if (scenario is null) e["scenarioId"] = ["The saved scenario was not found among yours."];
        }
        else if (dto.Calculation is { } calculation)
            scenario = FromCalculation(calculation);
        if (dto.ClientId is null) e["clientId"] = ["Choose the customer."];
        else if (!await db.Set<CrmClient>().AnyAsync(x => x.ClientId == dto.ClientId, cancellationToken)) e["clientId"] = ["The customer was not found."];
        if (scenario is null && dto.ScenarioId is null)
        {
            if (dto.ProductId is null && dto.SupplierApplicationId is null) e["productId"] = ["Choose the product."]; // a consumer finance: the CF product in force
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
            if (dto.RatePct is { } newRate && newRate != rate)
            {
                // the rate changed for this finance only: the schedule is worked out again on the scenario's terms; the scenario stays as saved
                if (newRate < 0 || newRate > 100) throw BusinessValidationException.For("ratePct", "The rate must be between 0 and 100%.");
                rate = newRate;
                lines = [.. TlsLoanCalculator.Generate(TlsLoanCalculator.Terms(Request(s, newRate)))
                    .Select(x => new LndScheduleLine(x.InstallmentNo, x.PaymentDate, x.OpeningPrincipal, x.PrincipalAmount, x.InterestAmount, x.ClosingPrincipal))];
            }
            else
                lines = [.. scenario.Lines.Select(x => new LndScheduleLine(x.InstallmentNo, x.PaymentDate, x.OpeningPrincipalAmount, x.PrincipalAmount,
                    x.InterestAmount, x.ClosingPrincipalAmount))];
            first = lines[0].DueDate;
        }
        else
        {
            amount = Money.Round(dto.PrincipalAmount!.Value, 2);
            term = dto.TermMonths!.Value;
            rate = version.AnnualRatePct;
            first = dto.FirstDueDate ?? LndSchedule.DefaultFirstDueDate(disbursement, version.RepaymentFrequencyCode);
            lines = [];
        }

        if (LndProductTerms.Licences[product.BusinessLineCode] == "CF")
        {
            var categories = await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.IsActive).Select(x => x.Code).ToListAsync(cancellationToken);
            // main data only: the amount is the price less the down payment (Credit decides the charges later)
            e = CheckGoods(dto, amount, categories);
            if (e.Count > 0) throw new BusinessValidationException(e);
            supplier = (await CfSuppliersAsync(cancellationToken)).FirstOrDefault(x => x.ApplicationId == dto.SupplierApplicationId);
            e = CheckSupplier(dto, supplier);
            if (e.Count > 0) throw new BusinessValidationException(e);
        }
        e = CheckTerms(version, amount, term, disbursement, first);
        // the products have no limits (Ahmed, 27 Nov 2026); the licence's legal limits per customer still apply to each finance
        var licence = LndProductTerms.Licences[product.BusinessLineCode];
        if (licence == LndParameterCatalog.Microfinance || product.ProductTypeCode == "CASH_ADVANCE")
        {
            var code = licence == LndParameterCatalog.Microfinance ? LndParameterCatalog.Codes.MaxFinancingAmount : LndParameterCatalog.Codes.CashMaxAmount;
            if (LegalLimit(amount, await parameters.NumberAsync(code, licence, disbursement, cancellationToken)) is { } limitProblem) e["principalAmount"] = [limitProblem];
        }
        if (scenario is not null && e.Count > 0)
            e = new Dictionary<string, string[]> { ["scenarioId"] = [.. e.Values.SelectMany(x => x).Select(x => "Scenario: " + x)] };
        if (e.Count > 0) throw new BusinessValidationException(e);
        if (scenario is null) lines = LndSchedule.Build(amount, term, version.RepaymentFrequencyCode, version.InterestMethodCode, version.AnnualRatePct, first);
        return new Terms(product, version, amount, term, disbursement, first, lines, rate, dayCount, scenario?.Scenario.ScenarioName,
            scenario?.Rates[0].InterestRatePct, supplier);
    }

    private sealed record TlsScenario(TlsLoanScenario Scenario, List<TlsLoanScenarioRate> Rates, List<TlsLoanScenarioLine> Lines);

    private const string Unsaved = "(unsaved calculator schedule)";

    private static string History(Terms t)
    {
        if (t.ScenarioName is null) return "Loan created.";
        var source = t.ScenarioName == Unsaved ? "a Finance Calculator schedule (not saved as a scenario)" : $"the saved scenario '{t.ScenarioName}'";
        return t.ScenarioRatePct == t.RatePct
            ? $"Created from {source}."
            : $"Created from {source}, rate changed from {t.ScenarioRatePct:0.######}% to {t.RatePct:0.######}% for this finance.";
    }

    /// <summary>
    /// A schedule the Finance Calculator handed back without saving it: its terms are worked out here again, so what is created is
    /// what the calculator showed, and then checked as a saved scenario would be.
    /// </summary>
    private static TlsScenario FromCalculation(TlsScenarioRequest request)
    {
        var terms = TlsLoanCalculator.Terms(request with { ScenarioId = null });
        var s = new TlsLoanScenario { ScenarioName = Unsaved };
        TlsLoanScenarioService.Apply(s, terms, request.RateIndexId);
        var rates = terms.Rates.Select(r => new TlsLoanScenarioRate { EffectiveDate = r.EffectiveDate, InterestRatePct = r.InterestRatePct, SequenceNo = r.SequenceNo }).ToList();
        var lines = TlsLoanCalculator.Generate(terms).Select(l => new TlsLoanScenarioLine
        {
            InstallmentNo = l.InstallmentNo, AccrualStartDate = l.AccrualStartDate, AccrualEndDate = l.AccrualEndDate, PaymentDate = l.PaymentDate,
            OpeningPrincipalAmount = l.OpeningPrincipal, PrincipalAmount = l.PrincipalAmount, InterestAmount = l.InterestAmount,
            TotalPaymentAmount = l.TotalPaymentAmount, ClosingPrincipalAmount = l.ClosingPrincipal, InterestRatePct = l.InterestRatePct,
            DayCountFraction = l.DayCountFraction,
        }).ToList();
        return new TlsScenario(s, rates, lines);
    }

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

    /// <summary>The user's saved scenarios of the Finance Calculator, for the New Finance page, with their amount and rate.</summary>
    public async Task<IReadOnlyList<LndScenarioOptionDto>> ScenariosAsync(CancellationToken cancellationToken)
    {
        var owner = currentUser.UserName ?? "SYSTEM";
        return await db.Set<TlsLoanScenario>().AsNoTracking().Where(x => x.CreatedBy == owner).OrderBy(x => x.ScenarioName)
            .Select(x => new LndScenarioOptionDto(x.ScenarioId, x.ScenarioName, x.PrincipalAmount,
                db.Set<TlsLoanScenarioRate>().Where(r => r.ScenarioId == x.ScenarioId).OrderBy(r => r.EffectiveDate).Select(r => r.InterestRatePct).FirstOrDefault(),
                x.ScheduleMode, x.PrincipalMethod))
            .ToListAsync(cancellationToken);
    }

    /// <summary>A saved scenario's terms as a calculator request, at another rate (one rate for the whole term).</summary>
    public static TlsScenarioRequest Request(TlsLoanScenario s, decimal ratePct) =>
        new(s.ScenarioId, s.ScenarioName, s.RateIndexId, s.PrincipalAmount, s.ContractDate, s.TenorValue, s.TenorUnit, ratePct, s.DayCountMethod,
            s.ScheduleMode, s.PrincipalMethod, s.InterestPaymentMethod, s.PrincipalPaymentStartDate, s.InterestPaymentStartDate,
            s.PrincipalFrequencyValue, s.PrincipalFrequencyUnit, s.PrincipalDateRule, s.InterestFrequencyValue, s.InterestFrequencyUnit, s.InterestDateRule,
            s.CombinedFrequencyValue, s.CombinedFrequencyUnit, s.CombinedDateRule, []);

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

    /// <summary>
    /// The calculator's day count as the loan's code, one for one, so the loan shows the basis its schedule was worked out on
    /// (end of day spreads the schedule's interest and does not use it; a part prepayment does).
    /// </summary>
    public static string DayCount(string method) => (method ?? "").Trim().ToUpperInvariant() switch
    {
        "ACT_360" => "ACT_360",
        "ACT_ACT" => "ACT_ACT",
        "30_360" => "THIRTY_360",
        "30_365" => "THIRTY_365",
        "30_ACT" => "THIRTY_ACT",
        _ => "ACT_365",
    };

    /// <summary>The loan's day count code as the calculator's method (the reverse of <see cref="DayCount"/>).</summary>
    public static string CalculatorDayCount(string? loanCode) => (loanCode ?? "").Trim().ToUpperInvariant() switch
    {
        "ACT_360" => "ACT_360",
        "ACT_ACT" => "ACT_ACT",
        "THIRTY_360" => "30_360",
        "THIRTY_365" => "30_365",
        "THIRTY_ACT" => "30_ACT",
        _ => "ACT_365",
    };

    /// <summary>
    /// The financed item of a consumer finance loan: an active category, a description, the price, and a down payment below it; the
    /// loan amount is the price less the down payment.
    /// </summary>
    public static Dictionary<string, string[]> CheckGoods(LndLoanRequestDto dto, decimal amount, IReadOnlyCollection<string> activeCategories, decimal addedToFinance = 0)
    {
        var e = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.GoodsCategoryCode)) e["goodsCategoryCode"] = ["Choose what is financed."];
        else if (!activeCategories.Contains(dto.GoodsCategoryCode.Trim().ToUpperInvariant())) e["goodsCategoryCode"] = ["The goods category is not active."];
        if (string.IsNullOrWhiteSpace(dto.GoodsDescription)) e["goodsDescription"] = ["Describe the financed item."];
        else if (dto.GoodsDescription.Trim().Length > 300) e["goodsDescription"] = ["The description is longer than 300 characters."];
        if (dto.GoodsBrandModel?.Trim().Length > 200) e["goodsBrandModel"] = ["The brand / model is longer than 200 characters."];
        if (dto.MerchantName?.Trim().Length > 200) e["merchantName"] = ["The merchant is longer than 200 characters."];
        var down = Money.Round(dto.DownPaymentAmount ?? 0, 2);
        if (dto.GoodsPriceAmount is not > 0) e["goodsPriceAmount"] = ["Enter the item's price."];
        else if (down < 0 || down >= dto.GoodsPriceAmount) e["downPaymentAmount"] = ["The down payment must be from 0 to less than the price."];
        else if (amount != Money.Round(dto.GoodsPriceAmount.Value, 2) - down + addedToFinance)
            e["principalAmount"] = [addedToFinance == 0
                ? $"The amount must be the price less the down payment ({Money.Round(dto.GoodsPriceAmount.Value, 2) - down:#,0.00})."
                : $"The amount must be the price less the down payment plus the charges added to the finance ({Money.Round(dto.GoodsPriceAmount.Value, 2) - down + addedToFinance:#,0.00})."];
        return e;
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>
    /// A finance's amount and term (the products have no limits): an amount above zero and a term of 1 to 120 months; the first
    /// instalment falls after the disbursement, within 3 months.
    /// </summary>
    public static Dictionary<string, string[]> CheckTerms(LndProductVersion v, decimal amount, int term, DateOnly disbursement, DateOnly first)
    {
        var e = new Dictionary<string, string[]>();
        if (amount <= 0) e["principalAmount"] = ["The amount must be more than zero."];
        if (term is < 1 or > 120) e["termMonths"] = ["The term must be from 1 to 120 months."];
        if (first <= disbursement) e["firstDueDate"] = ["The first instalment must fall after the disbursement date."];
        else if (first > disbursement.AddMonths(3)) e["firstDueDate"] = ["The first instalment must fall within 3 months of the disbursement."];
        return e;
    }

    /// <summary>The licence's legal limit per customer (microfinance, or cash financing): none approved stops the finance.</summary>
    public static string? LegalLimit(decimal amount, decimal? limit) =>
        limit is null ? "The licence's maximum financing per customer has no approved value. Approve it in Lending Parameters first."
        : amount > limit ? $"The amount is above the licence's maximum financing per customer of EGP {limit:#,0.##}." : null;

    public static LndLoanPreviewDto Preview(LndProductVersion v, decimal amount, IReadOnlyList<LndScheduleLine> lines, decimal ratePct)
    {
        var interest = lines.Sum(x => x.Interest);
        return new LndLoanPreviewDto(v.VersionNo, v.RepaymentFrequencyCode, v.InterestMethodCode, ratePct, lines.Count, lines[0].Total,
            interest, amount + interest, lines[0].DueDate, lines[^1].DueDate,
            [.. lines.Select(x => new LndScheduleLineDto(x.InstallmentNo, x.DueDate, x.OpeningPrincipal, x.Principal, x.Interest, x.Total, x.ClosingPrincipal))]);
    }
}
