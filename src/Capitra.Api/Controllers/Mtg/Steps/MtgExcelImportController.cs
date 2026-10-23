using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Excel Import &amp; Validation" (APEX App 105 page 208).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/excel-import")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgExcelImportController(IMtgExcelImportService service) : ControllerBase
{
    private const long MaxWorkbookBytes = 20 * 1024 * 1024;

    [HttpGet]
    public Task<ExcelImportDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Checks the workbook and records the result. Nothing is loaded into the portfolio.</summary>
    [HttpPost("validate")]
    [HasPermission(Permissions.Mtg.StepExcelImport)]
    [RequestSizeLimit(MaxWorkbookBytes)]
    public async Task<WorkbookResultDto> Validate(long portfolioId, IFormFile? file, CancellationToken cancellationToken)
    {
        await using var content = Open(file);
        return await service.ValidateAsync(portfolioId, file!.FileName, content, cancellationToken);
    }

    /// <summary>Checks the workbook again and, when it has no errors, loads it and completes the step.</summary>
    [HttpPost("import")]
    [HasPermission(Permissions.Mtg.StepExcelImport)]
    [RequestSizeLimit(MaxWorkbookBytes)]
    public async Task<WorkbookResultDto> Import(long portfolioId, IFormFile? file, CancellationToken cancellationToken)
    {
        await using var content = Open(file);
        return await service.ImportAsync(portfolioId, file!.FileName, content, cancellationToken);
    }

    /// <summary>The upload as a seekable stream (the Excel reader needs to seek).</summary>
    private static MemoryStream Open(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw BusinessValidationException.For("file", "Excel file is required.");

        var buffer = new MemoryStream((int)file.Length);
        file.CopyTo(buffer);
        buffer.Position = 0;
        return buffer;
    }
}
