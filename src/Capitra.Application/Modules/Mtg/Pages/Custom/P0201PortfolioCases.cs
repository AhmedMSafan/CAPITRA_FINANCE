using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 201 "Portfolio Cases": the PL/SQL region "Portfolio Case Board", ported.</summary>
public sealed class P0201PortfolioCaseBoard(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 201;

    public string Name => "REGION_PORTFOLIO_CASE_BOARD";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var totals = await reader.RowAsync(MtgApexPageSql.P201_TOTALS, items, ct);
        var total = totals?.Number("TOTAL") ?? 0;
        var wf = totals?.Number("WF") ?? 0;
        var ready = totals?.Number("READY") ?? 0;
        var face = totals?.Number("FACE") ?? 0;
        var outstanding = totals?.Number("OUTSTANDING") ?? 0;
        var statuses = await reader.QueryAsync(MtgApexPageSql.P201_STATUSES, items, ct);
        var cases = await reader.QueryAsync(MtgApexPageSql.P201_CASES, items, ct);

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"wcBoard\">");
        h.AppendLine("<header class=\"fxd-hero\"><div class=\"wc-hero-grid\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workspace", "مساحة عمل المحافظ المشتراة") + "</div>"
            + "<h1 class=\"fxd-h1\">" + E(request.Item("P201_DEVELOPER_NAME")) + "</h1><p class=\"fxd-lead\">" + T("Every acquisition case for this developer, where it stands in the workflow and how ready it is to submit.", "كل ملفات الاستحواذ لهذا المطور وموقعها في سير العمل ومدى جاهزيتها للإرسال.") + "</p>"
            + "<div class=\"wc-dev\"><span><b>" + E(request.Item("P201_DEVELOPER_CODE")) + "</b></span><span class=\"sep\"></span><span>" + E(request.Item("P201_PROJECTS")) + "</span><span class=\"sep\"></span>"
            + "<a href=\"" + Url(7) + "\"><span class=\"fa fa-exchange\" aria-hidden=\"true\"></span>" + T("Change developer", "تغيير المطور") + "</a></div>"
            + "<div class=\"wc-cta\"><a class=\"fxd-btn fxd-btn--primary\" href=\"" + Url(202) + "\"><span class=\"fa fa-plus\" aria-hidden=\"true\"></span>" + T("New portfolio", "محفظة جديدة") + "</a>"
            + "<a class=\"fxd-btn fxd-btn--glass\" href=\"" + Url(920) + "\"><span class=\"fa fa-inbox\" aria-hidden=\"true\"></span>" + T("Workflow inbox", "صندوق المهام") + "</a></div></div>");
        h.AppendLine("<div class=\"wc-kpis fxd-anim d1\"><div class=\"wc-kpi\"><b>" + Num(total) + "</b><span>" + T("Cases", "الملفات") + " &middot; <small>" + Num(wf) + " " + T("in workflow", "في سير العمل") + "</small></span></div>"
            + "<div class=\"wc-kpi\"><b>" + Num(ready) + "</b><span>" + T("Ready to submit", "جاهزة للإرسال") + "</span></div>"
            + "<div class=\"wc-kpi\"><b>" + Money(face) + "</b><span>" + T("Total face value", "إجمالي القيمة الاسمية") + "</span></div>"
            + "<div class=\"wc-kpi\"><b>" + Money(outstanding) + "</b><span>" + T("Total outstanding", "إجمالي القائم") + "</span></div></div></div></header>");

        h.AppendLine("<div class=\"fxd-wrap\">");
        // status pipeline
        h.AppendLine("<section class=\"fxd-card wc-pipe fxd-anim d2\"><div class=\"wc-pipe-h\"><b>" + T("Pipeline by status", "خط المعالجة حسب الحالة") + "</b><span class=\"fxd-label\">" + T("Click a status to filter", "اضغط على حالة للتصفية") + "</span></div><div class=\"wc-bar\" id=\"wcBar\">");
        foreach (var s in statuses)
        {
            var c = s.Text("C");
            h.AppendLine("<i data-s=\"" + Attr(c) + "\" title=\"" + Attr(s.Text("N") + ": " + Num(s.Number("K"))) + "\" style=\"flex:" + Num(s.Number("K")) + ";--c:" + Color(c) + "\"></i>");
        }

        h.AppendLine("</div><div class=\"wc-legend\" id=\"wcLegend\">");
        foreach (var s in statuses)
        {
            var c = s.Text("C");
            h.AppendLine("<button type=\"button\" data-s=\"" + Attr(c) + "\" aria-pressed=\"false\" style=\"--c:" + Color(c) + "\">" + T(s.Text("N") ?? string.Empty, s.Text("NA") ?? s.Text("N") ?? string.Empty) + " <em>" + Num(s.Number("K")) + "</em></button>");
        }

        if (total == 0)
            h.AppendLine("<span class=\"fxd-label\">" + T("No cases yet", "لا توجد ملفات بعد") + "</span>");
        h.AppendLine("</div></section>");

        // toolbar
        h.AppendLine("<div class=\"wc-tools\" role=\"search\"><label class=\"wc-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"wcQ\" placeholder=\"Search portfolio no., project or developer\" aria-label=\"Search cases\"></label>"
            + "<div class=\"wc-seg\" id=\"wcWf\" role=\"group\" aria-label=\"Workflow\"><button type=\"button\" data-v=\"\" aria-pressed=\"true\">" + T("All", "الكل") + "</button><button type=\"button\" data-v=\"Y\" aria-pressed=\"false\">" + T("In workflow", "في سير العمل") + "</button><button type=\"button\" data-v=\"N\" aria-pressed=\"false\">" + T("Not started", "لم يبدأ") + "</button></div>"
            + "<div class=\"wc-seg\" id=\"wcRdy\" role=\"group\" aria-label=\"Readiness\"><button type=\"button\" data-v=\"\" aria-pressed=\"true\">" + T("Any readiness", "أي جاهزية") + "</button><button type=\"button\" data-v=\"Y\" aria-pressed=\"false\">" + T("Ready", "جاهزة") + "</button></div>"
            + "<select class=\"wc-sel\" id=\"wcSort\" aria-label=\"Sort\"><option value=\"date\">Newest first</option><option value=\"face\">Highest face value</option><option value=\"ready\">Most ready</option><option value=\"no\">Portfolio no.</option></select>"
            + "<span class=\"wc-count\" id=\"wcCount\"></span></div>");

        // cases
        h.AppendLine("<div class=\"wc-grid\" id=\"wcGrid\">");
        foreach (var r in cases)
        {
            var id = Num(r.Number("PORTFOLIO_ID"));
            var code = r.Text("PORTFOLIO_STATUS_CODE");
            var color = Color(code);
            var statusEn = r.Text("STATUS_EN");
            var no = r.Text("PORTFOLIO_NO");
            var date = r.Date("REQUEST_DATE");
            var inWorkflow = r.Number("WORKFLOW_INSTANCE_ID") is not null;
            string[] flags = ["DOCUMENT_READY_FLAG", "DD_READY_FLAG", "PAYMENT_HISTORY_READY_FLAG", "VALUATION_READY_FLAG", "LEGAL_READY_FLAG", "FINANCE_READY_FLAG", "CHEQUE_READY_FLAG", "GUARANTEE_READY_FLAG"];
            var rdy = flags.Count(f => r.Text(f) == "Y");
            var search = (no + " " + r.Text("PROJ") + " " + r.Text("DEV") + " " + statusEn + " " + r.Text("STATUS_AR") + " " + r.Text("STEP_NAME")).ToLowerInvariant();
            if (search.Length > 2000)
                search = search[..2000];
            string On(string flag) => r.Text(flag) == "Y" ? "on" : string.Empty;

            h.AppendLine("<article class=\"wc-card\" style=\"--c:" + color + "\" data-s=\"" + Attr(code) + "\""
                + " data-wf=\"" + (inWorkflow ? "Y" : "N") + "\" data-rdy=\"" + (r.Text("READY_FOR_SUBMISSION_FLAG") ?? "N") + "\" data-n=\"" + rdy + "\""
                + " data-face=\"" + Num(r.Number("FACE_VALUE") ?? 0) + "\" data-date=\"" + date?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "\" data-no=\"" + Attr(no) + "\" data-q=\"" + Attr(search) + "\">");
            h.AppendLine("<div class=\"wc-head\"><div class=\"wc-r1\"><span class=\"wc-no\">" + E(no) + "</span>"
                + "<span class=\"fxd-pill fxd-pill--plain\" style=\"background:color-mix(in srgb," + color + " 12%,#fff);color:" + color + "\">" + T(statusEn ?? string.Empty, r.Text("STATUS_AR") ?? statusEn ?? string.Empty) + "</span></div>"
                + "<h3><a href=\"" + Url(203, "P203_PORTFOLIO_ID", id) + "\">" + E(r.Text("PROJ")) + "</a></h3>"
                + "<div class=\"wc-sub\">" + E(r.Text("DEV")) + (date is { } d ? " &middot; " + T("Requested", "تاريخ الطلب") + " " + d.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : string.Empty) + "</div></div>");
            h.AppendLine("<div class=\"wc-money\"><div><b>" + Money(r.Number("FACE_VALUE")) + "</b><span>" + T("Face value", "القيمة الاسمية") + "</span></div>"
                + "<div><b>" + Money(r.Number("OUTSTANDING_VALUE")) + "</b><span>" + T("Outstanding", "القائم") + "</span></div>"
                + "<div><b>" + Money(r.Number("PROPOSED_PURCHASE_VALUE")) + "</b><span>" + T("Proposed price", "سعر الشراء المقترح") + "</span></div></div>");
            h.AppendLine("<div class=\"wc-ready\"><div class=\"wc-ready-h\"><span>" + T("Readiness", "الجاهزية") + "</span><b>" + rdy + "/8</b></div><div class=\"wc-segs" + (rdy == 8 ? " all" : string.Empty) + "\">"
                + "<i class=\"" + On("DOCUMENT_READY_FLAG") + "\" title=\"Documents\"></i><i class=\"" + On("DD_READY_FLAG") + "\" title=\"Due diligence\"></i>"
                + "<i class=\"" + On("PAYMENT_HISTORY_READY_FLAG") + "\" title=\"Payment history\"></i><i class=\"" + On("VALUATION_READY_FLAG") + "\" title=\"Valuation\"></i>"
                + "<i class=\"" + On("LEGAL_READY_FLAG") + "\" title=\"Legal\"></i><i class=\"" + On("FINANCE_READY_FLAG") + "\" title=\"Finance\"></i>"
                + "<i class=\"" + On("CHEQUE_READY_FLAG") + "\" title=\"Cheques\"></i><i class=\"" + On("GUARANTEE_READY_FLAG") + "\" title=\"Guarantees\"></i></div></div>");
            h.AppendLine(inWorkflow
                ? "<div class=\"wc-wf\"><span class=\"fa fa-sitemap\" aria-hidden=\"true\"></span><span>" + T("Current step", "الخطوة الحالية") + ": <b>" + E(r.Text("STEP_NAME") ?? "-") + "</b></span></div>"
                : "<div class=\"wc-wf off\"><span class=\"fa fa-pause\" aria-hidden=\"true\"></span><span>" + T("Workflow not started", "لم يبدأ سير العمل") + "</span></div>");
            if (!inWorkflow)
            {
                h.AppendLine("<div class=\"wc-gate\"><label class=\"wc-chk\"><input type=\"checkbox\" data-docs=\"" + id + "\"><span class=\"box\" aria-hidden=\"true\"></span><span>" + T("All documents are loaded", "تم تحميل جميع المستندات") + "</span></label><button type=\"button\" class=\"wc-btn wc-btn--start\" data-start=\"" + id + "\" data-no=\"" + Attr(no) + "\" disabled><span class=\"fa fa-play\" aria-hidden=\"true\"></span>" + T("Start workflow", "بدء سير العمل") + "</button></div>");
            }

            h.AppendLine("<div class=\"wc-foot\"><small>#" + id + "</small><div class=\"acts\">"
                + "<a class=\"wc-btn wc-btn--open\" href=\"" + Url(203, "P203_PORTFOLIO_ID", id) + "\"><span class=\"fa fa-folder-open-o\" aria-hidden=\"true\"></span>" + T("Open", "فتح") + "</a></div></div></article>");
        }

        h.AppendLine("</div>");
        var none = cases.Count == 0;
        h.AppendLine("<div class=\"wc-empty" + (none ? " on" : string.Empty) + "\" id=\"wcEmpty\"><b>" + (none ? T("No cases yet", "لا توجد ملفات بعد") : T("No cases match", "لا توجد ملفات مطابقة")) + "</b>"
            + (none ? T("Create the first portfolio with New portfolio.", "أنشئ أول محفظة من زر محفظة جديدة.") : T("Try another search or clear the filters.", "جرّب بحثاً آخر أو امسح عوامل التصفية.")) + "</div>");
        h.AppendLine("</div></div>");
        return new { ok = true, html = h.ToString() };
    }

    /// <summary>num(p): to_char(nvl(p,0),'TM9').</summary>
    private static string Num(decimal? n) => (n ?? 0).ToString("0.############", CultureInfo.InvariantCulture);

    // the status colour the region repeats for the bar, the legend and each card
    private static string Color(string? code)
    {
        var c = (code ?? string.Empty).ToUpperInvariant();
        return c.Contains("REJECT", StringComparison.Ordinal) ? "#C43325"
            : c.Contains("CANCEL", StringComparison.Ordinal) ? "#8A9DA6"
            : c.Contains("ACCEPT", StringComparison.Ordinal) || c.Contains("COMPLET", StringComparison.Ordinal) || c.Contains("APPROV", StringComparison.Ordinal) ? "#0B8A57"
            : c.Contains("INPROG", StringComparison.Ordinal) ? "#2463C7"
            : c.Contains("REVIEW", StringComparison.Ordinal) || c.Contains("AML", StringComparison.Ordinal) ? "#E87722"
            : c.Contains("LOAD", StringComparison.Ordinal) ? "#2E9AA3"
            : c.Contains("DRAFT", StringComparison.Ordinal) ? "#8A9DA6"
            : "#00636D";
    }
}

/// <summary>
/// App 105 page 201, Ajax Callback START_PORTFOLIO_WF (x01 = portfolio id): starts PURCHASED_PORTFOLIO_WF for the
/// portfolio on the Capitra workflow engine and keeps the instance and first task on the portfolio.
/// </summary>
public sealed class P0201StartPortfolioWorkflow(ICapitraDbContext db, WfeRuntime runtime, ICurrentUser user) : IApexProcess
{
    public int App => 105;

    public int Page => 201;

    public string Name => "START_PORTFOLIO_WF";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (!long.TryParse(request.X01, NumberStyles.Integer, CultureInfo.InvariantCulture, out var portfolioId))
            return new { success = false, message = "Portfolio was not found." };
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var portfolio = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(p => p.PortfolioId == portfolioId, ct);
            if (portfolio is null)
                return new { success = false, message = "Portfolio was not found." };
            if (portfolio.WorkflowInstanceId is { } started)
                return new { success = true, alreadyStarted = true, instanceId = started };

            var name = user.UserName ?? string.Empty;
            var variables = new Dictionary<string, string?>
            {
                ["ENTITY_ID"] = portfolioId.ToString(CultureInfo.InvariantCulture),
                ["ENTITY_REF"] = portfolio.PortfolioNo,
                ["ENTITY_TYPE"] = "PURCHASED_PORTFOLIO",
                ["MODULE_CODE"] = "MORTGAGE",
            };
            var instance = await runtime.StartAsync("PURCHASED_PORTFOLIO_WF", portfolio.PortfolioNo, variables, name, ct);
            var task = await db.Set<Domain.Modules.Wfe.WfeTask>()
                .Where(t => t.StepInstance!.WorkflowInstanceId == instance.WorkflowInstanceId && t.TaskStatus == WfeRuntime.Open)
                .OrderBy(t => t.TaskId).Select(t => (long?)t.TaskId).FirstOrDefaultAsync(ct);
            portfolio.WorkflowInstanceId = instance.WorkflowInstanceId;
            portfolio.WorkflowTaskId = task;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new { success = true, alreadyStarted = false, instanceId = instance.WorkflowInstanceId, taskId = task };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            await tx.RollbackAsync(ct);
            return new { success = false, message = e.Message };
        }
    }
}
