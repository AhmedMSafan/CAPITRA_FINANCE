using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 109 "Investors": the PL/SQL region "Investor Directory", ported.</summary>
public sealed class P0109InvestorDirectory(IApexSqlReader reader) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];
    private static readonly Dictionary<string, string?> NoItems = [];

    public int App => 105;

    public int Page => 109;

    public string Name => "REGION_INVESTOR_DIRECTORY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        // headline figures (one block in the PL/SQL: the first failing select leaves the rest at 0)
        decimal total = 0, active = 0, inactive = 0, outstanding = 0, contracts = 0, arrearsN = 0, arrearsAmt = 0, top10 = 0, noEmail = 0, noPhone = 0, noNid = 0;
        try
        {
            var h0 = await reader.RowAsync(MtgApexPageSql.P109_HEAD, NoItems, ct);
            total = h0?.Number("TOTAL") ?? 0;
            active = h0?.Number("ACTIVE") ?? 0;
            inactive = h0?.Number("INACTIVE") ?? 0;
            noEmail = h0?.Number("NO_EMAIL") ?? 0;
            noPhone = h0?.Number("NO_PHONE") ?? 0;
            noNid = h0?.Number("NO_NID") ?? 0;
            var x = await reader.RowAsync(MtgApexPageSql.P109_EXPOSURE, NoItems, ct);
            outstanding = x?.Number("OUTSTANDING") ?? 0;
            contracts = x?.Number("CONTRACTS") ?? 0;
            var a = await reader.RowAsync(MtgApexPageSql.P109_ARREARS, NoItems, ct);
            arrearsN = a?.Number("ARREARS_N") ?? 0;
            arrearsAmt = a?.Number("ARREARS_AMT") ?? 0;
            top10 = (await reader.RowAsync(MtgApexPageSql.P109_TOP10, NoItems, ct))?.Number("TOP10") ?? 0;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"fxdInv\">");
        h.AppendLine("<header class=\"fxd-hero fnv-list-hero\"><div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Master Data", "البيانات الرئيسية") + "</div>"
            + "<h1 class=\"fxd-h1\">" + T("Investors", "المستثمرون") + "</h1><p class=\"fxd-lead\">"
            + T("Every unit buyer behind the purchased portfolios, with their exposure, repayment progress and data quality in one view.", "جميع مشتري الوحدات في المحافظ المشتراة مع التعرض ونسبة السداد وجودة البيانات في شاشة واحدة.") + "</p></div>"
            + "<div class=\"fxd-actions fxd-anim d1\"><button type=\"button\" class=\"fxd-btn fxd-btn--glass\" id=\"invTableView\" aria-pressed=\"false\"><span class=\"fa fa-table\" aria-hidden=\"true\"></span>" + T("Table view", "عرض الجدول") + "</button>"
            + "<a class=\"fxd-btn fxd-btn--primary\" href=\"" + Url(110) + "\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("New investor", "مستثمر جديد") + "</a></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap\"><div class=\"inv-kpis\">");
        h.AppendLine("<div class=\"fxd-card inv-kpi fxd-anim d1\"><span class=\"fxd-label\">" + T("Investors", "المستثمرون") + "</span><div class=\"v\">" + ToChar(total, "FM999G999G990") + "</div>"
            + "<div class=\"s\">" + Num(active) + " " + T("active", "نشط") + " &middot; " + Num(inactive) + " " + T("inactive", "غير نشط") + "</div>"
            + "<div class=\"inv-split\" role=\"img\" aria-label=\"" + Num(active) + " active, " + Num(inactive) + " inactive\"><i style=\"width:" + (total > 0 ? Num(Round(active / total * 100)) : "0") + "%;background:var(--teal-600)\"></i><i style=\"flex:1;background:#C9D6DC\"></i></div></div>");
        h.AppendLine("<div class=\"fxd-card inv-kpi fxd-anim d2\"><span class=\"fxd-label\">" + T("Outstanding exposure", "إجمالي التعرض القائم") + "</span><div class=\"v\">" + Money(outstanding) + "</div>"
            + "<div class=\"s\">" + T("across", "عبر") + " " + ToChar(contracts, "FM999G999G990") + " " + T("contracts", "عقد") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card inv-kpi fxd-anim d3\"><span class=\"fxd-label\">" + T("In arrears", "متأخرون في السداد") + "</span><div class=\"v\"" + (arrearsN > 0 ? " style=\"color:var(--bad)\"" : string.Empty) + ">" + Num(arrearsN) + "</div>"
            + "<div class=\"s\">" + (arrearsN > 0 ? "<span class=\"fxd-pill fxd-pill--bad\">" + Money(arrearsAmt) + " " + T("overdue", "متأخرات") + "</span>" : "<span class=\"fxd-pill fxd-pill--ok\">" + T("no overdue instalments", "لا توجد أقساط متأخرة") + "</span>") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card inv-kpi fxd-anim d4\"><span class=\"fxd-label\">" + T("Top-10 concentration", "تركز أكبر 10 مستثمرين") + "</span><div class=\"v\">"
            + (outstanding > 0 ? ToChar(Round(top10 / outstanding * 100, 1), "FM990D0") + "%" : "&ndash;") + "</div><div class=\"s\">" + T("of outstanding exposure", "من التعرض القائم") + "</div></div>");
        h.AppendLine("</div>");

        // directory
        h.AppendLine("<div class=\"inv-grid\"><section class=\"fxd-card\" style=\"overflow:hidden\"><div class=\"inv-tools\">"
            + "<label class=\"inv-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"invQ\" aria-label=\"Search investors\" placeholder=\"Search name, code, national ID, phone or employer\"></label>"
            + "<div class=\"inv-seg\" role=\"group\" aria-label=\"Filter investors\">"
            + "<button type=\"button\" data-f=\"all\" aria-pressed=\"true\">" + T("All", "الكل") + "<em>" + Num(total) + "</em></button>"
            + "<button type=\"button\" data-f=\"active\" aria-pressed=\"false\">" + T("Active", "نشط") + "<em>" + Num(active) + "</em></button>"
            + "<button type=\"button\" data-f=\"arrears\" aria-pressed=\"false\">" + T("In arrears", "متأخرون") + "<em>" + Num(arrearsN) + "</em></button>"
            + "<button type=\"button\" data-f=\"inactive\" aria-pressed=\"false\">" + T("Inactive", "غير نشط") + "<em>" + Num(inactive) + "</em></button></div>"
            + "<select class=\"inv-sort\" id=\"invSort\" aria-label=\"Sort\"><option value=\"exp\">Sort: exposure</option><option value=\"name\">Sort: name</option><option value=\"code\">Sort: code</option><option value=\"paid\">Sort: % repaid</option></select></div>");
        h.AppendLine("<div class=\"inv-table-wrap\"><table class=\"inv-table\"><thead><tr><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Contact", "التواصل") + "</th><th>" + T("Employer & holdings", "جهة العمل والحيازات") + "</th>"
            + "<th class=\"inv-r\">" + T("Outstanding", "القائم") + "</th><th>" + T("Status", "الحالة") + "</th><th></th></tr></thead><tbody id=\"invRows\">");
        var rank = 0;
        try
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P109_ROWS, NoItems, ct))
            {
                rank++;
                string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE"), nid = r.Text("NATIONAL_ID");
                string? phone = r.Text("PHONE_NO"), email = r.Text("EMAIL_ADDRESS"), employer = r.Text("EMPLOYER_NAME");
                var isActive = r.Text("ACTIVE_FLAG") == "Y";
                decimal pf = r.Number("PF") ?? 0, cnt = r.Number("CT") ?? 0, cv = r.Number("CV") ?? 0, pv = r.Number("PV") ?? 0, ov = r.Number("OV") ?? 0, arr = r.Number("ARR") ?? 0;
                var id = Num(r.Number("INVESTOR_ID"));
                h.AppendLine("<tr tabindex=\"0\" data-href=\"" + Url(110, "P110_INVESTOR_ID", id) + "\""
                    + " data-f=\"" + (!isActive ? "inactive" : "active") + (arr > 0 ? " arrears" : string.Empty) + "\""
                    + " data-exp=\"" + Num(ov) + "\" data-paid=\"" + (cv > 0 ? Num(Round(pv / cv * 100)) : "0") + "\""
                    + " data-name=\"" + Attr((en ?? ar)?.ToLowerInvariant()) + "\" data-code=\"" + Attr(code) + "\""
                    + " data-q=\"" + Attr((code + " " + en + " " + ar + " " + nid + " " + phone + " " + email + " " + employer).ToLowerInvariant()) + "\">"
                    + "<td><div class=\"inv-who\"><span class=\"inv-av\" style=\"background:" + Colors[(rank - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div><strong>" + E(en ?? ar) + "</strong>"
                    + (ar is not null && en is not null ? "<span class=\"ar\">" + E(ar) + "</span>" : string.Empty)
                    + "<div class=\"inv-meta\"><span class=\"inv-code\">" + E(code) + "</span>" + (nid is not null ? "<span class=\"inv-code\">NID " + E(Mask(nid)) + "</span>" : string.Empty) + "</div></div></div></td>"
                    + "<td class=\"inv-contact\">" + Nvl(phone) + "<span>" + Nvl(email) + "</span></td>"
                    + "<td class=\"inv-contact\">" + Nvl(employer) + "<span>" + Num(pf) + " " + T("portfolios", "محافظ") + " &middot; " + Num(cnt) + " " + T("contracts", "عقود") + "</span></td>"
                    + "<td class=\"inv-r inv-out\"><b>" + (cnt == 0 ? "&ndash;" : Money(ov)) + "</b>"
                    + (cv > 0 ? "<div class=\"inv-paid\"><span class=\"bar\"><i style=\"width:" + Num(Math.Min(100, Round(pv / cv * 100))) + "%\"></i></span>" + Num(Round(pv / cv * 100)) + "% " + T("paid", "مسدد") + "</div>" : string.Empty) + "</td>"
                    + "<td>" + (!isActive ? "<span class=\"fxd-pill fxd-pill--mute\">" + T("Inactive", "غير نشط") + "</span>"
                        : arr > 0 ? "<span class=\"fxd-pill fxd-pill--bad\">" + T("In arrears", "متأخر") + "</span>"
                        : "<span class=\"fxd-pill fxd-pill--ok\">" + T("Active", "نشط") + "</span>") + "</td>"
                    + "<td class=\"inv-chev\" aria-hidden=\"true\">&rsaquo;</td></tr>");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        h.AppendLine("</tbody></table></div><div class=\"inv-empty\" id=\"invEmpty\" hidden>" + T("No investor matches these filters.", "لا يوجد مستثمر مطابق.") + "</div>"
            + "<div class=\"inv-foot\"><span id=\"invCount\" aria-live=\"polite\"></span><nav class=\"inv-pg\" id=\"invPg\" aria-label=\"Pages\"></nav></div></section>");

        // side insights
        h.AppendLine("<aside style=\"display:grid;gap:18px\"><section class=\"fxd-card fxd-anim d2\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-pie-chart\" aria-hidden=\"true\"></span>" + T("Top exposure", "الأعلى تعرضاً") + "</h2><span class=\"fxd-label\">" + T("share", "الحصة") + "</span></div><div class=\"fxd-card-b\" style=\"padding-top:6px\">");
        rank = 0;
        try
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P109_TOP5, NoItems, ct))
            {
                rank++;
                var ov = r.Number("OV") ?? 0;
                h.AppendLine("<a class=\"inv-top\" href=\"" + Url(110, "P110_INVESTOR_ID", Num(r.Number("INVESTOR_ID"))) + "\"><span class=\"n\">" + rank + "</span><div><strong>" + E(r.Text("NM")) + "</strong><div class=\"bar\"><i style=\"width:"
                    + (outstanding > 0 ? Num(Math.Min(100, Round(ov / outstanding * 100 * 8))) : "0") + "%\"></i></div></div><b>" + (outstanding > 0 ? ToChar(Round(ov / outstanding * 100, 1), "FM990D0") + "%" : "-") + "</b></a>");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        if (rank == 0)
            h.AppendLine("<div class=\"inv-empty\">" + T("No exposure recorded yet.", "لا يوجد تعرض مسجل بعد.") + "</div>");
        h.AppendLine("</div></section>");
        h.AppendLine("<section class=\"fxd-card fxd-anim d3\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\"></span>" + T("Arrears watch", "متابعة المتأخرات") + "</h2></div>"
            + "<div class=\"fxd-card-b inv-watch" + (arrearsN == 0 ? " ok" : string.Empty) + "\"><span class=\"big\">" + Num(arrearsN) + "</span><p>"
            + (arrearsN > 0 ? T("investors have overdue instalments", "مستثمرين لديهم أقساط متأخرة") + ", <b style=\"color:var(--ink)\">" + Money(arrearsAmt) + "</b> " + T("past due.", "متأخرة.")
                : T("investors with overdue instalments.", "مستثمر لديه أقساط متأخرة."))
            + " <a href=\"" + Url(736) + "\">" + T("Open aging report", "فتح تقرير الأعمار") + " &rsaquo;</a></p></div></section>");
        var gaps = (noEmail > 0 ? 1 : 0) + (noPhone > 0 ? 1 : 0) + (noNid > 0 ? 1 : 0);
        h.AppendLine("<section class=\"fxd-card fxd-anim d4\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check-square-o\" aria-hidden=\"true\"></span>" + T("Data quality", "جودة البيانات") + "</h2>"
            + (noEmail + noPhone + noNid > 0 ? "<span class=\"fxd-pill fxd-pill--warn\">" + gaps + " " + T("gaps", "فجوات") + "</span>"
                : "<span class=\"fxd-pill fxd-pill--ok\">" + T("Complete", "مكتملة") + "</span>") + "</div><div class=\"fxd-card-b inv-dq\">"
            + "<button type=\"button\" data-dq=\"email\"><span><span class=\"fa fa-envelope-o\" aria-hidden=\"true\"></span>" + T("Missing email", "بريد إلكتروني مفقود") + "</span><b>" + Num(noEmail) + "</b></button>"
            + "<button type=\"button\" data-dq=\"phone\"><span><span class=\"fa fa-phone\" aria-hidden=\"true\"></span>" + T("Missing phone", "هاتف مفقود") + "</span><b>" + Num(noPhone) + "</b></button>"
            + "<button type=\"button\" data-dq=\"nid\"><span><span class=\"fa fa-id-card-o\" aria-hidden=\"true\"></span>" + T("Missing national ID", "رقم قومي مفقود") + "</span><b>" + Num(noNid) + "</b></button>"
            + "</div></section></aside></div></div></div>");
        h.AppendLine("<script>" + Script + "</script>");
        return new { ok = true, html = h.ToString() };
    }

    // nvl(e(p),'&ndash;')
    private static string Nvl(string? p) => p is null ? "&ndash;" : E(p);

    // mask(p): the first and last four characters, bullets between
    private static string Mask(string p) =>
        p.Length <= 6 ? p : p[..4] + (p.Length - 8 > 0 ? new string('•', p.Length - 8) : string.Empty) + p[^4..];

    private static string Num(decimal? n) => P0203Html.Num(n);

    private static decimal Round(decimal n, int d = 0) => P0203Html.Round(n, d);

    private const string Script = """
        (function(){
        var root=document.getElementById('fxdInv'); if(!root)return;
        var rows=[].slice.call(root.querySelectorAll('#invRows tr')), tbody=root.querySelector('#invRows'), PAGE=10, page=1, filter='all', dq=null;
        var q=root.querySelector('#invQ'), sortSel=root.querySelector('#invSort'), pg=root.querySelector('#invPg'), cnt=root.querySelector('#invCount'), empty=root.querySelector('#invEmpty');
        function ar(){return document.documentElement.dir==='rtl';}
        function match(tr){
          var f=tr.getAttribute('data-f'), s=(q.value||'').trim().toLowerCase(), t=tr.getAttribute('data-q')||'';
          if(filter!=='all'&&f.split(' ').indexOf(filter)<0)return false;
          if(dq==='email'&&tr.querySelector('.inv-contact span').textContent.trim()!=='–')return false;
          if(dq==='phone'&&tr.querySelector('.inv-contact').firstChild.textContent.trim()!=='–')return false;
          if(dq==='nid'&&tr.querySelector('.inv-code+.inv-code'))return false;
          return !s||t.indexOf(s)>=0;
        }
        function render(){
          var k=sortSel.value, list=rows.slice().sort(function(a,b){
            if(k==='name')return a.getAttribute('data-name').localeCompare(b.getAttribute('data-name'));
            if(k==='code')return a.getAttribute('data-code').localeCompare(b.getAttribute('data-code'));
            var x=k==='paid'?'data-paid':'data-exp';return parseFloat(b.getAttribute(x))-parseFloat(a.getAttribute(x));});
          var hit=list.filter(match), pages=Math.max(1,Math.ceil(hit.length/PAGE)); if(page>pages)page=pages;
          list.forEach(function(tr){tr.hidden=true;tbody.appendChild(tr);});
          hit.slice((page-1)*PAGE,page*PAGE).forEach(function(tr){tr.hidden=false;});
          empty.hidden=hit.length>0;
          var a=hit.length?(page-1)*PAGE+1:0, b=Math.min(page*PAGE,hit.length);
          cnt.textContent=ar()?('عرض '+a+'–'+b+' من '+hit.length):('Showing '+a+'–'+b+' of '+hit.length+' investors');
          var h='<button type="button" data-p="'+(page-1)+'"'+(page<=1?' disabled':'')+' aria-label="Previous page">&lsaquo;</button>';
          for(var i=1;i<=pages;i++){if(pages>7&&i>2&&i<pages-1&&Math.abs(i-page)>1){if(i===3||i===pages-2)h+='<span style="padding:6px 2px">&hellip;</span>';continue;}
            h+='<button type="button" data-p="'+i+'"'+(i===page?' aria-current="page"':'')+'>'+i+'</button>';}
          h+='<button type="button" data-p="'+(page+1)+'"'+(page>=pages?' disabled':'')+' aria-label="Next page">&rsaquo;</button>';
          pg.innerHTML=h;
        }
        q.addEventListener('input',function(){page=1;render();});
        sortSel.addEventListener('change',function(){page=1;render();});
        root.addEventListener('click',function(e){
          var f=e.target.closest('.inv-seg button'); if(f){filter=f.getAttribute('data-f');dq=null;[].forEach.call(root.querySelectorAll('.inv-seg button'),function(b){b.setAttribute('aria-pressed',b===f?'true':'false');});page=1;render();return;}
          var p=e.target.closest('[data-p]'); if(p&&!p.disabled){page=+p.getAttribute('data-p');render();root.querySelector('.inv-tools').scrollIntoView({behavior:'smooth',block:'start'});return;}
          var d=e.target.closest('[data-dq]'); if(d){dq=dq===d.getAttribute('data-dq')?null:d.getAttribute('data-dq');page=1;render();root.querySelector('.inv-tools').scrollIntoView({behavior:'smooth',block:'start'});return;}
          if(e.target.closest('#invTableView')){var ir=document.getElementById('investors'),b2=e.target.closest('#invTableView');if(ir){var on=ir.classList.toggle('fxd-ir-hidden');b2.setAttribute('aria-pressed',on?'false':'true');if(!on)ir.scrollIntoView({behavior:'smooth'});}return;}
          var tr=e.target.closest('#invRows tr'); if(tr&&!e.target.closest('a')){window.location.href=tr.getAttribute('data-href');}
        });
        root.addEventListener('keydown',function(e){var tr=e.target.closest&&e.target.closest('#invRows tr');if(tr&&(e.key==='Enter'||e.key===' ')){e.preventDefault();window.location.href=tr.getAttribute('data-href');}});
        if(window.apex&&apex.jQuery){apex.jQuery(window).on('apexafterclosedialog',function(){window.location.reload();});}
        render();
        })();
        """;
}
