using System.Globalization;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 405 "Financial Study", After Submit: RECALCULATE runs FNV_FINANCE_PKG.recalc_financial_study.</summary>
public sealed class P0405FinancialStudy(MtgFinancePackage finance) : IApexProcess
{
    public int App => 105;

    public int Page => 405;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") == "RECALCULATE" && request.Item("P405_PORTFOLIO_ID") is { } id)
            await finance.RecalcFinancialStudyAsync(long.Parse(id, CultureInfo.InvariantCulture), ct);
        return new { ok = true };
    }
}
