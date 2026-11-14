using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Rates;

/// <summary>A rate index with the rate in force today.</summary>
public sealed record RateIndexDto(
    long RateIndexId, string RateIndexCode, string RateIndexName, long? CurrencyId, string? CurrencyCode, bool IsActive, decimal? CurrentRatePct,
    int PeriodCount, int RowVersion);

public sealed record SaveRateIndexRequest(string? RateIndexCode, string? RateIndexName, long? CurrencyId, bool IsActive, int? RowVersion);

/// <summary>
/// A period of a rate index: it starts on its start date and ends the day before the next period starts (the latest one stays open).
/// IsCurrent: today is in it; IsPast: it ended before today and can no longer change.
/// </summary>
public sealed record RatePeriodDto(
    long RateValueId, DateOnly StartDate, DateOnly? EndDate, decimal AnnualRatePct, bool IsCurrent, bool IsPast, string ChangedBy, DateTime ChangedAt,
    int RowVersion);

public sealed record SaveRatePeriodRequest(DateOnly? StartDate, decimal? AnnualRatePct, int? RowVersion);

public sealed record RateCurrencyDto(long CurrencyId, string CurrencyCode);

/// <summary>
/// Admin Setup rate maintenance: ADM_RATE_INDEXES and their periods in ADM_RATE_INDEX_VALUES (Ahmed, 30 Oct 2026). A period has only a
/// start date and a rate between 0 and 100, so periods never overlap or leave gaps; EFFECTIVE_TO_DATE is kept as the day before the
/// next period's start for the readers of the table. A period that ended before today is history and cannot be changed, nor can a
/// change move its end.
/// </summary>
public sealed class AdmRateIndexService(ICapitraDbContext db, IClock clock)
{
    public async Task<IReadOnlyList<RateIndexDto>> ListAsync(CancellationToken cancellationToken)
    {
        var today = GlTime.Today(clock);
        var indexes = await db.Set<AdmRateIndex>().AsNoTracking().OrderBy(x => x.RateIndexName).ToListAsync(cancellationToken);
        var currencies = await db.Set<CrmLkpCurrency>().AsNoTracking().ToDictionaryAsync(x => x.CurrencyId, x => x.CurrencyCode, cancellationToken);
        var values = await db.Set<AdmRateIndexValue>().AsNoTracking().ToListAsync(cancellationToken);
        return [.. indexes.Select(i =>
        {
            var own = values.Where(v => v.RateIndexId == i.RateIndexId).ToList();
            var current = own.Where(v => v.EffectiveFromDate <= today).MaxBy(v => v.EffectiveFromDate);
            return new RateIndexDto(i.RateIndexId, i.RateIndexCode, i.RateIndexName, i.CurrencyId,
                i.CurrencyId is { } c && currencies.TryGetValue(c, out var code) ? code : null, i.IsActive, current?.AnnualRatePct, own.Count, i.RowVersion);
        })];
    }

    public async Task<IReadOnlyList<RateCurrencyDto>> CurrenciesAsync(CancellationToken cancellationToken) =>
        await db.Set<CrmLkpCurrency>().AsNoTracking().OrderBy(x => x.CurrencyCode).Select(x => new RateCurrencyDto(x.CurrencyId, x.CurrencyCode))
            .ToListAsync(cancellationToken);

    public async Task<RateIndexDto> CreateAsync(SaveRateIndexRequest request, CancellationToken cancellationToken)
    {
        var code = await ValidateAsync(null, request, cancellationToken);
        var entity = new AdmRateIndex { RateIndexCode = code };
        Apply(entity, request);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (await ListAsync(cancellationToken)).First(x => x.RateIndexId == entity.RateIndexId);
    }

    public async Task<RateIndexDto> UpdateAsync(long id, SaveRateIndexRequest request, CancellationToken cancellationToken)
    {
        var entity = await IndexAsync(id, cancellationToken);
        if (request.RowVersion != entity.RowVersion) throw ChangedByAnotherUser();
        entity.RateIndexCode = await ValidateAsync(id, request, cancellationToken);
        Apply(entity, request);
        await SaveAsync(cancellationToken);
        return (await ListAsync(cancellationToken)).First(x => x.RateIndexId == id);
    }

    public async Task<IReadOnlyList<RatePeriodDto>> PeriodsAsync(long id, CancellationToken cancellationToken)
    {
        await IndexAsync(id, cancellationToken);
        var today = GlTime.Today(clock);
        var values = await db.Set<AdmRateIndexValue>().AsNoTracking().Where(x => x.RateIndexId == id).OrderByDescending(x => x.EffectiveFromDate)
            .ToListAsync(cancellationToken);
        return [.. values.Select(v => new RatePeriodDto(v.RateValueId, v.EffectiveFromDate, v.EffectiveToDate, v.AnnualRatePct,
            v.EffectiveFromDate <= today && (v.EffectiveToDate is null || v.EffectiveToDate >= today), v.EffectiveToDate < today,
            v.UpdatedBy ?? v.CreatedBy, v.UpdatedAt ?? v.CreatedAt, v.RowVersion))];
    }

    public async Task<IReadOnlyList<RatePeriodDto>> AddPeriodAsync(long id, SaveRatePeriodRequest request, CancellationToken cancellationToken)
    {
        await IndexAsync(id, cancellationToken);
        var (start, rate) = ValidatePeriod(request);
        var values = await ValuesAsync(id, cancellationToken);
        var ends = Ends(values);
        if (values.Any(x => x.EffectiveFromDate == start))
            throw BusinessValidationException.For(nameof(request.StartDate), $"A period already starts on {start:dd/MM/yyyy}.");
        var row = new AdmRateIndexValue { RateIndexId = id, EffectiveFromDate = start, AnnualRatePct = rate };
        values.Add(row);
        Recalculate(values, ends);
        db.Add(row);
        await SaveAsync(cancellationToken);
        return await PeriodsAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<RatePeriodDto>> UpdatePeriodAsync(long id, long valueId, SaveRatePeriodRequest request, CancellationToken cancellationToken)
    {
        var (start, rate) = ValidatePeriod(request);
        var values = await ValuesAsync(id, cancellationToken);
        var ends = Ends(values);
        var row = values.FirstOrDefault(x => x.RateValueId == valueId) ?? throw new NotFoundException($"Rate period {valueId} was not found.");
        if (request.RowVersion != row.RowVersion) throw ChangedByAnotherUser();
        if (row.EffectiveToDate < GlTime.Today(clock)) throw PastPeriod();
        if (values.Any(x => x.RateValueId != valueId && x.EffectiveFromDate == start))
            throw BusinessValidationException.For(nameof(request.StartDate), $"A period already starts on {start:dd/MM/yyyy}.");
        row.EffectiveFromDate = start;
        row.AnnualRatePct = rate;
        Recalculate(values, ends);
        await SaveAsync(cancellationToken);
        return await PeriodsAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<RatePeriodDto>> DeletePeriodAsync(long id, long valueId, int rowVersion, CancellationToken cancellationToken)
    {
        var values = await ValuesAsync(id, cancellationToken);
        var ends = Ends(values);
        var row = values.FirstOrDefault(x => x.RateValueId == valueId) ?? throw new NotFoundException($"Rate period {valueId} was not found.");
        if (rowVersion != row.RowVersion) throw ChangedByAnotherUser();
        if (row.EffectiveToDate < GlTime.Today(clock)) throw PastPeriod();
        values.Remove(row);
        Recalculate(values, ends);
        db.Remove(row);
        await SaveAsync(cancellationToken);
        return await PeriodsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Each period ends the day before the next one starts and the latest stays open. A period that had ended before today keeps its
    /// end: a change that would move it is refused.
    /// </summary>
    private void Recalculate(List<AdmRateIndexValue> values, Dictionary<long, DateOnly?> endsBefore)
    {
        var today = GlTime.Today(clock);
        var ordered = values.OrderBy(x => x.EffectiveFromDate).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var row = ordered[i];
            DateOnly? end = i + 1 < ordered.Count ? ordered[i + 1].EffectiveFromDate.AddDays(-1) : null;
            if (endsBefore.TryGetValue(row.RateValueId, out var before) && before < today && before != end)
                throw PastPeriod();
            row.EffectiveToDate = end;
        }
    }

    /// <summary>The ends of the periods as they were before the change.</summary>
    private static Dictionary<long, DateOnly?> Ends(List<AdmRateIndexValue> values) => values.ToDictionary(x => x.RateValueId, x => x.EffectiveToDate);

    private Task<List<AdmRateIndexValue>> ValuesAsync(long id, CancellationToken cancellationToken) =>
        db.Set<AdmRateIndexValue>().Where(x => x.RateIndexId == id).ToListAsync(cancellationToken);

    private async Task<AdmRateIndex> IndexAsync(long id, CancellationToken cancellationToken) =>
        await db.Set<AdmRateIndex>().FirstOrDefaultAsync(x => x.RateIndexId == id, cancellationToken)
            ?? throw new NotFoundException($"Rate index {id} was not found.");

    private async Task<string> ValidateAsync(long? id, SaveRateIndexRequest request, CancellationToken cancellationToken)
    {
        var code = (request.RateIndexCode ?? string.Empty).Trim().ToUpperInvariant();
        var errors = new Dictionary<string, string[]>();
        if (code.Length is 0 or > 50)
            errors[nameof(request.RateIndexCode)] = ["Code is required, up to 50 characters."];
        else if (await db.Set<AdmRateIndex>().AnyAsync(x => x.RateIndexCode == code && x.RateIndexId != id, cancellationToken))
            errors[nameof(request.RateIndexCode)] = [$"Rate index '{code}' already exists."];
        if (string.IsNullOrWhiteSpace(request.RateIndexName) || request.RateIndexName.Trim().Length > 200)
            errors[nameof(request.RateIndexName)] = ["Name is required, up to 200 characters."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);
        return code;
    }

    private static (DateOnly Start, decimal Rate) ValidatePeriod(SaveRatePeriodRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.StartDate is null) errors[nameof(request.StartDate)] = ["Start date is required."];
        if (request.AnnualRatePct is null or < 0 or > 100) errors[nameof(request.AnnualRatePct)] = ["Rate must be between 0 and 100."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);
        return (request.StartDate!.Value, Math.Round(request.AnnualRatePct!.Value, 6, MidpointRounding.AwayFromZero));
    }

    private static void Apply(AdmRateIndex entity, SaveRateIndexRequest request)
    {
        entity.RateIndexName = request.RateIndexName!.Trim();
        entity.CurrencyId = request.CurrencyId;
        entity.IsActive = request.IsActive;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser();
        }
    }

    private static BusinessRuleException PastPeriod() =>
        new("A period that ended before today is history and cannot be changed.");

    private static ConflictException ChangedByAnotherUser() => new("The rate was changed by another user. Reload and try again.");
}
