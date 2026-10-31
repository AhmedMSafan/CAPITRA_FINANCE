using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_LEDGER_BRANCHES: copy of FINOVA GL_LEDGER_BRANCHES (APEX), all columns kept.
/// </summary>
public class GlLedgerBranch : AuditableEntity
{
    public long LedgerBranchId { get; set; }

    public long LedgerId { get; set; }

    public long BranchId { get; set; }

    public bool IsDefaultBranch { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
