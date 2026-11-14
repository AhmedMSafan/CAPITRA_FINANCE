using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_OUT_OF_OFFICE_ENTRIES: a user away between two dates and who works their tasks meanwhile (K2's out of office), for all workflows
/// (WorkflowCode null) or for one. A rule for one workflow wins over the rule for all.
/// </summary>
public class WfeOutOfOffice : AuditableEntity
{
    public long OutOfOfficeId { get; set; }

    /// <summary>The user who is away (user name).</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Who works the tasks meanwhile (user name).</summary>
    public string DelegateTo { get; set; } = string.Empty;

    /// <summary>Null for all workflows.</summary>
    public string? WorkflowCode { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Note { get; set; }
}
