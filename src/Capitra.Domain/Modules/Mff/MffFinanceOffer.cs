using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_FINANCE_OFFERS: copy of FINOVA MFF_FINANCE_OFFERS (APEX), all columns kept.
/// </summary>
public class MffFinanceOffer : AuditableEntity
{
    public long OfferId { get; set; }

    public long ApplicationId { get; set; }

    public string OfferNo { get; set; } = string.Empty;

    public DateOnly OfferDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? FinanceAmount { get; set; }

    public int? TermMonths { get; set; }

    public decimal? AnnualRatePct { get; set; }

    public decimal? InitialInstallment { get; set; }

    public decimal? DownpaymentAmount { get; set; }

    public decimal? SubsidyAmount { get; set; }

    public decimal? LtvPct { get; set; }

    public string OfferStatus { get; set; } = "DRAFT";

    public DateOnly? AcceptedDate { get; set; }

    public string? Notes { get; set; }
}
