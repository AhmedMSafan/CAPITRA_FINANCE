using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RATE_IMPORT_LINES: copy of FINOVA GL_RATE_IMPORT_LINES (APEX), all columns kept.
/// </summary>
public class GlRateImportLine : AuditableEntity
{
    public long ImportLineId { get; set; }

    public long ImportBatchId { get; set; }

    public int LineNo { get; set; }

    public string? RateTypeCode { get; set; }

    public string? FromCurrencyCode { get; set; }

    public string? ToCurrencyCode { get; set; }

    public DateOnly? ConversionDate { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal? InverseRate { get; set; }

    public string? SourceReference { get; set; }

    /// <summary>One of: NEW, VALID, ERROR, POSTED, SKIPPED.</summary>
    public string ValidationStatus { get; set; } = "NEW";

    public string? ValidationMessage { get; set; }

    public long? DailyRateId { get; set; }
}
