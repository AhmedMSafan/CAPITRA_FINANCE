using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_DEVELOPER_SIGNATORIES: person authorised to sign for a developer (APEX App 105 pages 103/104).</summary>
public class MtgDeveloperSignatory : AuditableEntity
{
    public long DeveloperSignatoryId { get; set; }

    public long DeveloperId { get; set; }

    public string SignatoryName { get; set; } = string.Empty;

    public string? NationalIdNo { get; set; }

    public string? JobTitle { get; set; }

    public string? AuthorizationReference { get; set; }

    public DateOnly? ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    public bool IsActive { get; set; } = true;
}
