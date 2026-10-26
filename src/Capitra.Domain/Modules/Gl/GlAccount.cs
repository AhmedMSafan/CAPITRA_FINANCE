using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTS: a natural account in the chart of accounts tree (APEX App 120 pages 2/3).
/// Its code is always a value of the structure's account segment; the two are saved together.
/// </summary>
public class GlAccount : AuditableEntity
{
    public long AccountId { get; set; }

    public long CoaId { get; set; }

    /// <summary>Cannot change once saved: it is the value of the account segment.</summary>
    public string AccountCode { get; set; } = string.Empty;

    public string AccountNameEn { get; set; } = string.Empty;

    public string? AccountNameAr { get; set; }

    public string? Description { get; set; }

    public long? ParentAccountId { get; set; }

    /// <summary>The matching GL_COA_SEGMENT_VALUES row of the account segment.</summary>
    public long? AccountSegmentValueId { get; set; }

    /// <summary>One of <see cref="GlCodes.AccountTypes"/>.</summary>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>DR or CR.</summary>
    public string NormalBalance { get; set; } = string.Empty;

    /// <summary>BALANCE_SHEET or INCOME_STATEMENT.</summary>
    public string FinancialStatementClass { get; set; } = string.Empty;

    /// <summary>One of <see cref="GlCodes.CashFlowClasses"/>.</summary>
    public string CashFlowClass { get; set; } = GlCodes.CashFlowNone;

    /// <summary>A summary (parent) account never allows posting.</summary>
    public bool IsSummary { get; set; }

    public bool IsPostingAllowed { get; set; } = true;

    public bool IsControlAccount { get; set; }

    public bool IsReconciliation { get; set; }

    public bool IsBranchRequired { get; set; }

    public bool IsCostCenterRequired { get; set; }

    public bool IsBusinessLineRequired { get; set; }

    public bool IsManualJournalAllowed { get; set; } = true;

    public bool IsSubledgerPostingAllowed { get; set; } = true;

    public bool IsCurrencyRevaluation { get; set; }

    /// <summary>Only one account of a structure can be the retained earnings account.</summary>
    public bool IsRetainedEarnings { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public int SortOrder { get; set; } = 10;
}
