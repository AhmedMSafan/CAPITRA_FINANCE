using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>TRS_SUBLIMIT_SCOPES: one business line a sublimit may fund (FACTORING, REVERSE_FACTORING, MORTGAGE, ...).</summary>
public class TrsSublimitScope : AuditableEntity
{
    public long ScopeId { get; set; }

    public long SublimitId { get; set; }

    /// <summary>One of: BUSINESS_LINE.</summary>
    public string ScopeTypeCode { get; set; } = "BUSINESS_LINE";

    public string ScopeValue { get; set; } = string.Empty;
}
