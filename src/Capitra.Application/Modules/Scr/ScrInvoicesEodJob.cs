using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Eod;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// End of Day job SCR_INVOICES: on the last day of each company's invoice period (month, quarter or year, as set on the company) the
/// period's invoice is issued, as the Issue button does. A period already invoiced is left alone, so the job can run again.
/// </summary>
public sealed class ScrInvoicesEodJob(ICapitraDbContext db, ScrBillingService billing) : IEodJob
{
    public const string IssueZero = "ISSUE_ZERO_INVOICES";

    public string JobCode => "SCR_INVOICES";
    public string ModuleCode => "SCR";
    public string NameEn => "Screening: period-end invoices";
    public string NameAr => "الاستعلام: فواتير نهاية الفترة";
    public string DescriptionEn => "On the last day of each member company's invoice period (monthly, quarterly or yearly), issues the period's invoice from its "
        + "price plan: user fees and inquiry fees. Companies without a price plan, and periods already invoiced, are left alone.";
    public string DescriptionAr => "في آخر يوم من فترة فوترة كل شركة عضو (شهرية أو ربع سنوية أو سنوية) تُصدر فاتورة الفترة حسب خطة الأسعار: رسوم المستخدمين "
        + "ورسوم الاستعلامات. لا تُفوتر الشركات بلا خطة أسعار ولا الفترات المفوترة من قبل.";

    public IReadOnlyList<EodParameterDefinition> Parameters =>
    [
        new(IssueZero, "Issue invoices of zero", "إصدار الفواتير الصفرية", "Y: a period with nothing to charge still gets an invoice of 0.00. N: it is skipped.",
            "Y: تصدر فاتورة بقيمة 0.00 للفترة التي لا رسوم فيها. N: لا تصدر.", "N"),
    ];

    public async Task<EodJobOutcome> RunAsync(EodJobContext context, CancellationToken cancellationToken)
    {
        var day = context.BusinessDate;
        var zero = context.Text(IssueZero).Trim().ToUpperInvariant() is "Y" or "YES";
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking()
            .Where(c => c.StatusCode == "ACTIVE" && c.PricePlanId != null).OrderBy(c => c.CompanyCode).ToListAsync(cancellationToken);
        int issued = 0, already = 0, empty = 0;
        var issuedNos = new List<string>();
        var failed = new List<string>();
        foreach (var c in companies)
        {
            var (from, to) = ScrBilling.PeriodOf(c.BillingCycleCode, day);
            if (to != day) continue;
            if (await db.Set<ScrInvoice>().AsNoTracking().AnyAsync(i => i.MemberCompanyId == c.MemberCompanyId && i.StatusCode == "ISSUED"
                    && i.PeriodFrom <= to && i.PeriodTo >= from, cancellationToken))
            {
                already++;
                continue;
            }
            try
            {
                var preview = await billing.PreviewAsync(c.MemberCompanyId, from, cancellationToken);
                if (preview.TotalAmount == 0 && !zero)
                {
                    empty++;
                    continue;
                }
                var invoice = await billing.IssueAsync(c.MemberCompanyId, new ScrInvoiceRequest(from, $"Issued by end of day {day:yyyy-MM-dd}."), cancellationToken);
                issued++;
                issuedNos.Add($"{c.CompanyCode} {invoice.InvoiceNo}");
            }
            catch (Exception ex) when (ex is BusinessRuleException or BusinessValidationException or ConflictException or NotFoundException)
            {
                db.DiscardChanges();
                failed.Add($"{c.CompanyCode}: {ex.Message}");
            }
        }

        var summary = $"{issued} invoice(s) issued" + (issuedNos.Count > 0 ? $" ({string.Join(", ", issuedNos)})" : string.Empty)
            + $"; {already} already invoiced; {empty} with nothing to charge.";
        if (failed.Count > 0)
            throw new BusinessRuleException($"{summary} Not issued: {string.Join(" | ", failed)}");
        return new EodJobOutcome(issued, summary);
    }
}
