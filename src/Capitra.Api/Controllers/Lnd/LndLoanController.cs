using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Retail Lending > Loans. Reading needs LND.LOAN.VIEW; previewing, creating and cancelling a loan LND.LOAN.EDIT.</summary>
[ApiController]
[Route("api/lnd/loans")]
public sealed class LndLoanController(LndLoanService loans, LndCfFinanceService workflow, LndLoanDocumentService documents, LndCfRulesService cfRules,
    LndCfClaimService claims) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<IReadOnlyList<LndLoanListDto>> List(CancellationToken cancellationToken) => loans.ListAsync(cancellationToken);

    [HttpGet("{loanId:long}")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndLoanDto> Get(long loanId, CancellationToken cancellationToken) => loans.GetAsync(loanId, cancellationToken);

    [HttpGet("products")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndOptionDto>> Products(CancellationToken cancellationToken) => loans.ProductsAsync(cancellationToken);

    /// <summary>The active Consumer Finance suppliers a finance can be bought from today, with their terms.</summary>
    [HttpGet("cf-suppliers")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndCfSupplierOptionDto>> CfSuppliers(CancellationToken cancellationToken) => loans.CfSuppliersAsync(cancellationToken);

    /// <summary>Rules and GL of New Finance and the CF_FINANCE steps (read only): a step's page, or all with step empty; for New Finance without loanId.</summary>
    [HttpGet("cf-rules")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndCfRulesDto> CfRules([FromQuery] long? loanId, [FromQuery] string? step, [FromQuery] string? charges, [FromQuery] decimal? down,
        CancellationToken cancellationToken)
    {
        // Credit's choices still on the screen: CODE:TIMING:AMOUNT, comma separated
        Dictionary<string, string>? timings = null;
        Dictionary<string, decimal>? amounts = null;
        foreach (var part in (charges ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var p = part.Split(':');
            if (p.Length < 2) continue;
            (timings ??= [])[p[0]] = p[1];
            if (p.Length > 2 && decimal.TryParse(p[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var a))
                (amounts ??= [])[p[0]] = a;
        }
        return cfRules.GetAsync(loanId, step, cancellationToken, timings, amounts, down);
    }

    [HttpGet("goods-categories")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndOptionDto>> GoodsCategories(CancellationToken cancellationToken) => loans.GoodsCategoriesAsync(cancellationToken);

    [HttpGet("scenarios")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndScenarioOptionDto>> Scenarios(CancellationToken cancellationToken) => loans.ScenariosAsync(cancellationToken);

    [HttpGet("clients")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndOptionDto>> Clients([FromQuery] string? q, CancellationToken cancellationToken) => loans.ClientsAsync(q, cancellationToken);

    [HttpPost("preview")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<LndLoanPreviewDto> Preview(LndLoanRequestDto dto, CancellationToken cancellationToken) => loans.PreviewAsync(dto, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndLoanDto> Create(LndLoanRequestDto dto, CancellationToken cancellationToken) =>
        await loans.GetAsync(await loans.CreateAsync(dto, cancellationToken), cancellationToken);

    /// <summary>The CF_FINANCE step waiting now; with inboxId, whether it is the signed-in user's task.</summary>
    [HttpGet("{loanId:long}/cf-step")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndCfStepDto?> CfStep(long loanId, [FromQuery] long? inboxId, [FromQuery] decimal? downPayment, CancellationToken cancellationToken) =>
        workflow.StepAsync(loanId, inboxId, cancellationToken, downPayment);

    /// <summary>
    /// Completes the signed-in user's CF_FINANCE task on the finance: ACCEPT, or REJECT with the reason. At Credit approval it carries
    /// Credit's decision on each charge.
    /// </summary>
    [HttpPost("{loanId:long}/cf-step/{inboxId:long}/complete")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public async Task<LndLoanDto> CompleteCfStep(long loanId, long inboxId, LndCfCompleteRequest request, CancellationToken cancellationToken)
    {
        await workflow.CompleteAsync(loanId, inboxId, request, cancellationToken);
        return await loans.GetAsync(loanId, cancellationToken);
    }

    /// <summary>Charges and claim: creates the upfront claim (the rule's charge GL lines), or NONE when nothing is paid upfront.</summary>
    [HttpPost("{loanId:long}/cf-step/{inboxId:long}/claim")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public async Task<LndLoanDto> CreateCfClaim(long loanId, long inboxId, CancellationToken cancellationToken)
    {
        await workflow.CreateClaimAsync(loanId, inboxId, cancellationToken);
        return await loans.GetAsync(loanId, cancellationToken);
    }

    /// <summary>A consumer finance's upfront claim (lines, payments, GL), read only; 204 when it has none. It is paid in Collection.</summary>
    [HttpGet("{loanId:long}/claim")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndCfClaimDetailDto?> CfClaim(long loanId, CancellationToken cancellationToken) => claims.GetAsync(loanId, cancellationToken);

    /// <summary>The finance's documents (e.g. the signed delivery note).</summary>
    [HttpGet("{loanId:long}/documents")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<IReadOnlyList<LndLoanDocumentDto>> Documents(long loanId, CancellationToken cancellationToken) => documents.ListAsync(loanId, cancellationToken);

    [HttpPost("{loanId:long}/documents")]
    [HasPermission(Permissions.Lnd.LoanView)]
    [RequestSizeLimit(LndLoanDocumentService.MaxFileBytes + 1024 * 1024)]
    public async Task<LndLoanDocumentDto> AddDocument(long loanId, IFormFile file, [FromForm] string? title, [FromForm] string? documentTypeCode, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        return await documents.AddAsync(loanId, documentTypeCode, title, file.FileName, file.ContentType, buffer.ToArray(), cancellationToken);
    }

    [HttpPost("documents/{documentId:long}/remove")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task RemoveDocument(long documentId, CancellationToken cancellationToken) => documents.RemoveAsync(documentId, cancellationToken);

    [HttpGet("documents/{documentId:long}")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public async Task<IActionResult> Document(long documentId, CancellationToken cancellationToken)
    {
        var f = await documents.FileAsync(documentId, cancellationToken);
        Response.Headers.ContentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = f.FileName }.ToString();
        Response.Headers["X-Content-SHA256"] = f.Sha256Hash;
        return File(f.Content, f.MimeType);
    }

    [HttpPost("{loanId:long}/cancel")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndLoanDto> Cancel(long loanId, LndLoanStatusChangeDto dto, CancellationToken cancellationToken)
    {
        await loans.CancelAsync(loanId, dto, cancellationToken);
        return await loans.GetAsync(loanId, cancellationToken);
    }
}
