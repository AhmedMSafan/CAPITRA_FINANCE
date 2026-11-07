using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LOAN_EVENTS: copy of FINOVA MFF_LOAN_EVENTS (APEX), all columns kept.
/// </summary>
public class MffLoanEvent : AuditableEntity
{
    public long LoanEventId { get; set; }

    public long LoanId { get; set; }

    public string EventTypeCode { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public DateOnly ReportedDate { get; set; }

    public string LoanEventStatus { get; set; } = "OPEN";

    public string? DetailsAr { get; set; }

    public string? ResolutionAr { get; set; }
}
