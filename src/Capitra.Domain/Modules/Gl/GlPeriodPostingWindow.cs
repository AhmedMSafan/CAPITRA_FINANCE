using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_POSTING_WINDOWS: copy of FINOVA GL_PERIOD_POSTING_WINDOWS (APEX), all columns kept.
/// </summary>
public class GlPeriodPostingWindow : AuditableEntity
{
    public long PostingWindowId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string? SourceCode { get; set; }

    public string? CategoryCode { get; set; }

    public bool IsManualPostAllowed { get; set; } = true;

    public bool IsAutoPostAllowed { get; set; } = true;

    public bool IsInterfaceAllowed { get; set; } = true;

    public DateTime? PostingCutoffAt { get; set; }

    public bool IsReopened { get; set; }

    public string? ReopenedBy { get; set; }

    public DateTime? ReopenedAt { get; set; }

    public string? ReopenReason { get; set; }

    public bool IsActive { get; set; } = true;
}
