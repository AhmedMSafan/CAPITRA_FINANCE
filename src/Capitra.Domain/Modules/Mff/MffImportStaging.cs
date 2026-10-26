using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_IMPORT_STAGINGS: copy of FINOVA MFF_IMPORT_STAGING (APEX), all columns kept.
/// </summary>
public class MffImportStaging : AuditableEntity
{
    public long StagingId { get; set; }

    public long BatchId { get; set; }

    public int SourceRowNo { get; set; }

    public string? ApplicantNameAr { get; set; }

    public string? ApplicantNationalId { get; set; }

    public string? SpouseNameAr { get; set; }

    public string? SpouseNationalId { get; set; }

    public string? MaritalStatusRaw { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public decimal? CustomerDownpayment { get; set; }

    public decimal? TotalSubsidy { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? MaintenanceDeposit { get; set; }

    public decimal? FinanceAmount { get; set; }

    public string? AnnouncementRaw { get; set; }

    public string? ProjectRaw { get; set; }

    public string? GovernorateRaw { get; set; }

    public string? CityRaw { get; set; }

    public string? DistrictRaw { get; set; }

    public decimal? AreaSqm { get; set; }

    public string? NeighborhoodRaw { get; set; }

    public string? BuildingNo { get; set; }

    public string? FloorNo { get; set; }

    public string? UnitNo { get; set; }

    public string? SiteRaw { get; set; }

    public string? MobileNo { get; set; }

    public string ValidationStatus { get; set; } = "NEW";

    public string? ErrorMessage { get; set; }

    public long? ApplicationId { get; set; }
}
