using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_RULES_PKG: the program limits an application and a finance offer must keep (null: valid).</summary>
public sealed class MffRulesPackage(ICapitraDbContext db, MffParamPackage param)
{
    /// <summary>VALIDATE_APPLICATION: the broken rules, one per line, or null.</summary>
    public async Task<string?> ValidateApplicationAsync(long applicationId, CancellationToken ct)
    {
        var a = await db.Set<MffApplication>().AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct);
        if (a is null)
            return "طلب التمويل غير موجود.";
        var msg = new List<string>();
        if ((a.MonthlyIncomeTotal ?? 0) <= 0)
            msg.Add("يجب تسجيل دخل شهري محتسب أكبر من صفر.");
        if ((a.PropertyPrice ?? 0) <= 0)
            msg.Add("يجب تسجيل سعر الوحدة.");
        if ((a.RequestedFinanceAmount ?? 0) < 0)
            msg.Add("مبلغ التمويل المطلوب غير صحيح.");
        if ((a.DownpaymentAmount ?? 0) < 0)
            msg.Add("قيمة مقدم العميل غير صحيحة.");

        if ((a.PropertyPrice ?? 0) > 0)
        {
            var ltv = Math.Round((a.RequestedFinanceAmount ?? 0) / a.PropertyPrice!.Value * 100, 4, MidpointRounding.AwayFromZero);
            var dpPct = Math.Round((a.DownpaymentAmount ?? 0) / a.PropertyPrice.Value * 100, 4, MidpointRounding.AwayFromZero);
            var maxLtv = await Param(a, "MAX_LTV_PCT", a.ApplicationDate, ct);
            var minDp = await Param(a, "MIN_DOWNPAYMENT_PCT", a.ApplicationDate, ct);
            var maxDp = await Param(a, "MAX_DOWNPAYMENT_PCT", a.ApplicationDate, ct);
            if (maxLtv is not null && ltv > maxLtv + 0.0001m)
                msg.Add("نسبة التمويل إلى قيمة الوحدة تتجاوز الحد المسموح (" + Num(maxLtv) + "%).");
            if (minDp is not null && dpPct < minDp - 0.0001m)
                msg.Add("نسبة مقدم العميل أقل من الحد الأدنى (" + Num(minDp) + "%).");
            if (maxDp is not null && dpPct > maxDp + 0.0001m)
                msg.Add("نسبة مقدم العميل تتجاوز الحد الأقصى (" + Num(maxDp) + "%).");
        }

        return msg.Count == 0 ? null : string.Join("\n", msg);
    }

    /// <summary>VALIDATE_OFFER: the broken rules of the offer, one per line, or null.</summary>
    public async Task<string?> ValidateOfferAsync(long offerId, CancellationToken ct)
    {
        var o = await db.Set<MffFinanceOffer>().AsNoTracking().FirstOrDefaultAsync(x => x.OfferId == offerId, ct);
        var a = o is null ? null : await db.Set<MffApplication>().AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == o.ApplicationId, ct);
        if (o is null || a is null)
            return "عرض التمويل أو الطلب المرتبط غير موجود.";
        var maxLtv = await Param(a, "MAX_LTV_PCT", o.OfferDate, ct);
        var minTerm = await Param(a, "MIN_TERM_YEARS", o.OfferDate, ct);
        var maxTerm = await Param(a, "MAX_TERM_YEARS", o.OfferDate, ct);
        var maxPti = await Param(a, "MAX_PAYMENT_INCOME_PCT", o.OfferDate, ct);
        var minDp = await Param(a, "MIN_DOWNPAYMENT_PCT", o.OfferDate, ct);
        var maxDp = await Param(a, "MAX_DOWNPAYMENT_PCT", o.OfferDate, ct);

        var msg = new List<string>();
        if (maxLtv is not null && (o.LtvPct ?? 0) > maxLtv + 0.0001m)
            msg.Add("نسبة LTV في عرض التمويل تتجاوز الحد المسموح.");
        if (minTerm is not null && (o.TermMonths ?? 0) < minTerm * 12)
            msg.Add("مدة التمويل أقل من الحد الأدنى للبرنامج.");
        if (maxTerm is not null && (o.TermMonths ?? 0) > maxTerm * 12)
            msg.Add("مدة التمويل تتجاوز الحد الأقصى للبرنامج.");
        if ((a.MonthlyIncomeTotal ?? 0) > 0)
        {
            var pti = (o.InitialInstallment ?? 0) / a.MonthlyIncomeTotal!.Value * 100;
            if (maxPti is not null && pti > maxPti + 0.0001m)
                msg.Add("نسبة القسط إلى الدخل تتجاوز الحد المسموح.");
        }

        if ((a.PropertyPrice ?? 0) > 0)
        {
            var dpPct = (o.DownpaymentAmount ?? 0) / a.PropertyPrice!.Value * 100;
            if (minDp is not null && dpPct < minDp - 0.0001m)
                msg.Add("مقدم العميل في العرض أقل من الحد الأدنى.");
            if (maxDp is not null && dpPct > maxDp + 0.0001m)
                msg.Add("مقدم العميل في العرض يتجاوز الحد الأقصى.");
        }

        return msg.Count == 0 ? null : string.Join("\n", msg);
    }

    /// <summary>IS_APPLICATION_VALID</summary>
    public async Task<bool> IsApplicationValidAsync(long applicationId, CancellationToken ct) => await ValidateApplicationAsync(applicationId, ct) is null;

    /// <summary>IS_OFFER_VALID</summary>
    public async Task<bool> IsOfferValidAsync(long offerId, CancellationToken ct) => await ValidateOfferAsync(offerId, ct) is null;

    private Task<decimal?> Param(MffApplication a, string code, DateOnly asOf, CancellationToken ct) =>
        param.GetNumberAsync(a.ProgramVersionId, code, a.FinancierId, asOf, null, ct);

    // TO_CHAR(n): the number without trailing zeros
    private static string Num(decimal? n) => n?.ToString("0.############", CultureInfo.InvariantCulture) ?? string.Empty;
}
