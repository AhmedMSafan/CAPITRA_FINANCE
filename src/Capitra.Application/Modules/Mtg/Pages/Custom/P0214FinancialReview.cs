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

/// <summary>App 105 page 214 "Portfolio Financial Review": the PL/SQL region "Financial Review", ported.</summary>
public sealed class P0214FinancialReview(IApexSqlReader reader, ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 214;

    public string Name => "REGION_FINANCIAL_REVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P214_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the Financial Review task in your inbox.", "افتح هذه الصفحة من مهمة المراجعة المالية في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P214_PORTFOLIO_ID");
        var pid = MtgAjax.Long(portfolioId);
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var cur = head.Text("CURRENCY_CODE");
        var rate = head.Number("PV_RATE");
        var marg = head.Number("MARGIN_PERCENTAGE");
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P214_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);
        var sel = await reader.RowAsync(MtgApexPageSql.P214_SELECTED, items, ct);
        var units = sel?.Number("UNITS") ?? 0;
        var fr = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
        var pvm = fr?.PvMethod ?? "ANNUAL";
        var mb = fr?.MarginBase ?? "SELECTED_OUTSTANDING";
        var ab = fr?.AdminBase ?? "SELECTED_OUTSTANDING";
        var saved = fr is null ? null : MtgAjax.Cairo(fr.UpdatedAt ?? fr.CreatedAt)?.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);
        var now = MtgAjax.CairoNow(clock);
        var today = DateOnly.FromDateTime(now);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,.fr-acts,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,.fr-ch input,.fr-ch select,.fr-opt,#frPvm,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"frRoot\" data-inbox=\"" + Attr(inboxUrl) + "\" data-cur=\"" + E(cur) + "\" data-rate=\"" + (rate is not null ? P0203Html.Num(rate) : string.Empty) + "\""
            + " data-marg=\"" + P0203Html.Num(marg ?? 0) + "\" data-admp=\"" + P0203Html.Num(head.Number("ADMIN_EXPENSE_PERCENT") ?? 0) + "\" data-legal=\"" + P0203Html.Num(head.Number("LEGAL_FEE_PER_UNIT") ?? 0) + "\" data-valfee=\"" + P0203Html.Num(head.Number("VALUATION_FEE_PER_UNIT") ?? 0) + "\""
            + " data-out=\"" + P0203Html.Num(sel?.Number("OV") ?? 0) + "\" data-face=\"" + P0203Html.Num(sel?.Number("FACE") ?? 0) + "\" data-units=\"" + P0203Html.Num(units) + "\""
            + " data-pvm=\"" + pvm + "\" data-mb=\"" + mb + "\" data-ab=\"" + ab + "\" data-saved=\"" + E(saved) + "\""
            + " data-ac=\"" + Opt(fr?.AdminCalcAmount) + "\" data-af=\"" + Opt(fr?.AdminFinalAmount) + "\" data-ai=\"" + Yn(fr?.IsAdminIncluded) + "\""
            + " data-lc=\"" + Opt(fr?.LegalCalcAmount) + "\" data-lf=\"" + Opt(fr?.LegalFinalAmount) + "\" data-li=\"" + Yn(fr?.IsLegalIncluded) + "\""
            + " data-vc=\"" + Opt(fr?.ValuationCalcAmount) + "\" data-vf=\"" + Opt(fr?.ValuationFinalAmount) + "\" data-vi=\"" + Yn(fr?.IsValuationIncluded) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Financial review", "المراجعة المالية") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Financial review", "المراجعة المالية") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Value the investor cheques today, deduct the margin and the charges, and set the proposed purchase value of the portfolio.", "احسب القيمة الحالية لشيكات المستثمرين، واخصم الهامش والمصروفات، وحدد قيمة الشراء المقترحة للمحفظة.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Currency", "العملة") + "</span><strong>" + E(cur) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Present value", "القيمة الحالية") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Margin", "الهامش") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Charges", "المصروفات") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Decision", "القرار") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\">");

        // 1. present value
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Cheques present value", "القيمة الحالية للشيكات") + "</h2><span class=\"fxd-label\" id=\"frCount\"></span></div>"
            + "<div class=\"fr-tools\"><div class=\"wb-seg\" role=\"group\" aria-label=\"Present value method\" id=\"frPvm\">"
            + "<button type=\"button\" data-v=\"ANNUAL\" aria-pressed=\"false\">" + T("Annual compounding", "خصم سنوي مركب") + "</button>"
            + "<button type=\"button\" data-v=\"MONTHLY\" aria-pressed=\"false\">" + T("Monthly compounding", "خصم شهري مركب") + "</button>"
            + "<button type=\"button\" data-v=\"SIMPLE\" aria-pressed=\"false\">" + T("Simple discount", "خصم بسيط") + "</button></div>"
            + "<div class=\"fr-meta\"><span class=\"fr-chip\"><span class=\"fa fa-percent\" aria-hidden=\"true\"></span>" + T("Rate", "المعدل") + " " + (rate is null ? "&ndash;" : ToChar(rate, "FM990D00") + "%") + "</span>"
            + "<span class=\"fr-chip\"><span class=\"fa fa-calendar\" aria-hidden=\"true\"></span>" + T("As of", "بتاريخ") + " " + now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + "</span></div></div>"
            + "<div class=\"fr-formula\" id=\"frFormula\"></div>"
            + "<div class=\"fr-kpis\"><div><span>" + T("Cheques", "الشيكات") + "</span><b id=\"frN\">-</b></div><div><span>" + T("Face amount", "القيمة الاسمية") + "</span><b id=\"frFace\">-</b></div>"
            + "<div class=\"hi\"><span>" + T("Present value", "القيمة الحالية") + "</span><b id=\"frPv\">-</b></div><div><span>" + T("Discount", "الخصم") + "</span><b id=\"frDisc\">-</b></div></div>"
            + "<div class=\"fr-scroll\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Cheque no.", "رقم الشيك") + "</th><th>" + T("Due date", "تاريخ الاستحقاق") + "</th>"
            + "<th class=\"fxd-num\">" + T("Days", "الأيام") + "</th><th class=\"fxd-num\">" + T("Amount", "المبلغ") + "</th><th class=\"fxd-num\">" + T("Present value", "القيمة الحالية") + "</th></tr></thead><tbody id=\"frTb\">");
        var n = 0;
        foreach (var c in await reader.QueryAsync(MtgApexPageSql.P214_CHEQUES, items, ct))
        {
            n++;
            var due = c.Date("DUE_DATE");
            var (d, m) = FinancialReviewMath.Period(due, today);
            h.AppendLine("<tr data-amt=\"" + P0203Html.Num(c.Number("CHEQUE_AMOUNT") ?? 0) + "\" data-d=\"" + P0203Html.Num(d) + "\" data-m=\"" + P0203Html.Num(m) + "\"><td>" + E(c.Text("INV")) + "</td><td><span class=\"wb-code\">" + E(c.Text("CHEQUE_NO")) + "</span></td>"
                + "<td>" + E(due?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "-") + "</td><td class=\"fxd-num\">" + P0203Html.Num(d) + "</td><td class=\"fxd-num\">" + P0213ChequeCoverageReview.Amt(c.Number("CHEQUE_AMOUNT")) + "</td><td class=\"fxd-num\"><b class=\"fr-pv\"></b></td></tr>");
        }

        h.AppendLine("</tbody></table>" + (n == 0 ? "<div class=\"wb-empty\"><span class=\"fa fa-money\" aria-hidden=\"true\"></span>" + T("No cheques were entered for the included investors.", "لم يتم إدخال شيكات للمستثمرين المدرجين.") + "</div>" : string.Empty) + "</div></section>");

        // 2. margin
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-percent\" aria-hidden=\"true\"></span>" + T("Margin", "الهامش") + "</h2><span class=\"fr-chip\">" + ToChar(marg ?? 0, "FM990D00") + "%</span></div>"
            + "<div class=\"fr-base\" id=\"frMb\" role=\"group\" aria-label=\"Margin base\"><button type=\"button\" class=\"fr-opt\" data-v=\"SELECTED_OUTSTANDING\" aria-pressed=\"false\"><span>" + T("Selected outstanding", "الرصيد القائم المختار") + "</span><b id=\"frMbOut\"></b></button>"
            + "<button type=\"button\" class=\"fr-opt\" data-v=\"CHEQUES_PV\" aria-pressed=\"false\"><span>" + T("Cheques present value", "القيمة الحالية للشيكات") + "</span><b id=\"frMbPv\"></b></button></div>"
            + "<div class=\"fr-line\"><span id=\"frMarginTxt\"></span><b id=\"frMargin\"></b></div></section>");

        // 3. charges
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-file-text-o\" aria-hidden=\"true\"></span>" + T("Charges", "المصروفات") + "</h2><span class=\"fr-chip\">" + P0203Html.Num(units) + " " + T("units", "وحدات") + "</span></div>"
            + "<div class=\"wb-tbl-wrap\"><table class=\"wb-tbl fr-ch\"><thead><tr><th>" + T("Charge", "المصروف") + "</th><th class=\"fxd-num\">" + T("Calculated", "المحسوب") + "</th><th class=\"fxd-num\">" + T("Final amount", "المبلغ النهائي") + "</th><th>" + T("Deduct", "خصم") + "</th></tr></thead><tbody>"
            + "<tr data-ch=\"A\"><td><strong>" + T("Admin expense", "المصروفات الإدارية") + "</strong><small id=\"frAdminTxt\"></small><select id=\"frAb\" aria-label=\"Admin expense base\">"
            + Option("SELECTED_OUTSTANDING", ab, "of selected outstanding", "من الرصيد القائم المختار") + Option("SELECTED_FACE", ab, "of selected face value", "من القيمة الاسمية المختارة")
            + Option("CHEQUES_PV", ab, "of cheques present value", "من القيمة الحالية للشيكات") + Option("PROPOSED_PURCHASE", ab, "of price before charges", "من السعر قبل المصروفات") + "</select></td>"
            + "<td class=\"fxd-num\"><b class=\"fr-calc\"></b></td><td class=\"fxd-num\"><input class=\"fr-final\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" aria-label=\"Admin expense final amount\"></td>"
            + "<td><label class=\"fr-sw\"><input type=\"checkbox\" class=\"fr-inc\" aria-label=\"Deduct admin expense\"><i></i></label></td></tr>"
            + "<tr data-ch=\"L\"><td><strong>" + T("Legal fees", "الرسوم القانونية") + "</strong><small id=\"frLegalTxt\"></small></td>"
            + "<td class=\"fxd-num\"><b class=\"fr-calc\"></b></td><td class=\"fxd-num\"><input class=\"fr-final\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" aria-label=\"Legal fees final amount\"></td>"
            + "<td><label class=\"fr-sw\"><input type=\"checkbox\" class=\"fr-inc\" aria-label=\"Deduct legal fees\"><i></i></label></td></tr>"
            + "<tr data-ch=\"V\"><td><strong>" + T("Valuation expense", "مصروفات التقييم") + "</strong><small id=\"frValTxt\"></small></td>"
            + "<td class=\"fxd-num\"><b class=\"fr-calc\"></b></td><td class=\"fxd-num\"><input class=\"fr-final\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" aria-label=\"Valuation expense final amount\"></td>"
            + "<td><label class=\"fr-sw\"><input type=\"checkbox\" class=\"fr-inc\" aria-label=\"Deduct valuation expense\"><i></i></label></td></tr></tbody></table></div></section></div>");

        // right rail: price
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-calculator\" aria-hidden=\"true\"></span>" + T("Proposed purchase value", "قيمة الشراء المقترحة") + "</h2><span class=\"fxd-label\">" + E(cur) + "</span></div>"
            + "<ul class=\"fr-wf\"><li><span>" + T("Cheques present value", "القيمة الحالية للشيكات") + "</span><b id=\"frWPv\"></b></li><li class=\"minus\"><span>" + T("Margin", "الهامش") + "</span><b id=\"frWM\"></b></li>"
            + "<li class=\"minus\" id=\"frWAl\"><span>" + T("Admin expense", "المصروفات الإدارية") + "</span><b id=\"frWA\"></b></li><li class=\"minus\" id=\"frWLl\"><span>" + T("Legal fees", "الرسوم القانونية") + "</span><b id=\"frWL\"></b></li>"
            + "<li class=\"minus\" id=\"frWVl\"><span>" + T("Valuation expense", "مصروفات التقييم") + "</span><b id=\"frWV\"></b></li></ul>"
            + "<div class=\"fr-total\" id=\"frTotal\"><span>" + T("Proposed purchase value", "قيمة الشراء المقترحة") + "</span><b id=\"frPrice\">-</b></div>"
            + "<div class=\"fr-er\"><span>" + T("Revenue", "الإيراد") + "</span><b id=\"frEr\">-</b><small id=\"frErTxt\">" + T("= cheques face amount - proposed purchase value", "القيمة الاسمية للشيكات - قيمة الشراء المقترحة") + "</small>"
            + "<span class=\"fr-np\">" + T("Net profit (today's value)", "صافي الربح بقيمة اليوم") + "</span><b id=\"frNp\" class=\"fr-np\">-</b><small id=\"frNpTxt\"></small></div><p class=\"fr-cof\" id=\"frCof\"></p>"
            + "<div class=\"fr-acts\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" id=\"frRecalc\"><span class=\"fa fa-refresh\" aria-hidden=\"true\"></span>" + T("Recalculate", "إعادة الحساب") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"frSave\"><span class=\"fa fa-save\" aria-hidden=\"true\"></span>" + T("Save", "حفظ") + "</button></div><div class=\"fr-state\" id=\"frState\"></div></section></aside></div></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Decision comment\" placeholder=\"Decision comment (required for accept and reject)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbReject\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject", "رفض") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbAccept\"><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Accept & continue", "قبول ومتابعة") + "</button></div></div>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Opt(decimal? p) => p is null ? string.Empty : P0203Html.Num(p);

    private static string Yn(bool? p) => p == false ? "N" : "Y";

    /// <summary>The region's opt(): an option of the admin expense base, selected when it is the saved base.</summary>
    private static string Option(string value, string current, string en, string ar) =>
        "<option value=\"" + value + "\"" + (value == current ? " selected" : string.Empty) + " data-finova-en=\"" + en + "\" data-finova-ar=\"" + ar + "\">" + en + "</option>";
}

/// <summary>The present value formulas of the financial review (page 214 and its FR_SAVE process).</summary>
internal static class FinancialReviewMath
{
    /// <summary>greatest(trunc(nvl(DUE_DATE,today)) - today, 0) days and greatest(months_between(..), 0) months.</summary>
    public static (decimal Days, decimal Months) Period(DateOnly? due, DateOnly today)
    {
        var d = due ?? today;
        return (Math.Max(d.DayNumber - today.DayNumber, 0), Math.Max(MtgAjax.MonthsBetween(d, today), 0));
    }

    /// <summary>The present value of one cheque at rate r (a fraction), rounded to 2 places.</summary>
    public static decimal PresentValue(decimal amount, decimal r, string method, decimal days, decimal months)
    {
        if (r <= 0)
            return P0203Html.Round(amount, 2);
        var pv = method switch
        {
            "MONTHLY" => (double)amount / Math.Pow(1 + (double)r / 12, (double)months),
            "SIMPLE" => (double)(amount / (1 + r * days / 365)),
            _ => (double)amount / Math.Pow(1 + (double)r, (double)days / 365),
        };
        return P0203Html.Round((decimal)pv, 2);
    }
}

/// <summary>App 105 page 214, the PL/SQL region "Step Actions".</summary>
public sealed class P0214StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 214;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(214, request, ct) };
}

/// <summary>App 105 page 214, Ajax Callback FR_DECISION: accept (approve the task) or reject the Financial Review.</summary>
public sealed class P0214FrDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 214;

    public string Name => "FR_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P214_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X02);
        var comment = MtgAjax.Trim(request.X03);
        if (comment is null)
            throw new ApexProcessException("ORA-20820: A comment is required for the decision.");
        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20821: Invalid financial review decision.");
        var newStatus = action == "ACCEPT" ? "FINANCIAL_REVIEW_ACCEPTED" : "FINANCIAL_REVIEW_REJECTED";
        if (!await db.Set<MtgLkpPortfolioStatus>().AnyAsync(s => s.StatusCode == newStatus, ct))
            throw new ApexProcessException("ORA-20825: Status " + newStatus + " is missing. Run the v84 database script (FINOVA_v84_FINANCIAL_REVIEW.sql) first.");
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "FINANCIAL", MtgAjax.Long(request.Item("P214_TASK_ID")), ct, like: true, page: 214);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20822: No open Financial Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
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
            var fr = await db.Set<MtgPurchasedPortfolioFinancialReview>().FirstOrDefaultAsync(x => x.PortfolioId == p.PortfolioId, ct)
                ?? throw new ApexProcessException("ORA-20823: Save the financial review before completing the step.");
            if ((fr.ChequeCount ?? 0) == 0)
                throw new ApexProcessException("ORA-20824: There are no cheques to value for the included investors.");
            if ((fr.ProposedPurchaseValue ?? 0) <= 0)
                throw new ApexProcessException("ORA-20826: The proposed purchase value must be above zero.");
            p.WorkflowTaskId = await steps.DecideAsync(task.TaskId, "APPROVE", comment, ct) ?? p.WorkflowTaskId;
            p.RejectionReason = null;
            p.SubmittedAt = clock.UtcNow;
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
        return action == "ACCEPT" ? "Financial review completed." : "Financial review rejected.";
    }, ct);
}

/// <summary>
/// App 105 page 214, Ajax Callback FR_SAVE: values the included investors' cheques today, deducts the margin and the
/// charges, and stores the review and the proposed purchase value on the portfolio.
/// </summary>
public sealed class P0214FrSave(ICapitraDbContext db, IClock clock) : IApexProcess
{
    private static readonly string[] AdminBases = ["SELECTED_OUTSTANDING", "SELECTED_FACE", "CHEQUES_PV", "PROPOSED_PURCHASE"];

    public int App => 105;

    public int Page => 214;

    public string Name => "FR_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var pf = MtgAjax.Long(request.Item("P214_PORTFOLIO_ID"));
        var now = MtgAjax.CairoNow(clock);
        var today = DateOnly.FromDateTime(now);
        var pvm = MtgAjax.Upper(request.X02) ?? "ANNUAL";
        var mb = MtgAjax.Upper(request.X03) ?? "SELECTED_OUTSTANDING";
        var ab = MtgAjax.Upper(request.X04) ?? "SELECTED_OUTSTANDING";
        var ai = MtgAjax.Upper(request.X06) ?? "Y";
        var li = MtgAjax.Upper(request.X08) ?? "Y";
        var vi = MtgAjax.Upper(request.X10) ?? "Y";
        if (pf is null)
            throw new ApexProcessException("ORA-20801: Portfolio ID is missing from page session state.");
        if (pvm is not ("ANNUAL" or "MONTHLY" or "SIMPLE"))
            throw new ApexProcessException("ORA-20802: Unknown present value method.");
        if (mb is not ("SELECTED_OUTSTANDING" or "CHEQUES_PV"))
            throw new ApexProcessException("ORA-20803: Unknown margin base.");
        if (!AdminBases.Contains(ab))
            throw new ApexProcessException("ORA-20804: Unknown admin expense base.");
        if (ai is not ("Y" or "N") || li is not ("Y" or "N") || vi is not ("Y" or "N"))
            throw new ApexProcessException("ORA-20805: Deduct flags must be Y or N.");
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == pf, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var rate = p.PvRate;
        decimal marg = p.MarginPercentage ?? 0, admp = p.AdminExpensePercent ?? 0, legal = p.LegalFeePerUnit ?? 0, valfee = p.ValuationFeePerUnit ?? 0;
        var r = rate / 100;
        var included = db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == pf && (a.DealDecisionCode ?? "SELECTED") == "SELECTED");
        var accounts = await included.Select(a => new { a.OutstandingValue, a.ContractValue, a.UnitId }).ToListAsync(ct);
        var outstanding = accounts.Sum(a => a.OutstandingValue);
        var face = accounts.Sum(a => a.ContractValue);
        var units = accounts.Select(a => a.UnitId).Distinct().Count();

        // present value of every cheque of the included investors (same formulas as the page)
        var cheques = await db.Set<MtgPurchasedPortfolioCheque>().Where(c => c.PortfolioId == pf)
            .Join(included, c => c.PortfolioAccountId, a => a.PortfolioAccountId, (c, a) => new { c.ChequeAmount, c.DueDate }).ToListAsync(ct);
        decimal fv = 0, pv = 0;
        foreach (var c in cheques)
        {
            var (days, months) = FinancialReviewMath.Period(c.DueDate, today);
            fv += c.ChequeAmount;
            pv += FinancialReviewMath.PresentValue(c.ChequeAmount, r, pvm, days, months);
        }

        var mba = mb == "CHEQUES_PV" ? pv : outstanding;
        var m = P0203Html.Round(mba * marg / 100, 2);
        var aba = ab switch { "SELECTED_FACE" => face, "CHEQUES_PV" => pv, "PROPOSED_PURCHASE" => pv - m, _ => outstanding };
        var ac = P0203Html.Round(aba * admp / 100, 2);
        var lc = P0203Html.Round(legal * units, 2);
        var vc = P0203Html.Round(valfee * units, 2);
        var af = Final(request.X05) ?? ac;
        var lf = Final(request.X07) ?? lc;
        var vf = Final(request.X09) ?? vc;
        if (af < 0 || lf < 0 || vf < 0)
            throw new ApexProcessException("ORA-20807: Charges cannot be negative.");
        var tot = (ai == "Y" ? af : 0) + (li == "Y" ? lf : 0) + (vi == "Y" ? vf : 0);
        var price = P0203Html.Round(pv - m - tot, 2);
        var er = P0203Html.Round(fv - price, 2);   // expected return = what the cheques pay minus what we pay for the portfolio

        var fr = await db.Set<MtgPurchasedPortfolioFinancialReview>().FirstOrDefaultAsync(x => x.PortfolioId == pf, ct);
        if (fr is null)
        {
            fr = new MtgPurchasedPortfolioFinancialReview { PortfolioId = pf.Value };
            db.Set<MtgPurchasedPortfolioFinancialReview>().Add(fr);
        }

        fr.PvMethod = pvm;
        fr.PvRate = rate;
        fr.PvBaseDate = today;
        fr.ChequeCount = cheques.Count;
        fr.ChequesFaceAmount = fv;
        fr.ChequesPvAmount = pv;
        fr.MarginBase = mb;
        fr.MarginPercentage = marg;
        fr.MarginBaseAmount = mba;
        fr.MarginAmount = m;
        fr.AdminBase = ab;
        fr.AdminExpensePercent = admp;
        fr.AdminBaseAmount = aba;
        fr.AdminCalcAmount = ac;
        fr.AdminFinalAmount = af;
        fr.IsAdminIncluded = ai == "Y";
        fr.UnitsCount = units;
        fr.LegalFeePerUnit = legal;
        fr.LegalCalcAmount = lc;
        fr.LegalFinalAmount = lf;
        fr.IsLegalIncluded = li == "Y";
        fr.ValuationFeePerUnit = valfee;
        fr.ValuationCalcAmount = vc;
        fr.ValuationFinalAmount = vf;
        fr.IsValuationIncluded = vi == "Y";
        fr.TotalChargesAmount = tot;
        fr.ProposedPurchaseValue = price;
        fr.ExpectedReturnAmount = er;
        p.ProposedPurchaseValue = price;
        p.ExpectedReturnAmount = er;
        p.AdminExpenseAmount = ai == "Y" ? af : 0;
        await db.SaveChangesAsync(ct);
        return new { success = true, pv, price, expectedReturn = er, savedOn = now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) };
    }, ct);

    /// <summary>round(n(p), 2): an entered final amount, null when empty.</summary>
    private static decimal? Final(string? p) => MtgAjax.Number(p) is { } d ? P0203Html.Round(d, 2) : null;
}
