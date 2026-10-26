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

/// <summary>App 105 page 401 "Portfolio Valuation": the PL/SQL region "Portfolio Valuation", ported.</summary>
public sealed class P0401PortfolioValuation(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "REGION_PORTFOLIO_VALUATION";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P401_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the valuation task in your inbox.", "افتح هذه الصفحة من مهمة التقييم في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P401_PORTFOLIO_ID");
        var pno = head.Text("PORTFOLIO_NO");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var dev = head.Text("DEV");
        var project = head.Text("PROJECT");
        var face = head.Number("FACE_VALUE");
        var outstanding = head.Number("OUTSTANDING_VALUE");
        var cur = head.Text("CUR");

        // lists for the form (same sources as the old 402 LOVs)
        var types = string.Join(";", (await reader.QueryAsync(MtgApexPageSql.P401_TYPES, items, ct))
            .Select(r => r.Text("C") + "=" + Cut((r.Text("N") ?? string.Empty).Replace(';', ',').Replace('=', '-'), 90)));
        var units = new StringBuilder();
        foreach (var u in await reader.QueryAsync(MtgApexPageSql.P401_UNITS, items, ct))
        {
            if (units.Length < 30000)
                units.Append("<option value=\"" + P0203Html.Num(u.Number("UNIT_ID")) + "\">" + E(u.Text("D")) + "</option>");
        }

        var docs = new StringBuilder();
        foreach (var d in await reader.QueryAsync(MtgApexPageSql.P401_DOCS, items, ct))
        {
            if (docs.Length < 30000)
                docs.Append("<option value=\"" + P0203Html.Num(d.Number("DOCUMENT_ID")) + "\">" + E(d.Text("D")) + "</option>");
        }

        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P401_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,#wbAddVal,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"wbVal\" data-inbox=\"" + Attr(inboxUrl) + "\" data-face=\"" + P0203Html.Num(face ?? 0) + "\" data-out=\"" + P0203Html.Num(outstanding ?? 0) + "\" data-ppv=\"" + P0203Html.Num(head.Number("PROPOSED_PURCHASE_VALUE") ?? 0) + "\" data-cur=\"" + E(cur) + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Valuation", "التقييم") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Portfolio valuation", "تقييم المحفظة") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Record the independent valuations for this portfolio, attach each valuation report, check the values against the deal, then complete this workflow step.", "سجّل التقييمات المستقلة لهذه المحفظة وأرفق تقرير كل تقييم وراجع القيم مقابل الصفقة ثم أكمل خطوة سير العمل.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Developer", "المطور") + "</span><strong>" + E(dev) + "</strong></div>"
            + "<div><span>" + T("Project", "المشروع") + "</span><strong>" + E(project) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\" id=\"wbSteps\"><div class=\"wb-step\"><b>1</b><span>" + T("Record valuations", "تسجيل التقييمات") + "</span></div><div class=\"wb-step\"><b>2</b><span>" + T("Set recommended values", "تحديد القيم الموصى بها") + "</span></div>"
            + "<div class=\"wb-step\"><b>3</b><span>" + T("Attach reports", "إرفاق التقارير") + "</span></div><div class=\"wb-step\"><b>4</b><span>" + T("Complete step", "إكمال الخطوة") + "</span></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap wb\"><div class=\"wb-grid\"><div class=\"wb-stack\">");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Valuations", "التقييمات") + "</h2>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" style=\"height:36px\" id=\"wbAddVal\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("Add valuation", "إضافة تقييم") + "</button></div>");
        h.AppendLine("<div class=\"wb-tbl-wrap\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Valuation", "التقييم") + "</th><th>" + T("Scope", "النطاق") + "</th><th>" + T("Date", "التاريخ") + "</th>"
            + "<th style=\"text-align:end\">" + T("Market / recommended", "السوقية / الموصى بها") + "</th><th>" + T("Completion", "الإنجاز") + "</th><th>" + T("Report", "التقرير") + "</th><th></th></tr></thead><tbody id=\"wbValRows\">");
        var n = 0;
        foreach (var v in await reader.QueryAsync(MtgApexPageSql.P401_VALUATIONS, items, ct))
        {
            n++;
            var id = P0203Html.Num(v.Number("VALUATION_ID"));
            var mv = v.Number("MARKET_VALUE");
            var rv = v.Number("RECOMMENDED_VALUE");
            var pct = v.Number("PROJECT_COMPLETION_PCT");
            var reg = v.Text("VALUER_REGISTRATION_NO");
            var unit = v.Text("UNIT_CODE");
            var approved = v.Date("APPROVED_DATE");
            var hasDoc = v.Number("REPORT_DOCUMENT_ID") is not null;
            h.AppendLine("<tr data-id=\"" + id + "\" data-mv=\"" + P0203Html.Num(mv ?? 0) + "\" data-rv=\"" + (rv is null ? string.Empty : P0203Html.Num(rv)) + "\""
                + " data-doc=\"" + (hasDoc ? "Y" : "N") + "\" data-pct=\"" + P0203Html.Num(pct ?? 0) + "\" data-date=\"" + v.Date("VALUATION_DATE")?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\">"
                + "<td><strong>" + E(v.Text("VTYPE")) + "</strong><small>" + E(v.Text("VALUER_NAME")) + (reg is not null ? " &middot; " + E(reg) : string.Empty) + "</small></td>"
                + "<td>" + (unit is null ? T("Whole project", "المشروع بالكامل") : T("Unit", "وحدة") + " <span class=\"wb-code\">" + E(unit) + "</span>") + "</td>"
                + "<td class=\"fxd-num\">" + (v.Date("VALUATION_DATE")?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "&ndash;") + "</td>"
                + "<td><div class=\"vl-val\"><b>" + Money(mv) + "</b><small>" + (rv is null ? T("No recommended value", "لا توجد قيمة موصى بها") : Money(rv) + " " + T("recommended", "موصى بها")) + "</small></div></td>"
                + "<td>" + (pct is null ? "&ndash;" : "<div class=\"vl-pct\"><i style=\"--w:" + P0203Html.Num(Math.Min(100, Math.Max(0, pct.Value))) + "%\"></i><span>" + P0203Html.Num(P0203Html.Round(pct.Value)) + "%</span></div>") + "</td>"
                + "<td>" + (hasDoc ? "<span class=\"vl-doc\"><span class=\"fa fa-file-text-o\" aria-hidden=\"true\"></span>" + T("Attached", "مرفق") + "</span>"
                    : "<span class=\"vl-doc no\"><span class=\"fa fa-exclamation-circle\" aria-hidden=\"true\"></span>" + T("Missing", "غير مرفق") + "</span>")
                + (approved is not null ? "<small>" + T("Approved", "معتمد") + " " + approved.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + "</small>" : string.Empty) + "</td>"
                + "<td style=\"text-align:end\"><button type=\"button\" class=\"wb-icon-btn\" data-edit=\"" + id + "\" aria-label=\"Edit\"><span class=\"fa fa-pencil\" aria-hidden=\"true\"></span></button> "
                + "<button type=\"button\" class=\"wb-icon-btn del\" data-del=\"" + id + "\" aria-label=\"Delete\"><span class=\"fa fa-trash-o\" aria-hidden=\"true\"></span></button></td></tr>");
        }

        h.AppendLine("</tbody></table></div>");
        if (n == 0)
            h.AppendLine("<div class=\"wb-empty\"><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("No valuations recorded yet. Start with Add valuation.", "لا توجد تقييمات بعد. ابدأ بإضافة تقييم.") + "</div>");
        h.AppendLine("</section></div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\"><section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-tasks\" aria-hidden=\"true\"></span>" + T("Progress", "التقدم") + "</h2><span id=\"wbReadyPill\"></span></div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" id=\"wbRing\"><span><span><b id=\"wbRingN\">0/3</b>" + T("done", "مكتمل") + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\" id=\"wbRingText\"></p></div>"
            + "<ul class=\"wb-checks\" id=\"wbChecks\"></ul></div></section>");
        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-balance-scale\" aria-hidden=\"true\"></span>" + T("Value check", "مراجعة القيمة") + "</h2><span class=\"fxd-label\">" + E(cur) + "</span></div>"
            + "<div class=\"vl-kpis\"><div><b id=\"vlMv\">-</b><small>" + T("Total market value", "إجمالي القيمة السوقية") + "</small></div><div><b id=\"vlRv\">-</b><small>" + T("Total recommended", "إجمالي الموصى به") + "</small></div>"
            + "<div><b>" + Money(outstanding) + "</b><small>" + T("Portfolio outstanding", "القائم في المحفظة") + "</small></div><div><b>" + Money(face) + "</b><small>" + T("Portfolio face value", "القيمة الاسمية للمحفظة") + "</small></div></div>"
            + "<div class=\"vl-cover\"><span class=\"fxd-label\">" + T("Recommended value covers outstanding", "تغطية القيمة الموصى بها للقائم") + "</span><div class=\"bar\"><i id=\"vlCovBar\"></i></div><p id=\"vlCovText\"></p></div></section></aside></div></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Completion comment\" placeholder=\"Completion comment (optional)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<a class=\"fxd-btn fxd-btn--ghost\" href=\"" + Attr(taskUrl) + "\">" + T("Cancel", "إلغاء") + "</a>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbComplete\"><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Complete valuation step", "إكمال خطوة التقييم") + "</button></div></div>");

        // valuation drawer
        h.AppendLine("<aside class=\"wb-drawer\" id=\"wbDrawer\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbDrT\" aria-hidden=\"true\"><div class=\"wb-dr-h\"><span class=\"wb-av\"><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span></span><div class=\"wb-who\"><strong id=\"wbDrT\"></strong><small>" + E(pno) + "</small></div>"
            + "<button type=\"button\" class=\"wb-x\" data-close-drawer aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></div><div class=\"wb-dr-b\"><input type=\"hidden\" id=\"wbValId\">"
            + "<div class=\"wb-f\"><span class=\"wb-f-l\">" + T("Valuation type", "نوع التقييم") + " *</span>" + Chips("wbVType", types) + "</div>"
            + "<div class=\"wb-fgrid\"><div class=\"wb-f wide\"><label for=\"wbUnit\">" + T("Scope", "النطاق") + "</label><select id=\"wbUnit\"><option value=\"\">" + T("Whole project", "المشروع بالكامل") + "</option>");
        h.AppendLine(units.ToString());
        h.AppendLine("</select></div>"
            + "<div class=\"wb-f\"><label for=\"wbValuer\">" + T("Valuer name", "اسم المقيّم") + " *</label><input id=\"wbValuer\" maxlength=\"300\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbReg\">" + T("Valuer registration no.", "رقم قيد المقيّم") + "</label><input id=\"wbReg\" maxlength=\"100\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbDate\">" + T("Valuation date", "تاريخ التقييم") + " *</label><input id=\"wbDate\" type=\"date\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbPct\">" + T("Project completion %", "نسبة إنجاز المشروع") + "</label><input id=\"wbPct\" type=\"number\" min=\"0\" max=\"100\" step=\"0.01\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbMv\">" + T("Market value", "القيمة السوقية") + " *</label><input id=\"wbMv\" type=\"number\" min=\"0\" step=\"0.01\" inputmode=\"decimal\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbRv\">" + T("Recommended value", "القيمة الموصى بها") + "</label><input id=\"wbRv\" type=\"number\" min=\"0\" step=\"0.01\" inputmode=\"decimal\"></div>"
            + "<div class=\"wb-f wide\"><label for=\"wbFac\">" + T("Facilities status", "حالة المرافق") + "</label><input id=\"wbFac\" maxlength=\"500\"></div>"
            + "<div class=\"wb-f wide\"><label for=\"wbDoc\">" + T("Valuation report", "تقرير التقييم") + "</label><select id=\"wbDoc\"><option value=\"\">" + T("Not attached yet", "لم يُرفق بعد") + "</option>");
        h.AppendLine(docs.ToString());
        h.AppendLine("</select></div>"
            + "<div class=\"wb-f\"><label for=\"wbAppBy\">" + T("Approved by", "اعتمد بواسطة") + "</label><input id=\"wbAppBy\" maxlength=\"255\"></div>"
            + "<div class=\"wb-f\"><label for=\"wbAppDate\">" + T("Approved date", "تاريخ الاعتماد") + "</label><input id=\"wbAppDate\" type=\"date\"></div></div></div>"
            + "<div class=\"wb-dr-f\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-close-drawer>" + T("Cancel", "إلغاء") + "</button><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"wbSave\">" + T("Save valuation", "حفظ التقييم") + "</button></div></aside>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }

    private static string Cut(string p, int length) => p.Length > length ? p[..length] : p;

    /// <summary>The region's chips(): a "CODE=Label;..." list as a chip group.</summary>
    private static string Chips(string id, string? list)
    {
        var o = new StringBuilder();
        foreach (var item in (list ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = item.IndexOf('=', StringComparison.Ordinal);
            o.Append("<button type=\"button\" data-v=\"" + (eq < 0 ? string.Empty : item[..eq]) + "\" aria-pressed=\"false\">" + E(Cut(item[(eq + 1)..], 100)) + "</button>");
        }

        return "<div class=\"wb-chips\" id=\"" + id + "\">" + o + "</div>";
    }
}

/// <summary>
/// App 105 page 401, the PL/SQL region "Step Actions": the Complete button follows the Allowed Actions of the step,
/// COMPLETE before APPROVE (the button reads "Approve valuation step" when the step only approves).
/// </summary>
public sealed class P0401StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (await steps.ActionsAsync(401, request, ct) is not { } codes)
            return new { ok = true, html = string.Empty };
        var pos = codes.Contains("COMPLETE") ? "COMPLETE" : codes.Contains("APPROVE") ? "APPROVE" : "NONE";
        var html = pos switch
        {
            "NONE" => "<style>#wbComplete{display:none!important}</style><script>(function(){var x=document.getElementById('wbComplete');var h=(x&&x.parentNode)||document.body;var s=document.createElement('span');s.className='wb-hint';s.style.cssText='color:var(--bad,#C43325);display:block;margin-top:8px';s.textContent='No Complete or Approve action is available for this task in the workflow setup (or the task is closed).';h.appendChild(s);})();</script>\n",
            "APPROVE" => "<script>(function(){var b=document.getElementById('wbComplete');if(!b)return;var s=b.querySelector('[data-finova-en]');if(!s)return;"
                + "s.setAttribute('data-finova-en','Approve valuation step');s.setAttribute('data-finova-ar','اعتماد خطوة التقييم');"
                + "s.textContent=document.documentElement.dir==='rtl'?'اعتماد خطوة التقييم':'Approve valuation step';})();</script>\n",
            _ => string.Empty,
        };
        return new { ok = true, html };
    }
}

/// <summary>App 105 page 401, Ajax Callback VAL_COMPLETE: completes (or approves) the Valuation task.</summary>
public sealed class P0401ValComplete(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "VAL_COMPLETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P401_PORTFOLIO_ID"));
        var comment = MtgAjax.Trim(request.X01);
        if (!await db.Set<MtgValuation>().AnyAsync(v => v.PortfolioId == portfolioId, ct))
            throw new ApexProcessException("ORA-20420: Record at least one valuation before completing the valuation step.");

        // find this portfolio's open valuation task (by step code, else by the task's target page)
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "VALUAT", MtgAjax.Long(request.Item("P401_TASK_ID")), ct, like: true, page: 401);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20421: No open Valuation task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);

        // COMPLETE when the step allows it, otherwise APPROVE (as set in the workflow designer)
        var newTaskId = await steps.DecideAsync(task.TaskId, "COMPLETE", comment, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        var known = await db.Set<MtgLkpPortfolioStatus>().AnyAsync(s => s.StatusCode == "VALUATION_COMPLETED", ct);
        if (known)
            p.PortfolioStatusCode = "VALUATION_COMPLETED";
        p.WorkflowTaskId = newTaskId ?? p.WorkflowTaskId;
        if (known && oldStatus != "VALUATION_COMPLETED")
        {
            db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
            {
                PortfolioId = p.PortfolioId,
                FromStatus = oldStatus,
                ToStatus = "VALUATION_COMPLETED",
                ChangeAt = clock.UtcNow,
                ChangedBy = user.UserName ?? string.Empty,
                Comments = comment ?? "Valuation step completed.",
            });
        }

        await db.SaveChangesAsync(ct);
        return new { success = true, newTaskId };
    }, ct);
}

/// <summary>App 105 page 401, Ajax Callback VAL_DELETE.</summary>
public sealed class P0401ValDelete(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "VAL_DELETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var id = MtgAjax.Long(request.X01);
        var portfolioId = MtgAjax.Long(request.Item("P401_PORTFOLIO_ID"));
        var row = await db.Set<MtgValuation>().FirstOrDefaultAsync(v => v.ValuationId == id && v.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-20411: Valuation record not found.");
        db.Set<MtgValuation>().Remove(row);
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);
}

/// <summary>App 105 page 401, Ajax Callback VAL_GET: one valuation of the portfolio, for the drawer ({} when it is not found).</summary>
public sealed class P0401ValGet(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "VAL_GET";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            var id = MtgAjax.Long(request.X01);
            var portfolioId = MtgAjax.Long(request.Item("P401_PORTFOLIO_ID"));
            var r = await db.Set<MtgValuation>().AsNoTracking().FirstOrDefaultAsync(v => v.ValuationId == id && v.PortfolioId == portfolioId, ct);
            if (r is null)
                return new Dictionary<string, object?>();
            return new
            {
                valuationId = r.ValuationId,
                valuationTypeCode = r.ValuationTypeCode,
                unitId = r.UnitId,
                valuerName = r.ValuerName,
                valuerRegistrationNo = r.ValuerRegistrationNo,
                valuationDate = r.ValuationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                marketValue = r.MarketValue is null ? null : P0203Html.Num(r.MarketValue),
                recommendedValue = r.RecommendedValue is null ? null : P0203Html.Num(r.RecommendedValue),
                projectCompletionPct = r.ProjectCompletionPct is null ? null : P0203Html.Num(r.ProjectCompletionPct),
                facilitiesStatus = r.FacilitiesStatus,
                reportDocumentId = r.ReportDocumentId,
                approvedBy = r.ApprovedBy,
                approvedDate = r.ApprovedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            };
        }
        catch (ApexProcessException e)
        {
            return new { success = false, message = e.Message };
        }
    }
}

/// <summary>App 105 page 401, Ajax Callback VAL_SAVE: adds or updates a valuation of the portfolio.</summary>
public sealed class P0401ValSave(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 401;

    public string Name => "VAL_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var id = MtgAjax.Long(request.X01);
        var type = MtgAjax.Upper(request.X02);
        var unit = MtgAjax.Long(request.X03);
        var doc = MtgAjax.Long(request.X11);
        var date = Date(request.X06);
        var appDate = Date(request.X13);
        var mv = MtgAjax.Number(request.X07);
        var rv = MtgAjax.Number(request.X08);
        var pct = MtgAjax.Number(request.X09);
        var portfolioId = MtgAjax.Long(request.Item("P401_PORTFOLIO_ID"));
        if (type is null)
            throw new ApexProcessException("ORA-20401: Valuation type is required.");
        if (MtgAjax.Trim(request.X04) is not { } valuer)
            throw new ApexProcessException("ORA-20402: Valuer name is required.");
        if (date is null)
            throw new ApexProcessException("ORA-20403: Valuation date is required.");
        if (mv is null or <= 0)
            throw new ApexProcessException("ORA-20404: Market value must be greater than zero.");
        if (rv is < 0)
            throw new ApexProcessException("ORA-20405: Recommended value cannot be negative.");
        if (pct is < 0 or > 100)
            throw new ApexProcessException("ORA-20406: Project completion % must be between 0 and 100.");
#pragma warning disable CA1862 // upper() runs in the database
        type = await db.Set<MtgLkpValuationType>().Where(t => t.ValuationTypeCode.ToUpper() == type).Select(t => t.ValuationTypeCode).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-20407: Invalid valuation type: it is not defined in Valuation Types.");
#pragma warning restore CA1862
        var projectId = await db.Set<MtgPurchasedPortfolio>().Where(p => p.PortfolioId == portfolioId).Select(p => (long?)p.ProjectId).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (unit is not null && !await db.Set<MtgProjectUnit>().AnyAsync(u => u.UnitId == unit && u.ProjectId == projectId, ct))
            throw new ApexProcessException("ORA-20408: The selected unit does not belong to this portfolio's project.");
        if (doc is not null && !await db.Set<MtgPurchasedPortfolioDocument>().AnyAsync(d => d.DocumentId == doc && d.PortfolioId == portfolioId, ct))
            throw new ApexProcessException("ORA-20409: The selected report does not belong to this portfolio.");
        MtgValuation row;
        if (id is null)
        {
            row = new MtgValuation { PortfolioId = portfolioId!.Value, ProjectId = projectId };
            db.Set<MtgValuation>().Add(row);
        }
        else
        {
            row = await db.Set<MtgValuation>().FirstOrDefaultAsync(v => v.ValuationId == id && v.PortfolioId == portfolioId, ct)
                ?? throw new ApexProcessException("ORA-20410: Valuation record not found.");
        }

        row.UnitId = unit;
        row.ValuationTypeCode = type;
        row.ValuerName = valuer;
        row.ValuerRegistrationNo = MtgAjax.Trim(request.X05);
        row.ValuationDate = date;
        row.MarketValue = mv;
        row.RecommendedValue = rv;
        row.ProjectCompletionPct = pct;
        row.FacilitiesStatus = MtgAjax.Trim(request.X10);
        row.ReportDocumentId = doc;
        row.ApprovedBy = MtgAjax.Trim(request.X12);
        row.ApprovedDate = appDate;
        await db.SaveChangesAsync(ct);
        return null;
    }, ct);

    private static DateOnly? Date(string? p) =>
        string.IsNullOrEmpty(p) ? null
        : DateOnly.TryParseExact(p, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : throw new ApexProcessException("ORA-01861: literal does not match format string");
}
