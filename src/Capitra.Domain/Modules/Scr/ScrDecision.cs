using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// SCR_DECISIONS: one decision entered from a letter. A LISTING puts its parties on the Federation Final List; an AMENDMENT changes
/// the scope of a listing; a LIFTING takes the parties off (no suspension: a party is on the list or not; SUSPENSION is history only). Every decision goes through the CAPITRA
/// workflow SCR_DECISION_APPROVAL (review, then approval) and reaches the list only when it is PUBLISHED.
/// </summary>
public class ScrDecision : AuditableEntity
{
    public long DecisionId { get; set; }

    /// <summary>Internal reference, DEC-000001.</summary>
    public string DecisionRef { get; set; } = string.Empty;

    /// <summary>The official number on the order, e.g. FRZ-106/2026.</summary>
    public string OfficialNo { get; set; } = string.Empty;

    /// <summary>One of: LISTING, AMENDMENT, LIFTING (SUSPENSION only on decisions entered before 22 Nov 2026).</summary>
    public string DecisionTypeCode { get; set; } = "LISTING";

    /// <summary>The listing an amendment, suspension or lifting applies to.</summary>
    public long? ParentDecisionId { get; set; }

    public long? LetterId { get; set; }

    /// <summary>What the decision covers, e.g. "Freeze of all assets and accounts".</summary>
    public string ScopeText { get; set; } = string.Empty;

    public DateOnly EffectiveDate { get; set; }

    /// <summary>One of: DRAFT, IN_REVIEW, AWAITING_APPROVAL, RETURNED, PUBLISHED, CANCELLED.</summary>
    public string StatusCode { get; set; } = "DRAFT";

    public string? ReturnReason { get; set; }

    public long? WorkflowInstanceId { get; set; }

    /// <summary>The user who entered it, and that user's person key (employee number, else user name) for segregation of duties.</summary>
    public string EnteredBy { get; set; } = string.Empty;

    public string EnteredByPerson { get; set; } = string.Empty;

    public string? ReviewedBy { get; set; }

    public string? ReviewedByPerson { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ApprovedByPerson { get; set; }

    public DateTime? PublishedAt { get; set; }

    /// <summary>The Final List version the decision was published in.</summary>
    public int? RegistryVersionNo { get; set; }

    public List<ScrDecisionParty> Parties { get; set; } = [];
}

/// <summary>SCR_DECISION_PARTIES: a party a decision is about.</summary>
public class ScrDecisionParty : AuditableEntity
{
    public long DecisionPartyId { get; set; }

    public long DecisionId { get; set; }

    public long PartyId { get; set; }
}

/// <summary>SCR_REGISTRY_VERSIONS: each publication of the Federation Final List, one per published decision.</summary>
public class ScrRegistryVersion : AuditableEntity
{
    public long RegistryVersionId { get; set; }

    public int VersionNo { get; set; }

    public long DecisionId { get; set; }

    public DateTime PublishedAt { get; set; }

    public string PublishedBy { get; set; } = string.Empty;
}
