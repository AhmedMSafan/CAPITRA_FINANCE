using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 217 "Portfolio Disbursement": the PL/SQL region "Disbursement", ported.</summary>
public sealed class P0217Disbursement(IApexSqlReader reader, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 217;

    public string Name => "REGION_DISBURSEMENT";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var head = await reader.RowAsync(MtgApexPageSql.P217_HEADER, items, ct);
        var h = new StringBuilder();
        if (head is null)
        {
            // exception when no_data_found
            h.AppendLine($$"""
            <div class="fxd"><div class="fxd-card fxd-empty-page" style="max-width:520px;margin:60px auto;text-align:center;padding:40px 28px"><span class="fa fa-folder-open-o" style="font-size:30px;color:var(--teal-400)" aria-hidden="true"></span><h2>{{T("Portfolio not found", "المحفظة غير موجودة")}}</h2><p style="color:var(--muted)">{{T("Open this page from the Disbursement task in your inbox.", "افتح هذه الصفحة من مهمة الصرف في صندوقك.")}}</p></div></div>
            """);
            return new { ok = true, html = h.ToString() };
        }

        var portfolioId = request.Item("P217_PORTFOLIO_ID");
        var pno = head.Text("PORTFOLIO_NO");
        var status = head.Text("PORTFOLIO_STATUS_CODE");
        var dev = head.Text("DEV");
        var cur = head.Text("CURRENCY_CODE");
        string? statusName;
        try
        {
            statusName = (await reader.RowAsync(MtgApexPageSql.P217_STATUS_NAME, Bind("STATUS", status), ct) ?? throw new ApexProcessException("ORA-01403: no data found")).Text("STATUS_NAME_EN");
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            statusName = P0203Html.Nice(status);
        }

        var taskUrl = ApexPages.Url(105, 921, "P921_TASK_ID", request.Item("P217_TASK_ID"), null);
        var inboxUrl = ApexPages.Url(105, 920);
        decimal done;
        try
        {
            done = (await reader.RowAsync(MtgApexPageSql.P217_DONE, items, ct))?.Number("N") ?? 0;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            done = 0;
        }

        var b = new StringBuilder("[");
        foreach (var r in await reader.QueryAsync(MtgApexPageSql.P217_BANKS, Bind("CUR", cur), ct))
        {
            b.Append(b.Length > 1 ? "," : string.Empty)
                .Append("{\"b\":").Append(P0203Html.Num(r.Number("BANK_ID"))).Append(",\"bn\":\"").Append(Json(r.Text("BANK_NAME_EN"))).Append("\",\"a\":").Append(P0203Html.Num(r.Number("ACCOUNT_ID")))
                .Append(",\"an\":\"").Append(Json(r.Text("ACCOUNT_NO") + " - " + r.Text("ACCOUNT_REF"))).Append("\",\"t\":\"").Append(Json(r.Text("ACCOUNT_TYPE_CODE")))
                .Append("\",\"p\":\"").Append(Json(r.Text("PURPOSE_EN"))).Append("\"}");
        }
        var banks = b.Append(']').ToString();
        var today = MtgAjax.CairoNow(clock).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (request.Item("REQUEST") == "VIEW")
        {
            h.AppendLine($$"""
            <style>.wb-dbar,.wb-crumbs,.wb-drawer,.wb-scrim,[data-screen],#wbBulk,#wbAddInq,.wb-icon-btn,[data-exclude],.lr-save,.wb-saved{display:none!important}.wb-toggle,.lr-chk,#lrCard input,#lrCard textarea{pointer-events:none!important}.wb-toggle button:not([aria-pressed=true]){display:none!important}.fxd-wrap.wb{padding-bottom:28px!important}.wb-ro{display:flex;align-items:center;gap:10px;padding:10px 28px;background:#FFF6EC;border-bottom:1px solid #F6D6B9;color:#7A4510;font-size:13.5px;font-weight:600}.wb-ro a{margin-inline-start:auto;color:#00636D!important;font-weight:700}</style>
            """);
            h.AppendLine($$"""
            <div class="wb-ro"><span class="fa fa-eye" aria-hidden="true"></span>{{T("Read-only view. Decisions are made from the workflow task.", "عرض فقط. تُتخذ القرارات من مهمة سير العمل.")}}<a href="{{Url(203, "P203_PORTFOLIO_ID", portfolioId)}}">{{T("Back to portfolio", "العودة إلى المحفظة")}} &rsaquo;</a></div>
            """);
        }

        if (done > 0)
            h.AppendLine($$"""
            <style>.wb-dbar{display:none!important}.fxd-wrap.wb{padding-bottom:28px!important}</style>
            """);

        h.AppendLine($$"""
        <div class="fxd" id="dsRoot" data-inbox="{{Attr(inboxUrl)}}" data-cur="{{E(cur)}}" data-done="{{(done > 0 ? "Y" : "N")}}" data-today="{{today}}" data-banks="{{Attr(banks)}}">
        """);
        h.AppendLine($$"""
        <header class="fxd-hero"><nav class="wb-crumbs" aria-label="Breadcrumb"><a href="{{Attr(taskUrl)}}"><span class="fa fa-arrow-left" aria-hidden="true"></span> {{T("Back to task", "العودة إلى المهمة")}}</a><span>/</span><span>{{T("Disbursement", "الصرف")}}</span></nav>
        """);
        h.AppendLine($$"""
        <div class="fxd-row"><div class="fxd-anim"><div class="fxd-eyebrow"><i></i>{{T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة")}}</div><h1 class="fxd-h1">{{T("Disbursement", "الصرف")}}</h1><p class="fxd-lead">{{T("Pay the developer, create the investor repayment and income schedules, store the cheques in the wallet and post the accounting entries in one step.", "ادفع للمطور، وأنشئ جداول سداد المستثمرين والإيراد، واحفظ الشيكات في المحفظة، ورحّل القيود المحاسبية في خطوة واحدة.")}}</p></div><div class="wb-facts fxd-anim d1"><div><span>{{T("Portfolio", "المحفظة")}}</span><strong>{{E(pno)}}</strong></div><div><span>{{T("Developer", "المطور")}}</span><strong>{{E(dev)}}</strong></div><div><span>{{T("Currency", "العملة")}}</span><strong>{{E(cur)}}</strong></div><div><span>{{T("Status", "الحالة")}}</span><span class="wb-stage">{{E(statusName)}}</span></div></div></div>
        """);
        h.AppendLine($$"""
        <div class="wb-steps" id="wbSteps"><div class="wb-step"><b>1</b><span>{{T("Payment", "الدفع")}}</span></div><div class="wb-step"><b>2</b><span>{{T("Charges", "المصروفات")}}</span></div><div class="wb-step"><b>3</b><span>{{T("Schedules and entries", "الجداول والقيود")}}</span></div><div class="wb-step"><b>4</b><span>{{T("Complete", "الإكمال")}}</span></div></div></header>
        """);
        h.AppendLine($$"""
        <div class="fxd-wrap wb"><div class="wb-grid"><div class="wb-stack">
        """);
        h.AppendLine($$"""
        <section class="fxd-card"><div class="fxd-card-h"><h2><span class="fa fa-university" aria-hidden="true"></span>{{T("Payment to the developer", "الدفع للمطور")}}</h2><span class="fxd-label">{{E(cur)}}</span></div><div class="ds-fields"><div class="wb-f"><label for="dsDate">{{T("Disbursement date", "تاريخ الصرف")}} *</label><input id="dsDate" type="date" class="ds-in"></div><div class="wb-f"><label for="dsRef">{{T("Payment reference", "مرجع الدفع")}} *</label><input id="dsRef" maxlength="200" class="ds-in" placeholder="{{Attr("Transfer / cheque no.")}}"></div><div class="wb-f"><label for="dsBank">{{T("Bank", "البنك")}} *</label><select id="dsBank" class="ds-in"></select></div><div class="wb-f"><label for="dsAcc">{{T("Bank account", "الحساب البنكي")}} *</label><select id="dsAcc" class="ds-in"></select></div></div><div class="ds-glline" id="dsGlLine"></div></section>
        """);
        h.AppendLine($$"""
        <section class="fxd-card"><div class="fxd-card-h"><h2><span class="fa fa-file-text-o" aria-hidden="true"></span>{{T("Margin and charges", "الهامش والمصروفات")}}</h2><span class="ds-chip ds-edit">{{T("Choose how each one is recognised", "اختر طريقة إثبات كل بند")}}</span></div><div class="wb-tbl-wrap"><table class="wb-tbl ds-ch"><thead><tr><th>{{T("Item", "البند")}}</th><th class="fxd-num">{{T("Financial review", "المراجعة المالية")}}</th><th class="fxd-num">{{T("Final amount", "المبلغ النهائي")}}</th><th>{{T("Income recognition", "إثبات الإيراد")}}</th><th>{{T("Waive", "إعفاء")}}</th></tr></thead><tbody><tr data-ch="M"><td><strong>{{T("Margin", "الهامش")}}</strong><small>{{T("Main profit of the deal", "الربح الأساسي للصفقة")}}</small></td><td class="fxd-num"><b class="ds-fr"></b></td><td class="fxd-num"><b class="ds-mfin"></b></td><td><div class="ds-tr" role="group" aria-label="Income recognition"><button type="button" class="ds-in" data-v="PRORATE" aria-pressed="false">{{T("Pro-rate", "توزيع زمني")}}</button><button type="button" class="ds-in" data-v="IMMEDIATE" aria-pressed="false">{{T("Immediate", "فوري")}}</button><button type="button" data-v="LATER" disabled title="Coming soon">{{T("Collect later", "تحصيل لاحق")}}</button></div><span class="ds-std" hidden>{{T("Not IFRS 9 / EAS 47", "غير متوافق مع IFRS 9 / EAS 47")}}</span></td><td class="ds-na">&ndash;</td></tr><tr data-ch="A"><td><strong>{{T("Admin expense", "المصروفات الإدارية")}}</strong><small>{{T("Charged to the developer", "تُحمّل على المطور")}}</small></td><td class="fxd-num"><b class="ds-fr"></b></td><td class="fxd-num"><input class="ds-fin ds-in" type="number" step="0.01" min="0" inputmode="decimal" aria-label="Admin expense final amount"></td><td><div class="ds-tr" role="group" aria-label="Income recognition"><button type="button" class="ds-in" data-v="PRORATE" aria-pressed="false">{{T("Pro-rate", "توزيع زمني")}}</button><button type="button" class="ds-in" data-v="IMMEDIATE" aria-pressed="false">{{T("Immediate", "فوري")}}</button><button type="button" data-v="LATER" disabled title="Coming soon">{{T("Collect later", "تحصيل لاحق")}}</button></div></td><td><label class="ds-sw"><input type="checkbox" class="ds-waive ds-in" aria-label="Waive admin expense"><i></i></label></td></tr><tr data-ch="L"><td><strong>{{T("Legal fees", "الرسوم القانونية")}}</strong><small>{{T("Charged to the developer", "تُحمّل على المطور")}}</small></td><td class="fxd-num"><b class="ds-fr"></b></td><td class="fxd-num"><input class="ds-fin ds-in" type="number" step="0.01" min="0" inputmode="decimal" aria-label="Legal fees final amount"></td><td><div class="ds-tr" role="group" aria-label="Income recognition"><button type="button" class="ds-in" data-v="PRORATE" aria-pressed="false">{{T("Pro-rate", "توزيع زمني")}}</button><button type="button" class="ds-in" data-v="IMMEDIATE" aria-pressed="false">{{T("Immediate", "فوري")}}</button><button type="button" data-v="LATER" disabled title="Coming soon">{{T("Collect later", "تحصيل لاحق")}}</button></div></td><td><label class="ds-sw"><input type="checkbox" class="ds-waive ds-in" aria-label="Waive legal fees"><i></i></label></td></tr><tr data-ch="V"><td><strong>{{T("Valuation expense", "مصروفات التقييم")}}</strong><small>{{T("Charged to the developer", "تُحمّل على المطور")}}</small></td><td class="fxd-num"><b class="ds-fr"></b></td><td class="fxd-num"><input class="ds-fin ds-in" type="number" step="0.01" min="0" inputmode="decimal" aria-label="Valuation expense final amount"></td><td><div class="ds-tr" role="group" aria-label="Income recognition"><button type="button" class="ds-in" data-v="PRORATE" aria-pressed="false">{{T("Pro-rate", "توزيع زمني")}}</button><button type="button" class="ds-in" data-v="IMMEDIATE" aria-pressed="false">{{T("Immediate", "فوري")}}</button><button type="button" data-v="LATER" disabled title="Coming soon">{{T("Collect later", "تحصيل لاحق")}}</button></div></td><td><label class="ds-sw"><input type="checkbox" class="ds-waive ds-in" aria-label="Waive valuation expense"><i></i></label></td></tr></tbody></table></div><div class="ds-note" id="dsTrNote"></div></section>
        """);
        h.AppendLine($$"""
        <section class="fxd-card"><div class="ds-tabs"><div class="wb-seg" role="group" aria-label="Details" id="dsTabs"><button type="button" data-tab="sch" aria-pressed="true">{{T("Repayment schedule", "جدول السداد")}}</button><button type="button" data-tab="inc" aria-pressed="false">{{T("Unearned income", "الإيراد المؤجل")}}</button><button type="button" data-tab="gl" aria-pressed="false">{{T("GL entries", "القيود")}}</button><button type="button" data-tab="crm" aria-pressed="false">{{T("CRM clients", "العملاء")}}</button><button type="button" data-tab="wal" aria-pressed="false">{{T("Cheque wallet", "محفظة الشيكات")}}</button></div><select id="dsAccSel" aria-label="Investor"></select></div><div class="ds-load" id="dsBusy"><i></i><span>{{T("Calculating...", "جارٍ الحساب...")}}</span></div><div id="dsPanel"></div></section></div>
        """);
        h.AppendLine($$"""
        <aside class="wb-stack"><section class="fxd-card"><div class="fxd-card-h"><h2><span class="fa fa-calculator" aria-hidden="true"></span>{{T("Disbursement summary", "ملخص الصرف")}}</h2><span class="fxd-label">{{E(cur)}}</span></div><ul class="ds-wf" id="dsWf"></ul><div class="ds-total" id="dsTotal"><span>{{T("Payable to the developer", "المستحق للمطور")}}</span><b id="dsPay">-</b><small id="dsBoard"></small></div><div class="ds-alerts" id="dsAlerts"></div><div id="dsDone"></div></section></aside></div></div>
        """);
        h.AppendLine($$"""
        <div class="wb-dbar"><div class="wb-dbar-in"><textarea id="wbComment" aria-label="Comment" placeholder="Comment (required)"></textarea><div class="wb-hint" id="wbHint"></div><button type="button" class="fxd-btn fxd-btn--ok" id="dsComplete"><span class="fa fa-check-circle" aria-hidden="true"></span>{{T("Complete disbursement", "إكمال الصرف")}}</button></div></div>
        """);
        h.AppendLine($$"""
        <div class="wb-scrim" id="wbScrim"></div>
        """);
        h.AppendLine($$"""
        <div class="wb-mask" id="wbMask"><div class="wb-modal" role="dialog" aria-modal="true" aria-labelledby="wbMT"><div class="wb-m-ico"></div><h2 id="wbMT"></h2><p></p><div class="wb-m-sum"></div><div class="wb-m-act"><button type="button" class="fxd-btn fxd-btn--ghost" data-m="no">Cancel</button><button type="button" class="fxd-btn" data-m="yes">OK</button></div></div></div>
        """);
        h.AppendLine($$"""
        <div class="wb-toasts" id="wbToasts" aria-live="polite"></div>
        """);
        h.AppendLine($$"""
        </div>
        """);
        return new { ok = true, html = h.ToString() };
    }

    private static Dictionary<string, string?> Bind(string name, string? value) => new(StringComparer.OrdinalIgnoreCase) { [name] = value };

    /// <summary>apex_escape.json: the text as a JSON string body.</summary>
    private static string Json(string? s)
    {
        var j = new StringBuilder();
        foreach (var c in s ?? string.Empty)
        {
            j.Append(c switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '/' => "\\/",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                < ' ' => "\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture),
                _ => c.ToString(),
            });
        }
        return j.ToString();
    }
}

/// <summary>App 105 page 217: the PL/SQL region "Step Actions" (the decision bar follows the step's Allowed Actions).</summary>
public sealed class P0217StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 217;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        new { ok = true, html = await steps.StepActionsAsync(217, request, ct, "dsComplete", "dsNoReject", "Complete disbursement", "إكمال الصرف") };
}

/// <summary>The x01 to x13 of DS_PREVIEW and DS_COMPLETE.</summary>
internal static class P0217Input
{
    /// <summary>to_date(nullif(trim(x),''),'YYYY-MM-DD').</summary>
    public static DateOnly? Date(string? x) =>
        string.IsNullOrWhiteSpace(x) ? null
        : DateOnly.TryParseExact(x.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : throw new ApexProcessException("ORA-01861: literal does not match format string");

    public static MtgDisbursementInput Read(ApexRequest request) => new(
        MtgAjax.Long(request.Item("P217_PORTFOLIO_ID")) ?? 0,
        MtgAjax.Number(request.X03),
        MtgAjax.Number(request.X04),
        MtgAjax.Number(request.X05),
        Date(request.X01),
        MtgAjax.Long(request.X02),
        request.X10,
        request.X11,
        request.X12,
        request.X13);
}

/// <summary>App 105 page 217, Ajax Callback DS_PREVIEW: FNV_PP_DISB_PKG.preview_json, or {success: false, message: sqlerrm}.</summary>
public sealed class P0217DsPreview(MtgDisbursementPackage disbursement) : IApexProcess
{
    public int App => 105;

    public int Page => 217;

    public string Name => "DS_PREVIEW";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            return await disbursement.PreviewJsonAsync(P0217Input.Read(request), ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new { success = false, message = e.Message };
        }
    }
}

/// <summary>
/// App 105 page 217, Ajax Callback DS_COMPLETE: FNV_PP_DISB_PKG.complete in one transaction (CRM clients, wallet
/// cheques, schedules, GL tran set and workflow); any error rolls everything back.
/// </summary>
public sealed class P0217DsComplete(ICapitraDbContext db, MtgDisbursementPackage disbursement) : IApexProcess
{
    public int App => 105;

    public int Page => 217;

    public string Name => "DS_COMPLETE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => MtgAjax.RunJsonAsync(db, async () =>
    {
        var (ts, _) = await disbursement.CompleteAsync(
            P0217Input.Read(request),
            MtgAjax.Long(request.X09) ?? MtgAjax.Long(request.Item("P217_TASK_ID")),
            MtgAjax.Long(request.X06),
            MtgAjax.Trim(request.X07),
            MtgAjax.Trim(request.X08),
            ct);
        return new { success = true, tranSetId = ts, message = "Disbursement " + ts + " completed." };
    }, ct);
}
