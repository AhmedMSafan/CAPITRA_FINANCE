using System.Text.RegularExpressions;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Capitra.Tests.Persistence;

/// <summary>
/// database/scripts/reset-all-portfolios.sql names every table of the model: emptied when it stores running data,
/// or commented out when it is kept. A new table that is in neither list fails the build here, so a table that
/// stores running data cannot be left out of the reset by mistake.
/// </summary>
public sealed partial class ResetScriptTests
{
    [GeneratedRegex("TRUNCATE TABLE \"([A-Z0-9_]+)\"")]
    private static partial Regex TruncateLine();

    private static string ScriptPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Capitra.sln")))
            dir = dir.Parent;
        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("Capitra.sln was not found."),
            "database", "scripts", "reset-all-portfolios.sql");
    }

    [Fact]
    public void Reset_script_names_every_table()
    {
        var named = TruncateLine().Matches(File.ReadAllText(ScriptPath())).Select(m => m.Groups[1].Value).ToHashSet();
        var model = new CapitraDbContextFactory().CreateDbContext([]).GetService<IDesignTimeModel>().Model;
        var missing = model.GetEntityTypes().Where(e => !e.IsOwned()).Select(e => e.GetTableName()!)
            .Distinct().Where(t => !named.Contains(t)).Order().ToList();

        Assert.True(missing.Count == 0,
            "Add these tables to database/scripts/reset-all-portfolios.sql (emptied if they store running data, else in the kept list): "
            + string.Join(", ", missing));
    }

    [Fact]
    public void Reset_script_resets_the_tran_set_counters() =>
        Assert.Contains("UPDATE \"GL_TRAN_SET_SEQUENCES\" SET \"LAST_NO\" = 0", File.ReadAllText(ScriptPath()));
}
