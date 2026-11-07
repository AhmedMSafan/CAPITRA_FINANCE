using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_GUARANTEES: copy of FINOVA FNV_GUARANTEE (APEX), all columns kept.
/// </summary>
public class MtgGuarantee : AuditableEntity
{
    public long GuaranteeId { get; set; }

    public long PortfolioId { get; set; }

    public string GuaranteeTypeCode { get; set; } = string.Empty;

    public string? GuarantorType { get; set; }

    public long? GuarantorId { get; set; }

    public decimal? GuaranteeAmount { get; set; }

    public decimal? GuaranteePct { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public long? DocumentId { get; set; }

    public bool IsVerified { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }
}
