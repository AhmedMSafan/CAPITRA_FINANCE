using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Tls;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Tls;

public sealed record TlsScenarioRateRequest(DateOnly EffectiveDate, decimal InterestRatePct);

/// <summary>What the calculator page sends (the FINOVA workspace request), with the rate index the rate came from.</summary>
public sealed record TlsScenarioRequest(
    long? ScenarioId, string? ScenarioName, long? RateIndexId, decimal PrincipalAmount, DateOnly ContractDate, int TenorValue, string TenorUnit,
    decimal InterestRatePct, string DayCountMethod, string ScheduleMode, string? PrincipalMethod, string? InterestPaymentMethod,
    DateOnly PrincipalPaymentStartDate, DateOnly InterestPaymentStartDate,
    int? PrincipalFrequencyValue, string? PrincipalFrequencyUnit, string? PrincipalDateRule,
    int? InterestFrequencyValue, string? InterestFrequencyUnit, string? InterestDateRule,
    int? CombinedFrequencyValue, string? CombinedFrequencyUnit, string? CombinedDateRule,
    IReadOnlyList<TlsScenarioRateRequest>? Rates);

public sealed record TlsScenarioResult(
    long ScenarioId, string ScenarioName, long? RateIndexId, decimal PrincipalAmount, DateOnly ContractDate, DateOnly MaturityDate, int TenorValue,
    string TenorUnit, string ScheduleMode, string PrincipalMethod, string InterestPaymentMethod, string DayCountMethod,
    DateOnly PrincipalPaymentStartDate, DateOnly InterestPaymentStartDate,
    int? PrincipalFrequencyValue, string? PrincipalFrequencyUnit, string? PrincipalDateRule,
    int? InterestFrequencyValue, string? InterestFrequencyUnit, string? InterestDateRule,
    int? CombinedFrequencyValue, string? CombinedFrequencyUnit, string? CombinedDateRule,
    IReadOnlyList<TlsRate> Rates, IReadOnlyList<TlsLine> Schedule);

public sealed record TlsScenarioListItem(long ScenarioId, string ScenarioName);

/// <summary>A rate index of Administration > Rate Maintenance, with its rate valid on the date asked (null when none is).</summary>
public sealed record TlsRateOptionDto(long RateIndexId, string RateIndexCode, string RateIndexName, decimal? RatePct);

/// <summary>
/// Tools > Loan Calculator (the FINOVA_FINANCEING calculator; Ahmed, 15 Nov 2026). A user sees, opens and saves only the scenarios
/// they created. The rate starts as the one Administration > Rate Maintenance has valid on the contract date (from date A to date B
/// rate 1, and so on) and the user may change it. Preview calculates without saving; Save stores the terms, rates and schedule.
/// </summary>
public sealed class TlsLoanScenarioService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser)
{
    private string User => currentUser.UserName ?? "SYSTEM";

    public async Task<IReadOnlyList<TlsScenarioListItem>> ListAsync(CancellationToken cancellationToken) =>
        await db.Set<TlsLoanScenario>().AsNoTracking().Where(x => x.CreatedBy == User).OrderBy(x => x.ScenarioName)
            .Select(x => new TlsScenarioListItem(x.ScenarioId, x.ScenarioName)).ToListAsync(cancellationToken);

    public async Task<TlsScenarioResult> GetAsync(long id, CancellationToken cancellationToken)
    {
        var s = await Own(id, cancellationToken);
        var rates = await db.Set<TlsLoanScenarioRate>().AsNoTracking().Where(x => x.ScenarioId == id).OrderBy(x => x.EffectiveDate)
            .Select(x => new TlsRate(x.EffectiveDate, x.InterestRatePct, x.SequenceNo)).ToListAsync(cancellationToken);
        var lines = await db.Set<TlsLoanScenarioLine>().AsNoTracking().Where(x => x.ScenarioId == id).OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken);
        return Map(s, rates, lines.Select(x => new TlsLine(x.InstallmentNo, x.AccrualStartDate, x.AccrualEndDate, x.PaymentDate,
            x.OpeningPrincipalAmount, x.PrincipalAmount, x.InterestAmount, x.TotalPaymentAmount, x.ClosingPrincipalAmount, x.InterestRatePct,
            TlsLoanCalculator.AccrualDays(x.AccrualStartDate, x.AccrualEndDate, s.DayCountMethod), x.DayCountFraction)).ToList());
    }

    /// <summary>Calculates the schedule in memory; nothing is saved.</summary>
    public TlsScenarioResult Preview(TlsScenarioRequest request)
    {
        var terms = TlsLoanCalculator.Terms(request);
        var s = new TlsLoanScenario { ScenarioName = request.ScenarioName?.Trim() ?? "Working Scenario" };
        Apply(s, terms, request.RateIndexId);
        return Map(s, terms.Rates, TlsLoanCalculator.Generate(terms));
    }

    public async Task<TlsScenarioResult> SaveAsync(TlsScenarioRequest request, CancellationToken cancellationToken)
    {
        var name = (request.ScenarioName ?? string.Empty).Trim();
        if (name.Length == 0) throw new BusinessRuleException("Scenario name is required before saving.");
        if (name.Length > 200) throw new BusinessRuleException("Scenario name can be at most 200 characters.");
        var id = request.ScenarioId is > 0 ? request.ScenarioId : null;
        // names are unique per user, whatever the case (a user has few scenarios, so they are compared here)
        var names = await db.Set<TlsLoanScenario>().AsNoTracking().Where(x => x.CreatedBy == User && x.ScenarioId != id)
            .Select(x => x.ScenarioName).ToListAsync(cancellationToken);
        if (names.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException($"Scenario name '{name}' already exists.");
        if (request.RateIndexId is { } rateIndexId && !await db.Set<AdmRateIndex>().AnyAsync(x => x.RateIndexId == rateIndexId, cancellationToken))
            throw new BusinessRuleException($"Rate index {rateIndexId} was not found.");

        var terms = TlsLoanCalculator.Terms(request);
        var lines = TlsLoanCalculator.Generate(terms);

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        TlsLoanScenario s;
        if (id is { } existing)
        {
            s = await db.Set<TlsLoanScenario>().FirstOrDefaultAsync(x => x.ScenarioId == existing && x.CreatedBy == User, cancellationToken)
                ?? throw new NotFoundException("The selected scenario no longer exists.");
            // update: keep the scenario and replace its rates and schedule with what is on the screen now
            foreach (var r in await db.Set<TlsLoanScenarioRate>().Where(x => x.ScenarioId == existing).ToListAsync(cancellationToken)) db.Remove(r);
            foreach (var l in await db.Set<TlsLoanScenarioLine>().Where(x => x.ScenarioId == existing).ToListAsync(cancellationToken)) db.Remove(l);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            s = new TlsLoanScenario();
            db.Add(s);
        }
        s.ScenarioName = name;
        Apply(s, terms, request.RateIndexId);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var r in terms.Rates)
            db.Add(new TlsLoanScenarioRate { ScenarioId = s.ScenarioId, EffectiveDate = r.EffectiveDate, InterestRatePct = r.InterestRatePct, SequenceNo = r.SequenceNo });
        foreach (var l in lines)
            db.Add(new TlsLoanScenarioLine
            {
                ScenarioId = s.ScenarioId, InstallmentNo = l.InstallmentNo, AccrualStartDate = l.AccrualStartDate, AccrualEndDate = l.AccrualEndDate,
                PaymentDate = l.PaymentDate, OpeningPrincipalAmount = l.OpeningPrincipal, PrincipalAmount = l.PrincipalAmount,
                InterestAmount = l.InterestAmount, TotalPaymentAmount = l.TotalPaymentAmount, ClosingPrincipalAmount = l.ClosingPrincipal,
                InterestRatePct = l.InterestRatePct, DayCountFraction = Math.Round(l.DayCountFraction, 12),
            });
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Map(s, terms.Rates, lines);
    }

    /// <summary>The active rate indexes with the rate each has valid on the date (today when none is given).</summary>
    public async Task<IReadOnlyList<TlsRateOptionDto>> RatesAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var on = date ?? GlTime.Today(clock);
        var indexes = await db.Set<AdmRateIndex>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.RateIndexName).ToListAsync(cancellationToken);
        var values = await db.Set<AdmRateIndexValue>().AsNoTracking()
            .Where(v => v.EffectiveFromDate <= on && (v.EffectiveToDate == null || v.EffectiveToDate >= on)).ToListAsync(cancellationToken);
        return [.. indexes.Select(i => new TlsRateOptionDto(i.RateIndexId, i.RateIndexCode, i.RateIndexName,
            values.Where(v => v.RateIndexId == i.RateIndexId).MaxBy(v => v.EffectiveFromDate)?.AnnualRatePct))];
    }

    private async Task<TlsLoanScenario> Own(long id, CancellationToken cancellationToken) =>
        await db.Set<TlsLoanScenario>().AsNoTracking().FirstOrDefaultAsync(x => x.ScenarioId == id && x.CreatedBy == User, cancellationToken)
        ?? throw new NotFoundException($"Scenario {id} was not found.");

    private static void Apply(TlsLoanScenario s, TlsTerms t, long? rateIndexId)
    {
        s.RateIndexId = rateIndexId;
        s.PrincipalAmount = t.PrincipalAmount;
        s.ContractDate = t.ContractDate;
        s.MaturityDate = t.MaturityDate;
        s.TenorValue = t.TenorValue;
        s.TenorUnit = t.TenorUnit;
        s.ScheduleMode = t.ScheduleMode;
        s.PrincipalMethod = t.PrincipalMethod;
        s.InterestPaymentMethod = t.InterestPaymentMethod;
        s.DayCountMethod = t.DayCountMethod;
        s.PrincipalPaymentStartDate = t.PrincipalPaymentStartDate;
        s.InterestPaymentStartDate = t.InterestPaymentStartDate;
        s.PrincipalFrequencyValue = t.PrincipalFrequencyValue;
        s.PrincipalFrequencyUnit = t.PrincipalFrequencyUnit;
        s.PrincipalDateRule = t.PrincipalDateRule;
        s.InterestFrequencyValue = t.InterestFrequencyValue;
        s.InterestFrequencyUnit = t.InterestFrequencyUnit;
        s.InterestDateRule = t.InterestDateRule;
        s.CombinedFrequencyValue = t.CombinedFrequencyValue;
        s.CombinedFrequencyUnit = t.CombinedFrequencyUnit;
        s.CombinedDateRule = t.CombinedDateRule;
    }

    private static TlsScenarioResult Map(TlsLoanScenario s, IReadOnlyList<TlsRate> rates, List<TlsLine> lines) =>
        new(s.ScenarioId, s.ScenarioName, s.RateIndexId, s.PrincipalAmount, s.ContractDate, s.MaturityDate, s.TenorValue, s.TenorUnit, s.ScheduleMode,
            s.PrincipalMethod, s.InterestPaymentMethod, s.DayCountMethod, s.PrincipalPaymentStartDate, s.InterestPaymentStartDate,
            s.PrincipalFrequencyValue, s.PrincipalFrequencyUnit, s.PrincipalDateRule, s.InterestFrequencyValue, s.InterestFrequencyUnit, s.InterestDateRule,
            s.CombinedFrequencyValue, s.CombinedFrequencyUnit, s.CombinedDateRule, rates, lines);
}
