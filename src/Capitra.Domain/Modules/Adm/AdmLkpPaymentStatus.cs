using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_PAYMENT_STATUSES: copy of FINOVA REF_PAYMENT_STATUS (APEX), all columns kept.
/// </summary>
public class AdmLkpPaymentStatus : AuditableEntity
{
    public string PaymentStatusCode { get; set; } = string.Empty;

    public string PaymentStatusAr { get; set; } = string.Empty;

    public string PaymentStatusEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
