using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_PERIOD_LOCKS: copy of FINOVA GL_CLOSE_PERIOD_LOCKS (APEX), all columns kept.
/// </summary>
public class GlClosePeriodLock : AuditableEntity
{
    public long PeriodLockId { get; set; }

    public long CloseRunId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    /// <summary>One of: SOURCE_POSTING, GL_POSTING, ADJUSTMENT, FULL.</summary>
    public string LockLevel { get; set; } = "FULL";

    /// <summary>One of: LOCKED, UNLOCKED, EXPIRED.</summary>
    public string LockStatus { get; set; } = "LOCKED";

    public string? LockReason { get; set; }

    public string LockedBy { get; set; } = string.Empty;

    public DateTime LockedAt { get; set; }

    public string? UnlockedBy { get; set; }

    public DateTime? UnlockedAt { get; set; }

    public string? UnlockReason { get; set; }
}
