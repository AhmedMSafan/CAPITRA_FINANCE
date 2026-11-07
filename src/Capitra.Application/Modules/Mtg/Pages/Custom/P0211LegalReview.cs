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

/// <summary>App 105 page 211 "Portfolio Legal Review": the PL/SQL region "Portfolio Legal Review", ported.</summary>
public sealed class P0211LegalReview(IApexSqlReader reader, ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexProcess
{
    private static readonly string[] Colors = ["#00636D", "#2E9AA3", "#C9620F", "#7A5AF8", "#014B55", "#0B8A57", "#B4520B", "#2463C7"];

    public int App => 105;

    public int Page => 211;

    public string Name => "REGION_PORTFOLIO_LEGAL_REVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P211_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the Legal Review task in your inbox.", "افتح هذه الصفحة من مهمة المراجعة القانونية في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P211_PORTFOLIO_ID");
        var pid = MtgAjax.Long(portfolioId);
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var project = head.Text("PROJECT");
        var lr = await db.Set<MtgLegalReview>().AsNoTracking().Where(x => x.PortfolioId == pid).OrderByDescending(x => x.LegalReviewId).FirstOrDefaultAsync(ct);
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P211_TASK_ID"), null);
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
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"wbLegal\" data-inbox=\"" + Attr(inboxUrl) + "\" data-has-review=\"" + (lr is null ? "N" : "Y") + "\" data-rdate=\"" + Iso(lr?.ReviewDate) + "\" data-adate=\"" + Iso(lr?.ApprovedDate) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Legal review", "المراجعة القانونية") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Legal review", "المراجعة القانونية") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Verify the six legal conditions, sign off the review, confirm which contracts stay in the deal, then accept or reject.", "تحقق من الشروط القانونية الستة، ووقّع المراجعة، وحدد العقود التي تبقى في الصفقة، ثم اقبل أو ارفض.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Project", "المشروع") + "</span><strong>" + E(project) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Legal checklist", "القائمة القانونية") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Sign-off", "الاعتماد") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Deal inclusion", "الإدراج في الصفقة") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Accept or reject", "القبول أو الرفض") + "</span></div></div></header>");

        // checklist + sign-off
        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\"><section class=\"fxd-card\" id=\"lrCard\">");
        h.AppendLine("<div class=\"fxd-card-h\"><h2><span class=\"fa fa-gavel\" aria-hidden=\"true\"></span>" + T("Legal checklist", "القائمة القانونية") + "</h2>"
            + "<div class=\"lr-prog\"><span id=\"lrProgT\"></span><span class=\"bar\"><i id=\"lrProgB\"></i></span></div></div><div class=\"lr-checks\" id=\"lrChecks\">");
        h.AppendLine(Chk("lrOwnership", lr?.IsOwnershipVerified, "fa-home", "Ownership verified", "الملكية موثقة", "The developer holds clear title to the land and units.", "المطور يملك الأرض والوحدات ملكية واضحة."));
        h.AppendLine(Chk("lrLicense", lr?.IsBuildingLicenseVerified, "fa-building", "Building license verified", "رخصة البناء موثقة", "Valid building permits cover the sold units.", "تراخيص بناء سارية تغطي الوحدات المباعة."));
        h.AppendLine(Chk("lrRegistration", lr?.IsRegistrationPossible, "fa-pencil-square-o", "Registration possible", "التسجيل ممكن", "Units can be registered in the buyers' names.", "يمكن تسجيل الوحدات بأسماء المشترين."));
        h.AppendLine(Chk("lrMortgage", lr?.IsMortgagePossible, "fa-university", "Mortgage possible", "الرهن ممكن", "A mortgage can be registered over the units.", "يمكن تسجيل رهن على الوحدات."));
        h.AppendLine(Chk("lrEncumbrance", lr?.IsEncumbranceFree, "fa-unlock-alt", "Free of encumbrances", "خالية من الأعباء", "No liens, seizures or third-party claims.", "لا رهونات أو حجوزات أو حقوق للغير."));
        h.AppendLine(Chk("lrTitle", lr?.IsTitleDeedVerified, "fa-file-text-o", "Title deed verified", "سند الملكية موثق", "The title deed was checked against the registry.", "تمت مطابقة سند الملكية مع السجل."));
        h.AppendLine("</div><div class=\"lr-form\"><div class=\"lr-2\">"
            + "<div class=\"wb-f\"><label for=\"lrFindings\">" + T("Legal findings", "الملاحظات القانونية") + "</label><textarea id=\"lrFindings\" rows=\"4\" placeholder=\"What did the legal team find?\">" + E(Cut(lr?.LegalFindings, 4000)) + "</textarea></div>"
            + "<div class=\"wb-f\"><label for=\"lrRecommendation\">" + T("Recommendation", "التوصية") + "</label><textarea id=\"lrRecommendation\" rows=\"4\" placeholder=\"Recommendation to the committee\">" + E(Cut(lr?.LegalRecommendation, 4000)) + "</textarea></div></div>");
        h.AppendLine("<div class=\"lr-sign\"><h3><span class=\"fa fa-pencil\" aria-hidden=\"true\"></span>" + T("Sign-off", "الاعتماد") + "</h3>"
            + "<div class=\"wb-f\"><label for=\"lrReviewedBy\">" + T("Reviewed by", "تمت المراجعة بواسطة") + "</label><input id=\"lrReviewedBy\" maxlength=\"255\" value=\"" + Attr(lr?.ReviewedBy ?? user.UserName?.ToLowerInvariant()) + "\"></div>"
            + "<div class=\"wb-f\"><label for=\"lrReviewDate\">" + T("Review date", "تاريخ المراجعة") + " *</label><input id=\"lrReviewDate\" type=\"date\" value=\"" + (lr?.ReviewDate is { } rd ? Iso(rd) : clock.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)) + "\"></div>"
            + "<div class=\"wb-f\"><label for=\"lrApprovedBy\">" + T("Approved by", "اعتمدها") + "</label><input id=\"lrApprovedBy\" maxlength=\"255\" value=\"" + Attr(lr?.ApprovedBy) + "\" placeholder=\"Legal manager\"></div>"
            + "<div class=\"wb-f\"><label for=\"lrApprovedDate\">" + T("Approval date", "تاريخ الاعتماد") + " *</label><input id=\"lrApprovedDate\" type=\"date\" value=\"" + Iso(lr?.ApprovedDate) + "\"></div></div></div>");
        h.AppendLine("<div class=\"lr-save\"><span class=\"lr-dirty\" id=\"lrDirty\"></span>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" id=\"lrReset\" hidden>" + T("Discard changes", "تجاهل التغييرات") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"lrSave\"><span class=\"fa fa-save\" aria-hidden=\"true\"></span>" + T("Save legal review", "حفظ المراجعة القانونية") + "</button></div></section>");

        // investors
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("Portfolio investors", "مستثمرو المحفظة") + "</h2><span class=\"fxd-label\" id=\"wbInCount\"></span></div>");
        h.AppendLine("<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wbQ\" placeholder=\"Search investor, code, contract or unit\" aria-label=\"Search investors\"></label>"
            + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Filter\" id=\"wbSeg\">"
            + "<button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"SELECTED\" aria-pressed=\"false\">" + T("Included", "مُدرج") + "<em></em></button>"
            + "<button type=\"button\" data-f=\"EXCLUDED\" aria-pressed=\"false\">" + T("Excluded", "مستبعد") + "<em></em></button></div></div><ul class=\"wb-rows\" id=\"wbRows\">");
        var n = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P211_ROWS, items, ct))
        {
            n++;
            string? en = r.Text("FULL_NAME_EN"), ar = r.Text("FULL_NAME_AR"), code = r.Text("INVESTOR_CODE"), contract = r.Text("SELLER_CONTRACT_NO"), unit = r.Text("UNIT_CODE");
            var dec = r.Text("DEC");
            decimal cv = r.Number("CV") ?? 0, pv = r.Number("PV") ?? 0, ov = r.Number("OV") ?? 0;
            h.AppendLine("<li class=\"wb-row lr" + (dec == "EXCLUDED" ? " is-out" : string.Empty) + "\" data-acc=\"" + P0203Html.Num(r.Number("PORTFOLIO_ACCOUNT_ID")) + "\" data-dec=\"" + dec + "\" data-cv=\"" + P0203Html.Num(cv) + "\" data-ov=\"" + P0203Html.Num(ov) + "\""
                + " data-q=\"" + Attr((code + " " + en + " " + ar + " " + contract + " " + unit).ToLowerInvariant()) + "\">"
                + "<div class=\"wb-person\"><span class=\"wb-av\" style=\"background:" + Colors[(n - 1) % 8] + "\">" + E(Initials(en ?? code)) + "</span><div class=\"wb-who\"><strong>" + E(en ?? ar) + "</strong>"
                + (en is not null && ar is not null ? "<small style=\"font-family:Cairo,sans-serif;direction:rtl;text-align:left\">" + E(ar) + "</small>" : string.Empty)
                + "<div class=\"wb-codes\"><span class=\"wb-code\">" + E(code) + "</span>" + (contract is not null ? "<span class=\"wb-code\">" + E(contract) + "</span>" : string.Empty)
                + (unit is not null ? "<span class=\"wb-code\">" + T("Unit", "وحدة") + " " + E(unit) + "</span>" : string.Empty) + "</div></div></div>"
                + "<div class=\"wb-money\"><b>" + Money(ov) + "</b><small>" + T("of", "من") + " " + Money(cv) + (cv > 0 ? " &middot; " + P0203Html.Num(P0203Html.Round(pv / cv * 100)) + "% " + T("paid", "مسدد") : string.Empty) + "</small></div>"
                + "<div><div class=\"wb-toggle\" role=\"group\" aria-label=\"Deal decision\"><button type=\"button\" class=\"in\" data-dec=\"SELECTED\" aria-pressed=\"" + (dec != "EXCLUDED" ? "true" : "false") + "\">" + T("Include", "إدراج") + "</button>"
                + "<button type=\"button\" class=\"out\" data-dec=\"EXCLUDED\" aria-pressed=\"" + (dec == "EXCLUDED" ? "true" : "false") + "\">" + T("Exclude", "استبعاد") + "</button></div><span class=\"wb-saved\"></span></div></li>");
        }

        if (n == 0)
            h.AppendLine("<li class=\"wb-empty\"><span class=\"fa fa-users\" aria-hidden=\"true\"></span>" + T("This portfolio has no investor contracts yet.", "لا توجد عقود مستثمرين في هذه المحفظة بعد.") + "</li>");
        h.AppendLine("</ul><div class=\"wb-empty\" id=\"wbNoMatch\" hidden>" + T("No contracts match this filter.", "لا توجد عقود مطابقة.") + "</div></section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Ready to accept?", "جاهزة للقبول؟") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/3</b>" + T("checks", "متطلبات") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Deal impact", "أثر القرار على الصفقة") + "</h2><span class=\"fxd-label\">" + T("live", "مباشر") + "</span></div>"
            + "<div class=\"wb-impact\"><div><span class=\"fxd-label\">" + T("Face value", "القيمة الاسمية") + "</span><b id=\"wbFace\">-</b><small id=\"wbFaceD\"></small></div><div><span class=\"fxd-label\">" + T("Outstanding", "القائم") + "</span><b id=\"wbOut\">-</b><small id=\"wbOutD\"></small></div></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-mix\" role=\"img\"><i id=\"wbMixIn\" style=\"background:var(--teal-600)\"></i><i id=\"wbMixOut\" style=\"background:var(--bad)\"></i></div>"
            + "<div class=\"wb-legend\"><span style=\"--c:var(--teal-600)\" id=\"wbLIn\"></span><span style=\"--c:var(--bad)\" id=\"wbLOut\"></span></div></div></section></aside></div></div>");

        // decision bar
        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (optional)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
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

    private static string Iso(DateOnly? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string? Cut(string? p, int length) => p is { } s && s.Length > length ? s[..length] : p;

    /// <summary>The region's chk(): a legal condition as a toggle button (data-saved is its saved Y/N).</summary>
    private static string Chk(string id, bool? value, string icon, string en, string ar, string den, string dar) =>
        "<button type=\"button\" class=\"lr-chk\" id=\"" + id + "\" data-saved=\"" + (value == true ? "Y" : "N") + "\" aria-pressed=\"" + (value == true ? "true" : "false") + "\">"
        + "<span class=\"ic fa " + icon + "\" aria-hidden=\"true\"></span><span><b>" + T(en, ar) + "</b><small>" + T(den, dar) + "</small>"
        + "<span class=\"st\"></span></span></button>";
}

/// <summary>App 105 page 211, the PL/SQL region "Step Actions".</summary>
public sealed class P0211StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 211;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(211, request, ct) };
}

/// <summary>App 105 page 211, Ajax Callback LEGAL_DECISION: accept (approve the task) or reject the Legal Review.</summary>
public sealed class P0211LegalDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 211;

    public string Name => "LEGAL_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P211_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X01);
        var comment = MtgAjax.Trim(request.X02);
        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20220: Invalid Legal Review decision.");
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "LEGAL", MtgAjax.Long(request.Item("P211_TASK_ID")), ct, like: true)
              ?? await steps.PageTaskAsync(portfolioId.Value, 211, ct);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20221: No open Legal Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        string newStatus;
        if (action == "REJECT")
        {
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "REJECT", comment, ct);
            newStatus = "LEGAL_REVIEW_REJECTED";
        }
        else
        {
            var lr = await db.Set<MtgLegalReview>().Where(x => x.PortfolioId == p.PortfolioId).OrderByDescending(x => x.LegalReviewId).FirstOrDefaultAsync(ct);
            if (lr is null || !(lr.IsOwnershipVerified && lr.IsBuildingLicenseVerified && lr.IsRegistrationPossible && lr.IsMortgagePossible && lr.IsEncumbranceFree && lr.IsTitleDeedVerified)
                || lr.ReviewDate is null || lr.ApprovedDate is null)
                throw new ApexProcessException("ORA-20222: Legal Review cannot be accepted until all six legal checks are Yes and both Review Date and Approved Date are completed.");
            var accounts = db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId);
            if (await accounts.AnyAsync(a => a.DealDecisionCode == null || (a.DealDecisionCode != "SELECTED" && a.DealDecisionCode != "EXCLUDED"), ct))
                throw new ApexProcessException("ORA-20224: Every investor must be either Selected or Excluded before Legal Review can be accepted.");
            var selected = await accounts.Where(a => a.DealDecisionCode == "SELECTED")
                .GroupBy(_ => 1)
                .Select(g => new { Count = g.Count(), Face = g.Sum(a => a.ContractValue), Outstanding = g.Sum(a => a.OutstandingValue), Paid = g.Sum(a => a.PaidValue) })
                .FirstOrDefaultAsync(ct);
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct);
            p.SelectedAccountCount = selected?.Count ?? 0;
            p.SelectedFaceValue = selected?.Face ?? 0;
            p.SelectedOutstandingValue = selected?.Outstanding ?? 0;
            p.SelectedPaidValue = selected?.Paid ?? 0;
            newStatus = "LEGAL_REVIEW_ACCEPTED";
        }

        p.PortfolioStatusCode = newStatus;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangeAt = clock.UtcNow,
            ChangedBy = user.UserName ?? string.Empty,
            Comments = comment ?? (action == "REJECT" ? "Legal Review rejected." : "Legal Review accepted."),
        });
        await db.SaveChangesAsync(ct);
        return new { success = true, message = action == "ACCEPT" ? "Legal Review accepted successfully." : "Legal Review rejected successfully.", newTaskId = p.WorkflowTaskId };
    }, ct);
}

/// <summary>App 105 page 211, Ajax Callback LEGAL_SAVE: the six checks, findings and sign-off of the portfolio's legal review.</summary>
public sealed class P0211LegalSave(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 211;

    public string Name => "LEGAL_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var reviewDate = Date(request.X10);
        var approvedDate = Date(request.X12);
        var portfolioId = MtgAjax.Long(request.Item("P211_PORTFOLIO_ID"));
        var lr = await db.Set<MtgLegalReview>().Where(x => x.PortfolioId == portfolioId).OrderByDescending(x => x.LegalReviewId).FirstOrDefaultAsync(ct);
        if (lr is null)
        {
            lr = new MtgLegalReview { PortfolioId = portfolioId ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"FNV_LEGAL_REVIEW\".\"PORTFOLIO_ID\")") };
            db.Set<MtgLegalReview>().Add(lr);
        }

        lr.IsOwnershipVerified = request.X01 == "Y";
        lr.IsBuildingLicenseVerified = request.X02 == "Y";
        lr.IsRegistrationPossible = request.X03 == "Y";
        lr.IsMortgagePossible = request.X04 == "Y";
        lr.IsEncumbranceFree = request.X05 == "Y";
        lr.IsTitleDeedVerified = request.X06 == "Y";
        lr.LegalFindings = MtgAjax.Trim(request.X07);
        lr.LegalRecommendation = MtgAjax.Trim(request.X08);
        lr.ReviewedBy = MtgAjax.Trim(request.X09);
        lr.ReviewDate = reviewDate;
        lr.ApprovedBy = MtgAjax.Trim(request.X11);
        lr.ApprovedDate = approvedDate;
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);

    private static DateOnly? Date(string? p) =>
        string.IsNullOrEmpty(p) ? null
        : DateOnly.TryParseExact(p, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : throw new ApexProcessException("ORA-01861: literal does not match format string");
}

/// <summary>App 105 page 211, Ajax Callback LEGAL_UPDATE_INVESTOR: include or exclude an investor's contract.</summary>
public sealed class P0211LegalUpdateInvestor(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 211;

    public string Name => "LEGAL_UPDATE_INVESTOR";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var accountId = MtgAjax.Long(request.X01);
        var decision = MtgAjax.Upper(request.X02);
        var portfolioId = MtgAjax.Long(request.Item("P211_PORTFOLIO_ID"));
        if (decision is not ("SELECTED" or "EXCLUDED"))
            throw new ApexProcessException("ORA-20210: Decision must be SELECTED or EXCLUDED.");
        var account = await db.Set<MtgPurchasedPortfolioAccount>().FirstOrDefaultAsync(a => a.PortfolioAccountId == accountId && a.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-20211: Portfolio investor account not found.");
        account.DealDecisionCode = decision;
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}
