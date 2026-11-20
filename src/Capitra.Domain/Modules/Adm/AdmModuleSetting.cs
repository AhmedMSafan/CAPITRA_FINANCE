using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_MODULE_SETTINGS: the value of one module setting (Administration > Module Settings, Ahmed, 7 Nov 2026). A setting with no row
/// uses its default from the catalog in code. A change is first a request (PENDING_VALUE, REQUESTED_BY); it takes effect only when a
/// second user approves it, which moves PENDING_VALUE into VALUE_TEXT.
/// </summary>
public class AdmModuleSetting : AuditableEntity
{
    public long SettingId { get; set; }

    public string ModuleCode { get; set; } = string.Empty;

    public string SettingCode { get; set; } = string.Empty;

    /// <summary>The value in force: "true"/"false", a whole number, a decimal (invariant culture) or a choice code.</summary>
    public string ValueText { get; set; } = string.Empty;

    public string? PendingValue { get; set; }

    public string? PendingReason { get; set; }

    public string? RequestedBy { get; set; }

    public DateTime? RequestedAt { get; set; }

    /// <summary>Who asked for the value in force, and who approved it.</summary>
    public string? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}

/// <summary>ADM_MODULE_SETTING_HISTORY: every request, approval, rejection and withdrawal of a module setting change.</summary>
public class AdmModuleSettingHistory : AuditableEntity
{
    public long HistoryId { get; set; }

    public string SettingCode { get; set; } = string.Empty;

    /// <summary>REQUEST, APPROVE, REJECT or CANCEL.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Notes { get; set; }
}
