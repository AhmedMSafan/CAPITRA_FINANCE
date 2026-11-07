using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_REPAYMENT_ALLOCATIONS: copy of FINOVA FNV_REPAYMENT_ALLOCATIONS (APEX), all columns kept.
/// </summary>
public class FctRepaymentAllocation : AuditableEntity
{
    public long AllocationId { get; set; }

    public long RepaymentTxnId { get; set; }

    public long RepaymentSourceId { get; set; }

    /// <summary>One of: SCHEDULE, CHARGE.</summary>
    public string TargetType { get; set; } = string.Empty;

    public long TargetId { get; set; }

    public long DisbursementId { get; set; }

    /// <summary>One of: PRINCIPAL, INTEREST, PENALTY, FEE, CHARGE.</summary>
    public string ComponentCode { get; set; } = string.Empty;

    /// <summary>One of: AMOUNT, PERCENT.</summary>
    public string AllocationMode { get; set; } = string.Empty;

    public decimal RequestedValue { get; set; }

    public decimal AllocationAmount { get; set; }

    public DateOnly AllocationDate { get; set; }

    /// <summary>One of: ACTIVE, REVERSED.</summary>
    public string StatusCode { get; set; } = "ACTIVE";

    public string? ReversalReason { get; set; }

    public string? ReversedBy { get; set; }

    public DateTime? ReversedAt { get; set; }
}
