using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPLICATION_PROPERTYS: copy of FINOVA MFF_APPLICATION_PROPERTY (APEX), all columns kept.
/// </summary>
public class MffApplicationProperty : AuditableEntity
{
    public long AppPropertyId { get; set; }

    public long ApplicationId { get; set; }

    public long PropertyId { get; set; }

    public decimal? RequestedPrice { get; set; }

    public bool IsSelected { get; set; } = true;

    public DateOnly? SelectionDate { get; set; }
}
