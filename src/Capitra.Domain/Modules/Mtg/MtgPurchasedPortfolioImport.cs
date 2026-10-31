using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_IMPORTS: copy of FINOVA MTG_PORTFOLIO_EXCEL_STAGING (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioImport : AuditableEntity
{
    public long StagingId { get; set; }

    public long PortfolioId { get; set; }

    public long WfInstanceId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ValidationStatus { get; set; } = string.Empty;

    public string ValidationJson { get; set; } = string.Empty;

    public decimal TotalRows { get; set; }

    public int ValidCount { get; set; }

    public int ErrorCount { get; set; }
}
