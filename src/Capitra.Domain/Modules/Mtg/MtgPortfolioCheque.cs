using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PORTFOLIO_CHEQUE: a post-dated cheque an investor hands over for one of their contracts in the portfolio
/// (APEX FNV_PURCHASED_PORTFOLIO_CHEQUE, App 105 page 213).
/// </summary>
public class MtgPortfolioCheque : AuditableEntity
{
    /// <summary>Status a cheque gets when none is chosen, as in APEX.</summary>
    public const string PendingStatus = "PENDING";

    public long ChequeId { get; set; }

    public long PortfolioId { get; set; }

    public long PortfolioAccountId { get; set; }

    public string ChequeNo { get; set; } = string.Empty;

    /// <summary>Issuing bank, as written on the cheque.</summary>
    public string? BankName { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal ChequeAmount { get; set; }

    public bool IsEndorsedToCompany { get; set; }

    /// <summary>ADM_LKP_CHEQUE_STATUS code.</summary>
    public string ChequeStatusCode { get; set; } = PendingStatus;

    public DateOnly? ClearingDate { get; set; }

    public string? ReturnReason { get; set; }
}
