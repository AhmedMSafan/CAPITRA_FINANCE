using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_DOCUMENTS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_DOCUMENT (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioDocument : AuditableEntity
{
    public long DocumentId { get; set; }

    public long PortfolioId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public long? EntityId { get; set; }

    public string DocumentTypeCode { get; set; } = string.Empty;

    public string? DocumentNo { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? FileName { get; set; }

    public string? MimeType { get; set; }

    public byte[]? FileBlob { get; set; }

    public string? FileUrl { get; set; }

    public bool IsVerified { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? DocumentStatus { get; set; } = "RECEIVED";
}
