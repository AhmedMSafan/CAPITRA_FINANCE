using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_DEVELOPER_SIGNATORIES: copy of FINOVA FNV_REALESTATE_DEVELOPER_SIGNATORY (APEX), all columns kept.
/// </summary>
public class MtgDeveloperSignatory : AuditableEntity
{
    public long SignatoryId { get; set; }

    public long DeveloperId { get; set; }

    public string SignatoryName { get; set; } = string.Empty;

    public string? NationalId { get; set; }

    public string? JobTitle { get; set; }

    public string? AuthorizationReference { get; set; }

    public DateOnly? ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    public bool IsActive { get; set; } = true;
}
