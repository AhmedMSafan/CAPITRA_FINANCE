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
    public static string Url(int app, int page) =>
        Routes.TryGetValue((app, page), out var route) ? route
        : app == 105 ? (page == 1 ? "/mortgage" : page is 920 or 604 ? "/workflow/inbox" : $"/mortgage/p{page}")
        : app == 110 ? (page == 1 ? "/mortgage-finance" : $"/mortgage-finance/p{page}")
        : app == 122 ? (page == 1 ? "/cheque-wallet" : $"/cheque-wallet/p{page}")
        : $"/apex/{app}/{page}";

    /// <summary>
    /// apex_page.get_url(p_page, p_request, p_items, p_values): the items (comma separated, as APEX takes them)
    /// travel in the query in lower case, the request as ?request=.
    /// </summary>
    public static string Url(int app, int page, string? items, string? values, string? request = null)
    {
        // page 921 (Task Details of the APEX inbox) is the Capitra task page; P921_TASK_ID carries the inbox item's id
        if (app == 105 && page == 921 && string.Equals(items, "P921_TASK_ID", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(values))
            return "/workflow/tasks/" + Uri.EscapeDataString(values);
        var query = new List<string>();
        if (!string.IsNullOrEmpty(request))
            query.Add("request=" + Uri.EscapeDataString(request));
        var names = (items ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var vals = (values ?? string.Empty).Split(',');
        for (var i = 0; i < names.Length; i++)
        {
            var v = i < vals.Length ? vals[i] : string.Empty;
            if (v.Length > 0)
                query.Add(names[i].ToLowerInvariant() + "=" + Uri.EscapeDataString(v));
        }

        return Url(app, page) + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
    }

    /// <summary>apex_page.get_url(p_page => page, p_items => item, p_values => value): the item travels in the query, e.g. ?p50_id=12.</summary>
    public static string Url(int app, int page, string item, string value) =>
        Url(app, page) + "?" + item.ToLowerInvariant() + "=" + Uri.EscapeDataString(value);
}
