using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_COMBINATIONS: copy of FINOVA GL_ACCOUNT_COMBINATIONS (APEX), all columns kept.
/// </summary>
public class GlAccountCombination : AuditableEntity
{
    public long AccountCombinationId { get; set; }

    public long CoaId { get; set; }

    public string ConcatenatedAccount { get; set; } = string.Empty;

    public string? DisplayAccount { get; set; }

    public long? LegalEntityId { get; set; }

    public long? BranchId { get; set; }

    public long? CostCenterId { get; set; }

    public string? BusinessLineCode { get; set; }

    public long AccountId { get; set; }

    public long? CompanySegmentValueId { get; set; }

    public long? BranchSegmentValueId { get; set; }

    public long? CostCenterSegmentValueId { get; set; }

    public long? BusinessLineSegmentValueId { get; set; }

    public long? AccountSegmentValueId { get; set; }

    public bool IsSummary { get; set; }

    public bool IsPostingAllowed { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    /// <summary>One of: VALID, PENDING_APPROVAL, INCOMPLETE, BLOCKED, EXPIRED, ERROR.</summary>
    public string ValidationStatus { get; set; } = "VALID";

    public string? ValidationMessage { get; set; }

    public bool IsManualCreated { get; set; }

    public bool IsDynamicCreated { get; set; }

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public DateTime? LastValidatedAt { get; set; }
}
