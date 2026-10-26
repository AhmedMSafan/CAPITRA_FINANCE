using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_JOURNAL_LINES: one debit or credit of a journal on one account combination.</summary>
public class GlJournalLine : AuditableEntity
{
    public long JournalLineId { get; set; }

    public long JournalHeaderId { get; set; }

    public int LineNo { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    /// <summary>In the ledger currency. Journals are entered in the ledger currency, so it equals the entered amount.</summary>
    public decimal AccountedDr { get; set; }

    public decimal AccountedCr { get; set; }

    public string? LineDescription { get; set; }

    public string? ReferenceNo { get; set; }
}
