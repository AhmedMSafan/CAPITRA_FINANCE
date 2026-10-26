using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_COA_SEGMENTS: copy of FINOVA GL_COA_SEGMENTS (APEX), all columns kept.
/// </summary>
public class GlCoaSegment : AuditableEntity
{
    public long CoaSegmentId { get; set; }

    public long CoaId { get; set; }

    public int SegmentNum { get; set; }

    public string SegmentCode { get; set; } = string.Empty;

    public string SegmentNameEn { get; set; } = string.Empty;

    public string? SegmentNameAr { get; set; }

    /// <summary>One of: COMPANY, BRANCH, COST_CENTER, ACCOUNT, BUSINESS_LINE, INTERCOMPANY, PROJECT, CUSTOMER, CONTRACT, FUTURE, CUSTOM.</summary>
    public string SegmentType { get; set; } = string.Empty;

    public int MinLength { get; set; } = 1;

    public int MaxLength { get; set; } = 30;

    /// <summary>One of: ALPHANUMERIC, NUMERIC, CHARACTER.</summary>
    public string ValueFormat { get; set; } = "ALPHANUMERIC";

    public string? PadChar { get; set; }

    public bool IsUppercase { get; set; } = true;

    public bool IsRequired { get; set; } = true;

    public bool IsSummaryValuesAllowed { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    public int DisplaySeq { get; set; } = 10;

    public string? Description { get; set; }

    public string? DefaultValueCode { get; set; }

    /// <summary>One of: SUGGEST, FIXED.</summary>
    public string DefaultMode { get; set; } = "SUGGEST";
}
