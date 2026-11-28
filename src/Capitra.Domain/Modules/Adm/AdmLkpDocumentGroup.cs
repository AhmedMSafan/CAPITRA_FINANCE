using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_DOCUMENT_GROUPS: copy of FINOVA REF_DOCUMENT_GROUP (APEX), all columns kept.
/// </summary>
public class AdmLkpDocumentGroup : AuditableEntity
{
    public string GroupCode { get; set; } = string.Empty;

    public string GroupNameAr { get; set; } = string.Empty;

    public string GroupNameEn { get; set; } = string.Empty;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
