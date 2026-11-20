using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 203 "Portfolio Workspace", the PL/SQL region "Portfolio Details": the tabs of investors, accounts,
/// project and units, workflow and status history, and the portfolio's modules.
/// </summary>
public sealed class P0203PortfolioDetails(IApexSqlReader reader) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    // the columns of the project the generic list leaves out
    private static readonly string[] Skipped = ["PROJECT_NAME_EN", "PROJECT_NAME_AR", "CREATED_BY", "CREATED_ON", "UPDATED_BY", "UPDATED_ON", "ROW_VERSION_NO", "CREATED_DATE", "UPDATED_DATE", "CREATED_AT", "UPDATED_AT", "ROW_VERSION"];

    public int App => 105;

    public int Page => 203;

    public string Name => "REGION_PORTFOLIO_DETAILS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var pid = request.Item("P203_PORTFOLIO_ID");
        var p = pid is null ? null : await reader.RowAsync(MtgApexPageSql.P203_PORTFOLIO, items, ct);
        if (p is null)
            return new { ok = true, html = string.Empty };          // when no_data_found then null

        var cur = p.Text("CURRENCY_CODE") ?? "EGP";
        var a = await reader.RowAsync(MtgApexPageSql.P203_ACCOUNTS, items, ct) ?? new Dictionary<string, object?>();
        var x = await reader.RowAsync(MtgApexPageSql.P203_PROJECT_EXTRA, items, ct) ?? new Dictionary<string, object?>();
        var project = await reader.RowAsync(MtgApexPageSql.P203_PROJECT, items, ct);
        var tasks = await reader.QueryAsync(MtgApexPageSql.P203_TASKS, items, ct);
        var investors = await reader.QueryAsync(MtgApexPageSql.P203_INVESTORS, items, ct);
        var accounts = await reader.QueryAsync(MtgApexPageSql.P203_ACCOUNT_LIST, items, ct);
        var units = await reader.QueryAsync(MtgApexPageSql.P203_UNITS, items, ct);
        IReadOnlyList<IReadOnlyDictionary<string, object?>> hist;
        try
        {
            hist = await reader.QueryAsync(MtgApexPageSql.P203_STATUS_HIST, items, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            hist = [];
        }

        var accN = a.Number("ACC_N") ?? 0;
        var invN = a.Number("INV_N") ?? 0;
        var unitN = a.Number("UNIT_N") ?? 0;
        var taskId = tasks.Count > 0 ? tasks[^1].Number("TASK_ID") : null;
        string Num(decimal? n) => P0203Html.Num(n);
        string Fmt(decimal? n) => n is null ? "&ndash;" : ToChar(n, "FM999G999G999G990D00");

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"fxdPo\"><div class=\"fxd-wrap\" style=\"padding-bottom:32px\">");
        // ===== tabs
        h.AppendLine("<div class=\"po-tabs\" role=\"tablist\">"
            + "<button type=\"button\" role=\"tab\" aria-selected=\"true\" data-tab=\"inv\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("Investors", "المستثمرون") + "<em>" + Num(invN) + "</em></button>"
            + "<button type=\"button\" role=\"tab\" aria-selected=\"false\" data-tab=\"acc\"><span class=\"fa fa-file-text-o\" aria-hidden=\"true\"></span>" + T("Accounts", "الحسابات") + "<em>" + Num(accN) + "</em></button>"
            + "<button type=\"button\" role=\"tab\" aria-selected=\"false\" data-tab=\"prj\"><span class=\"fa fa-building\" aria-hidden=\"true\"></span>" + T("Project & units", "المشروع والوحدات") + "<em>" + Num(unitN) + "</em></button>"
            + "<button type=\"button\" role=\"tab\" aria-selected=\"false\" data-tab=\"wf\"><span class=\"fa fa-sitemap\" aria-hidden=\"true\"></span>" + T("Workflow & history", "سير العمل والسجل") + "<em>0</em></button></div>");          // l_tasks: this region never counts the tasks

        // investors
        h.AppendLine("<section class=\"fxd-card po-panel\" data-panel=\"inv\"><div class=\"po-tools\"><label class=\"po-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" data-q=\"inv\" placeholder=\"Search investor, code or national ID\" aria-label=\"Search investors\"></label>"
            + "<div class=\"po-seg\" data-seg=\"inv\"><button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "</button><button type=\"button\" data-f=\"SELECTED\" aria-pressed=\"false\">" + T("Selected", "مختار") + "</button>"
            + "<button type=\"button\" data-f=\"EXCLUDED\" aria-pressed=\"false\">" + T("Excluded", "مستبعد") + "</button></div></div><div class=\"po-tw\"><table class=\"po-t\"><thead><tr><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Contracts", "العقود") + "</th>"
            + "<th class=\"r\">" + T("Contract value", "قيمة العقود") + "</th><th class=\"r\">" + T("Outstanding", "القائم") + "</th><th>" + T("AML", "مكافحة غسل الأموال") + "</th><th>" + T("Deal decision", "قرار الصفقة") + "</th></tr></thead><tbody>");
        var n = 0;
        foreach (var r in investors)
        {
            n++;
            var dec = r.Text("DEC");
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE");
            var cv = r.Number("CV") ?? 0;
            h.AppendLine("<tr class=\"" + (dec == "EXCLUDED" ? "out" : string.Empty) + "\" data-f=\"" + (dec == "EXCLUDED" ? "EXCLUDED" : "SELECTED") + "\" data-q=\"" + Attr((code + " " + en + " " + ar + " " + r.Text("NATIONAL_ID")).ToLowerInvariant()) + "\">"
                + "<td><div class=\"po-who\"><span class=\"po-av\" style=\"background:" + Colors[(n - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small>" + E(ar) + "</small>" : string.Empty) + "<span class=\"po-code\">" + E(code) + "</span></div></div></td>"
                + "<td>" + Num(r.Number("CT")) + "</td><td class=\"r po-money\"><b>" + Money(cv) + "</b><small>" + (cv > 0 ? Num(P0203Html.Round((r.Number("PVV") ?? 0) / cv * 100)) + "% " + T("paid", "مسدد") : string.Empty) + "</small></td>"
                + "<td class=\"r po-money\"><b>" + Money(r.Number("OV")) + "</b></td><td>" + Pill(r.Text("AML")) + "</td><td>" + DecPill(dec) + "</td></tr>");
        }

        if (n == 0)
            h.AppendLine("<tr><td colspan=\"6\" class=\"po-empty\">" + T("No investors in this portfolio yet. Upload the accounts first.", "لا يوجد مستثمرون بعد. ارفع الحسابات أولاً.") + "</td></tr>");
        h.AppendLine("</tbody></table></div></section>");

        // accounts (the former page 204 list)
        h.AppendLine("<section class=\"fxd-card po-panel\" data-panel=\"acc\" hidden><div class=\"po-tools\"><label class=\"po-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" data-q=\"acc\" placeholder=\"Search investor, code or unit\" aria-label=\"Search accounts\"></label>"
            + "<span class=\"fxd-label\" style=\"margin-inline-start:auto\">" + Num(accN) + " " + T("accounts", "حساب") + "</span></div>"
            + "<div class=\"po-tw\"><table class=\"po-t\"><thead><tr><th>" + T("Investor code", "كود المستثمر") + "</th><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Unit", "الوحدة") + "</th><th>" + T("Contract date", "تاريخ العقد") + "</th>"
            + "<th class=\"r\">" + T("Contract value", "قيمة العقد") + "</th><th class=\"r\">" + T("Paid value", "المبلغ المسدد") + "</th><th class=\"r\">" + T("Outstanding", "القائم") + "</th><th class=\"r\">" + T("Installment", "القسط") + "</th>"
            + "<th class=\"r\">" + T("Regular months", "أشهر السداد المنتظم") + "</th><th class=\"r\">" + T("Arrears", "المتأخرات") + "</th><th>" + T("Account status", "حالة الحساب") + "</th></tr></thead><tbody>");
        n = 0;
        foreach (var r in accounts)
        {
            n++;
            var status = r.Text("ACCOUNT_STATUS");
            var arrears = r.Number("ARREARS_AMOUNT");
            var months = r.Number("REGULAR_PAYMENT_MONTHS");
            h.AppendLine("<tr data-f=\"ALL\" data-q=\"" + Attr((r.Text("INVESTOR_CODE") + " " + r.Text("INVESTOR_NAME") + " " + r.Text("UNIT_CODE") + " " + status).ToLowerInvariant()) + "\">"
                + "<td><span class=\"po-code\">" + P0203Html.Txt(r.Text("INVESTOR_CODE")) + "</span></td><td><strong style=\"font-weight:600\">" + E(r.Text("INVESTOR_NAME")) + "</strong></td><td>" + P0203Html.Txt(r.Text("UNIT_CODE")) + "</td>"
                + "<td style=\"white-space:nowrap\">" + (r.Date("CONTRACT_DATE")?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToUpperInvariant() ?? "&ndash;") + "</td>"
                + "<td class=\"r po-money\"><b>" + Fmt(r.Number("CONTRACT_VALUE")) + "</b></td><td class=\"r\">" + Fmt(r.Number("PAID_VALUE")) + "</td>"
                + "<td class=\"r\">" + Fmt(r.Number("OUTSTANDING_VALUE")) + "</td><td class=\"r\">" + Fmt(r.Number("INSTALLMENT_AMOUNT")) + "</td>"
                + "<td class=\"r\">" + (months is null ? "&ndash;" : Num(months)) + "</td>"
                + "<td class=\"r\"" + (arrears > 0 ? " style=\"color:var(--bad);font-weight:600\"" : string.Empty) + ">" + Fmt(arrears) + "</td>"
                + "<td>" + (status is null ? "&ndash;" : "<span class=\"fxd-pill " + (string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? "fxd-pill--ok" : "fxd-pill--mute") + "\">" + E(P0203Html.Nice(status)) + "</span>") + "</td></tr>");
        }

        if (n == 0)
            h.AppendLine("<tr><td colspan=\"11\" class=\"po-empty\">" + T("No accounts uploaded yet.", "لم يتم رفع حسابات بعد.") + "</td></tr>");
        h.AppendLine("</tbody></table></div></section>");

        // project & units
        var projUnits = x.Number("PROJ_UNITS");
        h.AppendLine("<div class=\"po-panel po-proj\" data-panel=\"prj\" hidden><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-building\" aria-hidden=\"true\"></span>" + T("Project", "المشروع") + "</h2></div><dl class=\"po-kv\">"
            + Kv(T("Project", "المشروع"), P0203Html.Txt(x.Text("PROJ"))) + (x.Text("PROJ_AR") is { } projAr ? Kv(T("Arabic name", "الاسم بالعربية"), "<span style=\"font-family:Cairo,sans-serif\">" + E(projAr) + "</span>") : string.Empty)
            + Kv(T("Developer", "المطور"), P0203Html.Txt(x.Text("DEV"))) + Kv(T("Developer code", "كود المطور"), P0203Html.Txt(x.Text("DEV_CODE")))
            + Kv(T("Units in project", "وحدات المشروع"), projUnits is null ? "&ndash;" : Num(projUnits)) + Kv(T("Units in this portfolio", "وحدات هذه المحفظة"), Num(unitN)));
        // every other filled-in project column, read generically (in the order of their names)
        foreach (var (k, v) in (project ?? new Dictionary<string, object?>()).OrderBy(c => c.Key, StringComparer.Ordinal))
        {
            if (v is null || k.EndsWith("_ID", StringComparison.Ordinal) || Skipped.Contains(k))
                continue;
            var shown = v switch
            {
                decimal or int or long or double or float or short => ToChar(ApexValue.Number(v), "FM999G999G999G990D99"),
                DateTime t => t.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                DateOnly d => d.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                _ => E(Cut(ApexValue.Text(v), 300)),
            };
            h.AppendLine(Kv(E(P0203Html.Nice(k)), shown));
        }

        h.AppendLine("</dl></section><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-th\" aria-hidden=\"true\"></span>" + T("Units in this portfolio", "وحدات هذه المحفظة") + "</h2><span class=\"fxd-label\">" + Num(unitN) + "</span></div>"
            + "<div class=\"po-tw\"><table class=\"po-t\"><thead><tr><th>" + T("Unit", "الوحدة") + "</th><th>" + T("Type", "النوع") + "</th><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Contract", "العقد") + "</th><th class=\"r\">" + T("Contract value", "قيمة العقد") + "</th><th>" + T("Decision", "القرار") + "</th></tr></thead><tbody>");
        n = 0;
        foreach (var u in units)
        {
            n++;
            var dec = u.Text("DEC");
            h.AppendLine("<tr class=\"" + (dec == "EXCLUDED" ? "out" : string.Empty) + "\"><td><span class=\"po-code\">" + E(u.Text("UNIT_CODE")) + "</span></td><td>" + P0203Html.Txt(P0203Html.Nice(u.Text("UNIT_TYPE_CODE"))) + "</td><td>" + E(u.Text("INV")) + "</td><td>" + P0203Html.Txt(u.Text("SELLER_CONTRACT_NO")) + "</td>"
                + "<td class=\"r po-money\"><b>" + Money(u.Number("CV")) + "</b></td><td>" + DecPill(dec) + "</td></tr>");
        }

        if (n == 0)
            h.AppendLine("<tr><td colspan=\"6\" class=\"po-empty\">" + T("No units linked yet.", "لا توجد وحدات مرتبطة بعد.") + "</td></tr>");
        h.AppendLine("</tbody></table></div></section></div>");

        // workflow & status history
        h.AppendLine("<div class=\"po-panel po-proj\" data-panel=\"wf\" hidden><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-sitemap\" aria-hidden=\"true\"></span>" + T("Workflow tasks", "مهام سير العمل") + "</h2></div><ul class=\"po-hist\">");
        foreach (var w in tasks.Reverse())
        {
            var st = w.Text("TASK_STATUS");
            var created = w.Stamp("CREATED_AT");
            h.AppendLine("<li class=\"" + (w.Number("TASK_ID") == taskId && !string.Equals(st, "COMPLETED", StringComparison.OrdinalIgnoreCase) ? "now" : string.Empty) + "\"><i></i><strong>" + E(w.Text("STEP_NAME") ?? P0203Html.Nice(w.Text("STEP_CODE"))) + "</strong> &middot; " + E(P0203Html.Nice(st))
                + "<small>" + T("Task", "مهمة") + " #" + Num(w.Number("TASK_ID")) + (created is { } c ? " &middot; " + c.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) : string.Empty) + "</small></li>");
        }

        if (tasks.Count == 0)
            h.AppendLine("<li style=\"padding-inline-start:0;color:var(--muted)\">" + T("The workflow has not started yet.", "لم يبدأ سير العمل بعد.") + "</li>");
        h.AppendLine("</ul></section><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-history\" aria-hidden=\"true\"></span>" + T("Status history", "سجل الحالات") + "</h2></div><ul class=\"po-hist\">");
        foreach (var s in hist)
        {
            var from = s.Text("FROM_STATUS");
            var comments = s.Text("COMMENTS");
            h.AppendLine("<li><i></i><strong>" + E(P0203Html.Nice(s.Text("TO_STATUS"))) + "</strong>" + (from is not null ? " <span style=\"color:var(--muted)\">" + T("from", "من") + " " + E(P0203Html.Nice(from)) + "</span>" : string.Empty)
                + "<small>" + s.Stamp("CHANGE_DATE")?.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " &middot; " + E(s.Text("CHANGED_BY")) + (comments is not null ? " &middot; " + E(Cut(comments, 200)) : string.Empty) + "</small></li>");
        }

        if (hist.Count == 0)
            h.AppendLine("<li style=\"padding-inline-start:0;color:var(--muted)\">" + T("No status changes yet.", "لا توجد تغييرات حالة بعد.") + "</li>");
        h.AppendLine("</ul></section></div>");

        // ===== modules
        string Modl(int page, string icon, string en, string ar, string sen, string sar, string? cls = null, string? request = null) =>
            "<a class=\"po-mod " + cls + "\" href=\"" + Url(page, "P" + page + "_PORTFOLIO_ID", pid, request) + "\"><span class=\"ic fa " + icon + "\" aria-hidden=\"true\"></span><span><b>" + T(en, ar) + "</b><small>" + T(sen, sar) + "</small></span></a>";
        h.AppendLine("<section class=\"fxd-card po-mods\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-th-large\" aria-hidden=\"true\"></span>" + T("Portfolio modules", "وحدات المحفظة") + "</h2></div>");
        h.AppendLine("<div class=\"po-mgrp\"><h3>" + T("Workflow results", "نتائج سير العمل") + "</h3><div class=\"po-mgrid\">"
            + Modl(208, "fa-upload", "Upload data", "رفع البيانات", "Loaded data & results (view only)", "البيانات المحملة والنتائج (عرض فقط)", "wf", "VIEW")
            + Modl(209, "fa-shield", "AML review", "مراجعة مكافحة غسل الأموال", "Screening results (view only)", "نتائج الفحص (عرض فقط)", "wf", "VIEW")
            + Modl(210, "fa-comments-o", "Inquiries", "الاستعلامات", "Inquiry results (view only)", "نتائج الاستعلامات (عرض فقط)", "wf", "VIEW")
            + Modl(211, "fa-gavel", "Legal review", "المراجعة القانونية", "Legal checklist (view only)", "القائمة القانونية (عرض فقط)", "wf", "VIEW")
            + Modl(401, "fa-line-chart", "Valuation", "التقييم", "Portfolio valuation (view only)", "تقييم المحفظة (عرض فقط)", "wf", "VIEW")
            + Modl(212, "fa-pie-chart", "Coverage review", "مراجعة التغطية", "Investor coverage (view only)", "تغطية المستثمرين (عرض فقط)", "wf", "VIEW")
            + Modl(213, "fa-money", "Cheque coverage", "تغطية الشيكات", "Investor cheques (view only)", "شيكات المستثمرين (عرض فقط)", "wf", "VIEW")
            + Modl(214, "fa-calculator", "Financial review", "المراجعة المالية", "Present value and price (view only)", "القيمة الحالية والسعر (عرض فقط)", "wf", "VIEW")
            + Modl(215, "fa-university", "Board review", "مراجعة المجلس", "Board memorandum and decision (view only)", "مذكرة المجلس والقرار (عرض فقط)", "wf", "VIEW")
            + Modl(217, "fa-money", "Disbursement", "الصرف", "Payment, schedules and GL (view only)", "الدفع والجداول والقيود (عرض فقط)", "wf", "VIEW") + "</div></div>");
        h.AppendLine("<div class=\"po-mgrp\" style=\"padding-top:0\"><h3>" + T("Portfolio", "المحفظة") + "</h3><div class=\"po-mgrid\">"
            + Modl(301, "fa-file-text-o", "Documents", "المستندات", "Portfolio documents", "مستندات المحفظة")
            + Modl(206, "fa-history", "Payment history", "سجل السداد", "Instalment payments", "سداد الأقساط")
            + Modl(503, "fa-umbrella", "Insurance", "التأمين", "Insurance cover", "التغطية التأمينية")
            + Modl(606, "fa-clock-o", "Approval history", "سجل الاعتمادات", "Approval decisions", "قرارات الاعتماد")
            + Modl(603, "fa-paper-plane", "Submit for approval", "إرسال للاعتماد", "Send to committee", "إرسال إلى اللجنة", "primary")
            + "</div></div></section></div>");

        h.AppendLine("<script>" + Script + "</script>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Kv(string label, string value) => "<dt>" + label + "</dt><dd>" + value + "</dd>";

    private static string Cut(string? s, int n) => s is null ? string.Empty : s.Length > n ? s[..n] : s;

    private static string Pill(string? code) => code switch
    {
        "CLEAR" => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Clear", "سليم") + "</span>",
        "POTENTIAL_MATCH" => "<span class=\"fxd-pill fxd-pill--warn\">" + T("Potential match", "تطابق محتمل") + "</span>",
        "REVIEW_REQUIRED" => "<span class=\"fxd-pill fxd-pill--warn\">" + T("Review required", "يتطلب مراجعة") + "</span>",
        "CONFIRMED_MATCH" => "<span class=\"fxd-pill fxd-pill--bad\">" + T("Confirmed match", "تطابق مؤكد") + "</span>",
        _ => "<span class=\"fxd-pill fxd-pill--mute\">" + T("Not screened", "لم يتم الفحص") + "</span>",
    };

    private static string DecPill(string? p) => p switch
    {
        "EXCLUDED" => "<span class=\"fxd-pill fxd-pill--bad\">" + T("Excluded", "مستبعد") + "</span>",
        "MIXED" => "<span class=\"fxd-pill fxd-pill--warn\">" + T("Partly excluded", "مستبعد جزئياً") + "</span>",
        _ => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Selected", "مختار") + "</span>",
    };

    // the tabs, the segment filters and the searches of the region
    private const string Script = """
        (function(){
        var root=document.getElementById('fxdPo'); if(!root)return;
        var tabs=[].slice.call(root.querySelectorAll('.po-tabs [data-tab]')), panels=[].slice.call(root.querySelectorAll('[data-panel]')), filt={inv:'ALL',acc:'ALL'};
        function show(k){tabs.forEach(function(b){b.setAttribute('aria-selected',b.getAttribute('data-tab')===k?'true':'false');});panels.forEach(function(p){p.hidden=p.getAttribute('data-panel')!==k;});
          try{sessionStorage.setItem('fxdPoTab',k);}catch(e){}}
        function apply(k){var p=root.querySelector('[data-panel='+k+']');if(!p)return;var q=(p.querySelector('[data-q]').value||'').trim().toLowerCase();
          [].forEach.call(p.querySelectorAll('tbody tr[data-f]'),function(tr){var ok=(filt[k]==='ALL'||tr.getAttribute('data-f')===filt[k])&&(!q||(tr.getAttribute('data-q')||'').indexOf(q)>=0);tr.hidden=!ok;});}
        root.addEventListener('click',function(e){var b=e.target.closest('.po-tabs [data-tab]');if(b){show(b.getAttribute('data-tab'));return;}
          var s=e.target.closest('[data-seg] [data-f]');if(s){var k=s.parentNode.getAttribute('data-seg');filt[k]=s.getAttribute('data-f');
            [].forEach.call(s.parentNode.querySelectorAll('[data-f]'),function(x){x.setAttribute('aria-pressed',x===s?'true':'false');});apply(k);}});
        [].forEach.call(root.querySelectorAll('[data-q]'),function(i){i.addEventListener('input',function(){apply(i.getAttribute('data-q'));});});
        try{var t0=sessionStorage.getItem('fxdPoTab');if(t0&&root.querySelector('[data-tab='+t0+']'))show(t0);}catch(e){}
        })();
        """;
}
