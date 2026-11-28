using Capitra.Api.Security;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

/// <param name="MaxUsers">The licence's users besides the administrator, so the company administrator sees how many are left.</param>
/// <param name="Quota">The licence's yearly inquiry quota and what is used of it.</param>
public sealed record ScrMyCompanyDto(long MemberCompanyId, string CompanyCode, string NameEn, string NameAr, int? MaxUsers, ScrQuotaDto? Quota = null);

/// <summary>
/// Screening by member companies (SCR.INQUIRY.CREATE): screen, and read one's company's inquiries. Federation auditors
/// (SCR.INQUIRY.AUDIT) read every company's.
/// </summary>
[ApiController]
[Route("api/scr/inquiries")]
public sealed class ScrInquiryController(ScrInquiryService inquiries, CompanyAccess companyAccess) : ControllerBase
{
    [HttpGet("my-company")]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    public async Task<ScrMyCompanyDto> MyCompany(CancellationToken ct)
    {
        var c = await inquiries.MyCompanyAsync(ct);
        var licence = await companyAccess.CurrentLicenceAsync(c.MemberCompanyId, ct);
        return new ScrMyCompanyDto(c.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr, licence?.MaxUsers, await inquiries.QuotaAsync(c.MemberCompanyId, ct));
    }

    [HttpPost]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting(ScrRateLimits.PortalPolicy)]
    public Task<ScrInquiryDto> Screen(ScrInquiryRequest request, CancellationToken ct) => inquiries.ScreenAsync(request, ct);

    [HttpGet("mine")]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    public Task<IReadOnlyList<ScrInquiryDto>> Mine(CancellationToken ct) => inquiries.MineAsync(ct);

    [HttpGet]
    [HasPermission(Permissions.Scr.InquiryAudit)]
    public Task<IReadOnlyList<ScrInquiryDto>> All(long? companyId, string? result, CancellationToken ct) => inquiries.AllAsync(companyId, result, ct);

    /// <summary>The whole chain of sealed inquiries checked (federation audit).</summary>
    [HttpGet("seal-check")]
    [HasPermission(Permissions.Scr.InquiryAudit)]
    public Task<ScrSealCheckDto> SealCheck(CancellationToken ct) => inquiries.SealCheckAsync(ct);

    /// <summary>One inquiry's seal checked: an auditor any; a company user the company's own.</summary>
    [HttpGet("{inquiryId:long}/seal")]
    public async Task<IActionResult> Seal(long inquiryId, CancellationToken ct)
    {
        var audit = ScrClaims.Has(User, Permissions.Scr.InquiryAudit);
        if (!audit && !ScrClaims.Has(User, Permissions.Scr.InquiryCreate))
            return Forbid();
        return Ok(await inquiries.SealAsync(inquiryId, audit, ct));
    }

    /// <summary>One inquiry: an auditor reads any; a company user only the company's own.</summary>
    [HttpGet("{inquiryId:long}")]
    public async Task<IActionResult> Get(long inquiryId, CancellationToken ct)
    {
        var audit = ScrClaims.Has(User, Permissions.Scr.InquiryAudit);
        if (!audit && !ScrClaims.Has(User, Permissions.Scr.InquiryCreate))
            return Forbid();
        return Ok(await inquiries.GetAsync(inquiryId, audit, ct));
    }

    /// <summary>The decisions behind one entry an inquiry found, with their letters' scans: an auditor any; a company user the company's own.</summary>
    [HttpGet("{inquiryId:long}/hits/{partyId:long}/decisions")]
    public async Task<IActionResult> HitDecisions(long inquiryId, long partyId, CancellationToken ct)
    {
        var audit = ScrClaims.Has(User, Permissions.Scr.InquiryAudit);
        if (!audit && !ScrClaims.Has(User, Permissions.Scr.InquiryCreate))
            return Forbid();
        return Ok(await inquiries.HitDecisionsAsync(inquiryId, partyId, audit, ct));
    }

    /// <summary>One scan of a decision behind an inquiry's hit, shown in the browser.</summary>
    [HttpGet("{inquiryId:long}/hits/{partyId:long}/files/{letterFileId:long}")]
    public async Task<IActionResult> HitFile(long inquiryId, long partyId, long letterFileId, CancellationToken ct)
    {
        var audit = ScrClaims.Has(User, Permissions.Scr.InquiryAudit);
        if (!audit && !ScrClaims.Has(User, Permissions.Scr.InquiryCreate))
            return Forbid();
        var f = await inquiries.HitFileAsync(inquiryId, partyId, letterFileId, audit, ct);
        Response.Headers.ContentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = f.FileName }.ToString();
        Response.Headers["X-Content-SHA256"] = f.Sha256Hash;
        return File(f.FileBlob, f.MimeType);
    }
}

/// <summary>The permission check for actions open to either of two permissions ([HasPermission] twice means both).</summary>
internal static class ScrClaims
{
    public static bool Has(System.Security.Claims.ClaimsPrincipal user, string permission) => user.Claims.Any(c => c.Type == Permissions.ClaimType
        && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
