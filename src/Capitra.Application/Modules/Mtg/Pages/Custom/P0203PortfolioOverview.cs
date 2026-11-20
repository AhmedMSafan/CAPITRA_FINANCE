using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 203 "Portfolio Workspace", the PL/SQL region "Portfolio Overview": the hero with the workflow trail,
/// the value and selection bands, pricing, decision and readiness. The workflow tasks come from the Capitra engine.
/// </summary>
public sealed class P0203PortfolioOverview(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 203;

    public string Name => "REGION_PORTFOLIO_OVERVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var p = request.Item("P203_PORTFOLIO_ID") is null ? null : await reader.RowAsync(MtgApexPageSql.P203_PORTFOLIO, items, ct);
        if (p is null)
            return new { ok = true, html = NotFound() };

        var a = await reader.RowAsync(MtgApexPageSql.P203_ACCOUNTS, items, ct) ?? new Dictionary<string, object?>();
        var tasks = await reader.QueryAsync(MtgApexPageSql.P203_TASKS, items, ct);

        var no = p.Text("PORTFOLIO_NO");
        var cur = p.Text("CURRENCY_CODE") ?? "EGP";
        var statusName = p.Text("STATUS_NAME") ?? P0203Html.Nice(p.Text("PORTFOLIO_STATUS_CODE"));
        decimal accN = a.Number("ACC_N") ?? 0, cv = a.Number("CV") ?? 0, ov = a.Number("OV") ?? 0;
        decimal inN = a.Number("IN_N") ?? 0, inCv = a.Number("IN_CV") ?? 0, inOv = a.Number("IN_OV") ?? 0, inPv = a.Number("IN_PV") ?? 0;
        decimal exN = a.Number("EX_N") ?? 0, invN = a.Number("INV_N") ?? 0;
        var arr = a.Number("ARR");
        var face = p.Number("FACE_VALUE") ?? cv;
        var outstanding = p.Number("OUTSTANDING_VALUE") ?? ov;

        // stored selected balances (set by the workflow); before they exist (no count and no face value yet), the live figures
        decimal? selN = p.Number("SELECTED_ACCOUNT_COUNT"), selCv = p.Number("SELECTED_FACE_VALUE"), selOv = p.Number("SELECTED_OUTSTANDING_VALUE"), selPv = p.Number("SELECTED_PAID_VALUE");
        var live = (selN ?? 0) == 0 && (selCv ?? 0) == 0;
        if (live)
        {
            selN = inN;
            selCv = inCv;
            selOv = inOv;
            selPv = inPv;
        }

        // current workflow step = latest task of the portfolio's workflow
        var last = tasks.Count > 0 ? tasks[^1] : null;
        var taskId = last?.Number("TASK_ID");
        var taskStatus = last?.Text("TASK_STATUS");
        var stepCode = last?.Text("STEP_CODE");
        var stepName = last?.Text("STEP_NAME");
        var taskDate = last?.Date("CREATED_AT");
        var proposed = p.Number("PROPOSED_PURCHASE_VALUE");
        string Amt(decimal? n) => n is null ? "&ndash;" : Money(n);
        string? FullAmt(decimal? n) => n is null ? null : ToChar(n, "FM999G999G999G999G990D00") + " " + E(cur);

        var h = new StringBuilder();
        // ===== hero
        h.AppendLine("<div class=\"fxd\" id=\"fxdPoHead\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"po-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Url(201) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Portfolio cases", "ملفات المحافظ") + "</a><span>/</span><span>" + E(no) + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased portfolio", "محفظة مشتراة") + "</div><h1 class=\"fxd-h1\">" + E(no) + "</h1>"
            + "<div class=\"po-sub\"><span><span class=\"fa fa-building-o\" aria-hidden=\"true\"></span>" + P0203Html.Txt(p.Text("DEV")) + "</span><span><span class=\"fa fa-map-marker\" aria-hidden=\"true\"></span>" + P0203Html.Txt(p.Text("PROJ")) + "</span>"
            + "<span><span class=\"fa fa-calendar\" aria-hidden=\"true\"></span>" + T("Requested", "تاريخ الطلب") + " " + P0203Html.Txt(P0203Html.Day(p, "REQUEST_DATE")) + "</span><span><span class=\"fa fa-money\" aria-hidden=\"true\"></span>" + E(cur) + "</span></div></div>");
        h.AppendLine("<div class=\"po-wf fxd-anim d1\"><small>" + T("Status", "الحالة") + "</small><span class=\"po-stat\">" + E(statusName) + "</span>"
            + "<small style=\"margin-top:14px\">" + T("Workflow step", "خطوة سير العمل") + "</small><b>" + P0203Html.Txt(stepName ?? P0203Html.Nice(stepCode)) + "</b>"
            + "<div class=\"row\">" + (taskStatus is not null ? "<span class=\"fxd-pill fxd-pill--glass\">" + E(P0203Html.Nice(taskStatus)) + "</span>" : string.Empty)
            + (taskDate is { } td ? "<span>" + T("since", "منذ") + " " + td.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + "</span>" : string.Empty)
            + (taskId is not null ? "<span>" + T("Task", "مهمة") + " #" + P0203Html.Num(taskId) + "</span>" : string.Empty) + "</div></div></div>");
        // workflow trail
        h.AppendLine("<div class=\"po-trail\">");
        var n = 0;
        foreach (var w in tasks)
        {
            n++;
            var st = (w.Text("TASK_STATUS") ?? string.Empty).ToUpperInvariant();
            var done = st is "COMPLETED" or "APPROVED";
            var cls = w.Number("TASK_ID") == taskId && st != "COMPLETED" ? "now" : done ? "done" : string.Empty;
            h.AppendLine("<span class=\"po-tstep " + cls + "\"><i>" + (done ? "&#10003;" : n.ToString(CultureInfo.InvariantCulture)) + "</i>" + E(w.Text("STEP_NAME") ?? P0203Html.Nice(w.Text("STEP_CODE"))) + "</span>");
        }

        h.AppendLine("</div></header>");

        // ===== financial band
        h.AppendLine("<div class=\"fxd-wrap\"><div class=\"po-band\">");
        h.AppendLine("<section class=\"fxd-card fxd-anim d1\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-briefcase\" aria-hidden=\"true\"></span>" + T("Portfolio value", "قيمة المحفظة") + "</h2><span class=\"fxd-label\">" + E(cur) + "</span></div><div class=\"po-figs\">"
            + Fig(T("Face value", "القيمة الاسمية"), Amt(face), P0203Html.Num(accN) + " " + T("contracts", "عقد"), FullAmt(face))
            + Fig(T("Outstanding", "القائم"), Amt(outstanding), face > 0 ? P0203Html.Num(P0203Html.Round(outstanding / face * 100)) + "% " + T("of face value", "من القيمة الاسمية") : "&nbsp;", FullAmt(outstanding))
            + Fig(T("Proposed purchase", "قيمة الشراء المقترحة"), Amt(proposed), outstanding > 0 && proposed > 0 ? P0203Html.Num(P0203Html.Round(proposed!.Value / outstanding * 100, 1)) + "% " + T("of outstanding", "من القائم") : "&nbsp;", FullAmt(proposed))
            + Fig(T("Expected return", "العائد المتوقع"), Amt(p.Number("EXPECTED_RETURN_AMOUNT")), arr > 0 ? "<span style=\"color:var(--bad)\">" + Money(arr) + " " + T("in arrears", "متأخرات") + "</span>" : "&nbsp;", FullAmt(p.Number("EXPECTED_RETURN_AMOUNT")))
            + "</div></section>");
        var keptPct = face > 0 ? Math.Min(100, P0203Html.Round((selCv ?? 0) / face * 100, 1)) : 0;
        h.AppendLine("<section class=\"fxd-card po-sel fxd-anim d2\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("After exclusions", "بعد الاستبعاد") + "</h2>"
            + (live ? "<span class=\"fxd-pill fxd-pill--info\">" + T("Live", "مباشر") + "</span>" : "<span class=\"fxd-label\">" + T("Selected balances", "الأرصدة المختارة") + "</span>") + "</div><div class=\"po-figs\">"
            + Fig(T("Selected contracts", "العقود المختارة"), (selN is null ? "&ndash;" : P0203Html.Num(selN)) + "<span style=\"font-size:14px;color:var(--muted)\"> / " + P0203Html.Num(accN) + "</span>", exN > 0 ? "<span class=\"po-delta\">" + P0203Html.Num(exN) + " " + T("excluded", "مستبعد") + "</span>" : "<span class=\"po-delta zero\">" + T("none excluded", "لا يوجد استبعاد") + "</span>")
            + Fig(T("Selected face value", "القيمة الاسمية المختارة"), Amt(selCv), selCv < face ? "<span class=\"po-delta\">-" + Money(face - selCv) + "</span>" : "<span class=\"po-delta zero\">" + T("unchanged", "بدون تغيير") + "</span>", FullAmt(selCv))
            + Fig(T("Selected outstanding", "القائم المختار"), Amt(selOv), selOv < outstanding ? "<span class=\"po-delta\">-" + Money(outstanding - selOv) + "</span>" : "<span class=\"po-delta zero\">" + T("unchanged", "بدون تغيير") + "</span>", FullAmt(selOv))
            + Fig(T("Selected paid", "المسدد المختار"), Amt(selPv), selCv > 0 ? P0203Html.Num(P0203Html.Round((selPv ?? 0) / selCv.Value * 100)) + "% " + T("paid", "مسدد") : "&nbsp;", FullAmt(selPv))
            + "</div><div class=\"po-meter\" role=\"img\" aria-label=\"Selected share of face value\"><i style=\"width:" + P0203Html.Num(keptPct) + "%;background:var(--teal-600)\"></i>"
            + "<i style=\"flex:1;background:" + (exN > 0 ? "var(--bad)" : "#EDF2F4") + "\"></i></div><div class=\"po-meter-l\"><span>" + P0203Html.Num(face > 0 ? P0203Html.Round((selCv ?? 0) / face * 100, 1) : 0) + "% " + T("of face value kept", "من القيمة الاسمية باقٍ") + "</span><span>"
            + P0203Html.Num(invN) + " " + T("investors", "مستثمر") + "</span></div></section></div>");

        // ===== pricing / decision / readiness
        var term = p.Number("TERM_MONTHS");
        h.AppendLine("<div class=\"po-grid3\"><section class=\"fxd-card fxd-anim d2\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-percent\" aria-hidden=\"true\"></span>" + T("Pricing & terms", "التسعير والشروط") + "</h2></div>"
            + "<div style=\"display:flex;justify-content:flex-end;padding:10px 20px 0\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" id=\"poTermsEdit\" style=\"height:34px;font-size:13px\"><span class=\"fa fa-pencil\" aria-hidden=\"true\"></span>" + T("Edit rates & fees", "تعديل النسب والرسوم") + "</button></div><dl class=\"po-kv\">"
            + Kv(T("Currency", "العملة"), E(cur)) + Kv(T("Late fee rate", "نسبة غرامة التأخير"), P0203Html.Pct(p.Number("LATE_FEE_RATE"))) + Kv(T("PV rate", "معدل القيمة الحالية"), P0203Html.Pct(p.Number("PV_RATE")))
            + Kv(T("Reserved amount", "نسبة المبلغ المحتجز"), P0203Html.Pct(p.Number("RESERVED_AMOUNT_PERCENT"))) + Kv(T("Term", "المدة"), term is not null ? P0203Html.Num(term) + " " + T("months", "شهر") : "&ndash;")
            + Kv(T("Coverage", "نسبة التغطية"), P0203Html.Pct(p.Number("COVERAGE_PCT"))) + Kv(T("Margin", "الهامش"), P0203Html.Pct(p.Number("MARGIN_PERCENTAGE")))
            + Kv(T("Admin expense", "المصروفات الإدارية"), P0203Html.Pct(p.Number("ADMIN_EXPENSE_PERCENT")))
            + Kv(T("Legal fee / unit", "الرسوم القانونية للوحدة"), FullAmt(p.Number("LEGAL_FEE_PER_UNIT")) ?? "&ndash;")
            + Kv(T("Valuation fee / unit", "مصروف التقييم للوحدة"), FullAmt(p.Number("VALUATION_FEE_PER_UNIT")) ?? "&ndash;")
            + "</dl></section>");
        var board = p.Text("BOARD_DECISION");
        var boardUp = (board ?? string.Empty).ToUpperInvariant();
        h.AppendLine("<section class=\"fxd-card fxd-anim d3\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-gavel\" aria-hidden=\"true\"></span>" + T("Decision & dates", "القرار والتواريخ") + "</h2>"
            + (board is not null ? "<span class=\"fxd-pill " + (boardUp.StartsWith("APPROV", StringComparison.Ordinal) || boardUp.StartsWith("ACCEPT", StringComparison.Ordinal) ? "fxd-pill--ok" : boardUp.StartsWith("REJECT", StringComparison.Ordinal) ? "fxd-pill--bad" : "fxd-pill--info") + "\">" + E(P0203Html.Nice(board)) + "</span>" : string.Empty)
            + "</div><dl class=\"po-kv\">"
            + Kv(T("Submitted", "تاريخ الإرسال"), P0203Html.Txt(P0203Html.Day(p, "SUBMITTED_DATE"))) + Kv(T("Approved", "تاريخ الاعتماد"), P0203Html.Txt(P0203Html.Day(p, "APPROVED_DATE")))
            + Kv(T("Board decision no.", "رقم قرار المجلس"), P0203Html.Txt(p.Text("BOARD_DECISION_NO"))) + Kv(T("Board decision date", "تاريخ قرار المجلس"), P0203Html.Txt(P0203Html.Day(p, "BOARD_DECISION_DATE"))) + "</dl>"
            + (p.Text("REJECTION_REASON") is { } rr ? "<div class=\"po-note bad\"><b>" + T("Rejection reason", "سبب الرفض") + "</b>" + E(rr) + "</div>" : string.Empty)
            + (p.Text("COMMENTS") is { } cm ? "<div class=\"po-note\"><b>" + T("Comments", "ملاحظات") + "</b>" + E(cm) + "</div>" : string.Empty) + "</section>");
        h.AppendLine("<section class=\"fxd-card fxd-anim d4\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check-square-o\" aria-hidden=\"true\"></span>" + T("Readiness", "الجاهزية") + "</h2>"
            + (string.Equals(request.Item("P203_OVERALL_READY"), "READY", StringComparison.OrdinalIgnoreCase) ? "<span class=\"fxd-pill fxd-pill--ok\">" + T("Ready", "جاهزة") + "</span>" : "<span class=\"fxd-pill fxd-pill--warn\">" + T("Not ready", "غير جاهزة") + "</span>") + "</div><div class=\"po-ready\">"
            + Rdy(T("Documents", "المستندات"), request.Item("P203_DOC_READY")) + Rdy(T("Payment history", "سجل السداد"), request.Item("P203_PAY_READY")) + Rdy(T("Valuation", "التقييم"), request.Item("P203_VAL_READY")) + Rdy(T("Legal", "القانونية"), request.Item("P203_LEGAL_READY"))
            + Rdy(T("Financial study", "الدراسة المالية"), request.Item("P203_FIN_READY")) + Rdy(T("Cheques", "الشيكات"), request.Item("P203_CHQ_READY")) + Rdy(T("Due diligence", "العناية الواجبة"), request.Item("P203_DD_READY")) + Rdy(T("Guarantees", "الضمانات"), request.Item("P203_GUA_READY"))
            + "</div><dl class=\"po-kv\" style=\"padding-top:0\">" + Kv(T("Case age", "عمر الملف"), (request.Item("P203_AGE_DAYS") ?? "0") + " " + T("days", "يوم")) + Kv(T("Open exceptions", "الاستثناءات المفتوحة"), request.Item("P203_EXCEPTION_COUNT") ?? "0") + "</dl></section></div>");

        h.AppendLine("</div></div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string NotFound() =>
        "<div class=\"fxd\"><div class=\"fxd-card\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
        + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open a portfolio from the portfolio cases list.", "افتح محفظة من قائمة ملفات المحافظ.") + "</p></div></div>\n";

    private static string Fig(string label, string value, string sub, string? title = null) =>
        "<div class=\"po-fig\"" + (title is not null ? " title=\"" + Attr(title) + "\"" : string.Empty) + "><span class=\"fxd-label\">" + label + "</span><div class=\"v\">" + value + "</div><div class=\"s\">" + sub + "</div></div>";

    private static string Kv(string label, string value) => "<dt>" + label + "</dt><dd>" + value + "</dd>";

    private static string Rdy(string label, string? flag)
    {
        var ok = (flag ?? string.Empty).ToUpperInvariant().StartsWith("READY", StringComparison.Ordinal);
        return "<span class=\"" + (ok ? "ok" : string.Empty) + "\"><span class=\"fa " + (ok ? "fa-check" : "fa-circle-o") + "\" aria-hidden=\"true\"></span>" + label + "</span>";
    }
}

/// <summary>The small functions the regions of page 203 declare.</summary>
internal static class P0203Html
{
    /// <summary>num(p) / to_char(p,'TM9').</summary>
    public static string Num(decimal? n)
    {
        // Oracle writes no zero before the point: .5, -.25
        var s = (n ?? 0).ToString("0.############", CultureInfo.InvariantCulture);
        return s.StartsWith("0.", StringComparison.Ordinal) ? s[1..] : s.StartsWith("-0.", StringComparison.Ordinal) ? "-" + s[2..] : s;
    }

    /// <summary>round(n[, d]) as Oracle rounds: half away from zero.</summary>
    public static decimal Round(decimal n, int d = 0) => Math.Round(n, d, MidpointRounding.AwayFromZero);

    /// <summary>nice(p): initcap of the code with spaces, AML kept.</summary>
    public static string Nice(string? p) =>
        p is null ? string.Empty
        : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p.ToLowerInvariant().Replace('_', ' ')).Replace("Aml", "AML", StringComparison.Ordinal);

    /// <summary>txt(p): the escaped text, &amp;ndash; when null.</summary>
    public static string Txt(string? p) => string.IsNullOrEmpty(p) ? "&ndash;" : ApexHtml.E(p);

    /// <summary>pct(p): FM999G990D00%, &amp;ndash; when null.</summary>
    public static string Pct(decimal? p) => p is null ? "&ndash;" : ApexHtml.ToChar(p, "FM999G990D00") + "%";

    /// <summary>A date column as load_row printed it: DD Mon YYYY.</summary>
    public static string? Day(IReadOnlyDictionary<string, object?> row, string column) =>
        row.Date(column)?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
}
