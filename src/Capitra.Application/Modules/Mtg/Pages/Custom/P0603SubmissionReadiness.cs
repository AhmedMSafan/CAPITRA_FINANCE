using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 603 "Submission &amp; Readiness", Before Header "Load Readiness".</summary>
public sealed class P0603LoadReadiness(IApexSqlReader reader, ICapitraDbContext db, MtgPurchasedPortfolioPackage portfolios) : IApexProcess
{
    public int App => 105;

    public int Page => 603;

    public string Name => "PAGE_LOAD";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = ApexItems.Read(request.X01);
        var set = new Dictionary<string, string?>();
        var row = await reader.RowAsync(MtgApexPageSql.P603_LOAD, items, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        foreach (var c in row)
            set[c.Key] = ApexSqlLovValues.Text(c.Value);
        var id = long.Parse(items["P603_PORTFOLIO_ID"]!, CultureInfo.InvariantCulture);
        set["P603_READINESS_MESSAGE"] = await portfolios.ValidatePortfolioAsync(id, ct);
        set["P603_MEMO_COUNT"] = (await db.Set<MtgPurchasedPortfolioMemo>().CountAsync(m => m.PortfolioId == id, ct)).ToString(CultureInfo.InvariantCulture);
        return new { ok = true, items = set };
    }
}

/// <summary>App 105 page 603, After Submit "Submit Portfolio" (SUBMIT_FOR_APPROVAL).</summary>
public sealed class P0603SubmitPortfolio(ICapitraDbContext db, MtgPurchasedPortfolioPackage portfolios, ICurrentUser user) : IApexProcess
{
    public int App => 105;

    public int Page => 603;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SUBMIT_FOR_APPROVAL")
            return new { ok = true };
        var id = long.Parse(request.Item("P603_PORTFOLIO_ID") ?? throw new ApexProcessException("ORA-01403: no data found"), CultureInfo.InvariantCulture);
        try
        {
            if (!await db.Set<MtgPurchasedPortfolioMemo>().AnyAsync(m => m.PortfolioId == id, ct))
                throw new ApexProcessException("Comprehensive memo is required before submission");
            var status = await db.Set<MtgPurchasedPortfolio>().Where(p => p.PortfolioId == id).Select(p => p.PortfolioStatusCode).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            if (status is "PENDING_APPROVAL" or "APPROVED" or "REJECTED" or "CLOSED")
                throw new ApexProcessException("Portfolio cannot be submitted in its current status");
            await portfolios.SubmitPortfolioAsync(id, user.UserName ?? string.Empty, ct);
            return new { ok = true, message = "Portfolio submitted successfully" };
        }
        catch (ApexProcessException e)
        {
            return new { ok = false, message = e.Message };
        }
    }
}
