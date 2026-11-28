using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Lnd;

/// <summary>
/// LND_PARAMETER_VALUES: one dated value of a Retail Lending parameter for one licence (Ahmed, 4 Nov 2026: every legal figure is an
/// admin setting, not code). The parameters themselves are defined in code (LndParameterCatalog). A row is never changed once decided:
/// a new value is a new row with its own effective date, so the value used on any past day can always be found.
/// A value is first PENDING; it is used only once a second user APPROVES it, from its EFFECTIVE_FROM_DATE.
/// </summary>
public class LndParameterValue : AuditableEntity
{
    public long ParameterValueId { get; set; }

    /// <summary>The parameter, e.g. LND.MAX_FINANCING_AMOUNT.</summary>
    public string ParameterCode { get; set; } = string.Empty;

    /// <summary>The licence the value applies to: MF (microfinance) or CF (consumer finance).</summary>
    public string ScopeCode { get; set; } = string.Empty;

    /// <summary>"true"/"false", a whole number or a decimal (invariant culture).</summary>
    public string ValueText { get; set; } = string.Empty;

    /// <summary>The first business day the value is used.</summary>
    public DateOnly EffectiveFromDate { get; set; }

    /// <summary>The FRA decision (or internal policy) the value comes from, e.g. "FRA Decision 1 of 2026".</summary>
    public string? DecisionReference { get; set; }

    public string RequestReason { get; set; } = string.Empty;

    /// <summary>One of: PENDING, APPROVED, REJECTED, WITHDRAWN.</summary>
    public string StatusCode { get; set; } = "PENDING";

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    /// <summary>Who approved, rejected or withdrew it.</summary>
    public string? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? DecisionNotes { get; set; }
}
