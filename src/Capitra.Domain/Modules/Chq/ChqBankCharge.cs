using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_BANK_CHARGES: copy of FINOVA FND_CHEQUE_BANK_CHARGE (APEX), all columns kept.
/// </summary>
public class ChqBankCharge : AuditableEntity
{
    public long ChargeId { get; set; }

    public string ChargeRef { get; set; } = string.Empty;

    public long ChequeId { get; set; }

    public long LegalEntityId { get; set; }

    /// <summary>One of: RETURN_FEE, PROTEST_FEE, NSF_FEE, REPRESENTATION_FEE, OTHER.</summary>
    public string ChargeType { get; set; } = string.Empty;

    public string? ChargeDescriptionEn { get; set; }

    public string? ChargeDescriptionAr { get; set; }

    public decimal ChargeAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? ChargedByBank { get; set; }

    public DateTime ChargeAt { get; set; }

    public bool IsRecoveredFromCustomer { get; set; }

    public DateTime? RecoveryAt { get; set; }

    public decimal? RecoveryAmount { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlDebitAccount { get; set; }

    public string? GlCreditAccount { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime? GlPostAt { get; set; }

    public string? Notes { get; set; }
}
