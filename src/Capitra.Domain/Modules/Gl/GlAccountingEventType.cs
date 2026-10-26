using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_EVENT_TYPES: copy of FINOVA GL_ACCOUNTING_EVENT_TYPES (APEX), all columns kept.
/// </summary>
public class GlAccountingEventType : AuditableEntity
{
    public long EventTypeId { get; set; }

    public long SourceId { get; set; }

    public string EventTypeCode { get; set; } = string.Empty;

    public string EventTypeNameEn { get; set; } = string.Empty;

    public string? EventTypeNameAr { get; set; }

    public long? DefaultCategoryId { get; set; }

    public bool IsAccountingRequired { get; set; } = true;

    public bool IsManualAdjustAllowed { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
