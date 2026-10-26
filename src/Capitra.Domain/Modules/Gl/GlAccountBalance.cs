using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_BALANCES: posted debits and credits of one combination in one ledger period. The opening balance is the
/// sum of the earlier periods, worked out when it is read, so posting into an earlier open period never leaves it stale.
/// </summary>
public class GlAccountBalance : AuditableEntity
{
    public long BalanceId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal PeriodDr { get; set; }

    public decimal PeriodCr { get; set; }

    public long? LastPostedHeaderId { get; set; }

    public DateTime? LastPostedAt { get; set; }
}
