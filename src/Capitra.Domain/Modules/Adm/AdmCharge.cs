using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_CHARGES: a reusable charge such as a legal or valuation fee (FINOVA_FINANCEING ADM_CHARGE_COMPONENTS).</summary>
public class AdmCharge : AuditableEntity
{
    public long ChargeId { get; set; }

    public string ChargeCode { get; set; } = string.Empty;

    public string ChargeNameEn { get; set; } = string.Empty;

    public string? ChargeNameAr { get; set; }

    public string ChargeTypeCode { get; set; } = string.Empty;

    /// <summary>One of: REVENUE, EXPENSE, OTHER.</summary>
    public string FinancialNatureCode { get; set; } = string.Empty;

    public bool IsRecurring { get; set; }

    /// <summary>One of: FIXED, PERCENTAGE, FORMULA.</summary>
    public string CalculationTypeCode { get; set; } = string.Empty;

    /// <summary>Default rate in percent, PERCENTAGE charges only.</summary>
    public decimal? DefaultRate { get; set; }

    /// <summary>Default amount, FIXED charges only.</summary>
    public decimal? DefaultAmount { get; set; }

    /// <summary>Currency of the default amount, FIXED charges only.</summary>
    public string? CurrencyCode { get; set; }

    public string? Description { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;
}
