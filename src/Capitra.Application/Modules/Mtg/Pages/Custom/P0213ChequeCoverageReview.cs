using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 213 "Portfolio Cheque Coverage Review": the PL/SQL region "Cheque Coverage Review", ported.</summary>
public sealed class P0213ChequeCoverageReview(IApexSqlReader reader) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    public int App => 105;

    public int Page => 213;

    public string Name => "REGION_CHEQUE_COVERAGE_REVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P213_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the Cheque Coverage Review task in your inbox.", "افتح هذه الصفحة من مهمة مراجعة تغطية الشيكات في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P213_PORTFOLIO_ID");
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var cur = head.Text("CURRENCY_CODE");
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P213_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);

        // cheques that do not count for an included investor (portfolio level or excluded investors)
        var totals = await reader.RowAsync(MtgApexPageSql.P213_TOTALS, items, ct);
        var other = (totals?.Number("ALL_AMT") ?? 0) - (totals?.Number("INV_AMT") ?? 0);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,#chAdd,.ch-act,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"chRoot\" data-inbox=\"" + Attr(inboxUrl) + "\" data-pf=\"" + portfolioId + "\" data-cur=\"" + E(cur) + "\" data-other=\"" + P0203Html.Num(other) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Cheque coverage review", "مراجعة تغطية الشيكات") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Cheque coverage review", "مراجعة تغطية الشيكات") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Select an investor, record the cheques he provided, and make sure they cover his outstanding balance and the whole portfolio.", "اختر المستثمر وسجّل الشيكات التي قدّمها، وتأكد أنها تغطي رصيده القائم وتغطي المحفظة كاملة.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Currency", "العملة") + "</span><strong>" + E(cur) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Select investor", "اختيار المستثمر") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Enter cheques", "إدخال الشيكات") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Cover every investor", "تغطية كل مستثمر") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Accept or reject", "القبول أو الرفض") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\">");

        // portfolio coverage
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-pie-chart\" aria-hidden=\"true\"></span>" + T("Portfolio cheque coverage", "تغطية الشيكات للمحفظة") + "</h2><span id=\"chPfPill\"></span></div>"
            + "<div class=\"ch-pf\"><div class=\"ch-ring\" id=\"chRing\"><span><span><b id=\"chPct\">0%</b>" + T("covered", "مغطى") + "</span></span></div>"
            + "<div class=\"ch-stats\"><div><span>" + T("Outstanding", "الرصيد القائم") + "</span><b id=\"chOut\">-</b><small>" + T("all investors in the deal", "كل المستثمرين في الصفقة") + "</small></div>"
            + "<div><span>" + T("Cheques", "الشيكات") + "</span><b id=\"chChq\">-</b><small id=\"chChqN\"></small></div>"
            + "<div><span id=\"chGapL\">" + T("Remaining", "المتبقي") + "</span><b id=\"chGap\">-</b><small>" + T("to reach full coverage", "للوصول إلى تغطية كاملة") + "</small></div>"
            + "<div><span>" + T("Investors covered", "المستثمرون المغطّون") + "</span><b id=\"chInvOk\">-</b><small id=\"chInvShort\"></small></div>"
            + "<div class=\"ch-bar\" id=\"chBar\"><i></i><b></b></div></div>"
            + "<div class=\"ch-note\" id=\"chOther\" hidden style=\"margin:0 22px 18px\"><span class=\"fa fa-info-circle\" aria-hidden=\"true\"></span><span></span></div></div></section>");

        // investors
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("Investors", "المستثمرون") + "</h2><span class=\"fxd-label\" id=\"chCount\"></span></div>");
        h.AppendLine("<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wbQ\" placeholder=\"Search investor, code or contract\" aria-label=\"Search investors\"></label>"
            + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Filter\" id=\"wbSeg\">"
            + "<button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"SHORT\" aria-pressed=\"false\">" + T("Not covered", "غير مغطى") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"OK\" aria-pressed=\"false\">" + T("Covered", "مغطى") + "<em></em></button></div></div><ul class=\"wb-rows\" id=\"wbRows\">");
        var n = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P213_INVESTORS, items, ct))
        {
            n++;
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE");
            var count = r.Number("N") ?? 0;
            var keys = (code + " " + en + " " + ar + " " + r.Text("KEYS")).ToLowerInvariant();
            var color = Colors[(n - 1) % 8];
            h.AppendLine("<li class=\"wb-row ch\" tabindex=\"0\" aria-selected=\"false\" data-inv=\"" + P0203Html.Num(r.Number("INVESTOR_ID")) + "\" data-out=\"" + P0203Html.Num(r.Number("OV") ?? 0) + "\" data-av=\"" + color + "\""
                + " data-name=\"" + Attr(en ?? ar) + "\" data-code=\"" + Attr(code) + "\""
                + " data-q=\"" + Attr(keys.Length > 1500 ? keys[..1500] : keys) + "\">"
                + "<div class=\"wb-person\"><span class=\"wb-av\" style=\"background:" + color + "\">" + E(Initials(en ?? code)) + "</span><div class=\"wb-who\"><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small style=\"font-family:Cairo,sans-serif;unicode-bidi:plaintext;text-align:start\">" + E(ar) + "</small>" : string.Empty)
                + "<div class=\"wb-codes\"><span class=\"wb-code\">" + E(code) + "</span><span class=\"wb-code\">" + P0203Html.Num(count) + " " + (count == 1 ? T("contract", "عقد") : T("contracts", "عقود")) + "</span></div></div></div>"
                + "<div class=\"ch-cov\"></div><div class=\"ch-st\"></div><button type=\"button\" class=\"wb-link ch-open\"><span class=\"fa fa-money\" aria-hidden=\"true\"></span><span class=\"ch-nc\"></span></button></li>");
        }

        if (n == 0)
            h.AppendLine("<li class=\"wb-empty\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("No investors are included in this deal.", "لا يوجد مستثمرون مدرجون في هذه الصفقة.") + "</li>");
        h.AppendLine("</ul><div class=\"wb-empty\" id=\"wbNoMatch\" hidden>" + T("No investors match this filter.", "لا يوجد مستثمرون مطابقون.") + "</div></section>");

        // selected investor: cheques
        h.AppendLine("<section class=\"fxd-card\" id=\"chWs\"><div class=\"ch-pick\" id=\"chPick\"><span class=\"fa fa-hand-pointer-o\" aria-hidden=\"true\"></span>" + T("Select an investor above to see his outstanding balance and enter his cheques.", "اختر مستثمراً من الأعلى لعرض رصيده القائم وإدخال شيكاته.") + "</div>"
            + "<div id=\"chWsBody\" hidden><div class=\"ch-ws-h\"><span class=\"wb-av\" id=\"chWsAv\"></span><div class=\"wb-who\"><strong id=\"chWsName\"></strong><small id=\"chWsCode\"></small></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"chAdd\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("Add cheque", "إضافة شيك") + "</button></div>"
            + "<div class=\"ch-trio\"><div><span>" + T("Outstanding to cover", "الرصيد المطلوب تغطيته") + "</span><b id=\"chWsOut\"></b></div><div><span>" + T("Cheques entered", "الشيكات المدخلة") + "</span><b id=\"chWsChq\"></b></div>"
            + "<div><span id=\"chWsGapL\"></span><b id=\"chWsGap\"></b></div></div>"
            + "<div class=\"wb-tbl-wrap\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Cheque no.", "رقم الشيك") + "</th><th>" + T("Contract", "العقد") + "</th><th>" + T("Bank", "البنك") + "</th><th>" + T("Due date", "تاريخ الاستحقاق") + "</th>"
            + "<th class=\"fxd-num\">" + T("Amount", "المبلغ") + "</th><th>" + T("Endorsed", "مظهّر") + "</th><th>" + T("Status", "الحالة") + "</th><th></th></tr></thead><tbody id=\"chTb\">");
        foreach (var c in await reader.QueryAsync(MtgApexPageSql.P213_CHEQUES, items, ct))
        {
            var st = c.Text("CHEQUE_STATUS_CODE");
            var unit = c.Text("UNIT_CODE");
            var due = c.Date("DUE_DATE");
            h.AppendLine("<tr data-inv=\"" + P0203Html.Num(c.Number("INVESTOR_ID")) + "\" data-id=\"" + P0203Html.Num(c.Number("CHEQUE_ID")) + "\" data-acc=\"" + P0203Html.Num(c.Number("PORTFOLIO_ACCOUNT_ID")) + "\" data-amt=\"" + P0203Html.Num(c.Number("CHEQUE_AMOUNT") ?? 0) + "\" data-no=\"" + Attr(c.Text("CHEQUE_NO")) + "\""
                + " data-bank=\"" + Attr(c.Text("BANK_NAME")) + "\" data-cd=\"" + Dt(c.Date("CHEQUE_DATE")) + "\" data-dd=\"" + Dt(due) + "\" data-end=\"" + c.Text("ENDORSED") + "\" data-st=\"" + E(st) + "\""
                + " data-clr=\"" + Dt(c.Date("CLEARING_DATE")) + "\" data-rr=\"" + Attr(c.Text("RETURN_REASON")) + "\" hidden>"
                + "<td><span class=\"wb-code\">" + E(c.Text("CHEQUE_NO")) + "</span></td><td>" + E(c.Text("SELLER_CONTRACT_NO") ?? "-") + (unit is not null ? "<small>" + E(unit) + "</small>" : string.Empty) + "</td>"
                + "<td>" + E(c.Text("BANK_NAME") ?? "-") + "</td><td>" + E(due?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "-") + "</td><td class=\"fxd-num\"><b>" + Amt(c.Number("CHEQUE_AMOUNT")) + "</b></td>"
                + "<td>" + (c.Text("ENDORSED") == "Y" ? T("Yes", "نعم") : T("No", "لا")) + "</td><td><span class=\"fxd-pill fxd-pill--" + st switch { "CLEARED" => "ok", "RETURNED" => "bad", "PENDING" => "warn", _ => "mute" } + "\">" + E(P0203Html.Nice(st)) + "</span></td>"
                + "<td><span class=\"ch-act\"><button type=\"button\" class=\"wb-icon-btn\" data-edit title=\"Edit\" aria-label=\"Edit cheque\"><span class=\"fa fa-pencil\" aria-hidden=\"true\"></span></button>"
                + "<button type=\"button\" class=\"wb-icon-btn del\" data-del title=\"Delete\" aria-label=\"Delete cheque\"><span class=\"fa fa-trash-o\" aria-hidden=\"true\"></span></button></span></td></tr>");
        }

        h.AppendLine("</tbody></table></div><div class=\"wb-empty\" id=\"chNone\" hidden><span class=\"fa fa-money\" aria-hidden=\"true\"></span>" + T("No cheques yet for this investor. Use Add cheque.", "لا توجد شيكات لهذا المستثمر بعد. استخدم إضافة شيك.") + "</div></div></section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Ready to accept?", "جاهزة للقبول؟") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/3</b>" + T("checks", "متطلبات") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-balance-scale\" aria-hidden=\"true\"></span>" + T("Coverage rules", "قواعد التغطية") + "</h2></div><div class=\"fxd-card-b\"><ol class=\"ch-rules\">"
            + "<li>" + T("The cheques of each investor must cover his outstanding balance.", "يجب أن تغطي شيكات كل مستثمر رصيده القائم.") + "</li>"
            + "<li>" + T("The cheques of all investors together must cover the outstanding balance of the whole portfolio.", "يجب أن تغطي شيكات جميع المستثمرين معاً الرصيد القائم للمحفظة كاملة.") + "</li>"
            + "<li>" + T("Only investors included in the deal are counted.", "يتم احتساب المستثمرين المدرجين في الصفقة فقط.") + "</li></ol></div></section></aside></div></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (required for accept and reject)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbReject\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject", "رفض") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbAccept\" disabled><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Accept & continue", "قبول ومتابعة") + "</button></div></div>");

        // cheque form (drawer) + option lists
        h.AppendLine("<div class=\"wb-drawer\" id=\"wbDrawer\" role=\"dialog\" aria-modal=\"true\" aria-hidden=\"true\" aria-labelledby=\"chDrT\"><div class=\"wb-dr-h\"><span class=\"wb-av\" id=\"chDrAv\"></span><div class=\"wb-who\"><strong id=\"chDrT\"></strong><small id=\"chDrS\"></small></div>"
            + "<button type=\"button\" class=\"wb-x\" data-close-drawer aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></div><div class=\"wb-dr-b\">"
            + "<div class=\"ch-left\" id=\"chLeft\"></div><div class=\"wb-fgrid\">"
            + "<div class=\"wb-f wide\"><label for=\"chFAcc\">" + T("Contract", "العقد") + " *</label><select id=\"chFAcc\"></select></div>"
            + "<div class=\"wb-f\"><label for=\"chFNo\">" + T("Cheque no.", "رقم الشيك") + " *</label><input id=\"chFNo\" maxlength=\"100\" autocomplete=\"off\"></div>"
            + "<div class=\"wb-f\"><label for=\"chFBank\">" + T("Bank name", "اسم البنك") + "</label><input id=\"chFBank\" maxlength=\"300\"></div>"
            + "<div class=\"wb-f\"><label for=\"chFCd\">" + T("Cheque date", "تاريخ الشيك") + "</label><input id=\"chFCd\" type=\"date\"></div>"
            + "<div class=\"wb-f\"><label for=\"chFDd\">" + T("Due date", "تاريخ الاستحقاق") + " *</label><input id=\"chFDd\" type=\"date\"></div>"
            + "<div class=\"wb-f\"><label for=\"chFAmt\">" + T("Amount", "المبلغ") + " *</label><input id=\"chFAmt\" type=\"number\" min=\"0.01\" step=\"0.01\" inputmode=\"decimal\"></div>"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Endorsed to company", "مظهّر للشركة") + " *</span><div class=\"wb-chips\" id=\"chFEnd\"><button type=\"button\" data-v=\"Y\" aria-pressed=\"false\">" + T("Yes", "نعم") + "</button><button type=\"button\" data-v=\"N\" aria-pressed=\"true\">" + T("No", "لا") + "</button></div></div>"
            + "<div class=\"wb-f\"><label for=\"chFSt\">" + T("Cheque status", "حالة الشيك") + " *</label><select id=\"chFSt\">");
        foreach (var s in await reader.QueryAsync(MtgApexPageSql.P213_STATUSES, items, ct))
            h.AppendLine("<option value=\"" + E(s.Text("CHEQUE_STATUS_CODE")) + "\">" + E(P0203Html.Nice(s.Text("CHEQUE_STATUS_CODE"))) + "</option>");
        h.AppendLine("</select></div><div class=\"wb-f\"><label for=\"chFClr\">" + T("Clearing date", "تاريخ التحصيل") + "</label><input id=\"chFClr\" type=\"date\"></div>"
            + "<div class=\"wb-f wide\"><label for=\"chFRr\">" + T("Return reason", "سبب الارتجاع") + "</label><textarea id=\"chFRr\" maxlength=\"1000\"></textarea></div></div></div>"
            + "<div class=\"wb-dr-f\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-close-drawer>" + T("Cancel", "إلغاء") + "</button><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"chSave\"><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Save cheque", "حفظ الشيك") + "</button></div></div>");
        h.AppendLine("<select id=\"chAccTpl\" hidden>");
        foreach (var a in await reader.QueryAsync(MtgApexPageSql.P213_ACCOUNTS, items, ct))
        {
            var acc = P0203Html.Num(a.Number("PORTFOLIO_ACCOUNT_ID"));
            var unit = a.Text("UNIT_CODE");
            h.AppendLine("<option value=\"" + acc + "\" data-inv=\"" + P0203Html.Num(a.Number("INVESTOR_ID")) + "\">" + E(a.Text("SELLER_CONTRACT_NO") ?? "#" + acc) + (unit is not null ? " &middot; " + E(unit) : string.Empty) + " &middot; " + Amt(a.Number("OV")) + "</option>");
        }

        h.AppendLine("</select>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Dt(DateOnly? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>amt(): to_char(nvl(p,0),'FM999G999G999G990D00').</summary>
    internal static string Amt(decimal? p) => ToChar(p ?? 0, "FM999G999G999G990D00");
}

/// <summary>App 105 page 213, the PL/SQL region "Step Actions".</summary>
public sealed class P0213StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 213;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(213, request, ct) };
}

/// <summary>App 105 page 213, Ajax Callback CHQ_DECISION: accept (approve the task) or reject the Cheque Coverage Review.</summary>
public sealed class P0213ChqDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 213;

    public string Name => "CHQ_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P213_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X02);
        var comment = MtgAjax.Trim(request.X03);
        if (comment is null)
            throw new ApexProcessException("ORA-20720: A comment is required for both Accept and Reject.");
        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20721: Invalid cheque coverage decision.");
        var newStatus = action == "ACCEPT" ? "CHEQUE_COVERAGE_ACCEPTED" : "CHEQUE_COVERAGE_REJECTED";
        if (!await db.Set<MtgLkpPortfolioStatus>().AnyAsync(s => s.StatusCode == newStatus, ct))
            throw new ApexProcessException("ORA-20725: Status " + newStatus + " is missing. Run the v80 database script (FINOVA_v80_CHEQUE_COVERAGE.sql) first.");
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "CHEQUE", MtgAjax.Long(request.Item("P213_TASK_ID")), ct, like: true, page: 213);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20722: No open Cheque Coverage Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        if (action == "REJECT")
        {
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "REJECT", comment, ct) ?? p.WorkflowTaskId;
            p.RejectionReason = comment;
        }
        else
        {
            // both coverage rules are checked again on the server
            var included = db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId && (a.DealDecisionCode ?? "SELECTED") == "SELECTED");
            var outstanding = await included.GroupBy(a => a.InvestorId).Select(g => new { InvestorId = g.Key, Ov = g.Sum(a => a.OutstandingValue) }).ToListAsync(ct);
            var cheques = await db.Set<MtgPurchasedPortfolioCheque>().Where(c => c.PortfolioId == p.PortfolioId)
                .Join(included, c => c.PortfolioAccountId, a => a.PortfolioAccountId, (c, a) => new { a.InvestorId, c.ChequeAmount })
                .GroupBy(x => x.InvestorId).Select(g => new { InvestorId = g.Key, Chq = g.Sum(x => x.ChequeAmount) }).ToListAsync(ct);
            var rows = outstanding.Select(o => new { o.Ov, Chq = cheques.FirstOrDefault(c => c.InvestorId == o.InvestorId)?.Chq ?? 0 }).ToList();
            if (rows.Count == 0)
                throw new ApexProcessException("ORA-20723: No investors are included in this deal.");
            var shortCount = rows.Count(r => r.Chq < r.Ov - 0.005m);
            if (shortCount > 0)
                throw new ApexProcessException("ORA-20724: " + shortCount.ToString(CultureInfo.InvariantCulture) + " investor(s) do not have cheques covering their outstanding balance.");
            decimal totalOut = rows.Sum(r => r.Ov), totalChq = rows.Sum(r => r.Chq);
            if (totalChq < totalOut - 0.005m)
                throw new ApexProcessException("ORA-20726: Total cheques (" + P0213ChequeCoverageReview.Amt(totalChq) + ") do not cover the portfolio outstanding (" + P0213ChequeCoverageReview.Amt(totalOut) + ").");
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct) ?? p.WorkflowTaskId;
            p.RejectionReason = null;
        }

        p.PortfolioStatusCode = newStatus;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangeAt = clock.UtcNow,
            ChangedBy = user.UserName ?? string.Empty,
            Comments = comment,
        });
        await db.SaveChangesAsync(ct);
        return action == "ACCEPT" ? "Cheque coverage accepted." : "Cheque coverage rejected.";
    }, ct);
}

/// <summary>App 105 page 213, Ajax Callback CHQ_DELETE.</summary>
public sealed class P0213ChqDelete(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 213;

    public string Name => "CHQ_DELETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var id = MtgAjax.Long(request.X02);
        var portfolioId = MtgAjax.Long(request.Item("P213_PORTFOLIO_ID"));
        var row = await db.Set<MtgPurchasedPortfolioCheque>().FirstOrDefaultAsync(c => c.ChequeId == id && c.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-20709: This cheque was not found in the portfolio.");
        db.Set<MtgPurchasedPortfolioCheque>().Remove(row);
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}

/// <summary>App 105 page 213, Ajax Callback CHQ_SAVE: adds or updates a cheque of an included investor's contract.</summary>
public sealed class P0213ChqSave(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 213;

    public string Name => "CHQ_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var pf = MtgAjax.Long(request.Item("P213_PORTFOLIO_ID"));
        var id = MtgAjax.Long(request.X02);
        var acc = MtgAjax.Long(request.X03);
        var no = Cut(MtgAjax.Trim(request.X04), 100);
        var bank = Cut(MtgAjax.Trim(request.X05), 300);
        var cd = Date(request.X06);
        var dd = Date(request.X07);
        var amount = MtgAjax.Number(request.X08);
        var endorsed = MtgAjax.Upper(request.X09) ?? "N";
        var status = MtgAjax.Upper(request.X10) ?? "PENDING";
        var clr = Date(request.X11);
        var rr = Cut(MtgAjax.Trim(request.X12), 1000);
        if (pf is null)
            throw new ApexProcessException("ORA-20701: Portfolio ID is missing from page session state.");
        if (!await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(a => a.PortfolioAccountId == acc && a.PortfolioId == pf && (a.DealDecisionCode ?? "SELECTED") == "SELECTED", ct))
            throw new ApexProcessException("ORA-20702: Choose a contract of an investor who is included in the deal.");
        if (no is null)
            throw new ApexProcessException("ORA-20703: Cheque No. is required.");
        if (amount is null or <= 0)
            throw new ApexProcessException("ORA-20704: Cheque Amount must be greater than zero.");
        if (dd is null)
            throw new ApexProcessException("ORA-20705: Due Date is required.");
        if (endorsed is not ("Y" or "N"))
            throw new ApexProcessException("ORA-20706: Endorsed to Company must be Yes or No.");
        if (!await db.Set<AdmLkpChequeStatus>().AnyAsync(s => s.ChequeStatusCode == status, ct))
            throw new ApexProcessException("ORA-20707: Unknown cheque status " + status + ".");
        MtgPurchasedPortfolioCheque row;
        if (id is null)
        {
            row = new MtgPurchasedPortfolioCheque { PortfolioId = pf.Value };
            db.Set<MtgPurchasedPortfolioCheque>().Add(row);
        }
        else
        {
            row = await db.Set<MtgPurchasedPortfolioCheque>().FirstOrDefaultAsync(c => c.ChequeId == id && c.PortfolioId == pf, ct)
                ?? throw new ApexProcessException("ORA-20708: This cheque was not found in the portfolio.");
        }

        row.PortfolioAccountId = acc;
        row.ChequeNo = no;
        row.BankName = bank;
        row.ChequeDate = cd;
        row.DueDate = dd;
        row.ChequeAmount = P0203Html.Round(amount.Value, 2);
        row.IsEndorsedToCompany = endorsed == "Y";
        row.ChequeStatusCode = status;
        row.ClearingDate = clr;
        row.ReturnReason = rr;
        await db.SaveChangesAsync(ct);
        return new { success = true, id = row.ChequeId };
    }, ct);

    private static string? Cut(string? p, int length) => p is { } s && s.Length > length ? s[..length] : p;

    private static DateOnly? Date(string? p) =>
        string.IsNullOrWhiteSpace(p) ? null
        : DateOnly.TryParseExact(p.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : throw new ApexProcessException("ORA-01861: literal does not match format string");
}
