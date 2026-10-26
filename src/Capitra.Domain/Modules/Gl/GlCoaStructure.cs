using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_COA_STRUCTURES: an account structure, the layout of every account combination (APEX App 120 page 40).
/// Each role (account, balancing, branch ...) holds the number of the segment that plays it.
/// </summary>
public class GlCoaStructure : AuditableEntity
{
    public long CoaId { get; set; }

    public string CoaCode { get; set; } = string.Empty;

    public string CoaNameEn { get; set; } = string.Empty;

    public string? CoaNameAr { get; set; }

    public string? Description { get; set; }

    /// <summary>Put between segment values in a combination, e.g. 01-HQ-110101.</summary>
    public string DelimiterChar { get; set; } = "-";

    /// <summary>1 to 10.</summary>
    public int SegmentCount { get; set; }

    public int AccountSegmentNum { get; set; }

    public int? BalancingSegmentNum { get; set; }

    public int? BranchSegmentNum { get; set; }

    public int? CostCenterSegmentNum { get; set; }

    public int? BusinessLineSegmentNum { get; set; }

    public int? IntercompanySegmentNum { get; set; }

    public bool IsDynamicCombinationAllowed { get; set; }

    public bool IsCombinationApprovalRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }
}
