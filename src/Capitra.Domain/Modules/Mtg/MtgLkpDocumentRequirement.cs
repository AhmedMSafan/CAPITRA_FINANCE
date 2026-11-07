using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_DOCUMENT_REQUIREMENTS: copy of FINOVA REF_PORT_DOCUMENT_REQUIREMENT (APEX page 611), all columns kept.
/// </summary>
public class MtgLkpDocumentRequirement : AuditableEntity
{
    public string RequirementCode { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }

    public bool IsOriginalRequired { get; set; }

    public bool IsVerifiedRequired { get; set; }

    public int? MaxAgeDays { get; set; }

    public bool IsActive { get; set; } = true;
}
