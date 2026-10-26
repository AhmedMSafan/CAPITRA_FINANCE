using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_DOCUMENT_TYPES: copy of FINOVA REF_DOCUMENT_TYPE (APEX), all columns kept.
/// </summary>
public class AdmLkpDocumentType : AuditableEntity
{
    public long DocumentTypeId { get; set; }

    public string DocumentTypeCode { get; set; } = string.Empty;

    public string DocumentTypeName { get; set; } = string.Empty;

    public string? DocumentTypeNameAr { get; set; }

    public string? Description { get; set; }

    public int? MaxFileSizeMb { get; set; }

    public string? AllowedExtensions { get; set; }

    public bool IsMandatory { get; set; }

    public int? SortOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsExpiryRequired { get; set; }

    public string? GroupCode { get; set; }

    public string? DocumentCategory { get; set; }
}
