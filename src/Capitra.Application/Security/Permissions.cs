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
        /// <summary>Create, change and delete lookup values. Reading lookups only needs a signed-in user.</summary>
        public const string LookupEdit = "ADM.LOOKUP.EDIT";
    }

    public static class Mtg
    {
        /// <summary>Read developers, signatories, projects, units and investors.</summary>
        public const string MasterDataView = "MTG.MASTER_DATA.VIEW";

        /// <summary>Create, change and delete developers, signatories, projects, units and investors.</summary>
        public const string MasterDataEdit = "MTG.MASTER_DATA.EDIT";

        /// <summary>Read portfolio cases, their accounts and payment history.</summary>
        public const string PortfolioView = "MTG.PORTFOLIO.VIEW";

        /// <summary>Create, change and delete portfolio cases, accounts and payments.</summary>
        public const string PortfolioEdit = "MTG.PORTFOLIO.EDIT";
    }
}
