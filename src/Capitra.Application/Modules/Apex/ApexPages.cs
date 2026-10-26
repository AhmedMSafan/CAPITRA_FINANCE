namespace Capitra.Application.Modules.Apex;

/// <summary>
/// Where each copied APEX page lives in the Capitra web app, so links that APEX built with
/// apex_page.get_url or f?p=APP:PAGE point at the same page here.
/// </summary>
public static class ApexPages
{
    private static readonly Dictionary<(int App, int Page), string> Routes = new()
    {
        [(120, 1)] = "/gl",
        [(120, 2)] = "/gl/chart-of-accounts",
        [(120, 3)] = "/gl/account-details",
        [(120, 20)] = "/gl/account-balances",
        [(120, 30)] = "/gl/accounting-periods",
        [(120, 40)] = "/gl/account-structure",
        [(120, 50)] = "/gl/journal-entry",
        [(120, 51)] = "/gl/journals",
        [(120, 52)] = "/gl/account-ledger",
        [(120, 56)] = "/gl/financial-statements",
        [(120, 90)] = "/gl/help",
        [(120, 600)] = "/gl/event-sources",
        [(120, 601)] = "/gl/event-source",
        [(120, 605)] = "/gl/accounting-events",
        [(120, 606)] = "/gl/accounting-event",
        [(120, 610)] = "/gl/source-values",
        [(120, 611)] = "/gl/source-value",
        [(120, 615)] = "/gl/rule-sets",
        [(120, 616)] = "/gl/rule-set",
        [(120, 617)] = "/gl/rule-lines",
        [(120, 620)] = "/gl/generated-entries",
        [(120, 621)] = "/gl/generated-entry",
        [(120, 622)] = "/gl/generated-entry-lines",
        [(120, 623)] = "/gl/generated-entry-line",
        [(120, 718)] = "/gl/rule-line",
        [(120, 730)] = "/gl/interest-servicing",
    };

    /// <summary>apex_page.get_url(p_page => page) for a page of the given app.</summary>
    public static string Url(int app, int page) => Routes.TryGetValue((app, page), out var route) ? route : $"/apex/{app}/{page}";
}
