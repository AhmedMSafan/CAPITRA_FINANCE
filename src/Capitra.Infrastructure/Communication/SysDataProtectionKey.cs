using Capitra.Domain.Common;

namespace Capitra.Infrastructure.Communication;

/// <summary>
/// An ASP.NET Data Protection key, kept in the database so that what it protects survives restarts and works on every server: the
/// activation and password-reset links, and the Communication Center's stored mail-server passwords.
/// </summary>
public class SysDataProtectionKey : AuditableEntity
{
    public long DataProtectionKeyId { get; set; }

    public string? FriendlyName { get; set; }

    public string KeyXml { get; set; } = string.Empty;
}
