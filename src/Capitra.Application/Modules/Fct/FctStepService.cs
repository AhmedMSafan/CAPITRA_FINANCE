using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>The agreement header the factoring step pages show (FINOVA Factoring*SummaryResult, one shape for all of them).</summary>
public sealed record FctStepSummaryDto(
    long AgreementId, string AgreementNumber, long ClientId, string? ClientName, long CurrencyId, string? CurrencyCode,
    decimal FacilityLimit, decimal? ApprovedLimit, DateTime? ApplicationDate, string? RelationshipManager, string? StatusCode, string? CurrentStep);

public sealed record FctLovOption(long Value, string Label);

// ---------------------------------------------------------------- Sales Review (FINOVA FactoringAgreementService.GetByIdAsync / UpdateAsync)

public sealed record FctAgreementFormDto(
    long AgreementId, string AgreementNumber, string? AgreementRef, DateTime? ApplicationDate, long ClientId, string FactoringType,
    string RecourseType, long CurrencyId, decimal FacilityLimit, decimal AdvanceRatePct, int? TenorMonths, string? Notes,
    string? StatusCode, string CreatedBy, IReadOnlyList<FctLovOption> Clients, IReadOnlyList<FctLovOption> Currencies);

public sealed record FctAgreementUpdateRequest(
    DateTime? ApplicationDate, long ClientId, string? FactoringType, string? RecourseType, long CurrencyId, decimal FacilityLimit,
    decimal AdvanceRatePct, int? TenorMonths, string? Notes);

// ---------------------------------------------------------------- Investigation (FINOVA FactoringInquiryFieldReportService)

public sealed record FctInquiryDetails(DateTime? InvestigationReportDate, decimal? CreditBureauScore, string? InvestigationNotes);

public sealed record FctAmlDetails(DateTime? AmlScreeningReportDate, string? AmlRiskBand, string? AmlScreeningResult, string? AmlScreeningNotes);

public sealed record FctLegalDetails(DateTime? LegalOpenionReportDate, string? LegalOpenionNotes);

public sealed record FctDebtorInquiryDto(
    long AgrmtDebtorId, long AgreementId, long? DebtorId, string? DebtorName, DateOnly? VisitDate, string? OperationalStatus,
    string? PaymentHistory, string? Assessment);

public sealed record FctInquiryFieldReportDto(
    FctStepSummaryDto Summary, FctInquiryDetails Inquiry, FctAmlDetails Aml, FctLegalDetails Legal, IReadOnlyList<FctDebtorInquiryDto> Debtors);

public sealed record FctInquiryFieldReportRequest(FctInquiryDetails Inquiry, FctAmlDetails Aml, FctLegalDetails Legal);

// ---------------------------------------------------------------- Credit Analysis (FINOVA FactoringCreditAnalysisService)

public sealed record FctFinancialAnalysisDto(long? FsAssessmentId, DateOnly? FsAssessmentDate, string? FsAssessmentResult, string? FsNotes);

public sealed record FctRiskRatingDto(long? RrAssessmentId, DateOnly? RrAssessmentDate, string? RrAssessmentResult, string? RrNotes);

public sealed record FctCreditMemoDto(
    DateOnly? CreditMemoDate, decimal? RecommendedLimt, decimal? RecommendedAdvRate, int? RecommendedTenorMth, string? CreditNotes);

public sealed record FctReferenceDocumentDto(string SourceType, long DocumentId, string? DocumentTitle, string? FileName, string? MimeType, DateTime CreationDate, string CreatedBy);

public sealed record FctCreditAnalysisPageDto(
    FctStepSummaryDto Summary, FctFinancialAnalysisDto FinancialAnalysis, FctRiskRatingDto RiskRating, FctCreditMemoDto CreditMemo,
    IReadOnlyList<FctReferenceDocumentDto> ReferenceDocuments);

public sealed record FctCreditAnalysisProgressRequest(FctFinancialAnalysisDto FinancialAnalysis, FctRiskRatingDto RiskRating, FctCreditMemoDto CreditMemo);

// ---------------------------------------------------------------- Risk Review (FINOVA FactoringRiskReviewService)

public sealed record FctRiskIndicatorsDto(decimal? CreditBureauScore, string? RecourseType, int? TenorMonths, string? RrAssessmentResult);

public sealed record FctRiskReviewDto(DateOnly? RiskMemoDate, string? RiskNotes, string? RiskDecision);

public sealed record FctRiskReviewPageDto(FctStepSummaryDto Summary, FctRiskIndicatorsDto Indicators, FctRiskReviewDto RiskReview);

public sealed record FctRiskReviewRequest(DateOnly? RiskMemoDate, string? RiskNotes);

// ---------------------------------------------------------------- Committee Decision (FINOVA FactoringCommitteeDecisionService)

public sealed record FctCommitteeDecisionDto(DateOnly? ApprovalDate, decimal? ApprovedLimit, decimal? ApprovedAdvRate, int? ApprovedTenorMth);

public sealed record FctCommitteeDecisionPageDto(FctStepSummaryDto Summary, FctCommitteeDecisionDto Decision);

// ---------------------------------------------------------------- Customer Approval (FINOVA FactoringContractRequestService)

public sealed record FctApprovedTermsDto(decimal? ApprovedLimit, decimal? ApprovedAdvRate, int? ApprovedTenorMth, decimal? EffectiveRatePct);

public sealed record FctContractRequestPageDto(FctStepSummaryDto Summary, FctApprovedTermsDto ApprovedTerms, string? NotesToOperations);

public sealed record FctContractRequestRequest(string? NotesToOperations);

// ---------------------------------------------------------------- Contract Generation (FINOVA FactoringContractGenerationService)

public sealed record FctContractDto(
    string? AgreementNumber, DateTime? EffectiveDate, int? TenorMonths, DateOnly? ContractStartDate, DateOnly? ContractEndDate,
    DateOnly? ConstractSignoffDate, string? AdditionalTermsConditions);

public sealed record FctContractGenerationPageDto(FctStepSummaryDto Summary, FctContractDto Contract, FctApprovedTermsDto ApprovedTerms);

// ---------------------------------------------------------------- Legal Review (FINOVA FactoringLegalReviewService)

public sealed record FctLegalReviewDto(DateTime? LegalReviewDate, string? LegalOpinionNotes);

public sealed record FctLegalReviewPageDto(FctStepSummaryDto Summary, FctLegalReviewDto LegalReview);

/// <summary>
/// The FACTORING_LIMIT_APPROVAL step pages after Operation Received, from FINOVA_FINANCEING: each reads the agreement and
/// updates its own FCT_AGREEMENTS columns. ACCEPT / REJECT stay on the workflow task.
/// </summary>
public sealed class FctStepService(ICapitraDbContext db)
{
    private static readonly HashSet<string> AmlRiskBands = new(StringComparer.OrdinalIgnoreCase) { "LOW", "MEDIUM", "HIGH" };

    // ---------------------------------------------------------------- Sales Review

    public async Task<FctAgreementFormDto> AgreementAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await db.Set<FctAgreement>().AsNoTracking().FirstOrDefaultAsync(x => x.AgreementId == agreementId, cancellationToken)
            ?? throw NotFound(agreementId);
        var clients = await db.Set<CrmClient>().AsNoTracking().OrderBy(x => x.ClientName)
            .Select(x => new FctLovOption(x.ClientId, x.ClientName + " - " + x.ClientNumber)).ToListAsync(cancellationToken);
        var currencies = await db.Set<CrmLkpCurrency>().AsNoTracking().OrderBy(x => x.CurrencyCode)
            .Select(x => new FctLovOption(x.CurrencyId, x.CurrencyCode)).ToListAsync(cancellationToken);
        return new FctAgreementFormDto(
            a.AgreementId, a.AgreementNumber, a.AgreementRef, a.ApplicationAt, a.ClientId, a.FactoringType, a.RecourseType, a.CurrencyId,
            a.FacilityLimit, a.AdvanceRatePct, a.TenorMonths, a.Notes, a.StatusCode, a.CreatedBy, clients, currencies);
    }

    public async Task<FctAgreementFormDto> UpdateAgreementAsync(long agreementId, FctAgreementUpdateRequest r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.ApplicationAt = r.ApplicationDate;
        a.ClientId = r.ClientId;
        a.FactoringType = r.FactoringType ?? string.Empty;
        a.RecourseType = r.RecourseType ?? string.Empty;
        a.CurrencyId = r.CurrencyId;
        a.FacilityLimit = r.FacilityLimit;
        a.AdvanceRatePct = r.AdvanceRatePct;
        a.TenorMonths = r.TenorMonths;
        a.Notes = r.Notes;
        await db.SaveChangesAsync(cancellationToken);
        return await AgreementAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Investigation

    public async Task<FctInquiryFieldReportDto> InquiryFieldReportAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        // FINOVA scopes the debtor rows to the agreement being reviewed (the APEX region had no predicate).
        var debtors = await db.Set<FctAgreementDebtor>().AsNoTracking()
            .Where(x => x.AgreementId == agreementId)
            .OrderBy(x => x.AgrmtDebtorId)
            .Select(x => new FctDebtorInquiryDto(
                x.AgrmtDebtorId, x.AgreementId, x.DebtorId, x.DebtorId.ToString(), x.NotificationDate, x.RiskGrade, x.PaymentTerms, x.Remarks))
            .ToListAsync(cancellationToken);
        // FINOVA also shows the client's latest CRM_CLIENT_DOCUMENTS row here; that table is not in Capitra yet.
        return new FctInquiryFieldReportDto(
            await SummaryAsync(a, cancellationToken),
            new FctInquiryDetails(a.InvestigationReportAt, a.CreditBureauScore, a.InvestigationNotes),
            new FctAmlDetails(a.AmlScreeningReportAt, a.AmlRiskBand, a.AmlScreeningResult, a.AmlScreeningNotes),
            new FctLegalDetails(a.LegalOpenionReportAt, a.LegalOpenionNotes),
            debtors);
    }

    public async Task<FctInquiryFieldReportDto> SaveInquiryFieldReportAsync(long agreementId, FctInquiryFieldReportRequest r, CancellationToken cancellationToken)
    {
        var band = Upper(r.Aml.AmlRiskBand);
        if (band != null && !AmlRiskBands.Contains(band))
            throw new BusinessRuleException("AML Risk Band must be LOW, MEDIUM or HIGH.");
        if (r.Inquiry.CreditBureauScore < 0)
            throw new BusinessRuleException("Credit Bureau Score cannot be negative.");

        // one business action: inquiry, AML and legal are saved together
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.InvestigationReportAt = r.Inquiry.InvestigationReportDate;
        a.CreditBureauScore = r.Inquiry.CreditBureauScore;
        a.InvestigationNotes = Clean(r.Inquiry.InvestigationNotes);
        a.AmlScreeningReportAt = r.Aml.AmlScreeningReportDate;
        a.AmlRiskBand = band;
        a.AmlScreeningResult = Clean(r.Aml.AmlScreeningResult);
        a.AmlScreeningNotes = Clean(r.Aml.AmlScreeningNotes);
        a.LegalOpenionReportAt = r.Legal.LegalOpenionReportDate;
        a.LegalOpenionNotes = Clean(r.Legal.LegalOpenionNotes);
        await db.SaveChangesAsync(cancellationToken);
        return await InquiryFieldReportAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Credit Analysis

    public async Task<FctCreditAnalysisPageDto> CreditAnalysisAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        // FINOVA lists the latest four of the agreement's and the client's documents; client documents are not in Capitra yet.
        var documents = await db.Set<FctAgreementDocument>().AsNoTracking()
            .Where(x => x.AgreementId == agreementId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Take(4)
            .Select(x => new FctReferenceDocumentDto("AGREEMENT", x.DocumentId, x.DocumentTitle, x.FileName, x.MimeType, x.CreatedAt, x.CreatedBy))
            .ToListAsync(cancellationToken);
        return new FctCreditAnalysisPageDto(await SummaryAsync(a, cancellationToken), Financial(a), Risk(a), Memo(a), documents);
    }

    public async Task<FctFinancialAnalysisDto> SaveFinancialAnalysisAsync(long agreementId, FctFinancialAnalysisDto r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        ApplyFinancial(a, r);
        await db.SaveChangesAsync(cancellationToken);
        return Financial(a);
    }

    public async Task<FctRiskRatingDto> SaveRiskRatingAsync(long agreementId, FctRiskRatingDto r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        ApplyRisk(a, r);
        await db.SaveChangesAsync(cancellationToken);
        return Risk(a);
    }

    public async Task<FctCreditMemoDto> SaveCreditMemoAsync(long agreementId, FctCreditMemoDto r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        ApplyMemo(a, r);
        await db.SaveChangesAsync(cancellationToken);
        return Memo(a);
    }

    public async Task<FctCreditAnalysisPageDto> SaveCreditAnalysisProgressAsync(long agreementId, FctCreditAnalysisProgressRequest r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        ApplyFinancial(a, r.FinancialAnalysis);
        ApplyRisk(a, r.RiskRating);
        ApplyMemo(a, r.CreditMemo);
        await db.SaveChangesAsync(cancellationToken);
        return await CreditAnalysisAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Risk Review

    public async Task<FctRiskReviewPageDto> RiskReviewAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        return new FctRiskReviewPageDto(
            await SummaryAsync(a, cancellationToken),
            new FctRiskIndicatorsDto(a.CreditBureauScore, a.RecourseType, a.TenorMonths, a.RrAssessmentResult),
            new FctRiskReviewDto(a.RiskMemoDate, a.RiskNotes, a.RiskDecision));
    }

    public async Task<FctRiskReviewPageDto> SaveRiskReviewAsync(long agreementId, FctRiskReviewRequest r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.RiskMemoDate = r.RiskMemoDate;
        a.RiskNotes = r.RiskNotes?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await RiskReviewAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Committee Decision

    public async Task<FctCommitteeDecisionPageDto> CommitteeDecisionAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        return new FctCommitteeDecisionPageDto(
            await SummaryAsync(a, cancellationToken), new FctCommitteeDecisionDto(a.ApprovalDate, a.ApprovedLimit, a.ApprovedAdvRate, a.ApprovedTenorMth));
    }

    public async Task<FctCommitteeDecisionPageDto> SaveCommitteeDecisionAsync(long agreementId, FctCommitteeDecisionDto r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.ApprovalDate = r.ApprovalDate;
        a.ApprovedLimit = r.ApprovedLimit;
        a.ApprovedAdvRate = r.ApprovedAdvRate;
        a.ApprovedTenorMth = r.ApprovedTenorMth;
        await db.SaveChangesAsync(cancellationToken);
        return await CommitteeDecisionAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Customer Approval (contract request)

    public async Task<FctContractRequestPageDto> ContractRequestAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        return new FctContractRequestPageDto(await SummaryAsync(a, cancellationToken), Terms(a), a.Notes);
    }

    public async Task<FctContractRequestPageDto> SaveContractRequestAsync(long agreementId, FctContractRequestRequest r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.Notes = r.NotesToOperations?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await ContractRequestAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Contract Generation

    public async Task<FctContractGenerationPageDto> ContractGenerationAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        return new FctContractGenerationPageDto(
            await SummaryAsync(a, cancellationToken),
            new FctContractDto(a.AgreementNumber, a.EffectiveAt, a.TenorMonths, a.ContractStartDate, a.ContractEndDate, a.ConstractSignoffDate,
                a.AdditionalTermsConditions),
            Terms(a));
    }

    public async Task<FctContractGenerationPageDto> SaveContractGenerationAsync(long agreementId, FctContractDto r, CancellationToken cancellationToken)
    {
        var a = await TrackedAsync(agreementId, cancellationToken);
        var number = r.AgreementNumber?.Trim();
        if (string.IsNullOrWhiteSpace(number))
            throw new BusinessRuleException("Agreement number is required.");
        if (await db.Set<FctAgreement>().AnyAsync(x => x.AgreementId != agreementId && x.AgreementNumber == number, cancellationToken))
            throw new BusinessRuleException("Agreement number already exists.");

        a.AgreementNumber = number;
        a.EffectiveAt = r.EffectiveDate;
        a.TenorMonths = r.TenorMonths;
        a.ContractStartDate = r.ContractStartDate;
        a.ContractEndDate = r.ContractEndDate;
        a.ConstractSignoffDate = r.ConstractSignoffDate;
        a.AdditionalTermsConditions = r.AdditionalTermsConditions?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await ContractGenerationAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- Legal Review

    public async Task<FctLegalReviewPageDto> LegalReviewAsync(long agreementId, CancellationToken cancellationToken)
    {
        var a = await ReadAsync(agreementId, cancellationToken);
        return new FctLegalReviewPageDto(await SummaryAsync(a, cancellationToken), new FctLegalReviewDto(a.LegalOpenionReportAt, a.LegalOpenionNotes));
    }

    public async Task<FctLegalReviewPageDto> SaveLegalReviewAsync(long agreementId, FctLegalReviewDto r, CancellationToken cancellationToken)
    {
        // FINOVA keeps the legal fields here, not the APEX prototype's ContractStartDate / ContractEndDate / ConstractSignoffDate.
        var a = await TrackedAsync(agreementId, cancellationToken);
        a.LegalOpenionReportAt = r.LegalReviewDate;
        a.LegalOpenionNotes = r.LegalOpinionNotes?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await LegalReviewAsync(agreementId, cancellationToken);
    }

    // ---------------------------------------------------------------- helpers

    private async Task<FctAgreement> ReadAsync(long agreementId, CancellationToken cancellationToken) =>
        await db.Set<FctAgreement>().AsNoTracking().FirstOrDefaultAsync(x => x.AgreementId == agreementId, cancellationToken)
        ?? throw NotFound(agreementId);

    private async Task<FctAgreement> TrackedAsync(long agreementId, CancellationToken cancellationToken) =>
        await db.Set<FctAgreement>().FirstOrDefaultAsync(x => x.AgreementId == agreementId, cancellationToken)
        ?? throw NotFound(agreementId);

    private static NotFoundException NotFound(long agreementId) => new($"Factoring agreement {agreementId} was not found.");

    private async Task<FctStepSummaryDto> SummaryAsync(FctAgreement a, CancellationToken cancellationToken)
    {
        var clientName = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == a.ClientId).Select(x => x.ClientName)
            .FirstOrDefaultAsync(cancellationToken);
        var currencyCode = await db.Set<CrmLkpCurrency>().AsNoTracking().Where(x => x.CurrencyId == a.CurrencyId).Select(x => x.CurrencyCode)
            .FirstOrDefaultAsync(cancellationToken);
        return new FctStepSummaryDto(a.AgreementId, a.AgreementNumber, a.ClientId, clientName, a.CurrencyId, currencyCode, a.FacilityLimit,
            a.ApprovedLimit, a.ApplicationAt, a.RelationshipManager, a.StatusCode, a.CurrentStep);
    }

    private static FctApprovedTermsDto Terms(FctAgreement a) => new(a.ApprovedLimit, a.ApprovedAdvRate, a.ApprovedTenorMth, a.EffectiveRatePct);

    private static FctFinancialAnalysisDto Financial(FctAgreement a) => new(a.FsAssessmentId, a.FsAssessmentDate, a.FsAssessmentResult, a.FsNotes);

    private static FctRiskRatingDto Risk(FctAgreement a) => new(a.RrAssessmentId, a.RrAssessmentDate, a.RrAssessmentResult, a.RrNotes);

    private static FctCreditMemoDto Memo(FctAgreement a) =>
        new(a.CreditMemoDate, a.RecommendedLimt, a.RecommendedAdvRate, a.RecommendedTenorMth, a.CreditNotes);

    private static void ApplyFinancial(FctAgreement a, FctFinancialAnalysisDto r)
    {
        a.FsAssessmentId = r.FsAssessmentId;
        a.FsAssessmentDate = r.FsAssessmentDate;
        a.FsAssessmentResult = Upper(r.FsAssessmentResult);
        a.FsNotes = Clean(r.FsNotes);
    }

    private static void ApplyRisk(FctAgreement a, FctRiskRatingDto r)
    {
        a.RrAssessmentId = r.RrAssessmentId;
        a.RrAssessmentDate = r.RrAssessmentDate;
        a.RrAssessmentResult = Clean(r.RrAssessmentResult);
        a.RrNotes = Clean(r.RrNotes);
    }

    private static void ApplyMemo(FctAgreement a, FctCreditMemoDto r)
    {
        a.CreditMemoDate = r.CreditMemoDate;
        a.RecommendedLimt = r.RecommendedLimt;
        a.RecommendedAdvRate = r.RecommendedAdvRate;
        a.RecommendedTenorMth = r.RecommendedTenorMth;
        a.CreditNotes = Clean(r.CreditNotes);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Upper(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
