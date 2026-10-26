using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_INSTALLMENTS: copy of FINOVA MFF_INSTALLMENTS (APEX), all columns kept.
/// </summary>
public class MffInstallment : AuditableEntity
{
    public long InstallmentId { get; set; }

    public long LoanId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal? OpeningPrincipal { get; set; }

    public decimal? PrincipalDue { get; set; }

    public decimal? InterestDue { get; set; }

    public decimal? InsuranceDue { get; set; }

    public decimal? FeesDue { get; set; }

    public decimal? SubsidyDue { get; set; }

    public decimal? BorrowerDue { get; set; }

    public decimal? TotalDue { get; set; }

    public decimal? PaidAmount { get; set; }

    public string PaymentStatusCode { get; set; } = string.Empty;

    public DateOnly? PaidDate { get; set; }

    public decimal? ClosingPrincipal { get; set; }
}
