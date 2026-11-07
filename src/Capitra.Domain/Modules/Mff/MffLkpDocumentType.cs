using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_DOCUMENT_TYPES: copy of FINOVA REF_MFF_DOCUMENT_TYPE (APEX), all columns kept.
/// </summary>
public class MffLkpDocumentType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
