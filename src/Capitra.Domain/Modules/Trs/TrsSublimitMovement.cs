using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_SUBLIMIT_MOVEMENTS: every change to what a sublimit has drawn (outstanding) or holds for an approved disbursement (reserved).
/// OPENING carries a balance in, RESERVE and RELEASE move the reserved amount, DRAWDOWN moves reserved to outstanding (or adds to
/// outstanding), REPAYMENT lowers outstanding, ADJUSTMENT corrects either. The debtor and seller are kept so concentration can be
/// measured per bank.
/// </summary>
public class TrsSublimitMovement : AuditableEntity
{
    public long MovementId { get; set; }

    public long SublimitId { get; set; }

    /// <summary>One of: OPENING, RESERVE, RELEASE, DRAWDOWN, REPAYMENT, ADJUSTMENT.</summary>
    public string MovementTypeCode { get; set; } = "OPENING";

    public DateOnly MovementDate { get; set; }

    /// <summary>Signed change to the outstanding amount.</summary>
    public decimal OutstandingChange { get; set; }

    /// <summary>Signed change to the reserved amount.</summary>
    public decimal ReservedChange { get; set; }

    public long? DisbursementId { get; set; }

    /// <summary>The Treasury Disburse line (TRS_DISBURSEMENT_FUNDINGS) that made the movement.</summary>
    public long? FundingId { get; set; }

    public long? DebtorClientId { get; set; }

    public long? SellerClientId { get; set; }

    public string? Notes { get; set; }
}
