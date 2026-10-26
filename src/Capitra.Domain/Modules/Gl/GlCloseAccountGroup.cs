using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCOUNT_GROUPS: copy of FINOVA GL_CLOSE_ACCOUNT_GROUPS (APEX), all columns kept.
/// </summary>
public class GlCloseAccountGroup : AuditableEntity
{
    public long AccountGroupId { get; set; }

    public string AccountGroupCode { get; set; } = string.Empty;

    public string AccountGroupNameEn { get; set; } = string.Empty;

    public string? AccountGroupNameAr { get; set; }

    /// <summary>One of: ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE, SUSPENSE, OTHER.</summary>
    public string AccountGroupType { get; set; } = string.Empty;

    /// <summary>One of: DR, CR.</summary>
    public string? NormalBalance { get; set; }

    /// <summary>One of: CARRY_FORWARD, CLOSE_TO_RETAINED_EARNINGS, ZERO_BALANCE, VALIDATE_ZERO_ONLY.</summary>
    public string CloseBehavior { get; set; } = string.Empty;

    public int DisplaySeq { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
