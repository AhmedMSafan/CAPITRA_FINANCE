using System.Text.Json;

namespace Capitra.Application.Modules.Apex;

/// <summary>
/// The values an APEX page sends with apex.server.process: x01 to x20 and one CLOB (p_clob_01).
/// The copied page scripts send exactly these, so each page's processes read them as the PL/SQL did.
/// </summary>
public sealed class ApexRequest
{
    public string? X01 { get; init; }
    public string? X02 { get; init; }
    public string? X03 { get; init; }
    public string? X04 { get; init; }
    public string? X05 { get; init; }
    public string? X06 { get; init; }
    public string? X07 { get; init; }
    public string? X08 { get; init; }
    public string? X09 { get; init; }
    public string? X10 { get; init; }
    public string? X11 { get; init; }
    public string? X12 { get; init; }
    public string? X13 { get; init; }
    public string? X14 { get; init; }
    public string? X15 { get; init; }
    public string? X16 { get; init; }
    public string? X17 { get; init; }
    public string? X18 { get; init; }
    public string? X19 { get; init; }
    public string? X20 { get; init; }
    public string? Clob01 { get; init; }

    /// <summary>The other properties of the call's data, which APEX sends in p_json and parses for apex_json.</summary>
    public Dictionary<string, JsonElement>? Json { get; init; }

    /// <summary>The page's session state: its items (P209_PORTFOLIO_ID) and REQUEST, as the page had them when it called.</summary>
    public Dictionary<string, string?>? Items { get; init; }

    /// <summary>:ITEM, the value of a page item (null when empty, as in APEX).</summary>
    public string? Item(string name) =>
        Items is not null && Items.FirstOrDefault(i => string.Equals(i.Key, name, StringComparison.OrdinalIgnoreCase)).Value is { Length: > 0 } v ? v : null;
}

/// <summary>
/// One on-demand process of a copied APEX page, ported from its PL/SQL to C#.
/// The result is written back as JSON with the same keys the PL/SQL wrote with apex_json.
/// </summary>
public interface IApexProcess
{
    int App { get; }

    int Page { get; }

    /// <summary>The APEX process name, e.g. SEG_LOAD.</summary>
    string Name { get; }

    /// <summary>The permission a user needs to run it.</summary>
    string Permission { get; }

    Task<object> RunAsync(ApexRequest request, CancellationToken ct);
}

/// <summary>An error the PL/SQL raised with raise_application_error; pages show its message.</summary>
public sealed class ApexProcessException(string message) : Exception(message);
