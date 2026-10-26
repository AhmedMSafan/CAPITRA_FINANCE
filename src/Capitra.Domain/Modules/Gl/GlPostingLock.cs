using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_LOCKS: copy of FINOVA GL_POSTING_LOCKS (APEX), all columns kept.
/// </summary>
public class GlPostingLock : AuditableEntity
{
    public long PostingLockId { get; set; }

    /// <summary>One of: JOURNAL, BATCH, LEDGER_PERIOD, ACCOUNT_COMBINATION, SOURCE_TRANSACTION, BALANCE_REBUILD.</summary>
    public string LockType { get; set; } = string.Empty;

    public string LockKey { get; set; } = string.Empty;

    public long? LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public long? QueueId { get; set; }

    public long? PostingRunId { get; set; }

    /// <summary>One of: ACTIVE, RELEASED, EXPIRED, FORCE_RELEASED.</summary>
    public string LockStatus { get; set; } = "ACTIVE";

    public string LockedBy { get; set; } = string.Empty;

    public DateTime LockedAt { get; set; }

    public string? ReleasedBy { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public string? ReleaseReason { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
