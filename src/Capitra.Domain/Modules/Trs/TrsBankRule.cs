using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_BANK_RULES: a condition the bank puts on using its money, checked at every drawdown. It applies to all the bank's facilities, or
/// to one facility. CONCENTRATION caps what one debtor, seller or debtor group may take (a percent of the funded pool or of the
/// facility limit, or an amount); EXCLUDE refuses a subject; MAX_TENOR caps the invoice tenor. A new or changed rule is PENDING until a
/// second user verifies it (four eyes); only ACTIVE rules are checked.
/// </summary>
public class TrsBankRule : AuditableEntity
{
    public long RuleId { get; set; }

    public long BankId { get; set; }

    /// <summary>Null: the rule covers every facility of the bank.</summary>
    public long? FacilityId { get; set; }

    /// <summary>One of: CONCENTRATION, EXCLUDE, MAX_TENOR.</summary>
    public string RuleTypeCode { get; set; } = "CONCENTRATION";

    /// <summary>One of: DEBTOR, SELLER, DEBTOR_GROUP, INVOICE (MAX_TENOR).</summary>
    public string SubjectTypeCode { get; set; } = "DEBTOR";

    /// <summary>A specific customer (DEBTOR or SELLER). Null with no SUBJECT_VALUE: each one.</summary>
    public long? SubjectClientId { get; set; }

    /// <summary>A debtor group name (CRM_CLIENTS.GROUP_NAME). Null with no SUBJECT_CLIENT_ID: each one.</summary>
    public string? SubjectValue { get; set; }

    public decimal? LimitPct { get; set; }

    public decimal? LimitAmount { get; set; }

    public decimal? WarningPct { get; set; }

    public int? MaxTenorDays { get; set; }

    /// <summary>One of: FUNDED_POOL (what we drew from the bank), FACILITY_LIMIT.</summary>
    public string BasisCode { get; set; } = "FUNDED_POOL";

    /// <summary>One of: BLOCK, OVERRIDE (a second user may approve), WARN.</summary>
    public string BreachActionCode { get; set; } = "BLOCK";

    /// <summary>One of: PENDING, ACTIVE, INACTIVE.</summary>
    public string StatusCode { get; set; } = "PENDING";

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? Notes { get; set; }
}
