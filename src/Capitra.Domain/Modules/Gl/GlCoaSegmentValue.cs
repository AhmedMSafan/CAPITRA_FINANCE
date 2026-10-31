using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_COA_SEGMENT_VALUES: copy of FINOVA GL_COA_SEGMENT_VALUES (APEX), all columns kept.
/// </summary>
public class GlCoaSegmentValue : AuditableEntity
{
    public long SegmentValueId { get; set; }

    public long CoaSegmentId { get; set; }

    public string ValueCode { get; set; } = string.Empty;

    public string ValueNameEn { get; set; } = string.Empty;

    public string? ValueNameAr { get; set; }

    public string? Description { get; set; }

    public long? ParentValueId { get; set; }

    public bool IsSummary { get; set; }

    public bool IsPostingAllowed { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public long SortOrder { get; set; } = 10;

    public long? ReferenceLegalEntityId { get; set; }

    public long? ReferenceBranchId { get; set; }

    public long? ReferenceCostCenterId { get; set; }

    public string? ReferenceBusinessLineCode { get; set; }
}
