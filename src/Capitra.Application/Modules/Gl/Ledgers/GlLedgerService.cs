using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Ledgers;

/// <summary>Ledgers of each legal entity (GL_CORE_SETUP_PKG.UPSERT_LEDGER). Codes never change once saved.</summary>
public sealed class GlLedgerService(ICapitraDbContext db, IClock clock) : IGlLedgerService
{
    private const int AccountCodeMaxLength = 100;

    public async Task<LedgersPageDto> GetAsync(CancellationToken cancellationToken)
    {
        var ledgers = await Query(db.Set<GlLedger>().OrderBy(x => x.LedgerCode)).ToListAsync(cancellationToken);

        var legalEntities = await db.Set<AdmLkpLegalEntity>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
            .Select(x => new LedgerOptionDto(null, x.Code, x.NameEn)).ToListAsync(cancellationToken);

        var currencies = await db.Set<AdmLkpCurrency>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
            .Select(x => new LedgerOptionDto(null, x.Code, x.NameEn)).ToListAsync(cancellationToken);

        var calendars = await db.Set<GlCalendar>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.CalendarCode)
            .Select(x => new LedgerOptionDto(x.CalendarId, x.CalendarCode, x.CalendarNameEn)).ToListAsync(cancellationToken);

        var structures = await db.Set<GlCoaStructure>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.CoaCode)
            .Select(x => new LedgerOptionDto(x.CoaId, x.CoaCode, x.CoaNameEn)).ToListAsync(cancellationToken);

        var accounts = await db.Set<GlAccount>().AsNoTracking()
            .Where(x => x.IsActive && x.IsPostingAllowed && !x.IsSummary)
            .OrderBy(x => x.CoaId).ThenBy(x => x.AccountCode)
            .Select(x => new LedgerAccountOptionDto(x.CoaId, x.AccountCode, x.AccountNameEn, x.AccountType)).ToListAsync(cancellationToken);

        return new LedgersPageDto(ledgers, legalEntities, currencies, calendars, structures, accounts, GlCodes.LedgerTypes, GlCodes.LedgerStatuses);
    }

    public async Task<GlLedgerDto> CreateAsync(SaveGlLedgerRequest request, CancellationToken cancellationToken)
    {
        var entity = new GlLedger();
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await ApplyAsync(entity, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await Query(db.Set<GlLedger>().Where(x => x.LedgerId == entity.LedgerId)).FirstAsync(cancellationToken);
    }

    public async Task<GlLedgerDto> UpdateAsync(long ledgerId, SaveGlLedgerRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlLedger>().FirstOrDefaultAsync(x => x.LedgerId == ledgerId, cancellationToken)
            ?? throw new NotFoundException("Ledger not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This ledger was changed by someone else. Reload the page and try again.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await ApplyAsync(entity, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await Query(db.Set<GlLedger>().Where(x => x.LedgerId == ledgerId)).FirstAsync(cancellationToken);
    }

    private async Task ApplyAsync(GlLedger entity, SaveGlLedgerRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        if (isNew)
        {
            var code = errors.Text("ledgerCode", "Ledger code", GlRules.Code(r.LedgerCode), 30, required: true);
            if (code is not null && await db.Set<GlLedger>().AnyAsync(x => x.LedgerCode == code, cancellationToken))
                errors.Add("ledgerCode", $"Ledger {code} already exists.");
            entity.LedgerCode = code ?? string.Empty;
        }

        var nameEn = errors.Text("ledgerNameEn", "Name (English)", r.LedgerNameEn, 150, required: true);
        var nameAr = errors.Text("ledgerNameAr", "Name (Arabic)", r.LedgerNameAr, 150);
        var notes = errors.Text("notes", "Notes", r.Notes, 1000);

        var legalEntity = GlRules.Code(r.LegalEntityCode);
        if (legalEntity is null)
            errors.Add("legalEntityCode", "Legal entity is required.");
        else if (!await db.Set<AdmLkpLegalEntity>().AnyAsync(x => x.Code == legalEntity && x.IsActive, cancellationToken))
            errors.Add("legalEntityCode", $"Legal entity {legalEntity} is not an active legal entity.");

        var currency = GlRules.Code(r.LedgerCurrencyCode);
        if (currency is null)
            errors.Add("ledgerCurrencyCode", "Currency is required.");
        else if (!await db.Set<AdmLkpCurrency>().AnyAsync(x => x.Code == currency && x.IsActive, cancellationToken))
            errors.Add("ledgerCurrencyCode", $"Currency {currency} is not an active currency.");

        var type = GlRules.Code(r.LedgerType) ?? GlCodes.LedgerPrimary;
        if (!GlCodes.LedgerTypes.Contains(type))
            errors.Add("ledgerType", $"Unknown ledger type {type}.");

        var status = GlRules.Code(r.LedgerStatus) ?? GlCodes.LedgerActive;
        if (!GlCodes.LedgerStatuses.Contains(status))
            errors.Add("ledgerStatus", $"Unknown ledger status {status}.");

        if (r.CalendarId is not { } calendarId)
            errors.Add("calendarId", "Calendar is required.");
        else if (!await db.Set<GlCalendar>().AnyAsync(x => x.CalendarId == calendarId, cancellationToken))
            errors.Add("calendarId", "Calendar not found.");
        else if (!isNew && calendarId != entity.CalendarId
                 && await db.Set<GlLedgerPeriod>().AnyAsync(x => x.LedgerId == entity.LedgerId, cancellationToken))
            errors.Add("calendarId", "This ledger already has periods on its calendar, so the calendar cannot be changed.");

        if (r.CoaId is { } coaId && !await db.Set<GlCoaStructure>().AnyAsync(x => x.CoaId == coaId, cancellationToken))
            errors.Add("coaId", "Account structure not found.");

        var start = r.StartDate ?? GlRules.Today(clock);
        if (r.EndDate is { } end && end < start)
            errors.Add("endDate", "The end date cannot be before the start date.");

        // Control accounts are posting accounts of the ledger's account structure.
        var accounts = new Dictionary<string, string?>
        {
            ["retainedEarningsAcctCode"] = GlRules.Code(r.RetainedEarningsAcctCode),
            ["suspenseAcctCode"] = GlRules.Code(r.SuspenseAcctCode),
            ["roundingDiffAcctCode"] = GlRules.Code(r.RoundingDiffAcctCode),
            ["fxGainAcctCode"] = GlRules.Code(r.FxGainAcctCode),
            ["fxLossAcctCode"] = GlRules.Code(r.FxLossAcctCode),
            ["openingBalanceAcctCode"] = GlRules.Code(r.OpeningBalanceAcctCode),
        };
        foreach (var (field, code) in accounts)
        {
            if (code is null)
                continue;
            if (code.Length > AccountCodeMaxLength)
                errors.Add(field, $"The account code is limited to {AccountCodeMaxLength} characters.");
            else if (r.CoaId is not { } coa)
                errors.Add(field, "Choose the account structure before its accounts.");
            else if (!await db.Set<GlAccount>().AnyAsync(
                         x => x.CoaId == coa && x.AccountCode == code && x.IsActive && x.IsPostingAllowed && !x.IsSummary, cancellationToken))
                errors.Add(field, $"Account {code} is not an active posting account of this structure.");
        }

        errors.ThrowIfAny();

        // One primary ledger per legal entity: making this one primary demotes the other.
        if (r.IsPrimary)
        {
            var others = await db.Set<GlLedger>()
                .Where(x => x.LegalEntityCode == legalEntity && x.IsPrimary && x.LedgerId != entity.LedgerId)
                .ToListAsync(cancellationToken);
            foreach (var other in others)
                other.IsPrimary = false;
        }

        entity.LegalEntityCode = legalEntity!;
        entity.LedgerNameEn = nameEn!;
        entity.LedgerNameAr = nameAr;
        entity.LedgerType = type;
        entity.LedgerCurrencyCode = currency!;
        entity.CalendarId = r.CalendarId!.Value;
        entity.CoaId = r.CoaId;
        entity.IsPrimary = r.IsPrimary;
        entity.LedgerStatus = status;
        entity.RetainedEarningsAcctCode = accounts["retainedEarningsAcctCode"];
        entity.SuspenseAcctCode = accounts["suspenseAcctCode"];
        entity.RoundingDiffAcctCode = accounts["roundingDiffAcctCode"];
        entity.FxGainAcctCode = accounts["fxGainAcctCode"];
        entity.FxLossAcctCode = accounts["fxLossAcctCode"];
        entity.OpeningBalanceAcctCode = accounts["openingBalanceAcctCode"];
        entity.StartDate = start;
        entity.EndDate = r.EndDate;
        entity.Notes = notes;
    }

    private IQueryable<GlLedgerDto> Query(IQueryable<GlLedger> ledgers) =>
        from l in ledgers.AsNoTracking()
        select new GlLedgerDto(
            l.LedgerId, l.LegalEntityCode, l.LedgerCode, l.LedgerNameEn, l.LedgerNameAr, l.LedgerType, l.LedgerCurrencyCode,
            l.CalendarId,
            db.Set<GlCalendar>().Where(c => c.CalendarId == l.CalendarId).Select(c => c.CalendarCode).First(),
            l.CoaId,
            db.Set<GlCoaStructure>().Where(s => s.CoaId == l.CoaId).Select(s => s.CoaCode).FirstOrDefault(),
            l.IsPrimary, l.LedgerStatus,
            l.RetainedEarningsAcctCode, l.SuspenseAcctCode, l.RoundingDiffAcctCode, l.FxGainAcctCode, l.FxLossAcctCode, l.OpeningBalanceAcctCode,
            l.StartDate, l.EndDate, l.Notes,
            db.Set<GlLedgerPeriod>().Count(x => x.LedgerId == l.LedgerId),
            l.RowVersion);
}
