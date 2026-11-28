using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CREDIT_DECISIONS: copy of FINOVA MFF_CREDIT_DECISIONS (APEX), all columns kept.
/// </summary>
public class MffCreditDecision : AuditableEntity
{
    public long DecisionId { get; set; }

    public long ApplicationId { get; set; }

    public string DecisionCode { get; set; } = string.Empty;

    public DateOnly DecisionDate { get; set; }

    public decimal? ApprovedFinanceAmount { get; set; }

    public int? ApprovedTermMonths { get; set; }

    public decimal? ApprovedRatePct { get; set; }

    public string? ConditionsText { get; set; }

    public string? DecidedBy { get; set; }

    public string? CommitteeReference { get; set; }

    public string? Notes { get; set; }
}
