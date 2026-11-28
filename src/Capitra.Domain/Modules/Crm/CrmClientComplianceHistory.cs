using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>CRM_CLIENT_COMPLIANCE_HISTORY: a copy of the customer's compliance profile at each save and each verification.</summary>
public class CrmClientComplianceHistory : AuditableEntity
{
    public long ComplianceHistoryId { get; set; }

    public long ClientId { get; set; }

    /// <summary>SAVE or VERIFY.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? RiskGrade { get; set; }

    public string? KycStatus { get; set; }

    public DateOnly? KycExpiryDate { get; set; }

    public string? AmlStatus { get; set; }

    public DateOnly? AmlCheckDate { get; set; }

    public string? SanctionsStatus { get; set; }

    public bool IsPep { get; set; }

    public bool IsBlacklisted { get; set; }

    public string? BlacklistReason { get; set; }

    public DateOnly? NextReviewDate { get; set; }

    public string? Notes { get; set; }
}
