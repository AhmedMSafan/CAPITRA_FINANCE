using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RETAINED_EARNINGS_TRANSFERS: copy of FINOVA GL_RETAINED_EARNINGS_TRANSFERS (APEX), all columns kept.
/// </summary>
public class GlRetainedEarningsTransfer : AuditableEntity
{
    public long RetainedTransferId { get; set; }

    public long YearEndRunId { get; set; }

    public long RetainedEarningsAccountCombId { get; set; }

    public int NetProfitLoss { get; set; }

    public decimal TransferDrAmount { get; set; }

    public decimal TransferCrAmount { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    /// <summary>One of: DRAFT, GENERATED, POSTED, REVERSED, ERROR, CANCELLED.</summary>
    public string TransferStatus { get; set; } = "DRAFT";
}
