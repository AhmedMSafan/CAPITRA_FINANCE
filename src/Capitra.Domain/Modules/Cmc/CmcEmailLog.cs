using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Cmc;

/// <summary>
/// Communication Center: every e-mail the system sent or tried to send (account activation, password reset, test e-mails), to whom,
/// when, through which mail server, and whether it went out. Never holds a password.
/// </summary>
public class CmcEmailLog : AuditableEntity
{
    public long EmailLogId { get; set; }

    public string? UserId { get; set; }

    public string ToAddress { get; set; } = string.Empty;

    /// <summary>ACTIVATION, PASSWORD_RESET or TEST.</summary>
    public string KindCode { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    /// <summary>SENT, FAILED, NOT_CONFIGURED (no mail server set up), NO_ADDRESS or LINK_COPIED.</summary>
    public string StatusCode { get; set; } = string.Empty;

    public string? ErrorText { get; set; }

    public DateTime AttemptedAt { get; set; }

    /// <summary>The Communication Center mail server used; empty when the server came from the application settings, or none was.</summary>
    public long? MailServerId { get; set; }
}
