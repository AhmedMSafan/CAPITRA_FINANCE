using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_INVESTORS: copy of FINOVA FNV_INVESTOR (APEX), all columns kept.
/// </summary>
public class MtgInvestor : AuditableEntity
{
    public long InvestorId { get; set; }

    public string InvestorCode { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string? FullNameEn { get; set; }

    public string? NationalId { get; set; }

    public DateOnly? DateOfBirthDate { get; set; }

    public string? PhoneNo { get; set; }

    public string? EmailAddress { get; set; }

    public string? AddressText { get; set; }

    public string? EmployerName { get; set; }

    public decimal? DeclaredIncome { get; set; }

    public bool IsActive { get; set; } = true;
}
