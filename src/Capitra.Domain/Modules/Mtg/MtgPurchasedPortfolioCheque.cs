using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_CHEQUES: copy of FINOVA FNV_PURCHASED_PORTFOLIO_CHEQUE (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioCheque : AuditableEntity
{
    public long ChequeId { get; set; }

    public long PortfolioId { get; set; }

    public long? PortfolioAccountId { get; set; }

    public string ChequeNo { get; set; } = string.Empty;

    public string? BankName { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal ChequeAmount { get; set; }

    public bool IsEndorsedToCompany { get; set; }

    public string ChequeStatusCode { get; set; } = "PENDING";

    public DateOnly? ClearingDate { get; set; }

    public string? ReturnReason { get; set; }
}
