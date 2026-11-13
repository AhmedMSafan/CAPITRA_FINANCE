using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Col;

/// <summary>
/// COL_COLLECTION_BATCHES: one run of Apply Bank Results on the Cheques Collection page (agreed with Ahmed, 1 Nov 2026): the
/// bank's results it applied and the one GL transaction set of its collected cheques.
/// </summary>
public class ColCollectionBatch : AuditableEntity
{
    public long CollectionBatchId { get; set; }

    /// <summary>COL-yyyyMMdd-nnnnnn.</summary>
    public string BatchNo { get; set; } = string.Empty;

    public DateOnly BatchDate { get; set; }

    public int CollectedCount { get; set; }

    public decimal CollectedAmount { get; set; }

    public int BouncedCount { get; set; }

    public decimal BouncedAmount { get; set; }

    public int RejectedCount { get; set; }

    public decimal RejectedAmount { get; set; }

    /// <summary>How many client repayments the collected cheques made.</summary>
    public int RepaymentsCount { get; set; }

    /// <summary>The GL transaction set of the collected cheques (DR each deposit bank / CR customer liability), CAP-COL-...</summary>
    public string? GlTranSetId { get; set; }

    public string? Notes { get; set; }
}
