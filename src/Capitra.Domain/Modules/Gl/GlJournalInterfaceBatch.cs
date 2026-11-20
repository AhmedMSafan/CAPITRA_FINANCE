using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_INTERFACE_BATCHES: copy of FINOVA GL_JOURNAL_INTERFACE_BATCHES (APEX), all columns kept.
/// </summary>
public class GlJournalInterfaceBatch : AuditableEntity
{
    public long InterfaceBatchId { get; set; }

    public long LedgerId { get; set; }

    public long SourceId { get; set; }

    public string InterfaceBatchNo { get; set; } = string.Empty;

    public string? SourceBatchRef { get; set; }

    /// <summary>One of: NEW, VALIDATED, ERROR, PROCESSED, CANCELLED.</summary>
    public string BatchStatus { get; set; } = "NEW";

    public int LineCount { get; set; }

    public int ErrorCount { get; set; }

    public long? GeneratedBatchId { get; set; }

    public string? ImportedBy { get; set; }

    public DateTime? ImportedAt { get; set; }

    public string? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public string? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
