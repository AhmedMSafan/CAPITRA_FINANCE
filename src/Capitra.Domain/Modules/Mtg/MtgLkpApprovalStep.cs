using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_APPROVAL_STEPS: copy of FINOVA REF_APPROVAL_STEP (APEX), all columns kept.
/// </summary>
public class MtgLkpApprovalStep : AuditableEntity
{
    public string StepCode { get; set; } = string.Empty;

    public string StepNameAr { get; set; } = string.Empty;

    public string StepNameEn { get; set; } = string.Empty;

    public int SequenceNo { get; set; }

    public bool IsActive { get; set; } = true;
}
