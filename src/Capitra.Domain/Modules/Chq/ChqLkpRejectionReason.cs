using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_LKP_REJECTION_REASONS: copy of FINOVA LKP_CHEQUE_REJECTION_REASON (APEX), all columns kept.
/// </summary>
public class ChqLkpRejectionReason : AuditableEntity
{
    public string ReasonCode { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? NameAr { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
