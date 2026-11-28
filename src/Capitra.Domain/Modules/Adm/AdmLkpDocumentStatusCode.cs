using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_DOCUMENT_STATUS_CODES: copy of FINOVA_FINANCEING ADM_LKP_DOCUMENT_STATUS_CODES (not in the APEX export),
/// the status a factoring agreement document is uploaded with.
/// </summary>
public class AdmLkpDocumentStatusCode : AuditableEntity
{
    public long StatusId { get; set; }

    public string StatusCodeValue { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
