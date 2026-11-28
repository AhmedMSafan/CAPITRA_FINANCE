using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PAYMENTS: copy of FINOVA MFF_PAYMENTS (APEX), all columns kept.
/// </summary>
public class MffPayment : AuditableEntity
{
    public long PaymentId { get; set; }

    public long LoanId { get; set; }

    public long? InstallmentId { get; set; }

    public DateOnly PaymentDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public string PaymentType { get; set; } = "INSTALLMENT";

    public string? PaymentChannel { get; set; }

    public string? ReferenceNo { get; set; }

    public decimal? AllocatedPrincipal { get; set; }

    public decimal? AllocatedInterest { get; set; }

    public decimal? AllocatedFees { get; set; }

    public decimal? AllocatedInsurance { get; set; }

    public string StatusCode { get; set; } = string.Empty;
}
