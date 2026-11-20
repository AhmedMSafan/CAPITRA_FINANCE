namespace Capitra.Api.Security;

/// <summary>Claim names carried in the JWT. Inbound claim mapping is off, so these names are used as-is.</summary>
public static class CapitraClaimTypes
{
    public const string UserId = "sub";
    public const string UserName = "name";
    public const string FullName = "full_name";
    public const string Role = "role";

    /// <summary>Present ("1") on the token of a user who must change their password first; see PasswordChangeGate.</summary>
    public const string PasswordChangeRequired = "pwd_chg";
}
