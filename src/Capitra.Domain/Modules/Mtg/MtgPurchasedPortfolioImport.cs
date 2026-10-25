using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_IMPORTS: the seller workbook of a portfolio and the result of its last validation
/// (APEX MTG_PORTFOLIO_EXCEL_STAGING, App 105 page 208). One row per portfolio.
/// </summary>
public class MtgPurchasedPortfolioImport : AuditableEntity
{
    public const string ValidatedStatus = "VALIDATED";
    public const string ErrorStatus = "ERROR";
    public const string ImportedStatus = "IMPORTED";

    public long PortfolioImportId { get; set; }

    public long PortfolioId { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>VALIDATED, ERROR or IMPORTED.</summary>
    public string ImportStatusCode { get; set; } = ErrorStatus;

    public int TotalRowCount { get; set; }

    public int ValidRowCount { get; set; }

    public int ErrorRowCount { get; set; }

    public int ImportedRowCount { get; set; }
}
