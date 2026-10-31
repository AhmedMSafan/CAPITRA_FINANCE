using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_APPROVALS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_APPROVAL (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioApproval : AuditableEntity
{
    public long ApprovalId { get; set; }

    public long PortfolioId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public int SequenceNo { get; set; }

    public string? AssignedTo { get; set; }

    public DateOnly? AssignedDate { get; set; }

    public string? DecisionCode { get; set; }

    public DateOnly? DecisionDate { get; set; }

    public string? Comments { get; set; }

    public string? ReturnToStepCode { get; set; }
}
