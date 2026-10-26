using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Periods;

/// <summary>
/// Calendars, fiscal years, accounting periods and the status of each period per ledger
/// (APEX App 120 page 30, GL_PERIOD_SETUP_PKG and GL_PERIOD_CONTROL_PKG).
/// </summary>
public sealed class GlAccountingPeriodService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser) : IGlAccountingPeriodService
{
    private const int HistoryShown = 8;

    public async Task<AccountingPeriodsPageDto> GetAsync(long? calendarId, CancellationToken cancellationToken)
    {
        var calendars = await db.Set<GlCalendar>().AsNoTracking().OrderBy(x => x.CalendarCode).ToListAsync(cancellationToken);

        var selected = calendars.FirstOrDefault(x => x.CalendarId == calendarId)
            ?? calendars.FirstOrDefault(x => x.IsActive)
            ?? calendars.FirstOrDefault();

        List<CalendarLedgerDto> ledgers = [];
        List<GlFiscalYearDto> years = [];
        List<GlAccountingPeriodDto> periods = [];

        if (selected is not null)
        {
            var id = selected.CalendarId;

            ledgers = await db.Set<GlLedger>().AsNoTracking()
                .Where(x => x.CalendarId == id).OrderBy(x => x.LedgerCode)
                .Select(x => new CalendarLedgerDto(x.LedgerId, x.LedgerCode, x.LedgerNameEn))
                .ToListAsync(cancellationToken);

            years = await (
                from y in db.Set<GlFiscalYear>().AsNoTracking()
                where y.CalendarId == id
                orderby y.FiscalYear descending
                select new GlFiscalYearDto(y.FiscalYearId, y.FiscalYear, y.YearName, y.StartDate, y.EndDate, y.YearStatus,
                    db.Set<GlAccountingPeriod>().Count(p => p.FiscalYearId == y.FiscalYearId), y.RowVersion)
            ).ToListAsync(cancellationToken);

            var statuses = (await (
                    from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
                    join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
                    where l.CalendarId == id
                    orderby l.LedgerCode
                    select new { lp.PeriodId, Status = new PeriodLedgerStatusDto(lp.LedgerPeriodId, l.LedgerCode, lp.PeriodStatus, lp.RowVersion) }
                ).ToListAsync(cancellationToken))
                .ToLookup(x => x.PeriodId, x => x.Status);

            periods = (await db.Set<GlAccountingPeriod>().AsNoTracking()
                    .Where(x => x.CalendarId == id)
                    .OrderBy(x => x.FiscalYearId).ThenBy(x => x.PeriodNo).ThenBy(x => x.StartDate)
                    .ToListAsync(cancellationToken))
                .Select(p => ToDto(p, statuses[p.PeriodId].ToList()))
                .ToList();
        }

        return new AccountingPeriodsPageDto(
            selected?.CalendarId,
            GlRules.Today(clock),
            calendars.Select(ToDto).ToList(),
            ledgers,
            years,
            periods,
            new AccountingPeriodListsDto(GlCodes.CalendarTypes, GlCodes.PeriodFrequencies, GlCodes.YearStatuses, GlCodes.DefaultPeriodStatuses));
    }

    // ---------------------------------------------------------------- calendars

    public async Task<GlCalendarDto> CreateCalendarAsync(SaveGlCalendarRequest request, CancellationToken cancellationToken)
    {
        var entity = new GlCalendar();
        await ApplyAsync(entity, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<GlCalendarDto> UpdateCalendarAsync(long calendarId, SaveGlCalendarRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlCalendar>().FirstOrDefaultAsync(x => x.CalendarId == calendarId, cancellationToken)
            ?? throw new NotFoundException("Calendar not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This calendar was changed by someone else. Reload the page and try again.");

        await ApplyAsync(entity, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    private async Task ApplyAsync(GlCalendar entity, SaveGlCalendarRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        // The code identifies the calendar and cannot be changed.
        if (isNew)
        {
            var code = errors.Text("calendarCode", "Calendar code", GlRules.Code(r.CalendarCode), 30, required: true);
            if (code is not null && await db.Set<GlCalendar>().AnyAsync(x => x.CalendarCode == code, cancellationToken))
                errors.Add("calendarCode", $"Calendar {code} already exists.");
            entity.CalendarCode = code ?? string.Empty;
        }

        var nameEn = errors.Text("calendarNameEn", "Name (English)", r.CalendarNameEn, 150, required: true);
        var nameAr = errors.Text("calendarNameAr", "Name (Arabic)", r.CalendarNameAr, 150);

        var type = GlRules.Code(r.CalendarType) ?? GlCodes.Monthly;
        if (!GlCodes.CalendarTypes.Contains(type))
            errors.Add("calendarType", $"Unknown calendar type {type}.");

        var frequency = GlRules.Code(r.PeriodFrequency) ?? GlCodes.Monthly;
        if (!GlCodes.PeriodFrequencies.Contains(frequency))
            errors.Add("periodFrequency", $"Unknown period frequency {frequency}.");

        errors.ThrowIfAny();

        entity.CalendarNameEn = nameEn!;
        entity.CalendarNameAr = nameAr;
        entity.CalendarType = type;
        entity.PeriodFrequency = frequency;
        entity.IsAdjustmentPeriodUsed = r.IsAdjustmentPeriodUsed;
        entity.IsActive = r.IsActive;
    }

    // ---------------------------------------------------------------- fiscal years

    public async Task<GenerateFiscalYearResultDto> GenerateYearAsync(long calendarId, GenerateFiscalYearRequest request, CancellationToken cancellationToken)
    {
        var calendar = await db.Set<GlCalendar>().AsNoTracking().FirstOrDefaultAsync(x => x.CalendarId == calendarId, cancellationToken)
            ?? throw new NotFoundException("Calendar not found.");

        var errors = new ValidationErrors();

        if (request.FiscalYear is not { } fiscalYear)
            errors.Add("fiscalYear", "Fiscal year is required.");
        else if (fiscalYear < 1990 || fiscalYear > 2200)
            errors.Add("fiscalYear", "Fiscal year must be between 1990 and 2200.");

        // Every period ends on the last day of its month, so a year that starts mid-month would leave gaps.
        if (request.StartDate is not { } start)
            errors.Add("startDate", "First day of the year is required.");
        else if (start.Day != 1)
            errors.Add("startDate", "The fiscal year must start on the first day of a month.");

        var yearStatus = GlRules.Code(request.YearStatus) ?? GlCodes.YearOpen;
        if (!GlCodes.YearStatuses.Contains(yearStatus))
            errors.Add("yearStatus", $"Unknown fiscal year status {yearStatus}.");

        errors.ThrowIfAny();

        var number = request.FiscalYear!.Value;
        var first = request.StartDate!.Value;
        var last = first.AddMonths(12).AddDays(-1);

        // Never regenerate a year: its periods and their ledger statuses would be reset.
        var year = await db.Set<GlFiscalYear>().FirstOrDefaultAsync(x => x.CalendarId == calendarId && x.FiscalYear == number, cancellationToken);
        if (year is not null)
        {
            var existing = await db.Set<GlAccountingPeriod>().CountAsync(x => x.FiscalYearId == year.FiscalYearId, cancellationToken);
            if (existing > 0)
                throw BusinessValidationException.For("fiscalYear",
                    $"Fiscal year {number} already has {existing} periods in calendar {calendar.CalendarCode}. Edit its periods instead.");
        }

        var overlap = await db.Set<GlFiscalYear>().AsNoTracking()
            .Where(x => x.CalendarId == calendarId && x.FiscalYear != number && x.StartDate <= last && x.EndDate >= first)
            .Select(x => (int?)x.FiscalYear).FirstOrDefaultAsync(cancellationToken);
        if (overlap is not null)
            throw BusinessValidationException.For("startDate",
                $"{Format(first)} to {Format(last)} overlaps fiscal year {overlap} of calendar {calendar.CalendarCode}.");

        var periods = new List<GlAccountingPeriod>();
        for (var i = 1; i <= 12; i++)
        {
            var periodStart = first.AddMonths(i - 1);
            periods.Add(new GlAccountingPeriod
            {
                CalendarId = calendarId,
                PeriodNo = i,
                PeriodCode = $"{periodStart.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant()}-{number}",
                PeriodNameEn = $"{periodStart.ToString("MMMM", CultureInfo.InvariantCulture)} {number}",
                StartDate = periodStart,
                EndDate = periodStart.AddMonths(1).AddDays(-1),
                QuarterNo = (i + 2) / 3,
            });
        }

        if (request.IncludeAdjustmentPeriod)
        {
            periods.Add(new GlAccountingPeriod
            {
                CalendarId = calendarId,
                PeriodNo = 13,
                PeriodCode = $"ADJ-{number}",
                PeriodNameEn = $"Adjustment Period {number}",
                StartDate = last,
                EndDate = last,
                QuarterNo = 4,
                IsAdjustmentPeriod = true,
            });
        }

        var codes = periods.Select(x => x.PeriodCode).ToList();
        var taken = await db.Set<GlAccountingPeriod>().AsNoTracking()
            .Where(x => x.CalendarId == calendarId && codes.Contains(x.PeriodCode))
            .Select(x => x.PeriodCode).ToListAsync(cancellationToken);
        if (taken.Count > 0)
            throw BusinessValidationException.For("fiscalYear", $"Period codes {string.Join(", ", taken)} already exist in calendar {calendar.CalendarCode}.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        if (year is null)
        {
            year = new GlFiscalYear { CalendarId = calendarId, FiscalYear = number };
            db.Add(year);
        }

        year.YearName ??= number.ToString(CultureInfo.InvariantCulture);
        year.StartDate = first;
        year.EndDate = last;
        year.YearStatus = yearStatus;
        await db.SaveChangesAsync(cancellationToken);

        foreach (var period in periods)
            period.FiscalYearId = year.FiscalYearId;
        db.Set<GlAccountingPeriod>().AddRange(periods);
        await db.SaveChangesAsync(cancellationToken);

        var message = $"Fiscal year {number} generated with {periods.Count} periods";
        if (request.CreateLedgerPeriods)
        {
            var (created, ledgers) = await CreateLedgerPeriodsAsync(year, cancellationToken);
            if (ledgers > 0)
                message += $"; {created} ledger periods created for {ledgers} ledger(s)";
        }

        await transaction.CommitAsync(cancellationToken);
        return new GenerateFiscalYearResultDto(year.FiscalYearId, message + ".");
    }

    public async Task<GlFiscalYearDto> UpdateYearAsync(long fiscalYearId, SaveGlFiscalYearRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlFiscalYear>().FirstOrDefaultAsync(x => x.FiscalYearId == fiscalYearId, cancellationToken)
            ?? throw new NotFoundException("Fiscal year not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This fiscal year was changed by someone else. Reload the page and try again.");

        var errors = new ValidationErrors();
        var name = errors.Text("yearName", "Name", request.YearName, 50);

        if (request.StartDate is null)
            errors.Add("startDate", "Start date is required.");
        if (request.EndDate is null)
            errors.Add("endDate", "End date is required.");
        else if (request.StartDate is { } s && request.EndDate < s)
            errors.Add("endDate", "The end date cannot be before the start date.");

        var status = GlRules.Code(request.YearStatus);
        if (status is null)
            errors.Add("yearStatus", "Status is required.");
        else if (!GlCodes.YearStatuses.Contains(status))
            errors.Add("yearStatus", $"Unknown fiscal year status {status}.");

        errors.ThrowIfAny();

        var start = request.StartDate!.Value;
        var end = request.EndDate!.Value;

        var outside = await db.Set<GlAccountingPeriod>()
            .CountAsync(x => x.FiscalYearId == fiscalYearId && (x.StartDate < start || x.EndDate > end), cancellationToken);
        if (outside > 0)
            throw BusinessValidationException.For("startDate", $"{outside} period(s) of this year would fall outside the new dates.");

        var overlap = await db.Set<GlFiscalYear>().AsNoTracking()
            .Where(x => x.CalendarId == entity.CalendarId && x.FiscalYearId != fiscalYearId && x.StartDate <= end && x.EndDate >= start)
            .Select(x => (int?)x.FiscalYear).FirstOrDefaultAsync(cancellationToken);
        if (overlap is not null)
            throw BusinessValidationException.For("startDate", $"The dates overlap fiscal year {overlap} of this calendar.");

        entity.YearName = name ?? entity.FiscalYear.ToString(CultureInfo.InvariantCulture);
        entity.StartDate = start;
        entity.EndDate = end;
        entity.YearStatus = status!;
        await db.SaveChangesAsync(cancellationToken);

        var count = await db.Set<GlAccountingPeriod>().CountAsync(x => x.FiscalYearId == fiscalYearId, cancellationToken);
        return new GlFiscalYearDto(entity.FiscalYearId, entity.FiscalYear, entity.YearName, entity.StartDate, entity.EndDate,
            entity.YearStatus, count, entity.RowVersion);
    }

    // ---------------------------------------------------------------- periods

    public async Task<GlAccountingPeriodDto> CreatePeriodAsync(long fiscalYearId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
        var year = await db.Set<GlFiscalYear>().AsNoTracking().FirstOrDefaultAsync(x => x.FiscalYearId == fiscalYearId, cancellationToken)
            ?? throw new NotFoundException("Fiscal year not found.");

        var entity = new GlAccountingPeriod { FiscalYearId = fiscalYearId, CalendarId = year.CalendarId };
        await ApplyAsync(entity, year, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity, []);
    }

    public async Task<GlAccountingPeriodDto> UpdatePeriodAsync(long periodId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlAccountingPeriod>().FirstOrDefaultAsync(x => x.PeriodId == periodId, cancellationToken)
            ?? throw new NotFoundException("Period not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This period was changed by someone else. Reload the page and try again.");

        var year = await db.Set<GlFiscalYear>().AsNoTracking().FirstAsync(x => x.FiscalYearId == entity.FiscalYearId, cancellationToken);
        await ApplyAsync(entity, year, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var statuses = await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
            where lp.PeriodId == periodId
            orderby l.LedgerCode
            select new PeriodLedgerStatusDto(lp.LedgerPeriodId, l.LedgerCode, lp.PeriodStatus, lp.RowVersion)
        ).ToListAsync(cancellationToken);
        return ToDto(entity, statuses);
    }

    private async Task ApplyAsync(GlAccountingPeriod entity, GlFiscalYear year, SaveGlAccountingPeriodRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        // The code identifies the period and cannot be changed.
        var code = GlRules.Code(r.PeriodCode);
        if (isNew)
        {
            code = errors.Text("periodCode", "Period code", code, 30, required: true);
            if (code is not null && await db.Set<GlAccountingPeriod>().AnyAsync(x => x.CalendarId == year.CalendarId && x.PeriodCode == code, cancellationToken))
                errors.Add("periodCode", $"Period code {code} already exists in this calendar.");
            entity.PeriodCode = code ?? string.Empty;
        }
        else if (code is not null && code != entity.PeriodCode)
        {
            errors.Add("periodCode", "The period code cannot be changed.");
        }

        if (r.PeriodNo is not { } no)
            errors.Add("periodNo", "Period number is required.");
        else if (no < 1 || no > 99)
            errors.Add("periodNo", "Period number must be between 1 and 99.");

        var nameEn = errors.Text("periodNameEn", "Name (English)", r.PeriodNameEn, 80, required: true);
        var nameAr = errors.Text("periodNameAr", "Name (Arabic)", r.PeriodNameAr, 80);

        if (r.StartDate is null)
            errors.Add("startDate", "Start date is required.");
        if (r.EndDate is null)
            errors.Add("endDate", "End date is required.");
        else if (r.StartDate is { } s && r.EndDate < s)
            errors.Add("endDate", "The end date cannot be before the start date.");

        if (r.StartDate is { } start && r.EndDate is { } end && end >= start && (start < year.StartDate || end > year.EndDate))
            errors.Add("startDate", $"The period must fall inside fiscal year {year.YearName ?? year.FiscalYear.ToString(CultureInfo.InvariantCulture)} ({Format(year.StartDate)} to {Format(year.EndDate)}).");

        if (r.QuarterNo is { } q && (q < 1 || q > 4))
            errors.Add("quarterNo", "Quarter must be between 1 and 4.");

        var status = GlRules.Code(r.DefaultPeriodStatus) ?? GlCodes.PeriodNeverOpened;
        if (!GlCodes.DefaultPeriodStatuses.Contains(status))
            errors.Add("defaultPeriodStatus", $"Unknown period status {status}.");

        errors.ThrowIfAny();

        // Regular periods of a calendar never overlap; an adjustment period may.
        if (!r.IsAdjustmentPeriod)
        {
            var periodStart = r.StartDate!.Value;
            var periodEnd = r.EndDate!.Value;
            var hit = await db.Set<GlAccountingPeriod>().AsNoTracking()
                .Where(x => x.CalendarId == year.CalendarId && !x.IsAdjustmentPeriod && x.PeriodId != entity.PeriodId
                            && x.StartDate <= periodEnd && x.EndDate >= periodStart)
                .OrderBy(x => x.StartDate).Select(x => x.PeriodCode).FirstOrDefaultAsync(cancellationToken);
            if (hit is not null)
                throw BusinessValidationException.For("startDate", $"The dates overlap period {hit}.");
        }

        entity.PeriodNo = r.PeriodNo!.Value;
        entity.PeriodNameEn = nameEn!;
        entity.PeriodNameAr = nameAr;
        entity.StartDate = r.StartDate!.Value;
        entity.EndDate = r.EndDate!.Value;
        entity.QuarterNo = r.QuarterNo;
        entity.IsAdjustmentPeriod = r.IsAdjustmentPeriod;
        entity.DefaultPeriodStatus = status;
    }

    // ---------------------------------------------------------------- ledger periods

    public async Task<SyncLedgerPeriodsResultDto> SyncLedgerPeriodsAsync(long fiscalYearId, CancellationToken cancellationToken)
    {
        var year = await db.Set<GlFiscalYear>().AsNoTracking().FirstOrDefaultAsync(x => x.FiscalYearId == fiscalYearId, cancellationToken)
            ?? throw new NotFoundException("Fiscal year not found.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var (created, ledgers) = await CreateLedgerPeriodsAsync(year, cancellationToken);
        if (ledgers == 0)
            throw BusinessValidationException.For("ledgers", "No ledger uses this calendar.");
        await transaction.CommitAsync(cancellationToken);

        return new SyncLedgerPeriodsResultDto(created, $"{created} ledger period(s) created for {ledgers} ledger(s).");
    }

    /// <summary>
    /// Creates the missing ledger periods of a year for every ledger on its calendar (SYNC_LEDGER_PERIODS).
    /// A new ledger period takes the period's default status; existing ones are left as they are.
    /// </summary>
    private async Task<(int Created, int Ledgers)> CreateLedgerPeriodsAsync(GlFiscalYear year, CancellationToken cancellationToken)
    {
        var ledgerIds = await db.Set<GlLedger>().Where(x => x.CalendarId == year.CalendarId).Select(x => x.LedgerId).ToListAsync(cancellationToken);
        var periods = await db.Set<GlAccountingPeriod>().AsNoTracking().Where(x => x.FiscalYearId == year.FiscalYearId).ToListAsync(cancellationToken);
        var periodIds = periods.Select(x => x.PeriodId).ToList();
        var existing = (await db.Set<GlLedgerPeriod>()
                .Where(x => periodIds.Contains(x.PeriodId))
                .Select(x => new { x.LedgerId, x.PeriodId }).ToListAsync(cancellationToken))
            .Select(x => (x.LedgerId, x.PeriodId)).ToHashSet();

        var created = new List<GlLedgerPeriod>();
        foreach (var ledgerId in ledgerIds)
        {
            foreach (var period in periods.Where(p => !existing.Contains((ledgerId, p.PeriodId))))
            {
                var status = period.DefaultPeriodStatus;
                var open = status == GlCodes.PeriodOpen;
                created.Add(new GlLedgerPeriod
                {
                    LedgerId = ledgerId,
                    PeriodId = period.PeriodId,
                    PeriodStatus = status,
                    PostingMode = status switch
                    {
                        GlCodes.PeriodOpen when period.IsAdjustmentPeriod => GlCodes.PostingAdjustmentOnly,
                        GlCodes.PeriodOpen => GlCodes.PostingNormal,
                        GlCodes.PeriodFutureEntry => GlCodes.PostingFutureOnly,
                        _ => GlCodes.PostingNone,
                    },
                    IsManualJournalAllowed = open,
                    IsSubledgerAllowed = open,
                    IsReversalAllowed = open,
                    IsAdjustmentAllowed = open && period.IsAdjustmentPeriod,
                });
            }
        }

        if (created.Count > 0)
        {
            db.Set<GlLedgerPeriod>().AddRange(created);
            await db.SaveChangesAsync(cancellationToken);

            db.Set<GlLedgerPeriodHistory>().AddRange(created.Select(x => new GlLedgerPeriodHistory
            {
                LedgerPeriodId = x.LedgerPeriodId,
                NewPeriodStatus = x.PeriodStatus,
                ActionCode = "CREATE",
                ActionReason = "Ledger period created from the accounting period definition.",
            }));
            await db.SaveChangesAsync(cancellationToken);
        }

        return (created.Count, ledgerIds.Count);
    }

    public async Task<LedgerPeriodInfoDto> GetLedgerPeriodAsync(long ledgerPeriodId, CancellationToken cancellationToken)
    {
        var row = await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where lp.LedgerPeriodId == ledgerPeriodId
            select new { LedgerPeriod = lp, l.LedgerCode, Period = p }
        ).FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Ledger period not found.");

        var history = await db.Set<GlLedgerPeriodHistory>().AsNoTracking()
            .Where(x => x.LedgerPeriodId == ledgerPeriodId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.LedgerPeriodHistoryId)
            .Take(HistoryShown)
            .Select(x => new LedgerPeriodHistoryDto(x.OldPeriodStatus, x.NewPeriodStatus, x.ActionCode, x.ActionReason, x.CreatedBy, x.CreatedAt))
            .ToListAsync(cancellationToken);

        var current = row.LedgerPeriod;
        var isOpen = current.PeriodStatus == GlCodes.PeriodOpen;

        // The close checklist and subledger source statuses come later; until then an open period has no blockers.
        return new LedgerPeriodInfoDto(
            current.LedgerPeriodId, row.LedgerCode, row.Period.PeriodCode, current.PeriodStatus, current.PostingMode,
            current.IsManualJournalAllowed, current.IsSubledgerAllowed, current.IsReversalAllowed, current.IsAdjustmentAllowed,
            current.OpenedBy, current.OpenedAt, current.ClosedBy, current.ClosedAt, current.PermanentlyClosedBy, current.PermanentlyClosedAt,
            current.CloseNotes,
            isOpen ? true : null,
            isOpen ? "The period is ready to close." : null,
            await NextAsync(current.LedgerId, row.Period, cancellationToken),
            history,
            current.RowVersion);
    }

    public async Task<LedgerPeriodActionResultDto> ApplyActionAsync(long ledgerPeriodId, LedgerPeriodActionRequest request, CancellationToken cancellationToken)
    {
        var lp = await db.Set<GlLedgerPeriod>().FirstOrDefaultAsync(x => x.LedgerPeriodId == ledgerPeriodId, cancellationToken)
            ?? throw new NotFoundException("Ledger period not found.");

        if (request.RowVersion != lp.RowVersion)
            throw new ConflictException("This ledger period was changed by someone else. Reload the page and try again.");

        var period = await db.Set<GlAccountingPeriod>().AsNoTracking().FirstAsync(x => x.PeriodId == lp.PeriodId, cancellationToken);
        var action = GlRules.Code(request.Action);
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        if (reason?.Length > 1000)
            throw BusinessValidationException.For("reason", "Reason is limited to 1000 characters.");

        var old = lp.PeriodStatus;
        var user = currentUser.UserName ?? "system";
        var now = clock.UtcNow;
        var adjustment = period.IsAdjustmentPeriod;
        string actionCode;

        switch (action)
        {
            case "OPEN":
                if (old == GlCodes.PeriodOpen)
                    throw Invalid("The period is already open.");
                if (old == GlCodes.PeriodPermanentlyClosed)
                    throw Invalid("A permanently closed period cannot be opened.");
                if (old == GlCodes.PeriodClosed)
                    throw Invalid("A closed period is opened again with Reopen, which needs a reason.");
                SetOpen(lp, adjustment, adjustmentAllowed: adjustment);
                lp.OpenedBy = user;
                lp.OpenedAt = now;
                lp.CloseNotes = null;
                actionCode = "OPEN";
                break;

            // Future entry is for a period that has never been opened; a closed period is reopened instead.
            case "SET_FUTURE":
                if (old != GlCodes.PeriodNeverOpened)
                    throw Invalid($"Only a period that was never opened can allow future entry. It is {Label(old)}.");
                lp.PeriodStatus = GlCodes.PeriodFutureEntry;
                lp.PostingMode = GlCodes.PostingFutureOnly;
                SetFlags(lp, false, false, false, false);
                lp.CloseNotes = reason;
                actionCode = "SET_FUTURE_ENTRY";
                break;

            case "CLOSE":
                if (old != GlCodes.PeriodOpen)
                    throw Invalid($"Only an open period can be closed. It is {Label(old)}.");
                lp.PeriodStatus = GlCodes.PeriodClosed;
                lp.PostingMode = GlCodes.PostingNone;
                SetFlags(lp, false, false, false, false);
                lp.ClosedBy = user;
                lp.ClosedAt = now;
                lp.CloseNotes = reason;
                actionCode = "CLOSE";
                break;

            case "REOPEN":
                if (reason is null)
                    throw BusinessValidationException.For("reason", "A reason is required to reopen a period.");
                if (old != GlCodes.PeriodClosed)
                    throw Invalid($"Only a closed period can be reopened. It is {Label(old)}.");
                SetOpen(lp, adjustment, adjustmentAllowed: true);
                lp.CloseNotes = reason;
                actionCode = "REOPEN";
                break;

            case "PERMANENT":
                if (reason is null)
                    throw BusinessValidationException.For("reason", "A reason is required to close a period permanently.");
                if (old != GlCodes.PeriodClosed)
                    throw Invalid($"Only a closed period can be closed permanently. It is {Label(old)}.");
                lp.PeriodStatus = GlCodes.PeriodPermanentlyClosed;
                lp.PostingMode = GlCodes.PostingNone;
                SetFlags(lp, false, false, false, false);
                lp.PermanentlyClosedBy = user;
                lp.PermanentlyClosedAt = now;
                lp.CloseNotes = reason;
                actionCode = "PERMANENT_CLOSE";
                break;

            default:
                throw BusinessValidationException.For("action", $"Unknown action {action}.");
        }

        db.Add(new GlLedgerPeriodHistory
        {
            LedgerPeriodId = lp.LedgerPeriodId,
            OldPeriodStatus = old,
            NewPeriodStatus = lp.PeriodStatus,
            ActionCode = actionCode,
            ActionReason = reason,
        });
        await db.SaveChangesAsync(cancellationToken);

        return new LedgerPeriodActionResultDto(lp.PeriodStatus, await NextAsync(lp.LedgerId, period, cancellationToken));
    }

    private static BusinessValidationException Invalid(string message) => BusinessValidationException.For("action", message);

    /// <summary>An open regular period takes every journal; an adjustment period takes adjustments and no subledger postings.</summary>
    private static void SetOpen(GlLedgerPeriod lp, bool adjustmentPeriod, bool adjustmentAllowed)
    {
        lp.PeriodStatus = GlCodes.PeriodOpen;
        lp.PostingMode = adjustmentPeriod ? GlCodes.PostingAdjustmentOnly : GlCodes.PostingNormal;
        SetFlags(lp, manual: true, subledger: !adjustmentPeriod, reversal: true, adjustment: adjustmentAllowed);
    }

    private static void SetFlags(GlLedgerPeriod lp, bool manual, bool subledger, bool reversal, bool adjustment)
    {
        lp.IsManualJournalAllowed = manual;
        lp.IsSubledgerAllowed = subledger;
        lp.IsReversalAllowed = reversal;
        lp.IsAdjustmentAllowed = adjustment;
    }

    private async Task<NextLedgerPeriodDto?> NextAsync(long ledgerId, GlAccountingPeriod period, CancellationToken cancellationToken) =>
        await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where lp.LedgerId == ledgerId && !p.IsAdjustmentPeriod && p.StartDate > period.EndDate
            orderby p.StartDate
            select new NextLedgerPeriodDto(lp.LedgerPeriodId, p.PeriodCode, lp.PeriodStatus, lp.RowVersion)
        ).FirstOrDefaultAsync(cancellationToken);

    private static string Label(string status) => status.Replace('_', ' ').ToLowerInvariant();

    private static string Format(DateOnly date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    private static GlCalendarDto ToDto(GlCalendar x) =>
        new(x.CalendarId, x.CalendarCode, x.CalendarNameEn, x.CalendarNameAr, x.CalendarType, x.PeriodFrequency,
            x.IsAdjustmentPeriodUsed, x.IsActive, x.RowVersion);

    private static GlAccountingPeriodDto ToDto(GlAccountingPeriod x, IReadOnlyList<PeriodLedgerStatusDto> ledgers) =>
        new(x.PeriodId, x.FiscalYearId, x.PeriodNo, x.PeriodCode, x.PeriodNameEn, x.PeriodNameAr, x.StartDate, x.EndDate,
            x.QuarterNo, x.IsAdjustmentPeriod, x.DefaultPeriodStatus, ledgers, x.RowVersion);
}
