using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_COA_SEGMENT_VALUES: a value a segment accepts, in a parent/child hierarchy (APEX App 120 page 40).</summary>
public class GlCoaSegmentValue : AuditableEntity
{
    public long SegmentValueId { get; set; }

    public long CoaSegmentId { get; set; }

    public string ValueCode { get; set; } = string.Empty;

    public string ValueNameEn { get; set; } = string.Empty;

    public string? ValueNameAr { get; set; }

    public string? Description { get; set; }

    public long? ParentValueId { get; set; }

    /// <summary>A summary (parent) value never allows posting.</summary>
    public bool IsSummary { get; set; }

    public bool IsPostingAllowed { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public int SortOrder { get; set; } = 10;

    /// <summary>ADM_LKP_LEGAL_ENTITIES code.</summary>
    public string? ReferenceLegalEntityCode { get; set; }

    /// <summary>ADM_LKP_BUSINESS_LINES code.</summary>
    public string? ReferenceBusinessLineCode { get; set; }
}
