using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_CHEQUE_STATUSES: copy of FINOVA REF_CHEQUE_STATUS (APEX), all columns kept.
/// </summary>
public class AdmLkpChequeStatus : AuditableEntity
{
    public string ChequeStatusCode { get; set; } = string.Empty;

    public string ChequeStatusAr { get; set; } = string.Empty;

    public string ChequeStatusEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
