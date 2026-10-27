using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 2 "Purchased Portfolio": the PL/SQL region, ported. The user's tasks come from the Capitra workflow inbox.
/// </summary>
public sealed class P0002PurchasedPortfolio(IApexSqlReader reader, WfeInbox inbox, ICurrentUser user, IClock clock) : IApexProcess
{
    private static readonly string[] Colors = ["#7FB3B8", "#3E8D94", "#00636D", "#014B55", "#E87722", "#B4520B", "#0E8A5A", "#5F7787", "#9DB0BB", "#C8321F"];

    public int App => 105;

    public int Page => 2;

    public string Name => "REGION_PURCHASED_PORTFOLIO";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        decimal tasks = 0, overdue = 0, cases = 0, face = 0, ready = 0, expiring = 0, expCases = 0;
        IReadOnlyList<WfeInboxItemDto> mine = [];
        try
        {
            mine = await inbox.MyInboxAsync(ct);
            tasks = mine.Count;
            overdue = mine.Count(Overdue);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        if (await Try(MtgApexPageSql.P2_CASES, ct) is { } c)
        {
            cases = c.Number("CASES") ?? 0;
            face = c.Number("FACE") ?? 0;
            ready = c.Number("READY") ?? 0;
        }

        if (await Try(MtgApexPageSql.P2_EXPIRING, ct) is { } x)
        {
            expiring = x.Number("EXPIRING") ?? 0;
            expCases = x.Number("EXP_CASES") ?? 0;
        }

        var name = user.FullName ?? CultureInfo.InvariantCulture.TextInfo.ToTitleCase((user.UserName ?? string.Empty).ToLowerInvariant());
        var h = new StringBuilder();
        // balanced colours (asked for this page): the two brand colours, teal and orange, in turn; a neutral pill when there is nothing
        h.AppendLine("<style>.fx-p2 .fx-links .fx-link:nth-child(4n+2) .fa,.fx-p2 .fx-links .fx-link:nth-child(4n+3) .fa{background:var(--fin-orange-50);color:var(--fin-orange-700)}"
            + ".fx-p2 .fx-head a:not(.fx-btn){color:var(--fin-teal-600);font-weight:600;text-decoration:none}.fx-p2 .fx-head a:not(.fx-btn):hover{text-decoration:underline}</style>");
        h.AppendLine("<div class=\"fx fx-p2\">");
        h.AppendLine("<div class=\"fx-head\"><div><div class=\"fx-crumb\">" + T("Purchased Portfolio", "المحافظ المشتراة") + "</div>"
            + "<h1>" + T("Welcome back", "مرحباً بعودتك") + ", " + E(name) + "</h1>"
            + "<p>" + T("Your work, the pipeline and what needs attention today.", "مهامك وخط المعالجة وما يحتاج إلى متابعة اليوم.") + "</p></div>"
            + "<div class=\"fx-actions\"><a class=\"fx-btn\" href=\"" + Url(208) + "\"><span class=\"fa fa-upload\" aria-hidden=\"true\"></span>" + T("Import accounts", "استيراد الحسابات") + "</a>"
            + "<a class=\"fx-btn fx-btn--primary\" href=\"" + Url(202) + "\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("New portfolio case", "ملف محفظة جديد") + "</a></div></div>");

        h.AppendLine("<div class=\"fx-kpis\">");
        h.AppendLine("<a class=\"fx-card fx-kpi\" href=\"" + Url(920) + "\"><span class=\"fx-kpi-label\">" + T("Tasks waiting for me", "مهام بانتظاري") + "</span><span class=\"fx-kpi-value\">" + N(tasks) + "</span>"
            + (overdue > 0 ? "<span class=\"fx-chip fx-chip--bad\">" + N(overdue) + " " + T("overdue", "متأخرة") + "</span>" : "<span class=\"fx-chip fx-chip--teal\">" + T("nothing overdue", "لا يوجد تأخير") + "</span>") + "</a>");
        h.AppendLine("<a class=\"fx-card fx-kpi\" href=\"" + Url(201) + "\"><span class=\"fx-kpi-label\">" + T("Cases in pipeline", "ملفات قيد المعالجة") + "</span><span class=\"fx-kpi-value\">" + N(cases) + "</span>"
            + "<span class=\"fx-muted fx-num\">" + Money2(face) + " " + T("face value", "القيمة الاسمية") + "</span></a>");
        h.AppendLine("<a class=\"fx-card fx-kpi\" href=\"" + Url(706) + "\"><span class=\"fx-kpi-label\">" + T("Ready to submit", "جاهزة للإرسال") + "</span><span class=\"fx-kpi-value\">" + N(ready) + "</span>"
            + "<span class=\"fx-chip fx-chip--brand\">" + T("all modules complete", "اكتملت جميع الوحدات") + "</span></a>");
        h.AppendLine("<a class=\"fx-card fx-kpi\" href=\"" + Url(703) + "\"><span class=\"fx-kpi-label\">" + T("Documents expiring in 30 days", "مستندات تنتهي خلال 30 يوماً") + "</span><span class=\"fx-kpi-value\">" + N(expiring) + "</span>"
            + (expiring > 0 ? "<span class=\"fx-chip fx-chip--warn\">" + N(expCases) + " " + T("cases affected", "ملفات متأثرة") + "</span>" : "<span class=\"fx-chip\">" + T("none", "لا يوجد") + "</span>") + "</a>");
        h.AppendLine("</div>");

        h.AppendLine("<div class=\"fx-grid-split\">");
        // pipeline by stage
        h.AppendLine("<section class=\"fx-card fx-card-pad\"><div class=\"fx-head\" style=\"margin:0\"><h2 class=\"fx-h2\">" + T("Pipeline by status", "خط المعالجة حسب الحالة") + "</h2><a href=\"" + Url(713) + "\">" + T("Pipeline aging", "أعمار خط المعالجة") + "</a></div>");
        try
        {
            var total = await reader.NumberAsync(MtgApexPageSql.P2_PIPE_TOTAL, NoItems, ct) ?? 0;
            if (total > 0)
            {
                var pipe = await reader.QueryAsync(MtgApexPageSql.P2_PIPE, NoItems, ct);
                h.AppendLine("<div class=\"fx-pipe\" role=\"img\" aria-label=\"" + N(total) + " cases by status\">");
                var i = 0;
                foreach (var r in pipe)
                {
                    i++;
                    var cnt = N(r.Number("CNT"));
                    h.AppendLine("<a href=\"" + Url(201) + "\" style=\"flex:" + cnt + ";background:" + Colors[Math.Min(i, 10) - 1] + "\" title=\"" + Attr(r.Text("NAME")) + ": " + cnt + "\">" + cnt + "</a>");
                }

                h.AppendLine("</div><div class=\"fx-legend\">");
                i = 0;
                foreach (var r in pipe)
                {
                    i++;
                    h.AppendLine("<span><i style=\"background:" + Colors[Math.Min(i, 10) - 1] + "\"></i>" + E(r.Text("NAME")) + "</span>");
                }

                h.AppendLine("</div>");
            }
            else
            {
                h.AppendLine("<div class=\"fx-empty\"><span class=\"fa fa-inbox\" aria-hidden=\"true\"></span>" + T("No open cases yet.", "لا توجد ملفات مفتوحة بعد.") + "</div>");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            h.AppendLine("<div class=\"fx-empty\">" + T("Pipeline data is not available.", "بيانات خط المعالجة غير متاحة.") + "</div>");
        }

        // latest cases
        h.AppendLine("<div class=\"fx-table-wrap\" style=\"margin-top:14px\"><table class=\"fx-table\"><thead><tr><th>" + T("Latest cases", "أحدث الملفات") + "</th><th>" + T("Developer", "المطور") + "</th><th>" + T("Status", "الحالة") + "</th><th class=\"fx-r\">" + T("Face value", "القيمة الاسمية") + "</th></tr></thead><tbody>");
        try
        {
            foreach (var r in await reader.QueryAsync(MtgApexPageSql.P2_LATEST, NoItems, ct))
            {
                h.AppendLine("<tr><td><a href=\"" + Url(203, "P203_PORTFOLIO_ID", N(r.Number("PORTFOLIO_ID"))) + "\">" + E(r.Text("PORTFOLIO_NO")) + "</a></td>"
                    + "<td>" + E(r.Text("DEVELOPER_NAME_EN") ?? "-") + "<span class=\"fx-cell-sub\">" + E(r.Text("PROJECT_NAME_EN") ?? string.Empty) + "</span></td>"
                    + "<td><span class=\"fx-chip fx-chip--teal\">" + E(r.Text("STATUS_NAME_EN") ?? "-") + "</span></td>"
                    + "<td class=\"fx-r\">" + Money2(r.Number("FACE_VALUE")) + "</td></tr>");
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // exception when others then null
        }

        h.AppendLine("</tbody></table></div></section>");

        // my next tasks + shortcuts
        h.AppendLine("<section class=\"fx-card fx-card-pad\"><div class=\"fx-head\" style=\"margin:0\"><h2 class=\"fx-h2\">" + T("My next tasks", "مهامي القادمة") + "</h2><a href=\"" + Url(920) + "\">" + T("Open inbox", "فتح صندوق المهام") + "</a></div>");
        h.AppendLine("<table class=\"fx-table\"><tbody>");
        var next = mine.OrderBy(t => Overdue(t) ? 0 : 1).ThenBy(t => t.DueDate ?? DateTime.MaxValue).ThenBy(t => t.CreatedOn).Take(5).ToList();
        foreach (var r in next)
        {
            var od = Overdue(r);
            h.AppendLine("<tr" + (od ? " class=\"is-overdue\"" : string.Empty) + "><td><a href=\"" + Url(921, "P921_TASK_ID", r.InboxId.ToString(CultureInfo.InvariantCulture)) + "\">" + E(r.Subject) + "</a>"
                + "<span class=\"fx-cell-sub\">" + E(r.EntityRef ?? string.Empty) + "</span></td><td class=\"fx-r\">"
                + (od ? "<span class=\"fx-chip fx-chip--bad\">" + T("Overdue", "متأخرة") + "</span>"
                    : r.DueDate is { } due && due < clock.UtcNow.AddDays(1) ? "<span class=\"fx-chip fx-chip--warn\">" + T("Due today", "مستحقة اليوم") + "</span>"
                    : r.DueDate is { } d ? "<span class=\"fx-chip\">" + d.ToString("dd-MMM", CultureInfo.InvariantCulture).ToUpperInvariant() + "</span>"
                    : "<span class=\"fx-chip\">" + T("No due date", "بدون موعد") + "</span>")
                + "</td></tr>");
        }

        if (next.Count == 0)
            h.AppendLine("<tr><td><div class=\"fx-empty\"><span class=\"fa fa-check-circle-o\" aria-hidden=\"true\"></span>" + T("You are all caught up.", "لا توجد مهام بانتظارك.") + "</div></td></tr>");
        h.AppendLine("</tbody></table>");
        h.AppendLine("<h2 class=\"fx-h2\" style=\"margin:18px 0 10px\">" + T("Go to", "انتقل إلى") + "</h2><div class=\"fx-links\">");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(7) + "\"><span class=\"fa fa-building-o\" aria-hidden=\"true\"></span>" + T("Developer Workspace", "مساحة عمل المطورين") + "</a>");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(5) + "\"><span class=\"fa fa-heartbeat\" aria-hidden=\"true\"></span>" + T("Operational Monitoring", "المتابعة التشغيلية") + "</a>");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(6) + "\"><span class=\"fa fa-table\" aria-hidden=\"true\"></span>" + T("Report Catalogue", "دليل التقارير") + "</a>");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(4) + "\"><span class=\"fa fa-building\" aria-hidden=\"true\"></span>" + T("Master Data", "البيانات الرئيسية") + "</a>");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(3) + "\"><span class=\"fa fa-database\" aria-hidden=\"true\"></span>" + T("Reference Data", "البيانات المرجعية") + "</a>");
        h.AppendLine("<a class=\"fx-link\" href=\"" + Url(710) + "\"><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("MIS & Analytics", "التحليلات الإدارية") + "</a>");
        h.AppendLine("</div></section></div></div>");
        return new { ok = true, html = h.ToString() };
    }

    // task_sla_status = 'OVERDUE'
    private static readonly Dictionary<string, string?> NoItems = [];

    private bool Overdue(WfeInboxItemDto t) => t.DueDate is { } d && d < clock.UtcNow;

    private async Task<IReadOnlyDictionary<string, object?>?> Try(string sql, CancellationToken ct)
    {
        try
        {
            return await reader.RowAsync(sql, NoItems, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return null;
        }
    }

    private static string N(decimal? n) => (n ?? 0).ToString("0.############", CultureInfo.InvariantCulture);

    // this page's money(): '-' when null
    private static string Money2(decimal? n) => n is null ? "-" : Money(n);
}
