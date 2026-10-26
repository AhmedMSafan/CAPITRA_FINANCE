using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_COA_SEGMENTS: one segment of an account structure: company, branch, account ... (APEX App 120 page 40).</summary>
public class GlCoaSegment : AuditableEntity
{
    public long CoaSegmentId { get; set; }

    public long CoaId { get; set; }

    /// <summary>Position in the combination, from 1. Cannot change once saved.</summary>
    public int SegmentNum { get; set; }

    public string SegmentCode { get; set; } = string.Empty;

    public string SegmentNameEn { get; set; } = string.Empty;

    public string? SegmentNameAr { get; set; }

    /// <summary>One of <see cref="GlCodes.SegmentTypes"/>.</summary>
    public string SegmentType { get; set; } = string.Empty;

    public int MinLength { get; set; } = 1;

    /// <summary>Up to 30.</summary>
    public int MaxLength { get; set; } = 30;

    /// <summary>One of <see cref="GlCodes.ValueFormats"/>.</summary>
    public string ValueFormat { get; set; } = GlCodes.FormatAlphanumeric;

    public string? PadChar { get; set; }

    public bool IsUppercase { get; set; } = true;

    public bool IsRequired { get; set; } = true;

    public bool IsSummaryValueAllowed { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int DisplaySeq { get; set; } = 10;

    public string? Description { get; set; }

    /// <summary>Value filled in on every new journal line. The account segment never has one.</summary>
    public string? DefaultValueCode { get; set; }

    /// <summary>SUGGEST (the user may change it) or FIXED (always used, field hidden in journal entry).</summary>
    public string DefaultMode { get; set; } = GlCodes.DefaultModeSuggest;
}
