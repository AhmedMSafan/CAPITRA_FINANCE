using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_DAILY_RATES: copy of FINOVA GL_DAILY_RATES (APEX), all columns kept.
/// </summary>
public class GlDailyRate : AuditableEntity
{
    public long DailyRateId { get; set; }

    public long RateTypeId { get; set; }

    public string FromCurrencyCode { get; set; } = string.Empty;

    public string ToCurrencyCode { get; set; } = string.Empty;

    public DateOnly ConversionDate { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal? InverseRate { get; set; }

    /// <summary>One of: DRAFT, APPROVED, REJECTED, EXPIRED.</summary>
    public string RateStatus { get; set; } = "DRAFT";

    /// <summary>One of: MANUAL, CENTRAL_BANK, BANK, IMPORT, API, SYSTEM.</summary>
    public string SourceCode { get; set; } = "MANUAL";

    public string? SourceReference { get; set; }

    public string? TriangulationCurrency { get; set; }

    public decimal? TolerancePercent { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? Notes { get; set; }
}
