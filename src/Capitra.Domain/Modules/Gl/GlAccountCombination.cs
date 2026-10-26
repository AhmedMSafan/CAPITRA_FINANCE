using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_COMBINATIONS: one set of segment values, e.g. 01-HQ-110101-RET. Created the first time a journal line uses it;
/// its values are in <see cref="GlAccountCombinationSegment"/>.
/// </summary>
public class GlAccountCombination : AuditableEntity
{
    public long AccountCombinationId { get; set; }

    public long CoaId { get; set; }

    /// <summary>The values in segment order, joined by the structure's delimiter.</summary>
    public string ConcatenatedAccount { get; set; } = string.Empty;

    /// <summary>The natural account of the account segment.</summary>
    public long AccountId { get; set; }

    public bool IsEnabled { get; set; } = true;
}
