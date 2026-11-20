using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_INQUIRY_RESULTS: copy of FINOVA FNV_INQUIRY_RESULT (APEX), all columns kept.
/// </summary>
public class MtgInquiryResult : AuditableEntity
{
    public long InquiryId { get; set; }

    public long PortfolioId { get; set; }

    public string SubjectType { get; set; } = string.Empty;

    public long? SubjectId { get; set; }

    public string InquiryTypeCode { get; set; } = string.Empty;

    public DateOnly? InquiryDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? ResultCode { get; set; }

    public string? RiskLevelCode { get; set; }

    public string? ResultDetails { get; set; }
}
