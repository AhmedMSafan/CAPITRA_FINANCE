using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_PRODUCT_EVENT_RULES: the charges a product applies and their GL entries, posted under the rule's journal source and
/// category (FINOVA_FINANCEING ADM_PRODUCT_EVENT_RULES). A workflow step or a product action points at the rule itself;
/// there is no business event between the product and the rule.
/// </summary>
public class AdmProductEventRule : AuditableEntity
{
    public long ProductEventRuleId { get; set; }

    public long ProductId { get; set; }

    /// <summary>GL_JOURNAL_SOURCES.SOURCE_ID: the journal source the rule's GL entries are posted under.</summary>
    public long JournalSourceId { get; set; }

    /// <summary>GL_JOURNAL_CATEGORIES.CATEGORY_ID: the journal category of the rule's GL entries.</summary>
    public long JournalCategoryId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    public int PriorityNo { get; set; } = 10;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<AdmProductEventRuleCharge> Charges { get; set; } = [];

    public List<AdmProductEventRuleGlLine> GlLines { get; set; } = [];
}
