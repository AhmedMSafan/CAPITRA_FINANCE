using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

public sealed record FctCustomerDto(
    long ClientId, string ClientNumber, string ClientName, string ClientType, string? PhoneNumber, string? Email, bool IsActive);

public sealed record FctCustomerDetailsDto(
    long ClientId, string ClientNumber, string ClientName, string ClientNameAr, string ClientType, string? CountryName,
    string? CurrencyCode, string? RelationshipManager, string? PhoneNumber, string? Email);

public sealed record FctApplicationDto(
    long OfferId, string OfferRef, DateTime? OfferAt, decimal RequestedLimit, string? CurrencyCode, string FactoringType,
    string? StatusCode, string? AgreementNo, string? WorkflowStepName);

public sealed record FctAgreementDto(
    long AgreementId, string AgreementNumber, DateTime? ApplicationAt, decimal FacilityLimit, string? CurrencyCode, string FactoringType,
    string? StatusCode, string? WorkflowStepName);

/// <summary>FCT_CREDIT_LIMITS, as FINOVA's credit-limits/by-client returns it (with the currency code).</summary>
public sealed record FctCreditLimitDto(
    long CreditLimitId, string LimitNumber, string? LimitRef, string LimitName, string LimitLevel, string LimitType, string? LimitCategory,
    long CurrencyId, string? CurrencyCode, decimal ApprovedAmount, decimal CurrentLimit, decimal UtilizedAmount, decimal? AvailableAmount,
    DateTime EffectiveAt, DateTime ExpiryAt, bool IsActive);

/// <summary>
/// The Factoring home (customers) and the Factoring Customer Workspace, from FINOVA_FINANCEING FactoringHomePage and
/// FactoringCustomerWorkspace: customers are CRM_CLIENTS, applications FCT_OFFERS, agreements FCT_AGREEMENTS.
/// </summary>
public sealed class FctWorkspaceService(ICapitraDbContext db)
{
    public async Task<IReadOnlyList<FctCustomerDto>> ListCustomersAsync(CancellationToken cancellationToken) =>
        await db.Set<CrmClient>().AsNoTracking()
            .OrderBy(x => x.ClientNumber)
            .Select(x => new FctCustomerDto(x.ClientId, x.ClientNumber, x.ClientName, x.ClientType, x.PhoneNumber, x.Email, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<FctCustomerDetailsDto> GetCustomerAsync(long clientId, CancellationToken cancellationToken) =>
        await (from c in db.Set<CrmClient>().AsNoTracking()
               where c.ClientId == clientId
               join co in db.Set<CrmLkpCountry>() on c.CountryId equals co.CountryId into cos
               from co in cos.DefaultIfEmpty()
               join cu in db.Set<CrmLkpCurrency>() on c.BaseCurrencyId equals cu.CurrencyId into cus
               from cu in cus.DefaultIfEmpty()
               select new FctCustomerDetailsDto(c.ClientId, c.ClientNumber, c.ClientName, c.ClientNameAr, c.ClientType,
                   co == null ? null : co.CountryName, cu == null ? null : cu.CurrencyCode, c.RelationshipManager, c.PhoneNumber, c.Email))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Customer {clientId} was not found.");

    public async Task<IReadOnlyList<FctApplicationDto>> ListApplicationsAsync(long clientId, CancellationToken cancellationToken)
    {
        var offers = await (from o in db.Set<FctOffer>().AsNoTracking()
                            where o.ClientId == clientId
                            join cu in db.Set<CrmLkpCurrency>() on o.CurrencyId equals cu.CurrencyId into cus
                            from cu in cus.DefaultIfEmpty()
                            orderby o.OfferId descending
                            select new { o, CurrencyCode = cu == null ? null : cu.CurrencyCode })
            .ToListAsync(cancellationToken);

        // the workflow step of an application is the current step of the agreement generated from it (AGREEMENT_NO)
        var numbers = offers.Where(x => x.o.AgreementNo != null).Select(x => x.o.AgreementNo!).ToList();
        var agreements = await db.Set<FctAgreement>().AsNoTracking()
            .Where(x => numbers.Contains(x.AgreementNumber))
            .Select(x => new { x.AgreementNumber, x.InstanceId })
            .ToListAsync(cancellationToken);
        var steps = await CurrentStepsAsync(agreements.Select(x => x.InstanceId), cancellationToken);

        return [.. offers.Select(x =>
        {
            var instanceId = agreements.FirstOrDefault(a => string.Equals(a.AgreementNumber, x.o.AgreementNo, StringComparison.OrdinalIgnoreCase))?.InstanceId;
            return new FctApplicationDto(x.o.OfferId, x.o.OfferRef, x.o.OfferAt, x.o.RequestedLimit, x.CurrencyCode, x.o.FactoringType,
                x.o.StatusCode, x.o.AgreementNo, instanceId is { } id ? steps.GetValueOrDefault(id) : null);
        })];
    }

    public async Task<IReadOnlyList<FctAgreementDto>> ListAgreementsAsync(long clientId, CancellationToken cancellationToken)
    {
        var rows = await (from a in db.Set<FctAgreement>().AsNoTracking()
                          where a.ClientId == clientId
                          join cu in db.Set<CrmLkpCurrency>() on a.CurrencyId equals cu.CurrencyId into cus
                          from cu in cus.DefaultIfEmpty()
                          orderby a.AgreementId descending
                          select new { a, CurrencyCode = cu == null ? null : cu.CurrencyCode })
            .ToListAsync(cancellationToken);
        var steps = await CurrentStepsAsync(rows.Select(x => x.a.InstanceId), cancellationToken);

        return [.. rows.Select(x => new FctAgreementDto(x.a.AgreementId, x.a.AgreementNumber, x.a.ApplicationAt, x.a.FacilityLimit,
            x.CurrencyCode, x.a.FactoringType, x.a.StatusCode, steps.GetValueOrDefault(x.a.InstanceId)))];
    }

    /// <summary>The customer's credit limits, newest first; Legal Review ACCEPT creates the agreement-level one.</summary>
    public async Task<IReadOnlyList<FctCreditLimitDto>> ListCreditLimitsAsync(long clientId, CancellationToken cancellationToken) =>
        await (from x in db.Set<FctCreditLimit>().AsNoTracking()
               where x.ClientId == clientId
               join cu in db.Set<CrmLkpCurrency>() on x.CurrencyId equals cu.CurrencyId into cus
               from cu in cus.DefaultIfEmpty()
               orderby x.CreatedAt descending
               select new FctCreditLimitDto(x.CreditLimitId, x.LimitNumber, x.LimitRef, x.LimitName, x.LimitLevel, x.LimitType, x.LimitCategory,
                   x.CurrencyId, cu == null ? null : cu.CurrencyCode, x.ApprovedAmount, x.CurrentLimit, x.UtilizedAmount, x.AvailableAmount,
                   x.EffectiveAt, x.ExpiryAt, x.IsActive))
            .ToListAsync(cancellationToken);

    /// <summary>Per workflow instance, the name of its newest step that is not completed yet.</summary>
    private async Task<Dictionary<long, string?>> CurrentStepsAsync(IEnumerable<long> instanceIds, CancellationToken cancellationToken)
    {
        var ids = instanceIds.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var open = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(x => ids.Contains(x.WorkflowInstanceId) && x.StepStatus != WfeRuntime.Completed && x.ElementType != "startEvent")
            .Select(x => new { x.WorkflowInstanceId, x.StepInstanceId, x.StepName })
            .ToListAsync(cancellationToken);

        return open.GroupBy(x => x.WorkflowInstanceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StepInstanceId).First().StepName);
    }
}
