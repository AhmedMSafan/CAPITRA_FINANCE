using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CURRENCY_EXPOSURE_LINES: copy of FINOVA GL_CURRENCY_EXPOSURE_LINES (APEX), all columns kept.
/// </summary>
public class GlCurrencyExposureLine : AuditableEntity
{
    public long ExposureLineId { get; set; }

    public long ExposureSnapshotId { get; set; }

    public int LineNo { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string? BusinessLineCode { get; set; }

    public long? AccountCombinationId { get; set; }

    /// <summary>One of: ASSET, LIABILITY, REVENUE, EXPENSE, NET_BALANCE, BANK, FACILITY, OTHER.</summary>
    public string ExposureType { get; set; } = "NET_BALANCE";

    public decimal EnteredAmount { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal AccountedAmount { get; set; }

    public string FunctionalCurrencyCode { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
