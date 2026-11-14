using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_DELINQUENCIES: copy of FINOVA MFF_DELINQUENCIES (APEX), all columns kept.
/// </summary>
public class MffDelinquency : AuditableEntity
{
    public long DelinquencyId { get; set; }

    public long LoanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string StageCode { get; set; } = string.Empty;

    public int DaysPastDue { get; set; }

    public decimal OverdueAmount { get; set; }

    public int OverdueInstallments { get; set; }

    public string DelinquencyStatus { get; set; } = "OPEN";

    public string? CollectionActionAr { get; set; }
}
