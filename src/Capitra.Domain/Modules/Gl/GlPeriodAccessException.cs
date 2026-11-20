using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_ACCESS_EXCEPTIONS: copy of FINOVA GL_PERIOD_ACCESS_EXCEPTIONS (APEX), all columns kept.
/// </summary>
public class GlPeriodAccessException : AuditableEntity
{
    public long PeriodExceptionId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string ExceptionCode { get; set; } = string.Empty;

    /// <summary>One of: BACKDATED_POSTING, ADJUSTMENT_POSTING, REOPENED_PERIOD, FUTURE_POSTING, REVERSAL.</summary>
    public string ExceptionType { get; set; } = string.Empty;

    public string? SourceCode { get; set; }

    public string? AllowedUserName { get; set; }

    public string? AllowedRoleCode { get; set; }

    public DateTime ValidFromAt { get; set; }

    public DateTime? ValidToAt { get; set; }

    public decimal? MaxTotalAmount { get; set; }

    public decimal UsedTotalAmount { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>One of: DRAFT, SUBMITTED, APPROVED, REJECTED, EXPIRED, CANCELLED.</summary>
    public string ApprovalStatus { get; set; } = "DRAFT";

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string Reason { get; set; } = string.Empty;
}
