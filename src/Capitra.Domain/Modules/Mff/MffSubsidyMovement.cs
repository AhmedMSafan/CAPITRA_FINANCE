using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_SUBSIDY_MOVEMENTS: copy of FINOVA MFF_SUBSIDY_MOVEMENTS (APEX), all columns kept.
/// </summary>
public class MffSubsidyMovement : AuditableEntity
{
    public long MovementId { get; set; }

    public long SubsidyAccountId { get; set; }

    public DateOnly MovementDate { get; set; }

    public string MovementType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public long? InstallmentId { get; set; }

    public string? ReferenceNo { get; set; }

    public string? DescriptionAr { get; set; }
}
