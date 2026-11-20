using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 212 "Portfolio Coverage Review": the PL/SQL region "Coverage Review", ported.</summary>
public sealed class P0212CoverageReview(IApexSqlReader reader) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    public int App => 105;

    public int Page => 212;

    public string Name => "REGION_COVERAGE_REVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P212_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the Coverage Review task in your inbox.", "افتح هذه الصفحة من مهمة مراجعة التغطية في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P212_PORTFOLIO_ID");
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var req = head.Number("COVERAGE_PCT");
        var cur = head.Text("CURRENCY_CODE");
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P212_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);
        var editUrl = ApexPages.Url(105, 202, "P202_PORTFOLIO_ID", portfolioId, null);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,#cvApply,.cv-warn a,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"cvRoot\" data-inbox=\"" + Attr(inboxUrl) + "\" data-req=\"" + (req is not null ? P0203Html.Num(req) : string.Empty) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Coverage review", "مراجعة التغطية") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Coverage review", "مراجعة التغطية") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Compare how much each investor has paid against the required coverage, follow or override our recommendation, then accept or reject the review.", "قارن ما سدده كل مستثمر بنسبة التغطية المطلوبة، واتبع التوصية أو خالفها، ثم اقبل المراجعة أو ارفضها.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Required coverage", "التغطية المطلوبة") + "</span><strong>" + Pct(req) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Check required coverage", "مراجعة التغطية المطلوبة") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Review recommendations", "مراجعة التوصيات") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Decide deal inclusion", "تحديد الإدراج في الصفقة") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Accept or reject", "القبول أو الرفض") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\">");

        // required coverage + how it is calculated
        h.AppendLine("<section class=\"fxd-card\"><div class=\"cv-req\"><div class=\"cv-big" + (req is null ? " none" : string.Empty) + "\"><span><b>" + (req is null ? "&ndash;" : ToChar(req, "FM990D0") + "%") + "</b><small>" + T("Required", "المطلوب") + "</small></span></div>"
            + "<div class=\"cv-how\"><h3>" + T("How coverage is calculated", "كيف تُحسب التغطية") + "</h3><p>" + T("For each investor we add up all of their contracts in this portfolio and divide what they have paid by the total contract value.", "لكل مستثمر نجمع كل عقوده في هذه المحفظة ونقسم ما سدده على إجمالي قيمة العقود.")
            + " " + T("Investors below the required coverage are recommended for exclusion.", "يوصى باستبعاد المستثمرين الأقل من التغطية المطلوبة.") + "</p>"
            + "<div class=\"cv-formula\">" + T("Coverage", "التغطية") + " = <em>" + T("Paid value", "المبلغ المسدد") + "</em> &divide; <em>" + T("Contract value", "قيمة العقد") + "</em> &times; 100</div></div></div>");
        if (req is null)
        {
            h.AppendLine("<div class=\"cv-warn\"><span class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\"></span>" + T("No required coverage is set for this portfolio, so there are no recommendations yet.", "لم تُحدد نسبة التغطية المطلوبة لهذه المحفظة، لذلك لا توجد توصيات بعد.")
                + "<a href=\"" + Attr(editUrl) + "\">" + T("Set it on the portfolio", "حددها في بيانات المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"cv-spread\"><div class=\"fxd-label\" style=\"margin-bottom:22px\">" + T("Coverage spread", "توزيع التغطية") + "</div><div class=\"cv-track\" id=\"cvTrack\" style=\"--t:" + P0203Html.Num(Clamp(req)) + "%\">"
            + (req is not null ? "<div class=\"thr\"><span>" + ToChar(req, "FM990D0") + "%</span></div>" : string.Empty) + "</div><div class=\"cv-axis\"><span>0%</span><span>50%</span><span>100%</span></div></div></section>");

        // investors
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("Investors", "المستثمرون") + "</h2><span class=\"fxd-label\" id=\"cvCount\"></span></div>");
        h.AppendLine("<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wbQ\" placeholder=\"Search investor, code, contract or unit\" aria-label=\"Search investors\"></label>"
            + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Filter\" id=\"wbSeg\">"
            + "<button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"BELOW\" aria-pressed=\"false\">" + T("Below required", "أقل من المطلوب") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"MEETS\" aria-pressed=\"false\">" + T("Meets required", "يحقق المطلوب") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"AGAINST\" aria-pressed=\"false\">" + T("Against advice", "خلاف التوصية") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"EXCLUDED\" aria-pressed=\"false\">" + T("Excluded", "مستبعد") + "<em></em></button></div></div>");
        h.AppendLine("<div class=\"wb-bulk\" id=\"wbBulk\" hidden><span id=\"wbBulkText\"></span><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"cvApply\">" + T("Apply recommendations", "تطبيق التوصيات") + "</button></div><ul class=\"wb-rows\" id=\"wbRows\">");
        var n = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P212_INVESTORS, items, ct))
        {
            n++;
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE");
            var investorId = P0203Html.Num(r.Number("INVESTOR_ID"));
            decimal cv = r.Number("CV") ?? 0, pv = r.Number("PV") ?? 0, count = r.Number("N") ?? 0, exc = r.Number("EXC") ?? 0;
            decimal? cov = cv > 0 ? P0203Html.Round(pv / cv * 100, 2) : null;
            var rec = req is null || cov is null ? "NA" : cov < req ? "EXCLUDE" : "KEEP";
            var dec = exc == 0 ? "SELECTED" : exc == count ? "EXCLUDED" : "MIXED";
            var keys = (code + " " + en + " " + ar + " " + r.Text("KEYS")).ToLowerInvariant();
            h.AppendLine("<li class=\"wb-row cv" + (dec == "EXCLUDED" ? " is-out" : string.Empty) + "\" data-inv=\"" + investorId + "\" data-dec=\"" + dec + "\" data-rec=\"" + rec + "\" data-cov=\"" + P0203Html.Num(cov ?? 0) + "\""
                + " data-cv=\"" + P0203Html.Num(cv) + "\" data-pv=\"" + P0203Html.Num(pv) + "\" data-name=\"" + Attr(en ?? ar) + "\""
                + " data-q=\"" + Attr(keys.Length > 1500 ? keys[..1500] : keys) + "\">"
                + "<div class=\"wb-person\"><span class=\"wb-av\" style=\"background:" + Colors[(n - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div class=\"wb-who\"><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small style=\"font-family:Cairo,sans-serif;unicode-bidi:plaintext;text-align:start\">" + E(ar) + "</small>" : string.Empty)
                + "<div class=\"wb-codes\"><span class=\"wb-code\">" + E(code) + "</span><span class=\"wb-code\">" + P0203Html.Num(count) + " " + (count == 1 ? T("contract", "عقد") : T("contracts", "عقود")) + "</span></div></div></div>"
                + "<div><div class=\"cv-meter\" style=\"--v:" + P0203Html.Num(Clamp(cov)) + "%;--t:" + P0203Html.Num(Clamp(req)) + "%\"><i></i>" + (req is not null ? "<b></b>" : string.Empty) + "</div>"
                + "<div class=\"cv-val\"><b>" + Pct(cov) + "</b><span>" + Money(pv) + " " + T("of", "من") + " " + Money(cv) + "</span></div></div>"
                + "<div class=\"cv-rec\">" + rec switch
                {
                    "EXCLUDE" => "<span class=\"fxd-pill fxd-pill--bad\">" + T("Recommend: exclude", "التوصية: استبعاد") + "</span><small>" + ToChar(req - cov, "FM990D0") + " " + T("pts below required", "نقطة أقل من المطلوب") + "</small>",
                    "KEEP" => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Recommend: keep", "التوصية: إبقاء") + "</span><small>" + T("Meets required coverage", "يحقق التغطية المطلوبة") + "</small>",
                    _ => "<span class=\"fxd-pill fxd-pill--mute\">" + T("No recommendation", "لا توجد توصية") + "</span><small>" + (cov is null ? T("No contract value", "لا توجد قيمة عقد") : T("Required coverage not set", "التغطية المطلوبة غير محددة")) + "</small>",
                } + "</div>"
                + "<div><div class=\"wb-toggle\" role=\"group\" aria-label=\"Deal decision\"><button type=\"button\" class=\"in\" data-dec=\"SELECTED\" aria-pressed=\"" + (dec == "SELECTED" ? "true" : "false") + "\">" + T("Include", "إدراج") + "</button>"
                + "<button type=\"button\" class=\"out\" data-dec=\"EXCLUDED\" aria-pressed=\"" + (dec == "EXCLUDED" ? "true" : "false") + "\">" + T("Exclude", "استبعاد") + "</button></div><span class=\"wb-saved\">" + (dec == "MIXED" ? T("Partly excluded", "مستبعد جزئياً") : string.Empty) + "</span></div>"
                + "<details class=\"cv-more\"><summary>" + T("Contracts", "العقود") + " (" + P0203Html.Num(count) + ")</summary><div class=\"wb-tbl-wrap\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Contract", "العقد") + "</th><th>" + T("Unit", "الوحدة") + "</th>"
                + "<th class=\"fxd-num\">" + T("Contract value", "قيمة العقد") + "</th><th class=\"fxd-num\">" + T("Paid", "المسدد") + "</th><th class=\"fxd-num\">" + T("Coverage", "التغطية") + "</th><th>" + T("Decision", "القرار") + "</th></tr></thead><tbody>");
            foreach (var c in await reader.QueryAsync(MtgApexPageSql.P212_CONTRACTS, new Dictionary<string, string?>(items) { ["INVESTOR_ID"] = investorId }, ct))
            {
                decimal ccv = c.Number("CV") ?? 0, cpv = c.Number("PV") ?? 0;
                h.AppendLine("<tr><td><span class=\"wb-code\">" + E(c.Text("SELLER_CONTRACT_NO") ?? "-") + "</span></td><td>" + E(c.Text("UNIT_CODE") ?? "-") + "</td><td class=\"fxd-num\">" + ToChar(ccv, "FM999G999G999G990") + "</td>"
                    + "<td class=\"fxd-num\">" + ToChar(cpv, "FM999G999G999G990") + "</td><td class=\"fxd-num\">" + (ccv > 0 ? Pct(P0203Html.Round(cpv / ccv * 100, 2)) : "&ndash;") + "</td>"
                    + "<td>" + (c.Text("DEC") == "EXCLUDED" ? "<span class=\"fxd-pill fxd-pill--bad\">" + T("Excluded", "مستبعد") + "</span>" : "<span class=\"fxd-pill fxd-pill--ok\">" + T("Included", "مُدرج") + "</span>") + "</td></tr>");
            }

            h.AppendLine("</tbody></table></div></details></li>");
        }

        if (n == 0)
            h.AppendLine("<li class=\"wb-empty\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("This portfolio has no investor contracts yet.", "لا توجد عقود مستثمرين في هذه المحفظة بعد.") + "</li>");
        h.AppendLine("</ul><div class=\"wb-empty\" id=\"wbNoMatch\" hidden>" + T("No investors match this filter.", "لا يوجد مستثمرون مطابقون.") + "</div></section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Ready to accept?", "جاهزة للقبول؟") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/3</b>" + T("checks", "متطلبات") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Deal impact", "أثر القرار على الصفقة") + "</h2><span class=\"fxd-label\">" + E(cur) + "</span></div>"
            + "<div class=\"wb-impact\"><div><span class=\"fxd-label\">" + T("Contract value", "قيمة العقود") + "</span><b id=\"wbFace\">-</b><small id=\"wbFaceD\"></small></div><div><span class=\"fxd-label\">" + T("Portfolio coverage", "تغطية المحفظة") + "</span><b id=\"cvPf\">-</b><small id=\"cvPfD\"></small></div></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-mix\" role=\"img\"><i id=\"wbMixIn\" style=\"background:var(--teal-600)\"></i><i id=\"wbMixOut\" style=\"background:var(--bad)\"></i></div>"
            + "<div class=\"wb-legend\"><span style=\"--c:var(--teal-600)\" id=\"wbLIn\"></span><span style=\"--c:var(--bad)\" id=\"wbLOut\"></span></div></div></section></aside></div></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (required for accept and reject)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbReject\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject review", "رفض المراجعة") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbAccept\" disabled><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Accept & continue", "قبول ومتابعة") + "</button></div></div>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    /// <summary>pct(): to_char(p,'FM999G990D0')||'%'.</summary>
    private static string Pct(decimal? p) => p is null ? "&ndash;" : ToChar(p, "FM999G990D0") + "%";

    /// <summary>least(greatest(nvl(p,0),0),100).</summary>
    private static decimal Clamp(decimal? p) => Math.Min(Math.Max(p ?? 0, 0), 100);
}

/// <summary>App 105 page 212, the PL/SQL region "Step Actions".</summary>
public sealed class P0212StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 212;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(212, request, ct) };
}

/// <summary>App 105 page 212, Ajax Callback COV_APPLY: every investor's decision follows the coverage recommendation.</summary>
public sealed class P0212CovApply(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 212;

    public string Name => "COV_APPLY";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P212_PORTFOLIO_ID"));
        var req = await db.Set<MtgPurchasedPortfolio>().Where(p => p.PortfolioId == portfolioId).Select(p => new { p.CoveragePct }).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (req.CoveragePct is null)
            throw new ApexProcessException("ORA-20611: Set the required coverage percentage on the portfolio first.");
        var accounts = await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == portfolioId).ToListAsync(ct);
        var changed = 0;
        foreach (var g in accounts.GroupBy(a => a.InvestorId))
        {
            var cv = g.Sum(a => a.ContractValue);
            if (cv <= 0)
                continue;
            var decision = g.Sum(a => a.PaidValue) / cv * 100 < req.CoveragePct ? "EXCLUDED" : "SELECTED";
            var rows = g.Where(a => a.DealDecisionCode != decision).ToList();
            rows.ForEach(a => a.DealDecisionCode = decision);
            if (rows.Count > 0)
                changed++;
        }

        await db.SaveChangesAsync(ct);
        return new { success = true, changed };
    }, ct);
}

/// <summary>App 105 page 212, Ajax Callback COV_DECISION: accept (approve the task) or reject the Coverage Review.</summary>
public sealed class P0212CovDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 212;

    public string Name => "COV_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P212_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X02);
        var comment = MtgAjax.Trim(request.X03);
        if (comment is null)
            throw new ApexProcessException("ORA-20620: A comment is required for both Accept and Reject.");
        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20621: Invalid coverage review decision.");
        var newStatus = action == "ACCEPT" ? "COVERAGE_REVIEW_ACCEPTED" : "COVERAGE_REVIEW_REJECTED";
        if (!await db.Set<MtgLkpPortfolioStatus>().AnyAsync(s => s.StatusCode == newStatus, ct))
            throw new ApexProcessException("ORA-20625: Status " + newStatus + " is missing. Run the v67 database script (FINOVA_v67_COVERAGE_REVIEW.sql) first.");
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "COVERAGE", MtgAjax.Long(request.Item("P212_TASK_ID")), ct, like: true, exclude: "CHEQUE", page: 212);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20622: No open Coverage Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
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
            if (p.CoveragePct is null)
                throw new ApexProcessException("ORA-20623: Set the required coverage percentage on the portfolio before accepting.");
            var selected = await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId && (a.DealDecisionCode ?? "SELECTED") == "SELECTED")
                .GroupBy(_ => 1)
                .Select(g => new { Count = g.Count(), Face = g.Sum(a => a.ContractValue), Outstanding = g.Sum(a => a.OutstandingValue), Paid = g.Sum(a => a.PaidValue) })
                .FirstOrDefaultAsync(ct);
            if (selected is null || selected.Count == 0)
                throw new ApexProcessException("ORA-20624: At least one investor must stay in the deal before coverage can be accepted.");
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct) ?? p.WorkflowTaskId;
            p.SelectedAccountCount = selected.Count;
            p.SelectedFaceValue = selected.Face;
            p.SelectedOutstandingValue = selected.Outstanding;
            p.SelectedPaidValue = selected.Paid;
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
        return action == "ACCEPT" ? "Coverage review accepted." : "Coverage review rejected.";
    }, ct);
}

/// <summary>App 105 page 212, Ajax Callback COV_UPDATE_INVESTOR: include or exclude all of an investor's contracts.</summary>
public sealed class P0212CovUpdateInvestor(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 212;

    public string Name => "COV_UPDATE_INVESTOR";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var investorId = MtgAjax.Long(request.X02);
        var decision = MtgAjax.Upper(request.X03);
        var portfolioId = MtgAjax.Long(request.Item("P212_PORTFOLIO_ID"));
        if (portfolioId is null)
            throw new ApexProcessException("ORA-20601: Portfolio ID is missing from page session state.");
        if (decision is not ("SELECTED" or "EXCLUDED"))
            throw new ApexProcessException("ORA-20602: Decision must be SELECTED or EXCLUDED.");
        var rows = await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == portfolioId && a.InvestorId == investorId).ToListAsync(ct);
        if (rows.Count == 0)
            throw new ApexProcessException("ORA-20603: This investor has no contracts in the portfolio.");
        rows.ForEach(a => a.DealDecisionCode = decision);
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}
