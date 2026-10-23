using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_AML_RESULT: one AML check of the portfolio's developer or of one of its investors
/// (APEX FNV_AML_RESULTS, App 105 page 209). Checks are only ever added; the latest one per subject is its AML status.
/// </summary>
public class MtgAmlResult : AuditableEntity
{
    public const string DeveloperSubject = "DEVELOPER";
    public const string InvestorSubject = "INVESTOR";

    public static readonly IReadOnlyList<string> SubjectTypes = [DeveloperSubject, InvestorSubject];
    public static readonly IReadOnlyList<string> AmlTypes = ["AML_SCREENING", "PEP_SCREENING", "SANCTIONS_SCREENING", "ADVERSE_MEDIA"];
    public static readonly IReadOnlyList<string> Results = ["CLEAR", "POTENTIAL_MATCH", "CONFIRMED_MATCH", "REVIEW_REQUIRED"];
    public static readonly IReadOnlyList<string> RiskLevels = ["LOW", "MEDIUM", "HIGH", "CRITICAL"];

    public long AmlResultId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>DEVELOPER or INVESTOR.</summary>
    public string SubjectTypeCode { get; set; } = string.Empty;

    /// <summary>DEVELOPER_ID or INVESTOR_ID, depending on <see cref="SubjectTypeCode"/>.</summary>
    public long SubjectId { get; set; }

    public string AmlTypeCode { get; set; } = string.Empty;

    public DateOnly ScreeningDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string AmlResultCode { get; set; } = string.Empty;

    public string RiskLevelCode { get; set; } = string.Empty;

    /// <summary>0 to 100.</summary>
    public decimal? MatchScorePct { get; set; }

    public string? ResultDetails { get; set; }
}
