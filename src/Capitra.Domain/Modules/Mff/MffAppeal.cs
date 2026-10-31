using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPEALS: copy of FINOVA MFF_APPEALS (APEX), all columns kept.
/// </summary>
public class MffAppeal : AuditableEntity
{
    public long AppealId { get; set; }

    public long ApplicationId { get; set; }

    public DateOnly AppealDate { get; set; }

    public long? RejectionDecisionId { get; set; }

    public string ReasonAr { get; set; } = string.Empty;

    public string? DecisionCode { get; set; }

    public DateOnly? DecisionDate { get; set; }

    public string? DecisionReasonAr { get; set; }
}
