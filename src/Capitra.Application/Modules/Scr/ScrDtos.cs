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
    string StatusCode, string? Notes, IReadOnlyList<ScrLetterFileDto> Files, int DecisionCount);

public sealed record ScrLetterRequest(string? SenderReference, string? SenderName, string? ChannelCode, DateOnly? LetterDate, string? Notes);

public sealed record ScrPartyDto(
    long PartyId, string PartyTypeCode, string NameAr, string? NameEn, string IdentityTypeCode, string IdentityNo, string NationalityCode,
    DateOnly? BirthDate, string? Notes, IReadOnlyList<string> Aliases);

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
    string ResultCode, int RegistryVersionNo, DateTime ScreenedAt, IReadOnlyList<ScrInquiryHitDto> Hits);

public sealed record ScrCompanyDto(
    long MemberCompanyId, string CompanyCode, string NameEn, string NameAr, string? LicenceNo, string? ContactEmail, string StatusCode, string? Notes,
    int UserCount, int InquiryCount);

public sealed record ScrCompanyRequest(string? CompanyCode, string? NameEn, string? NameAr, string? LicenceNo, string? ContactEmail, string? StatusCode, string? Notes);
