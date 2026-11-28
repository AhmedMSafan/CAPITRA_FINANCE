using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_DISBURSEMENT_CHEQUES: the cheques of a disbursement request while it is a draft (agreed with Ahmed, 1 Nov 2026). Upload and
/// Save Draft keep them here only; Complete of the Invoice Upload step loads them into the cheques wallet (CHQ_CHEQUES) and posts
/// their memo GL, and the row keeps the wallet cheque it became.
/// </summary>
public class FctDisbursementCheque : AuditableEntity
{
    public long DisbursementChequeId { get; set; }

    public long DisbursementId { get; set; }

    public long? BatchId { get; set; }

    public string ChequeNo { get; set; } = string.Empty;

    public DateOnly ChequeDate { get; set; }

    public DateOnly DueDate { get; set; }

    public string IssuerBankName { get; set; } = string.Empty;

    public string? IssuerBankCode { get; set; }

    public string? IssuerBankBranch { get; set; }

    public string DrawerNameEn { get; set; } = string.Empty;

    public string? DrawerNameAr { get; set; }

    public string? DrawerNationalId { get; set; }

    public string? DrawerAccountNo { get; set; }

    public decimal ChequeAmount { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string? CustomerId { get; set; }

    public string? CustomerNameEn { get; set; }

    public string? CustomerNameAr { get; set; }

    public long? DepositBankAccountId { get; set; }

    public DateOnly ReceivedDate { get; set; }

    public DateOnly? DepositDate { get; set; }

    /// <summary>DRAFT until Complete, then IN_WALLET; CANCELLED is left out.</summary>
    public string StatusCode { get; set; } = "DRAFT";

    public string? Notes { get; set; }

    /// <summary>The CHQ_CHEQUES cheque Complete loaded it as.</summary>
    public long? WalletChequeId { get; set; }
}
