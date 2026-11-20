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

/// <summary>App 105 page 209 "Portfolio AML Review": the PL/SQL region "AML Review", ported.</summary>
public sealed class P0209AmlReview(IApexSqlReader reader, IClock clock) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    public int App => 105;

    public int Page => 209;

    public string Name => "REGION_AML_REVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P209_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the AML review task in your inbox.", "افتح هذه الصفحة من مهمة مراجعة مكافحة غسل الأموال في صندوقك.") + "</p></div></div>\n",
            };
        }

        var pno = head.Text("PORTFOLIO_NO");
        var status = head.Text("PORTFOLIO_STATUS_CODE");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(status);
        var devId = P0203Html.Num(head.Number("DEVELOPER_ID"));
        var dev = head.Text("DEV");
        var project = head.Text("PROJECT");
        var devStatus = (await reader.RowAsync(MtgApexPageSql.P209_DEV_STATUS, new Dictionary<string, string?>(items) { ["DEVELOPER_ID"] = devId }, ct))?.Text("RESULT_CODE") ?? "NOT_SCREENED";
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P209_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", request.Item("P209_PORTFOLIO_ID")) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"wbAml\" data-inbox=\"" + Attr(inboxUrl) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("AML review", "مراجعة مكافحة غسل الأموال") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("AML review", "مراجعة مكافحة غسل الأموال") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Screen the developer and every investor, decide who stays in the deal, then accept or reject the review.", "افحص المطور وكل مستثمر، وحدد من يبقى في الصفقة، ثم اقبل المراجعة أو ارفضها.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Project", "المشروع") + "</span><strong>" + E(project) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Screen developer", "فحص المطور") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Screen investors", "فحص المستثمرين") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Decide deal inclusion", "تحديد الإدراج في الصفقة") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Accept or reject", "القبول أو الرفض") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\"><section class=\"fxd-card\">");
        h.AppendLine("<div class=\"fxd-card-h\"><h2><span class=\"fa fa-shield\" aria-hidden=\"true\"></span>" + T("Subjects to screen", "الأطراف المطلوب فحصها") + "</h2><span class=\"fxd-label\" id=\"wbScreened\"></span></div>");
        var unscreened = devStatus == "NOT_SCREENED";
        h.AppendLine("<div class=\"wb-dev\" id=\"wbDev\" data-type=\"DEVELOPER\" data-id=\"" + devId + "\" data-name=\"" + Attr(dev) + "\" data-status=\"" + devStatus + "\">"
            + "<span class=\"wb-av\" style=\"background:linear-gradient(135deg,#014B55,#00636D)\">" + E(Initials(dev)) + "</span><div class=\"wb-who\"><strong>" + E(dev) + "</strong><small>" + T("Developer", "المطور") + "</small></div>"
            + Pill(devStatus) + "<button type=\"button\" class=\"fxd-btn " + (unscreened ? "fxd-btn--teal" : "fxd-btn--ghost") + "\" style=\"height:36px\" data-screen=\"DEVELOPER:" + devId + "\">"
            + (unscreened ? T("Screen developer", "فحص المطور") : T("Add check", "إضافة فحص")) + "</button></div>");
        h.AppendLine("<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wbQ\" placeholder=\"Search investor, code, contract or unit\" aria-label=\"Search investors\"></label>"
            + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Filter\" id=\"wbSeg\">"
            + "<button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"UNSCREENED\" aria-pressed=\"false\">" + T("Not screened", "لم يتم فحصه") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"FLAGGED\" aria-pressed=\"false\">" + T("Flagged", "عليه ملاحظات") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"UNDECIDED\" aria-pressed=\"false\">" + T("Undecided", "بدون قرار") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"EXCLUDED\" aria-pressed=\"false\">" + T("Excluded", "مستبعد") + "<em></em></button></div></div>");
        h.AppendLine("<div class=\"wb-bulk\" id=\"wbBulk\" hidden><span id=\"wbBulkText\"></span><button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbBulkBtn\">" + T("Exclude them", "استبعادهم") + "</button></div><ul class=\"wb-rows\" id=\"wbRows\">");
        var n = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P209_ROWS, items, ct))
        {
            n++;
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE"), contract = r.Text("SELLER_CONTRACT_NO"), unit = r.Text("UNIT_CODE");
            var dec = r.Text("DEAL_DECISION_CODE");
            var aml = r.Text("AML_STATUS");
            decimal cv = r.Number("CV") ?? 0, pv = r.Number("PV") ?? 0, ov = r.Number("OV") ?? 0;
            h.AppendLine("<li class=\"wb-row" + (dec == "EXCLUDED" ? " is-out" : string.Empty) + "\" data-acc=\"" + P0203Html.Num(r.Number("PORTFOLIO_ACCOUNT_ID")) + "\" data-inv=\"" + P0203Html.Num(r.Number("INVESTOR_ID")) + "\" data-status=\"" + aml + "\""
                + " data-dec=\"" + dec + "\" data-cv=\"" + P0203Html.Num(cv) + "\" data-ov=\"" + P0203Html.Num(ov) + "\" data-name=\"" + Attr(en ?? ar) + "\""
                + " data-q=\"" + Attr((code + " " + en + " " + ar + " " + contract + " " + unit).ToLowerInvariant()) + "\">"
                + "<div class=\"wb-person\"><span class=\"wb-av\" style=\"background:" + Colors[(n - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div class=\"wb-who\"><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small style=\"font-family:Cairo,sans-serif;unicode-bidi:plaintext;text-align:start\">" + E(ar) + "</small>" : string.Empty)
                + "<div class=\"wb-codes\"><span class=\"wb-code\">" + E(code) + "</span>" + (contract is not null ? "<span class=\"wb-code\">" + E(contract) + "</span>" : string.Empty)
                + (unit is not null ? "<span class=\"wb-code\">" + T("Unit", "وحدة") + " " + E(unit) + "</span>" : string.Empty) + "</div></div></div>"
                + "<div class=\"wb-aml\">" + Pill(aml) + "<button type=\"button\" class=\"wb-link\" data-screen=\"INVESTOR:" + P0203Html.Num(r.Number("INVESTOR_ID")) + "\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>"
                + (aml == "NOT_SCREENED" ? T("Screen now", "افحص الآن") : T("Add check", "إضافة فحص")) + "</button></div>"
                + "<div class=\"wb-money\"><b>" + Money(ov) + "</b><small>" + T("of", "من") + " " + Money(cv) + (cv > 0 ? " &middot; " + P0203Html.Num(P0203Html.Round(pv / cv * 100)) + "% " + T("paid", "مسدد") : string.Empty) + "</small></div>"
                + "<div><div class=\"wb-toggle\" role=\"group\" aria-label=\"Deal decision\"><button type=\"button\" class=\"in\" data-dec=\"SELECTED\" aria-pressed=\"" + (dec == "SELECTED" ? "true" : "false") + "\">" + T("Include", "إدراج") + "</button>"
                + "<button type=\"button\" class=\"out\" data-dec=\"EXCLUDED\" aria-pressed=\"" + (dec == "EXCLUDED" ? "true" : "false") + "\">" + T("Exclude", "استبعاد") + "</button></div><span class=\"wb-saved\"></span></div></li>");
        }

        if (n == 0)
            h.AppendLine("<li class=\"wb-empty\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("This portfolio has no investor contracts yet.", "لا توجد عقود مستثمرين في هذه المحفظة بعد.") + "</li>");
        h.AppendLine("</ul><div class=\"wb-empty\" id=\"wbNoMatch\" hidden>" + T("No contracts match this filter.", "لا توجد عقود مطابقة.") + "</div></section>");

        // screening log (also feeds the drawer history)
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-list\" aria-hidden=\"true\"></span>" + T("Screening log", "سجل الفحوصات") + "</h2><span class=\"fxd-label\" id=\"wbLogCount\"></span></div>"
            + "<div class=\"wb-tbl-wrap\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Subject", "الطرف") + "</th><th>" + T("Check", "الفحص") + "</th><th>" + T("Result", "النتيجة") + "</th><th>" + T("Risk", "المخاطر") + "</th>"
            + "<th>" + T("Match", "التطابق") + "</th><th>" + T("Date", "التاريخ") + "</th><th>" + T("Reference", "المرجع") + "</th></tr></thead><tbody id=\"wbLog\">");
        foreach (var a in await reader.QueryAsync(MtgApexPageSql.P209_LOG, items, ct))
        {
            var risk = a.Text("RISK_LEVEL_CODE");
            var score = a.Number("MATCH_SCORE_PCT");
            h.AppendLine("<tr data-subject=\"" + a.Text("SUBJECT_TYPE") + ":" + P0203Html.Num(a.Number("SUBJECT_ID")) + "\" data-type=\"" + Attr(P0203Html.Nice(a.Text("AML_TYPE_CODE"))) + "\" data-details=\"" + Attr(a.Text("DETAILS")) + "\"><td><strong>" + E(a.Text("SUBJECT_NAME")) + "</strong></td><td>" + E(P0203Html.Nice(a.Text("AML_TYPE_CODE"))) + "</td>"
                + "<td>" + Pill(a.Text("RESULT_CODE")) + "</td><td><span class=\"wb-risk " + risk + "\">" + E(risk is null ? "-" : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(risk.ToLowerInvariant())) + "</span></td>"
                + "<td class=\"fxd-num\">" + (score is null ? "&ndash;" : ToChar(score, "FM990D0") + "%") + "</td>"
                + "<td class=\"fxd-num\">" + a.Date("SCREENING_DATE")?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + "</td><td><span class=\"wb-code\">" + E(a.Text("REFERENCE_NO") ?? "-") + "</span></td></tr>");
        }

        h.AppendLine("</tbody></table></div></section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Ready to accept?", "جاهزة للقبول؟") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/3</b>" + T("checks", "متطلبات") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Deal impact", "أثر القرار على الصفقة") + "</h2><span class=\"fxd-label\">" + T("live", "مباشر") + "</span></div>"
            + "<div class=\"wb-impact\"><div><span class=\"fxd-label\">" + T("Face value", "القيمة الاسمية") + "</span><b id=\"wbFace\">-</b><small id=\"wbFaceD\"></small></div><div><span class=\"fxd-label\">" + T("Outstanding", "القائم") + "</span><b id=\"wbOut\">-</b><small id=\"wbOutD\"></small></div></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-mix\" id=\"wbMix\" role=\"img\"><i id=\"wbMixIn\" style=\"background:var(--teal-600)\"></i><i id=\"wbMixOut\" style=\"background:var(--bad)\"></i></div>"
            + "<div class=\"wb-legend\"><span style=\"--c:var(--teal-600)\" id=\"wbLIn\"></span><span style=\"--c:var(--bad)\" id=\"wbLOut\"></span></div></div></section></aside></div></div>");

        // decision bar
        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (required for accept and reject)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbReject\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject review", "رفض المراجعة") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbAccept\" disabled><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Accept & continue", "قبول ومتابعة") + "</button></div></div>");

        // screening drawer
        h.AppendLine("<aside class=\"wb-drawer\" id=\"wbDrawer\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbDrT\" aria-hidden=\"true\"><div class=\"wb-dr-h\"><span class=\"wb-av\" id=\"wbDrAv\"></span><div class=\"wb-who\"><strong id=\"wbDrT\"></strong><small id=\"wbDrS\"></small></div>"
            + "<button type=\"button\" class=\"wb-x\" data-close-drawer aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></div><div class=\"wb-dr-b\">"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Check type", "نوع الفحص") + "</span><div class=\"wb-chips\" id=\"wbType\"><button type=\"button\" data-v=\"AML_SCREENING\" aria-pressed=\"true\">AML</button>"
            + "<button type=\"button\" data-v=\"PEP_SCREENING\" aria-pressed=\"false\">PEP</button><button type=\"button\" data-v=\"SANCTIONS_SCREENING\" aria-pressed=\"false\">" + T("Sanctions", "العقوبات") + "</button>"
            + "<button type=\"button\" data-v=\"ADVERSE_MEDIA\" aria-pressed=\"false\">" + T("Adverse media", "الإعلام السلبي") + "</button></div></div>"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Result", "النتيجة") + "</span><div class=\"wb-chips\" id=\"wbRes\"><button type=\"button\" class=\"c-CLEAR\" data-v=\"CLEAR\" aria-pressed=\"true\">" + T("Clear", "سليم") + "</button>"
            + "<button type=\"button\" class=\"c-POTENTIAL_MATCH\" data-v=\"POTENTIAL_MATCH\" aria-pressed=\"false\">" + T("Potential match", "تطابق محتمل") + "</button>"
            + "<button type=\"button\" class=\"c-REVIEW_REQUIRED\" data-v=\"REVIEW_REQUIRED\" aria-pressed=\"false\">" + T("Review required", "يتطلب مراجعة") + "</button>"
            + "<button type=\"button\" class=\"c-CONFIRMED_MATCH\" data-v=\"CONFIRMED_MATCH\" aria-pressed=\"false\">" + T("Confirmed match", "تطابق مؤكد") + "</button></div></div>"
            + "<div class=\"wb-fgrid\"><div class=\"wb-f\"><label for=\"wbRisk\">" + T("Risk level", "مستوى المخاطر") + "</label><select id=\"wbRisk\"><option value=\"LOW\">Low</option><option value=\"MEDIUM\">Medium</option><option value=\"HIGH\">High</option><option value=\"CRITICAL\">Critical</option></select></div>"
            + "<div class=\"wb-f\"><label for=\"wbDate\">" + T("Screening date", "تاريخ الفحص") + "</label><input type=\"date\" id=\"wbDate\" value=\"" + clock.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\"></div>"
            + "<div class=\"wb-f wide\"><label for=\"wbScore\">" + T("Match score", "نسبة التطابق") + "</label><div class=\"wb-meter\"><input type=\"range\" id=\"wbScore\" min=\"0\" max=\"100\" value=\"0\"><output id=\"wbScoreOut\" for=\"wbScore\">0%</output></div></div>"
            + "<div class=\"wb-f wide\"><label for=\"wbRef\">" + T("Reference no.", "الرقم المرجعي") + "</label><input id=\"wbRef\" maxlength=\"100\"></div>"
            + "<div class=\"wb-f wide\"><label for=\"wbDet\">" + T("Result details", "تفاصيل النتيجة") + "</label><textarea id=\"wbDet\" rows=\"3\"></textarea></div></div>"
            + "<div><div class=\"fxd-label\" style=\"margin-bottom:10px\">" + T("Previous checks", "الفحوصات السابقة") + "</div><ul class=\"wb-hist\" id=\"wbHist\"></ul></div></div>"
            + "<div class=\"wb-dr-f\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-close-drawer>" + T("Cancel", "إلغاء") + "</button><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"wbSave\">" + T("Save screening", "حفظ الفحص") + "</button></div></aside>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Pill(string? code) => code switch
    {
        "CLEAR" => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Clear", "سليم") + "</span>",
        "POTENTIAL_MATCH" => "<span class=\"fxd-pill fxd-pill--warn\">" + T("Potential match", "تطابق محتمل") + "</span>",
        "REVIEW_REQUIRED" => "<span class=\"fxd-pill fxd-pill--warn\">" + T("Review required", "يتطلب مراجعة") + "</span>",
        "CONFIRMED_MATCH" => "<span class=\"fxd-pill fxd-pill--bad\">" + T("Confirmed match", "تطابق مؤكد") + "</span>",
        _ => "<span class=\"fxd-pill fxd-pill--mute\">" + T("Not screened", "لم يتم الفحص") + "</span>",
    };
}

/// <summary>App 105 page 209, the PL/SQL region "Step Actions".</summary>
public sealed class P0209StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 209;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(209, request, ct) };
}

/// <summary>App 105 page 209, Ajax Callback AML_ADD_RESULT: a screening result of the developer or an investor.</summary>
public sealed class P0209AmlAddResult(ICapitraDbContext db) : IApexProcess
{
    private static readonly string[] Types = ["AML_SCREENING", "PEP_SCREENING", "SANCTIONS_SCREENING", "ADVERSE_MEDIA"];
    private static readonly string[] Results = ["CLEAR", "POTENTIAL_MATCH", "CONFIRMED_MATCH", "REVIEW_REQUIRED"];
    private static readonly string[] Risks = ["LOW", "MEDIUM", "HIGH", "CRITICAL"];

    public int App => 105;

    public int Page => 209;

    public string Name => "AML_ADD_RESULT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P209_PORTFOLIO_ID"));
        var subjectType = MtgAjax.Upper(request.X02);
        var subjectId = MtgAjax.Long(request.X03);
        var amlType = MtgAjax.Upper(request.X04);
        var result = MtgAjax.Upper(request.X07);
        var risk = MtgAjax.Upper(request.X08);
        if (portfolioId is null)
            throw new ApexProcessException("ORA-20030: Portfolio ID is missing from page session state.");
        var score = request.X09 is null ? null : MtgAjax.Number(request.X09);
        if (subjectType is not ("DEVELOPER" or "INVESTOR"))
            throw new ApexProcessException("ORA-20001: Invalid subject type.");
        if (!Types.Contains(amlType))
            throw new ApexProcessException("ORA-20002: Invalid AML type.");
        if (!Results.Contains(result))
            throw new ApexProcessException("ORA-20003: Invalid AML result.");
        if (!Risks.Contains(risk))
            throw new ApexProcessException("ORA-20004: Invalid risk level.");
        if (score is < 0 or > 100)
            throw new ApexProcessException("ORA-20005: Match Score must be between 0 and 100.");
        var belongs = subjectType == "DEVELOPER"
            ? await db.Set<MtgPurchasedPortfolio>().AnyAsync(p => p.PortfolioId == portfolioId && p.DeveloperId == subjectId, ct)
            : await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(a => a.PortfolioId == portfolioId && a.InvestorId == subjectId, ct);
        if (!belongs)
            throw new ApexProcessException("ORA-20006: Selected subject does not belong to this portfolio.");
        DateOnly? date = string.IsNullOrEmpty(request.X05) ? null
            : DateOnly.TryParseExact(request.X05, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
            : throw new ApexProcessException("ORA-01861: literal does not match format string");
        db.Set<MtgAmlResult>().Add(new MtgAmlResult
        {
            PortfolioId = portfolioId.Value,
            SubjectType = subjectType,
            SubjectId = subjectId!.Value,
            AmlTypeCode = amlType!,
            ScreeningDate = date ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"FNV_AML_RESULTS\".\"SCREENING_DATE\")"),
            ReferenceNo = MtgAjax.Trim(request.X06),
            ResultCode = result!,
            RiskLevelCode = risk,
            MatchScorePct = score,
            ResultDetails = MtgAjax.Trim(request.X10),
        });
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}

/// <summary>App 105 page 209, Ajax Callback AML_DECISION: accept (approve the task) or reject the AML review.</summary>
public sealed class P0209AmlDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 209;

    public string Name => "AML_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var taskId = MtgAjax.Long(request.Item("P209_TASK_ID"));
        var portfolioId = MtgAjax.Long(request.Item("P209_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X02);
        var comment = MtgAjax.Trim(request.X03);
        if (comment is null)
            throw new ApexProcessException("ORA-20020: A comment is required for both AML Accept and Reject.");
        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20021: Invalid AML decision.");
        var task = portfolioId is null ? null : await steps.OpenTaskAsync(portfolioId.Value, "AML_REVIEW", taskId, ct);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20022: No open AML Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        var me = user.UserName ?? string.Empty;
        string newStatus;
        if (action == "REJECT")
        {
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "REJECT", comment, ct);
            newStatus = "AML_REJECTED";
            p.RejectionReason = comment;
        }
        else
        {
            if (!await db.Set<MtgAmlResult>().AnyAsync(a => a.PortfolioId == p.PortfolioId && a.SubjectType == "DEVELOPER" && a.SubjectId == p.DeveloperId, ct))
                throw new ApexProcessException("ORA-20023: AML information is required for the portfolio developer.");
            var accounts = db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId);
            if (await accounts.Select(a => a.InvestorId).Distinct()
                    .AnyAsync(i => !db.Set<MtgAmlResult>().Any(a => a.PortfolioId == p.PortfolioId && a.SubjectType == "INVESTOR" && a.SubjectId == i), ct))
                throw new ApexProcessException("ORA-20024: AML information is missing for one or more portfolio investors.");
            if (await accounts.AnyAsync(a => a.DealDecisionCode == null || (a.DealDecisionCode != "SELECTED" && a.DealDecisionCode != "EXCLUDED"), ct))
                throw new ApexProcessException("ORA-20025: Every investor must be either Selected or Excluded before AML can be accepted.");
            var selected = await accounts.Where(a => a.DealDecisionCode == "SELECTED")
                .GroupBy(_ => 1)
                .Select(g => new { Count = g.Count(), Face = g.Sum(a => a.ContractValue), Outstanding = g.Sum(a => a.OutstandingValue), Paid = g.Sum(a => a.PaidValue) })
                .FirstOrDefaultAsync(ct);
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct);
            p.SelectedAccountCount = selected?.Count ?? 0;
            p.SelectedFaceValue = selected?.Face ?? 0;
            p.SelectedOutstandingValue = selected?.Outstanding ?? 0;
            p.SelectedPaidValue = selected?.Paid ?? 0;
            newStatus = "AML_ACCEPTED";
            p.RejectionReason = null;
        }

        p.PortfolioStatusCode = newStatus;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangeAt = clock.UtcNow,
            ChangedBy = me,
            Comments = comment,
        });
        await db.SaveChangesAsync(ct);
        return action == "ACCEPT" ? "AML accepted successfully." : "AML rejected successfully.";
    }, ct);
}

/// <summary>App 105 page 209, Ajax Callback AML_UPDATE_INVESTOR: include or exclude an investor's contract.</summary>
public sealed class P0209AmlUpdateInvestor(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 209;

    public string Name => "AML_UPDATE_INVESTOR";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var accountId = MtgAjax.Long(request.X02);
        var decision = MtgAjax.Upper(request.X03);
        var portfolioId = MtgAjax.Long(request.Item("P209_PORTFOLIO_ID"));
        if (portfolioId is null)
            throw new ApexProcessException("ORA-20031: Portfolio ID is missing from page session state.");
        if (decision is not ("SELECTED" or "EXCLUDED"))
            throw new ApexProcessException("ORA-20011: Decision must be SELECTED or EXCLUDED.");
        var account = await db.Set<MtgPurchasedPortfolioAccount>().FirstOrDefaultAsync(a => a.PortfolioAccountId == accountId && a.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-20012: Portfolio investor account not found.");
        account.DealDecisionCode = decision;
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}
