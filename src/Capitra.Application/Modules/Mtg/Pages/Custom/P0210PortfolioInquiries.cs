using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 210 "Portfolio Inquiries": the PL/SQL region "Portfolio Inquiries", ported.</summary>
public sealed class P0210PortfolioInquiries(IApexSqlReader reader, IClock clock) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    public int App => 105;

    public int Page => 210;

    public string Name => "REGION_PORTFOLIO_INQUIRIES";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];

        // inquiry types come from REF_INQUIRY_TYPE (the table FNV_INQUIRY_RESULT.INQUIRY_TYPE_CODE references)
        var types = string.Join(";", (await reader.QueryAsync(MtgApexPageSql.P210_TYPES, items, ct))
            .Select(r => r.Text("C") + "=" + Cut((r.Text("N") ?? string.Empty).Replace(';', ',').Replace('=', '-'), 90)));
        var head = await reader.RowAsync(MtgApexPageSql.P210_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the inquiry task in your inbox.", "افتح هذه الصفحة من مهمة الاستعلام في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P210_PORTFOLIO_ID");
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var project = head.Text("PROJECT");
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P210_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,[data-inq-inv],.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"wbInq\" data-inbox=\"" + Attr(inboxUrl) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Portfolio inquiries", "استعلامات المحفظة") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Portfolio inquiries", "استعلامات المحفظة") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Record every inquiry and its result, review which investors stay in the deal, then complete this workflow step.", "سجّل كل استعلام ونتيجته، وراجع المستثمرين الباقين في الصفقة، ثم أكمل خطوة سير العمل.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Project", "المشروع") + "</span><strong>" + E(project) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Record inquiries", "تسجيل الاستعلامات") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Capture results", "تسجيل النتائج") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Review investors", "مراجعة المستثمرين") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Complete step", "إكمال الخطوة") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\">");

        // inquiries
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-comments\" aria-hidden=\"true\"></span>" + T("Inquiries", "الاستعلامات") + "</h2>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" style=\"height:36px\" id=\"wbAddInq\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("Add inquiry", "إضافة استعلام") + "</button></div>");
        h.AppendLine("<div class=\"wb-tbl-wrap\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Inquiry", "الاستعلام") + "</th><th>" + T("Subject", "الطرف") + "</th><th>" + T("Date", "التاريخ") + "</th><th>" + T("Result", "النتيجة") + "</th>"
            + "<th>" + T("Risk", "المخاطر") + "</th><th></th></tr></thead><tbody id=\"wbInqRows\">");
        var q = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P210_INQUIRIES, items, ct))
        {
            q++;
            var id = P0203Html.Num(r.Number("INQUIRY_ID"));
            var result = r.Text("RESULT_CODE");
            var risk = r.Text("RISK_LEVEL_CODE");
            var details = r.Text("DETAILS");
            var subjName = r.Text("SUBJ_NAME");
            var subjId = r.Number("SUBJECT_ID");
            h.AppendLine("<tr data-id=\"" + id + "\" data-result=\"" + (result is null ? "N" : "Y") + "\" data-risk=\"" + risk + "\">"
                + "<td><strong>" + E(P0203Html.Nice(r.Text("INQUIRY_TYPE"))) + "</strong><small>" + E(r.Text("REFERENCE_NO") ?? string.Empty) + "</small></td>"
                + "<td>" + E(P0203Html.Nice(r.Text("SUBJECT_TYPE")))
                + (subjName is not null ? "<small class=\"sn\">" + E(subjName) + "</small>" : subjId is not null ? " <span class=\"wb-code\">#" + P0203Html.Num(subjId) + "</span>" : string.Empty) + "</td>"
                + "<td class=\"fxd-num\">" + (r.Date("INQUIRY_DATE")?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "&ndash;") + "</td>"
                + "<td>" + (result is null ? "<span class=\"fxd-pill fxd-pill--warn\">" + T("Awaiting result", "بانتظار النتيجة") + "</span>" : "<strong>" + E(result) + "</strong>")
                + (details is not null ? "<small>" + E(details) + "</small>" : string.Empty) + "</td>"
                + "<td>" + (risk is null ? "&ndash;" : "<span class=\"wb-risk " + risk + "\">" + E(InitCap(risk)) + "</span>") + "</td>"
                + "<td style=\"text-align:end\"><button type=\"button\" class=\"wb-icon-btn\" data-edit=\"" + id + "\" aria-label=\"Edit\"><span class=\"fa fa-pencil\" aria-hidden=\"true\"></span></button> "
                + "<button type=\"button\" class=\"wb-icon-btn del\" data-del=\"" + id + "\" aria-label=\"Delete\"><span class=\"fa fa-trash-o\" aria-hidden=\"true\"></span></button></td></tr>");
        }

        h.AppendLine("</tbody></table></div>");
        if (q == 0)
            h.AppendLine("<div class=\"wb-empty\"><span class=\"fa fa-comments-o\" aria-hidden=\"true\"></span>" + T("No inquiries recorded yet. Start with Add inquiry.", "لا توجد استعلامات بعد. ابدأ بإضافة استعلام.") + "</div>");
        h.AppendLine("</section>");

        // investors
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("Portfolio investors", "مستثمرو المحفظة") + "</h2><span class=\"fxd-label\" id=\"wbInvCount\"></span></div>"
            + "<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wbQ\" placeholder=\"Search investor, code, contract or unit\" aria-label=\"Search investors\"></label>"
            + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Filter\" id=\"wbSeg\"><button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"IN\" aria-pressed=\"false\">" + T("Included", "مُدرج") + "<em></em></button><button type=\"button\" data-f=\"OUT\" aria-pressed=\"false\">" + T("Excluded", "مستبعد") + "<em></em></button></div></div><ul class=\"wb-rows\" id=\"wbRows\">");
        var n = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P210_ROWS, items, ct))
        {
            n++;
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE"), contract = r.Text("SELLER_CONTRACT_NO"), unit = r.Text("UNIT_CODE");
            var dec = r.Text("DEC");
            var acc = P0203Html.Num(r.Number("PORTFOLIO_ACCOUNT_ID"));
            var inqN = r.Number("INQ_N") ?? 0;
            var inqRisk = r.Text("INQ_RISK");
            decimal cv = r.Number("CV") ?? 0, pv = r.Number("PV") ?? 0, ov = r.Number("OV") ?? 0;
            h.AppendLine("<li class=\"wb-row" + (dec == "EXCLUDED" ? " is-out" : string.Empty) + "\" style=\"grid-template-columns:minmax(0,1.6fr) 150px 190px\" data-acc=\"" + acc + "\" data-dec=\"" + dec + "\" data-cv=\"" + P0203Html.Num(cv) + "\" data-ov=\"" + P0203Html.Num(ov) + "\""
                + " data-name=\"" + Attr(en ?? ar) + "\" data-q=\"" + Attr((code + " " + en + " " + ar + " " + contract + " " + unit).ToLowerInvariant()) + "\">"
                + "<div class=\"wb-person\"><span class=\"wb-av\" style=\"background:" + Colors[(n - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div class=\"wb-who\"><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small style=\"font-family:Cairo,sans-serif;unicode-bidi:plaintext;text-align:start\">" + E(ar) + "</small>" : string.Empty)
                + "<div class=\"wb-codes\"><span class=\"wb-code\">" + E(code) + "</span>" + (contract is not null ? "<span class=\"wb-code\">" + E(contract) + "</span>" : string.Empty)
                + (unit is not null ? "<span class=\"wb-code\">" + T("Unit", "وحدة") + " " + E(unit) + "</span>" : string.Empty) + "</div>"
                + "<div class=\"wb-inq\"><span class=\"n" + (inqN > 0 ? " has" : string.Empty) + "\">" + P0203Html.Num(inqN) + " " + (inqN == 1 ? T("inquiry", "استعلام") : T("inquiries", "استعلامات")) + "</span>"
                + (inqRisk is not null ? "<span class=\"wb-risk " + inqRisk + "\">" + E(InitCap(inqRisk)) + "</span>" : string.Empty)
                + "<button type=\"button\" class=\"wb-link\" data-inq-inv=\"" + P0203Html.Num(r.Number("INVESTOR_ID")) + "\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("Add inquiry", "إضافة استعلام") + "</button></div></div></div>"
                + "<div class=\"wb-money\"><b>" + Money(ov) + "</b><small>" + T("of", "من") + " " + Money(cv) + (cv > 0 ? " &middot; " + P0203Html.Num(P0203Html.Round(pv / cv * 100)) + "% " + T("paid", "مسدد") : string.Empty) + "</small></div>"
                + "<div class=\"wb-dec\">" + (dec == "EXCLUDED" ? "<span class=\"wb-lock\"><span class=\"fa fa-lock\" aria-hidden=\"true\"></span>" + T("Excluded", "مستبعد") + "</span>"
                    : "<div class=\"wb-toggle\" role=\"group\" aria-label=\"Deal decision\"><button type=\"button\" class=\"in\" aria-pressed=\"true\" disabled>" + T("Included", "مُدرج") + "</button>"
                    + "<button type=\"button\" class=\"out\" data-exclude=\"" + acc + "\" aria-pressed=\"false\">" + T("Exclude", "استبعاد") + "</button></div>") + "<span class=\"wb-saved\"></span></div></li>");
        }

        if (n == 0)
            h.AppendLine("<li class=\"wb-empty\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("This portfolio has no investor contracts yet.", "لا توجد عقود مستثمرين في هذه المحفظة بعد.") + "</li>");
        h.AppendLine("</ul><div class=\"wb-empty\" id=\"wbNoMatch\" hidden>" + T("No contracts match this filter.", "لا توجد عقود مطابقة.") + "</div></section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-tasks\" aria-hidden=\"true\"></span>" + T("Progress", "التقدم") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/2</b>" + T("done", "مكتمل") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Deal impact", "أثر القرار على الصفقة") + "</h2><span class=\"fxd-label\">" + T("live", "مباشر") + "</span></div>"
            + "<div class=\"wb-impact\"><div><span class=\"fxd-label\">" + T("Face value", "القيمة الاسمية") + "</span><b id=\"wbFace\">-</b><small id=\"wbFaceD\"></small></div><div><span class=\"fxd-label\">" + T("Outstanding", "القائم") + "</span><b id=\"wbOut\">-</b><small id=\"wbOutD\"></small></div></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-mix\" role=\"img\"><i id=\"wbMixIn\" style=\"background:var(--teal-600)\"></i><i id=\"wbMixOut\" style=\"background:var(--bad)\"></i></div>"
            + "<div class=\"wb-legend\"><span style=\"--c:var(--teal-600)\" id=\"wbLIn\"></span><span style=\"--c:var(--bad)\" id=\"wbLOut\"></span></div></div></section></aside></div></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (optional)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<a class=\"fxd-btn fxd-btn--ghost\" href=\"" + Attr(taskUrl) + "\">" + T("Cancel", "إلغاء") + "</a>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbRejectInq\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject", "رفض") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbComplete\"><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Approve inquiry step", "إكمال خطوة الاستعلام") + "</button></div></div>");

        // inquiry drawer
        h.AppendLine("<aside class=\"wb-drawer\" id=\"wbDrawer\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbDrT\" aria-hidden=\"true\"><div class=\"wb-dr-h\"><span class=\"wb-av\"><span class=\"fa fa-comments\" aria-hidden=\"true\"></span></span><div class=\"wb-who\"><strong id=\"wbDrT\"></strong><small>" + E(pno) + "</small></div>"
            + "<button type=\"button\" class=\"wb-x\" data-close-drawer aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></div><div class=\"wb-dr-b\"><input type=\"hidden\" id=\"wbInqId\">"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Inquiry type", "نوع الاستعلام") + " *</span>" + Chips("wbInqType", types) + "</div>"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Subject", "الطرف") + " *</span>" + Chips("wbSubType", "PORTFOLIO=Portfolio;DEVELOPER=Developer;PROJECT=Project;INVESTOR=Investor;SIGNATORY=Signatory;GUARANTEE=Guarantee", "PORTFOLIO") + "</div>"
            + "<div class=\"wb-f\" id=\"wbSubPickF\"><label for=\"wbSubPick\">" + T("Choose the subject", "اختر الطرف") + " *</label><select id=\"wbSubPick\"></select></div>"
            + "<div class=\"wb-fgrid\"><div class=\"wb-f\" id=\"wbSubIdF\" hidden><label for=\"wbSubId\">" + T("Subject ID", "رقم الطرف") + "</label><input id=\"wbSubId\" type=\"number\" min=\"1\" placeholder=\"Optional\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbDate\">" + T("Inquiry date", "تاريخ الاستعلام") + "</label><input id=\"wbDate\" type=\"date\" value=\"" + clock.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbRef\">" + T("Reference no.", "الرقم المرجعي") + "</label><input id=\"wbRef\" maxlength=\"100\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbResult\">" + T("Result", "النتيجة") + "</label><input id=\"wbResult\" maxlength=\"100\" placeholder=\"e.g. No adverse record\"></div>"
            + "<div class=\"wb-f wide\"><span class=\"wb-f-l\">" + T("Risk level", "مستوى المخاطر") + "</span>" + Chips("wbRisk", "=Not set;LOW=Low;MEDIUM=Medium;HIGH=High;CRITICAL=Critical", string.Empty) + "</div>"
            + "<div class=\"wb-f wide\"><label for=\"wbDet\">" + T("Result details", "تفاصيل النتيجة") + "</label><textarea id=\"wbDet\" rows=\"4\"></textarea></div></div></div>"
            + "<div class=\"wb-dr-f\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-close-drawer>" + T("Cancel", "إلغاء") + "</button><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"wbSave\">" + T("Save inquiry", "حفظ الاستعلام") + "</button></div></aside>");
        h.AppendLine("<div id=\"wbSubjData\" hidden data-pf=\"" + portfolioId + "\" data-pf-n=\"" + Attr(pno) + "\" data-dev=\"" + P0203Html.Num(head.Number("DEVELOPER_ID")) + "\" data-dev-n=\"" + Attr(dev) + "\""
            + " data-prj=\"" + P0203Html.Num(head.Number("PROJECT_ID")) + "\" data-prj-n=\"" + Attr(project) + "\">");
        foreach (var s in await reader.QueryAsync(MtgApexPageSql.P210_SUBJECTS, items, ct))
            h.AppendLine("<i data-id=\"" + P0203Html.Num(s.Number("INVESTOR_ID")) + "\" data-n=\"" + Attr(s.Text("CODE") + " - " + s.Text("NM")) + "\"></i>");
        h.AppendLine("</div>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Cut(string p, int length) => p.Length > length ? p[..length] : p;

    private static string InitCap(string p) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p.ToLowerInvariant());

    /// <summary>The region's chips(): a "CODE=Label;..." list as a chip group (an empty code never matches, as in Oracle).</summary>
    private static string Chips(string id, string? list, string? @default = null)
    {
        var o = new StringBuilder();
        foreach (var item in (list ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = item.IndexOf('=', StringComparison.Ordinal);
            var code = eq < 0 ? string.Empty : item[..eq];
            var label = Cut(item[(eq + 1)..], 100);
            var pressed = code.Length > 0 && code == @default;
            o.Append("<button type=\"button\" class=\"c-" + code + "\" data-v=\"" + code + "\" aria-pressed=\"" + (pressed ? "true" : "false") + "\">" + E(label) + "</button>");
        }

        return "<div class=\"wb-chips\" id=\"" + id + "\">" + o + "</div>";
    }
}

/// <summary>App 105 page 210, the PL/SQL region "Step Actions".</summary>
public sealed class P0210StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 210;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(210, request, ct, "wbComplete", "wbRejectInq", "Complete inquiry step", "إكمال خطوة الاستعلام") };
}

/// <summary>App 105 page 210, Ajax Callback INQ_COMPLETE: approve (or complete) or reject the Portfolio Inquiry task.</summary>
public sealed class P0210InqComplete(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 210;

    public string Name => "INQ_COMPLETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P210_PORTFOLIO_ID"));
        var comment = MtgAjax.Trim(request.X01);
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "INQUIR", MtgAjax.Long(request.Item("P210_TASK_ID")), ct, like: true)
              ?? await steps.PageTaskAsync(portfolioId.Value, 210, ct);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20120: No open Portfolio Inquiry task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        var selected = await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId && a.DealDecisionCode == "SELECTED")
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Face = g.Sum(a => a.ContractValue), Outstanding = g.Sum(a => a.OutstandingValue), Paid = g.Sum(a => a.PaidValue) })
            .FirstOrDefaultAsync(ct);
        string newStatus;
        if (MtgAjax.Upper(request.X02) == "REJECT")
        {
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "REJECT", comment, ct);
            newStatus = "INQUIRY_REJECTED";
        }
        else
        {
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct);
            p.SelectedAccountCount = selected?.Count ?? 0;
            p.SelectedFaceValue = selected?.Face ?? 0;
            p.SelectedOutstandingValue = selected?.Outstanding ?? 0;
            p.SelectedPaidValue = selected?.Paid ?? 0;
            newStatus = "INQUIRY_ACCEPTED";
        }

        p.PortfolioStatusCode = newStatus;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangeAt = clock.UtcNow,
            ChangedBy = user.UserName ?? string.Empty,
            Comments = comment ?? (newStatus == "INQUIRY_REJECTED" ? "Portfolio Inquiry rejected." : "Portfolio Inquiry workflow task completed."),
        });
        await db.SaveChangesAsync(ct);
        return new { success = true, newTaskId = p.WorkflowTaskId };
    }, ct);
}

/// <summary>App 105 page 210, Ajax Callback INQ_DELETE.</summary>
public sealed class P0210InqDelete(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 210;

    public string Name => "INQ_DELETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var id = MtgAjax.Long(request.X01);
        var portfolioId = MtgAjax.Long(request.Item("P210_PORTFOLIO_ID"));
        var row = await db.Set<MtgInquiryResult>().FirstOrDefaultAsync(i => i.InquiryId == id && i.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-20106: Inquiry record not found.");
        db.Set<MtgInquiryResult>().Remove(row);
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}

/// <summary>App 105 page 210, Ajax Callback INQ_GET: one inquiry of the portfolio, for the drawer.</summary>
public sealed class P0210InqGet(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 210;

    public string Name => "INQ_GET";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            var id = MtgAjax.Long(request.X01);
            var portfolioId = MtgAjax.Long(request.Item("P210_PORTFOLIO_ID"));
            var r = await db.Set<MtgInquiryResult>().AsNoTracking().FirstOrDefaultAsync(i => i.InquiryId == id && i.PortfolioId == portfolioId, ct);
            if (r is null)
                return new { success = false, message = "Inquiry record not found." };
            return new
            {
                success = true,
                inquiryId = r.InquiryId,
                subjectType = r.SubjectType,
                subjectId = r.SubjectId,
                inquiryTypeCode = r.InquiryTypeCode,
                inquiryDate = r.InquiryDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                referenceNo = r.ReferenceNo,
                resultCode = r.ResultCode,
                riskLevelCode = r.RiskLevelCode,
                resultDetails = r.ResultDetails is { Length: > 4000 } d ? d[..4000] : r.ResultDetails,
            };
        }
        catch (ApexProcessException e)
        {
            return new { success = false, message = e.Message };
        }
    }
}

/// <summary>App 105 page 210, Ajax Callback INQ_SAVE: adds or updates an inquiry of the portfolio.</summary>
public sealed class P0210InqSave(ICapitraDbContext db) : IApexProcess
{
    private static readonly string[] Subjects = ["PORTFOLIO", "DEVELOPER", "PROJECT", "INVESTOR", "SIGNATORY", "GUARANTEE"];

    public int App => 105;

    public int Page => 210;

    public string Name => "INQ_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var id = MtgAjax.Long(request.X01);
        var subjectType = MtgAjax.Upper(request.X02);
        var subjectId = MtgAjax.Long(request.X03);
        var inquiryType = MtgAjax.Upper(request.X04);
        DateOnly? date = string.IsNullOrEmpty(request.X05) ? null
            : DateOnly.TryParseExact(request.X05, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
            : throw new ApexProcessException("ORA-01861: literal does not match format string");
        if (subjectType is null)
            throw new ApexProcessException("ORA-20101: Subject Type is required.");
        if (inquiryType is null)
            throw new ApexProcessException("ORA-20102: Inquiry Type is required.");
        if (!Subjects.Contains(subjectType))
            throw new ApexProcessException("ORA-20103: Invalid Subject Type.");
#pragma warning disable CA1862 // upper() runs in the database
        inquiryType = await db.Set<MtgLkpInquiryType>().Where(t => t.InquiryTypeCode.ToUpper() == inquiryType).Select(t => t.InquiryTypeCode).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-20104: Invalid inquiry type: it is not defined in Inquiry Types.");
#pragma warning restore CA1862
        var portfolioId = MtgAjax.Long(request.Item("P210_PORTFOLIO_ID"));
        MtgInquiryResult row;
        if (id is null)
        {
            row = new MtgInquiryResult { PortfolioId = portfolioId ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"FNV_INQUIRY_RESULT\".\"PORTFOLIO_ID\")") };
            db.Set<MtgInquiryResult>().Add(row);
        }
        else
        {
            row = await db.Set<MtgInquiryResult>().FirstOrDefaultAsync(i => i.InquiryId == id && i.PortfolioId == portfolioId, ct)
                ?? throw new ApexProcessException("ORA-20105: Inquiry record not found.");
        }

        row.SubjectType = subjectType;
        row.SubjectId = subjectId;
        row.InquiryTypeCode = inquiryType;
        row.InquiryDate = date;
        row.ReferenceNo = MtgAjax.Trim(request.X06);
        row.ResultCode = MtgAjax.Trim(request.X07);
        row.RiskLevelCode = MtgAjax.Upper(request.X08);
        row.ResultDetails = MtgAjax.Trim(request.X09);
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}

/// <summary>App 105 page 210, Ajax Callback INQ_UPDATE_INVESTOR: excludes an investor's contract (an exclusion is final).</summary>
public sealed class P0210InqUpdateInvestor(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 210;

    public string Name => "INQ_UPDATE_INVESTOR";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var accountId = MtgAjax.Long(request.X01);
        var decision = MtgAjax.Upper(request.X02);
        var portfolioId = MtgAjax.Long(request.Item("P210_PORTFOLIO_ID"));
        if (decision is not ("SELECTED" or "EXCLUDED"))
            throw new ApexProcessException("ORA-20110: Decision must be SELECTED or EXCLUDED.");
        var account = await db.Set<MtgPurchasedPortfolioAccount>().FirstOrDefaultAsync(a => a.PortfolioAccountId == accountId && a.PortfolioId == portfolioId, ct);
        if (account is null)
            return new { success = false, message = "Portfolio investor account not found." };
        if ((account.DealDecisionCode ?? "SELECTED") == "EXCLUDED" && decision != "EXCLUDED")
            throw new ApexProcessException("ORA-20111: Excluded investors cannot be selected again.");
        account.DealDecisionCode = decision;
        await db.SaveChangesAsync(ct);
        return new { success = true, locked = decision == "EXCLUDED" };
    }, ct);
}
