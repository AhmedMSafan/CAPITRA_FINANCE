using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LEGAL_REVIEWS: copy of FINOVA MFF_LEGAL_REVIEWS (APEX), all columns kept.
/// </summary>
public class MffLegalReview : AuditableEntity
{
    public long LegalReviewId { get; set; }

    public long ApplicationId { get; set; }

    public long? PropertyId { get; set; }

    public string? ReviewerUser { get; set; }

    public DateOnly? ReviewDate { get; set; }

    public string ResultCode { get; set; } = string.Empty;

    public bool IsRegistrationFeasible { get; set; }

    public bool IsMortgageFeasible { get; set; }

    public string? Issues { get; set; }

    public string? Notes { get; set; }
}
