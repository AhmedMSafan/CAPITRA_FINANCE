using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_ACCOUNTING_ENTRIES: copy of FINOVA MFF_ACCOUNTING_ENTRIES (APEX), all columns kept.
/// </summary>
public class MffAccountingEntry : AuditableEntity
{
    public long EntryId { get; set; }

    public long? LoanId { get; set; }

    public long? ApplicationId { get; set; }

    public string EventCode { get; set; } = string.Empty;

    public DateOnly EntryDate { get; set; }

    public string GlAccountCode { get; set; } = string.Empty;

    public decimal DebitAmount { get; set; }

    public decimal CreditAmount { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string? ReferenceNo { get; set; }

    public string PostingStatus { get; set; } = "NEW";

    public string? CoreJournalRef { get; set; }

    public string? DescriptionAr { get; set; }
}
