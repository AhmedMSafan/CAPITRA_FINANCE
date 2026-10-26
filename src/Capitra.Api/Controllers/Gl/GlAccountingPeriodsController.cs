using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.Periods;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>Calendars, fiscal years, accounting periods and each ledger's period status (APEX App 120 page 30).</summary>
[ApiController]
[Route("api/gl/accounting-periods")]
[HasPermission(Permissions.Gl.SetupView)]
public sealed class GlAccountingPeriodsController(IGlAccountingPeriodService service) : ControllerBase
{
    [HttpGet]
    public Task<AccountingPeriodsPageDto> Get([FromQuery] long? calendarId, CancellationToken cancellationToken) =>
        service.GetAsync(calendarId, cancellationToken);

    [HttpPost("calendars")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlCalendarDto> CreateCalendar(SaveGlCalendarRequest request, CancellationToken cancellationToken) =>
        service.CreateCalendarAsync(request, cancellationToken);

    [HttpPut("calendars/{calendarId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlCalendarDto> UpdateCalendar(long calendarId, SaveGlCalendarRequest request, CancellationToken cancellationToken) =>
        service.UpdateCalendarAsync(calendarId, request, cancellationToken);

    [HttpPost("calendars/{calendarId:long}/fiscal-years")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GenerateFiscalYearResultDto> GenerateYear(long calendarId, GenerateFiscalYearRequest request, CancellationToken cancellationToken) =>
        service.GenerateYearAsync(calendarId, request, cancellationToken);

    [HttpPut("fiscal-years/{fiscalYearId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlFiscalYearDto> UpdateYear(long fiscalYearId, SaveGlFiscalYearRequest request, CancellationToken cancellationToken) =>
        service.UpdateYearAsync(fiscalYearId, request, cancellationToken);

    [HttpPost("fiscal-years/{fiscalYearId:long}/periods")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlAccountingPeriodDto> CreatePeriod(long fiscalYearId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken) =>
        service.CreatePeriodAsync(fiscalYearId, request, cancellationToken);

    [HttpPost("fiscal-years/{fiscalYearId:long}/ledger-periods")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<SyncLedgerPeriodsResultDto> SyncLedgerPeriods(long fiscalYearId, CancellationToken cancellationToken) =>
        service.SyncLedgerPeriodsAsync(fiscalYearId, cancellationToken);

    [HttpPut("periods/{periodId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlAccountingPeriodDto> UpdatePeriod(long periodId, SaveGlAccountingPeriodRequest request, CancellationToken cancellationToken) =>
        service.UpdatePeriodAsync(periodId, request, cancellationToken);

    [HttpGet("ledger-periods/{ledgerPeriodId:long}")]
    public Task<LedgerPeriodInfoDto> GetLedgerPeriod(long ledgerPeriodId, CancellationToken cancellationToken) =>
        service.GetLedgerPeriodAsync(ledgerPeriodId, cancellationToken);

    [HttpPost("ledger-periods/{ledgerPeriodId:long}/actions")]
    [HasPermission(Permissions.Gl.PeriodControl)]
    public Task<LedgerPeriodActionResultDto> ApplyAction(long ledgerPeriodId, LedgerPeriodActionRequest request, CancellationToken cancellationToken) =>
        service.ApplyActionAsync(ledgerPeriodId, request, cancellationToken);
}
