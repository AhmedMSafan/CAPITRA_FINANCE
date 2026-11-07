using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 608 "Portfolio Acquisition Settlement", Before Header "Load Settlement".</summary>
public sealed class P0608LoadSettlement(ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 608;

    public string Name => "PAGE_LOAD";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = ApexItems.Read(request.X01);
        var set = new Dictionary<string, string?>();
        var pid = MtgAjax.Long(items.GetValueOrDefault("P608_PORTFOLIO_ID"));
        if (pid is null)
            return new { ok = true, items = set };
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().Where(x => x.PortfolioId == pid)
            .Select(x => new
            {
                x.PortfolioNo,
                x.PortfolioStatusCode,
                x.ProposedPurchaseValue,
                x.OutstandingValue,
                x.CurrencyCode,
                Dev = db.Set<MtgDeveloper>().Where(d => d.DeveloperId == x.DeveloperId).Select(d => new { d.LegalNameEn, d.LegalNameAr }).FirstOrDefault(),
                Prj = db.Set<MtgProject>().Where(r => r.ProjectId == x.ProjectId).Select(r => new { r.ProjectNameEn, r.ProjectNameAr }).FirstOrDefault(),
            }).FirstOrDefaultAsync(ct);
        if (p is null || p.Dev is null || p.Prj is null)
            return new { ok = true, items = set };
        set["P608_PORTFOLIO_NO"] = p.PortfolioNo;
        set["P608_DEVELOPER_PROJECT"] = (p.Dev.LegalNameEn ?? p.Dev.LegalNameAr) + " / " + p.Dev.LegalNameAr + " | " + (p.Prj.ProjectNameEn ?? p.Prj.ProjectNameAr) + " / " + p.Prj.ProjectNameAr;
        set["P608_PORTFOLIO_STATUS"] = p.PortfolioStatusCode;
        set["P608_OUTSTANDING_VALUE"] = Num(p.OutstandingValue);
        set["P608_CURRENCY_CODE"] = p.CurrencyCode;
        set["P608_PURCHASE_VALUE"] = ApexHtml.ToChar(p.ProposedPurchaseValue, "FM999G999G999G990D00");
        var s = await db.Set<MtgPurchasedPortfolioSettlement>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
        if (s is not null)
        {
            set["P608_SETTLEMENT_ID"] = s.SettlementId.ToString(CultureInfo.InvariantCulture);
            set["P608_CONTRACT_EXECUTION_DATE"] = D(s.ContractExecutionDate);
            set["P608_CUT_OFF_DATE"] = D(s.CutOffDate);
            set["P608_ACQUISITION_DATE"] = D(s.AcquisitionDate);
            set["P608_SETTLEMENT_DATE"] = D(s.SettlementDate);
            set["P608_ACCOUNTING_DATE"] = D(s.AccountingDate);
            set["P608_PAYMENT_AMOUNT"] = Num(s.PaymentAmount);
            set["P608_ACQUISITION_COST_AMOUNT"] = Num(s.AcquisitionCostAmount);
            set["P608_PAYMENT_METHOD_CODE"] = s.PaymentMethodCode;
            set["P608_BANK_GL_ACCOUNT_CODE"] = s.BankGlAccountCode;
            set["P608_BANK_NAME"] = s.BankName;
            set["P608_BANK_ACCOUNT_NO"] = s.BankAccountNo;
            set["P608_TRANSFER_REFERENCE"] = s.TransferReference;
            set["P608_CHEQUE_NO"] = s.ChequeNo;
            set["P608_CHEQUE_DATE"] = D(s.ChequeDate);
            set["P608_SELLER_PAYMENT_REFERENCE"] = s.SellerPaymentReference;
            set["P608_SETTLEMENT_STATUS"] = s.SettlementStatusCode;
            set["P608_ACCOUNTING_STATUS"] = s.AccountingStatusCode;
            set["P608_NOTES"] = s.Notes;
        }
        else
        {
            var today = D(DateOnly.FromDateTime(MtgAjax.CairoNow(clock)));
            set["P608_SETTLEMENT_ID"] = null;
            set["P608_SETTLEMENT_STATUS"] = "DRAFT";
            set["P608_ACCOUNTING_STATUS"] = "UNPOSTED";
            set["P608_PAYMENT_AMOUNT"] = Num(p.ProposedPurchaseValue);
            set["P608_ACQUISITION_COST_AMOUNT"] = "0";
            set["P608_CONTRACT_EXECUTION_DATE"] = today;
            set["P608_CUT_OFF_DATE"] = today;
            set["P608_ACQUISITION_DATE"] = today;
            set["P608_SETTLEMENT_DATE"] = today;
            set["P608_ACCOUNTING_DATE"] = today;
        }

        return new { ok = true, items = set };
    }

    private static string? Num(decimal? n) => n is null ? null : P0203Html.Num(n);

    internal static string? D(DateOnly? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>App 105 page 608, After Submit "Save Settlement Draft" (SAVE_DRAFT) and "Confirm Acquisition Settlement" (CONFIRM_SETTLEMENT).</summary>
public sealed class P0608SubmitSettlement(ICapitraDbContext db, MtgSettlementPackage settlements, ICurrentUser user) : IApexProcess
{
    public int App => 105;

    public int Page => 608;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("SAVE_DRAFT" or "CONFIRM_SETTLEMENT"))
            return new { ok = true };
        var me = user.UserName ?? string.Empty;
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var input = new MtgSettlementInput(
                MtgAjax.Long(request.Item("P608_SETTLEMENT_ID")), MtgAjax.Long(request.Item("P608_PORTFOLIO_ID")),
                Date(request, "P608_CONTRACT_EXECUTION_DATE"), Date(request, "P608_CUT_OFF_DATE"), Date(request, "P608_ACQUISITION_DATE"),
                Date(request, "P608_SETTLEMENT_DATE"), Date(request, "P608_ACCOUNTING_DATE"),
                MtgAjax.Number(request.Item("P608_PAYMENT_AMOUNT")), MtgAjax.Number(request.Item("P608_ACQUISITION_COST_AMOUNT")),
                request.Item("P608_PAYMENT_METHOD_CODE"), request.Item("P608_BANK_GL_ACCOUNT_CODE"), request.Item("P608_BANK_NAME"),
                request.Item("P608_BANK_ACCOUNT_NO"), request.Item("P608_TRANSFER_REFERENCE"), request.Item("P608_CHEQUE_NO"),
                Date(request, "P608_CHEQUE_DATE"), request.Item("P608_SELLER_PAYMENT_REFERENCE"), request.Item("P608_NOTES"));
            var id = await settlements.SaveSettlementAsync(input, me, ct);
            var set = new Dictionary<string, string?>
            {
                ["P608_PORTFOLIO_ID"] = request.Item("P608_PORTFOLIO_ID"),
                ["P608_SETTLEMENT_ID"] = id.ToString(CultureInfo.InvariantCulture),
            };
            string message;
            if (req == "CONFIRM_SETTLEMENT")
            {
                await settlements.ConfirmSettlementAsync(id, me, ct);
                set["P608_SETTLEMENT_STATUS"] = "SETTLED";
                set["P608_ACCOUNTING_STATUS"] = "READY";
                set["P608_PORTFOLIO_STATUS"] = "ACTIVATED";
                message = "Acquisition settlement confirmed. Portfolio activated and accounting is ready.";
            }
            else
            {
                message = "Settlement draft saved.";
            }

            await tx.CommitAsync(ct);
            return new { ok = true, message, items = set };
        }
        catch (ApexProcessException e)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = e.Message };
        }
    }

    private static DateOnly? Date(ApexRequest request, string item)
    {
        var v = MtgAjax.Trim(request.Item(item));
        if (v is null)
            return null;
        return DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
            : throw new ApexProcessException("ORA-01861: literal does not match format string");
    }
}
