using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using C = Capitra.Application.Modules.Lnd.LndParameterCatalog.Codes;
using S = Capitra.Application.Modules.Lnd.LndProductTerms.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndProductVersionDto(
    long ProductVersionId, int VersionNo, DateOnly EffectiveFromDate, string StatusCode, bool IsInForce,
    decimal MinAmount, decimal MaxAmount, int MinTermMonths, int MaxTermMonths, string RepaymentFrequencyCode, string InterestMethodCode,
    decimal AnnualRatePct, string DayCountCode, int LateFeeGraceDays, string LateFeeTypeCode, decimal? LateFeeRatePct, decimal? LateFeeFlatAmount,
    string LateFeeBaseCode, IReadOnlyList<string> PaymentOrder, bool IsInsuranceRequired, string? ChangeReason,
    string CreatedBy, string? RequestedBy, DateTime? RequestedAt, string? DecidedBy, DateTime? DecidedAt, string? DecisionNotes,
    bool CanApprove, string? ApproveBlockedReason, int RowVersion);

public sealed record LndLendingProductDto(
    long ProductId, string ProductCode, string ProductNameEn, string? ProductNameAr, string BusinessLineCode, string LicenceCode,
    string ProductTypeCode, bool IsActive, int? VersionInForce, IReadOnlyList<LndProductVersionDto> Versions);

public sealed record LndProductOptionsDto(
    IReadOnlyList<CrmOptionDto> Frequencies, IReadOnlyList<CrmOptionDto> InterestMethods, IReadOnlyList<CrmOptionDto> DayCounts,
    IReadOnlyList<CrmOptionDto> LateFeeTypes, IReadOnlyList<CrmOptionDto> LateFeeBases, IReadOnlyList<CrmOptionDto> PaymentParts);

public sealed record LndProductTermsPageDto(
    DateOnly Today, bool CanEdit, bool CanApprove, IReadOnlyList<LndLendingProductDto> Products, LndProductOptionsDto Options);

public sealed record LndProductVersionSaveDto(
    DateOnly? EffectiveFromDate, decimal? MinAmount, decimal? MaxAmount, int? MinTermMonths, int? MaxTermMonths, string? RepaymentFrequencyCode,
    string? InterestMethodCode, decimal? AnnualRatePct, string? DayCountCode, int? LateFeeGraceDays, string? LateFeeTypeCode, decimal? LateFeeRatePct,
    decimal? LateFeeFlatAmount, string? LateFeeBaseCode, IReadOnlyList<string>? PaymentOrder, bool IsInsuranceRequired, string? ChangeReason, int? RowVersion);

public sealed record LndProductDecisionDto(string? Notes);

/// <summary>
/// Administration > Product Setup > Lending Product Terms. The lending products are the catalog products of business line MICROFINANCE
/// or CONSUMER_FINANCE. Their terms are versions: a draft is prepared (copied from the version in force), sent for approval, and
/// approved by another user (four eyes, always). An approved version is used from its start date and is never changed.
/// </summary>
public sealed class LndProductVersionService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, ILndParameters parameters)
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    private DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    public async Task<LndProductTermsPageDto> GetAsync(bool canEdit, bool canApprove, CancellationToken cancellationToken)
    {
        var lines = LndProductTerms.Licences.Keys.ToList();
        var products = await db.Set<AdmProduct>().AsNoTracking().Where(x => lines.Contains(x.BusinessLineCode))
            .OrderBy(x => x.BusinessLineCode).ThenBy(x => x.ProductNameEn).ToListAsync(cancellationToken);
        var ids = products.Select(x => x.ProductId).ToList();
        var versions = await db.Set<LndProductVersion>().AsNoTracking().Where(x => ids.Contains(x.ProductId)).ToListAsync(cancellationToken);
        var today = Today;

        return new LndProductTermsPageDto(today, canEdit, canApprove,
            [.. products.Select(p =>
            {
                var inForce = LndProductTerms.InForce(versions, p.ProductId, today);
                return new LndLendingProductDto(p.ProductId, p.ProductCode, p.ProductNameEn, p.ProductNameAr, p.BusinessLineCode,
                    LndProductTerms.Licences[p.BusinessLineCode], p.ProductTypeCode, p.IsActive, inForce?.VersionNo,
                    [.. versions.Where(v => v.ProductId == p.ProductId).OrderByDescending(v => v.VersionNo)
                        .Select(v => ToDto(v, v.ProductVersionId == inForce?.ProductVersionId, canApprove))]);
            })],
            new LndProductOptionsDto(LndProductTerms.Frequencies, LndProductTerms.InterestMethods, LndProductTerms.DayCounts,
                LndProductTerms.LateFeeTypes, LndProductTerms.LateFeeBases, LndProductTerms.PaymentParts));
    }

    /// <summary>A new draft, copied from the latest approved version (or the defaults for a product without one), starting today.</summary>
    public async Task CreateDraftAsync(long productId, CancellationToken cancellationToken)
    {
        await ProductAsync(productId, cancellationToken);
        var versions = await db.Set<LndProductVersion>().Where(x => x.ProductId == productId).ToListAsync(cancellationToken);
        if (versions.Any(x => x.StatusCode is S.Draft or S.Pending))
            throw new BusinessRuleException("This product already has a version in preparation. Finish it or discard it first.");
        var from = versions.Where(x => x.StatusCode == S.Approved).MaxBy(x => x.VersionNo);
        var v = from is null ? LndProductTerms.Defaults(productId) : Copy(from);
        v.VersionNo = versions.Count == 0 ? 1 : versions.Max(x => x.VersionNo) + 1;
        v.EffectiveFromDate = Today;
        v.StatusCode = S.Draft;
        db.Set<LndProductVersion>().Add(v);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveDraftAsync(long versionId, LndProductVersionSaveDto dto, CancellationToken cancellationToken)
    {
        var v = await VersionAsync(versionId, cancellationToken);
        if (v.StatusCode != S.Draft) throw new BusinessRuleException("Only a draft can be changed.");
        if (dto.RowVersion is { } rv && rv != v.RowVersion) throw new ConflictException("Another user has changed this version. Reload the page.");
        var missing = new Dictionary<string, string[]>();
        T Req<T>(T? value, string field, string label) where T : struct
        {
            if (value is null) missing[field] = [$"Enter the {label}."];
            return value ?? default;
        }
        v.EffectiveFromDate = Req(dto.EffectiveFromDate, "effectiveFromDate", "start date");
        v.MinAmount = Req(dto.MinAmount, "minAmount", "minimum amount");
        v.MaxAmount = Req(dto.MaxAmount, "maxAmount", "maximum amount");
        v.MinTermMonths = Req(dto.MinTermMonths, "minTermMonths", "shortest term");
        v.MaxTermMonths = Req(dto.MaxTermMonths, "maxTermMonths", "longest term");
        v.AnnualRatePct = Req(dto.AnnualRatePct, "annualRatePct", "yearly rate");
        v.LateFeeGraceDays = Req(dto.LateFeeGraceDays, "lateFeeGraceDays", "grace days");
        v.RepaymentFrequencyCode = Code(dto.RepaymentFrequencyCode);
        v.InterestMethodCode = Code(dto.InterestMethodCode);
        v.DayCountCode = Code(dto.DayCountCode);
        v.LateFeeTypeCode = Code(dto.LateFeeTypeCode);
        v.LateFeeRatePct = v.LateFeeTypeCode is "PCT_ONCE" or "PCT_DAILY" ? dto.LateFeeRatePct : null;
        v.LateFeeFlatAmount = v.LateFeeTypeCode == "FLAT" ? dto.LateFeeFlatAmount : null;
        v.LateFeeBaseCode = string.IsNullOrWhiteSpace(dto.LateFeeBaseCode) ? "OVERDUE_INSTALMENT" : Code(dto.LateFeeBaseCode);
        v.PaymentOrder = string.Join(",", (dto.PaymentOrder ?? []).Select(Code));
        v.IsInsuranceRequired = dto.IsInsuranceRequired;
        v.ChangeReason = Blank(dto.ChangeReason);
        if (v.EffectiveFromDate < Today && !missing.ContainsKey("effectiveFromDate"))
            missing["effectiveFromDate"] = ["A version cannot start before today."];
        foreach (var (k, e) in LndProductTerms.Check(v)) missing.TryAdd(k, e);
        if (missing.Count > 0) throw new BusinessValidationException(missing);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Sends a draft for approval. Its terms must pass the checks and the licence's parameters on its start date.</summary>
    public async Task SubmitAsync(long versionId, CancellationToken cancellationToken)
    {
        var v = await VersionAsync(versionId, cancellationToken);
        if (v.StatusCode != S.Draft) throw new BusinessRuleException("Only a draft can be sent for approval.");
        await CheckAllAsync(v, cancellationToken);
        if (Blank(v.ChangeReason) is null) throw BusinessValidationException.For("changeReason", "Say what this version changes and why.");
        v.StatusCode = S.Pending;
        v.RequestedBy = User;
        v.RequestedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(long versionId, LndProductDecisionDto dto, CancellationToken cancellationToken)
    {
        var v = await PendingAsync(versionId, cancellationToken);
        if (SameUser(v.RequestedBy)) throw new BusinessRuleException("You sent this version for approval. Another user must approve it.");
        if (v.EffectiveFromDate < Today) throw new BusinessRuleException("The version's start date has passed. Reject it and prepare it again with a new date.");
        await CheckAllAsync(v, cancellationToken);
        Decide(v, S.Approved, Blank(dto.Notes));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(long versionId, LndProductDecisionDto dto, CancellationToken cancellationToken)
    {
        var v = await PendingAsync(versionId, cancellationToken);
        if (SameUser(v.RequestedBy)) throw new BusinessRuleException("You sent this version for approval. Take it back instead.");
        Decide(v, S.Rejected, Blank(dto.Notes) ?? throw BusinessValidationException.For("notes", "Give the reason for the rejection."));
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The requester takes a pending version back to draft, to change it.</summary>
    public async Task TakeBackAsync(long versionId, CancellationToken cancellationToken)
    {
        var v = await PendingAsync(versionId, cancellationToken);
        if (!SameUser(v.RequestedBy)) throw new BusinessRuleException("Only the user who sent the version can take it back.");
        v.StatusCode = S.Draft;
        v.RequestedBy = null;
        v.RequestedAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>A draft was never used, so it can be discarded.</summary>
    public async Task DiscardAsync(long versionId, CancellationToken cancellationToken)
    {
        var v = await VersionAsync(versionId, cancellationToken);
        if (v.StatusCode != S.Draft) throw new BusinessRuleException("Only a draft can be discarded.");
        db.Set<LndProductVersion>().Remove(v);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAllAsync(LndProductVersion v, CancellationToken cancellationToken)
    {
        var errors = LndProductTerms.Check(v);
        if (errors.Count > 0) throw new BusinessValidationException(errors);
        var p = await ProductAsync(v.ProductId, cancellationToken);
        var licence = LndProductTerms.Licences[p.BusinessLineCode];
        var isCash = p.ProductTypeCode == "CASH_ADVANCE";
        var problems = LndProductTerms.CheckAgainstParameters(v, licence,
            licence == LndParameterCatalog.Microfinance ? await parameters.NumberAsync(C.MaxFinancingAmount, licence, v.EffectiveFromDate, cancellationToken) : null,
            isCash ? await parameters.NumberAsync(C.CashMaxAmount, licence, v.EffectiveFromDate, cancellationToken) : null,
            isCash);
        if (problems.Count > 0) throw new BusinessRuleException(string.Join(" ", problems));
    }

    private async Task<AdmProduct> ProductAsync(long productId, CancellationToken cancellationToken)
    {
        var p = await db.Set<AdmProduct>().AsNoTracking().SingleOrDefaultAsync(x => x.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Product {productId} was not found.");
        return LndProductTerms.Licences.ContainsKey(p.BusinessLineCode) ? p
            : throw new BusinessRuleException("Lending terms are only for products of business line Microfinance or Consumer Finance.");
    }

    private async Task<LndProductVersion> VersionAsync(long id, CancellationToken cancellationToken) =>
        await db.Set<LndProductVersion>().SingleOrDefaultAsync(x => x.ProductVersionId == id, cancellationToken)
            ?? throw new NotFoundException($"Product version {id} was not found.");

    private async Task<LndProductVersion> PendingAsync(long id, CancellationToken cancellationToken)
    {
        var v = await VersionAsync(id, cancellationToken);
        return v.StatusCode == S.Pending ? v : throw new BusinessRuleException("This version is not waiting for approval.");
    }

    private void Decide(LndProductVersion v, string status, string? notes)
    {
        v.StatusCode = status;
        v.DecidedBy = User;
        v.DecidedAt = clock.UtcNow;
        v.DecisionNotes = notes;
    }

    private LndProductVersionDto ToDto(LndProductVersion v, bool inForce, bool canApprove) =>
        new(v.ProductVersionId, v.VersionNo, v.EffectiveFromDate, v.StatusCode, inForce, v.MinAmount, v.MaxAmount, v.MinTermMonths, v.MaxTermMonths,
            v.RepaymentFrequencyCode, v.InterestMethodCode, v.AnnualRatePct, v.DayCountCode, v.LateFeeGraceDays, v.LateFeeTypeCode, v.LateFeeRatePct,
            v.LateFeeFlatAmount, v.LateFeeBaseCode, LndProductTerms.Parts(v.PaymentOrder), v.IsInsuranceRequired, v.ChangeReason,
            v.CreatedBy, v.RequestedBy, v.RequestedAt, v.DecidedBy, v.DecidedAt, v.DecisionNotes,
            v.StatusCode == S.Pending && canApprove && !SameUser(v.RequestedBy),
            v.StatusCode == S.Pending && SameUser(v.RequestedBy) ? "You sent this version; another user must approve it." : null, v.RowVersion);

    private static LndProductVersion Copy(LndProductVersion f) => new()
    {
        ProductId = f.ProductId, MinAmount = f.MinAmount, MaxAmount = f.MaxAmount, MinTermMonths = f.MinTermMonths, MaxTermMonths = f.MaxTermMonths,
        RepaymentFrequencyCode = f.RepaymentFrequencyCode, InterestMethodCode = f.InterestMethodCode, AnnualRatePct = f.AnnualRatePct,
        DayCountCode = f.DayCountCode, LateFeeGraceDays = f.LateFeeGraceDays, LateFeeTypeCode = f.LateFeeTypeCode, LateFeeRatePct = f.LateFeeRatePct,
        LateFeeFlatAmount = f.LateFeeFlatAmount, LateFeeBaseCode = f.LateFeeBaseCode, PaymentOrder = f.PaymentOrder,
        IsInsuranceRequired = f.IsInsuranceRequired,
    };

    private static string Code(string? s) => (s ?? "").Trim().ToUpperInvariant();

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
