namespace Capitra.Application.Security;

/// <summary>
/// The workflow step pages of App 105 and the step permission of each. A role with a step permission may use that step's
/// page as the portfolio permissions would allow it, so a step's role does not need access to every portfolio screen.
/// </summary>
public static class StepPermissions
{
    private static readonly Dictionary<int, string> ByPage = new Dictionary<int, string>
    {
        [208] = Permissions.Mtg.StepExcelImport,
        [209] = Permissions.Mtg.StepAmlReview,
        [210] = Permissions.Mtg.StepInquiries,
        [211] = Permissions.Mtg.StepLegalReview,
        [401] = Permissions.Mtg.StepValuation,
        [212] = Permissions.Mtg.StepCoverageReview,
        [213] = Permissions.Mtg.StepChequeCoverage,
        [214] = Permissions.Mtg.StepFinancialReview,
        [215] = Permissions.Mtg.StepBoardReview,
    };

    /// <summary>The step permission that also grants <paramref name="permission"/> on the page, or null.</summary>
    public static string? For(int app, int page, string permission) =>
        app == 105
        && (permission == Permissions.Mtg.PortfolioView || permission == Permissions.Mtg.PortfolioEdit)
        && ByPage.TryGetValue(page, out var step)
            ? step
            : null;
}
