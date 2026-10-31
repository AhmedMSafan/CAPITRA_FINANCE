using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fin;

/// <summary>
/// FIN_BUSINESS_LINES: copy of FINOVA FIN_BUSINESS_LINES (APEX), all columns kept.
/// </summary>
public class FinBusinessLine : AuditableEntity
{
    public long BusinessLineId { get; set; }

    public string BusinessLineCode { get; set; } = string.Empty;

    public string BusinessLineName { get; set; } = string.Empty;

    public string? BusinessLineNameAr { get; set; }

    public string ModuleCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 10;
}
