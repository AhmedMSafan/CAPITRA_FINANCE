using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_SOURCE_VALUE_DEFINITIONS: copy of FINOVA GL_SOURCE_VALUE_DEFINITIONS (APEX), all columns kept.
/// </summary>
public class GlSourceValueDefinition : AuditableEntity
{
    public long SourceValueId { get; set; }

    public long SourceId { get; set; }

    public long? EventTypeId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string ValueCode { get; set; } = string.Empty;

    public string ValueNameEn { get; set; } = string.Empty;

    public string? ValueNameAr { get; set; }

    /// <summary>One of: AMOUNT, NUMBER, TEXT, DATE, CURRENCY, BOOLEAN, ACCOUNT, SEGMENT, PERCENT, JSON.</summary>
    public string ValueKind { get; set; } = string.Empty;

    public string? ValueRole { get; set; }

    public string? DefaultCurrencyCode { get; set; }

    public bool IsRequired { get; set; }

    public bool IsRepeatable { get; set; }

    public bool IsRuleUsageAllowed { get; set; } = true;

    public string? SourceTableName { get; set; }

    public string? SourceColumnName { get; set; }

    public string? SourcePath { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
