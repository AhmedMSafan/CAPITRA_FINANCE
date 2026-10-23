using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Investors (APEX App 105 pages 109/110).</summary>
[ApiController]
[Route("api/mtg/investors")]
[HasPermission(Permissions.Mtg.MasterDataView)]
public sealed class MtgInvestorController(IMtgInvestorService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<InvestorDto>> List(CancellationToken cancellationToken) =>
        service.ListAsync(cancellationToken);

    [HttpGet("{investorId:long}")]
    public Task<InvestorDto> Get(long investorId, CancellationToken cancellationToken) =>
        service.GetAsync(investorId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<ActionResult<InvestorDto>> Create(SaveInvestorRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { investorId = created.InvestorId }, created);
    }

    [HttpPut("{investorId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public Task<InvestorDto> Update(long investorId, SaveInvestorRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(investorId, request, cancellationToken);

    [HttpDelete("{investorId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<IActionResult> Delete(long investorId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(investorId, rowVersion, cancellationToken);
        return NoContent();
    }
}
