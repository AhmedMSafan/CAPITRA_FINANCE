using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fin;

/// <summary>
/// FIN_COST_CENTERS: copy of FINOVA FIN_COST_CENTERS (APEX), all columns kept.
/// </summary>
public class FinCostCenter : AuditableEntity
{
    public long CostCenterId { get; set; }

    public long LegalEntityId { get; set; }

    public long? BranchId { get; set; }

    public string CostCenterCode { get; set; } = string.Empty;

    public string CostCenterNameEn { get; set; } = string.Empty;

    public string? CostCenterNameAr { get; set; }

    public long? ParentCostCenterId { get; set; }

    /// <summary>One of: OPERATING, SUPPORT, SALES, COLLECTION, TREASURY, ADMIN, IT, OTHER.</summary>
    public string CostCenterType { get; set; } = "OPERATING";

    public string? ManagerUserName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
