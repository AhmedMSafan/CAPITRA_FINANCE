using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>FCT_CUSTOMER_CLAIM_LINES: one charge of the claim (by its rule charge code), with what is paid of it.</summary>
public class FctCustomerClaimLine : AuditableEntity
{
    public long ClaimLineId { get; set; }

    public long ClaimId { get; set; }

    public int LineNo { get; set; }

    /// <summary>The charge code of the product rule, e.g. ADMIN_FEE (FCT_DISBURSEMENT_CHARGES.RULE_CODE).</summary>
    public string ChargeCode { get; set; } = string.Empty;

    public string? ChargeName { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }
}
