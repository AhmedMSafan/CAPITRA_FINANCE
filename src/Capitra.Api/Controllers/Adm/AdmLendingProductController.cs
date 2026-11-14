using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>
/// Administration > Product Setup > Lending Product Terms. Reading needs ADM.PRODUCT.VIEW; preparing a version (create, save, send,
/// take back, discard) ADM.PRODUCT.EDIT; approving or rejecting one ADM.PRODUCT.APPROVE.
/// </summary>
[ApiController]
[Route("api/adm/lending-products")]
public sealed class AdmLendingProductController(LndProductVersionService versions) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<LndProductTermsPageDto> Get(CancellationToken cancellationToken) => Page(cancellationToken);

    [HttpPost("{productId:long}/versions")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public async Task<LndProductTermsPageDto> CreateDraft(long productId, CancellationToken cancellationToken)
    {
        await versions.CreateDraftAsync(productId, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPut("versions/{versionId:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public async Task<LndProductTermsPageDto> Save(long versionId, LndProductVersionSaveDto dto, CancellationToken cancellationToken)
    {
        await versions.SaveDraftAsync(versionId, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("versions/{versionId:long}/submit")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public async Task<LndProductTermsPageDto> Submit(long versionId, CancellationToken cancellationToken)
    {
        await versions.SubmitAsync(versionId, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("versions/{versionId:long}/take-back")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public async Task<LndProductTermsPageDto> TakeBack(long versionId, CancellationToken cancellationToken)
    {
        await versions.TakeBackAsync(versionId, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpDelete("versions/{versionId:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public async Task<LndProductTermsPageDto> Discard(long versionId, CancellationToken cancellationToken)
    {
        await versions.DiscardAsync(versionId, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("versions/{versionId:long}/approve")]
    [HasPermission(Permissions.Adm.ProductApprove)]
    public async Task<LndProductTermsPageDto> Approve(long versionId, LndProductDecisionDto dto, CancellationToken cancellationToken)
    {
        await versions.ApproveAsync(versionId, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("versions/{versionId:long}/reject")]
    [HasPermission(Permissions.Adm.ProductApprove)]
    public async Task<LndProductTermsPageDto> Reject(long versionId, LndProductDecisionDto dto, CancellationToken cancellationToken)
    {
        await versions.RejectAsync(versionId, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    private Task<LndProductTermsPageDto> Page(CancellationToken cancellationToken) =>
        versions.GetAsync(Has(Permissions.Adm.ProductEdit), Has(Permissions.Adm.ProductApprove), cancellationToken);

    private bool Has(string permission) => User.Claims.Any(c =>
        c.Type == Permissions.ClaimType && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
