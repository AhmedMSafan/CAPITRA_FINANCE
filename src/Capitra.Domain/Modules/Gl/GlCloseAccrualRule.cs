using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCRUAL_RULES: copy of FINOVA GL_CLOSE_ACCRUAL_RULES (APEX), all columns kept.
/// </summary>
public class GlCloseAccrualRule : AuditableEntity
{
    public long AccrualRuleId { get; set; }

    public string AccrualRuleCode { get; set; } = string.Empty;

    public string AccrualRuleNameEn { get; set; } = string.Empty;

    public string? AccrualRuleNameAr { get; set; }

    public long? CloseProfileId { get; set; }

    public long? LedgerId { get; set; }

    public string? SourceModule { get; set; }

    public string AccrualCategoryCode { get; set; } = string.Empty;

    /// <summary>One of: FIXED, SOURCE_OPEN_ITEMS, FORMULA, PERCENTAGE, ESTIMATE, MANUAL_INPUT.</summary>
    public string AccrualMethod { get; set; } = string.Empty;

    /// <summary>One of: NONE, NEXT_PERIOD, SPECIFIC_DATE, ON_SOURCE_POSTING.</summary>
    public string ReversalPolicy { get; set; } = "NEXT_PERIOD";

    public bool IsAutoReverse { get; set; } = true;

    public bool IsAutoPost { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? Description { get; set; }
}
