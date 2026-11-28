using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_COA_STRUCTURES: copy of FINOVA GL_COA_STRUCTURES (APEX), all columns kept.
/// </summary>
public class GlCoaStructure : AuditableEntity
{
    public long CoaId { get; set; }

    public string CoaCode { get; set; } = string.Empty;

    public string CoaNameEn { get; set; } = string.Empty;

    public string? CoaNameAr { get; set; }

    public string? Description { get; set; }

    public string DelimiterChar { get; set; } = "-";

    public int SegmentCount { get; set; }

    public int? BalancingSegmentNum { get; set; }

    public int AccountSegmentNum { get; set; }

    public int? BranchSegmentNum { get; set; }

    public int? CostCenterSegmentNum { get; set; }

    public int? BusinessLineSegmentNum { get; set; }

    public int? IntercompanySegmentNum { get; set; }

    public bool IsDynamicCombinationsAllowed { get; set; } = true;

    public bool IsCombinationApprovalRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }
}
