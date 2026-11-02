using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_DISBURSEMENT_CUSTOM_SCHEDULES: the principal payments of an irregular (CUSTOM_SCHEDULE) request, entered on the disbursement page
/// (Ahmed, 2 Nov 2026): a due date and a principal amount each. They must add up to the amount financed; the last date is the maturity.
/// </summary>
public class FctDisbursementCustomSchedule : AuditableEntity
{
    public long CustomScheduleId { get; set; }

    public long DisbursementId { get; set; }

    public int LineNo { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal PrincipalAmount { get; set; }
}
