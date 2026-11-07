using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_CHEQUE_STATUS_HIST: copy of FINOVA FNV_CHEQUE_STATUS_HISTORY (APEX), all columns kept.
/// </summary>
public class ChqChequeStatusHist : AuditableEntity
{
    public long ChequeStatusHistoryId { get; set; }

    public long ChequeId { get; set; }

    public string? OldStatusCode { get; set; }

    /// <summary>One of: COLLECTED, BOUNCED, REJECTED.</summary>
    public string NewStatusCode { get; set; } = string.Empty;

    public DateOnly StatusDate { get; set; }

    public string? BankStatementReference { get; set; }

    public string? ReasonCode { get; set; }

    public string? Notes { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }
}
