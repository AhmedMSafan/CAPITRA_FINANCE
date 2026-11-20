using System.Xml.Linq;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Infrastructure.Communication;

/// <summary>
/// Keeps the Data Protection keys in SYS_DATA_PROTECTION_KEYS (instead of the server's disk), so e-mailed links and the stored
/// mail-server passwords keep working after a restart, a new deployment or on a second server.
/// </summary>
public sealed class DatabaseKeyRepository(IServiceScopeFactory scopes) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CapitraDbContext>();
        return db.Set<SysDataProtectionKey>().AsNoTracking().OrderBy(k => k.DataProtectionKeyId).Select(k => k.KeyXml).ToList()
            .Select(x => XElement.Parse(x)).ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CapitraDbContext>();
        db.Add(new SysDataProtectionKey { FriendlyName = friendlyName, KeyXml = element.ToString(SaveOptions.DisableFormatting) });
        db.SaveChanges();
    }
}
