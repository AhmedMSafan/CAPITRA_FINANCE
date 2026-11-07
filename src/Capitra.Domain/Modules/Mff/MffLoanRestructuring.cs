using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LOAN_RESTRUCTURINGS: copy of FINOVA MFF_LOAN_RESTRUCTURINGS (APEX), all columns kept.
/// </summary>
public class MffLoanRestructuring : AuditableEntity
{
    public long RestructuringId { get; set; }

    public long LoanId { get; set; }

    public DateOnly RequestDate { get; set; }

    public DateOnly? DecisionDate { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public string? ReasonAr { get; set; }

    public int? OldTermMonths { get; set; }

    public int? NewTermMonths { get; set; }

    public decimal? OldRatePct { get; set; }

    public decimal? NewRatePct { get; set; }

    public decimal? OldInstallment { get; set; }

    public decimal? NewInstallment { get; set; }

    public decimal CapitalizedArrears { get; set; }

    public string LoanRestructuringStatus { get; set; } = "REQUESTED";

    public string? ApprovedBy { get; set; }

    public string? NotesAr { get; set; }
}
