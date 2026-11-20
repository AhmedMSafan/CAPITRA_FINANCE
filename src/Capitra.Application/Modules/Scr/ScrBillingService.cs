using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>A tier of inquiry prices: inquiries FromInquiry to ToInquiry of a period (empty = and above) at UnitPrice each.</summary>
public sealed record ScrTierDto(int FromInquiry, int? ToInquiry, decimal UnitPrice);

public sealed record ScrPricePlanDto(
    long PricePlanId, string PlanCode, string NameEn, string NameAr, decimal UserMonthlyFee, decimal InquiryFee, int FreeInquiries,
    IReadOnlyList<ScrTierDto> Tiers, string CurrencyCode, bool IsActive, string? Notes, int CompanyCount, string CreatedBy, DateTime CreatedAt,
    string? UpdatedBy, DateTime? UpdatedAt);

/// <param name="Tiers">Empty = one flat InquiryFee after the free inquiries. Otherwise they start right after the free inquiries, follow
/// each other without gaps, and the last has no end.</param>
public sealed record ScrPricePlanRequest(
    string? PlanCode, string? NameEn, string? NameAr, decimal? UserMonthlyFee, decimal? InquiryFee, bool IsActive, string? Notes,
    int? FreeInquiries = null, IReadOnlyList<ScrTierDto>? Tiers = null);

public sealed record ScrInvoiceLineDto(int LineNo, string UserName, string? FullName, string? MonthsText, int UserMonths, decimal UserAmount, int InquiryCount);

/// <summary>The period's inquiries priced in one band: FREE, TIER or FLAT.</summary>
public sealed record ScrInvoiceBandDto(int BandNo, string BandTypeCode, int FromInquiry, int? ToInquiry, decimal UnitPrice, int Quantity, decimal Amount);

/// <summary>An invoice, issued or only previewed (InvoiceId 0). PeriodComplete is false while the period has not ended yet.</summary>
public sealed record ScrInvoiceDto(
    long InvoiceId, string? InvoiceNo, long MemberCompanyId, string CompanyCode, string CompanyNameEn, string CompanyNameAr,
    DateOnly PeriodFrom, DateOnly PeriodTo, string PeriodCode, string PlanCode, decimal UserMonthlyFee, decimal InquiryFee, int FreeInquiries,
    string CurrencyCode, int UserMonths, decimal UserAmount, int InquiryCount, decimal InquiryAmount, decimal TotalAmount, string StatusCode,
    DateTime? IssuedAt, string? IssuedBy, DateTime? CancelledAt, string? CancelledBy, string? CancelReason, string? Notes, bool PeriodComplete,
    IReadOnlyList<ScrInvoiceLineDto> Lines, IReadOnlyList<ScrInvoiceBandDto> Bands);

public sealed record ScrInvoiceRequest(DateOnly? PeriodFrom, string? Notes);

public sealed record ScrCancelInvoiceRequest(string? Reason);

/// <summary>A company's billing at a glance: its plan, its current period so far, and what it has been invoiced.</summary>
public sealed record ScrBillingCompanyDto(
    long MemberCompanyId, string CompanyCode, string NameEn, string NameAr, string StatusCode, long? PricePlanId, string? PlanCode,
    string BillingCycleCode, DateOnly CurrentFrom, DateOnly CurrentTo, string CurrentPeriodCode, int ActiveUsersNow, int CurrentUserMonths,
    int CurrentInquiries, decimal? CurrentAmount, bool CurrentInvoiced, int InvoiceCount, decimal InvoicedTotal, DateOnly? LastInvoicedTo);

public sealed record ScrInvoiceQuery(long? MemberCompanyId, string? StatusCode, int? Year);

/// <summary>Calendar periods and amounts of the screening service's billing.</summary>
public static class ScrBilling
{
    public static readonly string[] Cycles = ["MONTHLY", "QUARTERLY", "YEARLY"];

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public static DateOnly CairoDate(DateTime utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Cairo));

    /// <summary>The UTC instant a Cairo day starts.</summary>
    public static DateTime CairoStartUtc(DateOnly day) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Cairo);

    /// <summary>The calendar period of the cycle that contains the day.</summary>
    public static (DateOnly From, DateOnly To) PeriodOf(string cycle, DateOnly day)
    {
        var from = cycle switch
        {
            "YEARLY" => new DateOnly(day.Year, 1, 1),
            "QUARTERLY" => new DateOnly(day.Year, ((day.Month - 1) / 3 * 3) + 1, 1),
            _ => new DateOnly(day.Year, day.Month, 1),
        };
        var to = (cycle switch { "YEARLY" => from.AddYears(1), "QUARTERLY" => from.AddMonths(3), _ => from.AddMonths(1) }).AddDays(-1);
        return (from, to);
    }

    /// <summary>2026-11, 2026-Q4 or 2026.</summary>
    public static string PeriodCode(string cycle, DateOnly from) => cycle switch
    {
        "YEARLY" => $"{from.Year}",
        "QUARTERLY" => $"{from.Year}-Q{((from.Month - 1) / 3) + 1}",
        _ => $"{from.Year}-{from.Month:00}",
    };

    public static string CycleOf(DateOnly from, DateOnly to) =>
        to == from.AddYears(1).AddDays(-1) ? "YEARLY" : to == from.AddMonths(3).AddDays(-1) ? "QUARTERLY" : "MONTHLY";

    public static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public const int MaxTiers = 20;

    private static readonly System.Text.Json.JsonSerializerOptions Json = new(System.Text.Json.JsonSerializerDefaults.Web);

    public static IReadOnlyList<ScrTierDto> ReadTiers(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] : System.Text.Json.JsonSerializer.Deserialize<List<ScrTierDto>>(json, Json) ?? [];

    public static string? WriteTiers(IReadOnlyList<ScrTierDto> tiers) => tiers.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(tiers, Json);

    /// <summary>A plan's inquiry price schedule: the free inquiries, then its tiers, or the flat fee for every inquiry after them.</summary>
    public static List<(string Type, int From, int? To, decimal Price)> Schedule(int freeInquiries, decimal flatFee, IReadOnlyList<ScrTierDto> tiers)
    {
        var bands = new List<(string, int, int?, decimal)>();
        if (freeInquiries > 0)
            bands.Add(("FREE", 1, freeInquiries, 0m));
        if (tiers.Count == 0)
            bands.Add(("FLAT", freeInquiries + 1, null, flatFee));
        else
            bands.AddRange(tiers.Select(t => ("TIER", t.FromInquiry, t.ToInquiry, t.UnitPrice)));
        return bands;
    }

    /// <summary>The period's inquiries spread over the schedule: each priced at the rate of the band it falls in (graduated).</summary>
    public static List<ScrInvoiceBandDto> Price(List<(string Type, int From, int? To, decimal Price)> schedule, int inquiries)
    {
        var result = new List<ScrInvoiceBandDto>();
        var no = 0;
        foreach (var (type, from, to, price) in schedule)
        {
            var quantity = Math.Max(0, Math.Min(inquiries, to ?? int.MaxValue) - from + 1);
            if (quantity > 0)
                result.Add(new ScrInvoiceBandDto(++no, type, from, to, price, quantity, Money(quantity * price)));
        }

        return result;
    }

    /// <summary>Checks a plan's free inquiries and tiers; adds the problems to the errors.</summary>
    public static void ValidateTiers(Dictionary<string, string[]> e, int? freeInquiries, IReadOnlyList<ScrTierDto>? tiers)
    {
        if (freeInquiries is not { } free || free < 0 || free > 10_000_000)
            e["freeInquiries"] = ["Enter the free inquiries per invoice period, 0 or more."];
        if (tiers is null || tiers.Count == 0)
            return;
        if (tiers.Count > MaxTiers)
        {
            e["tiers"] = [$"Use at most {MaxTiers} tiers."];
            return;
        }

        var expected = (freeInquiries ?? 0) + 1;
        for (var i = 0; i < tiers.Count; i++)
        {
            var t = tiers[i];
            var last = i == tiers.Count - 1;
            if (t.FromInquiry != expected)
                e["tiers"] = [$"Tier {i + 1} must start at inquiry {expected}: tiers start right after the free inquiries and follow each other without gaps."];
            else if (!last && (t.ToInquiry is not { } to || to < t.FromInquiry))
                e["tiers"] = [$"Tier {i + 1} needs an end, at or after its start."];
            else if (last && t.ToInquiry is not null)
                e["tiers"] = ["The last tier has no end: it prices every inquiry above its start."];
            else if (t.UnitPrice < 0 || t.UnitPrice > 10_000_000 || t.UnitPrice != Money(t.UnitPrice))
                e["tiers"] = [$"Tier {i + 1}: enter a price in EGP, 0 or more, up to 2 decimals."];
            if (e.ContainsKey("tiers"))
                return;
            expected = (t.ToInquiry ?? 0) + 1;
        }
    }
}

/// <summary>
/// Screening service billing: price plans, and per member company an invoice per calendar period (month, quarter or year, as set on
/// the company). The user fee is per user per month: a user active on any day of a month (SCR_USER_ACTIVITIES) is billed for it,
/// the administrator included; invitations not yet activated are not billed. The inquiry fee is per inquiry made in the period,
/// whatever its result, after the plan's free inquiries of the period, at its graduated tiers or flat fee. Prices are copied onto the
/// invoice when it is issued. Nothing is deleted: a wrong invoice is cancelled with
/// a reason and issued again.
/// </summary>
public sealed class ScrBillingService(ICapitraDbContext db, IScrCompanyUserCounts users, ICurrentUser currentUser, IClock clock)
{
    // ---- price plans ------------------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<ScrPricePlanDto>> PlansAsync(CancellationToken ct)
    {
        var counts = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => c.PricePlanId != null).GroupBy(c => c.PricePlanId!.Value)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return [.. (await db.Set<ScrPricePlan>().AsNoTracking().OrderByDescending(p => p.IsActive).ThenBy(p => p.PlanCode).ToListAsync(ct))
            .Select(p => ToDto(p, counts.GetValueOrDefault(p.PricePlanId)))];
    }

    public async Task<ScrPricePlanDto> SavePlanAsync(long? id, ScrPricePlanRequest r, CancellationToken ct)
    {
        var e = new Dictionary<string, string[]>();
        var code = ScrValidation.Code(r.PlanCode);
        if (code.Length is < 2 or > 30 || code.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_')))
            e["planCode"] = ["Use 2 to 30 letters, digits, - or _."];
        else if (await db.Set<ScrPricePlan>().AnyAsync(p => p.PlanCode == code && p.PricePlanId != (id ?? 0), ct))
            e["planCode"] = ["This plan code is already used."];
        if (ScrValidation.Clean(r.NameEn) is null)
            e["nameEn"] = ["Enter the English name."];
        if (ScrValidation.Clean(r.NameAr) is null)
            e["nameAr"] = ["Enter the Arabic name."];
        if (r.UserMonthlyFee is not { } uf || uf < 0 || uf > 10_000_000 || uf != ScrBilling.Money(uf))
            e["userMonthlyFee"] = ["Enter the fee per user per month in EGP (0 or more, up to 2 decimals)."];
        var tiers = r.Tiers ?? [];
        var freeInquiries = r.FreeInquiries ?? 0;
        if (tiers.Count == 0 && (r.InquiryFee is not { } inf || inf < 0 || inf > 10_000_000 || inf != ScrBilling.Money(inf)))
            e["inquiryFee"] = ["Enter the fee per inquiry in EGP (0 or more, up to 2 decimals)."];
        ScrBilling.ValidateTiers(e, freeInquiries, tiers);
        ScrValidation.Throw(e);

        var p = id is { } x
            ? await db.Set<ScrPricePlan>().FirstOrDefaultAsync(q => q.PricePlanId == x, ct) ?? throw new NotFoundException("The price plan was not found.")
            : new ScrPricePlan();
        p.PlanCode = code;
        p.NameEn = r.NameEn!.Trim();
        p.NameAr = r.NameAr!.Trim();
        p.UserMonthlyFee = r.UserMonthlyFee!.Value;
        // with tiers, the flat fee shown is the last (open) tier's price
        p.InquiryFee = tiers.Count == 0 ? r.InquiryFee!.Value : tiers[^1].UnitPrice;
        p.FreeInquiries = freeInquiries;
        p.InquiryTiers = ScrBilling.WriteTiers(tiers);
        p.CurrencyCode = "EGP";
        p.IsActive = r.IsActive;
        p.Notes = ScrValidation.Clean(r.Notes);
        if (id is null)
            db.Add(p);
        await db.SaveChangesAsync(ct);
        return (await PlansAsync(ct)).First(q => q.PricePlanId == p.PricePlanId);
    }

    // ---- overview ---------------------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<ScrBillingCompanyDto>> OverviewAsync(CancellationToken ct)
    {
        var today = ScrBilling.CairoDate(clock.UtcNow);
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().OrderBy(c => c.CompanyCode).ToListAsync(ct);
        var plans = await db.Set<ScrPricePlan>().AsNoTracking().ToDictionaryAsync(p => p.PricePlanId, ct);
        var invoices = await db.Set<ScrInvoice>().AsNoTracking().Where(i => i.StatusCode == "ISSUED")
            .Select(i => new { i.MemberCompanyId, i.PeriodFrom, i.PeriodTo, i.TotalAmount }).ToListAsync(ct);
        var activeNow = await db.Set<ScrUserActivity>().AsNoTracking().Where(a => a.ActiveTo == null)
            .GroupBy(a => a.MemberCompanyId).Select(g => new { g.Key, Count = g.Select(a => a.UserId).Distinct().Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var list = new List<ScrBillingCompanyDto>();
        foreach (var c in companies)
        {
            var (from, to) = ScrBilling.PeriodOf(c.BillingCycleCode, today);
            var mine = invoices.Where(i => i.MemberCompanyId == c.MemberCompanyId).ToList();
            var plan = c.PricePlanId is { } pid ? plans.GetValueOrDefault(pid) : null;
            var calc = await CalculateAsync(c.MemberCompanyId, from, to, ct);
            var amount = plan is null ? (decimal?)null : Total(calc, plan);
            list.Add(new ScrBillingCompanyDto(
                c.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr, c.StatusCode, c.PricePlanId, plan?.PlanCode, c.BillingCycleCode,
                from, to, ScrBilling.PeriodCode(c.BillingCycleCode, from), activeNow.GetValueOrDefault(c.MemberCompanyId),
                calc.Sum(l => l.Months.Count), calc.Sum(l => l.Inquiries), amount,
                mine.Any(i => i.PeriodFrom <= to && i.PeriodTo >= from), mine.Count, mine.Sum(i => i.TotalAmount),
                mine.Count == 0 ? null : mine.Max(i => i.PeriodTo)));
        }

        return list;
    }

    // ---- invoices ---------------------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<ScrInvoiceDto>> InvoicesAsync(ScrInvoiceQuery q, CancellationToken ct)
    {
        var rows = db.Set<ScrInvoice>().AsNoTracking();
        if (q.MemberCompanyId is { } c)
            rows = rows.Where(i => i.MemberCompanyId == c);
        if (!string.IsNullOrWhiteSpace(q.StatusCode))
            rows = rows.Where(i => i.StatusCode == q.StatusCode);
        if (q.Year is { } y)
            rows = rows.Where(i => i.PeriodFrom.Year == y);
        var list = await rows.OrderByDescending(i => i.PeriodFrom).ThenByDescending(i => i.InvoiceId).Take(1000).ToListAsync(ct);
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().ToDictionaryAsync(m => m.MemberCompanyId, ct);
        var today = ScrBilling.CairoDate(clock.UtcNow);
        return [.. list.Select(i => ToDto(i, companies[i.MemberCompanyId], [], today))];
    }

    public async Task<ScrInvoiceDto> InvoiceAsync(long id, CancellationToken ct)
    {
        var i = await db.Set<ScrInvoice>().AsNoTracking().Include(x => x.Lines).Include(x => x.Bands).FirstOrDefaultAsync(x => x.InvoiceId == id, ct)
            ?? throw new NotFoundException("The invoice was not found.");
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().FirstAsync(m => m.MemberCompanyId == i.MemberCompanyId, ct);
        return ToDto(i, company, [.. i.Lines.OrderBy(l => l.LineNo).Select(ToDto)], ScrBilling.CairoDate(clock.UtcNow)) with
        {
            Bands = [.. i.Bands.OrderBy(b => b.BandNo).Select(b => new ScrInvoiceBandDto(b.BandNo, b.BandTypeCode, b.FromInquiry, b.ToInquiry, b.UnitPrice, b.Quantity, b.Amount))],
        };
    }

    /// <summary>What the company would be invoiced for the period starting on the day given (nothing is saved).</summary>
    public async Task<ScrInvoiceDto> PreviewAsync(long companyId, DateOnly? periodFrom, CancellationToken ct)
    {
        var (company, plan, from, to) = await PrepareAsync(companyId, periodFrom, ct);
        var lines = await LinesAsync(company.MemberCompanyId, from, to, plan, ct);
        var invoice = Build(company, plan, from, to, lines, notes: null);
        return ToDto(invoice, company, [.. lines.Select(ToDto)], ScrBilling.CairoDate(clock.UtcNow)) with
        {
            StatusCode = "PREVIEW",
            Bands = [.. invoice.Bands.Select(b => new ScrInvoiceBandDto(b.BandNo, b.BandTypeCode, b.FromInquiry, b.ToInquiry, b.UnitPrice, b.Quantity, b.Amount))],
        };
    }

    /// <summary>Issues the invoice for the period: prices copied from the plan, one line per user. The period may not overlap another.</summary>
    public async Task<ScrInvoiceDto> IssueAsync(long companyId, ScrInvoiceRequest r, CancellationToken ct)
    {
        var (company, plan, from, to) = await PrepareAsync(companyId, r.PeriodFrom, ct);
        var lines = await LinesAsync(company.MemberCompanyId, from, to, plan, ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var year = from.Year;
        var prefix = $"INV-{year}-";
        var last = await db.Set<ScrInvoice>().Where(i => i.InvoiceNo.StartsWith(prefix)).OrderByDescending(i => i.InvoiceNo)
            .Select(i => i.InvoiceNo).FirstOrDefaultAsync(ct);
        var next = last is null ? 1 : int.Parse(last[prefix.Length..], System.Globalization.CultureInfo.InvariantCulture) + 1;
        var invoice = Build(company, plan, from, to, lines, ScrValidation.Clean(r.Notes));
        invoice.InvoiceNo = $"{prefix}{next:000000}";
        invoice.IssuedAt = clock.UtcNow;
        invoice.IssuedBy = currentUser.UserName ?? "SYSTEM";
        invoice.Lines = lines;
        db.Add(invoice);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await InvoiceAsync(invoice.InvoiceId, ct);
    }

    /// <summary>Cancels an issued invoice (kept, with the reason); its period can then be invoiced again.</summary>
    public async Task<ScrInvoiceDto> CancelAsync(long id, ScrCancelInvoiceRequest r, CancellationToken ct)
    {
        var i = await db.Set<ScrInvoice>().FirstOrDefaultAsync(x => x.InvoiceId == id, ct) ?? throw new NotFoundException("The invoice was not found.");
        if (i.StatusCode != "ISSUED")
            throw new BusinessRuleException("The invoice is already cancelled.");
        if (ScrValidation.Clean(r.Reason) is not { } reason)
            throw BusinessValidationException.For("reason", "Enter why the invoice is cancelled.");
        i.StatusCode = "CANCELLED";
        i.CancelledAt = clock.UtcNow;
        i.CancelledBy = currentUser.UserName ?? "SYSTEM";
        i.CancelReason = reason;
        await db.SaveChangesAsync(ct);
        return await InvoiceAsync(id, ct);
    }

    // ---- calculation ------------------------------------------------------------------------------------------------------------

    private async Task<(ScrMemberCompany Company, ScrPricePlan Plan, DateOnly From, DateOnly To)> PrepareAsync(long companyId, DateOnly? periodFrom, CancellationToken ct)
    {
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().FirstOrDefaultAsync(c => c.MemberCompanyId == companyId, ct)
            ?? throw new NotFoundException($"Member company {companyId} was not found.");
        if (company.PricePlanId is not { } planId)
            throw new BusinessRuleException("Choose the company's price plan first (Edit company › Billing).");
        var plan = await db.Set<ScrPricePlan>().AsNoTracking().FirstAsync(p => p.PricePlanId == planId, ct);
        var day = periodFrom ?? ScrBilling.CairoDate(clock.UtcNow);
        var (from, to) = ScrBilling.PeriodOf(company.BillingCycleCode, day);
        if (periodFrom is { } given && given != from)
            throw BusinessValidationException.For("periodFrom", "The period must start on the first day of a month, quarter or year, as the company is invoiced.");
        if (from > ScrBilling.CairoDate(clock.UtcNow))
            throw BusinessValidationException.For("periodFrom", "The period has not started yet.");
        if (await db.Set<ScrInvoice>().AnyAsync(i => i.MemberCompanyId == companyId && i.StatusCode == "ISSUED" && i.PeriodFrom <= to && i.PeriodTo >= from, ct))
            throw new BusinessRuleException("This period is already invoiced. Cancel that invoice first to issue it again.");
        return (company, plan, from, to);
    }

    private sealed record UserUse(string UserName, List<string> Months, int Inquiries);

    /// <summary>Per user: the months of the period they were active on at least one day, and the inquiries they made in it.</summary>
    private async Task<List<UserUse>> CalculateAsync(long companyId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var activity = await db.Set<ScrUserActivity>().AsNoTracking()
            .Where(a => a.MemberCompanyId == companyId && a.ActiveFrom <= to && (a.ActiveTo == null || a.ActiveTo >= from))
            .Select(a => new { a.UserId, a.UserName, a.ActiveFrom, a.ActiveTo }).ToListAsync(ct);
        var startUtc = ScrBilling.CairoStartUtc(from);
        var endUtc = ScrBilling.CairoStartUtc(to.AddDays(1));
        var inquiries = await db.Set<ScrInquiry>().AsNoTracking()
            .Where(i => i.MemberCompanyId == companyId && i.ScreenedAt >= startUtc && i.ScreenedAt < endUtc)
            .GroupBy(i => i.RequestedBy).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);

        var months = new List<(string Code, DateOnly Start, DateOnly End)>();
        for (var m = new DateOnly(from.Year, from.Month, 1); m <= to; m = m.AddMonths(1))
            months.Add(($"{m.Year}-{m.Month:00}", m, m.AddMonths(1).AddDays(-1)));

        var byUser = new Dictionary<string, UserUse>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in activity.GroupBy(a => a.UserId))
        {
            var name = g.OrderByDescending(a => a.ActiveFrom).First().UserName;
            var active = months.Where(m => g.Any(a => a.ActiveFrom <= m.End && (a.ActiveTo is not { } t || t >= m.Start))).Select(m => m.Code).ToList();
            byUser[name] = byUser.TryGetValue(name, out var same) ? same with { Months = [.. same.Months.Union(active).Order(StringComparer.Ordinal)] } : new UserUse(name, active, 0);
        }

        foreach (var i in inquiries)
            byUser[i.Key] = byUser.TryGetValue(i.Key, out var u) ? u with { Inquiries = u.Inquiries + i.Count } : new UserUse(i.Key, [], i.Count);
        return [.. byUser.Values.OrderBy(u => u.UserName, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<List<ScrInvoiceLine>> LinesAsync(long companyId, DateOnly from, DateOnly to, ScrPricePlan plan, CancellationToken ct)
    {
        var names = await users.FullNamesAsync(companyId, ct);
        var lineNo = 0;
        return [.. (await CalculateAsync(companyId, from, to, ct)).Where(u => u.Months.Count > 0 || u.Inquiries > 0).Select(u =>
        {
            return new ScrInvoiceLine
            {
                LineNo = ++lineNo, UserName = u.UserName, FullName = names.GetValueOrDefault(u.UserName),
                MonthsText = u.Months.Count == 0 ? null : string.Join(", ", u.Months), UserMonths = u.Months.Count,
                UserAmount = ScrBilling.Money(u.Months.Count * plan.UserMonthlyFee), InquiryCount = u.Inquiries,
            };
        })];
    }

    private static List<ScrInvoiceBandDto> InquiryBands(ScrPricePlan plan, int inquiries) =>
        ScrBilling.Price(ScrBilling.Schedule(plan.FreeInquiries, plan.InquiryFee, ScrBilling.ReadTiers(plan.InquiryTiers)), inquiries);

    private static decimal Total(List<UserUse> calc, ScrPricePlan plan) =>
        calc.Sum(u => ScrBilling.Money(u.Months.Count * plan.UserMonthlyFee)) + InquiryBands(plan, calc.Sum(u => u.Inquiries)).Sum(b => b.Amount);

    private static ScrInvoice Build(ScrMemberCompany company, ScrPricePlan plan, DateOnly from, DateOnly to, List<ScrInvoiceLine> lines, string? notes)
    {
        var userAmount = lines.Sum(l => l.UserAmount);
        var inquiries = lines.Sum(l => l.InquiryCount);
        var bands = InquiryBands(plan, inquiries);
        var inquiryAmount = bands.Sum(b => b.Amount);
        return new ScrInvoice
        {
            MemberCompanyId = company.MemberCompanyId, PeriodFrom = from, PeriodTo = to, PricePlanId = plan.PricePlanId, PlanCode = plan.PlanCode,
            UserMonthlyFee = plan.UserMonthlyFee, InquiryFee = plan.InquiryFee, FreeInquiries = plan.FreeInquiries, CurrencyCode = plan.CurrencyCode,
            UserMonths = lines.Sum(l => l.UserMonths), UserAmount = userAmount, InquiryCount = inquiries,
            InquiryAmount = inquiryAmount, TotalAmount = userAmount + inquiryAmount, StatusCode = "ISSUED", Notes = notes,
            Bands = [.. bands.Select(b => new ScrInvoiceBand
            {
                BandNo = b.BandNo, BandTypeCode = b.BandTypeCode, FromInquiry = b.FromInquiry, ToInquiry = b.ToInquiry, UnitPrice = b.UnitPrice,
                Quantity = b.Quantity, Amount = b.Amount,
            })],
        };
    }

    private static ScrPricePlanDto ToDto(ScrPricePlan p, int companies) => new(
        p.PricePlanId, p.PlanCode, p.NameEn, p.NameAr, p.UserMonthlyFee, p.InquiryFee, p.FreeInquiries, ScrBilling.ReadTiers(p.InquiryTiers),
        p.CurrencyCode, p.IsActive, p.Notes, companies, p.CreatedBy, p.CreatedAt, p.UpdatedBy, p.UpdatedAt);

    private static ScrInvoiceLineDto ToDto(ScrInvoiceLine l) =>
        new(l.LineNo, l.UserName, l.FullName, l.MonthsText, l.UserMonths, l.UserAmount, l.InquiryCount);

    private static ScrInvoiceDto ToDto(ScrInvoice i, ScrMemberCompany c, IReadOnlyList<ScrInvoiceLineDto> lines, DateOnly today)
    {
        var cycle = ScrBilling.CycleOf(i.PeriodFrom, i.PeriodTo);
        return new ScrInvoiceDto(
            i.InvoiceId, i.InvoiceId == 0 ? null : i.InvoiceNo, i.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr, i.PeriodFrom, i.PeriodTo,
            ScrBilling.PeriodCode(cycle, i.PeriodFrom), i.PlanCode, i.UserMonthlyFee, i.InquiryFee, i.FreeInquiries, i.CurrencyCode, i.UserMonths,
            i.UserAmount, i.InquiryCount, i.InquiryAmount, i.TotalAmount, i.StatusCode, i.InvoiceId == 0 ? null : i.IssuedAt,
            i.InvoiceId == 0 ? null : i.IssuedBy, i.CancelledAt, i.CancelledBy, i.CancelReason, i.Notes, today > i.PeriodTo, lines, []);
    }
}
