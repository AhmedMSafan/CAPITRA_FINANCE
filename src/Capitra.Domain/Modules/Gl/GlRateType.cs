using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RATE_TYPES: copy of FINOVA GL_RATE_TYPES (APEX), all columns kept.
/// </summary>
public class GlRateType : AuditableEntity
{
    public long RateTypeId { get; set; }

    public string RateTypeCode { get; set; } = string.Empty;

    public string RateTypeNameEn { get; set; } = string.Empty;

    public string? RateTypeNameAr { get; set; }

    /// <summary>One of: MANUAL, CENTRAL_BANK, BANK, IMPORT, API, SYSTEM.</summary>
    public string RateSourceCode { get; set; } = "MANUAL";

    /// <summary>One of: ACCOUNTING, TREASURY, REVALUATION, REPORTING, STATUTORY.</summary>
    public string RatePurpose { get; set; } = "ACCOUNTING";

    public bool IsDefault { get; set; }

    public bool IsManualEntryAllowed { get; set; } = true;

    public bool IsApprovalRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public int DisplaySeq { get; set; } = 10;

    public string? Notes { get; set; }
}
