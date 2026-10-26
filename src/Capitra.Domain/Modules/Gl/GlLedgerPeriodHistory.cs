using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_LEDGER_PERIODS_HIST: every status change of a ledger period. The audit columns record who did it and when.</summary>
public class GlLedgerPeriodHistory : AuditableEntity
{
    public long LedgerPeriodHistoryId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string? OldPeriodStatus { get; set; }

    public string NewPeriodStatus { get; set; } = string.Empty;

    /// <summary>CREATE, OPEN, SET_FUTURE_ENTRY, CLOSE, REOPEN or PERMANENT_CLOSE.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? ActionReason { get; set; }
}
