using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_ACCOUNT_LOOKUPS: copy of FINOVA GL_ACCOUNTING_ACCOUNT_LOOKUPS (APEX), all columns kept.
/// </summary>
public class GlAccountingAccountLookup : AuditableEntity
{
    public long AccountLookupId { get; set; }

    public string LookupCode { get; set; } = string.Empty;

    public string LookupNameEn { get; set; } = string.Empty;

    public string? LookupNameAr { get; set; }

    public long? SourceId { get; set; }

    public long? EventTypeId { get; set; }

    public long? LedgerId { get; set; }

    /// <summary>One of: COMBINATION, ACCOUNT, SEGMENTS.</summary>
    public string LookupResultType { get; set; } = "COMBINATION";

    public long? DefaultAccountCombinationId { get; set; }

    public long? DefaultAccountId { get; set; }

    /// <summary>One of: ERROR, SUSPENSE, USE_DEFAULT, SKIP_LINE, WARN_ONLY.</summary>
    public string NoMatchBehavior { get; set; } = "ERROR";

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
