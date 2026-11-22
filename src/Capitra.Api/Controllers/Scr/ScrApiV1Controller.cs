using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Scr;
using Capitra.Domain.Modules.Scr;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Scr;

/// <summary>Client credentials, exchanged for a token.</summary>
public sealed record ScrApiTokenRequest(string? ClientId, string? ClientSecret);

public sealed record ScrApiTokenResponse(string AccessToken, string TokenType, int ExpiresIn, DateTime ExpiresAt);

public sealed record ScrApiError(string Error, string Message);

/// <summary>A Final List entry the subject matched.</summary>
public sealed record ScrApiHit(
    string MatchBasis, decimal NameScore, string MatchedName, string EntryStatus, string PartyType, string NameAr, string? NameEn, string IdentityType,
    string IdentityNo, string Nationality, string DecisionRef, string OfficialNo, string Scope, DateOnly ListedFrom, DateOnly? LiftedOn);

/// <summary>One screening, as the API answers it.</summary>
public sealed record ScrApiInquiry(
    string InquiryRef, string Result, int ListVersion, DateTime ScreenedAt, DateTime? ReceivedAt, DateTime? RespondedAt, string Channel,
    string SubjectType, string SubjectName, string IdentityType, string IdentityNo, DateOnly? BirthDate, string? Nationality, string Purpose,
    string? PurposeText, string? CaseReference, IReadOnlyList<ScrApiHit> Hits, string? RecordHash)
{
    public static ScrApiInquiry From(ScrInquiryDto i) => new(
        i.InquiryRef, i.ResultCode, i.RegistryVersionNo, i.ScreenedAt, i.ReceivedAt, i.RespondedAt, i.ChannelCode, i.SubjectTypeCode, i.SubjectName,
        i.IdentityTypeCode, i.IdentityNo, i.BirthDate, i.NationalityCode, i.PurposeCode, i.PurposeText, i.CaseReference,
        [.. i.Hits.Select(h => new ScrApiHit(h.MatchBasisCode, h.NameScorePct, h.MatchedName, h.EntryStatusCode, h.PartyTypeCode, h.NameAr, h.NameEn,
            h.IdentityTypeCode, h.IdentityNo, h.NationalityCode, h.DecisionRef, h.OfficialNo, h.ScopeText, h.ListedFrom, h.LiftedOn))], i.RecordHash);
}

/// <summary>
/// Screening API v1, for member companies' own systems. POST token with the API client's ID and secret; then send the token as
/// "Authorization: Bearer ..." to screen a person or company, read an inquiry by its reference, or list the company's inquiries. API
/// inquiries are kept, billed and audited like the Screening Portal's; the client must be active and the company's licence in force.
/// </summary>
[ApiController]
[Route("api/scr/v1")]
[Authorize(AuthenticationSchemes = ScrApiAuth.Scheme)]
public sealed class ScrApiV1Controller(
    ScrApiClientService clients, ScrInquiryService inquiries, CompanyAccess companyAccess, CapitraDbContext db, JwtTokenService tokens, IClock clock) : ControllerBase
{
    /// <summary>Client credentials for a token (an hour). The same refusal for an unknown client, a wrong secret or a revoked client.</summary>
    [HttpPost("token")]
    [AllowAnonymous]
    [EnableRateLimiting(ScrRateLimits.TokenPolicy)]
    [ProducesResponseType<ScrApiTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ScrApiError>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ScrApiError>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Token(ScrApiTokenRequest request, CancellationToken ct)
    {
        var client = await clients.VerifyAsync(request.ClientId, request.ClientSecret, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (client is null)
            return Unauthorized(new ScrApiError("invalid_client", "The client ID or secret is wrong, or the client is revoked."));
        if (await companyAccess.BlockReasonAsync(client.MemberCompanyId, ct) is { } blocked)
            return StatusCode(StatusCodes.Status403Forbidden, new ScrApiError("company_blocked", blocked));
        var issued = tokens.Issue(
        [
            new Claim(CapitraClaimTypes.UserId, "api-client-" + client.ApiClientId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(CapitraClaimTypes.UserName, "api:" + client.ClientId),
            new Claim(ScrApiAuth.ClientClaim, client.ApiClientId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(ScrApiAuth.CompanyClaim, client.MemberCompanyId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ], ScrApiAuth.Audience, ScrApiAuth.TokenMinutes);
        return Ok(new ScrApiTokenResponse(issued.Token, "Bearer", ScrApiAuth.TokenMinutes * 60, issued.ExpiresAt));
    }

    /// <summary>Screens one person or company against the Federation Final List; the inquiry is kept and its result returned.</summary>
    [HttpPost("inquiries")]
    [EnableRateLimiting(ScrRateLimits.ApiPolicy)]
    [ProducesResponseType<ScrApiInquiry>(StatusCodes.Status200OK)]
    [ProducesResponseType<ScrApiError>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ScrApiError>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Screen(ScrInquiryRequest request, CancellationToken ct)
    {
        var receivedAt = clock.UtcNow;
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().FirstAsync(c => c.MemberCompanyId == ScrApiAuth.CompanyOf(User), ct);
        try
        {
            var dto = await inquiries.ScreenForAsync(company, User.Identity!.Name!, null, "API", ScrApiAuth.ClientOf(User), receivedAt, request, ct);
            return Ok(ScrApiInquiry.From(dto));
        }
        catch (ScrQuotaExceededException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ScrApiError("quota_reached", ex.Message));
        }
    }

    /// <summary>The licence's yearly inquiry quota: the quota (null: none), used and left, and the licence year.</summary>
    [HttpGet("quota")]
    [EnableRateLimiting(ScrRateLimits.ApiPolicy)]
    public async Task<IActionResult> Quota(CancellationToken ct) =>
        await inquiries.QuotaAsync(ScrApiAuth.CompanyOf(User), ct) is { } q ? Ok(q) : NotFound(new ScrApiError("no_licence", "Your company has no licence in force."));

    /// <summary>One of the company's inquiries by its reference (SCR-2026-000123), from the API or the portal.</summary>
    [HttpGet("inquiries/{inquiryRef}")]
    [EnableRateLimiting(ScrRateLimits.ApiPolicy)]
    [ProducesResponseType<ScrApiInquiry>(StatusCodes.Status200OK)]
    [ProducesResponseType<ScrApiError>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string inquiryRef, CancellationToken ct) =>
        await inquiries.ByRefAsync(ScrApiAuth.CompanyOf(User), inquiryRef, ct) is { } i
            ? Ok(ScrApiInquiry.From(i))
            : NotFound(new ScrApiError("not_found", $"Inquiry {inquiryRef} was not found for your company."));

    /// <summary>The company's inquiries screened between two days (Cairo time, inclusive), newest first, at most 500 (default 100).</summary>
    [HttpGet("inquiries")]
    [EnableRateLimiting(ScrRateLimits.ApiPolicy)]
    public async Task<IReadOnlyList<ScrApiInquiry>> List(DateOnly? from, DateOnly? to, int? take, CancellationToken ct) =>
        [.. (await inquiries.OfCompanyAsync(ScrApiAuth.CompanyOf(User), from, to, take ?? 100, ct)).Select(ScrApiInquiry.From)];
}

/// <summary>The Federation's management of member companies' API clients (SCR.COMPANY.ADMIN).</summary>
[ApiController]
[Route("api/scr/companies/{companyId:long}/api-clients")]
public sealed class ScrApiClientsController(ScrApiClientService clients, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : ControllerBase
{
    [HttpGet]
    [HasPermission(Application.Security.Permissions.Scr.CompanyAdmin)]
    public Task<IReadOnlyList<ScrApiClientDto>> List(long companyId, CancellationToken ct) => clients.ListAsync(companyId, ct);

    /// <summary>A new client; its secret is in the answer, this once.</summary>
    [HttpPost]
    [HasPermission(Application.Security.Permissions.Scr.CompanyAdmin)]
    public Task<ScrApiClientSecretDto> Create(long companyId, ScrApiClientRequest request, CancellationToken ct) => clients.CreateAsync(companyId, request, ct);

    /// <summary>A new secret for the client (the old one stops working); in the answer, this once.</summary>
    [HttpPost("{apiClientId:long}/secret")]
    [HasPermission(Application.Security.Permissions.Scr.CompanyAdmin)]
    public Task<ScrApiClientSecretDto> NewSecret(long companyId, long apiClientId, CancellationToken ct) => clients.NewSecretAsync(companyId, apiClientId, ct);

    [HttpPost("{apiClientId:long}/revoke")]
    [HasPermission(Application.Security.Permissions.Scr.CompanyAdmin)]
    public async Task<ScrApiClientDto> Revoke(long companyId, long apiClientId, CancellationToken ct)
    {
        var dto = await clients.RevokeAsync(companyId, apiClientId, ct);
        ScrApiAuth.Forget(cache, apiClientId);
        return dto;
    }
}
