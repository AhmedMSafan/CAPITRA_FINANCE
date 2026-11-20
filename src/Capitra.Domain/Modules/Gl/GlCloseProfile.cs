using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_PROFILES: copy of FINOVA GL_CLOSE_PROFILES (APEX), all columns kept.
/// </summary>
public class GlCloseProfile : AuditableEntity
{
    public long CloseProfileId { get; set; }

    public string CloseProfileCode { get; set; } = string.Empty;

    public string CloseProfileNameEn { get; set; } = string.Empty;

    public string? CloseProfileNameAr { get; set; }

    /// <summary>One of: MONTH_END, YEAR_END, QUARTER_END, ADHOC.</summary>
    public string CloseType { get; set; } = string.Empty;

    public long? LedgerId { get; set; }

    public long? LegalEntityId { get; set; }

    public string? BusinessLineCode { get; set; }

    /// <summary>One of: MONTHLY, QUARTERLY, YEARLY, ADHOC.</summary>
    public string CloseFrequency { get; set; } = "MONTHLY";

    public bool IsDefaultProfile { get; set; }

    public bool IsTaskOwnerRequired { get; set; } = true;

    public bool IsAttachmentRequired { get; set; }

    public bool IsOverrideAllowed { get; set; }

    public bool IsAutoGenerateAccruals { get; set; }

    public bool IsAutoGenerateRecurring { get; set; }

    public bool IsAutoRunFxRevaluation { get; set; }

    public bool IsAutoLockPeriod { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? Description { get; set; }
}
