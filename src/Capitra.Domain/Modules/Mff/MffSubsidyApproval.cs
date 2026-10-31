using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_SUBSIDY_APPROVALS: copy of FINOVA MFF_SUBSIDY_APPROVALS (APEX), all columns kept.
/// </summary>
public class MffSubsidyApproval : AuditableEntity
{
    public long SubsidyApprovalId { get; set; }

    public long ApplicationId { get; set; }

    public string SupportTypeCode { get; set; } = string.Empty;

    public string? ApprovalNo { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? InstallmentSupportNpv { get; set; }

    public decimal? DownpaymentSupport { get; set; }

    public decimal? TotalSupport { get; set; }

    public bool IsDedicatedAccount { get; set; }

    public string ApprovalStatus { get; set; } = "PENDING";

    public string? FundNotes { get; set; }
}
