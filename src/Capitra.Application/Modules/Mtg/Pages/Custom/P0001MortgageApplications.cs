using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 1 "Mortgage Applications": the PL/SQL dynamic content region, ported. It prints the same HTML with
/// the same counts; the user's tasks come from the Capitra workflow inbox.
/// </summary>
public sealed class P0001MortgageApplications(IApexSqlReader reader, WfeInbox inbox, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 1;

    public string Name => "REGION_MORTGAGE_APPLICATIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var name = user.FullName ?? CultureInfo.InvariantCulture.TextInfo.ToTitleCase((user.UserName ?? string.Empty).ToLowerInvariant());
        var cases = await Num(MtgApexPageSql.P1_CASES, ct);
        var face = await Num(MtgApexPageSql.P1_FACE, ct);
        var ready = await Num(MtgApexPageSql.P1_READY, ct);
        var approval = await Num(MtgApexPageSql.P1_APPROVAL, ct);
        decimal? tasks = null, overdue = null;
        try
        {
            var mine = await inbox.MyInboxAsync(ct);
            tasks = mine.Count;
            overdue = mine.Count(t => t.DueDate < clock.UtcNow);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // num(): any error leaves the count empty
        }

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"fxdHome\">");
        h.AppendLine("<header class=\"fxd-hero\"><div class=\"fxd-row\"><div class=\"fxd-anim\">"
            + "<div class=\"fxd-eyebrow\"><i></i>" + T("FINOVA Mortgage Finance", "فينوفا للتمويل العقاري") + "</div>"
            + "<h1 class=\"fxd-h1\"><span id=\"fxdGreet\" data-finova-en=\"Welcome\" data-finova-ar=\"مرحباً\">Welcome</span>, " + E(name) + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Acquire, assess and approve mortgage portfolios in one place. Here is where things stand today.", "استحواذ وتقييم واعتماد محافظ التمويل العقاري في مكان واحد. إليك الوضع اليوم.") + "</p></div>"
            + "<div class=\"fxd-hero-side fxd-anim d1\"><div class=\"fxd-date\" id=\"fxdDate\"></div><div class=\"fxd-actions\">"
            + "<a class=\"fxd-btn fxd-btn--glass\" href=\"" + Url(7) + "\"><span class=\"fa fa-building-o\" aria-hidden=\"true\"></span>" + T("Developer Workspace", "مساحة عمل المطورين") + "</a>"
            + "<a class=\"fxd-btn fxd-btn--primary\" href=\"" + Url(920) + "\"><span class=\"fa fa-inbox\" aria-hidden=\"true\"></span>" + T("My Inbox", "صندوق مهامي")
            + ((tasks ?? 0) > 0 ? " <span class=\"fxd-pill fxd-pill--plain\" style=\"height:22px;background:rgba(255,255,255,.22);color:#fff\">" + tasks + "</span>" : string.Empty) + "</a>"
            + "</div></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap\">");
        h.AppendLine("<div class=\"fxd-kpis\">");
        h.AppendLine("<a class=\"fxd-card fxd-kpi fxd-anim d1\" href=\"" + Url(201) + "\"><div class=\"fxd-kpi-top\"><span class=\"fxd-label\">" + T("Active portfolios", "المحافظ النشطة") + "</span><span class=\"fxd-kpi-ico fa fa-briefcase\" aria-hidden=\"true\"></span></div>"
            + "<div class=\"fxd-kpi-v\">" + Show(cases) + "</div><div class=\"fxd-kpi-s\">" + T("cases in the acquisition pipeline", "ملف في خط الاستحواذ") + "</div></a>");
        h.AppendLine("<a class=\"fxd-card fxd-kpi fxd-anim d2\" href=\"" + Url(201) + "\"><div class=\"fxd-kpi-top\"><span class=\"fxd-label\">" + T("Pipeline face value", "القيمة الاسمية لخط المعالجة") + "</span><span class=\"fxd-kpi-ico fa fa-line-chart\" aria-hidden=\"true\"></span></div>"
            + "<div class=\"fxd-kpi-v\">" + Money(face) + "</div><div class=\"fxd-kpi-s\">" + T("across all open cases", "عبر جميع الملفات المفتوحة") + "</div></a>");
        h.AppendLine("<a class=\"fxd-card fxd-kpi fxd-anim d3\" href=\"" + Url(201) + "\"><div class=\"fxd-kpi-top\"><span class=\"fxd-label\">" + T("Awaiting decision", "بانتظار القرار") + "</span><span class=\"fxd-kpi-ico fa fa-gavel\" aria-hidden=\"true\"></span></div>"
            + "<div class=\"fxd-kpi-v\">" + Show(approval) + "</div><div class=\"fxd-kpi-s\">" + Show(ready) + " " + T("more ready to submit", "أخرى جاهزة للإرسال") + "</div></a>");
        h.AppendLine("<a class=\"fxd-card fxd-kpi fxd-anim d4\" href=\"" + Url(920) + "\"><div class=\"fxd-kpi-top\"><span class=\"fxd-label\">" + T("My open tasks", "مهامي المفتوحة") + "</span><span class=\"fxd-kpi-ico fa fa-check-square-o\" aria-hidden=\"true\"></span></div>"
            + "<div class=\"fxd-kpi-v\">" + Show(tasks) + "</div><div class=\"fxd-kpi-s\">"
            + ((overdue ?? 0) > 0 ? "<span class=\"fxd-pill fxd-pill--bad\">" + overdue + " " + T("overdue", "متأخرة") + "</span>" : "<span class=\"fxd-pill fxd-pill--ok\">" + T("all on track", "كلها في موعدها") + "</span>") + "</div></a>");
        h.AppendLine("</div>");

        h.AppendLine("<div class=\"fxd-section-title\"><h2>" + T("Lines of business", "خطوط الأعمال") + "</h2><span>" + T("Choose where you want to work", "اختر مجال العمل") + "</span></div>");
        h.AppendLine("<div class=\"fxd-mods\">");
        h.AppendLine("<a class=\"fxd-card fxd-mod fxd-mod--live fxd-anim d1\" href=\"" + Url(2) + "\"><span class=\"fxd-mod-tag fxd-pill fxd-pill--ok\">" + T("Live", "متاح") + "</span>"
            + "<span class=\"fxd-mod-ico fa fa-briefcase\" aria-hidden=\"true\"></span><h3>" + T("Purchased Portfolio", "المحافظ المشتراة") + "</h3>"
            + "<p>" + T("Acquire developer mortgage portfolios end to end: due diligence, valuation, legal review, pricing and committee approval.", "استحواذ محافظ التمويل العقاري من المطورين من البداية للنهاية: العناية الواجبة والتقييم والمراجعة القانونية والتسعير واعتماد اللجنة.") + "</p>"
            + "<div class=\"fxd-mod-stats\"><div><b>" + Show(cases) + "</b><span>" + T("open cases", "ملفات مفتوحة") + "</span></div>"
            + "<div><b>" + Show(approval) + "</b><span>" + T("in approval", "قيد الاعتماد") + "</span></div>"
            + "<div><b>" + Money(face) + "</b><span>" + T("face value", "القيمة الاسمية") + "</span></div></div>"
            + "<span class=\"fxd-mod-go\">" + T("Open workspace", "فتح مساحة العمل") + " <span class=\"fa fa-arrow-right\" aria-hidden=\"true\"></span></span></a>");
        h.AppendLine("<div class=\"fxd-card fxd-mod fxd-mod--soon fxd-anim d2\" aria-disabled=\"true\"><span class=\"fxd-mod-tag fxd-pill fxd-pill--mute\">" + T("Coming soon", "قريباً") + "</span>"
            + "<span class=\"fxd-mod-ico fa fa-file-text-o\" aria-hidden=\"true\"></span><h3>" + T("MFF Application", "طلبات صندوق التمويل العقاري") + "</h3>"
            + "<p>" + T("Origination and approval of Mortgage Finance Fund applications.", "إنشاء واعتماد طلبات صندوق التمويل العقاري.") + "</p>"
            + "<div class=\"fxd-mod-foot\"><span class=\"fa fa-clock-o\" aria-hidden=\"true\"></span> " + T("Planned for the next release", "مخطط له في الإصدار القادم") + "</div></div>");
        h.AppendLine("<div class=\"fxd-card fxd-mod fxd-mod--soon fxd-anim d3\" aria-disabled=\"true\"><span class=\"fxd-mod-tag fxd-pill fxd-pill--mute\">" + T("Coming soon", "قريباً") + "</span>"
            + "<span class=\"fxd-mod-ico fa fa-home\" aria-hidden=\"true\"></span><h3>" + T("Personal Mortgage", "التمويل العقاري للأفراد") + "</h3>"
            + "<p>" + T("Retail home-finance applications from intake to disbursement.", "طلبات التمويل العقاري للأفراد من التقديم حتى الصرف.") + "</p>"
            + "<div class=\"fxd-mod-foot\"><span class=\"fa fa-clock-o\" aria-hidden=\"true\"></span> " + T("Planned for the next release", "مخطط له في الإصدار القادم") + "</div></div>");
        h.AppendLine("</div>");

        h.AppendLine("<div class=\"fxd-section-title\"><h2>" + T("Quick access", "وصول سريع") + "</h2></div><div class=\"fxd-quick\">");
        h.AppendLine("<a class=\"fxd-card fxd-q\" href=\"" + Url(201) + "\"><span class=\"fa fa-folder-open-o\" aria-hidden=\"true\"></span><span><strong>" + T("Portfolio Cases", "ملفات المحافظ") + "</strong><small>" + T("All acquisition cases", "جميع ملفات الاستحواذ") + "</small></span></a>");
        h.AppendLine("<a class=\"fxd-card fxd-q\" href=\"" + Url(920) + "\"><span class=\"fa fa-inbox\" aria-hidden=\"true\"></span><span><strong>" + T("Workflow Inbox", "صندوق سير العمل") + "</strong><small>" + T("Tasks assigned to you", "المهام المسندة إليك") + "</small></span></a>");
        h.AppendLine("<a class=\"fxd-card fxd-q\" href=\"" + Url(5) + "\"><span class=\"fa fa-heartbeat\" aria-hidden=\"true\"></span><span><strong>" + T("Monitoring", "المتابعة") + "</strong><small>" + T("Exceptions and expiries", "الاستثناءات وانتهاء الصلاحية") + "</small></span></a>");
        h.AppendLine("<a class=\"fxd-card fxd-q\" href=\"" + Url(6) + "\"><span class=\"fa fa-bar-chart\" aria-hidden=\"true\"></span><span><strong>" + T("Reports", "التقارير") + "</strong><small>" + T("Registers and MIS", "السجلات والتحليلات") + "</small></span></a>");
        h.AppendLine("<a class=\"fxd-card fxd-q\" href=\"" + Url(4) + "\"><span class=\"fa fa-building\" aria-hidden=\"true\"></span><span><strong>" + T("Master Data", "البيانات الرئيسية") + "</strong><small>" + T("Developers, projects, units", "المطورون والمشروعات والوحدات") + "</small></span></a>");
        h.AppendLine("</div></div></div>");

        h.AppendLine("<script>(function(){var ar=(document.documentElement.lang||\"\").indexOf(\"ar\")===0||document.documentElement.dir===\"rtl\";"
            + "var h=new Date().getHours(),g=document.getElementById(\"fxdGreet\");"
            + "var en=h<12?\"Good morning\":(h<18?\"Good afternoon\":\"Good evening\"),a=h<12?\"صباح الخير\":\"مساء الخير\";"
            + "if(g){g.setAttribute(\"data-finova-en\",en);g.setAttribute(\"data-finova-ar\",a);g.textContent=ar?a:en;}"
            + "var d=document.getElementById(\"fxdDate\");if(d){try{d.textContent=new Date().toLocaleDateString(ar?\"ar-EG\":\"en-GB\",{weekday:\"long\",day:\"numeric\",month:\"long\",year:\"numeric\"});}catch(x){}}})();</script>");
        return new { html = h.ToString() };
    }

    // num(): the value of a count query, empty when it fails
    private async Task<decimal?> Num(string sql, CancellationToken ct)
    {
        try
        {
            return await reader.NumberAsync(sql, null, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return null;
        }
    }
}
