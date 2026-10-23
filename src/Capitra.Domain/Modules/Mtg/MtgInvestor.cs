using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_INVESTOR: end investor (unit buyer) whose accounts are purchased (APEX App 105 pages 109/110).</summary>
public class MtgInvestor : AuditableEntity
{
    public long InvestorId { get; set; }

    public string InvestorCode { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string? FullNameEn { get; set; }

    public string? NationalIdNo { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? PhoneNo { get; set; }

    public string? EmailAddress { get; set; }

    public string? AddressText { get; set; }

    public string? EmployerName { get; set; }

    public decimal? DeclaredIncomeAmount { get; set; }

    public bool IsActive { get; set; } = true;
}
