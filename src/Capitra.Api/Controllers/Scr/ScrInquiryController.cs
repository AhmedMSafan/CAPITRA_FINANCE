using Capitra.Api.Security;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

public sealed record ScrMyCompanyDto(long MemberCompanyId, string CompanyCode, string NameEn, string NameAr);

/// <summary>
/// Screening by member companies (SCR.INQUIRY.CREATE): screen, and read one's company's inquiries. Federation auditors
/// (SCR.INQUIRY.AUDIT) read every company's.
/// </summary>
[ApiController]
[Route("api/scr/inquiries")]
public sealed class ScrInquiryController(ScrInquiryService inquiries) : ControllerBase
{
    [HttpGet("my-company")]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    public async Task<ScrMyCompanyDto> MyCompany(CancellationToken ct)
    {
        var c = await inquiries.MyCompanyAsync(ct);
        return new ScrMyCompanyDto(c.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr);
    }

    [HttpPost]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    public Task<ScrInquiryDto> Screen(ScrInquiryRequest request, CancellationToken ct) => inquiries.ScreenAsync(request, ct);

    [HttpGet("mine")]
    [HasPermission(Permissions.Scr.InquiryCreate)]
    public Task<IReadOnlyList<ScrInquiryDto>> Mine(CancellationToken ct) => inquiries.MineAsync(ct);

    [HttpGet]
    [HasPermission(Permissions.Scr.InquiryAudit)]
    public Task<IReadOnlyList<ScrInquiryDto>> All(long? companyId, string? result, CancellationToken ct) => inquiries.AllAsync(companyId, result, ct);

    /// <summary>One inquiry: an auditor reads any; a company user only the company's own.</summary>
    [HttpGet("{inquiryId:long}")]
    public async Task<IActionResult> Get(long inquiryId, CancellationToken ct)
    {
        var audit = ScrClaims.Has(User, Permissions.Scr.InquiryAudit);
        if (!audit && !ScrClaims.Has(User, Permissions.Scr.InquiryCreate))
            return Forbid();
        return Ok(await inquiries.GetAsync(inquiryId, audit, ct));
    }
}

/// <summary>The permission check for actions open to either of two permissions ([HasPermission] twice means both).</summary>
internal static class ScrClaims
{
    public static bool Has(System.Security.Claims.ClaimsPrincipal user, string permission) => user.Claims.Any(c => c.Type == Permissions.ClaimType
        && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
