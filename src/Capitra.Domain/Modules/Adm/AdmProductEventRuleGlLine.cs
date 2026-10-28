using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_PRODUCT_EVENT_RULE_GL_LINES: a GL entry line of a product event rule (FINOVA_FINANCEING ADM_PRODUCT_EVENT_ACCOUNTING_LINES).
/// A line without a rule charge belongs to the rule itself; a line with one posts that charge.
/// The account is taken straight from the chart of accounts, in place of FINOVA_FINANCEING's account roles and mappings.
/// </summary>
public class AdmProductEventRuleGlLine : AuditableEntity
{
    public long RuleGlLineId { get; set; }

    public long ProductEventRuleId { get; set; }

    /// <summary>Null for a line of the rule itself (FINOVA_FINANCEING scope RULE); set for a charge's line (scope COMPONENT).</summary>
    public long? RuleChargeId { get; set; }

    public int LineNo { get; set; } = 10;

    /// <summary>D or C.</summary>
    public string DebitCreditCode { get; set; } = string.Empty;

    /// <summary>GL_ACCOUNTS.ACCOUNT_ID, a posting account.</summary>
    public long AccountId { get; set; }

    public string? DescriptionTemplate { get; set; }

    public bool IsActive { get; set; } = true;
}
