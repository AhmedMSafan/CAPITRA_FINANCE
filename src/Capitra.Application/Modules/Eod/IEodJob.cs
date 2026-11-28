namespace Capitra.Application.Modules.Eod;

/// <summary>A setting of a job: its code, its words, and the value used while none is saved.</summary>
public sealed record EodParameterDefinition(string Code, string LabelEn, string LabelAr, string HelpEn, string HelpAr, string DefaultValue);

/// <summary>What one job is given for a business day: the day, and its settings (saved values, else the defaults).</summary>
public sealed record EodJobContext(DateOnly BusinessDate, IReadOnlyDictionary<string, string> Parameters)
{
    public string Text(string code) => Parameters.TryGetValue(code, out var v) ? v : string.Empty;

    public int Number(string code, int fallback = 0) =>
        int.TryParse(Text(code), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : fallback;

    /// <summary>A list of whole numbers such as "30,7,1".</summary>
    public IReadOnlyList<int> Numbers(string code) =>
        [.. Text(code).Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : -1)
            .Where(n => n >= 0).Distinct()];
}

/// <summary>What a job did on the day: how many items, and a sentence for the End of Day page.</summary>
public sealed record EodJobOutcome(int ItemsCount, string Summary);

/// <summary>
/// One job of the End of Day (night batch). Each application adds its own jobs; the End of Day runs them for each business day in the
/// order of EOD_JOBS. A job does all of its day's work or throws (Business rule, validation, conflict) with the reason in words; the
/// day then stays open and the job alone is run again after the cause is fixed, so a job must be safe to run again on the same day.
/// </summary>
public interface IEodJob
{
    string JobCode { get; }

    /// <summary>The application: LND, SCR ...</summary>
    string ModuleCode { get; }

    string NameEn { get; }

    string NameAr { get; }

    /// <summary>What the job does, for the jobs page.</summary>
    string DescriptionEn { get; }

    string DescriptionAr { get; }

    IReadOnlyList<EodParameterDefinition> Parameters { get; }

    Task<EodJobOutcome> RunAsync(EodJobContext context, CancellationToken cancellationToken);
}
