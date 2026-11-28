using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_BALANCE_REBUILD_LINES: copy of FINOVA GL_BALANCE_REBUILD_LINES (APEX), all columns kept.
/// </summary>
public class GlBalanceRebuildLine : AuditableEntity
{
    public long BalanceRebuildLineId { get; set; }

    public long BalanceRebuildRunId { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public long? LedgerPeriodId { get; set; }

    public DateOnly? AccountingDate { get; set; }

    public decimal? OldPeriodDr { get; set; }

    public decimal? OldPeriodCr { get; set; }

    public decimal? NewPeriodDr { get; set; }

    public decimal? NewPeriodCr { get; set; }

    /// <summary>One of: PENDING, APPLIED, ERROR, SKIPPED.</summary>
    public string RebuildStatus { get; set; } = "PENDING";

    public string? ErrorMessage { get; set; }
}
