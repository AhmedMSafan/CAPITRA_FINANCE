using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_INSURANCES: copy of FINOVA FNV_INSURANCE (APEX), all columns kept.
/// </summary>
public class MtgInsurance : AuditableEntity
{
    public long InsuranceId { get; set; }

    public long PortfolioId { get; set; }

    public long? UnitId { get; set; }

    public string InsuranceTypeCode { get; set; } = string.Empty;

    public string? InsuranceCompany { get; set; }

    public string? PolicyNo { get; set; }

    public decimal? SumInsured { get; set; }

    public DateOnly? ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    public bool IsAssignedToCompany { get; set; }

    public long? DocumentId { get; set; }
}
