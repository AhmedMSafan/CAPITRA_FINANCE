using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 611 "Portfolio Document Requirements", After Submit "Load Requirement" (LOAD) and "Save Requirement" (SAVE).</summary>
public sealed class P0611SubmitRequirement(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 611;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        var code = request.Item("P611_REQUIREMENT_CODE");
        if (req == "LOAD")
        {
            var r = await db.Set<MtgLkpDocumentRequirement>().AsNoTracking().FirstOrDefaultAsync(x => x.RequirementCode == code, ct);
            if (r is null)
                return new { ok = false, message = "ORA-01403: no data found" };
            return new
            {
                ok = true,
                items = new Dictionary<string, string?>
                {
                    ["P611_REQUIREMENT_CODE"] = code,
                    ["P611_MANDATORY_FLAG"] = Yn(r.IsMandatory),
                    ["P611_ORIGINAL_REQUIRED_FLAG"] = Yn(r.IsOriginalRequired),
                    ["P611_VERIFIED_REQUIRED_FLAG"] = Yn(r.IsVerifiedRequired),
                    ["P611_MAX_AGE_DAYS"] = r.MaxAgeDays?.ToString(CultureInfo.InvariantCulture),
                    ["P611_ACTIVE_FLAG"] = Yn(r.IsActive),
                },
            };
        }

        if (req == "SAVE")
        {
            // an update by code: no row is changed when the requirement is not chosen
            var r = await db.Set<MtgLkpDocumentRequirement>().FirstOrDefaultAsync(x => x.RequirementCode == code, ct);
            if (r is not null)
            {
                r.IsMandatory = request.Item("P611_MANDATORY_FLAG") == "Y";
                r.IsOriginalRequired = request.Item("P611_ORIGINAL_REQUIRED_FLAG") == "Y";
                r.IsVerifiedRequired = request.Item("P611_VERIFIED_REQUIRED_FLAG") == "Y";
                r.MaxAgeDays = MtgAjax.Number(request.Item("P611_MAX_AGE_DAYS")) is { } d ? (int)d : null;
                r.IsActive = request.Item("P611_ACTIVE_FLAG") == "Y";
                await db.SaveChangesAsync(ct);
            }

            return new { ok = true, message = "Document requirement updated" };
        }

        return new { ok = true };
    }

    private static string Yn(bool p) => p ? "Y" : "N";
}
