namespace Capitra.Application.Security;

/// <summary>
/// Permission codes, {MODULE}.{AREA}.{ACTION} (naming standard, section 7).
/// Permissions are granted to roles as role claims of type <see cref="ClaimType"/>.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    /// <summary>Grants every permission. Given to the seeded administrator role only.</summary>
    public const string All = "*";

    public static class Adm
    {
        public const string LookupView = "ADM.LOOKUP.VIEW";
        public const string LookupEdit = "ADM.LOOKUP.EDIT";
    }
}
