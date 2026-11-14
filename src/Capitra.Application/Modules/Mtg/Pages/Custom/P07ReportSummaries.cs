using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// The PL/SQL region "Report Summary" of the App 105 report pages: the header with the KPIs of the report and its top 5,
/// over the report's own query and filters.
/// </summary>
internal static class ReportSummary
{
    public sealed record Kpi(string Format, bool Highlight, string En, string Ar);

    public sealed record Spec(string CrumbEn, string CrumbAr, string TitleEn, string TitleAr, string LeadEn, string LeadAr,
        Kpi[] Kpis, string Icon, string TopEn, string TopAr, bool TopMoney);

    public static async Task<object> RenderAsync(IApexSqlReader reader, ApexRequest request, Spec s, string kpiSql, string topSql, CancellationToken ct)
    {
        var items = request.Items ?? [];
        try
        {
            var k = await reader.RowAsync(kpiSql, items, ct);
            var h = new StringBuilder();
            h.AppendLine("<div class=\"fxd rp\"><header class=\"fxd-hero rp-hero\"><nav class=\"rp-crumbs\"><a href=\"" + Url(6) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span>" + T("Report catalogue", "دليل التقارير") + "</a><span>/</span><span>" + T(s.CrumbEn, s.CrumbAr) + "</span></nav>");
            h.AppendLine("<div class=\"rp-grid\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T(s.CrumbEn, s.CrumbAr) + "</div><h1 class=\"fxd-h1\">" + T(s.TitleEn, s.TitleAr) + "</h1><p class=\"fxd-lead\">" + T(s.LeadEn, s.LeadAr) + "</p><div class=\"rp-kpis\">");
            for (var i = 0; i < s.Kpis.Length; i++)
            {
                var kp = s.Kpis[i];
                var v = k?.Number("K" + (i + 1));
                var b = kp.Format switch
                {
                    "money" => Money(v),
                    "pct1" => (v is null ? "0" : ToChar(v, "FM990D0")) + "%",
                    "dec1" => v is null ? "0" : ToChar(v, "FM999G990D0"),
                    _ => ToChar(v ?? 0, "FM999G999G999G990"),
                };
                h.AppendLine("<div class=\"rp-kpi" + (kp.Highlight ? " hl" : string.Empty) + "\"><b>" + b + "</b><span>" + T(kp.En, kp.Ar) + "</span></div>");
            }

            h.AppendLine("</div></div><aside class=\"rp-top fxd-anim d1\"><h3><span class=\"fa " + s.Icon + "\" aria-hidden=\"true\"></span>" + T(s.TopEn, s.TopAr) + "</h3><ol>");
            var n = 0;
            foreach (var r in await reader.QueryAsync(topSql, items, ct))
            {
                n++;
                var lbl = r.Text("LBL");
                var val = r.Number("VAL");
                var mx = r.Number("MX");
                var tot = r.Number("TOT");
                var b = s.TopMoney
                    ? Money(val) + ((tot ?? 0) > 0 ? " &middot; " + Rnd(100 * val!.Value / tot!.Value) + "%" : string.Empty)
                    : ToChar(val ?? 0, "FM999G999G999G990");
                h.AppendLine("<li><span title=\"" + Attr(lbl) + "\">" + E(lbl ?? "-") + "</span><b>" + b + "</b><i style=\"--w:" + ((mx ?? 0) > 0 ? Rnd(100 * (val ?? 0) / mx!.Value) : "0") + "%\"></i></li>");
            }

            if (n == 0)
                h.AppendLine("<li class=\"none\">" + T("No data for these filters.", "لا توجد بيانات لهذه التصفية.") + "</li>");
            h.AppendLine("</ol></aside></div></header></div>");
            return new { ok = true, html = h.ToString() };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd rp\"><header class=\"fxd-hero rp-hero\"><nav class=\"rp-crumbs\"><a href=\"" + Url(6) + "\">" + T("Report catalogue", "دليل التقارير") + "</a></nav><h1 class=\"fxd-h1\">" + T(s.TitleEn, s.TitleAr) + "</h1><p class=\"fxd-lead\">" + E(e.Message) + "</p></header></div>\n",
            };
        }
    }

    /// <summary>round(n): half away from zero.</summary>
    private static string Rnd(decimal n) => Math.Round(n, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>App 105 page 711 "Developer Exposure": the PL/SQL region "Report Summary".</summary>
public sealed class P0711ReportSummary(IApexSqlReader reader) : IApexProcess
{
    private static readonly ReportSummary.Spec Spec = new(
        "MIS & analytics", "التحليلات الإدارية",
        "Developer Exposure", "تعرض المطورين",
        "Outstanding exposure, purchase value and reach of every real-estate developer.", "التعرض القائم وقيمة الشراء وانتشار كل مطور عقاري.",
        [
            new("count", false, "Developers", "المطورون"),
            new("count", false, "Portfolios", "المحافظ"),
            new("count", false, "Investors", "المستثمرون"),
            new("money", false, "Face value", "القيمة الاسمية"),
            new("money", true, "Outstanding exposure", "التعرض القائم"),
        ],
        "fa-building", "Largest developer exposures", "أكبر تعرضات المطورين", true);

    public int App => 105;

    public int Page => 711;

    public string Name => "REGION_REPORT_SUMMARY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        ReportSummary.RenderAsync(reader, request, Spec, MtgApexPageSql.P711_KPIS, MtgApexPageSql.P711_TOP, ct);
}

/// <summary>App 105 page 730 "Portfolio Acquisition Register": the PL/SQL region "Report Summary".</summary>
public sealed class P0730ReportSummary(IApexSqlReader reader) : IApexProcess
{
    private static readonly ReportSummary.Spec Spec = new(
        "Pipeline & workflow", "خط المعالجة وسير العمل",
        "Portfolio Acquisition Register", "سجل شراء المحافظ",
        "Every portfolio case with its status, values, discount and settlement, for the filters you choose.", "كل ملفات المحافظ بحالتها وقيمها والخصم والتسوية حسب عوامل التصفية.",
        [
            new("count", false, "Cases", "الملفات"),
            new("money", false, "Face value", "القيمة الاسمية"),
            new("money", false, "Proposed price", "سعر الشراء المقترح"),
            new("pct1", true, "Average discount", "متوسط الخصم"),
            new("count", false, "Accounts", "الحسابات"),
        ],
        "fa-briefcase", "Cases by status", "الملفات حسب الحالة", false);

    public int App => 105;

    public int Page => 730;

    public string Name => "REGION_REPORT_SUMMARY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        ReportSummary.RenderAsync(reader, request, Spec, MtgApexPageSql.P730_KPIS, MtgApexPageSql.P730_TOP, ct);
}

/// <summary>App 105 page 731 "Portfolio Account & Contract Detail": the PL/SQL region "Report Summary".</summary>
public sealed class P0731ReportSummary(IApexSqlReader reader) : IApexProcess
{
    private static readonly ReportSummary.Spec Spec = new(
        "Accounts & collections", "الحسابات والتحصيل",
        "Portfolio Account & Contract Detail", "تفاصيل حسابات وعقود المحفظة",
        "Every investor contract with its unit, values, collection progress and arrears.", "كل عقود المستثمرين بوحداتها وقيمها ونسبة التحصيل والمتأخرات.",
        [
            new("count", false, "Accounts", "الحسابات"),
            new("money", false, "Contract value", "قيمة العقود"),
            new("pct1", true, "Collected", "نسبة التحصيل"),
            new("money", false, "Outstanding", "القائم"),
            new("count", false, "Accounts in arrears", "حسابات متأخرة"),
            new("money", false, "Arrears", "المتأخرات"),
        ],
        "fa-file-text-o", "Outstanding by portfolio", "القائم حسب المحفظة", true);

    public int App => 105;

    public int Page => 731;

    public string Name => "REGION_REPORT_SUMMARY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        ReportSummary.RenderAsync(reader, request, Spec, MtgApexPageSql.P731_KPIS, MtgApexPageSql.P731_TOP, ct);
}

/// <summary>App 105 page 732 "Investor Exposure & Concentration": the PL/SQL region "Report Summary".</summary>
public sealed class P0732ReportSummary(IApexSqlReader reader) : IApexProcess
{
    private static readonly ReportSummary.Spec Spec = new(
        "Accounts & collections", "الحسابات والتحصيل",
        "Investor Exposure & Concentration", "تعرض وتركز المستثمرين",
        "Exposure per investor across portfolios, and how concentrated the outstanding balance is.", "تعرض كل مستثمر عبر المحافظ ومدى تركز الرصيد القائم.",
        [
            new("count", false, "Investors", "المستثمرون"),
            new("money", false, "Outstanding", "القائم"),
            new("pct1", true, "Largest single investor", "أكبر مستثمر"),
            new("money", false, "Arrears", "المتأخرات"),
            new("count", false, "Investors in arrears", "مستثمرون متأخرون"),
        ],
        "fa-users", "Top investors by outstanding", "أكبر المستثمرين حسب القائم", true);

    public int App => 105;

    public int Page => 732;

    public string Name => "REGION_REPORT_SUMMARY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        ReportSummary.RenderAsync(reader, request, Spec, MtgApexPageSql.P732_KPIS, MtgApexPageSql.P732_TOP, ct);
}

/// <summary>App 105 page 748 "Status Movement & Pipeline History": the PL/SQL region "Report Summary".</summary>
public sealed class P0748ReportSummary(IApexSqlReader reader) : IApexProcess
{
    private static readonly ReportSummary.Spec Spec = new(
        "Pipeline & workflow", "خط المعالجة وسير العمل",
        "Status Movement & Pipeline History", "حركة الحالات وتاريخ خط المعالجة",
        "How cases moved between statuses, who moved them and how long they stayed in each status.", "كيف انتقلت الملفات بين الحالات ومن نقلها وكم بقيت في كل حالة.",
        [
            new("count", false, "Status changes", "تغييرات الحالة"),
            new("count", false, "Cases", "الملفات"),
            new("dec1", true, "Avg. days in a status", "متوسط الأيام في الحالة"),
            new("count", false, "Rejections", "الرفض"),
            new("count", false, "Users involved", "المستخدمون"),
        ],
        "fa-exchange", "Most common destination status", "أكثر الحالات انتقالاً إليها", false);

    public int App => 105;

    public int Page => 748;

    public string Name => "REGION_REPORT_SUMMARY";

    public string Permission => Permissions.Mtg.PortfolioView;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        ReportSummary.RenderAsync(reader, request, Spec, MtgApexPageSql.P748_KPIS, MtgApexPageSql.P748_TOP, ct);
}
