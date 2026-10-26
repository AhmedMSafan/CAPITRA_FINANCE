using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_BALANCES: copy of FINOVA GL_ACCOUNT_BALANCES (APEX), all columns kept.
/// </summary>
public class GlAccountBalance : AuditableEntity
{
    public long BalanceId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal OpeningDr { get; set; }

    public decimal OpeningCr { get; set; }

    public decimal PeriodDr { get; set; }

    public decimal PeriodCr { get; set; }

    public decimal ClosingDr { get; set; }

    public decimal ClosingCr { get; set; }

    public long? LastPostedHeaderId { get; set; }

    public DateTime? LastPostedAt { get; set; }
}
