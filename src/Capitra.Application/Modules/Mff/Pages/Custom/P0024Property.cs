using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 24, After Submit "Save Property" (SAVE).</summary>
public sealed class P0024Property(ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 24;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var appId = Long(request, "P24_APPLICATION_ID");
            var propertyId = Long(request, "P24_PROPERTY_ID");
            var salePrice = Number(request, "P24_SALE_PRICE");
            MffProperty? p;
            if (propertyId is null)
            {
                p = new MffProperty();
                db.Add(p);
            }
            else
            {
                p = await db.Set<MffProperty>().FirstOrDefaultAsync(x => x.PropertyId == propertyId, ct);
            }

            if (p is not null)
            {
                p.ProjectId = Long(request, "P24_PROJECT_ID");
                p.GovernorateCode = Text(request, "P24_GOVERNORATE_CODE");
                p.CityCode = Text(request, "P24_CITY_CODE");
                p.DistrictAr = Text(request, "P24_DISTRICT_AR");
                p.NeighborhoodAr = Text(request, "P24_NEIGHBORHOOD_AR");
                p.BuildingNo = Text(request, "P24_BUILDING_NO");
                p.FloorNo = Text(request, "P24_FLOOR_NO");
                p.UnitNo = Text(request, "P24_UNIT_NO");
                p.AreaSqm = Number(request, "P24_AREA_SQM");
                p.AddressAr = Text(request, "P24_ADDRESS_AR");
                p.RegistrationStatusCode = Text(request, "P24_REGISTRATION_STATUS_CODE");
                p.RegistryReference = Text(request, "P24_REGISTRY_REFERENCE");
                p.SalePrice = salePrice;
                p.AppraisedValue = Number(request, "P24_APPRAISED_VALUE");
                p.MaintenanceDeposit = Number(request, "P24_MAINTENANCE_DEPOSIT");
            }

            await db.SaveChangesAsync(ct);
            if (propertyId is null && p is not null)
            {
                db.Add(new MffApplicationProperty
                {
                    ApplicationId = Req(appId, "MFF_APPLICATION_PROPERTIES", "APPLICATION_ID"),
                    PropertyId = p.PropertyId,
                    RequestedPrice = salePrice,
                    IsSelected = true,
                    SelectionDate = Today(clock),
                });
                propertyId = p.PropertyId;
            }
            else
            {
                foreach (var ap in await db.Set<MffApplicationProperty>().Where(x => x.ApplicationId == appId && x.PropertyId == propertyId).ToListAsync(ct))
                    ap.RequestedPrice = salePrice;
            }

            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == appId, ct);
            if (a is not null)
                a.PropertyPrice = salePrice;
            await db.SaveChangesAsync(ct);
            return ("Property details saved.", new Dictionary<string, string?> { ["P24_PROPERTY_ID"] = Id(propertyId!.Value) });
        }, ct);
    }
}
