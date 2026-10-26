using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_UPLOAD_LINES: copy of FINOVA FND_CHEQUE_UPLOAD_LINE (APEX), all columns kept.
/// </summary>
public class ChqUploadLine : AuditableEntity
{
    public long LineId { get; set; }

    public long BatchId { get; set; }

    public int LineNo { get; set; }

    public string? RawSr { get; set; }

    public string? RawClientId { get; set; }

    public string? RawPartyTaxId { get; set; }

    public string? RawChequeNo { get; set; }

    public string? RawDueDate { get; set; }

    public string? RawChequeAmount { get; set; }

    public string? RawIssuerBankCode { get; set; }

    public string? ClientId { get; set; }

    public string? PartyTaxId { get; set; }

    public string? ChequeNo { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal? ChequeAmount { get; set; }

    public string? IssuerBankCode { get; set; }

    public string? IssuerBankName { get; set; }

    /// <summary>One of: LOADED, VALID, ERROR, SUBMITTED.</summary>
    public string LineStatus { get; set; } = "LOADED";

    public string? ErrorMessage { get; set; }

    public long? TargetChequeId { get; set; }

    public string? TargetChequeRef { get; set; }
}
