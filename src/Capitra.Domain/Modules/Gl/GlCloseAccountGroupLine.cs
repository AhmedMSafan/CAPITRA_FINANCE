using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCOUNT_GROUP_LINES: copy of FINOVA GL_CLOSE_ACCOUNT_GROUP_LINES (APEX), all columns kept.
/// </summary>
public class GlCloseAccountGroupLine : AuditableEntity
{
    public long AccountGroupLineId { get; set; }

    public long AccountGroupId { get; set; }

    public long? AccountId { get; set; }

    public long? AccountCombinationId { get; set; }

    public string? AccountCodeFrom { get; set; }

    public string? AccountCodeTo { get; set; }

    /// <summary>One of: INCLUDE, EXCLUDE.</summary>
    public string IncludeExcludeFlag { get; set; } = "INCLUDE";

    public int PriorityNo { get; set; } = 100;

    public bool IsActive { get; set; } = true;
}
