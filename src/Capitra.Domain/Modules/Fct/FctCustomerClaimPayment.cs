using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_CUSTOMER_CLAIM_PAYMENTS: one payment of a claim from one payment source, posted under its own GL transaction set
/// (DR customer liability / CR each charge's income).
/// </summary>
public class FctCustomerClaimPayment : AuditableEntity
{
    public long ClaimPaymentId { get; set; }

    public long ClaimId { get; set; }

    public long RepaymentTxnId { get; set; }

    public long RepaymentSourceId { get; set; }

    public DateOnly PaymentDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public string? GlTranSetId { get; set; }
}
