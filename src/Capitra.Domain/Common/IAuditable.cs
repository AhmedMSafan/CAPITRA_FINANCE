namespace Capitra.Domain.Common;

/// <summary>
/// Standard audit columns carried by every table (naming standard, section 4).
/// Values are filled by the data layer from the logged-in user and the server clock, never by the client.
/// </summary>
public interface IAuditable
{
    string CreatedBy { get; set; }

    DateTime CreatedAt { get; set; }

    string? UpdatedBy { get; set; }

    DateTime? UpdatedAt { get; set; }

    /// <summary>Optimistic concurrency counter, incremented on every update.</summary>
    int RowVersion { get; set; }
}
