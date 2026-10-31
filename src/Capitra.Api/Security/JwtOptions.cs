using System.ComponentModel.DataAnnotations;

namespace Capitra.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "CAPITRA";

    [Required]
    public string Audience { get; set; } = "CAPITRA_WEB";

    /// <summary>Signing key, at least 32 characters. Never stored in appsettings.json; use user-secrets or an environment variable.</summary>
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
