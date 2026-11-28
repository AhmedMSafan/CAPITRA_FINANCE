using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTS: copy of FINOVA GL_ACCOUNTS (APEX), all columns kept.
/// </summary>
public class GlAccount : AuditableEntity
{
    public long AccountId { get; set; }

    public long CoaId { get; set; }

    public string AccountCode { get; set; } = string.Empty;

    public string AccountNameEn { get; set; } = string.Empty;

    public string? AccountNameAr { get; set; }

    public string? Description { get; set; }

    public long? ParentAccountId { get; set; }

    public long? AccountSegmentValueId { get; set; }

    /// <summary>One of: ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE, CONTRA_ASSET, CONTRA_REVENUE, MEMO.</summary>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>One of: DR, CR.</summary>
    public string NormalBalance { get; set; } = string.Empty;

    /// <summary>One of: BALANCE_SHEET, INCOME_STATEMENT, CASH_FLOW, EQUITY, OFF_BALANCE.</summary>
    public string FinancialStatementClass { get; set; } = string.Empty;

    /// <summary>One of: OPERATING, INVESTING, FINANCING, CASH_EQUIVALENT, NONE.</summary>
    public string CashFlowClass { get; set; } = "NONE";

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

    public bool IsRetainedEarnings { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public long SortOrder { get; set; } = 10;

    public string? ExternalGlAccountMap { get; set; }
}
