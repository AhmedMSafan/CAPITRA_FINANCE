using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_INQUIRY_RESULT: one inquiry made about the portfolio or one of its parties and what came back
/// (APEX FNV_INQUIRY_RESULT, App 105 page 210).
/// </summary>
public class MtgInquiryResult : AuditableEntity
{
    public const string PortfolioSubject = "PORTFOLIO";
    public const string DeveloperSubject = "DEVELOPER";
    public const string ProjectSubject = "PROJECT";
    public const string InvestorSubject = "INVESTOR";
    public const string SignatorySubject = "SIGNATORY";
    public const string GuaranteeSubject = "GUARANTEE";

    public static readonly IReadOnlyList<string> SubjectTypes =
        [PortfolioSubject, DeveloperSubject, ProjectSubject, InvestorSubject, SignatorySubject, GuaranteeSubject];

    /// <summary>Optional on an inquiry; the same levels as the AML checks.</summary>
    public static readonly IReadOnlyList<string> RiskLevels = MtgAmlResult.RiskLevels;

    public long InquiryResultId { get; set; }

    public long PortfolioId { get; set; }

    public string SubjectTypeCode { get; set; } = string.Empty;

    /// <summary>Id of the portfolio, developer, project, investor, signatory or guarantee, by <see cref="SubjectTypeCode"/>.</summary>
    public long SubjectId { get; set; }

    /// <summary>MTG_LKP_INQUIRY_TYPE code.</summary>
    public string InquiryTypeCode { get; set; } = string.Empty;

    public DateOnly? InquiryDate { get; set; }

    public string? ReferenceNo { get; set; }

    /// <summary>What came back, in a few words (e.g. "No adverse record"). Blank while the result is awaited.</summary>
    public string? ResultText { get; set; }

    public string? RiskLevelCode { get; set; }

    public string? ResultDetails { get; set; }
}
