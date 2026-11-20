using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_DISBURSEMENT_FUNDINGS: one source of funds of a disbursement, chosen at the Treasury Disburse step: own funds (no sublimit) or a
/// bank sublimit. A saved line is RESERVED (it holds the sublimit's room); Execute makes it EXECUTED (a drawdown); a changed choice
/// RELEASES the old lines. A line above a bank rule whose breach action is OVERRIDE needs a second user's approval before Execute.
/// </summary>
public class TrsDisbursementFunding : AuditableEntity
{
    public long FundingId { get; set; }

    public long DisbursementId { get; set; }

    /// <summary>One of: OWN_FUNDS, BANK.</summary>
    public string SourceTypeCode { get; set; } = "BANK";

    public long? SublimitId { get; set; }

    public long? FacilityId { get; set; }

    public long? BankId { get; set; }

    public decimal FundedAmount { get; set; }

    /// <summary>One of: RESERVED, EXECUTED, RELEASED.</summary>
    public string StatusCode { get; set; } = "RESERVED";

    public bool IsOverrideRequired { get; set; }

    public string? OverrideReason { get; set; }

    public string? OverrideApprovedBy { get; set; }

    public DateTime? OverrideApprovedAt { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? ExecutedBy { get; set; }

    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Our bank account the client is paid from (Execute): for a bank line our account at that bank, where the bank puts the drawdown;
    /// for own funds the main operating account.
    /// </summary>
    public long? PaidFromAccountId { get; set; }

    /// <summary>The GL transaction set ID Execute posted the drawdown and the payment under, e.g. CAP-TRS-2026-00000001.</summary>
    public string? GlTranSetId { get; set; }
}
