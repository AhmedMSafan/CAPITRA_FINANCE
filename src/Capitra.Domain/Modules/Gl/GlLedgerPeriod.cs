using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_LEDGER_PERIODS: copy of FINOVA GL_LEDGER_PERIODS (APEX), all columns kept.
/// </summary>
public class GlLedgerPeriod : AuditableEntity
{
    public long LedgerPeriodId { get; set; }

    public long LedgerId { get; set; }

    public long PeriodId { get; set; }

    /// <summary>One of: NEVER_OPENED, FUTURE_ENTRY, OPEN, CLOSED, PERMANENTLY_CLOSED.</summary>
    public string PeriodStatus { get; set; } = "NEVER_OPENED";

    /// <summary>One of: NONE, FUTURE_ONLY, NORMAL, ADJUSTMENT_ONLY.</summary>
    public string PostingMode { get; set; } = "NONE";

    public bool IsManualJournalAllowed { get; set; }

    public bool IsSubledgerAllowed { get; set; }

    public bool IsReversalAllowed { get; set; }

    public bool IsAdjustmentAllowed { get; set; }

    public string? OpenedBy { get; set; }

    public DateTime? OpenedAt { get; set; }

    public string? ClosedBy { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? PermanentlyClosedBy { get; set; }

    public DateTime? PermanentlyClosedAt { get; set; }

    public string? CloseNotes { get; set; }
}
