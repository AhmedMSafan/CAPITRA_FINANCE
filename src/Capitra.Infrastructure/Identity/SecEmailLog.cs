using Capitra.Domain.Common;

namespace Capitra.Infrastructure.Identity;

/// <summary>Every account e-mail the system sent or tried to send (activation, password reset): to whom, when, and whether it went out.</summary>
public class SecEmailLog : AuditableEntity
{
    public long EmailLogId { get; set; }

    public string? UserId { get; set; }

    public string ToAddress { get; set; } = string.Empty;

    /// <summary>ACTIVATION or PASSWORD_RESET.</summary>
    public string KindCode { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    /// <summary>SENT, FAILED or NOT_CONFIGURED (no mail server set up).</summary>
    public string StatusCode { get; set; } = string.Empty;

    public string? ErrorText { get; set; }

    public DateTime AttemptedAt { get; set; }
}
