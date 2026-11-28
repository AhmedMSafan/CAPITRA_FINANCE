using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_DUE_DILIGENCES: copy of FINOVA FNV_DUE_DILIGENCE (APEX), all columns kept.
/// </summary>
public class MtgDueDiligence : AuditableEntity
{
    public long DdId { get; set; }

    public long PortfolioId { get; set; }

    public string CheckTypeCode { get; set; } = string.Empty;

    public string? SubjectType { get; set; }

    public long? SubjectId { get; set; }

    public string? AssignedTo { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public string? ResultCode { get; set; }

    public string? RiskLevelCode { get; set; }

    public bool IsException { get; set; }

    public string? Findings { get; set; }

    public string? Recommendation { get; set; }
}
