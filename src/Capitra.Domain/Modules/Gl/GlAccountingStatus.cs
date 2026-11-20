using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_STATUSES: copy of FINOVA GL_ACCOUNTING_STATUSES (APEX), all columns kept.
/// </summary>
public class GlAccountingStatus : AuditableEntity
{
    public string StatusCode { get; set; } = string.Empty;

    public string StatusNameEn { get; set; } = string.Empty;

    public string? StatusNameAr { get; set; }

    public int? DisplaySeq { get; set; }

    public bool IsFinal { get; set; }

    public bool IsActive { get; set; } = true;
}
