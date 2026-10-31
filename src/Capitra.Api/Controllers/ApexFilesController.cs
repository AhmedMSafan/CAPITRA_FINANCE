using Capitra.Application.Modules.Apex;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers;

/// <summary>
/// The upload of a File Browse item of a copied APEX page: the file goes to the user's APEX session
/// (APEX_APPLICATION_TEMP_FILES) and its name becomes the item's value, as APEX did on page submit.
/// </summary>
[ApiController]
[Authorize]
[Route("api/apex/{app:int}/{page:int}/files")]
public sealed class ApexFilesController(ApexSession session) : ControllerBase
{
    private const long MaxBytes = 25 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxBytes)]
    public async Task<IActionResult> Upload(int app, int page, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaxBytes)
            return BadRequest(new { message = "The file is empty or larger than 25 MB." });
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        return Ok(new { name = session.AddFile(file.FileName, file.ContentType, buffer.ToArray()) });
    }
}
