using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_LKP_DOCUMENT_TYPES: document types (APEX App 105 pages 15/16).</summary>
public class AdmLkpDocumentType : LookupEntity
{
    public string? DocumentGroupCode { get; set; }

    public string? DocumentCategory { get; set; }

    /// <summary>Comma-separated file extensions, e.g. "pdf,jpg".</summary>
    public string? AllowedExtensions { get; set; }

    public int? MaxFileSizeMb { get; set; }

    public bool IsMandatory { get; set; }

    public bool IsExpiryRequired { get; set; }
}
