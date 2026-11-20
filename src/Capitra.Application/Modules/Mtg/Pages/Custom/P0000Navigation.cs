using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 0 (Global Page), region "Navigation Badges": window.FINOVA_NAV, the counts finova-portfolio-bilingual.js
/// shows on the navigation menu (My Inbox, Open Exceptions) and the header (inbox bell). Each count is 0 when it fails.
/// </summary>
public sealed class P0000NavigationBadges(IApexSqlReader reader, WfeInbox inbox, ICurrentUser user) : IApexProcess
{
    public int App => 105;

    public int Page => 0;

    public string Name => "NAVIGATION_BADGES";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var inboxCount = 0;
        var exceptions = 0;
        try
        {
            inboxCount = (await inbox.MyInboxAsync(ct)).Count;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
        }

        try
        {
            var rows = await reader.QueryAsync(MtgApexPageSql.P0_EXCEPTIONS, new Dictionary<string, string?>(), ct);
            exceptions = (int)(rows.Count > 0 ? rows[0].Number("EXCEPTIONS") ?? 0 : 0);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
        }

        return new
        {
            ok = true,
            nav = new
            {
                inbox = inboxCount,
                exceptions,
                user = user.UserName,
                inboxUrl = ApexPages.Url(105, 920),
                homeUrl = ApexPages.Url(105, 1)
            }
        };
    }
}

/// <summary>
/// App 105 application process FNV_FIND_PORTFOLIO (On Demand): the header search. x01 is the text; the 8 best portfolios
/// whose number, developer or project holds it, each with a link to page 203.
/// </summary>
public sealed class P0000FindPortfolio(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 0;

    public string Name => "FNV_FIND_PORTFOLIO";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var q = (request.X01 ?? string.Empty).Trim();
        q = q[..Math.Min(100, q.Length)].Trim().ToUpperInvariant();
        var rows = new List<object>();
        if (q.Length >= 2)
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P0_FIND_PORTFOLIO, new Dictionary<string, string?> { ["Q"] = q }, ct))
            {
                rows.Add(new
                {
                    no = r.Text("PORTFOLIO_NO"),
                    developer = r.Text("DEVELOPER_NAME_EN") ?? "-",
                    project = r.Text("PROJECT_NAME_EN") ?? "-",
                    status = r.Text("STATUS_NAME_EN") ?? "-",
                    url = ApexPages.Url(105, 203, "P203_PORTFOLIO_ID", r.Number("PORTFOLIO_ID")?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)
                });
            }
        }

        return new { rows };
    }
}
