using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_INCOME_SOURCES: copy of FINOVA MFF_INCOME_SOURCES (APEX), all columns kept.
/// </summary>
public class MffIncomeSource : AuditableEntity
{
    public long IncomeId { get; set; }

    public long ApplicationPartyId { get; set; }

    public string IncomeTypeCode { get; set; } = string.Empty;

    public string? EmployerName { get; set; }

    public decimal MonthlyNetIncome { get; set; }

    public decimal? AnnualNetIncome { get; set; }

    public bool IsVerified { get; set; }

    public DateOnly? VerificationDate { get; set; }

    public string? SourceReference { get; set; }

    public string? Notes { get; set; }
}
