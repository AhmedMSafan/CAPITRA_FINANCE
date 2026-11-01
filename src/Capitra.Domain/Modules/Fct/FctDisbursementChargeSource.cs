using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_DISBURSEMENT_CHARGE_SOURCES: the customer's payment sources chosen on the draft to pay the request's charges, in order
/// (agreed with Ahmed, 1 Nov 2026). Complete pays the charges from them.
/// </summary>
public class FctDisbursementChargeSource : AuditableEntity
{
    public long DisbursementChargeSourceId { get; set; }

    public long DisbursementId { get; set; }

    public long RepaymentSourceId { get; set; }

    public int SortOrder { get; set; }
}
