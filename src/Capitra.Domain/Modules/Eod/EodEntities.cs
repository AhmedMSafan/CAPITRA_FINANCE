using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Eod;

/// <summary>
/// EOD_JOBS: the jobs of the End of Day (night batch) of every application, in the order they run. What a job does is in the code;
/// this row holds whether it runs and its place in the night.
/// </summary>
public class EodJob : AuditableEntity
{
    public long JobId { get; set; }

    /// <summary>The job's code in the code, e.g. LND_DAILY, SCR_INVOICES.</summary>
    public string JobCode { get; set; } = string.Empty;

    /// <summary>The application the job belongs to: LND, SCR ...</summary>
    public string ModuleCode { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    /// <summary>Jobs run from the lowest number up.</summary>
    public int SequenceNo { get; set; } = 10;

    /// <summary>A job switched off is skipped (recorded as SKIPPED on each day).</summary>
    public bool IsEnabled { get; set; } = true;

    public string? Notes { get; set; }
}

/// <summary>EOD_JOB_PARAMETERS: a setting of a job (e.g. the days before a licence ends to send reminders). The list of settings is in the code.</summary>
public class EodJobParameter : AuditableEntity
{
    public long JobParameterId { get; set; }

    public long JobId { get; set; }

    public string ParameterCode { get; set; } = string.Empty;

    public string? ParameterValue { get; set; }
}

/// <summary>
/// EOD_DAYS: one business day of the night batch. Days run in order and never ahead of today. A day is OPEN while any of its jobs is
/// not done; the next day can run only once it is DONE.
/// </summary>
public class EodDay : AuditableEntity
{
    public long DayId { get; set; }

    public DateOnly BusinessDate { get; set; }

    /// <summary>OPEN or DONE.</summary>
    public string StatusCode { get; set; } = "OPEN";

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string RunBy { get; set; } = string.Empty;

    public List<EodDayJob> Jobs { get; set; } = [];
}

/// <summary>EOD_DAY_JOBS: what one job did on one business day.</summary>
public class EodDayJob : AuditableEntity
{
    public long DayJobId { get; set; }

    public long DayId { get; set; }

    public string JobCode { get; set; } = string.Empty;

    public int SequenceNo { get; set; }

    /// <summary>PENDING, RUNNING, DONE, FAILED or SKIPPED.</summary>
    public string StatusCode { get; set; } = "PENDING";

    /// <summary>How many items the job handled (loans, invoices, e-mails ...).</summary>
    public int ItemsCount { get; set; }

    /// <summary>What the job did, in words.</summary>
    public string? Summary { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>How many times the job ran on this day (a failed job is run again after the cause is fixed).</summary>
    public int Attempts { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? RunBy { get; set; }
}
