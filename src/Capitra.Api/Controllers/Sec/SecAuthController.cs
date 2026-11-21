using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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

/// <summary>
/// The password was right but a second step is needed (two-factor): CODE (enter the authenticator's code) or SETUP (set the
/// authenticator up first). The challenge proves the password step for 5 minutes; it is not a token.
/// </summary>
public sealed record SecondStepChallenge(string Kind, string Challenge, string UserName, string? FullName, int ExpiresInSeconds);

public sealed record SecondStepResponse(SecondStepChallenge SecondStep);

public sealed record SecondStepRequest([Required] string Challenge, string? Code, string? RecoveryCode);

/// <summary>What the user adds to the authenticator app: the QR code (PNG data URI), or the key typed by hand.</summary>
public sealed record AuthenticatorSetupDto(string QrCode, string SharedKey, string OtpAuthUri);

/// <summary>The completed sign-in, the backup codes when two-factor was just set up, and how many backup codes are left.</summary>
public sealed record SecondStepResult(LoginResponse Login, IReadOnlyList<string>? RecoveryCodes, int RecoveryCodesLeft);

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
    CapitraDbContext db,
    Microsoft.AspNetCore.DataProtection.IDataProtectionProvider protection) : ControllerBase
{
    private const string InvalidLogin = "Invalid user name or password.";

    private const string WrongCode = "The code is not right. Check the time on your phone and try the newest code.";

    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

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

        // every member-company account signs in with a second step (authenticator app); others only if they turned it on
        if (user.MemberCompanyId is not null || user.TwoFactorEnabled)
        {
            var pending = NewSignIn(request.UserName, user, "AWAITING_CODE", null);
            db.Add(pending);
            await db.SaveChangesAsync(HttpContext.RequestAborted);
            var challenge = Challenger().Protect($"{user.Id}|{pending.SignInId}", ChallengeLifetime);
            return Ok(new SecondStepResponse(new SecondStepChallenge(
                user.TwoFactorEnabled ? "CODE" : "SETUP", challenge, user.UserName!, user.FullName, (int)ChallengeLifetime.TotalSeconds)));
        }

        return await CompleteSignInAsync(user, NewSignIn(request.UserName, user, "SUCCESS", null), null);
    }

    /// <summary>A successful sign-in opens a session: its id goes in the token, so what is done in it points back to this sign-in.</summary>
    private async Task<LoginResponse> CompleteSignInAsync(SecUser user, SecSignIn signIn, string? secondStep)
    {
        await users.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = clock.UtcNow;
        await users.UpdateAsync(user);
        var sessionId = Guid.NewGuid().ToString("N");
        var response = await IssueAsync(user, sessionId);
        signIn.ResultCode = "SUCCESS";
        signIn.SessionId = sessionId;
        signIn.ExpiresAt = response.ExpiresAt;
        signIn.SecondStepCode = secondStep;
        if (signIn.SignInId == 0)
            db.Add(signIn);
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return response;
    }

    private Microsoft.AspNetCore.DataProtection.ITimeLimitedDataProtector Challenger() =>
        Microsoft.AspNetCore.DataProtection.DataProtectionAdvancedExtensions.ToTimeLimitedDataProtector(
            protection.CreateProtector("Capitra.Sec.SignInChallenge"));

    /// <summary>The user and the pending sign-in a challenge stands for, if it is genuine, unexpired and the sign-in still waits.</summary>
    private async Task<(SecUser User, SecSignIn SignIn)?> ReadChallengeAsync(string challenge)
    {
        string payload;
        try
        {
            payload = Challenger().Unprotect(challenge);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }

        var parts = payload.Split('|');
        if (parts.Length != 2 || !long.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var signInId))
            return null;
        var user = await users.FindByIdAsync(parts[0]);
        var signIn = await db.Set<SecSignIn>().FirstOrDefaultAsync(s => s.SignInId == signInId && s.UserId == parts[0], HttpContext.RequestAborted);
        if (user is null || signIn is not { ResultCode: "AWAITING_CODE" })
            return null;
        return (user, signIn);
    }

    /// <summary>Checks the account again at the second step: still active, not locked, company still allowed.</summary>
    private async Task<string?> StillAllowedAsync(SecUser user)
    {
        if (!user.IsActive)
            return InvalidLogin;
        if (await users.IsLockedOutAsync(user))
            return "The account is locked. Try again later.";
        return user.MemberCompanyId is { } companyId ? await companyAccess.BlockReasonAsync(companyId, HttpContext.RequestAborted) : null;
    }

    private UnauthorizedObjectResult ChallengeExpired() =>
        Unauthorized(Problem("The sign-in took too long or was already used. Sign in again."));

    /// <summary>First sign-in with two-factor: the key to add to the authenticator app, as a QR code and as text.</summary>
    [AllowAnonymous]
    [HttpPost("second-step/setup")]
    public async Task<ActionResult<AuthenticatorSetupDto>> SecondStepSetup(SecondStepRequest request)
    {
        if (await ReadChallengeAsync(request.Challenge) is not { } found)
            return ChallengeExpired();
        var user = found.User;
        if (user.TwoFactorEnabled)
            return BadRequest(Problem("Two-factor is already set up for this account."));
        if (await StillAllowedAsync(user) is { } refused)
            return Unauthorized(Problem(refused));

        // the same key until it is confirmed, so scanning twice is harmless
        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await users.ResetAuthenticatorKeyAsync(user);
            key = await users.GetAuthenticatorKeyAsync(user);
        }

        var uri = $"otpauth://totp/{Uri.EscapeDataString("CAPITRA:" + user.UserName)}?secret={key}&issuer=CAPITRA&digits=6&period=30";
        using var data = new QRCoder.QRCodeGenerator().CreateQrCode(uri, QRCoder.QRCodeGenerator.ECCLevel.Q);
        using var qr = new QRCoder.PngByteQRCode(data);
        var png = qr.GetGraphic(6);
        var spaced = string.Join(' ', Enumerable.Range(0, (key!.Length + 3) / 4).Select(i => key.Substring(i * 4, Math.Min(4, key.Length - (i * 4)))));
        return new AuthenticatorSetupDto($"data:image/png;base64,{Convert.ToBase64String(png)}", spaced, uri);
    }

    /// <summary>Confirms the set-up with a first code from the app: two-factor is on, 10 backup codes are shown once, and the user is in.</summary>
    [AllowAnonymous]
    [HttpPost("second-step/enable")]
    public async Task<ActionResult<SecondStepResult>> SecondStepEnable(SecondStepRequest request)
    {
        if (await ReadChallengeAsync(request.Challenge) is not { } found)
            return ChallengeExpired();
        var (user, signIn) = found;
        if (user.TwoFactorEnabled)
            return BadRequest(Problem("Two-factor is already set up for this account."));
        if (await StillAllowedAsync(user) is { } refused)
            return Unauthorized(Problem(refused));
        var code = (request.Code ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
        if (!await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, code))
        {
            await users.AccessFailedAsync(user);
            return BadRequest(Problem(WrongCode));
        }

        await users.SetTwoFactorEnabledAsync(user, true);
        var recovery = (await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToList() ?? [];
        access.Clear();
        var login = await CompleteSignInAsync(user, signIn, "SETUP");
        return new SecondStepResult(login, recovery, recovery.Count);
    }

    /// <summary>The second step of a sign-in: the authenticator's 6-digit code, or one of the backup codes (each works once).</summary>
    [AllowAnonymous]
    [HttpPost("second-step/verify")]
    public async Task<ActionResult<SecondStepResult>> SecondStepVerify(SecondStepRequest request)
    {
        if (await ReadChallengeAsync(request.Challenge) is not { } found)
            return ChallengeExpired();
        var (user, signIn) = found;
        if (!user.TwoFactorEnabled)
            return BadRequest(Problem("Set up two-factor first."));
        if (await StillAllowedAsync(user) is { } refused)
            return Unauthorized(Problem(refused));

        bool ok;
        string method;
        if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
        {
            method = "RECOVERY_CODE";
            ok = (await users.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode.Trim().Replace(" ", string.Empty, StringComparison.Ordinal))).Succeeded;
        }
        else
        {
            method = "AUTHENTICATOR";
            var code = (request.Code ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
            ok = await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, code);
        }

        if (!ok)
        {
            // wrong codes count towards the lockout like wrong passwords, and each is in the sign-in log
            await users.AccessFailedAsync(user);
            var bad = NewSignIn(user.UserName!, user, "BAD_CODE", method == "RECOVERY_CODE" ? "A backup code that is not right or was already used." : WrongCode);
            bad.SecondStepCode = method;
            db.Add(bad);
            await db.SaveChangesAsync(HttpContext.RequestAborted);
            return BadRequest(Problem(method == "RECOVERY_CODE" ? "The backup code is not right or was already used." : WrongCode));
        }

        var login = await CompleteSignInAsync(user, signIn, method);
        return new SecondStepResult(login, null, await users.CountRecoveryCodesAsync(user));
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
