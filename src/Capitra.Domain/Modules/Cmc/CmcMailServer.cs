using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Cmc;

/// <summary>
/// Communication Center: an outgoing mail server (SMTP). One is active at a time and every e-mail of the system goes through it.
/// The password is stored encrypted (ASP.NET Data Protection) and is never sent back to any screen.
/// </summary>
public class CmcMailServer : AuditableEntity
{
    public long MailServerId { get; set; }

    /// <summary>A name to tell servers apart, e.g. "Gmail (testing)" or "Federation Exchange".</summary>
    public string ServerName { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;

    public int PortNo { get; set; } = 587;

    /// <summary>Secure connection (STARTTLS), as Gmail and Office 365 need on port 587.</summary>
    public bool IsSslEnabled { get; set; } = true;

    public string? UserName { get; set; }

    /// <summary>The password, encrypted; empty when the server needs no sign-in.</summary>
    public string? PasswordProtected { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "CAPITRA";

    /// <summary>The server in use; only one is active.</summary>
    public bool IsActive { get; set; }

    public DateTime? LastTestAt { get; set; }

    /// <summary>SENT or FAILED: the last test e-mail.</summary>
    public string? LastTestStatusCode { get; set; }

    public string? LastTestError { get; set; }
}
