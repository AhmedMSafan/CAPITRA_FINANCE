using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LOAN_MONTHLY_SNAPSHOTS: copy of FINOVA MFF_LOAN_MONTHLY_SNAPSHOT (APEX), all columns kept.
/// </summary>
public class MffLoanMonthlySnapshot : AuditableEntity
{
    public long SnapshotId { get; set; }

    public DateOnly SnapshotMonthDate { get; set; }

    public long LoanId { get; set; }

    public string LoanStatus { get; set; } = string.Empty;

    public decimal OutstandingPrincipal { get; set; }

    public decimal OverdueAmount { get; set; }

    public int DaysPastDue { get; set; }

    public decimal SubsidyBalance { get; set; }

    public DateOnly? LastPaymentDate { get; set; }

    public decimal? LastPaymentAmount { get; set; }
}
