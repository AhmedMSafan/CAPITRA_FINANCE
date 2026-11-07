using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_GL_ACCOUNT_MAPS: copy of FINOVA MFF_GL_ACCOUNT_MAP (APEX), all columns kept.
/// </summary>
public class MffGlAccountMap : AuditableEntity
{
    public long MapId { get; set; }

    public long FinancierId { get; set; }

    public string EventCode { get; set; } = string.Empty;

    public string? DebitAccount { get; set; }

    public string? CreditAccount { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;
}
