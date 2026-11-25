using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_RECENT_CLIENTS: the customers each user opened in Customer 360, for "Recently opened" on the Customers page (Ahmed,
/// 25 Nov 2026). One row per user and customer; opening it again moves OpenedAt forward.
/// </summary>
public class CrmRecentClient : AuditableEntity
{
    public long RecentClientId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public long ClientId { get; set; }

    public DateTime OpenedAt { get; set; }
}
