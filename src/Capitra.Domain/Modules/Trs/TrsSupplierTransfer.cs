using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_SUPPLIER_TRANSFERS (agreed with Ahmed, 7 Nov 2026): in reverse factoring, Execute posts the bank drawdown into our account and
/// saves one transfer per supplier and paying account, to the supplier's IBAN, as SENT. When our bank confirms it, Confirm posts
/// DR reverse factoring receivable (the buyer) / CR our bank account under its own GL transaction set, and the transfer is CONFIRMED.
/// </summary>
public class TrsSupplierTransfer : AuditableEntity
{
    public const string Sent = "SENT";
    public const string Confirmed = "CONFIRMED";

    public long TransferId { get; set; }

    public long DisbursementId { get; set; }

    /// <summary>The Execute line it is paid from (our account at that bank, or the operating account for own funds).</summary>
    public long FundingId { get; set; }

    public long? SupplierClientId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    /// <summary>The supplier's account (CRM_CLIENT_BANK_ACCOUNTS) and its details as they were when the transfer was sent.</summary>
    public long? SupplierBankAccountId { get; set; }

    public string? SupplierBankName { get; set; }

    public string? SupplierIban { get; set; }

    public string? SupplierAccountNo { get; set; }

    public long PaidFromAccountId { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string StatusCode { get; set; } = Sent;

    public string SentBy { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    /// <summary>Our bank's reference of the confirmed transfer.</summary>
    public string? BankReference { get; set; }

    public string? ConfirmedBy { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public string? GlTranSetId { get; set; }
}
