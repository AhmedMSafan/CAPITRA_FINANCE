using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_LEDGERS: copy of FINOVA GL_LEDGERS (APEX), all columns kept.
/// </summary>
public class GlLedger : AuditableEntity
{
    public long LedgerId { get; set; }

    public long LegalEntityId { get; set; }

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerNameEn { get; set; } = string.Empty;

    public string? LedgerNameAr { get; set; }

    /// <summary>One of: PRIMARY, SECONDARY, REPORTING, CONSOLIDATION.</summary>
    public string LedgerType { get; set; } = "PRIMARY";

    public string LedgerCurrencyCode { get; set; } = string.Empty;

    public long CalendarId { get; set; }

    public bool IsPrimaryLedger { get; set; } = true;

    /// <summary>One of: DRAFT, ACTIVE, SUSPENDED, CLOSED.</summary>
    public string LedgerStatus { get; set; } = "DRAFT";

    public string? RetainedEarningsAcctCode { get; set; }

    public string? SuspenseAcctCode { get; set; }

    public string? RoundingDiffAcctCode { get; set; }

    public string? FxGainAcctCode { get; set; }

    public string? FxLossAcctCode { get; set; }

    public string? OpeningBalanceAcctCode { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Notes { get; set; }

    public long? CoaId { get; set; }
}
