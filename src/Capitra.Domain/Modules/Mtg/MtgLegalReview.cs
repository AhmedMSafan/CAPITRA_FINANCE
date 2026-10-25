using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LEGAL_REVIEWS: the legal checklist, findings and sign-off for a portfolio, one per portfolio
/// (APEX FNV_LEGAL_REVIEW, App 105 page 211).
/// </summary>
public class MtgLegalReview : AuditableEntity
{
    public long LegalReviewId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>The developer holds clear title to the land and units.</summary>
    public bool IsOwnershipVerified { get; set; }

    /// <summary>Valid building permits cover the sold units.</summary>
    public bool IsBuildingLicenseVerified { get; set; }

    /// <summary>Units can be registered in the buyers' names.</summary>
    public bool IsRegistrationPossible { get; set; }

    /// <summary>A mortgage can be registered over the units.</summary>
    public bool IsMortgagePossible { get; set; }

    /// <summary>No liens, seizures or third-party claims.</summary>
    public bool IsEncumbranceFree { get; set; }

    /// <summary>The title deed was checked against the registry.</summary>
    public bool IsTitleDeedVerified { get; set; }

    public string? LegalFindings { get; set; }

    public string? LegalRecommendation { get; set; }

    /// <summary>Name of the lawyer who did the review, as typed (not necessarily a system user).</summary>
    public string? ReviewedBy { get; set; }

    public DateOnly? ReviewDate { get; set; }

    /// <summary>Name of the legal manager who approved the review, as typed.</summary>
    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    /// <summary>All six checks are Yes and both dates are filled: what accepting the step requires.</summary>
    public bool IsComplete =>
        IsOwnershipVerified && IsBuildingLicenseVerified && IsRegistrationPossible && IsMortgagePossible
        && IsEncumbranceFree && IsTitleDeedVerified && ReviewDate is not null && ApprovedDate is not null;
}
