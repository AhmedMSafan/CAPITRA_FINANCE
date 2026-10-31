using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_SOURCE_STATUSES: copy of FINOVA GL_PERIOD_SOURCE_STATUS (APEX), all columns kept.
/// </summary>
public class GlPeriodSourceStatus : AuditableEntity
{
    public long SourceStatusId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public string BusinessLineCode { get; set; } = "ALL";

    /// <summary>One of: OPEN, READY_TO_CLOSE, CLOSED, LOCKED, WAIVED.</summary>
    public string SourceStatus { get; set; } = "OPEN";

    /// <summary>One of: NOT_REQUIRED, PENDING, IN_PROGRESS, TRANSFERRED, ERROR.</summary>
    public string TransferStatus { get; set; } = "NOT_REQUIRED";

    public int UntransferredCount { get; set; }

    public int UnpostedCount { get; set; }

    public int ErrorCount { get; set; }

    public DateTime? LastTransferAt { get; set; }

    public DateTime? LastCheckedAt { get; set; }

    public string? ResponsibleRoleCode { get; set; }

    public string? Notes { get; set; }
}
