using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_AML_RESULTS: copy of FINOVA FNV_AML_RESULTS (APEX), all columns kept.
/// </summary>
public class MtgAmlResult : AuditableEntity
{
    public long AmlId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>One of: DEVELOPER, INVESTOR.</summary>
    public string SubjectType { get; set; } = string.Empty;

    public long SubjectId { get; set; }

    public string AmlTypeCode { get; set; } = string.Empty;

    public DateOnly ScreeningDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string ResultCode { get; set; } = string.Empty;

    public string? RiskLevelCode { get; set; }

    public decimal? MatchScorePct { get; set; }

    public string? ResultDetails { get; set; }
}
