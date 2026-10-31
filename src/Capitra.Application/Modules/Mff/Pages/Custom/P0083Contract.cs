using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 83, After Submit "Save Contract" (SAVE_CONTRACT).</summary>
public sealed class P0083Contract(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 83;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_CONTRACT")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_CONTRACTS";
            var propertyId = Long(request, "P83_PROPERTY_ID")
                ?? throw new ApexProcessException("ORA-20062: A selected property is required before creating the contract.");
            var appId = Long(request, "P83_APPLICATION_ID");
            var id = Long(request, "P83_CONTRACT_ID");
            MffContract? c;
            if (id is null)
            {
                c = new MffContract
                {
                    ApplicationId = Req(appId, t, "APPLICATION_ID"),
                    ContractType = "TRIPARTITE",
                    BuyerPersonId = Req(Long(request, "P83_BUYER_PERSON_ID"), t, "BUYER_PERSON_ID"),
                    FinancierId = Req(Long(request, "P83_FINANCIER_ID"), t, "FINANCIER_ID"),
                    PropertyId = propertyId,
                };
                db.Add(c);
            }
            else
            {
                c = await db.Set<MffContract>().FirstOrDefaultAsync(x => x.ContractId == id, ct);
            }

            if (c is not null)
            {
                c.ContractNo = Req(Text(request, "P83_CONTRACT_NO"), t, "CONTRACT_NO");
                c.ContractDate = Date(request, "P83_CONTRACT_DATE");
                c.SellerPersonId = Long(request, "P83_SELLER_PERSON_ID");
                c.FinanceAmount = Number(request, "P83_FINANCE_AMOUNT");
                c.PropertyPrice = Number(request, "P83_PROPERTY_PRICE");
                c.IsSigned = (Text(request, "P83_SIGNED_FLAG") ?? "N") == "Y";
                c.ContractStatus = Req(Text(request, "P83_CONTRACT_STATUS"), t, "CONTRACT_STATUS");
            }

            await db.SaveChangesAsync(ct);
            await control.ChangeStatusCheckedAsync(appId ?? 0, "CONTRACT_PENDING", "OPERATIONS", user.UserName, "Tripartite contract prepared", ct);
            return ("Contract saved.", c is null ? null : new Dictionary<string, string?> { ["P83_CONTRACT_ID"] = Id(c.ContractId) });
        }, ct);
    }
}
