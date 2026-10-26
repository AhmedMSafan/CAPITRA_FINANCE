using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_LEDGER_OPTIONS: copy of FINOVA GL_LEDGER_OPTIONS (APEX), all columns kept.
/// </summary>
public class GlLedgerOption : AuditableEntity
{
    public long LedgerOptionId { get; set; }

    public long LedgerId { get; set; }

    public string AllowManualJournals { get; set; } = "Y";

    public string RequireManualJournalAppr { get; set; } = "Y";

    public string AllowSubledgerJournals { get; set; } = "Y";

    public bool IsAutoPostSubledger { get; set; }

    public string AllowFuturePeriodEntry { get; set; } = "N";

    public string AllowBackdatedEntry { get; set; } = "N";

    public string AllowReversalJournals { get; set; } = "Y";

    public bool IsAttachmentRequired { get; set; }

    public string RequireBalancedJournal { get; set; } = "Y";

    public string AllowStatisticalJournals { get; set; } = "N";

    public int? MaxBackdateDays { get; set; }

    /// <summary>One of: NEXT_PERIOD, SAME_PERIOD, SPECIFIC_DATE.</summary>
    public string DefaultReversalMethod { get; set; } = "NEXT_PERIOD";

    /// <summary>One of: SAVE_ONLY, APPROVAL_THEN_POST, AUTO_POST_SUBLEDGER.</summary>
    public string PostingMode { get; set; } = "APPROVAL_THEN_POST";

    /// <summary>One of: STRICT, WARNING, MANUAL_OVERRIDE.</summary>
    public string PeriodClosePolicy { get; set; } = "STRICT";
}
