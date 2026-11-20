using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>The small functions and the shared query block of the three PL/SQL regions of page 216.</summary>
internal sealed class Agreement
{
    public const string Bl = "<span class=\"ct-blank\"></span>";

    public required IReadOnlyDictionary<string, object?> Head { get; init; }

    public decimal N { get; init; }

    public decimal Units { get; init; }

    public decimal Out { get; init; }

    public decimal ChqN { get; init; }

    public decimal Chq { get; init; }

    public decimal? Adm { get; init; }

    public decimal? Leg { get; init; }

    public decimal? Val { get; init; }

    public static string A2(decimal? p) => p is null ? Bl : "<span class=\"ct-num\">" + ToChar(p, "FM999G999G999G990D00") + "</span>";

    public static string Pc(decimal? p) => p is null ? Bl : "<span class=\"ct-num\">" + ToChar(p, "FM990D00") + "%</span>";

    public static string N0(decimal? p) => "<span class=\"ct-num\">" + P0203Html.Num(p ?? 0) + "</span>";

    public static string V(string? p) => p is null ? Bl : "<b>" + E(p) + "</b>";

    public static string Dt(DateOnly? p) => p is null ? Bl : "<span class=\"ct-num\">" + p.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "</span>";

    public static string Cl(string no, string te, string ta, string en, string ar) =>
        "<section class=\"ct-row\"><div><h3>" + no + te + "</h3><p>" + en + "</p></div><div class=\"ar\" lang=\"ar\"><h3>" + ta + "</h3><p>" + ar + "</p></div></section>";

    /// <summary>The select-into block both agreement regions start with; null when the portfolio is not found.</summary>
    public static async Task<Agreement?> LoadAsync(IApexSqlReader reader, ICapitraDbContext db, ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P216_HEADER, items, ct);
        if (head is null)
            return null;
        var sel = await reader.RowAsync(MtgApexPageSql.P216_SELECTED, items, ct);
        var chq = await reader.RowAsync(MtgApexPageSql.P216_CHEQUES, items, ct);
        var pid = MtgAjax.Long(request.Item("P216_PORTFOLIO_ID"));
        var fr = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
        return new Agreement
        {
            Head = head,
            N = sel?.Number("N") ?? 0,
            Units = sel?.Number("UNITS") ?? 0,
            Out = sel?.Number("OV") ?? 0,
            ChqN = chq?.Number("N") ?? 0,
            Chq = chq?.Number("AMT") ?? 0,
            Adm = fr is null ? null : fr.IsAdminIncluded ? fr.AdminFinalAmount : 0,
            Leg = fr is null ? null : fr.IsLegalIncluded ? fr.LegalFinalAmount : 0,
            Val = fr is null ? null : fr.IsValuationIncluded ? fr.ValuationFinalAmount : 0,
        };
    }
}

/// <summary>App 105 page 216 "Portfolio Purchase Agreement": the PL/SQL region "Agreement" (parties, preamble, clauses 1 to 6).</summary>
public sealed class P0216Agreement(IApexSqlReader reader, ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 216;

    public string Name => "REGION_AGREEMENT";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var a = await Agreement.LoadAsync(reader, db, request, ct);
        if (a is null)
            return new { ok = true, html = "<div class=\"fxd\"><div class=\"ct-page\">Portfolio not found.</div>\n" };

        const string bl = Agreement.Bl;
        var r = a.Head;
        var pno = r.Text("PORTFOLIO_NO");
        var den = r.Text("LEGAL_NAME_EN");
        var pen = r.Text("PROJECT_NAME_EN");
        var cur = E(r.Text("CURRENCY_CODE"));
        var bdn = r.Text("BOARD_DECISION_NO");
        var bdd = r.Date("BOARD_DECISION_DATE");
        var price = r.Number("PROPOSED_PURCHASE_VALUE");
        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\"><div class=\"ct-bar\"><span class=\"fa fa-file-text-o\" style=\"color:var(--teal-600)\" aria-hidden=\"true\"></span><b>Proposed purchase agreement &middot; " + E(pno) + "</b><span class=\"sp\"></span>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--teal\" onclick=\"window.print()\"><span class=\"fa fa-file-pdf-o\" aria-hidden=\"true\"></span>Print / Save as PDF</button>"
            + "<button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" onclick=\"window.close();history.back()\">Close</button></div></div>");
        h.AppendLine("<div class=\"fxd\"><div class=\"ct-page ct-a\"><div class=\"ct-head\"><div><span class=\"ct-draft\">Draft &middot; for legal review</span><h1>Mortgage Portfolio Purchase Agreement</h1><p>Ref. CAP-PPA-" + E(pno) + " &middot; Made on " + bl + " at " + bl + "</p></div>"
            + "<div class=\"ar\" lang=\"ar\"><span class=\"ct-draft\">مسودة للمراجعة القانونية</span><h1>عقد شراء محفظة تمويل عقاري</h1><p>مرجع CAP-PPA-" + E(pno) + " &middot; حُرر في " + bl + " بمدينة " + bl + "</p></div></div>");
        h.AppendLine(Agreement.Cl(string.Empty, "Parties", "الأطراف",
            "<b>First Party (the Buyer):</b> CAPITRA, a company established under the laws of the Arab Republic of Egypt, Commercial Register No. " + bl + ", address " + bl + ", represented by " + bl + " in his capacity as " + bl + ".<br><br>"
            + "<b>Second Party (the Seller):</b> " + Agreement.V(den) + ", Commercial Register No. " + bl + ", address " + bl + ", represented by " + bl + " in his capacity as " + bl + ".",
            "<b>الطرف الأول (المشتري):</b> شركة كابيترا، شركة مؤسسة وفقاً لقوانين جمهورية مصر العربية، سجل تجاري رقم " + bl + "، وعنوانها " + bl + "، ويمثلها السيد/ " + bl + " بصفته " + bl + ".<br><br>"
            + "<b>الطرف الثاني (البائع):</b> " + Agreement.V(r.Text("LEGAL_NAME_AR") ?? den) + "، سجل تجاري رقم " + bl + "، وعنوانها " + bl + "، ويمثلها السيد/ " + bl + " بصفته " + bl + "."));
        h.AppendLine(Agreement.Cl(string.Empty, "Preamble", "تمهيد",
            "Whereas the Second Party has sold residential units in the project " + Agreement.V(pen) + " under instalment sale contracts with its customers (the &ldquo;Investors&rdquo;) and wishes to sell to the First Party its rights to the outstanding instalments under the contracts listed in Annex (A); and whereas the First Party has reviewed the portfolio and its Board of Directors approved the purchase by decision No. " + Agreement.V(bdn) + " dated " + Agreement.Dt(bdd) + "; the Parties, having acknowledged their full legal capacity to contract, have agreed as follows:",
            "حيث إن الطرف الثاني قد باع وحدات سكنية في مشروع " + Agreement.V(r.Text("PROJECT_NAME_AR") ?? pen) + " بموجب عقود بيع بالتقسيط مع عملائه (&laquo;المستثمرين&raquo;)، ويرغب في بيع حقوقه في الأقساط المتبقية بموجب العقود الواردة في الملحق (أ) إلى الطرف الأول؛ وحيث إن الطرف الأول قد قام بدراسة المحفظة ووافق مجلس إدارته على الشراء بموجب القرار رقم " + Agreement.V(bdn) + " بتاريخ " + Agreement.Dt(bdd) + "؛ فقد اتفق الطرفان، بعد أن أقرا بأهليتهما القانونية الكاملة للتعاقد، على ما يلي:"));
        h.AppendLine(Agreement.Cl("1. ", "Preamble and annexes", "البند الأول: التمهيد والملاحق",
            "The preamble above and the annexes form an integral part of this Agreement.",
            "يعتبر التمهيد السابق والملاحق جزءاً لا يتجزأ من هذا العقد ومكملاً له."));
        h.AppendLine(Agreement.Cl("2. ", "Subject of the agreement", "البند الثاني: موضوع العقد",
            "The Second Party sells and assigns to the First Party, who accepts, all its rights to the outstanding balances of " + Agreement.N0(a.N) + " instalment contracts covering " + Agreement.N0(a.Units) + " units in portfolio " + Agreement.V(pno) + ", with a total outstanding balance of " + Agreement.A2(a.Out) + " " + cur + ", together with " + Agreement.N0(a.ChqN) + " post-dated cheques of a total face value of " + Agreement.A2(a.Chq) + " " + cur + " listed in Annex (B).",
            "باع وتنازل الطرف الثاني للطرف الأول، القابل لذلك، عن كافة حقوقه في الأرصدة المتبقية لعدد " + Agreement.N0(a.N) + " عقد بيع بالتقسيط عن عدد " + Agreement.N0(a.Units) + " وحدة ضمن المحفظة رقم " + Agreement.V(pno) + "، بإجمالي رصيد قائم قدره " + Agreement.A2(a.Out) + " " + cur + "، بالإضافة إلى عدد " + Agreement.N0(a.ChqN) + " شيكاً آجلاً بقيمة اسمية إجمالية قدرها " + Agreement.A2(a.Chq) + " " + cur + " والمبينة في الملحق (ب)."));
        h.AppendLine(Agreement.Cl("3. ", "Purchase price", "البند الثالث: الثمن",
            "The purchase price is " + Agreement.A2(price) + " " + cur + ". It represents the present value of the cheques discounted at " + Agreement.Pc(r.Number("PV_RATE")) + " per annum, less the agreed margin and the charges stated in clause (9).",
            "حُدد ثمن الشراء بمبلغ " + Agreement.A2(price) + " " + cur + "، ويمثل القيمة الحالية للشيكات مخصومة بمعدل " + Agreement.Pc(r.Number("PV_RATE")) + " سنوياً، مخصوماً منها الهامش المتفق عليه والمصروفات المبينة في البند التاسع."));
        h.AppendLine(Agreement.Cl("4. ", "Payment and reserved amount", "البند الرابع: سداد الثمن والمبلغ المحتجز",
            "The First Party pays the price by bank transfer to the Second Party within " + bl + " business days after the cheques are endorsed and delivered, retaining " + Agreement.Pc(r.Number("RESERVED_AMOUNT_PERCENT")) + " of the price as a reserved amount. The reserved amount is released after " + bl + " months provided no cheque has been returned, after deducting any amount due under clause (7).",
            "يسدد الطرف الأول الثمن بتحويل بنكي إلى حساب الطرف الثاني خلال " + bl + " يوم عمل من تاريخ تظهير الشيكات وتسليمها، مع احتجاز نسبة " + Agreement.Pc(r.Number("RESERVED_AMOUNT_PERCENT")) + " من الثمن كمبلغ محتجز يُفرج عنه بعد " + bl + " شهراً بشرط عدم ارتداد أي شيك، وذلك بعد خصم أي مبالغ مستحقة طبقاً للبند السابع."));
        h.AppendLine(Agreement.Cl("5. ", "Transfer of rights and cheques", "البند الخامس: نقل الحقوق والشيكات",
            "On signing, the Second Party endorses all cheques listed in Annex (B) to the order of the First Party, delivers them together with copies of the investor contracts, and notifies the investors in writing that all payments are now due to the First Party.",
            "يلتزم الطرف الثاني عند التوقيع بتظهير جميع الشيكات الواردة في الملحق (ب) لأمر الطرف الأول وتسليمها مع صور عقود المستثمرين، وإخطار المستثمرين كتابياً بأن جميع المدفوعات أصبحت مستحقة للطرف الأول."));
        h.AppendLine(Agreement.Cl("6. ", "Representations and warranties", "البند السادس: الإقرارات والضمانات",
            "The Second Party warrants that the contracts are valid and enforceable, that the units are free of any third-party right or encumbrance, that the title documents and building licences are valid, that the cheques are genuine, and that it has not assigned these rights to any third party.",
            "يقر الطرف الثاني ويضمن أن العقود صحيحة ونافذة، وأن الوحدات خالية من أي حقوق أو أعباء للغير، وأن مستندات الملكية وتراخيص البناء سارية، وأن الشيكات صحيحة، وأنه لم يسبق له التنازل عن هذه الحقوق للغير."));
        h.AppendLine("</div></div>");
        return new { ok = true, html = h.ToString() };
    }
}

/// <summary>App 105 page 216, the PL/SQL region "Agreement Clauses" (clauses 7 to 13 and the signatures).</summary>
public sealed class P0216AgreementClauses(IApexSqlReader reader, ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 216;

    public string Name => "REGION_AGREEMENT_CLAUSES";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var a = await Agreement.LoadAsync(reader, db, request, ct);
        if (a is null)
            return new { ok = true, html = string.Empty };

        const string bl = Agreement.Bl;
        var cur = E(a.Head.Text("CURRENCY_CODE"));
        var late = Agreement.Pc(a.Head.Number("LATE_FEE_RATE"));
        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\"><div class=\"ct-page ct-b\">");
        h.AppendLine(Agreement.Cl("7. ", "Returned cheques", "البند السابع: الشيكات المرتدة",
            "If any cheque is returned unpaid, the Second Party shall, within " + bl + " days of being notified, replace it with a valid cheque or pay its value in cash; otherwise the First Party may deduct the amount from the reserved amount or recover it from the Second Party.",
            "في حالة ارتداد أي شيك دون صرف، يلتزم الطرف الثاني خلال " + bl + " يوماً من تاريخ إخطاره باستبداله بشيك صالح أو سداد قيمته نقداً، وإلا جاز للطرف الأول خصم قيمته من المبلغ المحتجز أو الرجوع بها على الطرف الثاني."));
        h.AppendLine(Agreement.Cl("8. ", "Late payment", "البند الثامن: غرامة التأخير",
            "Any amount due from the Second Party and not paid on time bears a late fee of " + late + " per annum from its due date until full payment.",
            "يستحق على أي مبلغ مستحق على الطرف الثاني ولم يُسدد في موعده غرامة تأخير بنسبة " + late + " سنوياً من تاريخ الاستحقاق وحتى تمام السداد."));
        h.AppendLine(Agreement.Cl("9. ", "Fees and expenses", "البند التاسع: المصروفات والرسوم",
            "The following charges were deducted when determining the price: administrative expenses " + Agreement.A2(a.Adm) + " " + cur + ", legal fees " + Agreement.A2(a.Leg) + " " + cur + " and valuation expenses " + Agreement.A2(a.Val) + " " + cur + ". Each Party otherwise bears its own costs.",
            "تم خصم المصروفات التالية عند تحديد الثمن: مصروفات إدارية " + Agreement.A2(a.Adm) + " " + cur + "، ورسوم قانونية " + Agreement.A2(a.Leg) + " " + cur + "، ومصروفات تقييم " + Agreement.A2(a.Val) + " " + cur + ". ويتحمل كل طرف فيما عدا ذلك مصروفاته الخاصة."));
        h.AppendLine(Agreement.Cl("10. ", "Confidentiality", "البند العاشر: السرية",
            "Each Party keeps the terms of this Agreement and the investors' data confidential and uses them only to perform this Agreement, subject to the applicable laws.",
            "يلتزم كل طرف بالحفاظ على سرية شروط هذا العقد وبيانات المستثمرين وعدم استخدامها إلا في تنفيذ هذا العقد، وذلك مع مراعاة القوانين المعمول بها."));
        h.AppendLine(Agreement.Cl("11. ", "Governing law and disputes", "البند الحادي عشر: القانون الواجب التطبيق وفض المنازعات",
            "This Agreement is governed by the laws of the Arab Republic of Egypt. The courts of " + bl + " have jurisdiction over any dispute arising from it.",
            "يخضع هذا العقد لقوانين جمهورية مصر العربية، وتختص محاكم " + bl + " بنظر أي نزاع ينشأ عن تفسيره أو تنفيذه."));
        h.AppendLine(Agreement.Cl("12. ", "Notices", "البند الثاني عشر: الإخطارات",
            "Notices are valid when delivered to the addresses stated above, or to any other address a Party notifies to the other in writing.",
            "تكون الإخطارات صحيحة ومنتجة لآثارها متى أُرسلت إلى العناوين المبينة أعلاه، أو إلى أي عنوان آخر يخطر به أحد الطرفين الآخر كتابياً."));
        h.AppendLine(Agreement.Cl("13. ", "Copies and language", "البند الثالث عشر: النسخ واللغة",
            "This Agreement is made in two originals, one for each Party, in Arabic and English. In case of any difference, the Arabic text prevails.",
            "حُرر هذا العقد من نسختين أصليتين بيد كل طرف نسخة للعمل بموجبها، باللغتين العربية والإنجليزية، وفي حالة الاختلاف يُعتد بالنص العربي."));
        h.AppendLine("<div class=\"ct-sign\"><div><h4>First Party (Buyer) &middot; الطرف الأول (المشتري)</h4>CAPITRA<br>Name / الاسم: " + bl + "<br>Title / الصفة: " + bl + "<br>Signature / التوقيع: " + bl + "</div>"
            + "<div><h4>Second Party (Seller) &middot; الطرف الثاني (البائع)</h4>" + E(a.Head.Text("LEGAL_NAME_EN")) + "<br>Name / الاسم: " + bl + "<br>Title / الصفة: " + bl + "<br>Signature / التوقيع: " + bl + "</div></div></div></div>");
        return new { ok = true, html = h.ToString() };
    }
}

/// <summary>App 105 page 216, the PL/SQL region "Agreement Annexes": Annex (A) investor contracts and Annex (B) cheques.</summary>
public sealed class P0216AgreementAnnexes(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 216;

    public string Name => "REGION_AGREEMENT_ANNEXES";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P216_HEADER, items, ct);
        if (head is null)
            return new { ok = true, html = string.Empty };

        var cur = E(head.Text("CURRENCY_CODE"));
        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\"><div class=\"ct-page ct-c\"><section class=\"ct-annex\"><h2><span>Annex (A) &middot; Investor contracts</span><span class=\"ar\">الملحق (أ) &middot; عقود المستثمرين</span></h2><table class=\"ct-tbl\"><thead><tr>"
            + "<th>#</th><th>Investor<small>المستثمر</small></th><th>Contract no.<small>رقم العقد</small></th><th>Unit<small>الوحدة</small></th><th class=\"n\">Outstanding<small>الرصيد القائم</small></th></tr></thead><tbody>");
        var n = 0;
        decimal tot = 0;
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P216_CONTRACTS, items, ct))
        {
            n++;
            var ov = r.Number("OV") ?? 0;
            tot += ov;
            var ne = r.Text("NE");
            var na = r.Text("NA");
            h.AppendLine("<tr><td>" + n + "</td><td>" + E(ne) + (na is not null && na != ne ? "<br><span class=\"ar\" style=\"display:block\">" + E(na) + "</span>" : string.Empty) + "</td><td>" + E(r.Text("SELLER_CONTRACT_NO")) + "</td><td>" + E(r.Text("UNIT_CODE")) + "</td><td class=\"n\">" + Agreement.A2(ov) + "</td></tr>");
        }

        h.AppendLine("<tr class=\"tot\"><td colspan=\"4\">Total &middot; الإجمالي</td><td class=\"n\">" + Agreement.A2(tot) + " " + cur + "</td></tr></tbody></table></section>");
        n = 0;
        decimal tot2 = 0;
        h.AppendLine("<section class=\"ct-annex\"><h2><span>Annex (B) &middot; Cheques</span><span class=\"ar\">الملحق (ب) &middot; الشيكات</span></h2><table class=\"ct-tbl\"><thead><tr>"
            + "<th>#</th><th>Investor<small>المستثمر</small></th><th>Cheque no.<small>رقم الشيك</small></th><th>Bank<small>البنك</small></th><th>Due date<small>تاريخ الاستحقاق</small></th><th class=\"n\">Amount<small>المبلغ</small></th></tr></thead><tbody>");
        foreach (var c in await reader.QueryAsync(MtgApexPageSql.P216_CHEQUE_LIST, items, ct))
        {
            n++;
            tot2 += c.Number("CHEQUE_AMOUNT") ?? 0;
            h.AppendLine("<tr><td>" + n + "</td><td>" + E(c.Text("NE")) + "</td><td>" + E(c.Text("CHEQUE_NO")) + "</td><td>" + E(c.Text("BANK_NAME")) + "</td><td>" + Agreement.Dt(c.Date("DUE_DATE")) + "</td><td class=\"n\">" + Agreement.A2(c.Number("CHEQUE_AMOUNT")) + "</td></tr>");
        }

        h.AppendLine("<tr class=\"tot\"><td colspan=\"5\">Total &middot; الإجمالي</td><td class=\"n\">" + Agreement.A2(tot2) + " " + cur + "</td></tr></tbody></table></section></div></div>");
        return new { ok = true, html = h.ToString() };
    }
}
