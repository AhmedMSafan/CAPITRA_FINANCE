using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>The settlement figures save_settlement takes from page 608.</summary>
public sealed record MtgSettlementInput(
    long? SettlementId, long? PortfolioId, DateOnly? ContractExecutionDate, DateOnly? CutOffDate, DateOnly? AcquisitionDate,
    DateOnly? SettlementDate, DateOnly? AccountingDate, decimal? PaymentAmount, decimal? AcquisitionCostAmount, string? PaymentMethodCode,
    string? BankGlAccountCode, string? BankName, string? BankAccountNo, string? TransferReference, string? ChequeNo, DateOnly? ChequeDate,
    string? SellerPaymentReference, string? Notes);

/// <summary>FNV_PORTFOLIO_SETTLEMENT_PKG: the acquisition settlement of an approved portfolio, and its confirmation (ACTIVATED).</summary>
public sealed class MtgSettlementPackage(ICapitraDbContext db, IClock clock)
{
    /// <summary>save_settlement: creates or updates the draft settlement of the portfolio; returns its id.</summary>
    public async Task<long> SaveSettlementAsync(MtgSettlementInput s, string user, CancellationToken ct)
    {
        if (s.PortfolioId is null)
            throw new ApexProcessException("Portfolio is required / يجب تحديد المحفظة");
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == s.PortfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (p.PortfolioStatusCode is not ("APPROVED" or "ACTIVATED"))
            throw new ApexProcessException("Settlement is allowed only after final approval / التسوية مسموح بها فقط بعد الاعتماد النهائي");

        MtgPurchasedPortfolioSettlement row;
        if (s.SettlementId is not null)
        {
            row = await db.Set<MtgPurchasedPortfolioSettlement>().FirstOrDefaultAsync(x => x.SettlementId == s.SettlementId, ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            if (row.SettlementStatusCode == "SETTLED")
                throw new ApexProcessException("Settled record cannot be edited / لا يمكن تعديل تسوية تم اعتمادها");
        }
        else
        {
            if (await db.Set<MtgPurchasedPortfolioSettlement>().AnyAsync(x => x.PortfolioId == s.PortfolioId, ct))
                throw new ApexProcessException("A settlement already exists for this portfolio / توجد تسوية مسجلة مسبقاً لهذه المحفظة");
            row = new MtgPurchasedPortfolioSettlement { PortfolioId = s.PortfolioId.Value, SettlementStatusCode = "DRAFT", AccountingStatusCode = "UNPOSTED" };
            db.Set<MtgPurchasedPortfolioSettlement>().Add(row);
        }

        row.ContractExecutionDate = s.ContractExecutionDate;
        row.CutOffDate = s.CutOffDate;
        row.AcquisitionDate = s.AcquisitionDate;
        row.SettlementDate = s.SettlementDate;
        row.AccountingDate = s.AccountingDate;
        row.ApprovedPurchaseValue = p.ProposedPurchaseValue;
        row.PaymentAmount = s.PaymentAmount;
        row.AcquisitionCostAmount = s.AcquisitionCostAmount ?? 0;
        row.CurrencyCode = p.CurrencyCode;
        row.PaymentMethodCode = Up(s.PaymentMethodCode);
        row.BankGlAccountCode = Up(s.BankGlAccountCode);
        row.BankName = Trim(s.BankName);
        row.BankAccountNo = Trim(s.BankAccountNo);
        row.TransferReference = Trim(s.TransferReference);
        row.ChequeNo = Trim(s.ChequeNo);
        row.ChequeDate = s.ChequeDate;
        row.SellerPaymentReference = Trim(s.SellerPaymentReference);
        row.Notes = s.Notes;
        await db.SaveChangesAsync(ct);
        return row.SettlementId;
    }

    /// <summary>confirm_settlement: checks the settlement, marks it SETTLED (accounting READY) and activates the portfolio.</summary>
    public async Task ConfirmSettlementAsync(long settlementId, string user, CancellationToken ct)
    {
        var r = await db.Set<MtgPurchasedPortfolioSettlement>().FirstOrDefaultAsync(x => x.SettlementId == settlementId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (r.SettlementStatusCode == "SETTLED")
            return;
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == r.PortfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (p.PortfolioStatusCode != "APPROVED")
            throw new ApexProcessException("Portfolio must be APPROVED before settlement / يجب أن تكون المحفظة معتمدة قبل التسوية");
        if (r.ContractExecutionDate is null)
            throw new ApexProcessException("Contract execution date is required / تاريخ تنفيذ العقد مطلوب");
        if (r.CutOffDate is null)
            throw new ApexProcessException("Cut-off date is required / تاريخ القطع مطلوب");
        if (r.AcquisitionDate is null)
            throw new ApexProcessException("Acquisition date is required / تاريخ الاستحواذ مطلوب");
        if (r.SettlementDate is null)
            throw new ApexProcessException("Settlement date is required / تاريخ التسوية مطلوب");
        if (r.AccountingDate is null)
            throw new ApexProcessException("Accounting date is required / التاريخ المحاسبي مطلوب");
        if (r.PaymentMethodCode is null)
            throw new ApexProcessException("Payment method is required / طريقة الدفع مطلوبة");
        if (r.PaymentAmount is null or <= 0)
            throw new ApexProcessException("Payment amount must be greater than zero / يجب أن يكون مبلغ الدفع أكبر من صفر");
        if (Math.Abs(r.PaymentAmount.Value - r.ApprovedPurchaseValue) > 0.01m)
            throw new ApexProcessException("Payment amount must equal the approved purchase value / يجب أن يساوي مبلغ الدفع قيمة الشراء المعتمدة");
        if (string.IsNullOrWhiteSpace(r.BankGlAccountCode))
            throw new ApexProcessException("Bank GL account code is required / رمز حساب البنك في دفتر الأستاذ مطلوب");
        if (r.PaymentMethodCode == "BANK_TRANSFER" && string.IsNullOrWhiteSpace(r.TransferReference))
            throw new ApexProcessException("Transfer reference is required for bank transfer / مرجع التحويل مطلوب للتحويل البنكي");
        if (r.PaymentMethodCode == "CHEQUE" && (string.IsNullOrWhiteSpace(r.ChequeNo) || r.ChequeDate is null))
            throw new ApexProcessException("Cheque number and cheque date are required / رقم الشيك وتاريخه مطلوبان");

        r.SettlementStatusCode = "SETTLED";
        r.AccountingStatusCode = "READY";
        r.SettledBy = user;
        r.SettledOnAt = clock.UtcNow;

        // FNV_PURSHASED_PORTFOLIO_PKG.change_status
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = p.PortfolioStatusCode,
            ToStatus = "ACTIVATED",
            ChangeAt = clock.UtcNow,
            ChangedBy = user,
            Comments = "Acquisition settlement confirmed / تم تأكيد تسوية شراء المحفظة",
        });
        p.PortfolioStatusCode = "ACTIVATED";

        // FNV_AUDIT_PKG.log_action is not ported: FNV_AUDIT_LOG is replaced by the audit columns every Capitra table carries.
        await db.SaveChangesAsync(ct);
    }

    private static string? Trim(string? p) => string.IsNullOrWhiteSpace(p) ? null : p.Trim();

    private static string? Up(string? p) => Trim(p)?.ToUpperInvariant();
}
