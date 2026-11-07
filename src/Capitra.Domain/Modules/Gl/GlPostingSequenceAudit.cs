using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_SEQUENCE_AUDITS: copy of FINOVA GL_POSTING_SEQUENCE_AUDIT (APEX), all columns kept.
/// </summary>
public class GlPostingSequenceAudit : AuditableEntity
{
    public long PostingSequenceAuditId { get; set; }

    /// <summary>One of: QUEUE, POSTING_RUN, ROLLBACK_RUN, REBUILD_RUN, RECONCILIATION.</summary>
    public string SequenceScope { get; set; } = string.Empty;

    public long? LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public string? SourceCode { get; set; }

    public int LastSequenceNo { get; set; }

    public string GeneratedReference { get; set; } = string.Empty;

    public string? RelatedTableName { get; set; }

    public long? RelatedRecordId { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }
}
