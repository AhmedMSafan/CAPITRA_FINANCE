using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_ACTION_CATALOGS: copy of FINOVA FND_CHEQUE_ACTION_CATALOG (APEX), all columns kept.
/// </summary>
public class ChqActionCatalog : AuditableEntity
{
    public string ActionCode { get; set; } = string.Empty;

    public string ActionNameEn { get; set; } = string.Empty;

    public string? ActionNameAr { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string? TargetLocationCode { get; set; }

    public string? AllowedFromStatus { get; set; }

    public string? AllowedFromLocation { get; set; }

    public bool IsDepositBatchRequired { get; set; }

    public bool IsBankRefRequired { get; set; }

    public bool IsRejectionReasonRequired { get; set; }

    public bool IsNotesRequired { get; set; }

    public string? PackageProcedureName { get; set; }

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
