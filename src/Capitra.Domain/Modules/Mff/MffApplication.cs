using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPLICATIONS: copy of FINOVA MFF_APPLICATIONS (APEX), all columns kept.
/// </summary>
public class MffApplication : AuditableEntity
{
    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = string.Empty;

    public long? ImportStagingId { get; set; }

    public long ProgramVersionId { get; set; }

    public long FinancierId { get; set; }

    public long FundOrgId { get; set; }

    public string SourceChannelCode { get; set; } = string.Empty;

    public long PrimaryPersonId { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public DateOnly ApplicationDate { get; set; }

    public string? FundFileNo { get; set; }

    public string? EligibilityCertNo { get; set; }

    public DateOnly? EligibilityCertDate { get; set; }

    public DateOnly? EligibilityCertExpiryDate { get; set; }

    public decimal? RequestedFinanceAmount { get; set; }

    public decimal? ApprovedFinanceAmount { get; set; }

    public decimal? TotalSubsidyAmount { get; set; }

    public decimal? PropertyPrice { get; set; }

    public decimal? DownpaymentAmount { get; set; }

    public decimal? MonthlyIncomeTotal { get; set; }

    public string? RejectionReason { get; set; }

    public string? CurrentOwnerUser { get; set; }

    public string? BranchCode { get; set; }
}
