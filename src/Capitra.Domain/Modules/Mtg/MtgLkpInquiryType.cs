using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_INQUIRY_TYPES: copy of FINOVA REF_INQUIRY_TYPE (APEX), all columns kept.
/// </summary>
public class MtgLkpInquiryType : AuditableEntity
{
    public string InquiryTypeCode { get; set; } = string.Empty;

    public string InquiryNameAr { get; set; } = string.Empty;

    public string InquiryNameEn { get; set; } = string.Empty;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
