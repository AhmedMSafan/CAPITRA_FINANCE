using Capitra.Domain.Modules.Cmc;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Capitra.Infrastructure.Communication;

/// <summary>The mail server an e-mail goes through, with its password in clear only for the moment of sending.</summary>
public sealed record MailServerSettings(
    long? MailServerId, string ServerName, string Host, int Port, bool EnableSsl, string? UserName, string? Password, string FromAddress, string FromName);

/// <summary>
/// Which mail server the system uses: the active one of the Communication Center; when none is active, the "Email" application
/// settings (user-secrets or environment), as before the Communication Center existed; otherwise none.
/// </summary>
public sealed class MailSettings(CapitraDbContext db, IDataProtectionProvider protection, IOptions<EmailOptions> options)
{
    private const string Purpose = "Capitra.Cmc.MailServerPassword";

    public string Protect(string password) => protection.CreateProtector(Purpose).Protect(password);

    public string? Unprotect(string? protectedPassword)
    {
        if (string.IsNullOrEmpty(protectedPassword))
            return null;
        try
        {
            return protection.CreateProtector(Purpose).Unprotect(protectedPassword);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // the key that protected it is gone: the password must be entered again
            return null;
        }
    }

    public MailServerSettings For(CmcMailServer s) =>
        new(s.MailServerId, s.ServerName, s.HostName, s.PortNo, s.IsSslEnabled, s.UserName, Unprotect(s.PasswordProtected), s.FromAddress, s.FromName);

    public async Task<MailServerSettings?> CurrentAsync(CancellationToken ct)
    {
        var active = await db.Set<CmcMailServer>().AsNoTracking().FirstOrDefaultAsync(s => s.IsActive, ct);
        if (active is not null)
            return For(active);
        var o = options.Value;
        return o.IsConfigured
            ? new MailServerSettings(null, "Application settings", o.Host!, o.Port, o.EnableSsl, o.UserName, o.Password, o.FromAddress!, o.FromName)
            : null;
    }
}
