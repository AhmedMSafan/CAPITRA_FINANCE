using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 7 "Developer Workspace": the PL/SQL region, ported.</summary>
public sealed class P0007DeveloperWorkspace(IApexSqlReader reader) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];
    private static readonly Dictionary<string, string?> NoItems = [];

    public int App => 105;

    public int Page => 7;

    public string Name => "REGION_DEVELOPER_WORKSPACE";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        // pipeline figures per developer, in the order the view returns them (the PL/SQL maps are keyed by to_char(DEVELOPER_ID))
        var pipe = new Dictionary<string, Figures>(StringComparer.Ordinal);
        try
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P7_PIPE, NoItems, ct))
                if (r.Number("DEVELOPER_ID") is { } id)
                    pipe[Num(id)] = new Figures(r.Number("OPEN_CNT"), r.Number("FACE"), r.Number("READY"), r.Number("PEND"), r.Number("APPR"), r.Date("LAST_REQUEST"));
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            pipe.Clear();
        }

        decimal devs = 0, projects = 0, pf = 0;
        try
        {
            if (await reader.RowAsync(MtgApexPageSql.P7_COUNTS, NoItems, ct) is { } c)
            {
                devs = c.Number("DEVS") ?? 0;
                projects = c.Number("PROJECTS") ?? 0;
                pf = c.Number("PF") ?? 0;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        decimal open = 0, face = 0, ready = 0, pend = 0, appr = 0, withOpen = 0;
        foreach (var f in pipe.Values)
        {
            open += f.Open ?? 0;
            face += f.Face ?? 0;
            ready += f.Ready ?? 0;
            pend += f.Pend ?? 0;
            appr += f.Appr ?? 0;
            if ((f.Open ?? 0) > 0)
                withOpen++;
        }

        var idle = Math.Max(devs - withOpen, 0);
        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"fxdDev\">");
        h.AppendLine("<header class=\"fxd-hero fnv-list-hero\"><div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Work", "العمل") + "</div>"
            + "<h1 class=\"fxd-h1\">" + T("Developer Workspace", "مساحة عمل المطورين") + "</h1><p class=\"fxd-lead\">"
            + T("Every real-estate developer you buy portfolios from, with their projects, live pipeline and what is waiting on you. Pick one to open its workspace.",
                "جميع المطورين العقاريين الذين تشتري منهم المحافظ مع مشروعاتهم والمحافظ الجارية وما ينتظر إجراءك. اختر مطوراً لفتح مساحة عمله.") + "</p></div>"
            + "<div class=\"fxd-actions fxd-anim d1\"><a class=\"fxd-btn fxd-btn--glass\" href=\"" + Url(2) + "\"><span class=\"fa fa-dashboard\" aria-hidden=\"true\"></span>" + T("Portfolio dashboard", "لوحة المحافظ") + "</a></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap\"><div class=\"dw-kpis\">");
        h.AppendLine("<div class=\"fxd-card dw-kpi fxd-anim d1\"><span class=\"ico fa fa-building-o\" aria-hidden=\"true\"></span><span class=\"fxd-label\">" + T("Developers", "المطورون") + "</span><div class=\"v\">" + ToChar(devs, "FM999G990") + "</div>"
            + "<div class=\"s\">" + ToChar(projects, "FM999G990") + " " + T("projects", "مشروع") + " &middot; " + ToChar(pf, "FM999G990") + " " + T("portfolios", "محفظة") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card dw-kpi fxd-anim d2\"><span class=\"ico fa fa-folder-open-o\" aria-hidden=\"true\"></span><span class=\"fxd-label\">" + T("Open portfolios", "المحافظ الجارية") + "</span><div class=\"v\">" + ToChar(open, "FM999G990") + "</div>"
            + "<div class=\"s\">" + T("with", "لدى") + " " + Num(withOpen) + " " + T("developers", "مطور") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card dw-kpi fxd-anim d3\"><span class=\"ico fa fa-money\" aria-hidden=\"true\"></span><span class=\"fxd-label\">" + T("Pipeline face value", "القيمة الاسمية الجارية") + "</span><div class=\"v\">" + Money(face) + "</div>"
            + "<div class=\"s\">" + T("across open portfolios", "عبر المحافظ الجارية") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card dw-kpi fxd-anim d4\"><span class=\"ico fa fa-hourglass-half\" aria-hidden=\"true\"></span><span class=\"fxd-label\">" + T("Waiting on decisions", "بانتظار القرار") + "</span><div class=\"v\"" + (pend > 0 ? " style=\"color:var(--amber-700,#B4570E)\"" : string.Empty) + ">" + Num(pend) + "</div>"
            + "<div class=\"s\">" + Num(ready) + " " + T("more ready to submit", "جاهزة للإرسال") + "</div></div>");
        h.AppendLine("</div>");

        h.AppendLine("<div class=\"dw-grid\"><section><div class=\"dw-tools\">"
            + "<label class=\"dw-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"dwQ\" aria-label=\"Search developers\" placeholder=\"Search developer name or code\"></label>"
            + "<div class=\"dw-seg\" role=\"group\" aria-label=\"Filter developers\">"
            + "<button type=\"button\" data-f=\"all\" aria-pressed=\"true\">" + T("All", "الكل") + "<em>" + Num(devs) + "</em></button>"
            + "<button type=\"button\" data-f=\"open\" aria-pressed=\"false\">" + T("Active pipeline", "لديهم محافظ جارية") + "<em>" + Num(withOpen) + "</em></button>"
            + "<button type=\"button\" data-f=\"pend\" aria-pressed=\"false\">" + T("Awaiting decision", "بانتظار القرار") + "</button>"
            + "<button type=\"button\" data-f=\"idle\" aria-pressed=\"false\">" + T("No open work", "بدون أعمال جارية") + "<em>" + Num(idle) + "</em></button></div>"
            + "<select class=\"dw-sort\" id=\"dwSort\" aria-label=\"Sort\"><option value=\"face\">Sort: pipeline value</option><option value=\"open\">Sort: open portfolios</option><option value=\"name\">Sort: name</option><option value=\"recent\">Sort: latest request</option></select></div>");
        h.AppendLine("<div class=\"dw-cards\" id=\"dwCards\">");
        var rank = 0;
        var names = new Dictionary<string, string?>(StringComparer.Ordinal);
        try
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P7_DEVELOPERS, NoItems, ct))
            {
                rank++;
                var k = Num(r.Number("DEVELOPER_ID"));
                string? en = r.Text("LEGAL_NAME_EN"), ar = r.Text("LEGAL_NAME_AR"), code = r.Text("DEVELOPER_CODE");
                names[k] = en ?? ar;
                pipe.TryGetValue(k, out var f);
                decimal o = f?.Open ?? 0, fv = f?.Face ?? 0, rd = f?.Ready ?? 0, pd = f?.Pend ?? 0;
                var other = Math.Max(o - rd - pd, 0);
                var last = f?.Last;
                h.AppendLine("<a class=\"dw-card fxd-anim\" href=\"" + Url(201, "P201_DEVELOPER_ID", k) + "\""
                    + " data-f=\"" + (o > 0 ? "open" : "idle") + (pd > 0 ? " pend" : string.Empty) + "\""
                    + " data-face=\"" + Num(fv) + "\" data-open=\"" + Num(o) + "\""
                    + " data-recent=\"" + (f is not null ? last?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? string.Empty : "0") + "\""
                    + " data-name=\"" + Attr((en ?? ar)?.ToLowerInvariant()) + "\""
                    + " data-q=\"" + Attr((code + " " + en + " " + ar).ToLowerInvariant()) + "\">"
                    + (pd > 0 ? "<span class=\"dw-flag fxd-pill fxd-pill--warn\">" + Num(pd) + " " + T("awaiting", "بانتظار") + "</span>" : string.Empty)
                    + "<div class=\"dw-top\"><span class=\"dw-av\" style=\"background:" + Colors[(rank - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span>"
                    + "<div class=\"dw-name\"" + (pd > 0 ? " style=\"padding-inline-end:90px\"" : string.Empty) + "><strong>" + E(en ?? ar) + "</strong>"
                    + (ar is not null && en is not null ? "<span class=\"ar\">" + E(ar) + "</span>" : string.Empty)
                    + "<span class=\"dw-code\">" + E(code) + "</span></div></div>"
                    + "<div class=\"dw-stats\"><div class=\"dw-stat\"><b>" + Num(r.Number("PROJECT_COUNT")) + "</b><span>" + T("Projects", "المشروعات") + "</span></div>"
                    + "<div class=\"dw-stat\"><b>" + Num(r.Number("PORTFOLIO_COUNT")) + "</b><span>" + T("Portfolios", "المحافظ") + "</span></div>"
                    + "<div class=\"dw-stat\"><b>" + Num(o) + "</b><span>" + T("Open", "جارية") + "</span></div></div>"
                    + "<div class=\"dw-pipe\"><div class=\"dw-pipe-h\"><span>" + T("Pipeline value", "القيمة الجارية") + "</span><b>" + (o > 0 ? Money(fv) : "&ndash;") + "</b></div>"
                    + "<div class=\"dw-bar\" role=\"img\" aria-label=\"" + Num(rd) + " ready, " + Num(pd) + " awaiting decision, " + Num(other) + " in progress\">"
                    + (o > 0 ? "<i style=\"width:" + Pct(other, o) + "%;background:#2E9AA3\"></i><i style=\"width:" + Pct(rd, o) + "%;background:#0B8A57\"></i><i style=\"width:" + Pct(pd, o) + "%;background:#E87722\"></i>" : string.Empty) + "</div>"
                    + "<div class=\"dw-legend\">" + (o == 0 ? "<span>" + T("No open portfolios", "لا توجد محافظ جارية") + "</span>"
                        : (other > 0 ? "<span><i style=\"background:#2E9AA3\"></i>" + Num(other) + " " + T("in progress", "قيد العمل") + "</span>" : string.Empty)
                        + (rd > 0 ? "<span><i style=\"background:#0B8A57\"></i>" + Num(rd) + " " + T("ready", "جاهزة") + "</span>" : string.Empty)
                        + (pd > 0 ? "<span><i style=\"background:#E87722\"></i>" + Num(pd) + " " + T("awaiting", "بانتظار") + "</span>" : string.Empty)) + "</div></div>"
                    + "<div class=\"dw-foot\"><span>" + (last is { } d ? T("Last request", "آخر طلب") + " " + d.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : T("No requests yet", "لا توجد طلبات بعد")) + "</span>"
                    + "<span class=\"dw-go\">" + T("Open workspace", "فتح المساحة") + "<span class=\"fa fa-arrow-right\" aria-hidden=\"true\"></span></span></div></a>");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        h.AppendLine("</div><div class=\"dw-empty\" id=\"dwEmpty\" hidden>" + T("No developer matches these filters.", "لا يوجد مطور مطابق.") + "</div>"
            + "<div class=\"dw-more\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" id=\"dwMore\" hidden>" + T("Show more developers", "عرض المزيد") + "</button></div></section>");

        // side: pipeline leaders and status mix
        h.AppendLine("<aside style=\"display:grid;gap:18px\"><section class=\"fxd-card fxd-anim d2\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-trophy\" aria-hidden=\"true\"></span>" + T("Largest pipelines", "أكبر المحافظ الجارية") + "</h2><span class=\"fxd-label\">" + T("share", "الحصة") + "</span></div><div class=\"fxd-card-b\" style=\"padding-top:6px\">");
        rank = 0;

        // the top 5 open pipelines; the PL/SQL walks its map in key order (varchar2 keys), the first of equal values wins
        var used = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 1; i <= 5; i++)
        {
            string? best = null;
            decimal bv = 0;
            foreach (var (k, f) in pipe.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                if (!used.Contains(k) && (f.Open ?? 0) > 0 && (f.Face ?? 0) > bv)
                {
                    best = k;
                    bv = f.Face!.Value;
                }
            }

            if (best is null)
                break;
            used.Add(best);
            rank++;
            var nm = names.TryGetValue(best, out var n) ? n : best;
            h.AppendLine("<a class=\"dw-lead\" href=\"" + Url(201, "P201_DEVELOPER_ID", best) + "\"><span class=\"n\">" + rank + "</span><div><strong>" + E(nm) + "</strong><div class=\"bar\"><i style=\"width:"
                + (face > 0 ? Num(Math.Min(100, P0203Html.Round(bv / face * 100))) : "0") + "%\"></i></div></div><b>" + Money(bv) + "</b></a>");
        }

        if (rank == 0)
            h.AppendLine("<div class=\"dw-empty\" style=\"border:0;padding:18px 0\">" + T("No open pipeline yet.", "لا توجد محافظ جارية بعد.") + "</div>");
        h.AppendLine("</div></section>");
        h.AppendLine("<section class=\"fxd-card fxd-anim d3\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-tasks\" aria-hidden=\"true\"></span>" + T("Where the work is", "حالة الأعمال") + "</h2></div><div class=\"fxd-card-b dw-mix\">"
            + "<div><span><i style=\"background:#2E9AA3\"></i>" + T("In progress", "قيد العمل") + "</span><b>" + Num(Math.Max(open - ready - pend, 0)) + "</b></div>"
            + "<div><span><i style=\"background:#0B8A57\"></i>" + T("Ready to submit", "جاهزة للإرسال") + "</span><b>" + Num(ready) + "</b></div>"
            + "<div><span><i style=\"background:#E87722\"></i>" + T("Awaiting decision", "بانتظار القرار") + "</span><b>" + Num(pend) + "</b></div>"
            + "<div><span><i style=\"background:#014B55\"></i>" + T("Approved", "معتمدة") + "</span><b>" + Num(appr) + "</b></div>"
            + "</div></section></aside></div></div></div>");
        h.AppendLine("<script>" + Script + "</script>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Num(decimal? n) => P0203Html.Num(n);

    // round(x/o*100,1)
    private static string Pct(decimal x, decimal o) => Num(P0203Html.Round(x / o * 100, 1));

    private sealed record Figures(decimal? Open, decimal? Face, decimal? Ready, decimal? Pend, decimal? Appr, DateOnly? Last);

    private const string Script = """
        (function(){
        var root=document.getElementById('fxdDev'); if(!root)return;
        var box=root.querySelector('#dwCards'), cards=[].slice.call(box.querySelectorAll('.dw-card')), STEP=12, shown=STEP, filter='all';
        var q=root.querySelector('#dwQ'), sortSel=root.querySelector('#dwSort'), more=root.querySelector('#dwMore'), empty=root.querySelector('#dwEmpty');
        var pendBtn=root.querySelector('.dw-seg [data-f=pend]');
        if(pendBtn){var n=cards.filter(function(c){return c.getAttribute('data-f').indexOf('pend')>=0;}).length;var em=document.createElement('em');em.textContent=n;pendBtn.appendChild(em);}
        function match(c){var f=c.getAttribute('data-f').split(' '),s=(q.value||'').trim().toLowerCase();
          if(filter!=='all'&&f.indexOf(filter)<0)return false; return !s||(c.getAttribute('data-q')||'').indexOf(s)>=0;}
        function render(){
          var k=sortSel.value, list=cards.slice().sort(function(a,b){
            if(k==='name')return a.getAttribute('data-name').localeCompare(b.getAttribute('data-name'));
            var x=k==='open'?'data-open':(k==='recent'?'data-recent':'data-face');
            return (parseFloat(b.getAttribute(x))||0)-(parseFloat(a.getAttribute(x))||0)||a.getAttribute('data-name').localeCompare(b.getAttribute('data-name'));});
          var hit=list.filter(match);
          list.forEach(function(c){c.hidden=true;box.appendChild(c);});
          hit.slice(0,shown).forEach(function(c){c.hidden=false;});
          empty.hidden=hit.length>0; more.hidden=hit.length<=shown;
        }
        q.addEventListener('input',function(){shown=STEP;render();});
        sortSel.addEventListener('change',function(){render();});
        more.addEventListener('click',function(){shown+=STEP;render();});
        root.addEventListener('click',function(e){var f=e.target.closest('.dw-seg button');if(!f)return;filter=f.getAttribute('data-f');
          [].forEach.call(root.querySelectorAll('.dw-seg button'),function(b){b.setAttribute('aria-pressed',b===f?'true':'false');});shown=STEP;render();});
        render();
        })();
        """;
}
