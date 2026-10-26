using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_LEDGER_PERIODS: the status of one accounting period in one ledger, and what it lets post.</summary>
public class GlLedgerPeriod : AuditableEntity
{
    public long LedgerPeriodId { get; set; }

    public long LedgerId { get; set; }

    public long PeriodId { get; set; }

    /// <summary>NEVER_OPENED, FUTURE_ENTRY, OPEN, CLOSED or PERMANENTLY_CLOSED.</summary>
    public string PeriodStatus { get; set; } = GlCodes.PeriodNeverOpened;

    /// <summary>NONE, NORMAL, ADJUSTMENT_ONLY or FUTURE_ONLY.</summary>
    public string PostingMode { get; set; } = GlCodes.PostingNone;

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
