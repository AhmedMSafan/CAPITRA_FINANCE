namespace Capitra.Application.Modules.Gl.ChartOfAccounts;

/// <summary>Everything the chart of accounts tree shows for one structure (APEX App 120 page 2, process GL_TREE_LOAD).</summary>
public sealed record ChartOfAccountsDto(
    long? CoaId,
    IReadOnlyList<CoaOptionDto> Structures,
    IReadOnlyList<AccountSegmentInfoDto> Segments,
    IReadOnlyList<GlAccountDto> Accounts,
    IReadOnlyList<AccountTypeDefaultDto> TypeDefaults,
    AccountListsDto Lists);

public sealed record CoaOptionDto(long CoaId, string CoaCode, string CoaNameEn, string DelimiterChar, int SegmentCount, int AccountSegmentNum);

public sealed record AccountSegmentInfoDto(int SegmentNum, string SegmentCode, string SegmentNameEn, string SegmentType, int MinLength, int MaxLength, string ValueFormat);

/// <summary>Normal balance, statement and cash flow class offered for a new account of this type.</summary>
public sealed record AccountTypeDefaultDto(string AccountType, string NormalBalance, string FinancialStatementClass, string CashFlowClass);

public sealed record AccountListsDto(
    IReadOnlyList<string> AccountTypes,
    IReadOnlyList<string> NormalBalances,
    IReadOnlyList<string> FinancialStatementClasses,
    IReadOnlyList<string> CashFlowClasses);

public sealed record GlAccountDto(
    long AccountId,
    string AccountCode,
    string AccountNameEn,
    string? AccountNameAr,
    string? Description,
    long? ParentAccountId,
    string AccountType,
    string NormalBalance,
    string FinancialStatementClass,
    string CashFlowClass,
    bool IsSummary,
    bool IsPostingAllowed,
    bool IsControlAccount,
    bool IsReconciliation,
    bool IsBranchRequired,
    bool IsCostCenterRequired,
    bool IsBusinessLineRequired,
    bool IsManualJournalAllowed,
    bool IsSubledgerPostingAllowed,
    bool IsCurrencyRevaluation,
    bool IsRetainedEarnings,
    bool IsActive,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int SortOrder,
    string? SegmentValueCode, // the linked value of the account segment
    string CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt,
    int RowVersion);

public sealed record SaveGlAccountRequest(
    long? CoaId, // used when the account is created
    string? AccountCode,
    string? AccountNameEn,
    string? AccountNameAr,
    string? Description,
    long? ParentAccountId,
    string? AccountType,
    string? NormalBalance,
    string? FinancialStatementClass,
    string? CashFlowClass,
    bool IsSummary,
    bool IsPostingAllowed,
    bool IsControlAccount,
    bool IsReconciliation,
    bool IsBranchRequired,
    bool IsCostCenterRequired,
    bool IsBusinessLineRequired,
    bool IsManualJournalAllowed,
    bool IsSubledgerPostingAllowed,
    bool IsCurrencyRevaluation,
    bool IsRetainedEarnings,
    bool IsActive,
    DateOnly? EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int? RowVersion);

public interface IGlChartOfAccountsService
{
    /// <summary>The chosen structure's accounts, or those of the first structure that has accounts.</summary>
    Task<ChartOfAccountsDto> GetAsync(long? coaId, CancellationToken cancellationToken);

    Task<GlAccountDto> CreateAsync(SaveGlAccountRequest request, CancellationToken cancellationToken);

    Task<GlAccountDto> UpdateAsync(long accountId, SaveGlAccountRequest request, CancellationToken cancellationToken);
}
