using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Lnd;

/// <summary>
/// LND_PRODUCT_VERSIONS: the lending terms of a Retail Lending product (an ADM_PRODUCTS row of business line MICROFINANCE or
/// CONSUMER_FINANCE), set in Administration > Product Setup > Lending Product Terms. A version is a DRAFT while it is prepared, PENDING
/// once sent for approval, then APPROVED (used from EFFECTIVE_FROM_DATE) or REJECTED. An approved version is never changed: new terms
/// are a new version. A loan keeps a copy of the version in force when its contract is signed, so later versions never touch it.
/// </summary>
public class LndProductVersion : AuditableEntity
{
    public long ProductVersionId { get; set; }

    public long ProductId { get; set; }

    /// <summary>1, 2, 3... per product.</summary>
    public int VersionNo { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    /// <summary>One of: DRAFT, PENDING, APPROVED, REJECTED.</summary>
    public string StatusCode { get; set; } = "DRAFT";

    public decimal MinAmount { get; set; }

    public decimal MaxAmount { get; set; }

    public int MinTermMonths { get; set; }

    public int MaxTermMonths { get; set; }

    /// <summary>One of: WEEKLY, FORTNIGHTLY, MONTHLY.</summary>
    public string RepaymentFrequencyCode { get; set; } = "MONTHLY";

    /// <summary>DECLINING: interest on the reducing balance, equal instalments. FLAT: interest on the original amount for the whole term.</summary>
    public string InterestMethodCode { get; set; } = "DECLINING";

    /// <summary>The yearly rate, in percent (for FLAT, the flat rate a year).</summary>
    public decimal AnnualRatePct { get; set; }

    /// <summary>One of: ACT_365, ACT_360, THIRTY_360. Used by the daily accrual.</summary>
    public string DayCountCode { get; set; } = "ACT_365";

    /// <summary>Late fee: none while the instalment is paid in full within these days; after them, charged back to day 1.</summary>
    public int LateFeeGraceDays { get; set; }

    /// <summary>One of: NONE, FLAT, PCT_ONCE, PCT_DAILY.</summary>
    public string LateFeeTypeCode { get; set; } = "NONE";

    /// <summary>For PCT_ONCE and PCT_DAILY, in percent.</summary>
    public decimal? LateFeeRatePct { get; set; }

    /// <summary>For FLAT: the fee once per late instalment.</summary>
    public decimal? LateFeeFlatAmount { get; set; }

    /// <summary>One of: OVERDUE_INSTALMENT, OVERDUE_PRINCIPAL: what a percentage fee is calculated on.</summary>
    public string LateFeeBaseCode { get; set; } = "OVERDUE_INSTALMENT";

    /// <summary>The order a payment is applied within each instalment, oldest instalment first, e.g. "LATE_FEE,CHARGES,INSURANCE,INTEREST,PRINCIPAL".</summary>
    public string PaymentOrder { get; set; } = "LATE_FEE,CHARGES,INSURANCE,INTEREST,PRINCIPAL";

    /// <summary>Life and disability insurance is required (and its premium is billed with the instalment).</summary>
    public bool IsInsuranceRequired { get; set; }

    /// <summary>The customer may pay principal before it falls due (part prepayment or full early settlement).</summary>
    public bool IsPrepaymentAllowed { get; set; } = true;

    /// <summary>After a part prepayment: REDUCE_INSTALMENT (same number of instalments, smaller) or REDUCE_TERM (same instalment, fewer).</summary>
    public string PrepaymentRecalcCode { get; set; } = "REDUCE_INSTALMENT";

    /// <summary>Fee on the principal paid early by a part prepayment, in % (0 for none).</summary>
    public decimal PrepaymentFeePct { get; set; }

    /// <summary>Fee on the principal settled by a full early settlement, in % (0 for none).</summary>
    public decimal EarlySettlementFeePct { get; set; }

    /// <summary>What changed in this version and why.</summary>
    public string? ChangeReason { get; set; }

    public string? RequestedBy { get; set; }

    public DateTime? RequestedAt { get; set; }

    /// <summary>Who approved or rejected it.</summary>
    public string? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? DecisionNotes { get; set; }
}
