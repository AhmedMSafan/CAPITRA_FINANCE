using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_SUBSIDY_ACCOUNTS: copy of FINOVA MFF_SUBSIDY_ACCOUNTS (APEX), all columns kept.
/// </summary>
public class MffSubsidyAccount : AuditableEntity
{
    public long SubsidyAccountId { get; set; }

    public long LoanId { get; set; }

    public string AccountNo { get; set; } = string.Empty;

    public DateOnly OpenDate { get; set; }

    public decimal OpeningAmount { get; set; }

    public decimal CurrentBalance { get; set; }

    public string AccountStatus { get; set; } = "ACTIVE";

    public string? CoreSystemRef { get; set; }
}
