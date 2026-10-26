using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CALCULATION_SCHEDULES: copy of FINOVA MFF_CALCULATION_SCHEDULE (APEX), all columns kept.
/// </summary>
public class MffCalculationSchedule : AuditableEntity
{
    public long CalcScheduleId { get; set; }

    public long CalcRunId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly? DueMonthDate { get; set; }

    public decimal? OpeningPrincipal { get; set; }

    public decimal? BorrowerPayment { get; set; }

    public decimal? SubsidyPayment { get; set; }

    public decimal? BulletPayment { get; set; }

    public decimal? InterestAmount { get; set; }

    public decimal? PrincipalAmount { get; set; }

    public decimal? ClosingPrincipal { get; set; }
}
