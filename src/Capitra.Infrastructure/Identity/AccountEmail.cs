using System.Net;
using System.Net.Mail;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Cmc;
using Capitra.Infrastructure.Communication;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Capitra.Infrastructure.Identity;

/// <summary>The mail server for account e-mails ("Email" settings). The password comes from user secrets or the environment, never the repository.</summary>
public sealed class EmailOptions
{
    public const string Section = "Email";

    public string? Host { get; set; }

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? FromAddress { get; set; }

    public string FromName { get; set; } = "CAPITRA";

    /// <summary>The web address users open, e.g. http://localhost:5173 (links in e-mails point there).</summary>
    public string WebBaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>How long an activation or reset link works.</summary>
    public int LinkHours { get; set; } = 48;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromAddress);
}

/// <summary>Sends one e-mail through a given mail server; the SMTP sender in production, a recorder in tests.</summary>
public interface IMailTransport
{
    Task SendAsync(MailServerSettings server, string to, string subject, string html, CancellationToken ct);
}

public sealed class SmtpMailTransport : IMailTransport
{
    public async Task SendAsync(MailServerSettings server, string to, string subject, string html, CancellationToken ct)
    {
        using var message = new MailMessage { From = new MailAddress(server.FromAddress, server.FromName), Subject = subject, Body = html, IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
        message.To.Add(to);
        using var client = new SmtpClient(server.Host, server.Port) { EnableSsl = server.EnableSsl, DeliveryMethod = SmtpDeliveryMethod.Network, Timeout = 30_000 };
        if (!string.IsNullOrWhiteSpace(server.UserName))
            client.Credentials = new NetworkCredential(server.UserName, server.Password);
        await client.SendMailAsync(message, ct);
    }

    /// <summary>The errors a mail server can give: recorded on the e-mail, never thrown to the user.</summary>
    public static bool IsMailError(Exception ex) => ex is SmtpException or InvalidOperationException or IOException or FormatException
        or System.Security.Authentication.AuthenticationException or System.Net.Sockets.SocketException;
}

/// <summary>The result of an account e-mail: whether it went out, and the link it carried (for "Copy link" when it did not).</summary>
public sealed record AccountEmailResult(bool Sent, string StatusCode, string? Error, string Link);

/// <summary>
/// Activation and password-reset e-mails. The link carries ASP.NET Identity's password-reset token: signed, valid for
/// <see cref="EmailOptions.LinkHours"/>, single use (setting the password changes the security stamp, which voids it), never stored.
/// No password is ever e-mailed.
/// </summary>
public sealed class AccountEmails(
    UserManager<SecUser> users, IMailTransport transport, MailSettings mail, IOptions<EmailOptions> options, CapitraDbContext db, IClock clock,
    ILogger<AccountEmails> logger)
{
    public const string Activation = "ACTIVATION";
    public const string PasswordReset = "PASSWORD_RESET";

    /// <summary>A fresh activation link; every earlier link for this user stops working.</summary>
    public async Task<string> NewActivationLinkAsync(SecUser user)
    {
        await users.UpdateSecurityStampAsync(user);
        return await LinkAsync(user, "activate");
    }

    public async Task<AccountEmailResult> SendActivationAsync(SecUser user, string companyName, string roleName, CancellationToken ct)
    {
        var link = await NewActivationLinkAsync(user);
        var subject = $"CAPITRA — activate your account / تفعيل حسابك ({companyName})";
        var body = Template(
            $"Your CAPITRA account for <b>{E(companyName)}</b> has been created as <b>{E(roleName)}</b>.",
            $"تم إنشاء حسابك في كابيترا لشركة <b>{E(companyName)}</b> بصفة <b>{E(RoleAr(roleName))}</b>.",
            user, link, "Activate my account", "تفعيل حسابي");
        return await SendAsync(user, Activation, subject, body, link, ct);
    }

    /// <summary>An activation e-mail worded from the user's own company and role (resend, forgot password before activating).</summary>
    public async Task<AccountEmailResult> SendActivationAsync(SecUser user, CancellationToken ct)
    {
        var company = user.MemberCompanyId is { } id
            ? await db.Set<Capitra.Domain.Modules.Scr.ScrMemberCompany>().Where(c => c.MemberCompanyId == id).Select(c => c.NameEn).FirstOrDefaultAsync(ct)
            : null;
        var roles = await users.GetRolesAsync(user);
        var role = roles.Contains("MEMBER_COMPANY_ADMIN") ? "Company administrator" : roles.Contains("MEMBER_COMPANY_USER") ? "Company user" : "user";
        return await SendActivationAsync(user, company ?? "CAPITRA", role, ct);
    }

    public async Task<AccountEmailResult> SendPasswordResetAsync(SecUser user, CancellationToken ct)
    {
        var link = await LinkAsync(user, "reset-password");
        var subject = "CAPITRA — reset your password / إعادة تعيين كلمة المرور";
        var body = Template(
            "A password reset was requested for your CAPITRA account. If it was not you, ignore this e-mail.",
            "طُلبت إعادة تعيين كلمة مرور حسابك في كابيترا. إن لم تكن أنت، تجاهل هذه الرسالة.",
            user, link, "Choose a new password", "اختيار كلمة مرور جديدة");
        return await SendAsync(user, PasswordReset, subject, body, link, ct);
    }

    /// <summary>
    /// The link itself, for an administrator to pass on when the e-mail does not arrive: a new activation link (pending account)
    /// or a password-reset link; every earlier link stops working. Logged as LINK_COPIED.
    /// </summary>
    public async Task<(string Link, string Kind)> CopyLinkAsync(SecUser user, CancellationToken ct)
    {
        await users.UpdateSecurityStampAsync(user);
        var activating = user.PasswordHash is null;
        var link = await LinkAsync(user, activating ? "activate" : "reset-password");
        var kind = activating ? Activation : PasswordReset;
        db.Add(new CmcEmailLog
        {
            UserId = user.Id, ToAddress = user.Email ?? string.Empty, KindCode = kind, Subject = "(link copied by an administrator)",
            StatusCode = "LINK_COPIED", AttemptedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        return (link, kind);
    }

    private async Task<string> LinkAsync(SecUser user, string page)
    {
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var baseUrl = options.Value.WebBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{page}?user={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
    }

    private async Task<AccountEmailResult> SendAsync(SecUser user, string kind, string subject, string body, string link, CancellationToken ct)
    {
        var log = new CmcEmailLog { UserId = user.Id, ToAddress = user.Email ?? string.Empty, KindCode = kind, Subject = subject, AttemptedAt = clock.UtcNow };
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            log.StatusCode = "NO_ADDRESS";
        }
        else if (await mail.CurrentAsync(ct) is not { } server)
        {
            log.StatusCode = "NOT_CONFIGURED";
        }
        else
        {
            log.MailServerId = server.MailServerId;
            try
            {
                await transport.SendAsync(server, user.Email, subject, body, ct);
                log.StatusCode = "SENT";
            }
            catch (Exception ex) when (SmtpMailTransport.IsMailError(ex))
            {
                log.StatusCode = "FAILED";
                log.ErrorText = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                logger.LogWarning(ex, "Account e-mail {Kind} to {To} failed", kind, user.Email);
            }
        }

        db.Add(log);
        await db.SaveChangesAsync(ct);
        return new AccountEmailResult(log.StatusCode == "SENT", log.StatusCode, log.ErrorText, link);
    }

    private string Template(string en, string ar, SecUser user, string link, string buttonEn, string buttonAr)
    {
        var hours = options.Value.LinkHours;
        var login = $"{options.Value.WebBaseUrl.TrimEnd('/')}/login";
        static string Button(string href, string text) =>
            $"<a href=\"{href}\" style=\"display:inline-block;padding:12px 22px;border-radius:10px;background:#0f766e;color:#ffffff;text-decoration:none;font-weight:700\">{text}</a>";
        return $"""
            <div style="font-family:Segoe UI,Tahoma,Arial,sans-serif;max-width:560px;margin:auto;color:#0b2e35">
              <div style="background:#014b55;color:#fff;padding:16px 22px;border-radius:12px 12px 0 0;font-weight:800;font-size:18px">CAPITRA</div>
              <div style="border:1px solid #d7e3e5;border-top:0;border-radius:0 0 12px 12px;padding:22px">
                <p>Hello {E(user.FullName ?? user.UserName ?? "")},</p>
                <p>{en}</p>
                <p>User name: <b dir="ltr">{E(user.UserName ?? "")}</b></p>
                <p>{Button(link, buttonEn)}</p>
                <p style="color:#5d7a80;font-size:13px">The link works once and expires in {hours} hours. Then sign in at <a href="{login}">{login}</a>.</p>
                <hr style="border:0;border-top:1px solid #d7e3e5;margin:20px 0">
                <div dir="rtl" style="text-align:right">
                  <p>مرحبًا {E(user.FullName ?? user.UserName ?? "")}،</p>
                  <p>{ar}</p>
                  <p>اسم المستخدم: <b dir="ltr">{E(user.UserName ?? "")}</b></p>
                  <p>{Button(link, buttonAr)}</p>
                  <p style="color:#5d7a80;font-size:13px">يعمل الرابط مرة واحدة وتنتهي صلاحيته خلال {hours} ساعة. بعدها سجّل الدخول من <a href="{login}">{login}</a>.</p>
                </div>
                <p style="color:#8aa1a6;font-size:12px">CAPITRA never e-mails passwords. / كابيترا لا ترسل كلمات المرور بالبريد أبدًا.</p>
              </div>
            </div>
            """;
    }

    private static string E(string s) => WebUtility.HtmlEncode(s);

    private static string RoleAr(string role) => role switch
    {
        "Company administrator" => "مسؤول الشركة",
        "Company user" => "مستخدم الشركة",
        _ => role,
    };
}
