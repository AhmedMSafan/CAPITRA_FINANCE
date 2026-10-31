using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_ACCOUNT_PAYMENTS: copy of FINOVA FNV_ACCOUNT_PAYMENT (APEX), all columns kept.
/// </summary>
public class MtgAccountPayment : AuditableEntity
{
    public long PaymentId { get; set; }

    public long PortfolioAccountId { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal DueAmount { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public decimal PaidAmount { get; set; }

    public string PaymentStatusCode { get; set; } = string.Empty;

    public int DaysLate { get; set; }

    public string? ChequeNo { get; set; }

    public string? BankName { get; set; }
}
