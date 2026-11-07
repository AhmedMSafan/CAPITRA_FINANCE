using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_LKP_STATUS_CODES: copy of FINOVA LKP_STATUS_CODES (APEX), all columns kept.
/// </summary>
public class CrmLkpStatusCode : AuditableEntity
{
    public long StatusId { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string StatusNameAr { get; set; } = string.Empty;

    public string StatusGroup { get; set; } = string.Empty;

    public string? StatusGroupAr { get; set; }

    public string ModuleCode { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string? ColorCode { get; set; }

    public string? IconClass { get; set; }

    public bool IsInitial { get; set; }

    public bool IsFinal { get; set; }

    public bool IsNegative { get; set; }

    public bool IsAllowsEdit { get; set; } = true;

    public bool IsAllowsDelete { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
