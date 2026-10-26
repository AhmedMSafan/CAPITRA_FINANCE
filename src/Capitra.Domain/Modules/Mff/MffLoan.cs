using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LOANS: copy of FINOVA MFF_LOANS (APEX), all columns kept.
/// </summary>
public class MffLoan : AuditableEntity
{
    public long LoanId { get; set; }

    public long ApplicationId { get; set; }

    public long ContractId { get; set; }

    public string LoanAccountNo { get; set; } = string.Empty;

    public DateOnly DisbursementDate { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal? AnnualRatePct { get; set; }

    public int TermMonths { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public decimal? InitialInstallment { get; set; }

    public string InstallmentFrequency { get; set; } = "MONTHLY";

    public string LoanStatus { get; set; } = "ACTIVE";

    public decimal? OutstandingPrincipal { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public int DelinquentDays { get; set; }

    public string? CoreSystemRef { get; set; }
}
