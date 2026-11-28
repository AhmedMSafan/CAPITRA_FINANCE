using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_BANKS: copy of FINOVA TRS_BANKS (APEX), all columns kept.
/// </summary>
public class TrsBank : AuditableEntity
{
    public long BankId { get; set; }

    public long CompanyId { get; set; } = 1;

    public string BankCode { get; set; } = string.Empty;

    public string BankNameEn { get; set; } = string.Empty;

    public string BankNameAr { get; set; } = string.Empty;

    public string? CbeCode { get; set; }

    public string? SwiftBic { get; set; }

    public string? FraRegistrationNo { get; set; }

    public string? RelationshipManagerName { get; set; }

    public string? RelationshipManagerEmail { get; set; }

    public string? RelationshipManagerPhone { get; set; }

    /// <summary>One of: CURRENT, EXPIRING, EXPIRED, PENDING.</summary>
    public string KycStatus { get; set; } = "CURRENT";

    public DateOnly? KycReviewDate { get; set; }

    public DateOnly? KycExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;
}
