using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Collection;

/// <summary>Customer Payments: customer claims paid from the customer's payment sources. Reading needs COLLECTION.VIEW; paying needs COLLECTION.EDIT.</summary>
[ApiController]
[Route("api/collection/claims")]
public sealed class CollectionClaimController(FctCustomerClaimService claims, LndCfClaimService cfClaims) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctClaimRowDto>> List(long? clientId, bool openOnly, CancellationToken cancellationToken) =>
        claims.ListAsync(clientId, openOnly, cancellationToken);

    /// <summary>The customers that have claims to pay: factoring and consumer finance.</summary>
    [HttpGet("customers")]
    [HasPermission(Permissions.Collection.View)]
    public async Task<IReadOnlyList<FctOptionDto>> Customers(CancellationToken cancellationToken) =>
        (await claims.CustomersWithOpenClaimsAsync(cancellationToken)).Concat(await cfClaims.CustomersWithOpenClaimsAsync(cancellationToken))
            .DistinctBy(x => x.Value).OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>A customer's open consumer finance claims (Ahmed, 27 Nov 2026: paid here like the factoring claims).</summary>
    [HttpGet("cf")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctClaimRowDto>> CfList(long clientId, CancellationToken cancellationToken) => cfClaims.OpenClaimsAsync(clientId, cancellationToken);

    [HttpGet("cf/{loanId:long}")]
    [HasPermission(Permissions.Collection.View)]
    public Task<FctClaimDto> CfGet(long loanId, CancellationToken cancellationToken) => cfClaims.CollectionClaimAsync(loanId, cancellationToken);

    [HttpPost("cf/{loanId:long}/pay")]
    [HasPermission(Permissions.Collection.Edit)]
    public Task<FctClaimPaidDto> CfPay(long loanId, FctClaimPayDto dto, CancellationToken cancellationToken) =>
        cfClaims.PayFromSourceAsync(loanId, dto, cancellationToken);

    [HttpGet("{claimId:long}")]
    [HasPermission(Permissions.Collection.View)]
    public Task<FctClaimDto> Get(long claimId, CancellationToken cancellationToken) => claims.GetAsync(claimId, cancellationToken);

    /// <summary>The customer's payment sources with balance left.</summary>
    [HttpGet("sources/{clientId:long}")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctClaimSourceDto>> Sources(long clientId, CancellationToken cancellationToken) =>
        claims.SourcesAsync(clientId, cancellationToken);

    [HttpPost("{claimId:long}/pay")]
    [HasPermission(Permissions.Collection.Edit)]
    public Task<FctClaimPaidDto> Pay(long claimId, FctClaimPayDto dto, CancellationToken cancellationToken) =>
        claims.PayAsync(claimId, dto, cancellationToken);
}
