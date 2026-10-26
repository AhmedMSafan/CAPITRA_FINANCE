using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>The account ledger (APEX App 120 page 52) and account balances (page 20).</summary>
[ApiController]
[Route("api/gl")]
[HasPermission(Permissions.Gl.JournalView)]
public sealed class GlAccountActivityController(IGlJournalQueryService queries) : ControllerBase
{
    [HttpGet("account-ledger")]
    public Task<AccountLedgerResultDto> AccountLedger(
        [FromQuery] long? ledgerId, [FromQuery] string? from, [FromQuery] string? to, [FromQuery] string? q,
        [FromQuery] string? segments, CancellationToken cancellationToken) =>
        queries.AccountLedgerAsync(new AccountLedgerRequest(ledgerId, from, to, q, GlJournalsController.ParseSegments(segments)), cancellationToken);

    [HttpGet("account-balances")]
    public Task<AccountBalancesResultDto> Balances(
        [FromQuery] long? ledgerId, [FromQuery] string? period, [FromQuery] string? q, CancellationToken cancellationToken) =>
        queries.BalancesAsync(new AccountBalancesRequest(ledgerId, period, q), cancellationToken);
}
