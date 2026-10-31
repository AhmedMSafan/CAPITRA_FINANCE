using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_PORTFOLIO_PAYMENT_METHODS: copy of FINOVA REF_PORTFOLIO_PAYMENT_METHOD (APEX), all columns kept.
/// </summary>
public class MtgLkpPortfolioPaymentMethod : AuditableEntity
{
    public string PaymentMethodCode { get; set; } = string.Empty;

    public string PaymentMethodNameAr { get; set; } = string.Empty;

    public string PaymentMethodNameEn { get; set; } = string.Empty;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
