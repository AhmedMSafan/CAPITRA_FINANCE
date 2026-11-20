using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_INTERFACE_LINES: copy of FINOVA GL_JOURNAL_INTERFACE_LINES (APEX), all columns kept.
/// </summary>
public class GlJournalInterfaceLine : AuditableEntity
{
    public long InterfaceLineId { get; set; }

    public long InterfaceBatchId { get; set; }

    public long? AccountingEventId { get; set; }

    public string? SourceLineRef { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string SourceDocumentType { get; set; } = string.Empty;

    public string SourceDocumentId { get; set; } = string.Empty;

    public string? SourceDocumentNo { get; set; }

    public int LineNo { get; set; }

    public DateOnly JournalDate { get; set; }

    public DateOnly AccountingDate { get; set; }

    public string? CategoryCode { get; set; }

    public long? AccountCombinationId { get; set; }

    public string? CombinationCode { get; set; }

    public string EnteredCurrencyCode { get; set; } = string.Empty;

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    public string? AccountedCurrencyCode { get; set; }

    public decimal? AccountedDr { get; set; }

    public decimal? AccountedCr { get; set; }

    public string? RateTypeCode { get; set; }

    public DateOnly? ConversionDate { get; set; }

    public decimal? ConversionRate { get; set; }

    public string? LineDescription { get; set; }

    /// <summary>One of: NEW, VALID, INVALID, PROCESSED, ERROR.</summary>
    public string ValidationStatus { get; set; } = "NEW";

    public string? ValidationErrorCode { get; set; }

    public string? ValidationErrorMsg { get; set; }

    public long? GeneratedHeaderId { get; set; }

    public long? GeneratedLineId { get; set; }
}
