using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Fct;

/// <summary>A user the next workflow task can be assigned to.</summary>
public sealed record FctUserDto(string Id, string? UserName, string? Email, string? FullName, string? EmployeeNo);

/// <summary>What the factoring workflow step pages read: the agreement summary, and the users a next task can go to.</summary>
[ApiController]
[Route("api/fct")]
public sealed class FctReviewController(FctReviewService review, UserManager<SecUser> users) : ControllerBase
{
    [HttpGet("agreements/{agreementId:long}/review-summary")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctAgreementReviewSummaryDto> ReviewSummary(long agreementId, CancellationToken cancellationToken) =>
        review.ReviewSummaryAsync(agreementId, cancellationToken);

    /// <summary>FINOVA read /api/admin/users here; the active users are enough, and need no user-admin right.</summary>
    [HttpGet("users")]
    [HasPermission(Permissions.Fct.Edit)]
    public async Task<IReadOnlyList<FctUserDto>> Users(CancellationToken cancellationToken) =>
        await users.Users.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.UserName)
            .Select(x => new FctUserDto(x.Id, x.UserName, x.Email, x.FullName, x.EmployeeNo))
            .ToListAsync(cancellationToken);
}
