using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_DAILY_BALANCES: copy of FINOVA GL_DAILY_BALANCES (APEX), all columns kept.
/// </summary>
public class GlDailyBalance : AuditableEntity
{
    public long DailyBalanceId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public DateOnly BalanceDate { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal DailyDr { get; set; }

    public decimal DailyCr { get; set; }

    public decimal ClosingDr { get; set; }

    public decimal ClosingCr { get; set; }
}
