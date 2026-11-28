using System.Net;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Cmc;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Communication;

/// <summary>
/// Notices the system sends by itself (e.g. the End of Day's licence reminders), through the Communication Center's mail server, each
/// logged in CMC_EMAIL_LOGS. A notice already sent to the same address with the same subject is not sent again, so a job can run twice.
/// Mail errors are recorded on the log, never thrown.
/// </summary>
public sealed class SystemEmails(IMailTransport transport, MailSettings mail, CapitraDbContext db, IClock clock)
{
    /// <summary>SENT, ALREADY_SENT, FAILED, NOT_CONFIGURED or NO_ADDRESS.</summary>
    public async Task<string> SendAsync(string? to, string? userId, string kind, string subject, string englishHtml, string arabicHtml, CancellationToken ct)
    {
        to = to?.Trim();
        if (!string.IsNullOrEmpty(to) && await db.Set<CmcEmailLog>().AsNoTracking()
                .AnyAsync(l => l.ToAddress == to && l.KindCode == kind && l.Subject == subject && l.StatusCode == "SENT", ct))
            return "ALREADY_SENT";

        var log = new CmcEmailLog { UserId = userId, ToAddress = to ?? string.Empty, KindCode = kind, Subject = subject, AttemptedAt = clock.UtcNow };
        if (string.IsNullOrEmpty(to))
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
                await transport.SendAsync(server, to, subject, Template(englishHtml, arabicHtml), ct);
                log.StatusCode = "SENT";
            }
            catch (Exception ex) when (SmtpMailTransport.IsMailError(ex))
            {
                log.StatusCode = "FAILED";
                log.ErrorText = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            }
        }
        db.Add(log);
        await db.SaveChangesAsync(ct);
        return log.StatusCode;
    }

    public static string E(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

    private static string Template(string en, string ar) => $"""
        <div style="font-family:Segoe UI,Tahoma,Arial,sans-serif;max-width:560px;margin:auto;color:#0b2e35">
          <div style="background:#014b55;color:#fff;padding:16px 22px;border-radius:12px 12px 0 0;font-weight:800;font-size:18px">CAPITRA</div>
          <div style="border:1px solid #d7e3e5;border-top:0;border-radius:0 0 12px 12px;padding:22px">
            {en}
            <hr style="border:0;border-top:1px solid #d7e3e5;margin:20px 0">
            <div dir="rtl" style="text-align:right">{ar}</div>
          </div>
        </div>
        """;
}
