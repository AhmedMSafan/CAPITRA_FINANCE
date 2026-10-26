using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_BALANCE_IMPACTS: what posting one journal line did to its period balance.
/// Before and after are the period's net balance (debit minus credit) at that moment.
/// </summary>
public class GlPostingBalanceImpact : AuditableEntity
{
    public long BalanceImpactId { get; set; }

    public long JournalHeaderId { get; set; }

    public long JournalLineId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public DateOnly AccountingDate { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal AccountedDr { get; set; }

    public decimal AccountedCr { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }
}
