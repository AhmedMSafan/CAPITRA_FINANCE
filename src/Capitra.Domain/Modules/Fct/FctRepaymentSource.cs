using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_REPAYMENT_SOURCES: copy of FINOVA FNV_REPAYMENT_SOURCES (APEX), all columns kept.
/// </summary>
public class FctRepaymentSource : AuditableEntity
{
    public long RepaymentSourceId { get; set; }

    public long RepaymentTxnId { get; set; }

    /// <summary>One of: CHEQUE, BANK_TRANSFER, CREDIT_BALANCE.</summary>
    public string SourceType { get; set; } = string.Empty;

    public long? ChequeId { get; set; }

    public long? SourceRepaymentTxnId { get; set; }

    public long? SourceGlAccountId { get; set; }

    public string? SourceReference { get; set; }

    public decimal SourceAmount { get; set; }

    public decimal AllocatedAmount { get; set; }

    /// <summary>One of: ACTIVE, REVERSED, CANCELLED.</summary>
    public string StatusCode { get; set; } = "ACTIVE";
}
