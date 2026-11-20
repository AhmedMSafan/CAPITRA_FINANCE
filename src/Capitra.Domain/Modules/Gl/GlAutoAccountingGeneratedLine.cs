using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_ACCOUNTING_GENERATED_LINES: copy of FINOVA GL_AUTO_ACCOUNTING_GENERATED_LINES (APEX), all columns kept.
/// </summary>
public class GlAutoAccountingGeneratedLine : AuditableEntity
{
    public long GeneratedLineId { get; set; }

    public long GenerationId { get; set; }

    public long RuleLineId { get; set; }

    public int LineNo { get; set; }

    /// <summary>One of: DR, CR.</summary>
    public string DrCrFlag { get; set; } = string.Empty;

    public long? AccountCombinationId { get; set; }

    public string? CombinationCode { get; set; }

    public long? AccountId { get; set; }

    public string EnteredCurrencyCode { get; set; } = string.Empty;

    public decimal EnteredAmount { get; set; }

    public string? AccountedCurrencyCode { get; set; }

    public decimal? AccountedAmount { get; set; }

    public long? RateTypeId { get; set; }

    public DateOnly? ConversionDate { get; set; }

    public decimal? ConversionRate { get; set; }

    public string? AmountCalculationTrace { get; set; }

    public string? AccountDerivationTrace { get; set; }

    public string? LineDescription { get; set; }

    /// <summary>One of: NEW, VALID, INVALID, INTERFACED, POSTED, ERROR.</summary>
    public string ValidationStatus { get; set; } = "NEW";

    public string? ValidationErrorCode { get; set; }

    public string? ValidationErrorMsg { get; set; }

    public long? InterfaceLineId { get; set; }

    public long? JournalLineId { get; set; }
}
