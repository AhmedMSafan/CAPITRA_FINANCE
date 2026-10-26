namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>Filters of the journals page (APEX App 120 page 51, process J51_SEARCH).</summary>
public sealed record JournalSearchRequest(
    string? Stage,
    string? Q,
    string? Tran,
    long? LedgerId,
    string? Source,
    string? Category,
    string? Period,
    DateOnly? From,
    DateOnly? To,
    decimal? MinAmount,
    decimal? MaxAmount,
    string? CreatedBy,
    IReadOnlyDictionary<int, string?>? Segments);

public sealed record JournalSearchResultDto(
    IReadOnlyList<JournalRowDto> Rows,
    bool More,
    IReadOnlyDictionary<string, int> Counts,
    JournalSearchListsDto Lists);

/// <summary>Stage is DRAFT, SUBMITTED, APPROVED, POSTED, REVERSED, REJECTED or CANCELLED.</summary>
public sealed record JournalRowDto(
    long JournalHeaderId,
    string JournalNo,
    string JournalName,
    string? Description,
    string BatchNo,
    string? TranSetId,
    string LedgerCode,
    string SourceCode,
    string CategoryCode,
    DateOnly AccountingDate,
    string PeriodCode,
    string CurrencyCode,
    decimal TotalDr,
    int LineCount,
    string Stage,
    bool IsReversal,
    string CreatedBy,
    string? SubmittedBy,
    string? PostError,
    int RowVersion);

public sealed record JournalSearchListsDto(
    IReadOnlyList<JournalLedgerDto> Ledgers,
    IReadOnlyList<JournalOptionDto> Sources,
    IReadOnlyList<JournalOptionDto> Categories,
    IReadOnlyList<string> Periods,
    IReadOnlyList<JournalFilterSegmentDto> Segments);

/// <summary>A segment of the primary ledger's structure, to filter by its value. The account matches by prefix.</summary>
public sealed record JournalFilterSegmentDto(int SegmentNum, string SegmentCode, string SegmentNameEn, bool IsAccount, IReadOnlyList<JournalOptionDto> Values);

/// <summary>Filters of the account ledger (APEX App 120 page 52, process J52_SEARCH).</summary>
public sealed record AccountLedgerRequest(long? LedgerId, string? FromPeriod, string? ToPeriod, string? Q, IReadOnlyDictionary<int, string?>? Segments);

public sealed record AccountLedgerResultDto(
    long? LedgerId,
    string? FromPeriod,
    string? ToPeriod,
    bool Searched,
    decimal BroughtForward,
    decimal TotalDr,
    decimal TotalCr,
    decimal Closing,
    IReadOnlyList<AccountLedgerRowDto> Rows,
    bool More,
    IReadOnlyList<JournalLedgerDto> Ledgers,
    IReadOnlyList<string> Periods,
    IReadOnlyList<JournalFilterSegmentDto> Segments);

/// <summary>One posted movement. Before and after are the running balance of everything the filter matches.</summary>
public sealed record AccountLedgerRowDto(
    DateOnly AccountingDate,
    long JournalHeaderId,
    string JournalNo,
    string JournalName,
    string? TranSetId,
    string ConcatenatedAccount,
    string? Description,
    decimal Dr,
    decimal Cr,
    decimal BalanceBefore,
    decimal BalanceAfter);

/// <summary>Balances of one ledger period (APEX App 120 page 20, GL_ACCOUNT_BALANCE_SUMMARY_V).</summary>
public sealed record AccountBalancesRequest(long? LedgerId, string? Period, string? Q);

public sealed record AccountBalancesResultDto(
    long? LedgerId,
    string? Period,
    IReadOnlyList<AccountBalanceRowDto> Rows,
    decimal TotalOpening,
    decimal TotalDr,
    decimal TotalCr,
    decimal TotalClosing,
    IReadOnlyList<JournalLedgerDto> Ledgers,
    IReadOnlyList<string> Periods);

/// <summary>Opening is every earlier period; amounts are net debit minus credit.</summary>
public sealed record AccountBalanceRowDto(
    long AccountCombinationId,
    string ConcatenatedAccount,
    string AccountCode,
    string AccountNameEn,
    string AccountType,
    string FinancialStatementClass,
    string CurrencyCode,
    decimal Opening,
    decimal PeriodDr,
    decimal PeriodCr,
    decimal Closing,
    string? LastJournalNo,
    DateTime? LastPostedAt);
