using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_RULE_LINE_SEGMENTS: copy of FINOVA GL_AUTO_RULE_LINE_SEGMENTS (APEX), all columns kept.
/// </summary>
public class GlAutoRuleLineSegment : AuditableEntity
{
    public long RuleLineSegmentId { get; set; }

    public long RuleLineId { get; set; }

    public long CoaSegmentId { get; set; }

    /// <summary>One of: FIXED_VALUE, SOURCE_VALUE, EVENT_CONTEXT, LEDGER_DEFAULT, LOOKUP, EXPRESSION, DEFAULT_VALUE.</summary>
    public string DerivationMethod { get; set; } = "SOURCE_VALUE";

    public long? FixedSegmentValueId { get; set; }

    public long? SourceValueId { get; set; }

    public string? SourceValueCode { get; set; }

    public long? LookupId { get; set; }

    public string? ExpressionText { get; set; }

    public long? DefaultSegmentValueId { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;
}
