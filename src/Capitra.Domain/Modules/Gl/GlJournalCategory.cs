using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_CATEGORIES: copy of FINOVA GL_JOURNAL_CATEGORIES (APEX), all columns kept.
/// </summary>
public class GlJournalCategory : AuditableEntity
{
    public long CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryNameEn { get; set; } = string.Empty;

    public string? CategoryNameAr { get; set; }

    /// <summary>One of: STANDARD, ACCRUAL, REVERSAL, RECURRING, OPENING_BALANCE, CLOSING, DEPRECIATION, ASSET_ADDITION, ASSET_DISPOSAL, BANK_TRANSFER, BANK_CHARGE, FX_REVALUATION, SOURCE_ACCOUNTING, MIGRATION, OTHER.</summary>
    public string CategoryType { get; set; } = "STANDARD";

    public long? DefaultSourceId { get; set; }

    public bool IsReversalRequired { get; set; }

    public bool IsManualAllowed { get; set; } = true;

    public bool IsImportAllowed { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
