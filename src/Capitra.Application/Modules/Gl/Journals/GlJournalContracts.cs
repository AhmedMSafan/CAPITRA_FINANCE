namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>Lists the journal entry page needs (APEX App 120 page 50, process J50_LOAD).</summary>
public sealed record JournalEntrySetupDto(
    DateOnly Today,
    string? Me,
    long? DefaultLedgerId,
    string DefaultCategory,
    IReadOnlyList<JournalLedgerDto> Ledgers,
    IReadOnlyList<JournalLedgerPeriodDto> Periods,
    IReadOnlyList<JournalOptionDto> Categories,
    IReadOnlyList<JournalStructureDto> Structures);

public sealed record JournalLedgerDto(long LedgerId, string LedgerCode, string LedgerNameEn, string CurrencyCode, long? CoaId, bool IsPrimary);

/// <summary>A ledger period, so the page can show the period of the accounting date before saving.</summary>
public sealed record JournalLedgerPeriodDto(
    long LedgerPeriodId,
    long LedgerId,
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsAdjustmentPeriod,
    string PeriodStatus,
    bool IsManualJournalAllowed);

public sealed record JournalOptionDto(string Code, string Name);

public sealed record JournalStructureDto(long CoaId, string CoaCode, string Delimiter, IReadOnlyList<JournalSegmentDto> Segments);

/// <summary>An enabled segment and the values a journal line may use.</summary>
public sealed record JournalSegmentDto(
    int SegmentNum,
    string SegmentCode,
    string SegmentNameEn,
    bool IsAccount,
    bool IsRequired,
    string? DefaultValueCode,
    string DefaultMode,
    IReadOnlyList<JournalSegmentValueDto> Values);

/// <summary>A value the segment accepts. For an account, which other segments it needs (branch, cost centre, business line).</summary>
public sealed record JournalSegmentValueDto(string Code, string Name, IReadOnlyList<int>? RequiredSegments = null);

public sealed record JournalDto(
    long JournalHeaderId,
    string JournalNo,
    string BatchNo,
    string? TranSetId,
    long LedgerId,
    string LedgerCode,
    long CoaId,
    string PeriodCode,
    string SourceCode,
    string CategoryCode,
    string JournalName,
    string? Description,
    string? ReferenceNo,
    DateOnly JournalDate,
    DateOnly AccountingDate,
    string CurrencyCode,
    string JournalStatus,
    decimal TotalDr,
    decimal TotalCr,
    bool IsReversal,
    JournalLinkDto? ReversalOf,
    JournalLinkDto? ReversedBy,
    string? SourceDocumentType,
    string? SourceDocumentNo,
    string CreatedBy,
    DateTime CreatedAt,
    string? SubmittedBy,
    DateTime? SubmittedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? PostedBy,
    DateTime? PostedAt,
    string? CancelledBy,
    DateTime? CancelledAt,
    string? CancelReason,
    string? RejectReason,
    string? LastPostError,
    JournalAllowedDto Allowed,
    IReadOnlyList<JournalLineDto> Lines,
    IReadOnlyList<JournalImpactDto> Impacts,
    IReadOnlyList<JournalHistoryDto> History,
    int RowVersion);

public sealed record JournalLinkDto(long JournalHeaderId, string JournalNo, string JournalStatus);

/// <summary>What the journal's state allows. The page also checks the user's permissions.</summary>
public sealed record JournalAllowedDto(
    bool Edit,
    bool Submit,
    bool Approve,
    bool Reject,
    bool Post,
    bool Cancel,
    bool Reverse,
    string? ApproveBlockedReason);

public sealed record JournalLineDto(
    int LineNo,
    string ConcatenatedAccount,
    string AccountCode,
    string AccountNameEn,
    IReadOnlyDictionary<int, string> Segments,
    decimal Dr,
    decimal Cr,
    string? Description);

/// <summary>What posting a line did to its account: the balance before and after, including earlier periods.</summary>
public sealed record JournalImpactDto(
    int LineNo,
    string ConcatenatedAccount,
    string AccountNameEn,
    string PeriodCode,
    decimal Dr,
    decimal Cr,
    decimal BalanceBefore,
    decimal BalanceAfter);

public sealed record JournalHistoryDto(string ActionCode, string? OldStatus, string NewStatus, string? Reason, string By, DateTime At);

public sealed record SaveJournalRequest(
    long? LedgerId,
    string? CategoryCode,
    DateOnly? AccountingDate,
    DateOnly? JournalDate,
    string? JournalName,
    string? Description,
    string? ReferenceNo,
    IReadOnlyList<SaveJournalLineRequest>? Lines,
    bool Submit,
    int? RowVersion);

/// <summary>A line: the value of each segment by segment number, and a debit or a credit.</summary>
public sealed record SaveJournalLineRequest(
    IReadOnlyDictionary<int, string?>? Segments,
    decimal? Dr,
    decimal? Cr,
    string? Description);

/// <summary>The saved journal. When the save also submitted and the submit failed, the draft is kept and the reason given.</summary>
public sealed record SaveJournalResultDto(JournalDto Journal, string? SubmitError);

public sealed record JournalActionRequest(string? Reason, DateOnly? ReversalDate, int? RowVersion);

public sealed record JournalActionResultDto(JournalDto Journal, string Message, JournalLinkDto? Created);

public sealed record BulkJournalActionRequest(IReadOnlyList<long>? JournalIds, string? Reason);

public sealed record BulkJournalResultDto(int Succeeded, int Failed, IReadOnlyList<BulkJournalItemDto> Results);

public sealed record BulkJournalItemDto(long JournalHeaderId, string JournalNo, bool Ok, string? Message);
