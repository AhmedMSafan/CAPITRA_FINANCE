using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>The formatting helpers shared by the two PL/SQL regions of page 215.</summary>
internal static class BoardHtml
{
    /// <summary>a2(): FM999G999G999G990D00, or &amp;ndash; when null.</summary>
    public static string A2(decimal? p) => p is null ? "&ndash;" : ToChar(p, "FM999G999G999G990D00");

    /// <summary>pc(): FM999G990D00 and a percent sign.</summary>
    public static string Pc(decimal? p) => p is null ? "&ndash;" : ToChar(p, "FM999G990D00") + "%";

    /// <summary>dt(): DD Mon YYYY.</summary>
    public static string Dt(DateOnly? p) => p is null ? "&ndash;" : p.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string Dt(DateTime? p) => p is null ? "&ndash;" : p.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string Kv(string l, string v) => "<dt>" + l + "</dt><dd>" + v + "</dd>";

    public static string G4(string l, string v) => "<div><span>" + l + "</span><b>" + v + "</b></div>";

    public static string Yn(bool p, string l) => "<li" + (p ? string.Empty : " class=\"no\"") + ">" + l + "</li>";

    public static string N(decimal? p) => P0203Html.Num(p ?? 0);
}

/// <summary>App 105 page 215 "Portfolio Board Review": the PL/SQL region "Board Report", ported.</summary>
public sealed class P0215BoardReport(IApexSqlReader reader, ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 215;

    public string Name => "REGION_BOARD_REPORT";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P215_HEADER, items, ct);
        if (head is null)
        {
            return new
            {
                ok = true,
                html = "<div class=\"fxd\"><div class=\"fxd-card fxd-empty-page\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-folder-open-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                    + "<h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from the Board Review task in your inbox.", "افتح هذه الصفحة من مهمة مراجعة المجلس في صندوقك.") + "</p></div></div>\n",
            };
        }

        var portfolioId = request.Item("P215_PORTFOLIO_ID");
        var pid = MtgAjax.Long(portfolioId);
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstAsync(x => x.PortfolioId == pid, ct);
        var cur = head.Text("CURRENCY_CODE");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P215_TASK_ID"), null);
        var acc = await reader.RowAsync(MtgApexPageSql.P215_ACCOUNTS, items, ct);
        var chq = await reader.RowAsync(MtgApexPageSql.P215_CHEQUES, items, ct);
        var r = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
        decimal face = acc?.Number("FACE") ?? 0, outs = acc?.Number("OUT") ?? 0, paid = acc?.Number("PAID") ?? 0, n = acc?.Number("N") ?? 0, inv = acc?.Number("INV") ?? 0;
        decimal sface = acc?.Number("SFACE") ?? 0, sout = acc?.Number("SOUT") ?? 0, spaid = acc?.Number("SPAID") ?? 0, sn = acc?.Number("SN") ?? 0, sinv = acc?.Number("SINV") ?? 0, units = acc?.Number("UNITS") ?? 0;

        var h = new StringBuilder();
        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine("<style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,.bd-dec-form,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}"
                + ".wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}"
                + ".fxd-wrap.wb{padding-bottom:28px!important}"
                + ".wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}"
                + ".wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>");
            h.AppendLine("<div class=\"wb-ro\"><span class=\"fa fa-eye\" aria-hidden=\"true\"></span>" + T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")
                + "<a href=\"" + Url(203, "P203_PORTFOLIO_ID", portfolioId) + "\">" + T("Back to portfolio", "العودة إلى المحفظة") + " &rsaquo;</a></div>");
        }

        h.AppendLine("<div class=\"fxd\" id=\"bdTop\"><header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(taskUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to task", "العودة إلى المهمة") + "</a><span>/</span><span>" + T("Board review", "مراجعة مجلس الإدارة") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + T("Board review", "مراجعة مجلس الإدارة") + "</h1>"
            + "<p class=\"fxd-lead\">" + T("Everything the board needs on one page: the deal, the price, the checks that were done and the workflow trail. Record the board decision at the end.", "كل ما يحتاجه المجلس في صفحة واحدة: الصفقة والسعر والفحوصات ومسار سير العمل، ثم سجّل قرار المجلس في النهاية.") + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(head.Text("PORTFOLIO_NO")) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div>"
            + "<div class=\"bd-noprint bd-acts\"><button type=\"button\" class=\"fxd-btn fxd-btn--glass\" data-pdf=\"EN\"><span class=\"fa fa-file-pdf-o\" aria-hidden=\"true\"></span>PDF &middot; English</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--glass\" data-pdf=\"AR\"><span class=\"fa fa-file-pdf-o\" aria-hidden=\"true\"></span>PDF &middot; عربي</button>"
            + "<a class=\"fxd-btn fxd-btn--glass\" target=\"_blank\" rel=\"noopener\" href=\"" + Url(216, "P216_PORTFOLIO_ID", portfolioId) + "\"><span class=\"fa fa-file-text-o\" aria-hidden=\"true\"></span>" + T("Proposed contract", "العقد المقترح") + "</a></div></div></div></header>");

        h.AppendLine("<div class=\"bd-sheet\"><article class=\"bd-doc\" id=\"bdDoc\"><div class=\"bd-mast\"><div><small>" + T("Board memorandum", "مذكرة مجلس الإدارة") + "</small><h2>" + T("Purchase of mortgage portfolio", "شراء محفظة تمويل عقاري") + " " + E(head.Text("PORTFOLIO_NO")) + "</h2>"
            + "<p>" + E(head.Text("DEV")) + " &middot; " + E(head.Text("PRJ")) + "</p></div><div class=\"bd-ref\"><span>" + T("Requested", "تاريخ الطلب") + "</span><b>" + BoardHtml.Dt(p.RequestDate) + "</b><span>" + T("Submitted to board", "تاريخ العرض على المجلس") + "</span><b>" + BoardHtml.Dt(MtgAjax.Cairo(p.SubmittedAt)) + "</b>"
            + "<span>" + T("Prepared", "تاريخ الإعداد") + "</span><b>" + BoardHtml.Dt(MtgAjax.CairoNow(clock)) + "</b><span>" + T("Currency", "العملة") + "</span><b>" + E(cur) + "</b></div></div>");

        // 1. recommendation
        h.AppendLine("<section class=\"bd-sec\"><h3><i>1</i>" + T("The proposal", "المقترح") + "</h3><div class=\"bd-hero\">"
            + "<div class=\"main\"><span>" + T("Proposed purchase value", "قيمة الشراء المقترحة") + "</span><b>" + BoardHtml.A2(r?.ProposedPurchaseValue) + "</b><small>" + E(cur) + " &middot; " + T("from the financial review", "من المراجعة المالية") + "</small></div>"
            + "<div class=\"ok\"><span>" + T("Expected return", "العائد المتوقع") + "</span><b>" + BoardHtml.A2(r?.ExpectedReturnAmount) + "</b><small>"
            + (r is not null && (r.ProposedPurchaseValue ?? 0) > 0 ? BoardHtml.Pc(r.ExpectedReturnAmount / r.ProposedPurchaseValue * 100) + " " + T("of the price", "من السعر") : "&nbsp;") + "</small></div>"
            + "<div><span>" + T("Cheques collected", "الشيكات المحصلة") + "</span><b>" + BoardHtml.A2(chq?.Number("AMT") ?? 0) + "</b><small>" + BoardHtml.N(chq?.Number("N")) + " " + T("cheques", "شيك") + "</small></div>"
            + "<div><span>" + T("Investors in the deal", "المستثمرون في الصفقة") + "</span><b>" + BoardHtml.N(sinv) + " / " + BoardHtml.N(inv) + "</b><small>" + BoardHtml.N(sn) + " " + T("contracts", "عقود") + " &middot; " + BoardHtml.N(units) + " " + T("units", "وحدات") + "</small></div></div></section>");

        // 2. the portfolio
        h.AppendLine("<section class=\"bd-sec\"><h3><i>2</i>" + T("The portfolio", "المحفظة") + "</h3><div class=\"bd-grid4\">"
            + BoardHtml.G4(T("Face value (all)", "القيمة الاسمية (الكل)"), BoardHtml.A2(face)) + BoardHtml.G4(T("Outstanding (all)", "الرصيد القائم (الكل)"), BoardHtml.A2(outs)) + BoardHtml.G4(T("Paid (all)", "المسدد (الكل)"), BoardHtml.A2(paid)) + BoardHtml.G4(T("Contracts (all)", "العقود (الكل)"), BoardHtml.N(n))
            + BoardHtml.G4(T("Selected face value", "القيمة الاسمية المختارة"), BoardHtml.A2(sface)) + BoardHtml.G4(T("Selected outstanding", "الرصيد القائم المختار"), BoardHtml.A2(sout))
            + BoardHtml.G4(T("Selected paid", "المسدد المختار"), BoardHtml.A2(spaid) + (sface > 0 ? " <small style=\"color:var(--muted);font-weight:400\">(" + BoardHtml.Pc(spaid / sface * 100) + ")</small>" : string.Empty))
            + BoardHtml.G4(T("Excluded investors", "المستثمرون المستبعدون"), BoardHtml.N(inv - sinv)) + "</div></section>");

        // 3. pricing build-up and terms
        h.AppendLine("<section class=\"bd-sec\"><h3><i>3</i>" + T("Pricing", "التسعير") + "</h3><div class=\"bd-two\"><div>");
        if (r is not null)
        {
            h.AppendLine("<table class=\"bd-tbl\"><tbody>"
                + "<tr><td>" + T("Cheques face amount", "القيمة الاسمية للشيكات") + " <small style=\"color:var(--muted)\">(" + r.ChequeCount + ")</small></td><td class=\"n\">" + BoardHtml.A2(r.ChequesFaceAmount) + "</td></tr>"
                + "<tr><td>" + T("Present value", "القيمة الحالية") + " <small style=\"color:var(--muted)\">" + P0203Html.Nice(r.PvMethod) + " &middot; " + BoardHtml.Pc(r.PvRate) + "</small></td><td class=\"n\"><b>" + BoardHtml.A2(r.ChequesPvAmount) + "</b></td></tr>"
                + "<tr class=\"minus\"><td>" + T("Margin", "الهامش") + " <small style=\"color:var(--muted)\">" + BoardHtml.Pc(r.MarginPercentage) + " &times; " + P0203Html.Nice(r.MarginBase) + "</small></td><td class=\"n\">&minus; " + BoardHtml.A2(r.MarginAmount) + "</td></tr>"
                + "<tr class=\"minus" + (r.IsAdminIncluded ? string.Empty : " off") + "\"><td>" + T("Admin expense", "المصروفات الإدارية") + " <small style=\"color:var(--muted)\">" + BoardHtml.Pc(r.AdminExpensePercent) + " &times; " + P0203Html.Nice(r.AdminBase) + "</small></td><td class=\"n\">&minus; " + BoardHtml.A2(r.AdminFinalAmount) + "</td></tr>"
                + "<tr class=\"minus" + (r.IsLegalIncluded ? string.Empty : " off") + "\"><td>" + T("Legal fees", "الرسوم القانونية") + " <small style=\"color:var(--muted)\">" + BoardHtml.A2(r.LegalFeePerUnit) + " &times; " + r.UnitsCount + "</small></td><td class=\"n\">&minus; " + BoardHtml.A2(r.LegalFinalAmount) + "</td></tr>"
                + "<tr class=\"minus" + (r.IsValuationIncluded ? string.Empty : " off") + "\"><td>" + T("Valuation expense", "مصروفات التقييم") + " <small style=\"color:var(--muted)\">" + BoardHtml.A2(r.ValuationFeePerUnit) + " &times; " + r.UnitsCount + "</small></td><td class=\"n\">&minus; " + BoardHtml.A2(r.ValuationFinalAmount) + "</td></tr>"
                + "<tr class=\"tot\"><td>" + T("Proposed purchase value", "قيمة الشراء المقترحة") + "</td><td class=\"n\">" + BoardHtml.A2(r.ProposedPurchaseValue) + " " + E(cur) + "</td></tr></tbody></table>");
        }
        else
        {
            h.AppendLine("<div class=\"bd-done bad\"><span class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\"></span>" + T("No financial review has been saved for this portfolio.", "لم يتم حفظ مراجعة مالية لهذه المحفظة.") + "</div>");
        }

        var term = head.Number("TERM_MONTHS");
        h.AppendLine("</div><div><dl class=\"bd-kv\">" + BoardHtml.Kv(T("Coverage required", "التغطية المطلوبة"), BoardHtml.Pc(head.Number("COVERAGE_PCT"))) + BoardHtml.Kv(T("Margin", "الهامش"), BoardHtml.Pc(head.Number("MARGIN_PERCENTAGE"))) + BoardHtml.Kv(T("PV rate", "معدل القيمة الحالية"), BoardHtml.Pc(head.Number("PV_RATE")))
            + BoardHtml.Kv(T("Reserved amount", "المبلغ المحتجز"), BoardHtml.Pc(head.Number("RESERVED_AMOUNT_PERCENT"))) + BoardHtml.Kv(T("Late fee rate", "غرامة التأخير"), BoardHtml.Pc(head.Number("LATE_FEE_RATE"))) + BoardHtml.Kv(T("Admin expense", "المصروفات الإدارية"), BoardHtml.Pc(head.Number("ADMIN_EXPENSE_PERCENT")))
            + BoardHtml.Kv(T("Legal fee / unit", "الرسوم القانونية للوحدة"), BoardHtml.A2(head.Number("LEGAL_FEE_PER_UNIT"))) + BoardHtml.Kv(T("Valuation fee / unit", "مصروف التقييم للوحدة"), BoardHtml.A2(head.Number("VALUATION_FEE_PER_UNIT")))
            + BoardHtml.Kv(T("Term", "المدة"), term is not null ? BoardHtml.N(term) + " " + T("months", "شهر") : "&ndash;") + "</dl></div></div></section></article>");
        return new { ok = true, html = h.ToString() };
    }
}

/// <summary>App 105 page 215, the PL/SQL region "Board Report Details": checks, investors, workflow trail and the board decision.</summary>
public sealed class P0215BoardReportDetails(IApexSqlReader reader, ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 215;

    public string Name => "REGION_BOARD_REPORT_DETAILS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var pid = MtgAjax.Long(request.Item("P215_PORTFOLIO_ID"));
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
        if (p is null)
            return new { ok = true, html = string.Empty };

        var inboxUrl = ApexPages.Url(105, 920);
        var aml = await reader.RowAsync(MtgApexPageSql.P215_AML, items, ct);
        var inq = await reader.RowAsync(MtgApexPageSql.P215_INQ, items, ct);
        var lg = await db.Set<MtgLegalReview>().AsNoTracking().Where(x => x.PortfolioId == pid).OrderByDescending(x => x.LegalReviewId).FirstOrDefaultAsync(ct);
        var v = await reader.RowAsync(MtgApexPageSql.P215_VALUATION, items, ct);
        decimal amlN = aml?.Number("N") ?? 0, amlFlag = aml?.Number("FLAG") ?? 0, inqN = inq?.Number("N") ?? 0, inqHigh = inq?.Number("HIGH") ?? 0;

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"bdRoot\" data-inbox=\"" + Attr(inboxUrl) + "\"><div class=\"bd-sheet\"><article class=\"bd-doc\" style=\"margin-top:18px\">");

        // 4. due diligence
        h.AppendLine("<section class=\"bd-sec\"><h3><i>4</i>" + T("Checks performed", "الفحوصات التي تمت") + "</h3><div class=\"bd-dd\">"
            + "<div><h4><span class=\"fa fa-shield\" aria-hidden=\"true\"></span>" + T("AML screening", "فحص مكافحة غسل الأموال") + "</h4>"
            + (amlN == 0 ? "<span class=\"fxd-pill fxd-pill--mute\">" + T("Not screened", "لم يتم الفحص") + "</span>" : amlFlag == 0 ? "<span class=\"fxd-pill fxd-pill--ok\">" + T("All clear", "سليم بالكامل") + "</span>"
                : "<span class=\"fxd-pill fxd-pill--warn\">" + BoardHtml.N(amlFlag) + " " + T("flagged", "مُعلّم") + "</span>") + "<p>" + BoardHtml.N(amlN) + " " + T("subjects screened", "جهة تم فحصها") + "</p></div>"
            + "<div><h4><span class=\"fa fa-comments-o\" aria-hidden=\"true\"></span>" + T("Inquiries", "الاستعلامات") + "</h4>"
            + (inqN == 0 ? "<span class=\"fxd-pill fxd-pill--mute\">" + T("None recorded", "لا توجد") + "</span>" : inqHigh == 0 ? "<span class=\"fxd-pill fxd-pill--ok\">" + T("No high risk", "لا مخاطر عالية") + "</span>"
                : "<span class=\"fxd-pill fxd-pill--bad\">" + BoardHtml.N(inqHigh) + " " + T("high risk", "مخاطر عالية") + "</span>") + "<p>" + BoardHtml.N(inqN) + " " + T("inquiries", "استعلام") + "</p></div>"
            + "<div><h4><span class=\"fa fa-gavel\" aria-hidden=\"true\"></span>" + T("Legal review", "المراجعة القانونية") + "</h4>"
            + (lg is not null ? "<ul class=\"bd-chk\">" + BoardHtml.Yn(lg.IsOwnershipVerified, T("Ownership", "الملكية")) + BoardHtml.Yn(lg.IsBuildingLicenseVerified, T("Building licence", "رخصة البناء"))
                + BoardHtml.Yn(lg.IsTitleDeedVerified, T("Title deed", "سند الملكية")) + BoardHtml.Yn(lg.IsEncumbranceFree, T("No encumbrance", "خالٍ من الأعباء"))
                + BoardHtml.Yn(lg.IsRegistrationPossible, T("Registration possible", "إمكانية التسجيل")) + BoardHtml.Yn(lg.IsMortgagePossible, T("Mortgage possible", "إمكانية الرهن")) + "</ul><p>" + T("Reviewed", "تاريخ المراجعة") + " " + BoardHtml.Dt(lg.ReviewDate) + "</p>"
                : "<span class=\"fxd-pill fxd-pill--mute\">" + T("Not reviewed", "لم تتم المراجعة") + "</span>") + "</div>"
            + "<div><h4><span class=\"fa fa-line-chart\" aria-hidden=\"true\"></span>" + T("Valuation", "التقييم") + "</h4>"
            + (v is not null ? "<dl class=\"bd-kv\" style=\"font-size:12.5px\">" + BoardHtml.Kv(T("Market", "السوقية"), BoardHtml.A2(v.Number("MARKET_VALUE"))) + BoardHtml.Kv(T("Recommended", "الموصى بها"), BoardHtml.A2(v.Number("RECOMMENDED_VALUE"))) + BoardHtml.Kv(T("Completion", "الإنجاز"), BoardHtml.Pc(v.Number("PROJECT_COMPLETION_PCT"))) + "</dl><p>" + E(v.Text("VALUER_NAME")) + " &middot; " + BoardHtml.Dt(v.Date("VALUATION_DATE")) + "</p>"
                : "<span class=\"fxd-pill fxd-pill--mute\">" + T("No valuation", "لا يوجد تقييم") + "</span>") + "</div></div></section>");

        // 5. investors
        h.AppendLine("<section class=\"bd-sec\"><h3><i>5</i>" + T("Investors", "المستثمرون") + "</h3><div class=\"wb-tbl-wrap\"><table class=\"bd-tbl\"><thead><tr><th>" + T("Investor", "المستثمر") + "</th><th class=\"n\">" + T("Contracts", "العقود") + "</th>"
            + "<th class=\"n\">" + T("Outstanding", "الرصيد القائم") + "</th><th class=\"n\">" + T("Paid", "المسدد") + "</th><th class=\"n\">" + T("Cheques", "الشيكات") + "</th><th class=\"n\">" + T("Cheque cover", "تغطية الشيكات") + "</th><th>" + T("Decision", "القرار") + "</th></tr></thead><tbody>");
        foreach (var i in await reader.QueryAsync(MtgApexPageSql.P215_INVESTORS, items, ct))
        {
            decimal cnt = i.Number("N") ?? 0, sn = i.Number("SN") ?? 0, ov = i.Number("OV") ?? 0, cv = i.Number("CV") ?? 0, pv = i.Number("PV") ?? 0, chq = i.Number("CHQ") ?? 0;
            h.AppendLine("<tr" + (sn == 0 ? " class=\"ex\"" : string.Empty) + "><td><b>" + E(i.Text("NM")) + "</b> <small style=\"color:var(--muted)\">" + E(i.Text("CD")) + "</small></td><td class=\"n\">" + BoardHtml.N(cnt) + "</td><td class=\"n\">" + BoardHtml.A2(ov) + "</td>"
                + "<td class=\"n\">" + (cv > 0 ? BoardHtml.Pc(pv / cv * 100) : "&ndash;") + "</td><td class=\"n\">" + BoardHtml.A2(chq) + "</td><td class=\"n\">" + (ov > 0 ? BoardHtml.Pc(chq / ov * 100) : "&ndash;") + "</td>"
                + "<td>" + (sn == 0 ? "<span class=\"fxd-pill fxd-pill--bad\">" + T("Excluded", "مستبعد") + "</span>" : sn < cnt ? "<span class=\"fxd-pill fxd-pill--warn\">" + T("Partly", "جزئي") + "</span>" : "<span class=\"fxd-pill fxd-pill--ok\">" + T("Included", "مُدرج") + "</span>") + "</td></tr>");
        }

        h.AppendLine("</tbody></table></div></section>");

        // 6. workflow trail
        h.AppendLine("<section class=\"bd-sec\"><h3><i>6</i>" + T("Workflow trail", "مسار سير العمل") + "</h3><ol class=\"bd-trail\">");
        var trail = p.WorkflowInstanceId is null ? [] : await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => s.WorkflowInstanceId == p.WorkflowInstanceId && s.StepCode != null)
            .OrderBy(s => s.EnteredAt).ThenBy(s => s.StepInstanceId).ToListAsync(ct);
        foreach (var s in trail)
        {
            var bad = (s.Outcome ?? string.Empty).StartsWith("REJECT", StringComparison.OrdinalIgnoreCase);
            h.AppendLine("<li class=\"" + (bad ? "bad" : s.CompletedAt is null ? "now" : string.Empty) + "\"><b>" + E(s.StepName ?? P0203Html.Nice(s.StepCode)) + "</b><span>"
                + (s.CompletedAt is null ? "<span class=\"fxd-pill fxd-pill--warn\">" + T("In progress", "قيد التنفيذ") + "</span>" : "<span class=\"fxd-pill fxd-pill--" + (bad ? "bad" : "ok") + "\">" + E(P0203Html.Nice(s.Outcome)) + "</span>")
                + "</span><small>" + (s.CompletedAt is not null ? E(s.CompletedBy) + " &middot; " + MtgAjax.Cairo(s.CompletedAt)!.Value.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)
                    : T("Started", "بدأ") + " " + MtgAjax.Cairo(s.EnteredAt)!.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)) + "</small></li>");
        }

        if (trail.Count == 0)
            h.AppendLine("<li>" + T("No workflow history was found.", "لا يوجد سجل لسير العمل.") + "</li>");
        h.AppendLine("</ol></section>");

        // 7. board decision
        h.AppendLine("<section class=\"bd-sec\"><h3><i>7</i>" + T("Board decision", "قرار المجلس") + "</h3>");
        if (p.BoardDecision is not null)
        {
            var rej = p.BoardDecision.StartsWith("REJECT", StringComparison.OrdinalIgnoreCase);
            h.AppendLine("<div class=\"bd-done " + (rej ? "bad" : "ok") + "\"><span class=\"fa " + (rej ? "fa-times-circle" : "fa-check-circle") + "\" style=\"font-size:24px\" aria-hidden=\"true\"></span>"
                + "<div><b>" + E(P0203Html.Nice(p.BoardDecision)) + "</b><br>" + T("Decision no.", "رقم القرار") + " " + E(p.BoardDecisionNo) + " &middot; " + BoardHtml.Dt(p.BoardDecisionDate)
                + (p.ApprovedAt is not null ? " &middot; " + T("Approved", "الاعتماد") + " " + BoardHtml.Dt(MtgAjax.Cairo(p.ApprovedAt)) : string.Empty) + "</div></div>");
        }

        var bdDate = p.BoardDecisionDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? MtgAjax.CairoNow(clock).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        h.AppendLine("<div class=\"bd-dec-form bd-noprint\" style=\"margin-top:12px\"><div class=\"wb-fgrid\"><div class=\"wb-f\"><label for=\"bdNo\">" + T("Board decision no.", "رقم قرار المجلس") + " *</label><input id=\"bdNo\" maxlength=\"100\" value=\"" + Attr(p.BoardDecisionNo) + "\"></div>"
            + "<div class=\"wb-f\"><label for=\"bdDate\">" + T("Board decision date", "تاريخ قرار المجلس") + " *</label><input id=\"bdDate\" type=\"date\" value=\"" + bdDate + "\"></div></div></div>"
            + "<div class=\"bd-sign\"><div>" + T("Chairman", "رئيس المجلس") + "</div><div>" + T("Board secretary", "أمين سر المجلس") + "</div><div>" + T("Chief executive", "الرئيس التنفيذي") + "</div></div></section></article></div>");

        h.AppendLine("<div class=\"wb-dbar\"><div class=\"wb-dbar-in\"><textarea id=\"wbComment\" aria-label=\"Board comment\" placeholder=\"Board comment (required)\"></textarea><div class=\"wb-hint\" id=\"wbHint\"></div>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--danger\" id=\"wbReject\"><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span>" + T("Reject", "رفض") + "</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"wbAccept\"><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Accept", "قبول") + "</button></div></div>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return new { ok = true, html = h.ToString() };
    }
}

/// <summary>App 105 page 215, the PL/SQL region "Step Actions".</summary>
public sealed class P0215StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 215;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(215, request, ct) };
}

/// <summary>App 105 page 215, Ajax Callback BOARD_DECISION: records the board decision and moves the Board Review task on.</summary>
public sealed class P0215BoardDecision(ICapitraDbContext db, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 215;

    public string Name => "BOARD_DECISION";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunAsync(db, async () =>
    {
        var portfolioId = MtgAjax.Long(request.Item("P215_PORTFOLIO_ID"));
        var action = MtgAjax.Upper(request.X02);
        var comment = MtgAjax.Trim(request.X03);
        var no = MtgAjax.Trim(request.X04);
        var rawDate = MtgAjax.Trim(request.X05);
        DateOnly? date = null;
        if (rawDate is not null)
        {
            date = DateOnly.TryParseExact(rawDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d : throw new ApexProcessException("ORA-01861: literal does not match format string");
        }

        if (action is not ("ACCEPT" or "REJECT"))
            throw new ApexProcessException("ORA-20921: Invalid board decision.");
        if (no is null)
            throw new ApexProcessException("ORA-20923: The board decision number is required.");
        if (date is null)
            throw new ApexProcessException("ORA-20924: The board decision date is required.");
        if (comment is null)
            throw new ApexProcessException("ORA-20920: A comment is required for the board decision.");
        var newStatus = action == "ACCEPT" ? "BOARD_REVIEW_ACCEPTED" : "BOARD_REVIEW_REJECTED";
        if (!await db.Set<MtgLkpPortfolioStatus>().AnyAsync(s => s.StatusCode == newStatus, ct))
            throw new ApexProcessException("ORA-20925: Status " + newStatus + " is missing. Run the v86 database script (FINOVA_v86_BOARD_REVIEW.sql) first.");
        var task = portfolioId is null ? null
            : await steps.OpenTaskAsync(portfolioId.Value, "BOARD", MtgAjax.Long(request.Item("P215_TASK_ID")), ct, like: true, page: 215);
        if (task is null)
        {
            var open = portfolioId is null ? null : await steps.OpenTasksTextAsync(portfolioId.Value, ct);
            throw new ApexProcessException("ORA-20922: No open Board Review task was found for portfolio " + P0203Html.Num(portfolioId) + ". Open tasks for it: " + (open is null ? "none" : open[..Math.Min(300, open.Length)]) + ".");
        }

        await steps.StartAsync(task, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var oldStatus = p.PortfolioStatusCode;
        var newTask = await steps.DecideAsync(task.TaskId, action == "REJECT" ? "REJECT" : "APPROVE", comment, ct);
        var now = clock.UtcNow;
        p.BoardDecision = action == "ACCEPT" ? "ACCEPTED" : "REJECTED";
        p.BoardDecisionNo = no;
        p.BoardDecisionDate = date;
        p.ApprovedAt = action == "ACCEPT" ? now : null;
        p.SubmittedAt ??= now;
        p.Comments = comment;
        p.RejectionReason = action == "REJECT" ? comment : null;
        p.PortfolioStatusCode = newStatus;
        p.WorkflowTaskId = newTask ?? p.WorkflowTaskId;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangeAt = now,
            ChangedBy = user.UserName ?? string.Empty,
            Comments = "Board decision " + no + ": " + comment,
        });
        await db.SaveChangesAsync(ct);
        return action == "ACCEPT" ? "Board approval recorded." : "Board rejection recorded.";
    }, ct);
}
