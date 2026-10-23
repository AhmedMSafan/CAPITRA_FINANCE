using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_ACCOUNT_PAYMENT: one instalment of a portfolio account (APEX FNV_ACCOUNT_PAYMENT, App 105 pages 206/207).</summary>
public class MtgAccountPayment : AuditableEntity
{
    public long AccountPaymentId { get; set; }

    public long PortfolioAccountId { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal DueAmount { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public decimal PaidAmount { get; set; }

    /// <summary>ADM_LKP_PAYMENT_STATUS code.</summary>
    public string PaymentStatusCode { get; set; } = string.Empty;

    public int DaysLate { get; set; }

    public string? ChequeNo { get; set; }

    public string? BankName { get; set; }
}
