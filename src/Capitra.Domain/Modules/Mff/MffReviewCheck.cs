using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_REVIEW_CHECKS: copy of FINOVA MFF_REVIEW_CHECKS (APEX), all columns kept.
/// </summary>
public class MffReviewCheck : AuditableEntity
{
    public long CheckId { get; set; }

    public long ApplicationId { get; set; }

    public string CheckType { get; set; } = string.Empty;

    public string CheckCode { get; set; } = string.Empty;

    public string ResultCode { get; set; } = string.Empty;

    public string? CheckedBy { get; set; }

    public DateTime CheckedAt { get; set; }

    public string? Notes { get; set; }
}
