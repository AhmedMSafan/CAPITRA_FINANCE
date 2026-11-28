using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CREDIT_STUDIES: copy of FINOVA MFF_CREDIT_STUDIES (APEX), all columns kept.
/// </summary>
public class MffCreditStudy : AuditableEntity
{
    public long CreditStudyId { get; set; }

    public long ApplicationId { get; set; }

    public string? AnalystUser { get; set; }

    public DateOnly StudyDate { get; set; }

    public decimal? VerifiedIncomeAmount { get; set; }

    public decimal? MonthlyObligationsAmount { get; set; }

    public decimal? DisposableIncomeAmount { get; set; }

    public decimal? ProposedFinanceAmount { get; set; }

    public int? ProposedTermMonths { get; set; }

    public decimal? ProposedRatePct { get; set; }

    public decimal? InstallmentIncomePct { get; set; }

    public decimal? LtvPct { get; set; }

    public string? RecommendationCode { get; set; }

    public string? Notes { get; set; }
}
