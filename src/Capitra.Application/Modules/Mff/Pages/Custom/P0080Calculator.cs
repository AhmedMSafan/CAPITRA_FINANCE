using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 80, After Submit "Run Mortgage Calculation" (CALCULATE): MFF_CALC_PKG (new in C#).</summary>
public sealed class P0080Calculator(ICapitraDbContext db, MffCalcPackage calc) : IApexProcess
{
    public int App => 110;

    public int Page => 80;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "CALCULATE")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var run = await calc.RunApplicationAsync(Long(request, "P80_APPLICATION_ID") ?? 0,
                Number(request, "P80_RATE"), Number(request, "P80_TERM_YEARS"), Number(request, "P80_GROWTH_PCT"), Number(request, "P80_GROWTH_YEARS"),
                Number(request, "P80_MAX_PTI"), Number(request, "P80_DP_ASSIST"), Number(request, "P80_MONTHLY_SUBSIDY"), Int(request, "P80_SUBSIDY_MONTHS"),
                Number(request, "P80_DISCOUNT_RATE"), "APEX App 110 Page 80", ct);
            await calc.BuildScheduleAsync(run, ct);
            var r = await db.Set<MffCalculationRun>().AsNoTracking().FirstAsync(x => x.CalcRunId == run, ct);
            return (string.Empty, new Dictionary<string, string?>
            {
                ["P80_CALC_RUN_ID"] = Id(run),
                ["P80_OUT_LOAN"] = ApexHtml.ToChar(r.OutMortgageLoan, "FM999G999G999G990D00"),
                ["P80_OUT_PAY"] = ApexHtml.ToChar(r.OutInitialPayment, "FM999G999G990D00"),
                ["P80_OUT_SUBSIDY"] = ApexHtml.ToChar(r.OutTotalSubsidy, "FM999G999G999G990D00"),
                ["P80_OUT_LTV"] = ApexHtml.ToChar(r.OutLtvPct, "FM990D00"),
                ["P80_OUT_DP"] = ApexHtml.ToChar(r.OutBorrowerDownpayment, "FM999G999G999G990D00"),
                ["P80_OUT_PROP"] = ApexHtml.ToChar(r.OutPropertyValue, "FM999G999G999G990D00"),
                ["P80_OUT_MAXPROP"] = ApexHtml.ToChar(r.OutMaxPropertyValue, "FM999G999G999G990D00"),
                ["P80_NEG_AM"] = r.IsNegativeAmortization ? "Y" : "N",
            });
        }, ct);
    }
}
