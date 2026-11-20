using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_PROFILES: copy of FINOVA GL_POSTING_PROFILES (APEX), all columns kept.
/// </summary>
public class GlPostingProfile : AuditableEntity
{
    public long PostingProfileId { get; set; }

    public string ProfileCode { get; set; } = string.Empty;

    public string ProfileNameEn { get; set; } = string.Empty;

    public string? ProfileNameAr { get; set; }

    public long LedgerId { get; set; }

    public string? SourceCode { get; set; }

    public string? CategoryCode { get; set; }

    /// <summary>One of: MANUAL, REVIEWED, AUTO, SCHEDULED.</summary>
    public string PostingMode { get; set; } = "REVIEWED";

    /// <summary>One of: JOURNAL, BATCH, SOURCE_TRANSACTION, PERIOD.</summary>
    public string PostingLevel { get; set; } = "JOURNAL";

    /// <summary>One of: STANDARD, SUMMARY_ONLY, DAILY_AND_PERIOD, NO_BALANCE_UPDATE.</summary>
    public string BalanceUpdateMode { get; set; } = "STANDARD";

    public bool IsAutoPostAllowed { get; set; }

    public bool IsBatchPostAllowed { get; set; } = true;

    public bool IsApprovalRequired { get; set; } = true;

    public bool IsUnapprovedAllowed { get; set; }

    public bool IsBackdatedAllowed { get; set; }

    public bool IsFuturePeriodAllowed { get; set; }

    public bool IsBalancedRequired { get; set; } = true;

    public bool IsFxRateRequired { get; set; } = true;

    public bool IsUpdateDailyBalance { get; set; } = true;

    public bool IsCreateAudit { get; set; } = true;

    public bool IsNotifySource { get; set; } = true;

    public int MaxRetryCount { get; set; } = 3;

    public int Priority { get; set; } = 100;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
