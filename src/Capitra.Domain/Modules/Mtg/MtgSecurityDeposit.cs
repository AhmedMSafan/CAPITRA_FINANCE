using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_SECURITY_DEPOSITS: copy of FINOVA FNV_SECURITY_DEPOSIT (APEX), all columns kept.
/// </summary>
public class MtgSecurityDeposit : AuditableEntity
{
    public long DepositId { get; set; }

    public long PortfolioId { get; set; }

    public string? BankName { get; set; }

    public string? AccountReference { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal? DepositPct { get; set; }

    public bool IsBlocked { get; set; } = true;

    public DateOnly? StartDate { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public long? DocumentId { get; set; }
}
