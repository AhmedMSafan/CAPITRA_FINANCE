using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_FX_RATE_USAGE_LOGS: copy of FINOVA GL_FX_RATE_USAGE_LOG (APEX), all columns kept.
/// </summary>
public class GlFxRateUsageLog : AuditableEntity
{
    public long RateUsageId { get; set; }

    public long DailyRateId { get; set; }

    public long? LedgerId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string? SourceTransactionType { get; set; }

    public string? SourceTransactionId { get; set; }

    public DateOnly? TransactionDate { get; set; }

    public string EnteredCurrencyCode { get; set; } = string.Empty;

    public string AccountedCurrencyCode { get; set; } = string.Empty;

    public decimal? EnteredAmount { get; set; }

    public decimal? AccountedAmount { get; set; }

    public decimal ExchangeRate { get; set; }
}
