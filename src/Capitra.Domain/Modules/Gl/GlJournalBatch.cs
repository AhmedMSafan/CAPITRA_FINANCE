using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_BATCHES: a group of journals entered or received together. A manual journal has a batch of its own;
/// a sub-ledger batch carries the TRAN_SET_ID of the module in <see cref="SourceBatchRef"/>.
/// </summary>
public class GlJournalBatch : AuditableEntity
{
    public long BatchId { get; set; }

    public long LedgerId { get; set; }

    public string BatchNo { get; set; } = string.Empty;

    public string BatchName { get; set; } = string.Empty;

    public long SourceId { get; set; }

    public long CategoryId { get; set; }

    public DateOnly BatchDate { get; set; }

    /// <summary>TRAN_SET_ID of the sub-ledger transaction set.</summary>
    public string? SourceBatchRef { get; set; }

    public string? Description { get; set; }
}
