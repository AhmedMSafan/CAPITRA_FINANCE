using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Fct;

/// <summary>The FACTORING_LIMIT_APPROVAL step pages (FINOVA Factoring*Controller): read needs FCT.VIEW, save needs FCT.EDIT.</summary>
[ApiController]
[Route("api/fct")]
public sealed class FctStepController(FctStepService steps) : ControllerBase
{
    [HttpGet("agreements/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctAgreementFormDto> Agreement(long agreementId, CancellationToken cancellationToken) => steps.AgreementAsync(agreementId, cancellationToken);

    [HttpPut("agreements/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctAgreementFormDto> UpdateAgreement(long agreementId, FctAgreementUpdateRequest request, CancellationToken cancellationToken) =>
        steps.UpdateAgreementAsync(agreementId, request, cancellationToken);

    [HttpGet("inquiry-field-report/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctInquiryFieldReportDto> InquiryFieldReport(long agreementId, CancellationToken cancellationToken) => steps.InquiryFieldReportAsync(agreementId, cancellationToken);

    [HttpPut("inquiry-field-report/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctInquiryFieldReportDto> SaveInquiryFieldReport(long agreementId, FctInquiryFieldReportRequest request, CancellationToken cancellationToken) =>
        steps.SaveInquiryFieldReportAsync(agreementId, request, cancellationToken);

    [HttpGet("credit-analysis/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctCreditAnalysisPageDto> CreditAnalysis(long agreementId, CancellationToken cancellationToken) => steps.CreditAnalysisAsync(agreementId, cancellationToken);

    [HttpPut("credit-analysis/{agreementId:long}/financial-analysis")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctFinancialAnalysisDto> SaveFinancialAnalysis(long agreementId, FctFinancialAnalysisDto request, CancellationToken cancellationToken) =>
        steps.SaveFinancialAnalysisAsync(agreementId, request, cancellationToken);

    [HttpPut("credit-analysis/{agreementId:long}/risk-rating")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctRiskRatingDto> SaveRiskRating(long agreementId, FctRiskRatingDto request, CancellationToken cancellationToken) =>
        steps.SaveRiskRatingAsync(agreementId, request, cancellationToken);

    [HttpPut("credit-analysis/{agreementId:long}/credit-memo")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctCreditMemoDto> SaveCreditMemo(long agreementId, FctCreditMemoDto request, CancellationToken cancellationToken) =>
        steps.SaveCreditMemoAsync(agreementId, request, cancellationToken);

    [HttpPut("credit-analysis/{agreementId:long}/save-progress")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctCreditAnalysisPageDto> SaveCreditAnalysisProgress(long agreementId, FctCreditAnalysisProgressRequest request, CancellationToken cancellationToken) =>
        steps.SaveCreditAnalysisProgressAsync(agreementId, request, cancellationToken);

    [HttpGet("risk-review/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctRiskReviewPageDto> RiskReview(long agreementId, CancellationToken cancellationToken) => steps.RiskReviewAsync(agreementId, cancellationToken);

    [HttpPut("risk-review/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctRiskReviewPageDto> SaveRiskReview(long agreementId, FctRiskReviewRequest request, CancellationToken cancellationToken) =>
        steps.SaveRiskReviewAsync(agreementId, request, cancellationToken);

    [HttpGet("committee-decision/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctCommitteeDecisionPageDto> CommitteeDecision(long agreementId, CancellationToken cancellationToken) => steps.CommitteeDecisionAsync(agreementId, cancellationToken);

    [HttpPut("committee-decision/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctCommitteeDecisionPageDto> SaveCommitteeDecision(long agreementId, FctCommitteeDecisionDto request, CancellationToken cancellationToken) =>
        steps.SaveCommitteeDecisionAsync(agreementId, request, cancellationToken);

    [HttpGet("contract-request/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctContractRequestPageDto> ContractRequest(long agreementId, CancellationToken cancellationToken) => steps.ContractRequestAsync(agreementId, cancellationToken);

    [HttpPut("contract-request/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctContractRequestPageDto> SaveContractRequest(long agreementId, FctContractRequestRequest request, CancellationToken cancellationToken) =>
        steps.SaveContractRequestAsync(agreementId, request, cancellationToken);

    [HttpGet("contract-generation/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctContractGenerationPageDto> ContractGeneration(long agreementId, CancellationToken cancellationToken) => steps.ContractGenerationAsync(agreementId, cancellationToken);

    [HttpPut("contract-generation/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctContractGenerationPageDto> SaveContractGeneration(long agreementId, FctContractDto request, CancellationToken cancellationToken) =>
        steps.SaveContractGenerationAsync(agreementId, request, cancellationToken);

    [HttpGet("legal-review/{agreementId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctLegalReviewPageDto> LegalReview(long agreementId, CancellationToken cancellationToken) => steps.LegalReviewAsync(agreementId, cancellationToken);

    [HttpPut("legal-review/{agreementId:long}")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctLegalReviewPageDto> SaveLegalReview(long agreementId, FctLegalReviewDto request, CancellationToken cancellationToken) =>
        steps.SaveLegalReviewAsync(agreementId, request, cancellationToken);
}
