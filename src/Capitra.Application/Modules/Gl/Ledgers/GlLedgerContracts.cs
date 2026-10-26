namespace Capitra.Application.Modules.Gl.Ledgers;

/// <summary>The ledgers and the choices their form offers. APEX App 120 has no ledger page: its ledgers were set up by script (GL_CORE_SETUP_PKG).</summary>
public sealed record LedgersPageDto(
    IReadOnlyList<GlLedgerDto> Ledgers,
    IReadOnlyList<LedgerOptionDto> LegalEntities,
    IReadOnlyList<LedgerOptionDto> Currencies,
    IReadOnlyList<LedgerOptionDto> Calendars,
    IReadOnlyList<LedgerOptionDto> Structures,
    IReadOnlyList<LedgerAccountOptionDto> Accounts,
    IReadOnlyList<string> LedgerTypes,
    IReadOnlyList<string> LedgerStatuses);

/// <summary>Id is set for calendars and structures; Code is what the ledger stores for lookups.</summary>
public sealed record LedgerOptionDto(long? Id, string Code, string Name);

/// <summary>A posting account a ledger can use as a control account.</summary>
public sealed record LedgerAccountOptionDto(long CoaId, string AccountCode, string AccountNameEn, string AccountType);

public sealed record GlLedgerDto(
    long LedgerId,
    string LegalEntityCode,
    string LedgerCode,
    string LedgerNameEn,
    string? LedgerNameAr,
    string LedgerType,
    string LedgerCurrencyCode,
    long CalendarId,
    string CalendarCode,
    long? CoaId,
    string? CoaCode,
    bool IsPrimary,
    string LedgerStatus,
    string? RetainedEarningsAcctCode,
    string? SuspenseAcctCode,
    string? RoundingDiffAcctCode,
    string? FxGainAcctCode,
    string? FxLossAcctCode,
    string? OpeningBalanceAcctCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    int PeriodCount,
    int RowVersion);

public sealed record SaveGlLedgerRequest(
    string? LegalEntityCode,
    string? LedgerCode,
    string? LedgerNameEn,
    string? LedgerNameAr,
    string? LedgerType,
    string? LedgerCurrencyCode,
    long? CalendarId,
    long? CoaId,
    bool IsPrimary,
    string? LedgerStatus,
    string? RetainedEarningsAcctCode,
    string? SuspenseAcctCode,
    string? RoundingDiffAcctCode,
    string? FxGainAcctCode,
    string? FxLossAcctCode,
    string? OpeningBalanceAcctCode,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Notes,
    int? RowVersion);

public interface IGlLedgerService
{
    Task<LedgersPageDto> GetAsync(CancellationToken cancellationToken);

    Task<GlLedgerDto> CreateAsync(SaveGlLedgerRequest request, CancellationToken cancellationToken);

    Task<GlLedgerDto> UpdateAsync(long ledgerId, SaveGlLedgerRequest request, CancellationToken cancellationToken);
}
