using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Chq.Pages.Custom;

/// <summary>App 122 page 1 "Home": the PL/SQL region, ported.</summary>
public sealed class P0001Home(IApexSqlReader reader, ICurrentUser currentUser) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#E87722", "#5B5BD6", "#14734F", "#0EA5E9", "#B4570E", "#7C3AED", "#64748B"];

    public int App => ChqAjax.App;

    public int Page => 1;

    public string Name => "REGION_HOME";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        // the cnt() function returned null when its query failed
        decimal? chq = null, amount = null, due = null, dueAmount = null, month = null, all = null, sub = null, err = null, ok = null, open = null;
        try
        {
            if (await reader.RowAsync(ChqApexPageSql.CHQ_HOME_COUNTS, ChqAjax.NoItems, ct) is { } c)
            {
                chq = c.Number("CHQ");
                amount = c.Number("AMT");
                due = c.Number("DUE");
                dueAmount = c.Number("DUE_AMT");
                month = c.Number("MONTH_CNT");
                all = c.Number("B_ALL");
                sub = c.Number("B_SUB");
                err = c.Number("B_ERR");
                ok = c.Number("B_OK");
                open = c.Number("B_OPEN");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // each count stays null
        }

        var user = InitCap((currentUser.UserName ?? string.Empty).ToLowerInvariant().Replace('_', ' '));
        var h = new StringBuilder();
        h.AppendLine($$"""
        <div class="wx wh">
        """);
        h.AppendLine($$"""
        <section class="wx-hero wh-hero"><div>
        """);
        h.AppendLine($$"""
        <span class="wx-eyebrow"><i></i>FINOVA Cheques Wallet</span>
        """);
        h.AppendLine($$"""
        <h1><span id="whGreet">Welcome back</span>, <em>{{E(user)}}</em></h1>
        """);
        h.AppendLine($$"""
        <p>Load client cheque batches from Excel, keep every cheque in the client wallet and follow it through operations, treasury and the bank until it is collected or rejected.</p>
        """);
        h.AppendLine($$"""
        <div class="wh-cta"><a class="wx-btn wx-btn--hot" href="{{Url(122)}}"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><path d="M17 8l-5-5-5 5M12 3v12"/></svg><span>Upload cheques</span></a><a class="wx-btn wx-btn--ghost" href="{{Url(130)}}"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg><span>Cheques wallet</span></a><a class="wx-btn wx-btn--ghost" href="{{Url(12)}}"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="17" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg><span>Due cheques</span></a></div>
        """);
        h.AppendLine($$"""
        </div><div class="wh-kpis" role="list">
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(chq)}}</b><span>Cheques in the wallet &middot; <small>{{Fmt(month)}} this month</small></span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Amt(amount)}}</b><span>Total amount of the cheques</span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(due)}}</b><span>Due in the next 30 days &middot; <small>{{Amt(dueAmount)}}</small></span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(open)}}</b><span>Batches not submitted yet &middot; <small>{{Fmt(all)}} batches</small></span></div>
        """);
        h.AppendLine($$"""
        </div></section>
        """);
        h.AppendLine($$"""
        <div class="wx-sec"><h2>Cheque batches</h2></div><div class="wh-grid wh-grid--2">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card wh-feat" href="{{Url(122)}}" style="--c:#E87722;--cl:#FFF1E6"><div class="wh-body">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><path d="M17 8l-5-5-5 5M12 3v12"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Upload cheques</h3><p>Drop the client&rsquo;s Excel file. Every line is checked first &ndash; empty cells, banks, one customer, customer in CRM, duplicates &ndash; and the batch is created only when all checks pass.</p></div>
        """);
        h.AppendLine($$"""
        <div class="ch-st"><span>1 Choose the file</span><span>2 Automatic checks</span><span>3 Create the batch</span><span>4 Submit</span></div></div>
        """);
        h.AppendLine($$"""
        <div class="wh-mini ch-mini"><svg viewBox="0 0 300 140" aria-hidden="true"><rect x="14" y="20" width="96" height="100" rx="9" fill="#fff" stroke="#B9CBD1"/><rect x="14" y="20" width="96" height="20" rx="9" fill="#1D6F42"/><rect x="14" y="32" width="96" height="8" fill="#1D6F42"/><text x="25" y="35" fill="#fff" font-size="10" font-weight="800" font-family="Inter,Arial">XLSX</text><rect x="22" y="48" width="80" height="9" rx="2" fill="#E6F3F4"/><rect x="22" y="62" width="80" height="9" rx="2" fill="#E6F3F4"/><rect x="22" y="76" width="80" height="9" rx="2" fill="#E6F3F4"/><rect x="22" y="90" width="80" height="9" rx="2" fill="#E6F3F4"/><rect x="22" y="104" width="80" height="9" rx="2" fill="#E6F3F4"/><path d="M118 70 H178" stroke="#9FBFC5" stroke-width="1.6" stroke-dasharray="4 4"/><g transform="translate(140 52) rotate(-6)"><rect x="-15" y="-9" width="30" height="18" rx="3" fill="#FFF4E2" stroke="#E87722" stroke-width="1.4"/><path d="M-10 -3h11M-10 3h7M5 3h6" stroke="#B86A2A" stroke-width="1.3" stroke-linecap="round"/></g><g transform="translate(160 86) rotate(5)"><rect x="-15" y="-9" width="30" height="18" rx="3" fill="#FFF4E2" stroke="#E87722" stroke-width="1.4"/><path d="M-10 -3h11M-10 3h7M5 3h6" stroke="#B86A2A" stroke-width="1.3" stroke-linecap="round"/></g><circle cx="186" cy="34" r="11" fill="#14734F"/><path d="M181 34l3.4 3.4 6-6.6" stroke="#fff" stroke-width="2.2" fill="none" stroke-linecap="round" stroke-linejoin="round"/><rect x="196" y="46" width="88" height="62" rx="10" fill="#0B3A41" stroke="#00636D" stroke-width="2"/><rect x="196" y="40" width="74" height="16" rx="6" fill="#0E4B54" stroke="#00636D" stroke-width="2"/><rect x="252" y="66" width="34" height="22" rx="6" fill="#0E4B54" stroke="#00636D" stroke-width="2"/><circle cx="266" cy="77" r="4" fill="#FFB25E"/><text x="240" y="128" text-anchor="middle" fill="#5F7B8A" font-size="10.5" font-family="Inter,Arial">Client wallet</text></svg></div></a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(120)}}" style="--c:#00636D;--cl:#E6F3F4">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Cheque batches</h3><p>Every uploaded batch: open it to review the lines and submit the cheques to the wallet.</p></div>
        """);
        h.AppendLine($$"""
        <div class="ch-st"><span class="ok"><b>{{Fmt(ok)}}</b> ready to submit</span><span class="bad"><b>{{Fmt(err)}}</b> with errors</span><span><b>{{Fmt(sub)}}</b> submitted</span></div>
        """);
        h.AppendLine($$"""
        </a></div>
        """);
        h.AppendLine($$"""
        <div class="wx-sec"><h2>Wallet &amp; movements</h2></div><div class="wh-grid">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(130)}}" style="--c:#014B55;--cl:#E2EFF1">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Cheques wallet</h3><p>All cheques with their status, custody and location.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(chq)}}</b><span>cheques</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(140)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Cheque movements</h3><p>Hand-overs between client, operations, treasury and bank.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Custody trail</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(12)}}" style="--c:#14734F;--cl:#E7F6EF">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="17" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Due cheques</h3><p>Cheques by due date, to plan deposits and collections.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(due)}}</b><span>due in 30 days</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(150)}}" style="--c:#5B5BD6;--cl:#EEEEFC">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6M16 13H8M16 17H8M10 9H8"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Cheque actions catalog</h3><p>The actions and movements a cheque can go through.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Actions</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        </div>
        """);
        try
        {
            var i = 0;
            foreach (var r in await reader.QueryAsync(ChqApexPageSql.CHQ_HOME_LOCATIONS, ChqAjax.NoItems, ct))
            {
                if (i == 0)
                    h.AppendLine($$"""
                    <div class="wx-sec"><h2>Where the cheques are</h2></div><div class="ch-loc">
                    """);
                i++;
                var n = r.Number("N") ?? 0;
                h.AppendLine($$"""
                <div class="ch-l{{(n == 0 ? " zero" : "")}}" style="--c:{{Colors[(i - 1) % 8]}}"><small>{{E(r.Text("LOCATION_NAME"))}}</small><b>{{Fmt(n)}}</b><span>{{(n > 0 ? Amt(r.Number("A")) : "no cheques")}}</span></div>
                """);
            }
            if (i > 0)
                h.AppendLine($$"""
                </div>
                """);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        try
        {
            var rows = await reader.QueryAsync(ChqApexPageSql.CHQ_HOME_STATUSES, ChqAjax.NoItems, ct);
            var i = 0;
            foreach (var r in rows)
            {
                if (i == 0)
                    h.AppendLine($$"""
                    <div class="wx-sec"><h2>Cheques by status</h2></div><div class="ch-box"><div class="ch-bar">
                    """);
                i++;
                var n = r.Number("N") ?? 0;
                h.AppendLine($$"""
                <i style="width:{{Pct(n, chq)}}%;background:{{Colors[(i - 1) % 8]}}" title="{{E(r.Text("NAME_EN") ?? r.Text("STATUS_CODE"))}}: {{Num(n)}}"></i>
                """);
            }
            if (i > 0)
            {
                h.AppendLine($$"""
                </div><div class="ch-pills">
                """);
                i = 0;
                foreach (var r in rows)
                {
                    i++;
                    var n = r.Number("N") ?? 0;
                    h.AppendLine($$"""
                    <span class="wx-tag" style="--c:{{Colors[(i - 1) % 8]}}"><i></i>{{E(r.Text("NAME_EN") ?? r.Text("STATUS_CODE"))}} <b>{{Fmt(n)}}</b></span>
                    """);
                }
                h.AppendLine($$"""
                </div></div>
                """);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        h.AppendLine($$"""
        <div class="wx-sec"><h2>Calculators</h2></div><div class="wh-grid">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(2)}}" style="--c:#00636D;--cl:#E6F3F4">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="4" y="2" width="16" height="20" rx="2"/><path d="M8 6h8M8 11h.01M12 11h.01M16 11h.01M8 15h.01M12 15h.01M16 15h.01M8 19h8"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Factoring calculator</h3><p>Advance, discount and fees of a factoring deal.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Pricing</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(3)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 3v18h18"/><path d="M7 15l4-4 3 3 5-6"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Universal amortization</h3><p>Repayment schedules for any product.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Schedules</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{Url(333)}}" style="--c:#5B5BD6;--cl:#EEEEFC">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2"/><path d="M3 10h18M9 10v10"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Universal calculator</h3><p>Quick financial calculations.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Tools</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        <script>(function(){var h=new Date().getHours(),g=h<12?'Good morning':(h<18?'Good afternoon':'Good evening'),el=document.getElementById('whGreet');if(el)el.textContent=g;})();</script>
        """);
        return new { ok = true, html = h.ToString() };
    }

    private static string Url(int page) => ChqAjax.Url(page);

    private static string Fmt(decimal? n) => Show(n);

    private static string Amt(decimal? n) => n is null ? "&ndash;" : ToChar(n, "FM999G999G999G990D00");

    /// <summary>round(n / nullif(total, 0) * 100, 2) as Oracle writes it (".5", "33.33"); empty when there is no total.</summary>
    private static string Pct(decimal n, decimal? total) => total is null or 0 ? string.Empty : Num(Math.Round(n / total.Value * 100, 2, MidpointRounding.AwayFromZero));

    /// <summary>A number as Oracle converts it to text: no trailing zeros, no leading zero before the point.</summary>
    private static string Num(decimal n)
    {
        var s = ChqAjax.Normal(n).ToString(CultureInfo.InvariantCulture);
        return s.StartsWith("0.", StringComparison.Ordinal) ? s[1..] : s.StartsWith("-0.", StringComparison.Ordinal) ? "-" + s[2..] : s;
    }

    /// <summary>initcap: the first letter of each word upper case.</summary>
    private static string InitCap(string s)
    {
        var b = new StringBuilder(s.Length);
        var start = true;
        foreach (var ch in s)
        {
            b.Append(start ? char.ToUpperInvariant(ch) : ch);
            start = !char.IsLetterOrDigit(ch);
        }
        return b.ToString();
    }
}
