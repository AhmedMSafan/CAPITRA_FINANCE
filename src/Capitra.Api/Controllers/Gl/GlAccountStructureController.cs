using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.AccountStructure;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>Account structures, segments and segment values (APEX App 120 page 40).</summary>
[ApiController]
[Route("api/gl/account-structures")]
[HasPermission(Permissions.Gl.SetupView)]
public sealed class GlAccountStructureController(IGlAccountStructureService service) : ControllerBase
{
    [HttpGet]
    public Task<AccountStructurePageDto> Get([FromQuery] long? coaId, CancellationToken cancellationToken) =>
        service.GetAsync(coaId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<CoaStructureDto> CreateStructure(SaveCoaStructureRequest request, CancellationToken cancellationToken) =>
        service.CreateStructureAsync(request, cancellationToken);

    [HttpPut("{coaId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<CoaStructureDto> UpdateStructure(long coaId, SaveCoaStructureRequest request, CancellationToken cancellationToken) =>
        service.UpdateStructureAsync(coaId, request, cancellationToken);

    [HttpPost("{coaId:long}/segments")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<CoaSegmentDto> CreateSegment(long coaId, SaveCoaSegmentRequest request, CancellationToken cancellationToken) =>
        service.CreateSegmentAsync(coaId, request, cancellationToken);

    [HttpPut("segments/{coaSegmentId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<CoaSegmentDto> UpdateSegment(long coaSegmentId, SaveCoaSegmentRequest request, CancellationToken cancellationToken) =>
        service.UpdateSegmentAsync(coaSegmentId, request, cancellationToken);

    /// <summary>Sets or removes the value filled in on every new journal line.</summary>
    [HttpPut("segments/{coaSegmentId:long}/default")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public async Task<IActionResult> SaveDefault(long coaSegmentId, SaveSegmentDefaultRequest request, CancellationToken cancellationToken)
    {
        await service.SaveDefaultAsync(coaSegmentId, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("segments/{coaSegmentId:long}/values")]
    public Task<IReadOnlyList<SegmentValueDto>> GetValues(long coaSegmentId, CancellationToken cancellationToken) =>
        service.GetValuesAsync(coaSegmentId, cancellationToken);

    [HttpPost("segments/{coaSegmentId:long}/values")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<SegmentValueDto> CreateValue(long coaSegmentId, SaveSegmentValueRequest request, CancellationToken cancellationToken) =>
        service.CreateValueAsync(coaSegmentId, request, cancellationToken);

    [HttpPut("segment-values/{segmentValueId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<SegmentValueDto> UpdateValue(long segmentValueId, SaveSegmentValueRequest request, CancellationToken cancellationToken) =>
        service.UpdateValueAsync(segmentValueId, request, cancellationToken);
}
