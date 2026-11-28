using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_LKP_STATUSES: copy of FINOVA LKP_CHEQUE_STATUS (APEX), all columns kept.
/// </summary>
public class ChqLkpStatus : AuditableEntity
{
    public string StatusCode { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? NameAr { get; set; }

    public bool IsTerminal { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
