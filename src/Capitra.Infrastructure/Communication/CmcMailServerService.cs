using System.Net.Mail;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Cmc;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Capitra.Infrastructure.Communication;

/// <summary>A mail server as the screens see it: never its password, only whether one is stored.</summary>
public sealed record CmcMailServerDto(
    long MailServerId, string ServerName, string HostName, int PortNo, bool IsSslEnabled, string? UserName, bool HasPassword,
    bool PasswordUnreadable, string FromAddress, string FromName, bool IsActive, DateTime? LastTestAt, string? LastTestStatusCode,
    string? LastTestError, string CreatedBy, DateTime CreatedAt, string? UpdatedBy, DateTime? UpdatedAt);

/// <param name="Password">Empty keeps the stored password; <paramref name="ClearPassword"/> removes it (a server without sign-in).</param>
public sealed record CmcMailServerRequest(
    string? ServerName, string? HostName, int? PortNo, bool IsSslEnabled, string? UserName, string? Password, bool ClearPassword,
    string? FromAddress, string? FromName);

public sealed record CmcTestRequest(string? ToAddress);

public sealed record CmcTestResult(bool Sent, string StatusCode, string? Error);

/// <summary>Which mail server the system uses right now, for the Communication Center's home page.</summary>
public sealed record CmcMailStatus(string SourceCode, string? ServerName, string? HostName, string? FromAddress, DateTime? LastTestAt, string? LastTestStatusCode);

public sealed record CmcEmailLogDto(
    long EmailLogId, string ToAddress, string KindCode, string Subject, string StatusCode, string? ErrorText, DateTime AttemptedAt,
    string? ServerName, string? UserName, string CreatedBy);

public sealed record CmcEmailLogQuery(string? Search, string? StatusCode, string? KindCode, DateOnly? FromDate, DateOnly? ToDate, int? Take);

/// <summary>
/// Communication Center: the outgoing mail servers (one active at a time, used by every e-mail of the system), test e-mails, and
/// the log of every e-mail sent. Nothing is deleted: a server no longer used is simply not active.
/// </summary>
public sealed class CmcMailServerService(
    CapitraDbContext db, MailSettings mail, IMailTransport transport, IOptions<EmailOptions> options, IClock clock, ILogger<CmcMailServerService> logger)
{
    public const string Test = "TEST";

    public async Task<IReadOnlyList<CmcMailServerDto>> ListAsync(CancellationToken ct) =>
        (await db.Set<CmcMailServer>().AsNoTracking().OrderByDescending(s => s.IsActive).ThenBy(s => s.ServerName).ToListAsync(ct))
        .Select(ToDto).ToList();

    public async Task<CmcMailStatus> StatusAsync(CancellationToken ct)
    {
        var active = await db.Set<CmcMailServer>().AsNoTracking().FirstOrDefaultAsync(s => s.IsActive, ct);
        if (active is not null)
            return new CmcMailStatus("COMMUNICATION_CENTER", active.ServerName, active.HostName, active.FromAddress, active.LastTestAt, active.LastTestStatusCode);
        var o = options.Value;
        return o.IsConfigured
            ? new CmcMailStatus("APPLICATION_SETTINGS", null, o.Host, o.FromAddress, null, null)
            : new CmcMailStatus("NONE", null, null, null, null, null);
    }

    public async Task<CmcMailServerDto> SaveAsync(long? id, CmcMailServerRequest r, CancellationToken ct)
    {
        var server = id is { } x
            ? await db.Set<CmcMailServer>().FirstOrDefaultAsync(s => s.MailServerId == x, ct) ?? throw new NotFoundException("The mail server was not found.")
            : new CmcMailServer();

        var errors = new Dictionary<string, string[]>();
        var name = r.ServerName?.Trim();
        var host = r.HostName?.Trim();
        var from = r.FromAddress?.Trim();
        var userName = string.IsNullOrWhiteSpace(r.UserName) ? null : r.UserName.Trim();
        if (string.IsNullOrEmpty(name))
            errors["serverName"] = ["Enter a name for the server."];
        else if (await db.Set<CmcMailServer>().AnyAsync(s => s.ServerName == name && s.MailServerId != server.MailServerId, ct))
            errors["serverName"] = ["Another mail server has this name."];
        if (string.IsNullOrEmpty(host) || host.Contains(' ', StringComparison.Ordinal) || host.Contains("://", StringComparison.Ordinal))
            errors["hostName"] = ["Enter the server's host name, e.g. smtp.gmail.com."];
        if (r.PortNo is not (>= 1 and <= 65535))
            errors["portNo"] = ["Enter a port from 1 to 65535 (usually 587)."];
        if (!IsAddress(from))
            errors["fromAddress"] = ["Enter a valid sender e-mail address."];
        var newPassword = string.IsNullOrEmpty(r.Password) ? null : r.Password;
        if (userName is not null && newPassword is null && (r.ClearPassword || server.PasswordProtected is null))
            errors["password"] = ["Enter the password of the sign-in (for Gmail: an app password)."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        server.ServerName = name!;
        server.HostName = host!;
        server.PortNo = r.PortNo!.Value;
        server.IsSslEnabled = r.IsSslEnabled;
        server.UserName = userName;
        server.FromAddress = from!;
        server.FromName = string.IsNullOrWhiteSpace(r.FromName) ? "CAPITRA" : r.FromName.Trim();
        if (userName is null || r.ClearPassword)
            server.PasswordProtected = null;
        if (newPassword is not null && userName is not null)
            server.PasswordProtected = mail.Protect(newPassword);
        // a changed server has not been tested yet
        server.LastTestAt = null;
        server.LastTestStatusCode = null;
        server.LastTestError = null;
        if (id is null)
            db.Add(server);
        await db.SaveChangesAsync(ct);
        return ToDto(server);
    }

    /// <summary>Makes this the server every e-mail goes through; the one active before stops being used (it is kept).</summary>
    public async Task<CmcMailServerDto> ActivateAsync(long id, CancellationToken ct)
    {
        var server = await FindAsync(id, ct);
        if (server.LastTestStatusCode != "SENT")
            throw new BusinessRuleException("Send a test e-mail through this server first: only a server that worked can be used.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        foreach (var other in await db.Set<CmcMailServer>().Where(s => s.IsActive && s.MailServerId != id).ToListAsync(ct))
            other.IsActive = false;
        await db.SaveChangesAsync(ct);
        server.IsActive = true;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ToDto(server);
    }

    /// <summary>Stops using the server; e-mails then go through the application settings' server, if there is one.</summary>
    public async Task<CmcMailServerDto> DeactivateAsync(long id, CancellationToken ct)
    {
        var server = await FindAsync(id, ct);
        server.IsActive = false;
        await db.SaveChangesAsync(ct);
        return ToDto(server);
    }

    /// <summary>Sends a test e-mail through the server (active or not) and records the result on the server and in the log.</summary>
    public async Task<CmcTestResult> TestAsync(long id, CmcTestRequest r, string? requestedBy, CancellationToken ct)
    {
        var server = await FindAsync(id, ct);
        var to = r.ToAddress?.Trim();
        if (!IsAddress(to))
            throw BusinessValidationException.For("toAddress", "Enter the e-mail address to send the test to.");
        var settings = mail.For(server);
        var subject = "CAPITRA — test e-mail / رسالة تجريبية";
        var log = new CmcEmailLog { ToAddress = to!, KindCode = Test, Subject = subject, AttemptedAt = clock.UtcNow, MailServerId = server.MailServerId };
        if (server.UserName is not null && settings.Password is null)
        {
            log.StatusCode = "FAILED";
            log.ErrorText = "The stored password cannot be read any more (the encryption key changed). Enter the password again.";
        }
        else
        {
            try
            {
                await transport.SendAsync(settings, to!, subject, TestBody(server, requestedBy), ct);
                log.StatusCode = "SENT";
            }
            catch (Exception ex) when (SmtpMailTransport.IsMailError(ex))
            {
                log.StatusCode = "FAILED";
                log.ErrorText = Describe(ex);
                logger.LogWarning(ex, "Test e-mail through {Server} failed", server.ServerName);
            }
        }

        server.LastTestAt = log.AttemptedAt;
        server.LastTestStatusCode = log.StatusCode;
        server.LastTestError = log.ErrorText;
        db.Add(log);
        await db.SaveChangesAsync(ct);
        return new CmcTestResult(log.StatusCode == "SENT", log.StatusCode, log.ErrorText);
    }

    public async Task<IReadOnlyList<CmcEmailLogDto>> LogAsync(CmcEmailLogQuery q, CancellationToken ct)
    {
        var rows = db.Set<CmcEmailLog>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.StatusCode))
            rows = rows.Where(l => l.StatusCode == q.StatusCode);
        if (!string.IsNullOrWhiteSpace(q.KindCode))
            rows = rows.Where(l => l.KindCode == q.KindCode);
        if (q.FromDate is { } f)
        {
            var start = f.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            rows = rows.Where(l => l.AttemptedAt >= start);
        }

        if (q.ToDate is { } t)
        {
            var end = t.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            rows = rows.Where(l => l.AttemptedAt < end);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = $"%{q.Search.Trim()}%";
            rows = rows.Where(l => EF.Functions.ILike(l.ToAddress, s) || EF.Functions.ILike(l.Subject, s));
        }

        var take = Math.Clamp(q.Take ?? 500, 1, 2000);
        return await (
            from l in rows.OrderByDescending(l => l.AttemptedAt).ThenByDescending(l => l.EmailLogId).Take(take)
            join s in db.Set<CmcMailServer>() on l.MailServerId equals s.MailServerId into ss
            from s in ss.DefaultIfEmpty()
            join u in db.Users on l.UserId equals u.Id into us
            from u in us.DefaultIfEmpty()
            orderby l.AttemptedAt descending, l.EmailLogId descending
            select new CmcEmailLogDto(l.EmailLogId, l.ToAddress, l.KindCode, l.Subject, l.StatusCode, l.ErrorText, l.AttemptedAt,
                s == null ? null : s.ServerName, u == null ? null : u.UserName, l.CreatedBy))
            .ToListAsync(ct);
    }

    private async Task<CmcMailServer> FindAsync(long id, CancellationToken ct) =>
        await db.Set<CmcMailServer>().FirstOrDefaultAsync(s => s.MailServerId == id, ct) ?? throw new NotFoundException("The mail server was not found.");

    private CmcMailServerDto ToDto(CmcMailServer s) => new(
        s.MailServerId, s.ServerName, s.HostName, s.PortNo, s.IsSslEnabled, s.UserName, s.PasswordProtected is not null,
        s.PasswordProtected is not null && mail.Unprotect(s.PasswordProtected) is null, s.FromAddress, s.FromName, s.IsActive,
        s.LastTestAt, s.LastTestStatusCode, s.LastTestError, s.CreatedBy, s.CreatedAt, s.UpdatedBy, s.UpdatedAt);

    private static bool IsAddress(string? value) => !string.IsNullOrWhiteSpace(value) && MailAddress.TryCreate(value, out var a) && a.Address == value;

    /// <summary>The mail server's answer in words an administrator can act on, with the usual causes.</summary>
    private static string Describe(Exception ex)
    {
        var text = ex.InnerException is { } inner && !string.IsNullOrWhiteSpace(inner.Message) ? $"{ex.Message} {inner.Message}" : ex.Message;
        if (text.Contains("5.7.0", StringComparison.Ordinal) || text.Contains("5.7.8", StringComparison.Ordinal)
            || text.Contains("Authentication", StringComparison.OrdinalIgnoreCase))
            text += " — Check the user name and password; Gmail needs an app password (2-Step Verification on).";
        else if (ex is System.Net.Sockets.SocketException || text.Contains("Failure sending mail", StringComparison.Ordinal))
            text += " — Check the host name and port, and that the network allows it.";
        return text.Length > 1000 ? text[..1000] : text;
    }

    private static string TestBody(CmcMailServer s, string? requestedBy)
    {
        static string E(string v) => System.Net.WebUtility.HtmlEncode(v);
        return $"""
            <div style="font-family:Segoe UI,Tahoma,Arial,sans-serif;max-width:560px;margin:auto;color:#0b2e35">
              <div style="background:#014b55;color:#fff;padding:16px 22px;border-radius:12px 12px 0 0;font-weight:800;font-size:18px">CAPITRA · Communication Center</div>
              <div style="border:1px solid #d7e3e5;border-top:0;border-radius:0 0 12px 12px;padding:22px">
                <p>This test e-mail went out through the mail server <b>{E(s.ServerName)}</b> ({E(s.HostName)}:{s.PortNo}). It works.</p>
                <p dir="rtl" style="text-align:right">أُرسلت هذه الرسالة التجريبية عبر خادم البريد <b>{E(s.ServerName)}</b>. الخادم يعمل.</p>
                <p style="color:#8aa1a6;font-size:12px">Requested by {E(requestedBy ?? "-")}.</p>
              </div>
            </div>
            """;
    }
}
