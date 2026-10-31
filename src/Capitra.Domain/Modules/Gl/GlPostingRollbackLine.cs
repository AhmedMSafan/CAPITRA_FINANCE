using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_ROLLBACK_LINES: copy of FINOVA GL_POSTING_ROLLBACK_LINES (APEX), all columns kept.
/// </summary>
public class GlPostingRollbackLine : AuditableEntity
{
    public long RollbackLineId { get; set; }

    public long RollbackRunId { get; set; }

    public long BalanceImpactId { get; set; }

    public long JournalHeaderId { get; set; }

    public long JournalLineId { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal RollbackAccountedDr { get; set; }

    public decimal RollbackAccountedCr { get; set; }

    /// <summary>One of: PENDING, APPLIED, ERROR, SKIPPED.</summary>
    public string RollbackStatus { get; set; } = "PENDING";

    public string? ErrorMessage { get; set; }
}
