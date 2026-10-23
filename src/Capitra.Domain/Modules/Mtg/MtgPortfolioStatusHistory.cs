using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PORTFOLIO_STATUS_HISTORY: every status a portfolio moved through (APEX FNV_PURSHASED_PORTFOLIO_STATUS_HIST).
/// Who and when are the audit columns CREATED_BY and CREATED_AT. Rows are only ever added.
/// </summary>
public class MtgPortfolioStatusHistory : AuditableEntity
{
    public long PortfolioStatusHistoryId { get; set; }

    public long PortfolioId { get; set; }

    public string? FromStatusCode { get; set; }

    public string ToStatusCode { get; set; } = string.Empty;

    public string? Comments { get; set; }
}
