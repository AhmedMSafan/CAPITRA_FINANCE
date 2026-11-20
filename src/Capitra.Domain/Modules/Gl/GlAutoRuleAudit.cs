using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_RULE_AUDITS: copy of FINOVA GL_AUTO_RULE_AUDIT (APEX), all columns kept.
/// </summary>
public class GlAutoRuleAudit : AuditableEntity
{
    public long RuleAuditId { get; set; }

    /// <summary>One of: RULE_SET, RULE_LINE, SEGMENT_LINE, LOOKUP, LOOKUP_LINE, CONDITION, VALUE_DEF.</summary>
    public string ObjectType { get; set; } = string.Empty;

    public long ObjectId { get; set; }

    /// <summary>One of: INSERT, UPDATE, DELETE, STATUS_CHANGE, APPROVAL.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? ColumnName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? ChangeReason { get; set; }
}
