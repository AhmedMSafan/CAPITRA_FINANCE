using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LEGAL_ACTIONS: copy of FINOVA MFF_LEGAL_ACTIONS (APEX), all columns kept.
/// </summary>
public class MffLegalAction : AuditableEntity
{
    public long LegalActionId { get; set; }

    public long? LoanId { get; set; }

    public long? ApplicationId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public DateOnly ActionDate { get; set; }

    public string? CaseNo { get; set; }

    public string LegalActionStatus { get; set; } = string.Empty;

    public string? OutcomeAr { get; set; }

    public DateOnly? NextActionDate { get; set; }
}
