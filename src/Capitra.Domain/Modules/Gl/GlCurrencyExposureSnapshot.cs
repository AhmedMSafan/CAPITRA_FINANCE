using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CURRENCY_EXPOSURE_SNAPSHOTS: copy of FINOVA GL_CURRENCY_EXPOSURE_SNAPSHOTS (APEX), all columns kept.
/// </summary>
public class GlCurrencyExposureSnapshot : AuditableEntity
{
    public long ExposureSnapshotId { get; set; }

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public string SnapshotCode { get; set; } = string.Empty;

    public DateOnly SnapshotDate { get; set; }

    public long RateTypeId { get; set; }

    /// <summary>One of: DRAFT, GENERATED, APPROVED, CANCELLED.</summary>
    public string SnapshotStatus { get; set; } = "DRAFT";

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Notes { get; set; }
}
