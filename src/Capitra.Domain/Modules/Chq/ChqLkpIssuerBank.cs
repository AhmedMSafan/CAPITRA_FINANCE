using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_LKP_ISSUER_BANKS: copy of FINOVA LKP_CHEQUE_ISSUER_BANK (APEX), all columns kept.
/// </summary>
public class ChqLkpIssuerBank : AuditableEntity
{
    public string BankCode { get; set; } = string.Empty;

    public string BankNameEn { get; set; } = string.Empty;

    public string? BankNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public int? DisplaySeq { get; set; }
}
