namespace Capitra.Application.Modules.Mtg.MasterData;

internal static class MasterDataRules
{
    /// <summary>Trimmed text, or null when blank.</summary>
    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>A lookup code as stored: trimmed and upper case, or null when blank.</summary>
    public static string? Code(string? value) => Clean(value)?.ToUpperInvariant();
}
