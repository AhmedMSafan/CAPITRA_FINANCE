namespace Capitra.Application.Modules.Gl.Periods;

/// <summary>Everything the accounting periods page shows for one calendar (APEX App 120 page 30, process PER_LOAD).</summary>
public sealed record AccountingPeriodsPageDto(
    long? CalendarId,
    DateOnly Today,
    IReadOnlyList<GlCalendarDto> Calendars,
    IReadOnlyList<CalendarLedgerDto> Ledgers,
    IReadOnlyList<GlFiscalYearDto> Years,
    IReadOnlyList<GlAccountingPeriodDto> Periods,
    AccountingPeriodListsDto Lists);

public sealed record AccountingPeriodListsDto(
    IReadOnlyList<string> CalendarTypes,
    IReadOnlyList<string> PeriodFrequencies,
    IReadOnlyList<string> YearStatuses,
    IReadOnlyList<string> DefaultPeriodStatuses);

public sealed record GlCalendarDto(
    long CalendarId,
    string CalendarCode,
    string CalendarNameEn,
    string? CalendarNameAr,
    string CalendarType,
    string PeriodFrequency,
    bool IsAdjustmentPeriodUsed,
    bool IsActive,
    int RowVersion);

public sealed record CalendarLedgerDto(long LedgerId, string LedgerCode, string LedgerNameEn);

public sealed record GlFiscalYearDto(
    long FiscalYearId,
    int FiscalYear,
    string? YearName,
    DateOnly StartDate,
    DateOnly EndDate,
    string YearStatus,
    int PeriodCount,
    int RowVersion);

public sealed record GlAccountingPeriodDto(
    long PeriodId,
    long FiscalYearId,
    int PeriodNo,
    string PeriodCode,
    string PeriodNameEn,
    string? PeriodNameAr,
    DateOnly StartDate,
    DateOnly EndDate,
    int? QuarterNo,
    bool IsAdjustmentPeriod,
    string DefaultPeriodStatus,
    IReadOnlyList<PeriodLedgerStatusDto> Ledgers,
    int RowVersion);

/// <summary>The status of the period in one ledger.</summary>
public sealed record PeriodLedgerStatusDto(long LedgerPeriodId, string LedgerCode, string PeriodStatus, int RowVersion);

public sealed record SaveGlCalendarRequest(
    string? CalendarCode,
    string? CalendarNameEn,
    string? CalendarNameAr,
    string? CalendarType,
    string? PeriodFrequency,
    bool IsAdjustmentPeriodUsed,
    bool IsActive,
    int? RowVersion);

public sealed record GenerateFiscalYearRequest(
    int? FiscalYear,
    DateOnly? StartDate,
    bool IncludeAdjustmentPeriod,
    string? YearStatus,
    bool CreateLedgerPeriods);

public sealed record GenerateFiscalYearResultDto(long FiscalYearId, string Message);

public sealed record SaveGlFiscalYearRequest(string? YearName, DateOnly? StartDate, DateOnly? EndDate, string? YearStatus, int? RowVersion);

public sealed record SaveGlAccountingPeriodRequest(
    string? PeriodCode,
    int? PeriodNo,
    string? PeriodNameEn,
    string? PeriodNameAr,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? QuarterNo,
    bool IsAdjustmentPeriod,
    string? DefaultPeriodStatus,
    int? RowVersion);

public sealed record SyncLedgerPeriodsResultDto(int Created, string Message);

/// <summary>A ledger period with what the details dialog shows (process PER_LP_INFO).</summary>
public sealed record LedgerPeriodInfoDto(
    long LedgerPeriodId,
    string LedgerCode,
    string PeriodCode,
    string PeriodStatus,
    string PostingMode,
    bool IsManualJournalAllowed,
    bool IsSubledgerAllowed,
    bool IsReversalAllowed,
    bool IsAdjustmentAllowed,
    string? OpenedBy,
    DateTime? OpenedAt,
    string? ClosedBy,
    DateTime? ClosedAt,
    string? PermanentlyClosedBy,
    DateTime? PermanentlyClosedAt,
    string? CloseNotes,
    bool? CanClose,
    string? CanCloseMessage,
    NextLedgerPeriodDto? Next,
    IReadOnlyList<LedgerPeriodHistoryDto> History,
    int RowVersion);

/// <summary>The ledger's next regular period, offered for opening once this one is closed.</summary>
public sealed record NextLedgerPeriodDto(long LedgerPeriodId, string PeriodCode, string PeriodStatus, int RowVersion);

public sealed record LedgerPeriodHistoryDto(string? OldPeriodStatus, string NewPeriodStatus, string ActionCode, string? ActionReason, string ActionBy, DateTime ActionAt);

/// <summary>OPEN, SET_FUTURE, CLOSE, REOPEN or PERMANENT. A reason is required to reopen or close permanently.</summary>
public sealed record LedgerPeriodActionRequest(string? Action, string? Reason, int? RowVersion);

public sealed record LedgerPeriodActionResultDto(string PeriodStatus, NextLedgerPeriodDto? Next);

public interface IGlAccountingPeriodService
{
    /// <summary>The chosen calendar, or the first active one, with its years, periods and ledger statuses.</summary>
    Task<AccountingPeriodsPageDto> GetAsync(long? calendarId, CancellationToken cancellationToken);

    Task<GlCalendarDto> CreateCalendarAsync(SaveGlCalendarRequest request, CancellationToken cancellationToken);

    Task<GlCalendarDto> UpdateCalendarAsync(long calendarId, SaveGlCalendarRequest request, CancellationToken cancellationToken);

    /// <summary>Creates a fiscal year with twelve monthly periods and, if asked, an adjustment period and the ledger periods.</summary>
    Task<GenerateFiscalYearResultDto> GenerateYearAsync(long calendarId, GenerateFiscalYearRequest request, CancellationToken cancellationToken);

    Task<GlFiscalYearDto> UpdateYearAsync(long fiscalYearId, SaveGlFiscalYearRequest request, CancellationToken cancellationToken);

    Task<GlAccountingPeriodDto> CreatePeriodAsync(long fiscalYearId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken);

    Task<GlAccountingPeriodDto> UpdatePeriodAsync(long periodId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken);

    /// <summary>Creates the missing ledger periods of a fiscal year for every ledger on its calendar. Existing ones are not changed.</summary>
    Task<SyncLedgerPeriodsResultDto> SyncLedgerPeriodsAsync(long fiscalYearId, CancellationToken cancellationToken);

    Task<LedgerPeriodInfoDto> GetLedgerPeriodAsync(long ledgerPeriodId, CancellationToken cancellationToken);

    Task<LedgerPeriodActionResultDto> ApplyActionAsync(long ledgerPeriodId, LedgerPeriodActionRequest request, CancellationToken cancellationToken);
}
