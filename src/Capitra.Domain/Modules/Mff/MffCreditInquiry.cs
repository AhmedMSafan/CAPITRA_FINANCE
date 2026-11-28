using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CREDIT_INQUIRIES: copy of FINOVA MFF_CREDIT_INQUIRIES (APEX), all columns kept.
/// </summary>
public class MffCreditInquiry : AuditableEntity
{
    public long InquiryId { get; set; }

    public long ApplicationId { get; set; }

    public long BureauOrgId { get; set; }

    public DateOnly RequestDate { get; set; }

    public DateOnly? ResponseDate { get; set; }

    public decimal? CreditScore { get; set; }

    public string? ResultCode { get; set; }

    public string? ReferenceNo { get; set; }

    public long? ReportDocumentId { get; set; }

    public string? Notes { get; set; }
}
