using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 53, Before Header "Load Readiness" and After Submit "Load Application" (LOAD_APPLICATION): the same block.</summary>
internal static class P0053Readiness
{
    public static async Task<Dictionary<string, string?>> LoadAsync(MffControlPackage control, string? applicationId, CancellationToken ct)
    {
        var id = Mtg.Pages.Custom.MtgAjax.Long(applicationId);
        var r = id is null ? null : await control.ReadinessAsync(id.Value, ct);
        if (id is null || r is null)
        {
            return new Dictionary<string, string?>
            {
                ["P53_READINESS_PCT"] = "0", ["P53_STATUS"] = null, ["P53_OPEN_TASKS"] = "0", ["P53_OVERDUE_TASKS"] = "0",
                ["P53_READINESS_MESSAGE"] = "Application not found",
            };
        }

        return new Dictionary<string, string?>
        {
            ["P53_READINESS_PCT"] = Num(r.Number("READINESS_PCT")),
            ["P53_STATUS"] = r.Text("STATUS_CODE"),
            ["P53_OPEN_TASKS"] = Num(r.Number("OPEN_TASKS")),
            ["P53_OVERDUE_TASKS"] = Num(r.Number("OVERDUE_TASKS")),
            ["P53_READINESS_MESSAGE"] = await control.ReadinessMessageAsync(id.Value, null, ct),
        };
    }

    private static string? Num(decimal? d) => d?.ToString(CultureInfo.InvariantCulture);
}

/// <summary>Page 53 Before Header.</summary>
public sealed class P0053PageLoad(MffControlPackage control) : IApexProcess
{
    public int App => 110;

    public int Page => 53;

    public string Name => "PAGE_LOAD";

    public string Permission => Permissions.Mff.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = ApexItems.Read(request.X01);
        return new { ok = true, items = await P0053Readiness.LoadAsync(control, items.GetValueOrDefault("P53_APPLICATION_ID") is { Length: > 0 } v ? v : null, ct) };
    }
}

/// <summary>Page 53 LOAD_APPLICATION.</summary>
public sealed class P0053Submit(MffControlPackage control) : IApexProcess
{
    public int App => 110;

    public int Page => 53;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "LOAD_APPLICATION")
            return new { ok = true };
        return new { ok = true, items = await P0053Readiness.LoadAsync(control, request.Item("P53_APPLICATION_ID"), ct) };
    }
}
