using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>
/// FINOVA_FINANCEING FactoringLegalReviewCompletionHandler: ACCEPT on the LEGAL_REVIEW step creates the customer's
/// agreement-level credit limit (FCT_CREDIT_LIMITS) from the agreement. REJECT does nothing here. FINOVA's INSERT ... SELECT
/// is copied column by column; it refuses a second active AGREEMENT-level limit for the same agreement.
/// </summary>
public sealed class FctLegalReviewCompletionHandler(ICapitraDbContext db, IClock clock) : IWfeTaskCompletionHandler
{
    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.Action, "ACCEPT", StringComparison.OrdinalIgnoreCase)
        && string.Equals(context.StepCode, "LEGAL_REVIEW", StringComparison.OrdinalIgnoreCase)
        && (string.IsNullOrWhiteSpace(context.WorkflowCode) || context.WorkflowCode.StartsWith("FACTORING", StringComparison.OrdinalIgnoreCase));

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var variables = await db.Set<WfeInstanceVariable>().AsNoTracking()
            .Where(x => x.WorkflowInstanceId == context.WorkflowInstanceId)
            .ToListAsync(cancellationToken);
        string? Value(params string[] names) => names
            .Select(n => variables.FirstOrDefault(x => string.Equals(x.VariableName, n, StringComparison.OrdinalIgnoreCase))?.VariableValue)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (!long.TryParse(Value("AGREEMENT_ID", "AgreementId", "ENTITY_ID", "EntityId"), out var agreementId))
            throw new BusinessRuleException("LEGAL_REVIEW ACCEPT cannot create the credit limit because the workflow agreement/entity ID is missing.");

        var a = await db.Set<FctAgreement>().AsNoTracking().FirstOrDefaultAsync(x => x.AgreementId == agreementId, cancellationToken);
        var exists = await db.Set<FctCreditLimit>()
            .AnyAsync(x => x.AgreementId == agreementId && x.LimitLevel == "AGREEMENT" && x.IsActive, cancellationToken);
        if (a == null || exists)
            throw new BusinessRuleException("No credit limit was created. The agreement does not exist, or it already has an active agreement-level credit limit.");

        var limit = a.ApprovedLimit ?? a.FacilityLimit;
        var utilized = a.UtilizedAmount ?? 0;
        var today = clock.UtcNow.Date;
        var start = ToDateTime(a.ContractStartDate) ?? a.EffectiveAt ?? ToDateTime(a.SigningDate) ?? ToDateTime(a.ApprovalDate) ?? a.ApplicationAt ?? today;
        var expiry = ToDateTime(a.ContractEndDate) ?? a.MaturityAt ?? ToDateTime(a.TerminationDate) ?? ToDateTime(a.RenewalDate)
            ?? start.AddMonths(a.ApprovedTenorMth ?? a.TenorMonths ?? 12);
        var name = a.AgreementName ?? $"Credit Limit for Agreement {a.AgreementNumber}";
        var notes = string.Join('\n', new[]
        {
            a.CreditNotes is null ? null : "Credit: " + a.CreditNotes,
            a.RiskNotes is null ? null : "Risk: " + a.RiskNotes,
            a.LegalOpenionNotes is null ? null : "Legal: " + a.LegalOpenionNotes,
            a.AdditionalTermsConditions is null ? null : "Additional Terms: " + a.AdditionalTermsConditions,
        }.Where(x => x != null));
        var previousLimitId = a.PreviousAgreementId is { } previous
            ? await db.Set<FctAgreement>().AsNoTracking().Where(x => x.AgreementId == previous).Select(x => x.LimitId).FirstOrDefaultAsync(cancellationToken)
            : null;

        // the audit columns are filled by the save, as for every Capitra row
        db.Add(new FctCreditLimit
        {
            LimitNumber = Left("CL-" + a.AgreementNumber, 30),
            LimitRef = a.AgreementRef,
            LimitName = name,
            LimitNameAr = a.AgreementNameAr ?? name,
            LimitLevel = "AGREEMENT",
            ClientId = a.ClientId,
            AgreementId = a.AgreementId,
            LimitType = string.IsNullOrEmpty(a.FactoringType) ? "FACTORING" : a.FactoringType,
            LimitTypeAr = a.FactoringTypeAr ?? a.FactoringType,
            LimitCategory = a.AgreementType ?? a.ProductCode,
            LimitCategoryAr = a.AgreementTypeAr ?? a.AgreementType ?? a.ProductCode,
            CurrencyId = a.CurrencyId,
            RequestedAmount = a.RequiredLimit ?? a.FacilityLimit,
            RecommendedAmount = a.RecommendedLimt ?? a.ApprovedLimit ?? a.FacilityLimit,
            ApprovedAmount = limit,
            CurrentLimit = limit,
            UtilizedAmount = utilized,
            AvailableAmount = a.AvailableAmount ?? Math.Max(limit - utilized, 0),
            ExcessAmount = Math.Max(utilized - limit, 0),
            UtilizationPct = limit == 0 ? null : Math.Round(utilized / limit * 100, 4),
            SublimitAmount = a.SublimitAmount,
            RiskGrade = a.AmlRiskBand,
            RiskScore = a.CreditBureauScore,
            AdvanceRatePct = a.ApprovedAdvRate ?? a.RecommendedAdvRate ?? a.AdvanceRatePct,
            MaxAdvanceRatePct = a.MaxAdvanceRatePct ?? a.ApprovedAdvRate ?? a.AdvanceRatePct,
            ConcentrationLimitPct = a.MaxDebtorConcentrationPct,
            ApplicationAt = a.ApplicationAt,
            ApprovalDate = a.ApprovalDate,
            EffectiveAt = start,
            ExpiryAt = expiry,
            LastReviewDate = a.RrAssessmentDate ?? a.FsAssessmentDate ?? a.ApprovalDate,
            NextReviewDate = a.NextReviewDate,
            LastUtilizationDate = a.LastUtilizationDate,
            RequestedBy = string.IsNullOrEmpty(a.CreatedBy) ? context.User : a.CreatedBy,
            RecommendedBy = a.RelationshipManager ?? a.ApprovedBy,
            RecommendationDate = a.RiskMemoDate ?? a.CreditMemoDate ?? a.FsAssessmentDate,
            ApprovedBy = a.ApprovedBy,
            ApprovalAuthority = a.ApprovalAuthority,
            ApprovalAuthorityAr = a.ApprovalAuthorityAr,
            ApprovalReference = a.ApprovalReference,
            ApprovalNotes = Left(notes, 2000),
            LimitConditions = a.SpecialConditions,
            LimitConditionsAr = a.SpecialConditionsAr,
            StatusId = a.StatusId ?? 1,
            PreviousLimitId = previousLimitId,
            VersionNumber = a.VersionNumber ?? 1,
            Remarks = a.Notes,
            RemarksAr = a.NotesAr,
            IsActive = a.IsActive,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static DateTime? ToDateTime(DateOnly? value) => value?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private static string Left(string value, int length) => value.Length <= length ? value : value[..length];
}
