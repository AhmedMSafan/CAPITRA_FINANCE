using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_EVENT_VALUES: copy of FINOVA GL_ACCOUNTING_EVENT_VALUES (APEX), all columns kept.
/// </summary>
public class GlAccountingEventValue : AuditableEntity
{
    public long EventValueId { get; set; }

    public long AccountingEventId { get; set; }

    public long? SourceValueId { get; set; }

    public string ValueCode { get; set; } = string.Empty;

    public decimal ValueSequence { get; set; } = 1m;

    /// <summary>One of: AMOUNT, NUMBER, TEXT, DATE, CURRENCY, BOOLEAN, ACCOUNT, SEGMENT, PERCENT, JSON.</summary>
    public string ValueKind { get; set; } = string.Empty;

    public decimal? AmountValue { get; set; }

    public decimal? NumberValue { get; set; }

    public decimal? PercentValue { get; set; }

    public string? TextValue { get; set; }

    public DateOnly? DateValueDate { get; set; }

    public string? CurrencyCode { get; set; }

    public long? AccountCombinationId { get; set; }

    public long? AccountId { get; set; }

    public long? SegmentValueId { get; set; }

    public string? JsonValue { get; set; }

    public string? SourceReference { get; set; }
}
