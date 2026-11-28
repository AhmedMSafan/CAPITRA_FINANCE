using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 203 "Portfolio Workspace", the PL/SQL region "Financial Terms Editor" (the rates and fees dialog).</summary>
public sealed class P0203FinancialTermsEditor(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 203;

    public string Name => "REGION_FINANCIAL_TERMS_EDITOR";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (!long.TryParse(request.Item("P203_PORTFOLIO_ID"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            return new { ok = true, html = string.Empty };
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == id, ct);
        if (p is null)
            return new { ok = true, html = string.Empty };

        var h = new StringBuilder();
        h.AppendLine("<style>#ptMask{--ink:#0B2533;--line:#DFE8EC;--teal-600:#00636D;--teal-800:#014B55;font-family:Inter,Segoe UI,Arial,sans-serif;position:fixed;inset:0;z-index:900;display:none;place-items:center;padding:20px;background:rgba(3,24,30,.55);backdrop-filter:blur(3px)}#ptMask.on{display:grid}html[dir=rtl] #ptMask{font-family:Cairo,Segoe UI,Arial,sans-serif}#ptBox{width:min(620px,100%);max-height:92vh;overflow:auto;background:#fff;border-radius:16px;box-shadow:0 30px 80px rgba(3,24,30,.35)}#ptBox header{display:flex;align-items:center;gap:12px;padding:18px 22px;border-bottom:1px solid #E6EDF0}#ptBox header h2{margin:0;font-size:18px;color:#0B2533}#ptBox header button{margin-inline-start:auto;width:34px;height:34px;border:0;border-radius:10px;background:#EEF2F4;cursor:pointer}.pt-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;padding:18px 22px}.pt-f{display:flex;flex-direction:column;gap:6px}.pt-f label{font-size:12.5px;font-weight:600;color:#29495A}.pt-in{position:relative}.pt-in input{width:100%;box-sizing:border-box;height:42px;border:1px solid #C9D8DD;padding:0 58px 0 12px;font:inherit;font-size:14px}html[dir=rtl] .pt-in input{padding:0 12px 0 58px}.pt-in input:focus{outline:0;border-color:#00636D;box-shadow:0 0 0 3px rgba(0,99,109,.12)}.pt-in span{position:absolute;inset-inline-end:6px;top:50%;transform:translateY(-50%);min-width:30px;height:28px;padding:0 6px;display:grid;place-items:center;background:#EAF4F4;color:#00636D;font-size:12px;font-weight:700}.pt-note{margin:0 22px;padding:10px 12px;background:#FFF6EC;border:1px solid #F6D6B9;color:#7A4510;font-size:12.5px;border-radius:10px}.pt-err{margin:12px 22px 0;padding:10px 12px;background:#FCEBE8;color:#C43325;font-size:13px;border-radius:10px;display:none}.pt-err.on{display:block}#ptBox footer{display:flex;justify-content:flex-end;gap:10px;padding:16px 22px;border-top:1px solid #E6EDF0;margin-top:16px}@media(max-width:640px){.pt-grid{grid-template-columns:1fr}}</style>");
        h.AppendLine("<div id=\"ptMask\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"ptTitle\"><div id=\"ptBox\"><header><span class=\"fa fa-percent\" aria-hidden=\"true\" style=\"color:#00636D\"></span><h2 id=\"ptTitle\"><span data-finova-en=\"Financial rates & fees\" data-finova-ar=\"النسب والرسوم المالية\">Financial rates & fees</span></h2><button type=\"button\" data-pt-close aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></header>");
        h.AppendLine("<div class=\"pt-err\" id=\"ptErr\" role=\"alert\"></div><div class=\"pt-grid\">");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptCov\"><span data-finova-en=\"Coverage percentage\" data-finova-ar=\"نسبة التغطية\">Coverage percentage</span></label><div class=\"pt-in\"><input id=\"ptCov\" type=\"number\" step=\"0.01\" min=\"0\" max=\"100\" inputmode=\"decimal\" value=\"" + V(p.CoveragePct) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptMar\"><span data-finova-en=\"Margin percentage\" data-finova-ar=\"نسبة الهامش\">Margin percentage</span></label><div class=\"pt-in\"><input id=\"ptMar\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.MarginPercentage) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptPv\"><span data-finova-en=\"Present value calculation rate\" data-finova-ar=\"معدل حساب القيمة الحالية\">Present value calculation rate</span></label><div class=\"pt-in\"><input id=\"ptPv\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.PvRate) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptRes\"><span data-finova-en=\"Reserved amount percentage\" data-finova-ar=\"نسبة المبلغ المحتجز\">Reserved amount percentage</span></label><div class=\"pt-in\"><input id=\"ptRes\" type=\"number\" step=\"0.01\" min=\"0\" max=\"100\" inputmode=\"decimal\" value=\"" + V(p.ReservedAmountPercent) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptLate\"><span data-finova-en=\"Late fees rate\" data-finova-ar=\"نسبة غرامة التأخير\">Late fees rate</span></label><div class=\"pt-in\"><input id=\"ptLate\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.LateFeeRate) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptAdm\"><span data-finova-en=\"Admin expense percentage\" data-finova-ar=\"نسبة المصروفات الإدارية\">Admin expense percentage</span></label><div class=\"pt-in\"><input id=\"ptAdm\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.AdminExpensePercent) + "\"><span>%</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptLeg\"><span data-finova-en=\"Legal fees per unit\" data-finova-ar=\"الرسوم القانونية لكل وحدة\">Legal fees per unit</span></label><div class=\"pt-in\"><input id=\"ptLeg\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.LegalFeePerUnit) + "\"><span>" + E(p.CurrencyCode) + "</span></div></div>");
        h.AppendLine("<div class=\"pt-f\"><label for=\"ptVal\"><span data-finova-en=\"Valuation expense per unit\" data-finova-ar=\"مصروف التقييم لكل وحدة\">Valuation expense per unit</span></label><div class=\"pt-in\"><input id=\"ptVal\" type=\"number\" step=\"0.01\" min=\"0\" inputmode=\"decimal\" value=\"" + V(p.ValuationFeePerUnit) + "\"><span>" + E(p.CurrencyCode) + "</span></div></div>");
        h.AppendLine("</div><div class=\"pt-note\"><span data-finova-en=\"Changes apply from now on. A proposed purchase value that was already calculated changes only when the Financial Review is saved again.\" data-finova-ar=\"تسري التغييرات من الآن. قيمة الشراء المقترحة المحسوبة سابقاً لا تتغير إلا عند حفظ المراجعة المالية مرة أخرى.\">Changes apply from now on. A proposed purchase value that was already calculated changes only when the Financial Review is saved again.</span></div>");
        h.AppendLine("<footer><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-pt-close><span data-finova-en=\"Cancel\" data-finova-ar=\"إلغاء\">Cancel</span></button><button type=\"button\" class=\"fxd-btn fxd-btn--teal\" id=\"ptSave\"><span class=\"fa fa-check\" aria-hidden=\"true\"></span><span data-finova-en=\"Save\" data-finova-ar=\"حفظ\">Save</span></button></footer></div></div>");
        h.AppendLine("<script>(function(){var m=document.getElementById(\"ptMask\"),e=document.getElementById(\"ptErr\");if(!m)return;document.body.appendChild(m);function open(){e.classList.remove(\"on\");m.classList.add(\"on\");setTimeout(function(){document.getElementById(\"ptCov\").focus();},50);}function close(){m.classList.remove(\"on\");}document.addEventListener(\"click\",function(ev){if(ev.target.closest(\"#poTermsEdit\")){open();return;}if(ev.target.closest(\"[data-pt-close]\") + ev.target===m){close();return;}var s=ev.target.closest(\"#ptSave\");if(!s)return;s.disabled=true;var g=function(i){return document.getElementById(i).value;};apex.server.process(\"FIN_TERMS_SAVE\",{x01:g(\"ptCov\"),x02:g(\"ptMar\"),x03:g(\"ptPv\"),x04:g(\"ptRes\"),x05:g(\"ptLate\"),x06:g(\"ptAdm\"),x07:g(\"ptLeg\"),x08:g(\"ptVal\")},{pageItems:\"#P203_PORTFOLIO_ID\",dataType:\"json\",success:function(d){if(d&&d.success===false){s.disabled=false;e.textContent=d.message + \"Not saved.\";e.classList.add(\"on\");}else{location.reload();}},error:function(j){s.disabled=false;e.textContent=(j&&j.responseText) + \"Server error\";e.classList.add(\"on\");}});});document.addEventListener(\"keydown\",function(ev){if(ev.key===\"Escape\")close();});})();</script>");
        return new { ok = true, html = h.ToString() };
    }

    /// <summary>v(p): to_char(p,'TM9').</summary>
    private static string V(decimal? n) => n?.ToString("0.############", CultureInfo.InvariantCulture) ?? string.Empty;
}

/// <summary>App 105 page 203, Ajax Callback FIN_TERMS_SAVE: the rates and fees (x01-x08) of the portfolio.</summary>
public sealed class P0203FinancialTermsSave(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 203;

    public string Name => "FIN_TERMS_SAVE";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            if (!long.TryParse(request.Item("P203_PORTFOLIO_ID"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                throw new ApexProcessException("ORA-20901: Portfolio ID is missing from page session state.");
            var cov = N(request.X01);
            var mar = N(request.X02);
            var pv = N(request.X03);
            var res = N(request.X04);
            var late = N(request.X05);
            var adm = N(request.X06);
            var leg = N(request.X07);
            var val = N(request.X08);
            if (cov is < 0 or > 100)
                throw new ApexProcessException("ORA-20902: Coverage Percentage must be between 0 and 100.");
            if (res is < 0 or > 100)
                throw new ApexProcessException("ORA-20903: Reserved Amount Percentage must be between 0 and 100.");
            if (new[] { mar, pv, late, adm, leg, val }.Min(x => x ?? 0) < 0)
                throw new ApexProcessException("ORA-20904: Rates and fees cannot be negative.");
            var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == id, ct)
                ?? throw new ApexProcessException("ORA-20905: Portfolio not found.");
            p.CoveragePct = cov;
            p.MarginPercentage = mar;
            p.PvRate = pv ?? 0;
            p.ReservedAmountPercent = res ?? 0;
            p.LateFeeRate = late ?? 0;
            p.AdminExpensePercent = adm;
            p.LegalFeePerUnit = leg;
            p.ValuationFeePerUnit = val;
            await db.SaveChangesAsync(ct);
            return new { success = true };
        }
        catch (ApexProcessException e)
        {
            return new { success = false, message = e.Message };
        }
    }

    // n(p): to_number of the text, null when empty
    private static decimal? N(string? p) =>
        string.IsNullOrWhiteSpace(p) ? null
        : decimal.TryParse(p.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d
        : throw new ApexProcessException("ORA-01722: invalid number");
}
