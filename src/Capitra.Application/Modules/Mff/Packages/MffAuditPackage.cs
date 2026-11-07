using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mff;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_AUDIT_PKG: a row of MFF_AUDIT_LOGS for a change.</summary>
public sealed class MffAuditPackage(ICapitraDbContext db, ICurrentUser user, IClock clock)
{
    /// <summary>LOG_CHANGE: records the change (table and action in upper case); the user is the signed-in user unless given.</summary>
    public async Task LogChangeAsync(string tableName, string recordKey, string actionType, string? oldDataJson = null, string? newDataJson = null,
        string? actionBy = null, long? appId = null, long? pageId = null, string? sessionId = null, string? clientIp = null, CancellationToken ct = default)
    {
        db.Add(new MffAuditLog
        {
            TableName = tableName.ToUpperInvariant(),
            RecordKey = recordKey,
            ActionType = actionType.ToUpperInvariant(),
            OldDataJson = oldDataJson,
            NewDataJson = newDataJson,
            ActionAt = clock.UtcNow,
            ActionBy = actionBy ?? user.UserName ?? string.Empty,
            AppId = appId,
            PageId = pageId,
            SessionId = sessionId,
            ClientIp = clientIp
        });
        await db.SaveChangesAsync(ct);
    }
}
