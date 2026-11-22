namespace Capitra.Application.Modules.Scr;

/// <summary>A signed-in user as screening needs them: the person behind the account, and the member company if any.</summary>
public sealed record ScrUserProfile(string UserId, string UserName, string? FullName, string? EmployeeNo, long? MemberCompanyId);

/// <summary>Reads users for screening (the identity tables live in Infrastructure).</summary>
public interface IScrUserProfiles
{
    Task<ScrUserProfile?> ByUserNameAsync(string userName, CancellationToken cancellationToken);
}

public sealed record ScrLetterFileDto(long LetterFileId, string FileName, string MimeType, long FileSizeBytes, string Sha256Hash);

public sealed record ScrLetterDto(
    long LetterId, string LetterNo, string? SenderReference, string SenderName, string ChannelCode, DateOnly LetterDate, DateTime ReceivedAt,
    string StatusCode, string? Notes, IReadOnlyList<ScrLetterFileDto> Files, int DecisionCount, string? CreatedBy = null);

public sealed record ScrLetterRequest(string? SenderReference, string? SenderName, string? ChannelCode, DateOnly? LetterDate, string? Notes);

public sealed record ScrPartyDto(
    long PartyId, string PartyTypeCode, string NameAr, string? NameEn, string IdentityTypeCode, string IdentityNo, string NationalityCode,
    DateOnly? BirthDate, string? Notes, IReadOnlyList<string> Aliases, string? CreatedBy = null);

public sealed record ScrPartyRequest(
    string? PartyTypeCode, string? NameAr, string? NameEn, string? IdentityTypeCode, string? IdentityNo, string? NationalityCode,
    DateOnly? BirthDate, string? Notes, IReadOnlyList<string>? Aliases);

public sealed record ScrDecisionDto(
    long DecisionId, string DecisionRef, string OfficialNo, string DecisionTypeCode, long? ParentDecisionId, string? ParentDecisionRef,
    long? LetterId, string? LetterNo, string ScopeText, DateOnly EffectiveDate, string StatusCode, string? ReturnReason, long? WorkflowInstanceId,
    string EnteredBy, string? ReviewedBy, DateTime? ReviewedAt, string? ApprovedBy, DateTime? PublishedAt, int? RegistryVersionNo,
    IReadOnlyList<ScrPartyDto> Parties, DateTime CreatedAt, string? WorkflowStepName = null,
    IReadOnlyList<ScrCompletedStepDto>? CompletedSteps = null, string? WorkflowStepHolder = null);

public sealed record ScrDecisionRequest(
    string? OfficialNo, string? DecisionTypeCode, long? ParentDecisionId, long? LetterId, string? ScopeText, DateOnly? EffectiveDate,
    IReadOnlyList<long>? PartyIds);

public sealed record ScrRegistryDto(int VersionNo, DateTime? PublishedAt, int ActiveCount, int SuspendedCount, int LiftedCount, int PendingCount,
    IReadOnlyList<ScrRegistryEntry> Entries);

public sealed record ScrInquiryRequest(
    string? SubjectTypeCode, string? SubjectName, string? IdentityTypeCode, string? IdentityNo, DateOnly? BirthDate, string? NationalityCode,
    string? PurposeCode, string? PurposeText, string? CaseReference);

public sealed record ScrInquiryHitDto(
    string MatchBasisCode, decimal NameScorePct, string MatchedName, string EntryStatusCode, string PartyTypeCode, string NameAr, string? NameEn,
    string IdentityTypeCode, string IdentityNo, string NationalityCode, string DecisionRef, string OfficialNo, string ScopeText,
    DateOnly ListedFrom, DateOnly? LiftedOn);

public sealed record ScrInquiryDto(
    long InquiryId, string InquiryRef, string CompanyCode, string RequestedBy, string SubjectTypeCode, string SubjectName, string IdentityTypeCode,
    string IdentityNo, DateOnly? BirthDate, string? NationalityCode, string PurposeCode, string? PurposeText, string? CaseReference,
    string ResultCode, int RegistryVersionNo, DateTime ScreenedAt, IReadOnlyList<ScrInquiryHitDto> Hits, string ChannelCode = "PORTAL",
    DateTime? ReceivedAt = null, DateTime? RespondedAt = null, string? RecordHash = null, long MemberCompanyId = 0, string? CompanyNameEn = null,
    string? CompanyNameAr = null);

/// <summary>One inquiry's seal checked: its hash recomputed, and its link to the sealed inquiry before it.</summary>
public sealed record ScrInquirySealDto(string InquiryRef, bool Sealed, bool Valid, string? RecordHash, string? PrevHash, string Message, DateTime CheckedAt);

/// <summary>The whole chain checked: sealed inquiries checked, the broken ones, and inquiries saved without a seal after sealing began.</summary>
public sealed record ScrSealCheckDto(
    int Checked, int Broken, IReadOnlyList<string> BrokenRefs, int UnsealedAfterStart, IReadOnlyList<string> UnsealedRefs, int UnsealedBeforeStart,
    string? FirstSealedRef, string? LastSealedRef, DateTime CheckedAt);

public sealed record ScrCompanyDto(
    long MemberCompanyId, string CompanyCode, string NameEn, string NameAr, string? LicenceNo, string? ContactEmail, string StatusCode, string? Notes,
    int UserCount, int InquiryCount, string? CreatedBy = null,
    ScrLicenceDto? Licence = null, ScrCompanySeats? Seats = null, string? LicenceStateCode = null,
    long? PricePlanId = null, string? PricePlanCode = null, string BillingCycleCode = "MONTHLY");

/// <summary>A licence row: the administrator plus MaxUsers users, the period, the yearly inquiry quota.</summary>
public sealed record ScrLicenceDto(
    long LicenceId, int MaxUsers, DateOnly ValidFrom, DateOnly? ValidTo, int? YearlyInquiryQuota, string? Notes, string CreatedBy, DateTime CreatedAt);

public sealed record ScrLicenceRequest(int? MaxUsers, DateOnly? ValidFrom, DateOnly? ValidTo, int? YearlyInquiryQuota, string? Notes);

/// <summary>
/// A company's accounts against its licence: its administrator (AdminUserName null = none yet), and UsersInUse = active company
/// users besides the administrator, pending activation included (each holds a seat).
/// </summary>
public sealed record ScrCompanySeats(string? AdminUserName, string? AdminEmail, string? AdminStatusCode, int UsersInUse);

/// <param name="PricePlanId">The price plan the company is billed by; empty = not billed yet.</param>
/// <param name="BillingCycleCode">MONTHLY, QUARTERLY or YEARLY; empty = MONTHLY.</param>
public sealed record ScrCompanyRequest(
    string? CompanyCode, string? NameEn, string? NameAr, string? LicenceNo, string? ContactEmail, string? StatusCode, string? Notes,
    long? PricePlanId = null, string? BillingCycleCode = null);

/// <summary>The licence's yearly inquiry quota (null: none), used and left in the licence year From..To.</summary>
public sealed record ScrQuotaDto(int? Quota, int Used, int? Left, DateOnly From, DateOnly To);

/// <summary>The company used its yearly inquiry quota: the portal shows the message, the API answers 403 quota_reached.</summary>
public sealed class ScrQuotaExceededException(string message) : Capitra.Application.Common.Exceptions.BusinessRuleException(message);
