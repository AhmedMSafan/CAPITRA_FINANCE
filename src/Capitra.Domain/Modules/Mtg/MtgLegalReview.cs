using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LEGAL_REVIEWS: copy of FINOVA FNV_LEGAL_REVIEW (APEX), all columns kept.
/// </summary>
public class MtgLegalReview : AuditableEntity
{
    public long LegalReviewId { get; set; }

    public long PortfolioId { get; set; }

    public bool IsOwnershipVerified { get; set; }

    public bool IsBuildingLicenseVerified { get; set; }

    public bool IsRegistrationPossible { get; set; }

    public bool IsMortgagePossible { get; set; }

    public bool IsEncumbranceFree { get; set; }

    public bool IsTitleDeedVerified { get; set; }

    public string? LegalFindings { get; set; }

    public string? LegalRecommendation { get; set; }

    public string? ReviewedBy { get; set; }

    public DateOnly? ReviewDate { get; set; }

    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }
}
