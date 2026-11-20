using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_EARLY_SETTLEMENTS: copy of FINOVA MFF_EARLY_SETTLEMENTS (APEX), all columns kept.
/// </summary>
public class MffEarlySettlement : AuditableEntity
{
    public long SettlementId { get; set; }

    public long LoanId { get; set; }

    public DateOnly RequestDate { get; set; }

    public DateOnly? SettlementDate { get; set; }

    public string SettlementType { get; set; } = string.Empty;

    public decimal PrincipalSettled { get; set; }

    public decimal? FinancePctSettled { get; set; }

    public decimal? SubsidyRefundPct { get; set; }

    public decimal? SubsidyRefundAmount { get; set; }

    public string EarlySettlementStatus { get; set; } = "REQUESTED";

    public string? ApprovedBy { get; set; }

    public string? NotesAr { get; set; }
}
