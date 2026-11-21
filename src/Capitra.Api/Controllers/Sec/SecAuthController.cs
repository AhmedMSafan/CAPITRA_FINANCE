using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Sec;

public sealed record LoginRequest([Required] string UserName, [Required] string Password);

public sealed record CurrentUserDto(
    string UserId,
    string UserName,
    string? FullName,
    string? EmployeeNo,
    string? Department,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

/// <summary>MustChangePassword: the token only allows changing the password (a temporary password was used).</summary>
public sealed record LoginResponse(string Token, DateTime ExpiresAt, CurrentUserDto User, bool MustChangePassword = false);

public sealed record ChangePasswordRequest([Required] string CurrentPassword, [Required] string NewPassword);

public sealed record AccountLinkRequest([Required] string UserId, [Required] string Token);

public sealed record AccountLinkPasswordRequest([Required] string UserId, [Required] string Token, [Required] string NewPassword);

/// <summary>Activating: the account has no password yet (an activation link), otherwise a password reset.</summary>
public sealed record AccountLinkInfo(string UserName, string? FullName, bool Activating);

public sealed record ForgotPasswordRequest(string? UserNameOrEmail);

[ApiController]
[Route("api/sec/auth")]
public sealed class SecAuthController(
    UserManager<SecUser> users,
    RoleManager<SecRole> roles,
    JwtTokenService tokens,
    IClock clock,
    ICurrentUser currentUser,
    UserAccessCache access,
    CompanyAccess companyAccess,
    AccountEmails emails,
    CapitraDbContext db) : ControllerBase
{
    private const string InvalidLogin = "Invalid user name or password.";

    /// <summary>The only anonymous endpoint: exchanges user name and password for a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // every attempt is recorded (SEC_SIGN_INS), with why it was refused; the user is shown the same message as before
        var user = await users.FindByNameAsync(request.UserName);
        if (user is null || !user.IsActive)
            return await RefuseAsync(request.UserName, user, user is null ? "UNKNOWN_USER" : "INACTIVE", InvalidLogin);

        if (await users.IsLockedOutAsync(user))
            return await RefuseAsync(request.UserName, user, "LOCKED", "The account is locked. Try again later.");

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            return await RefuseAsync(request.UserName, user, "BAD_PASSWORD", InvalidLogin);
        }

        // a member company's users: only while the company is active and its licence covers today
        if (user.MemberCompanyId is { } companyId && await companyAccess.BlockReasonAsync(companyId, HttpContext.RequestAborted) is { } blocked)
            return await RefuseAsync(request.UserName, user, "COMPANY_BLOCKED", blocked);

        await users.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = clock.UtcNow;
        await users.UpdateAsync(user);

        // a successful sign-in opens a session: its id goes in the token, so what is done in it points back to this sign-in
        var sessionId = Guid.NewGuid().ToString("N");
        var response = await IssueAsync(user, sessionId);
        var signIn = NewSignIn(request.UserName, user, "SUCCESS", null);
        signIn.SessionId = sessionId;
        signIn.ExpiresAt = response.ExpiresAt;
        db.Add(signIn);
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return response;
    }

    /// <summary>Ends the session: the token is dropped by the browser and the sign-in log shows when the user left.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (currentUser.SessionId is { } sid
            && await db.Set<SecSignIn>().FirstOrDefaultAsync(s => s.SessionId == sid, HttpContext.RequestAborted) is { SignedOutAt: null } session)
        {
            session.SignedOutAt = clock.UtcNow;
            await db.SaveChangesAsync(HttpContext.RequestAborted);
            // the token is refused from now on, not only when it expires
            access.Clear();
        }

        return NoContent();
    }

    private async Task<ActionResult<LoginResponse>> RefuseAsync(string userName, SecUser? user, string resultCode, string message)
    {
        db.Add(NewSignIn(userName, user, resultCode, message));
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return Unauthorized(Problem(message));
    }

    private SecSignIn NewSignIn(string userName, SecUser? user, string resultCode, string? failure)
    {
        var agent = Request.Headers.UserAgent.ToString();
        var name = (userName ?? string.Empty).Trim();
        return new SecSignIn
        {
            UserName = name.Length > 256 ? name[..256] : name, UserId = user?.Id, MemberCompanyId = user?.MemberCompanyId, AttemptedAt = clock.UtcNow,
            ResultCode = resultCode, FailureText = failure, IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = string.IsNullOrEmpty(agent) ? null : agent.Length > 500 ? agent[..500] : agent,
        };
    }

    /// <summary>
    /// Whether an activation or password-reset link is still good, and whose it is (the page greets the user by name). A used,
    /// replaced or expired link answers 400.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("link/check")]
    public async Task<ActionResult<AccountLinkInfo>> CheckLink(AccountLinkRequest request)
    {
        var user = await LinkUserAsync(request.UserId, request.Token);
        return new AccountLinkInfo(user.UserName ?? string.Empty, user.FullName, user.PasswordHash is null);
    }

    /// <summary>
    /// The user sets their own password from an activation or password-reset link. The link works once (setting the password
    /// voids it) and every open session ends. The user then signs in.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("link/set-password")]
    public async Task<IActionResult> SetPasswordFromLink(AccountLinkPasswordRequest request)
    {
        var user = await LinkUserAsync(request.UserId, request.Token);
        var result = await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var invalidToken = result.Errors.Any(e => e.Code == "InvalidToken");
            throw BusinessValidationException.For(invalidToken ? "token" : "newPassword",
                invalidToken ? LinkInvalid : string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        user.EmailConfirmed = true;
        user.MustChangePassword = false;
        user.PasswordChangedAt = clock.UtcNow;
        await users.UpdateAsync(user);
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
        access.Clear();
        return NoContent();
    }

    /// <summary>
    /// Forgot password: e-mails a reset link (or, to an account never activated, a new activation link). The answer is the same
    /// whether or not the account exists, so it cannot be used to find user names.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var key = request.UserNameOrEmail?.Trim() ?? string.Empty;
        var user = key.Length == 0 ? null : await users.FindByNameAsync(key) ?? await users.FindByEmailAsync(key);
        if (user is { IsActive: true } && !string.IsNullOrWhiteSpace(user.Email))
        {
            if (user.PasswordHash is null)
                await emails.SendActivationAsync(user, HttpContext.RequestAborted);
            else
                await emails.SendPasswordResetAsync(user, HttpContext.RequestAborted);
        }

        return Ok(new { message = "If the account exists and has an e-mail address, a link has been sent to it." });
    }

    private const string LinkInvalid = "This link is not valid any more: it was used, replaced by a newer one, or it expired. Ask for a new one.";

    private async Task<SecUser> LinkUserAsync(string? userId, string? token)
    {
        var user = string.IsNullOrWhiteSpace(userId) ? null : await users.FindByIdAsync(userId);
        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(token)
            || !await users.VerifyUserTokenAsync(user, users.Options.Tokens.PasswordResetTokenProvider, UserManager<SecUser>.ResetPasswordTokenPurpose, token))
            throw BusinessValidationException.For("token", LinkInvalid);
        return user;
    }

    /// <summary>
    /// The signed-in user replaces their password (required after a temporary one). The current password is checked (a wrong
    /// one counts towards lock-out), the new one must differ and meet the policy. Every earlier token stops working; the reply
    /// carries a new token.
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<LoginResponse>> ChangePassword(ChangePasswordRequest request)
    {
        var user = currentUser.UserId is null ? null : await users.FindByIdAsync(currentUser.UserId);
        if (user is null || !user.IsActive)
            return Unauthorized();
        if (await users.IsLockedOutAsync(user))
            return Unauthorized(Problem("The account is locked. Try again later."));
        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
            throw BusinessValidationException.For("newPassword", "The new password must be different from the current one.");
        if (!await users.CheckPasswordAsync(user, request.CurrentPassword))
        {
            await users.AccessFailedAsync(user);
            throw BusinessValidationException.For("currentPassword", "The current password is not correct.");
        }

        var changed = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changed.Succeeded)
            throw BusinessValidationException.For("newPassword", string.Join(" ", changed.Errors.Select(e => e.Description)));

        await users.ResetAccessFailedCountAsync(user);
        user.MustChangePassword = false;
        user.PasswordChangedAt = clock.UtcNow;
        await users.UpdateAsync(user);
        // the cached security stamp is now stale: drop it so the old session ends and the new one works at once
        access.Clear();
        // the same session goes on with a new token
        var sessionId = currentUser.SessionId ?? Guid.NewGuid().ToString("N");
        var response = await IssueAsync(user, sessionId);
        if (await db.Set<SecSignIn>().FirstOrDefaultAsync(s => s.SessionId == sessionId, HttpContext.RequestAborted) is { } session)
        {
            session.ExpiresAt = response.ExpiresAt;
            await db.SaveChangesAsync(HttpContext.RequestAborted);
        }

        return response;
    }

    private async Task<LoginResponse> IssueAsync(SecUser user, string sessionId)
    {
        var profile = await BuildProfileAsync(user);
        var claims = new List<Claim>
        {
            new(CapitraClaimTypes.UserId, user.Id),
            new(CapitraClaimTypes.UserName, user.UserName!),
            new(UserAccessCheck.SecurityStampClaim, user.SecurityStamp ?? string.Empty),
            new(CapitraClaimTypes.SessionId, sessionId),
        };
        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim(CapitraClaimTypes.FullName, user.FullName));
        claims.AddRange(profile.Roles.Select(r => new Claim(CapitraClaimTypes.Role, r)));
        claims.AddRange(profile.Permissions.Select(p => new Claim(Permissions.ClaimType, p)));
        if (user.MustChangePassword)
            claims.Add(new Claim(CapitraClaimTypes.PasswordChangeRequired, "1"));

        var token = tokens.Issue(claims);
        return new LoginResponse(token.Token, token.ExpiresAt, profile, user.MustChangePassword);
    }

    /// <summary>The signed-in user's profile, roles and permissions, read fresh from the database.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var user = currentUser.UserId is null ? null : await users.FindByIdAsync(currentUser.UserId);
        if (user is null || !user.IsActive)
            return Unauthorized();

        return await BuildProfileAsync(user);
    }

    private async Task<CurrentUserDto> BuildProfileAsync(SecUser user)
    {
        var roleNames = (await users.GetRolesAsync(user)).Order(StringComparer.Ordinal).ToList();

        var permissions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var roleName in roleNames)
        {
            var role = await roles.FindByNameAsync(roleName);
            if (role is null)
                continue;
            foreach (var claim in await roles.GetClaimsAsync(role))
                if (claim.Type == Permissions.ClaimType)
                    permissions.Add(claim.Value);
        }

        return new CurrentUserDto(user.Id, user.UserName!, user.FullName, user.EmployeeNo, user.Department, roleNames, [.. permissions]);
    }

    private static ProblemDetails Problem(string detail) =>
        new() { Status = StatusCodes.Status401Unauthorized, Title = "Login failed", Detail = detail };
}
