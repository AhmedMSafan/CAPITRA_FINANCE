using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_ACCOUNTING_RULE_SETS: copy of FINOVA GL_AUTO_ACCOUNTING_RULE_SETS (APEX), all columns kept.
/// </summary>
public class GlAutoAccountingRuleSet : AuditableEntity
{
    public long RuleSetId { get; set; }

    public string RuleSetCode { get; set; } = string.Empty;

    public string RuleSetNameEn { get; set; } = string.Empty;

    public string? RuleSetNameAr { get; set; }

    public long SourceId { get; set; }

    public long EventTypeId { get; set; }

    public long? LedgerId { get; set; }

    public long? LegalEntityId { get; set; }

    public string? BusinessLineCode { get; set; }

    public string? ProductCode { get; set; }

    public string? TransactionTypeCode { get; set; }

    public string? CurrencyCode { get; set; }

    public int PriorityNo { get; set; } = 100;

    public int VersionNo { get; set; } = 1;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    /// <summary>One of: DRAFT, ACTIVE, INACTIVE, RETIRED, ERROR.</summary>
    public string RuleStatus { get; set; } = "DRAFT";

    /// <summary>One of: NOT_SUBMITTED, SUBMITTED, APPROVED, REJECTED, RETURNED.</summary>
    public string ApprovalStatus { get; set; } = "NOT_SUBMITTED";

    /// <summary>One of: DRAFT_JOURNAL, READY_FOR_REVIEW, AUTO_SUBMIT, AUTO_APPROVE, AUTO_POST.</summary>
    public string GenerationMode { get; set; } = "DRAFT_JOURNAL";

    public bool IsBalancingRequired { get; set; } = true;

    public bool IsOverrideAllowed { get; set; }

    public bool IsManualAdjustAllowed { get; set; }

    /// <summary>One of: SWITCH_DR_CR, NEGATE_AMOUNT, CUSTOM_RULE, NO_AUTO_REVERSAL.</summary>
    public string ReversalBehavior { get; set; } = "SWITCH_DR_CR";

    /// <summary>One of: BLOCK_DUPLICATE, ALLOW_DUPLICATE, WARN_ONLY, REPLACE_DRAFT.</summary>
    public string DuplicatePolicy { get; set; } = "BLOCK_DUPLICATE";

    public long? DefaultCategoryId { get; set; }

    public string? Description { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}
