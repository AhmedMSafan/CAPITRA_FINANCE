using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>FINOVA AgreementReviewSummaryDto. FINOVA left ClientName empty until CRM_CLIENTS was linked; it is filled here.</summary>
public sealed record FctAgreementReviewSummaryDto(
    long AgreementId, string? AgreementNumber, long? ClientId, string? ClientName, string? CurrencyCode, decimal? FacilityLimit,
    decimal? ApprovedLimit, DateTime? ApplicationDate, DateTime? EffectiveDate, string? StatusCode);

/// <summary>The agreement data the factoring workflow step pages read (FINOVA FactoringAgreementReviewController).</summary>
public sealed class FctReviewService(ICapitraDbContext db)
{
    public async Task<FctAgreementReviewSummaryDto> ReviewSummaryAsync(long agreementId, CancellationToken cancellationToken) =>
        await (from a in db.Set<FctAgreement>().AsNoTracking()
               where a.AgreementId == agreementId
               join c in db.Set<CrmClient>() on a.ClientId equals c.ClientId into cs
               from c in cs.DefaultIfEmpty()
               join cu in db.Set<CrmLkpCurrency>() on a.CurrencyId equals cu.CurrencyId into cus
               from cu in cus.DefaultIfEmpty()
               select new FctAgreementReviewSummaryDto(
                   a.AgreementId, a.AgreementNumber, a.ClientId, c == null ? null : c.ClientName, cu == null ? null : cu.CurrencyCode,
                   a.FacilityLimit, a.ApprovedLimit, a.ApplicationAt, a.EffectiveAt, a.StatusCode))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Factoring agreement {agreementId} was not found.");
}
