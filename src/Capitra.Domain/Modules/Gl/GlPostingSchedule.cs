using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_SCHEDULES: copy of FINOVA GL_POSTING_SCHEDULES (APEX), all columns kept.
/// </summary>
public class GlPostingSchedule : AuditableEntity
{
    public long PostingScheduleId { get; set; }

    public string ScheduleCode { get; set; } = string.Empty;

    public string ScheduleName { get; set; } = string.Empty;

    public long LedgerId { get; set; }

    public string? SourceCode { get; set; }

    public string? CategoryCode { get; set; }

    public long? PostingProfileId { get; set; }

    /// <summary>One of: ON_DEMAND, HOURLY, DAILY, WEEKLY, MONTHLY, PERIOD_CLOSE.</summary>
    public string FrequencyCode { get; set; } = string.Empty;

    public string? ScheduleTime { get; set; }

    public int? DayOfMonth { get; set; }

    public string? DayOfWeek { get; set; }

    public DateTime? LastRunAt { get; set; }

    public DateTime? NextRunAt { get; set; }

    public bool IsActive { get; set; } = true;
}
