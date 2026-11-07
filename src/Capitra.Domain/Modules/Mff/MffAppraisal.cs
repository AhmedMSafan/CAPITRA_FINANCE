using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPRAISALS: copy of FINOVA MFF_APPRAISALS (APEX), all columns kept.
/// </summary>
public class MffAppraisal : AuditableEntity
{
    public long AppraisalId { get; set; }

    public long ApplicationId { get; set; }

    public long PropertyId { get; set; }

    public long? AppraiserOrgId { get; set; }

    public string? AppraiserName { get; set; }

    public string? LicenseNo { get; set; }

    public DateOnly? RequestDate { get; set; }

    public DateOnly? ReportDate { get; set; }

    public decimal? MarketValue { get; set; }

    public decimal? ForcedSaleValue { get; set; }

    public decimal? AcceptedValue { get; set; }

    public long? ReportDocumentId { get; set; }

    public string AppraisalStatus { get; set; } = "PENDING";

    public string? Notes { get; set; }
}
