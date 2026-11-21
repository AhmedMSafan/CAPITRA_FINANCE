using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Scr;

/// <summary>
/// A member company's account. StatusCode: PENDING_ACTIVATION (the activation link not used yet; holds a seat), ACTIVE or
/// INACTIVE. No password is ever shown or sent: every account is activated by an e-mailed link.
/// </summary>
public sealed record ScrCompanyUserDto(
    string UserId, string UserName, string? Email, string? FullName, bool IsActive, string RoleCode, DateTime? LastLoginAt, string StatusCode,
    bool TwoFactorEnabled = false);

/// <summary>A new company user (always MEMBER_COMPANY_USER; the administrator is created with the company).</summary>
public sealed record ScrCompanyUserRequest([Required] string UserName, string? Email, string? FullName, bool IsActive = true);

/// <summary>What happened to the e-mail; Link only when asked for it (Copy link).</summary>
public sealed record ScrEmailOutcome(bool Sent, string StatusCode, string? Error);

public sealed record ScrCompanyUserResult(ScrCompanyUserDto User, ScrEmailOutcome Email);

public sealed record ScrCompanyAdminRequest(string? UserName, string? Email, string? FullName);

/// <summary>A new member company in one step: the company, its licence and its administrator.</summary>
public sealed record ScrCompanyOnboardRequest(ScrCompanyRequest? Company, ScrLicenceRequest? Licence, ScrCompanyAdminRequest? Admin);

public sealed record ScrCompanyOnboardResult(ScrCompanyDto Company, ScrCompanyUserDto Admin, ScrEmailOutcome Email);

public sealed record ScrLinkResult(string Link, string Kind);

/// <summary>Member companies and their licences (SCR.COMPANY.ADMIN, the federation).</summary>
[ApiController]
[Route("api/scr/companies")]
public sealed class ScrCompaniesController(
    ScrCompanyService companies, ICapitraDbContext db, UserManager<SecUser> users, AccountEmails emails, UserAccessCache access) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public Task<IReadOnlyList<ScrCompanyDto>> List(CancellationToken ct) => companies.ListAsync(ct);

    /// <summary>
    /// Onboards a member company in one transaction: the company, its licence (administrator + MaxUsers users, period, quota) and
    /// its administrator, who gets an activation e-mail. A company never exists without its administrator.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public async Task<ScrCompanyOnboardResult> Create(ScrCompanyOnboardRequest request, CancellationToken ct)
    {
        var errors = ScrCompanyService.ValidateLicence(request.Licence);
        var admin = request.Admin ?? new ScrCompanyAdminRequest(null, null, null);
        var userName = CompanyUserRules.Clean(admin.UserName) ?? CompanyUserRules.Clean(admin.Email);
        CompanyUserRules.ValidatePerson(errors, "admin.", userName, admin.Email, admin.FullName);
        if (errors.Count == 0)
            await CompanyUserRules.EnsureUniqueAsync(users, errors, "admin.", userName!, admin.Email!, null, ct);
        if (request.Company is null)
            errors["company"] = ["Enter the company."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        SecUser user;
        ScrCompanyDto company;
        await using (var tx = await db.BeginTransactionAsync(ct))
        {
            company = await companies.SaveAsync(null, request.Company!, ct);
            await companies.AddLicenceAsync(company.MemberCompanyId, request.Licence!, ct);
            user = new SecUser
            {
                UserName = userName, Email = admin.Email!.Trim(), FullName = admin.FullName!.Trim(), IsActive = true,
                MemberCompanyId = company.MemberCompanyId,
            };
            CompanyUserRules.Ensure(await users.CreateAsync(user), "admin.userName");
            CompanyUserRules.Ensure(await users.AddToRoleAsync(user, CompanyUserRules.AdminRole), "admin");
            await tx.CommitAsync(ct);
        }

        var mail = await emails.SendActivationAsync(user, company.NameEn, "Company administrator", ct);
        return new ScrCompanyOnboardResult(await companies.GetAsync(company.MemberCompanyId, ct), await CompanyUserRules.ToDtoAsync(users, user),
            new ScrEmailOutcome(mail.Sent, mail.StatusCode, mail.Error));
    }

    /// <summary>Company details and status. Suspending a company ends its users' sessions at once.</summary>
    [HttpPut("{companyId:long}")]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public async Task<ScrCompanyDto> Update(long companyId, ScrCompanyRequest request, CancellationToken ct)
    {
        var result = await companies.SaveAsync(companyId, request, ct);
        access.Clear();
        return result;
    }

    [HttpGet("{companyId:long}/licences")]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public Task<IReadOnlyList<ScrLicenceDto>> Licences(long companyId, CancellationToken ct) => companies.LicencesAsync(companyId, ct);

    /// <summary>A new licence for the company (renewal, more users, a new quota); it is in force at once.</summary>
    [HttpPost("{companyId:long}/licences")]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public async Task<ScrLicenceDto> AddLicence(long companyId, ScrLicenceRequest request, CancellationToken ct)
    {
        var result = await companies.AddLicenceAsync(companyId, request, ct);
        access.Clear();
        return result;
    }
}

/// <summary>
/// The user accounts of a member company. The federation (SCR.COMPANY.ADMIN) manages any company's; the company administrator
/// (SCR.COMPANY.USERS) only its own. Users are added up to the licence's number of users; each gets an activation e-mail.
/// </summary>
[ApiController]
[Route("api/scr/companies/{companyId:long}/users")]
public sealed class ScrCompanyUsersController(
    UserManager<SecUser> users, IScrUserProfiles profiles, ScrCompanyService companies, IScrCompanyUserCounts seats,
    AccountEmails emails, UserAccessCache access, Capitra.Infrastructure.Persistence.CapitraDbContext db, IClock clock) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(long companyId, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var list = await users.Users.AsNoTracking().Where(u => u.MemberCompanyId == companyId).OrderBy(u => u.UserName).ToListAsync(ct);
        var result = new List<ScrCompanyUserDto>();
        foreach (var u in list)
            result.Add(await CompanyUserRules.ToDtoAsync(users, u));
        // the administrator first
        return Ok(result.OrderByDescending(u => u.RoleCode == CompanyUserRules.AdminRole).ToList());
    }

    /// <summary>
    /// A new company user, within the licence's number of users; an activation e-mail goes to them. A company without an
    /// administrator (one made before administrators came with the company) gets its first one this way: when the Federation adds a
    /// user to it, that user becomes the administrator and holds no user seat.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(long companyId, ScrCompanyUserRequest request, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var company = await companies.GetAsync(companyId, ct);
        var errors = new Dictionary<string, string[]>();
        var userName = CompanyUserRules.Clean(request.UserName);
        CompanyUserRules.ValidatePerson(errors, "", userName, request.Email, request.FullName);
        if (errors.Count == 0)
            await CompanyUserRules.EnsureUniqueAsync(users, errors, "", userName!, request.Email!, null, ct);
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        var makeAdmin = ScrClaims.Has(User, Permissions.Scr.CompanyAdmin) && company.Seats?.AdminUserName is null;
        if (!makeAdmin)
            await EnsureSeatAsync(company, ct);

        var user = new SecUser
        {
            UserName = userName, Email = request.Email!.Trim(), FullName = request.FullName!.Trim(), IsActive = true, MemberCompanyId = companyId,
        };
        CompanyUserRules.Ensure(await users.CreateAsync(user), "userName");
        var added = await users.AddToRoleAsync(user, makeAdmin ? CompanyUserRules.AdminRole : CompanyUserRules.UserRole);
        if (!added.Succeeded)
        {
            await users.DeleteAsync(user);
            CompanyUserRules.Ensure(added, "user");
        }

        var mail = await emails.SendActivationAsync(user, company.NameEn, makeAdmin ? "Company administrator" : "Company user", ct);
        return Ok(new ScrCompanyUserResult(await CompanyUserRules.ToDtoAsync(users, user), new ScrEmailOutcome(mail.Sent, mail.StatusCode, mail.Error)));
    }

    /// <summary>
    /// Name, e-mail and active. The administrator cannot be deactivated (a company always has one). Reactivating a user needs a free
    /// seat. Applies at once to open sessions.
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(long companyId, string userId, ScrCompanyUserRequest request, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var company = await companies.GetAsync(companyId, ct);
        var user = await FindAsync(companyId, userId);
        var isAdmin = (await users.GetRolesAsync(user)).Contains(CompanyUserRules.AdminRole);
        var errors = new Dictionary<string, string[]>();
        CompanyUserRules.ValidatePerson(errors, "", user.UserName, request.Email, request.FullName);
        if (errors.Count == 0)
            await CompanyUserRules.EnsureUniqueAsync(users, errors, "", user.UserName!, request.Email!, user.Id, ct);
        if (isAdmin && !request.IsActive)
            errors["isActive"] = ["The company administrator cannot be deactivated: a company always has one. Ask the Federation to replace them."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        if (!isAdmin && request.IsActive && !user.IsActive)
            await EnsureSeatAsync(company, ct);

        user.Email = request.Email!.Trim();
        user.FullName = request.FullName!.Trim();
        user.IsActive = request.IsActive;
        CompanyUserRules.Ensure(await users.UpdateAsync(user), "user");
        access.Clear();
        return Ok(await CompanyUserRules.ToDtoAsync(users, user));
    }

    /// <summary>
    /// E-mails the user a link: a new activation link while the account is pending (the earlier one stops working), otherwise a
    /// password-reset link. A lock-out from wrong passwords is lifted.
    /// </summary>
    [HttpPost("{userId}/send-link")]
    public async Task<IActionResult> SendLink(long companyId, string userId, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var user = await FindAsync(companyId, userId);
        if (!user.IsActive)
            throw new BusinessRuleException("Activate the user first.");
        await users.SetLockoutEndDateAsync(user, null);
        var mail = user.PasswordHash is null ? await emails.SendActivationAsync(user, ct) : await emails.SendPasswordResetAsync(user, ct);
        return Ok(new ScrEmailOutcome(mail.Sent, mail.StatusCode, mail.Error));
    }

    /// <summary>
    /// The link itself, for when the e-mail does not arrive (to send another way). Making it voids every earlier link; it is
    /// recorded in the e-mail log as LINK_COPIED with who asked.
    /// </summary>
    [HttpPost("{userId}/copy-link")]
    public async Task<IActionResult> CopyLink(long companyId, string userId, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var user = await FindAsync(companyId, userId);
        if (!user.IsActive)
            throw new BusinessRuleException("Activate the user first.");
        var (link, kind) = await emails.CopyLinkAsync(user, ct);
        return Ok(new ScrLinkResult(link, kind));
    }

    /// <summary>
    /// Resets a user's two-factor (a lost or changed phone): the authenticator and backup codes stop working, the user's sessions end,
    /// and the user sets it up again at the next sign-in. A company administrator resets its users; the administrator's own is reset
    /// by the Federation. Recorded in the sign-in log.
    /// </summary>
    [HttpPost("{userId}/reset-two-factor")]
    public async Task<ActionResult<ScrCompanyUserDto>> ResetTwoFactor(long companyId, string userId, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var user = await FindAsync(companyId, userId);
        var federation = ScrClaims.Has(User, Permissions.Scr.CompanyAdmin);
        if (!federation && (await users.IsInRoleAsync(user, CompanyUserRules.AdminRole) || user.UserName == User.Identity?.Name))
            throw new BusinessRuleException("The company administrator's two-factor is reset by the Federation.");
        await users.SetTwoFactorEnabledAsync(user, false);
        await users.ResetAuthenticatorKeyAsync(user);
        await users.UpdateSecurityStampAsync(user);
        access.Clear();
        db.Add(new SecSignIn
        {
            UserName = user.UserName ?? user.Id, UserId = user.Id, MemberCompanyId = user.MemberCompanyId, AttemptedAt = clock.UtcNow,
            ResultCode = "TWO_FACTOR_RESET", FailureText = $"Reset by {User.Identity?.Name}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });
        await db.SaveChangesAsync(ct);
        return await CompanyUserRules.ToDtoAsync(users, user);
    }

    private async Task EnsureSeatAsync(ScrCompanyDto company, CancellationToken ct)
    {
        var allowed = company.Licence?.MaxUsers ?? 0;
        var inUse = (await seats.SeatsAsync(ct)).GetValueOrDefault(company.MemberCompanyId)?.UsersInUse ?? 0;
        if (inUse >= allowed)
            throw new BusinessRuleException(allowed == 0
                ? "The company's licence is for its administrator only. Ask the Federation to add users to the licence."
                : "All the licence's users are in use. Deactivate a user or ask the Federation for more.");
    }

    /// <summary>The federation for any company; a company administrator for its own only.</summary>
    private async Task<bool> AllowedAsync(long companyId, CancellationToken ct)
    {
        if (ScrClaims.Has(User, Permissions.Scr.CompanyAdmin))
            return true;
        if (!ScrClaims.Has(User, Permissions.Scr.CompanyUsers) || User.Identity?.Name is not { } me)
            return false;
        return (await profiles.ByUserNameAsync(me, ct))?.MemberCompanyId == companyId;
    }

    private async Task<SecUser> FindAsync(long companyId, string userId)
    {
        var user = await users.FindByIdAsync(userId);
        return user is not null && user.MemberCompanyId == companyId ? user : throw new NotFoundException("The user was not found in this company.");
    }
}

/// <summary>The rules every member company account follows, wherever it is made.</summary>
internal static class CompanyUserRules
{
    public const string UserRole = "MEMBER_COMPANY_USER";
    public const string AdminRole = "MEMBER_COMPANY_ADMIN";

    public static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    public static void ValidatePerson(Dictionary<string, string[]> e, string prefix, string? userName, string? email, string? fullName)
    {
        if (userName is null || userName.Length > 256)
            e[$"{prefix}userName"] = ["Enter the user name."];
        if (Clean(fullName) is null)
            e[$"{prefix}fullName"] = ["Enter the full name."];
        if (Clean(email) is not { } mail || !MailAddress.TryCreate(mail, out var parsed) || parsed.Address != mail)
            e[$"{prefix}email"] = ["Enter a valid e-mail address: the activation link is sent there."];
    }

    /// <summary>User names are unique (Identity); e-mail addresses are unique among accounts too, so a link reaches one person.</summary>
    public static async Task EnsureUniqueAsync(UserManager<SecUser> users, Dictionary<string, string[]> e, string prefix, string userName, string email,
        string? exceptUserId, CancellationToken ct)
    {
        var byName = await users.FindByNameAsync(userName);
        if (byName is not null && byName.Id != exceptUserId)
            e[$"{prefix}userName"] = ["This user name is already used."];
        var normalized = users.NormalizeEmail(email.Trim());
        if (await users.Users.AnyAsync(u => u.NormalizedEmail == normalized && u.Id != exceptUserId, ct))
            e[$"{prefix}email"] = ["This e-mail address is already used by another account."];
    }

    public static async Task<ScrCompanyUserDto> ToDtoAsync(UserManager<SecUser> users, SecUser u)
    {
        var roles = await users.GetRolesAsync(u);
        var status = !u.IsActive ? "INACTIVE" : u.PasswordHash is null ? "PENDING_ACTIVATION" : "ACTIVE";
        return new(u.Id, u.UserName ?? u.Id, u.Email, u.FullName, u.IsActive, roles.Count > 0 ? roles[0] : "-", u.LastLoginAt, status, u.TwoFactorEnabled);
    }

    public static void Ensure(IdentityResult result, string field)
    {
        if (!result.Succeeded)
            throw new BusinessValidationException(new Dictionary<string, string[]> { [field] = [.. result.Errors.Select(e => e.Description)] });
    }
}
