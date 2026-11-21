using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Eod;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// End of Day job LND_DAILY: Retail Lending's day (<see cref="LndEodService"/>, unchanged): credit balances applied, interest accrued, late
/// fees, non-accrual and suspense, cure, write-off proposals and GL per product. Lending days run in order, so days the job was switched off
/// are run first (catch-up); a day already done is not run again.
/// </summary>
public sealed class LndDailyEodJob(LndEodService eod, ICapitraDbContext db) : IEodJob
{
    public string JobCode => "LND_DAILY";
    public string ModuleCode => "LND";
    public string NameEn => "Retail Lending: daily accrual and arrears";
    public string NameAr => "التمويل الاستهلاكي: الاستحقاق اليومي والمتأخرات";
    public string DescriptionEn => "Customer credit balances pay what fell due; interest accrues; late fees after the grace days; late finances move into or out of "
        + "non-accrual and suspense; finances late beyond the write-off days are proposed for write-off; GL posts each product's rule LND_EOD.";
    public string DescriptionAr => "تُسدَّد المستحقات من الأرصدة الدائنة للعملاء، ويُحتسب العائد اليومي، وتُحتسب غرامات التأخير بعد أيام السماح، وتنتقل التمويلات المتأخرة "
        + "إلى عدم الاستحقاق والعوائد المجنبة أو تخرج منها، وتُقترح التمويلات المتأخرة بعد أيام الإعدام للإعدام، ويُرحَّل القيد حسب قاعدة LND_EOD لكل منتج.";
    public IReadOnlyList<EodParameterDefinition> Parameters => [];

    public async Task<EodJobOutcome> RunAsync(EodJobContext context, CancellationToken cancellationToken)
    {
        var day = context.BusinessDate;
        var lastDone = await eod.LastDoneAsync(cancellationToken);
        if (lastDone is { } d && d >= day)
            return await OutcomeAsync(day, "Done already in Retail Lending.", cancellationToken);

        var from = lastDone?.AddDays(1) ?? day;
        for (var x = from; x <= day; x = x.AddDays(1))
            await eod.RunAsync(new LndEodRequestDto(x), cancellationToken);
        return await OutcomeAsync(day, from < day ? $"Lending days {from:yyyy-MM-dd} to {day.AddDays(-1):yyyy-MM-dd} were run first (catch-up)." : null, cancellationToken);
    }

    private async Task<EodJobOutcome> OutcomeAsync(DateOnly day, string? note, CancellationToken cancellationToken)
    {
        var run = await db.Set<LndEodRun>().AsNoTracking().FirstOrDefaultAsync(r => r.RunDate == day, cancellationToken);
        if (run is null) return new EodJobOutcome(0, note ?? "Nothing to do.");
        var c = CultureInfo.InvariantCulture;
        var text = string.Create(c, $"{run.LoansCount} finances: interest accrued {run.InterestAccruedAmount:#,0.00}, to suspense {run.InterestSuspendedAmount:#,0.00}, late fees {run.LateFeeAmount:#,0.00}, credit applied {run.CreditAppliedAmount:#,0.00}; non-accrual {run.NonAccrualCount}, cured {run.CuredCount}, proposed for write-off {run.WriteOffProposedCount}.")
            + (run.GlTranSetIds is null ? string.Empty : $" GL: {run.GlTranSetIds}.")
            + (note is null ? string.Empty : " " + note);
        return new EodJobOutcome(run.LoansCount, text);
    }
}
