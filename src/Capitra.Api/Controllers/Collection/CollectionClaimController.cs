using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Collection;

/// <summary>Customer Payments: customer claims paid from the customer's payment sources. Reading needs COLLECTION.VIEW; paying needs COLLECTION.EDIT.</summary>
[ApiController]
[Route("api/collection/claims")]
public sealed class CollectionClaimController(FctCustomerClaimService claims) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctClaimRowDto>> List(long? clientId, bool openOnly, CancellationToken cancellationToken) =>
        claims.ListAsync(clientId, openOnly, cancellationToken);

    /// <summary>The customers that have claims to pay.</summary>
    [HttpGet("customers")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctOptionDto>> Customers(CancellationToken cancellationToken) => claims.CustomersWithOpenClaimsAsync(cancellationToken);

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
