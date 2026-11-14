using System.Collections.Concurrent;
using System.Security.Cryptography;
using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Apex;

/// <summary>A file the user chose in a File Browse item (APEX_APPLICATION_TEMP_FILES).</summary>
public sealed record ApexTempFile(string Name, string FileName, string? MimeType, byte[] Content);

/// <summary>A member of an APEX collection: c001.. and n001.. by position (C[0] is c001), and its seq_id.</summary>
public sealed record ApexCollectionMember(int SeqId, string?[] C, decimal?[] N);

/// <summary>
/// What APEX keeps in the user's session besides page items: the temporary files of File Browse items and the
/// APEX collections. Kept in memory per user for a working day, as an APEX session lives.
/// </summary>
public sealed class ApexSessionStore
{
    private static readonly TimeSpan Life = TimeSpan.FromHours(12);
    private readonly ConcurrentDictionary<string, (DateTime At, object Value)> entries = new(StringComparer.Ordinal);

    internal T? Get<T>(string key, DateTime now) where T : class =>
        entries.TryGetValue(key, out var e) && now - e.At < Life ? e.Value as T : null;

    internal void Set(string key, object value, DateTime now)
    {
        entries[key] = (now, value);
        foreach (var old in entries.Where(x => now - x.Value.At >= Life).Select(x => x.Key).ToList())
            entries.TryRemove(old, out _);
    }

    internal void Remove(string key) => entries.TryRemove(key, out _);
}

/// <summary>The signed-in user's APEX session: temp files and collections (apex_collection).</summary>
public sealed class ApexSession(ApexSessionStore store, ICurrentUser user, IClock clock)
{
    private string Me => user.UserId ?? user.UserName ?? string.Empty;

    /// <summary>Stores an uploaded file; its name ("&lt;id&gt;/&lt;file name&gt;") is the File Browse item's value.</summary>
    public string AddFile(string fileName, string? mimeType, byte[] content)
    {
        var name = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + "/" + Path.GetFileName(fileName);
        store.Set(Me + "|file|" + name, new ApexTempFile(name, Path.GetFileName(fileName), mimeType, content), clock.UtcNow);
        return name;
    }

    /// <summary>select ... from APEX_APPLICATION_TEMP_FILES where NAME = :name.</summary>
    public ApexTempFile? File(string? name) => name is null ? null : store.Get<ApexTempFile>(Me + "|file|" + name, clock.UtcNow);

    /// <summary>apex_collection.create_or_truncate_collection.</summary>
    public void CreateOrTruncate(string collection) =>
        store.Set(Me + "|coll|" + collection.ToUpperInvariant(), new List<ApexCollectionMember>(), clock.UtcNow);

    /// <summary>apex_collection.add_member (p_c001.., p_n001..).</summary>
    public void AddMember(string collection, string?[] c, decimal?[] n)
    {
        var key = Me + "|coll|" + collection.ToUpperInvariant();
        var list = store.Get<List<ApexCollectionMember>>(key, clock.UtcNow)
            ?? throw new ApexProcessException("ORA-20101: Application collection " + collection.ToUpperInvariant() + " does not exist");
        lock (list)
            list.Add(new ApexCollectionMember(list.Count + 1, c, n));
        store.Set(key, list, clock.UtcNow);
    }

    /// <summary>The members of a collection (apex_collections where collection_name = ...), empty when it does not exist.</summary>
    public IReadOnlyList<ApexCollectionMember> Members(string collection)
    {
        var list = store.Get<List<ApexCollectionMember>>(Me + "|coll|" + collection.ToUpperInvariant(), clock.UtcNow);
        if (list is null)
            return [];
        lock (list)
            return list.ToList();
    }
}
