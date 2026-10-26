using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_LEDGERS: a set of books of a legal entity, with its currency, calendar and account structure.
/// The control accounts hold natural account codes of the ledger's account structure.
/// </summary>
public class GlLedger : AuditableEntity
{
    public long LedgerId { get; set; }

    public string LegalEntityCode { get; set; } = string.Empty;

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerNameEn { get; set; } = string.Empty;

    public string? LedgerNameAr { get; set; }

    /// <summary>PRIMARY or SECONDARY.</summary>
    public string LedgerType { get; set; } = GlCodes.LedgerPrimary;

    public string LedgerCurrencyCode { get; set; } = string.Empty;

    public long CalendarId { get; set; }

    public long? CoaId { get; set; }

    /// <summary>One primary ledger per legal entity.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>ACTIVE or INACTIVE.</summary>
    public string LedgerStatus { get; set; } = GlCodes.LedgerActive;

    public string? RetainedEarningsAcctCode { get; set; }

    public string? SuspenseAcctCode { get; set; }

    public string? RoundingDiffAcctCode { get; set; }

    public string? FxGainAcctCode { get; set; }

    public string? FxLossAcctCode { get; set; }

    public string? OpeningBalanceAcctCode { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Notes { get; set; }
}
