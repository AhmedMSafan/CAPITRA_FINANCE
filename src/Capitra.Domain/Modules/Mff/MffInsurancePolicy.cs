using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_INSURANCE_POLICIES: copy of FINOVA MFF_INSURANCE_POLICIES (APEX), all columns kept.
/// </summary>
public class MffInsurancePolicy : AuditableEntity
{
    public long PolicyId { get; set; }

    public long LoanId { get; set; }

    public long InsurerOrgId { get; set; }

    public string PolicyNo { get; set; } = string.Empty;

    public string PolicyType { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? PremiumAmount { get; set; }

    public string InsurancePolicyStatus { get; set; } = "ACTIVE";
}
