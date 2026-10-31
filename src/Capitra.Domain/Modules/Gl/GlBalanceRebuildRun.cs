using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_BALANCE_REBUILD_RUNS: copy of FINOVA GL_BALANCE_REBUILD_RUNS (APEX), all columns kept.
/// </summary>
public class GlBalanceRebuildRun : AuditableEntity
{
    public long BalanceRebuildRunId { get; set; }

    public string RebuildRunNo { get; set; } = string.Empty;

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    /// <summary>One of: LEDGER, PERIOD, ACCOUNT, ACCOUNT_DATE_RANGE, DAILY_BALANCE.</summary>
    public string RebuildScope { get; set; } = "PERIOD";

    public long? AccountCombinationId { get; set; }

    public string? CurrencyCode { get; set; }

    public DateOnly? FromAccountingDate { get; set; }

    public DateOnly? ToAccountingDate { get; set; }

    public string RebuildReason { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, APPROVED, IN_PROGRESS, COMPLETED, ERROR, CANCELLED.</summary>
    public string RebuildStatus { get; set; } = "DRAFT";

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
