using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_LINES: copy of FINOVA GL_JOURNAL_LINES (APEX), all columns kept.
/// </summary>
public class GlJournalLine : AuditableEntity
{
    public long JournalLineId { get; set; }

    public long JournalHeaderId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public int LineNo { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    public decimal AccountedDr { get; set; }

    public decimal AccountedCr { get; set; }

    public decimal ConversionRate { get; set; } = 1m;

    public string? LineDescription { get; set; }

    public string? ReferenceNo { get; set; }

    public string? SourceLineId { get; set; }

    public string? SourceLineNo { get; set; }

    public decimal? StatisticalAmount { get; set; }

    public bool IsReconciliation { get; set; }

    public bool IsCleared { get; set; }

    public bool IsActive { get; set; } = true;
}
