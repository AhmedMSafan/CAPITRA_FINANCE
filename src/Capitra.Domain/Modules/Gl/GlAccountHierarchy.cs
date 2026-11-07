using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_HIERARCHIES: copy of FINOVA GL_ACCOUNT_HIERARCHIES (APEX), all columns kept.
/// </summary>
public class GlAccountHierarchy : AuditableEntity
{
    public long HierarchyId { get; set; }

    public long CoaId { get; set; }

    public string HierarchyCode { get; set; } = string.Empty;

    public string HierarchyNameEn { get; set; } = string.Empty;

    public string? HierarchyNameAr { get; set; }

    /// <summary>One of: ACCOUNT, TRIAL_BALANCE, BALANCE_SHEET, INCOME_STATEMENT, CASH_FLOW, EQUITY, MANAGEMENT, CONSOLIDATION.</summary>
    public string HierarchyType { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public string? Description { get; set; }
}
