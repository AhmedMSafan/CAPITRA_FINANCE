using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Eod;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Eod;

public sealed record EodParameterDto(string Code, string LabelEn, string LabelAr, string HelpEn, string HelpAr, string DefaultValue, string? Value);

public sealed record EodJobDto(
    string JobCode, string ModuleCode, string NameEn, string NameAr, string DescriptionEn, string DescriptionAr, int SequenceNo, bool IsEnabled,
    bool IsAvailable, string? Notes, IReadOnlyList<EodParameterDto> Parameters);

public sealed record EodDayJobDto(
    string JobCode, string ModuleCode, string NameEn, string NameAr, int SequenceNo, string StatusCode, int ItemsCount, string? Summary,
    string? ErrorMessage, int Attempts, DateTime? StartedAt, DateTime? FinishedAt, string? RunBy);

public sealed record EodDayDto(long DayId, DateOnly BusinessDate, string StatusCode, DateTime StartedAt, DateTime? FinishedAt, string RunBy, IReadOnlyList<EodDayJobDto> Jobs);

public sealed record EodPageDto(
    DateOnly Today, DateOnly? LastDoneDate, DateOnly? NextDate, bool IsNextDayOpen, bool CanRun, string? Blocked,
    IReadOnlyList<EodDayDto> Days, IReadOnlyList<EodJobDto> Jobs);

/// <summary>Run the night batch up to the day given (today when none), from the next day not done.</summary>
public sealed record EodRunRequest(DateOnly? ToDate);

public sealed record EodRunResultDto(int DaysDone, DateOnly? OpenDate, string Message);

public sealed record EodJobRequest(bool IsEnabled, int SequenceNo, string? Notes, IReadOnlyDictionary<string, string?>? Parameters);

/// <summary>
/// End of Day: the night batch of every application (Ahmed, 22 Nov 2026). An administrator runs it by hand, for today or for several
/// days up to a date. Days run in order, never ahead of today. On each day every job switched on runs in the order of EOD_JOBS; when one
/// fails the others still run and the day stays OPEN. Running again runs only the jobs not done yet, and the next days follow once the
/// day is DONE. Each job keeps its own detail (e.g. LND_EOD_RUNS for Retail Lending).
/// </summary>
public sealed class EodService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser, IEnumerable<IEodJob> jobs)
{
    public static class Statuses
    {
        public const string Open = "OPEN";
        public const string Done = "DONE";
        public const string Pending = "PENDING";
        public const string Running = "RUNNING";
        public const string Failed = "FAILED";
        public const string Skipped = "SKIPPED";
    }

    private readonly Dictionary<string, IEodJob> registry = jobs.ToDictionary(j => j.JobCode, StringComparer.Ordinal);

    // ---- reading --------------------------------------------------------------------------------------------------------------

    public async Task<EodPageDto> GetAsync(bool canRun, CancellationToken ct)
    {
        await EnsureJobsAsync(ct);
        var today = GlTime.Today(clock);
        var (next, open) = await NextAsync(today, ct);
        var lastDone = await db.Set<EodDay>().AsNoTracking().Where(d => d.StatusCode == Statuses.Done).MaxAsync(d => (DateOnly?)d.BusinessDate, ct);
        var days = await db.Set<EodDay>().AsNoTracking().Include(d => d.Jobs).OrderByDescending(d => d.BusinessDate).Take(60).ToListAsync(ct);
        var jobList = await JobsAsync(ct);
        var names = jobList.ToDictionary(j => j.JobCode);
        var blocked = next is null ? $"End of day is done up to today ({today:yyyy-MM-dd})." : null;
        return new EodPageDto(today, lastDone, next, open, canRun && next is not null, blocked,
            [.. days.Select(d => new EodDayDto(d.DayId, d.BusinessDate, d.StatusCode, d.StartedAt, d.FinishedAt, d.RunBy,
                [.. d.Jobs.OrderBy(j => j.SequenceNo).ThenBy(j => j.DayJobId).Select(j =>
                {
                    var n = names.GetValueOrDefault(j.JobCode);
                    return new EodDayJobDto(j.JobCode, n?.ModuleCode ?? string.Empty, n?.NameEn ?? j.JobCode, n?.NameAr ?? j.JobCode, j.SequenceNo, j.StatusCode,
                        j.ItemsCount, j.Summary, j.ErrorMessage, j.Attempts, j.StartedAt, j.FinishedAt, j.RunBy);
                })]))],
            jobList);
    }

    /// <summary>Every job, in the order it runs, with its settings.</summary>
    public async Task<IReadOnlyList<EodJobDto>> JobsAsync(CancellationToken ct)
    {
        await EnsureJobsAsync(ct);
        var rows = await db.Set<EodJob>().AsNoTracking().OrderBy(j => j.SequenceNo).ThenBy(j => j.JobId).ToListAsync(ct);
        var values = (await db.Set<EodJobParameter>().AsNoTracking().ToListAsync(ct)).ToLookup(p => p.JobId);
        return [.. rows.Select(r =>
        {
            var job = registry.GetValueOrDefault(r.JobCode);
            var saved = values[r.JobId].ToDictionary(p => p.ParameterCode, p => p.ParameterValue);
            return new EodJobDto(r.JobCode, r.ModuleCode, r.NameEn, r.NameAr, job?.DescriptionEn ?? string.Empty, job?.DescriptionAr ?? string.Empty,
                r.SequenceNo, r.IsEnabled, job is not null, r.Notes,
                [.. (job?.Parameters ?? []).Select(p => new EodParameterDto(p.Code, p.LabelEn, p.LabelAr, p.HelpEn, p.HelpAr, p.DefaultValue, saved.GetValueOrDefault(p.Code)))]);
        })];
    }

    /// <summary>The next day to run (the open day, else the day after the last done, else today) and whether it is open; null when done up to today.</summary>
    private async Task<(DateOnly? Next, bool Open)> NextAsync(DateOnly today, CancellationToken ct)
    {
        var open = await db.Set<EodDay>().AsNoTracking().Where(d => d.StatusCode == Statuses.Open).OrderBy(d => d.BusinessDate)
            .Select(d => (DateOnly?)d.BusinessDate).FirstOrDefaultAsync(ct);
        if (open is { } o) return (o, true);
        var lastDone = await db.Set<EodDay>().AsNoTracking().MaxAsync(d => (DateOnly?)d.BusinessDate, ct);
        var next = lastDone?.AddDays(1) ?? today;
        return (next > today ? null : next, false);
    }

    // ---- settings ---------------------------------------------------------------------------------------------------------------

    public async Task<EodJobDto> SaveJobAsync(string jobCode, EodJobRequest r, CancellationToken ct)
    {
        await EnsureJobsAsync(ct);
        var row = await db.Set<EodJob>().FirstOrDefaultAsync(j => j.JobCode == jobCode, ct) ?? throw new NotFoundException($"Job {jobCode} was not found.");
        if (r.SequenceNo is < 1 or > 9999)
            throw BusinessValidationException.For("sequenceNo", "The order is a number from 1 to 9999.");
        row.IsEnabled = r.IsEnabled;
        row.SequenceNo = r.SequenceNo;
        row.Notes = string.IsNullOrWhiteSpace(r.Notes) ? null : r.Notes.Trim();
        var definitions = registry.GetValueOrDefault(jobCode)?.Parameters ?? [];
        var saved = await db.Set<EodJobParameter>().Where(p => p.JobId == row.JobId).ToListAsync(ct);
        foreach (var d in definitions)
        {
            if (r.Parameters is null || !r.Parameters.TryGetValue(d.Code, out var value)) continue;
            value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (value?.Length > 500)
                throw BusinessValidationException.For(d.Code, "The value is too long.");
            var p = saved.FirstOrDefault(x => x.ParameterCode == d.Code);
            if (p is null) db.Add(new EodJobParameter { JobId = row.JobId, ParameterCode = d.Code, ParameterValue = value });
            else p.ParameterValue = value;
        }
        await db.SaveChangesAsync(ct);
        return (await JobsAsync(ct)).First(j => j.JobCode == jobCode);
    }

    /// <summary>A job in the code without its row gets one, at the end of the night, switched on.</summary>
    private async Task EnsureJobsAsync(CancellationToken ct)
    {
        var have = await db.Set<EodJob>().AsNoTracking().Select(j => j.JobCode).ToListAsync(ct);
        var missing = registry.Values.Where(j => !have.Contains(j.JobCode)).ToList();
        if (missing.Count == 0) return;
        var last = await db.Set<EodJob>().AsNoTracking().MaxAsync(j => (int?)j.SequenceNo, ct) ?? 0;
        foreach (var j in missing)
        {
            last += 10;
            db.Add(new EodJob { JobCode = j.JobCode, ModuleCode = j.ModuleCode, NameEn = j.NameEn, NameAr = j.NameAr, SequenceNo = last });
        }
        await db.SaveChangesAsync(ct);
    }

    // ---- running ----------------------------------------------------------------------------------------------------------------

    /// <summary>Runs the days in order from the next one not done up to the day asked (today when none); stops at a day left open.</summary>
    public async Task<EodRunResultDto> RunAsync(EodRunRequest r, CancellationToken ct)
    {
        await EnsureJobsAsync(ct);
        var today = GlTime.Today(clock);
        var to = r.ToDate ?? today;
        if (to > today)
            throw BusinessValidationException.For("toDate", "End of day cannot run for a day after today.");
        var (next, _) = await NextAsync(today, ct);
        if (next is not { } day)
            throw new BusinessRuleException($"End of day is done up to today ({today:yyyy-MM-dd}).");
        if (day > to)
            throw BusinessValidationException.For("toDate", $"End of day is done up to {day.AddDays(-1):yyyy-MM-dd}. Choose a later day.");

        var done = 0;
        while (day <= to)
        {
            var status = await RunDayAsync(day, ct);
            if (status != Statuses.Done)
                return new EodRunResultDto(done, day, done == 0
                    ? $"{day:yyyy-MM-dd} is still open: a job failed. Fix the cause and run again; only the jobs not done run."
                    : $"{done} day(s) done. {day:yyyy-MM-dd} is still open: a job failed. Fix the cause and run again; only the jobs not done run.");
            done++;
            day = day.AddDays(1);
        }
        return new EodRunResultDto(done, null, $"End of day done up to {to:yyyy-MM-dd} ({done} day(s)).");
    }

    /// <summary>One business day: each job not done yet, in order; the day is DONE once every job is done or skipped.</summary>
    private async Task<string> RunDayAsync(DateOnly date, CancellationToken ct)
    {
        var by = currentUser.UserName ?? "SYSTEM";
        var day = await db.Set<EodDay>().Include(d => d.Jobs).FirstOrDefaultAsync(d => d.BusinessDate == date, ct);
        if (day?.Jobs.Any(j => j.StatusCode == Statuses.Running && j.StartedAt > clock.UtcNow.AddHours(-1)) == true)
            throw new BusinessRuleException($"End of day of {date:yyyy-MM-dd} is running. Wait for it to finish.");
        if (day is null)
        {
            day = new EodDay { BusinessDate = date, StatusCode = Statuses.Open, StartedAt = clock.UtcNow, RunBy = by };
            db.Add(day);
        }

        var jobRows = await db.Set<EodJob>().AsNoTracking().OrderBy(j => j.SequenceNo).ThenBy(j => j.JobId).ToListAsync(ct);
        foreach (var row in jobRows.Where(row => day.Jobs.All(j => j.JobCode != row.JobCode)))
        {
            var available = registry.ContainsKey(row.JobCode);
            day.Jobs.Add(new EodDayJob
            {
                JobCode = row.JobCode, SequenceNo = row.SequenceNo,
                StatusCode = row.IsEnabled && available ? Statuses.Pending : Statuses.Skipped,
                Summary = !available ? "Not in this version of the system." : !row.IsEnabled ? "Switched off." : null,
            });
        }
        await db.SaveChangesAsync(ct);
        var dayId = day.DayId;
        var parameters = (await db.Set<EodJobParameter>().AsNoTracking().ToListAsync(ct)).ToLookup(p => p.JobId);

        foreach (var row in jobRows)
        {
            var dayJob = await db.Set<EodDayJob>().FirstAsync(j => j.DayId == dayId && j.JobCode == row.JobCode, ct);
            if (dayJob.StatusCode is Statuses.Done or Statuses.Skipped) continue;
            if (!row.IsEnabled || !registry.TryGetValue(row.JobCode, out var job))
            {
                dayJob.StatusCode = Statuses.Skipped;
                dayJob.Summary = !row.IsEnabled ? "Switched off." : "Not in this version of the system.";
                await db.SaveChangesAsync(ct);
                continue;
            }

            dayJob.StatusCode = Statuses.Running;
            dayJob.Attempts++;
            dayJob.StartedAt = clock.UtcNow;
            dayJob.FinishedAt = null;
            dayJob.ErrorMessage = null;
            dayJob.RunBy = by;
            await db.SaveChangesAsync(ct);
            var dayJobId = dayJob.DayJobId;

            var saved = parameters[row.JobId].ToDictionary(p => p.ParameterCode, p => p.ParameterValue);
            var values = job.Parameters.ToDictionary(p => p.Code, p => saved.GetValueOrDefault(p.Code) ?? p.DefaultValue, StringComparer.Ordinal);
            EodJobOutcome? outcome = null;
            string? error = null;
            try
            {
                outcome = await job.RunAsync(new EodJobContext(date, values), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
#pragma warning disable CA1031 // a job's failure is recorded on its row and the other jobs still run
            catch (Exception ex)
#pragma warning restore CA1031
            {
                error = ex is BusinessRuleException or BusinessValidationException or ConflictException or NotFoundException
                    ? ex.Message : $"Unexpected error ({ex.GetType().Name}): {ex.Message}";
                if (db.CurrentTransaction is { } open) await open.RollbackAsync(ct);
            }

            db.DiscardChanges();
            var result = await db.Set<EodDayJob>().FirstAsync(j => j.DayJobId == dayJobId, ct);
            result.FinishedAt = clock.UtcNow;
            if (outcome is not null)
            {
                result.StatusCode = Statuses.Done;
                result.ItemsCount = outcome.ItemsCount;
                result.Summary = Cut(outcome.Summary);
            }
            else
            {
                result.StatusCode = Statuses.Failed;
                result.ErrorMessage = Cut(error);
            }
            await db.SaveChangesAsync(ct);
        }

        var d = await db.Set<EodDay>().Include(x => x.Jobs).FirstAsync(x => x.DayId == dayId, ct);
        if (d.Jobs.All(j => j.StatusCode is Statuses.Done or Statuses.Skipped))
        {
            d.StatusCode = Statuses.Done;
            d.FinishedAt = clock.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return d.StatusCode;
    }

    private static string? Cut(string? text) => text is { Length: > 2000 } ? text[..2000] : text;
}
